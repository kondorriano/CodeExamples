using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGrab : MonoBehaviour
{
    [Header("Item Detector")]
    [SerializeField] private Transform _grabLocatorBig = null;
    [SerializeField] private Transform _grabLocatorSmall = null;
    [SerializeField] private GrabTrigger _grabTriggerBig = null;
    [SerializeField] private GrabTrigger _grabTriggerSmall = null;

    //Grab Item List
    List<BasicGrabbableScript> _grabbableObjects;
    BasicGrabbableScript _nearestObject = null;

    //Components
    private PlayerController _playerController;

    public void Init(PlayerController pc)
    {
        //Set Player Controller
        _playerController = pc;

        //PrepareTrigger
        _grabTriggerBig.Init(this);
        _grabTriggerSmall.Init(this);

        //PrepareGrabList
        _grabbableObjects = new List<BasicGrabbableScript>();
    }

    public void Tick(float fixedDeltaTime, bool grabDown, bool grabPressed)
    {
        //InEndLocked
        if (_playerController.IsPlayerEndLocked == true)
        {
            return;
        }

        //InDialogue
        if (_playerController.InDialogue == true)
        {
            return;
        }

        //PlayerDisabled
        if (_playerController.IsPlayerDisabled == true)
        {
            return;
        }

        //Set Nearest
        SetNearestObject(GetNearestObject());

        /*GENERIC BLOCKERS*/
        if (_playerController.IsDoingAction == true)
        {
            return;
        }

        /*PLAYER TICK*/
        if (_playerController.IsPlayerBig == true)
        {
            TickBeast(fixedDeltaTime, grabDown, grabPressed);
        }
        else
        {
            TickChicken(fixedDeltaTime, grabDown, grabPressed);
        }
    }

    public void TickChicken(float fixedDeltaTime, bool grabDown, bool grabPressed)
    {
        //Grab Item in ground
        if (grabDown == true)
        {
            //No item in beak
            if (_playerController.GrabbedObject == null)
            {
                _playerController.StartPlayerAction(true);
                _playerController.SetAction("Grab");
                _grabTriggerSmall.Activate(true);
            }            
        }
    }

    public void TickBeast(float fixedDeltaTime, bool grabDown, bool grabPressed)
    {
        //Player in ground
        if (_playerController.TouchingGround == true)
        {
            if (grabDown == true)
            {
                //No item in hand
                if (_playerController.GrabbedObject == null)
                {
                    _playerController.StartPlayerAction(true);
                    _playerController.SetAction("Grab");
                }               
            }
        }
    }

    public void SwapCharacters(bool swapToSmall)
    {
        _grabTriggerBig.Activate(!swapToSmall);
        _grabTriggerSmall.Activate(swapToSmall);
        SetNearestObject(null);
        ClearGrabbableObjects();
    }

    public void PlayerRespawn()
    {
        SetNearestObject(null);
        ClearGrabbableObjects();
    }

    #region Player Item Detection
    public void AddObject(BasicGrabbableScript item)
    {
        if (_playerController.IsPlayerDisabled == true) return;
        if (_playerController.GrabbedObject != null) return;
        if (!_grabbableObjects.Contains(item)) _grabbableObjects.Add(item);
    }

    public void RemoveObject(BasicGrabbableScript item)
    {
        if (_playerController.IsPlayerDisabled == true) return;
        if (_playerController.GrabbedObject != null) return;
        if (_grabbableObjects.Contains(item)) _grabbableObjects.Remove(item);
    }

    public void ClearGrabbableObjects()
    {
        _grabbableObjects.Clear();
    }

    BasicGrabbableScript GetNearestObject()
    {
        if (_grabbableObjects.Count <= 0) return null;
        int nearestObjectIndex = -1;
        Transform playerTransform = transform;

        for (int i = 0; i < _grabbableObjects.Count; ++i)
        {
            if (_grabbableObjects[i] == null)
            {
                continue;
            }
            if (nearestObjectIndex == -1)
            {
                nearestObjectIndex = i;
                continue;
            }

            float dist1 = (_grabbableObjects[nearestObjectIndex].transform.position - playerTransform.position).sqrMagnitude;
            float dist2 = (_grabbableObjects[i].transform.position - playerTransform.position).sqrMagnitude;

            if (dist1 >= dist2) nearestObjectIndex = i;
        }

        if (nearestObjectIndex != -1) return _grabbableObjects[nearestObjectIndex];
        return null;
    }

    public void GrabObject()
    {
        Transform grabLocator = (_playerController.IsPlayerBig == true) ? _grabLocatorBig.transform : _grabLocatorSmall.transform;
        if (_nearestObject != null)
        {
            _nearestObject.Grab(grabLocator);
            _playerController.GrabbedObject = _nearestObject;
            SetNearestObject(null);
        }
        ClearGrabbableObjects();
    }

    void SetNearestObject(BasicGrabbableScript newNearestObject)
    {
        if (_nearestObject == newNearestObject) return;

        _nearestObject = newNearestObject;
    }

    public BasicGrabbableScript GetNearestGrab()
    {
        return _nearestObject;
    }

    #endregion
}
