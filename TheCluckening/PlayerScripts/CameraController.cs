using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;


public class CameraController : MonoBehaviour
{
    [System.Serializable]
    public class CameraSettings
    {
        public Transform cameraAimer;
        public CinemachineFreeLook freeCamera;
        public float horizontalOrbitSpeed = 150;

        // 10 4 -2 2 10 4 4
        [Header("Big Character Camera Settings")]
        public float topRigHeightBig = 30;
        public float middleRigHeightBig = 6;
        public float bottomRigHeightBig = -1.5f;

        public float topRigRadiusBig = 0.1f;
        public float middleRigRadiusBig = 10;
        public float bottomRigRadiusBig = 2;

        public float aimerHeightOffsetPositionBig = 2f;

        //5 .8 -.2 1 3 1 .5
        [Header("Small Character Camera Settings")]
        public float topRigHeightSmall = 7.5f;
        public float middleRigHeightSmall = 1.5f;
        public float bottomRigHeightSmall = -0.1875f;

        public float topRigRadiusSmall = 0.1f;
        public float middleRigRadiusSmall = 5;
        public float bottomRigRadiusSmall = .125f;

        public float aimerHeightOffsetPositionSmall = .5f;

        [Header("Target Settings")]
        public Camera targetCamera;

        [Header("Transform Settings")]
        public float firstTransformTime = 6f;
        public float transformTime = 1.25f;

        [Header("Transition Settings")]
        public float fastTransitionTime = .2f;
        public float normalTransitionTime = 1f;
        public float slowTransitionTime = 4f;

        [Header("MiniMap Settings")]
        public Transform miniMapCam;

    }

    [SerializeField] private CameraSettings _cameraSettings = null;

    //Variables
    bool _isExtraLinear = true;
    bool _hasExtraTarget = false;
    Transform _dummyTarget;
    Transform _playerTarget;

    float _shakeDuration = 0;
    float _shakeCounter = 0;
    bool _isShaking = false;

    string _cameraYAxisName;
    bool _isCameraLocked = false;
    Quaternion _lockedLookRotation;

    bool inCreditsTransition = false;

    //Transformation
    float _swapCounter = 0;
    float _swapTime = 1;
    bool _isSwapping = false;
    float _aimerHeightOffsetPosition = 0;

    //Transitions
    bool _inTransition = false;
    float _transitionCounter = 0;
    float _transitionTime = 1;
    Vector3 _initialTransitionPosition;

    //Components
    private PlayerController _playerController;
    SimpleBezierCurve _transitionCurve;
    CinemachineBasicMultiChannelPerlin[] noises;

    public void Init(PlayerController pc)
    {
        //Set Player Controller
        _playerController = pc;
        //Set AimPlayer
        _playerTarget = pc.transform;
        _cameraSettings.freeCamera.m_Follow = _cameraSettings.cameraAimer;
        _cameraSettings.freeCamera.m_LookAt = _cameraSettings.cameraAimer;

        //Variables
        _cameraSettings.freeCamera.m_Orbits[0].m_Height = _cameraSettings.topRigHeightBig;
        _cameraSettings.freeCamera.m_Orbits[1].m_Height = _cameraSettings.middleRigHeightBig;
        _cameraSettings.freeCamera.m_Orbits[2].m_Height = _cameraSettings.bottomRigHeightBig;
        _cameraSettings.freeCamera.m_Orbits[0].m_Radius = _cameraSettings.topRigRadiusBig;
        _cameraSettings.freeCamera.m_Orbits[1].m_Radius = _cameraSettings.middleRigRadiusBig;
        _cameraSettings.freeCamera.m_Orbits[2].m_Radius = _cameraSettings.bottomRigRadiusBig;
        _aimerHeightOffsetPosition = (_playerController.IsPlayerBig) ? _cameraSettings.aimerHeightOffsetPositionBig : _cameraSettings.aimerHeightOffsetPositionSmall;
        ResetPosition(_aimerHeightOffsetPosition);

        _cameraYAxisName = _cameraSettings.freeCamera.m_YAxis.m_InputAxisName;

        //BezierCurve
        _transitionCurve = new SimpleBezierCurve();

        //Noise
        noises = new CinemachineBasicMultiChannelPerlin[3];
        for(int i = 0; i < noises.Length; i++)
        {
            noises[i] = _cameraSettings.freeCamera.GetRig(i).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        }
    }

    public void SetExtraTransition(Variables.ExtraCameraTransition extraTransition, Variables.CameraTransition transition)
    {
        if (extraTransition == null && _hasExtraTarget == false && _dummyTarget == null) return;
        _initialTransitionPosition = _cameraSettings.cameraAimer.position;
        if (extraTransition == null)
        {
            _dummyTarget = null;
            _hasExtraTarget = false;
        }
        else
        {
            _hasExtraTarget = true;
            if (extraTransition.isLinear == true)
            {
                _isExtraLinear = true;
                _dummyTarget = extraTransition.endPosition;
            }
            else
            {
                bool bezierPrepared = _transitionCurve.InitCurve(extraTransition);
                _isExtraLinear = !bezierPrepared;
                _dummyTarget = null;
            }
        }
        if (transition == Variables.CameraTransition.Immediate) return;
        _inTransition = true;
        _transitionCounter = 0;
        switch (transition)
        {
            case Variables.CameraTransition.Fast:
                _transitionTime = _cameraSettings.fastTransitionTime;
                break;
            case Variables.CameraTransition.Normal:
                _transitionTime = _cameraSettings.normalTransitionTime;
                break;
            case Variables.CameraTransition.Slow:
                _transitionTime = _cameraSettings.slowTransitionTime;
                break;
        }
    }

    //Need a second one because of the offset position for the dummy
    public void SetDummyCameraTarget(Transform target)
    {
        if (target == null && _dummyTarget == null && _hasExtraTarget == false) return;
        _hasExtraTarget = false;
        _dummyTarget = target;
        _initialTransitionPosition = _cameraSettings.cameraAimer.position;
        _inTransition = true;
        _transitionCounter = 0;
        _transitionTime = _cameraSettings.fastTransitionTime;
    }

    public void LockCameraSmoothly(Transform lookTransform)
    {
        _cameraSettings.freeCamera.m_YAxis.m_InputAxisName = "";
        _lockedLookRotation = Quaternion.LookRotation(lookTransform.forward, Vector3.up);
        _isCameraLocked = true;
    }

    public void UnlockCamera()
    {
        _isCameraLocked = false;
        _cameraSettings.freeCamera.m_YAxis.m_InputAxisName = _cameraYAxisName;
    }

    void ResetPosition(float aimerOffsetPosition)
    {
        //Reset Aimer Position and Rotation
        _cameraSettings.cameraAimer.position = _playerTarget.position + Vector3.up * aimerOffsetPosition;
        _cameraSettings.cameraAimer.rotation = _playerTarget.rotation;
        //Reset Y Orbit position
        _cameraSettings.freeCamera.m_YAxis.Value = .6f;
        //Reset Target aimer
        _cameraSettings.freeCamera.m_LookAt = _cameraSettings.cameraAimer;
    }

    public void Tick(float fixedDeltaTime, float vOrbit, float hOrbit)
    {
        ShakeCamera(fixedDeltaTime);

        SwapTransition(fixedDeltaTime);

        //Calculate target position
        SetTargetPosition(fixedDeltaTime, _aimerHeightOffsetPosition);

        //SmoothTransition if Locked
        SmoothLockTransition();
        //Player Input Orbit
        OrbitTarget(fixedDeltaTime, hOrbit);

        //SetTargetCamera
        SetTargetCamera();

        //SetMiniMap
        SetMiniMapCamera();
    }

    void SmoothLockTransition()
    {
        if (!_isCameraLocked) return;
        _cameraSettings.cameraAimer.rotation = Quaternion.Slerp(_cameraSettings.cameraAimer.rotation, _lockedLookRotation, .1f);
        _cameraSettings.freeCamera.m_YAxis.Value = Mathf.Lerp(_cameraSettings.freeCamera.m_YAxis.Value, .5f, .1f);
    }

    void SetTargetPosition(float fixedDeltaTime, float aimerOffsetPosition)
    {
        if (_hasExtraTarget == true && _isExtraLinear == false)
        {
            if (_inTransition == false)
            {
                _cameraSettings.cameraAimer.position = _transitionCurve.GetBezierPoint(1);
                return;
            }

            _transitionCounter += fixedDeltaTime;
            float easing = Mathf.Min(_transitionCounter / _transitionTime, 1);
            if (_transitionCounter >= _transitionTime) _inTransition = false;
            _cameraSettings.cameraAimer.position = _transitionCurve.GetBezierPoint(easing);
        }
        else
        {
            Vector3 targetPosition;
            if(_hasExtraTarget == true && _dummyTarget != null) targetPosition = _dummyTarget.position;
            else if (_dummyTarget != null) targetPosition = _dummyTarget.position + Vector3.up * aimerOffsetPosition;
            else targetPosition = _playerTarget.position + Vector3.up * aimerOffsetPosition;

            if (_inTransition == false)
            {
                _cameraSettings.cameraAimer.position = targetPosition;
                return;
            }

            _transitionCounter += fixedDeltaTime;
            float easing = _transitionCounter / _transitionTime;
            if (_transitionCounter >= _transitionTime) _inTransition = false;
            _cameraSettings.cameraAimer.position = Vector3.Lerp(_initialTransitionPosition, targetPosition, easing);
        }        
    }

    void OrbitTarget(float fixedDeltaTime, float hOrbit)
    {
        if (_isCameraLocked) return;
        if (Mathf.Abs(hOrbit) > 0.1f)
        {
            _cameraSettings.cameraAimer.Rotate(Vector3.up, hOrbit * _cameraSettings.horizontalOrbitSpeed * fixedDeltaTime);
        }        
    }

    public void SetCreditsTransition(bool start)
    {
        if (inCreditsTransition) return;
        StartCoroutine(CreditsTransition((start)?.5f:1f));
    }


    IEnumerator CreditsTransition(float endViewport)
    {
        inCreditsTransition = true;
        Camera mainCam = Camera.main;
        Rect rect = mainCam.rect;

        float transitionCounter = 0;
        float start = rect.width;
        float end = endViewport;
        while (transitionCounter <= 1f)
        {
            transitionCounter += Time.deltaTime;
            rect.width = Mathf.Lerp(start, end, transitionCounter / 1f);
            mainCam.rect = rect;
            _cameraSettings.targetCamera.rect = rect;
            yield return null;
        }

        inCreditsTransition = false;
    }


    void SetTargetCamera()
    {
        if (_cameraSettings.targetCamera.enabled == true && _playerController.GrabbedObject == null) {
            _cameraSettings.targetCamera.enabled = false;
            return;
        }
        if (_cameraSettings.targetCamera.enabled == false && _playerController.GrabbedObject != null)
        {
            _cameraSettings.targetCamera.enabled = true;
            return;
        }
    }

    #region SHAKE
    void ShakeCamera(float fixedDeltaTime)
    {
        if (!_isShaking) return;
        _shakeCounter += fixedDeltaTime;
        if(_shakeCounter >= _shakeDuration)
        {
            _isShaking = false;
            for (int i = 0; i < noises.Length; i++)
            {
                noises[i].m_AmplitudeGain = 0;
                noises[i].m_FrequencyGain = 1;
            }
        }
    }

    public void StartShake(float shakeDuration, float shakeAmplitude, float shakeFrequency)
    {
        if (_isShaking)
        {
            if(_shakeDuration -_shakeCounter  < shakeDuration)
            {
                _shakeCounter = 0;
                _shakeDuration = shakeDuration;
                for (int i = 0; i < noises.Length; i++)
                {
                    noises[i].m_AmplitudeGain = shakeAmplitude;
                    noises[i].m_FrequencyGain = shakeFrequency;
                }
            }
        }
        else
        {
            _isShaking = true;
            _shakeCounter = 0;
            _shakeDuration = shakeDuration;
            for (int i = 0; i < noises.Length; i++)
            {
                noises[i].m_AmplitudeGain = shakeAmplitude;
                noises[i].m_FrequencyGain = shakeFrequency;
            }
        }
    }
    #endregion

    #region SWAP

    public void SwapFast(bool isPlayerBig)
    {
        if (isPlayerBig == false)
        {
            _cameraSettings.freeCamera.m_Orbits[0].m_Height = _cameraSettings.topRigHeightSmall;
            _cameraSettings.freeCamera.m_Orbits[1].m_Height = _cameraSettings.middleRigHeightSmall;
            _cameraSettings.freeCamera.m_Orbits[2].m_Height = _cameraSettings.bottomRigHeightSmall;
            _cameraSettings.freeCamera.m_Orbits[0].m_Radius = _cameraSettings.topRigRadiusSmall;
            _cameraSettings.freeCamera.m_Orbits[1].m_Radius = _cameraSettings.middleRigRadiusSmall;
            _cameraSettings.freeCamera.m_Orbits[2].m_Radius = _cameraSettings.bottomRigRadiusSmall;
            _aimerHeightOffsetPosition = _cameraSettings.aimerHeightOffsetPositionSmall;
        }
        else
        {
            _cameraSettings.freeCamera.m_Orbits[0].m_Height = _cameraSettings.topRigHeightBig;
            _cameraSettings.freeCamera.m_Orbits[1].m_Height = _cameraSettings.middleRigHeightBig;
            _cameraSettings.freeCamera.m_Orbits[2].m_Height = _cameraSettings.bottomRigHeightBig;
            _cameraSettings.freeCamera.m_Orbits[0].m_Radius = _cameraSettings.topRigRadiusBig;
            _cameraSettings.freeCamera.m_Orbits[1].m_Radius = _cameraSettings.middleRigRadiusBig;
            _cameraSettings.freeCamera.m_Orbits[2].m_Radius = _cameraSettings.bottomRigRadiusBig;
            _aimerHeightOffsetPosition = _cameraSettings.aimerHeightOffsetPositionBig;
        }
    }

    public void SwapCharacters(bool isPlayerBig, bool firstTransform)
    {
        _swapTime = (firstTransform) ? _cameraSettings.firstTransformTime : _cameraSettings.transformTime;

        _isSwapping = true;
        _swapCounter = 0;
    }

    void SwapTransition(float fixedDeltaTime)
    {
        if (_isSwapping == false) return;
        _swapCounter += fixedDeltaTime;
        float easing = _swapCounter / _swapTime;
        if (_swapCounter >= _swapTime) _isSwapping = false;

        if(_playerController.IsPlayerBig == true)
        {
            _cameraSettings.freeCamera.m_Orbits[0].m_Height = Mathf.Lerp(_cameraSettings.topRigHeightSmall, _cameraSettings.topRigHeightBig, easing);
            _cameraSettings.freeCamera.m_Orbits[1].m_Height = Mathf.Lerp(_cameraSettings.middleRigHeightSmall, _cameraSettings.middleRigHeightBig, easing);
            _cameraSettings.freeCamera.m_Orbits[2].m_Height = Mathf.Lerp(_cameraSettings.bottomRigHeightSmall, _cameraSettings.bottomRigHeightBig, easing);
            _cameraSettings.freeCamera.m_Orbits[0].m_Radius = Mathf.Lerp(_cameraSettings.topRigRadiusSmall, _cameraSettings.topRigRadiusBig, easing);
            _cameraSettings.freeCamera.m_Orbits[1].m_Radius = Mathf.Lerp(_cameraSettings.middleRigRadiusSmall, _cameraSettings.middleRigRadiusBig, easing);
            _cameraSettings.freeCamera.m_Orbits[2].m_Radius = Mathf.Lerp(_cameraSettings.bottomRigRadiusSmall, _cameraSettings.bottomRigRadiusBig, easing);
            _aimerHeightOffsetPosition = Mathf.Lerp(_cameraSettings.aimerHeightOffsetPositionSmall, _cameraSettings.aimerHeightOffsetPositionBig, easing);
        }
        else
        {
            _cameraSettings.freeCamera.m_Orbits[0].m_Height = Mathf.Lerp(_cameraSettings.topRigHeightBig, _cameraSettings.topRigHeightSmall, easing);
            _cameraSettings.freeCamera.m_Orbits[1].m_Height = Mathf.Lerp(_cameraSettings.middleRigHeightBig, _cameraSettings.middleRigHeightSmall, easing);
            _cameraSettings.freeCamera.m_Orbits[2].m_Height = Mathf.Lerp(_cameraSettings.bottomRigHeightBig, _cameraSettings.bottomRigHeightSmall, easing);
            _cameraSettings.freeCamera.m_Orbits[0].m_Radius = Mathf.Lerp(_cameraSettings.topRigRadiusBig, _cameraSettings.topRigRadiusSmall, easing);
            _cameraSettings.freeCamera.m_Orbits[1].m_Radius = Mathf.Lerp(_cameraSettings.middleRigRadiusBig, _cameraSettings.middleRigRadiusSmall, easing);
            _cameraSettings.freeCamera.m_Orbits[2].m_Radius = Mathf.Lerp(_cameraSettings.bottomRigRadiusBig, _cameraSettings.bottomRigRadiusSmall, easing);
            _aimerHeightOffsetPosition = Mathf.Lerp(_cameraSettings.aimerHeightOffsetPositionBig, _cameraSettings.aimerHeightOffsetPositionSmall, easing);
        }
    }
    #endregion

    public void AimPlayerToCamera()
    {
        _playerTarget.rotation = _cameraSettings.cameraAimer.rotation;
    }

    #region QuestSounds
    public void PlayQuestSound(string sound)
    {
        AkSoundEngine.PostEvent(sound, _cameraSettings.cameraAimer.gameObject);
    }
    #endregion

    #region MiniMap
    void SetMiniMapCamera()
    {
        Vector3 position = _cameraSettings.cameraAimer.position;
        position.y = _cameraSettings.miniMapCam.position.y;
        Vector3 rotation = Camera.main.transform.eulerAngles;
        _cameraSettings.miniMapCam.position = position;
        _cameraSettings.miniMapCam.eulerAngles = rotation;
        rotation = _cameraSettings.miniMapCam.localEulerAngles;
        rotation.x = 90;
        _cameraSettings.miniMapCam.localEulerAngles = rotation;
    }
    #endregion

}
