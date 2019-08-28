using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTransformation : MonoBehaviour
{
    //TranformType
    [Header("Transform Data")]
    [SerializeField] private int _transformationPoints = 30;
    [Header("Transform References")]
    [SerializeField] private Animator _animatorDummy;
    GameObject _holderDummy;

    //Variables
    bool canTransform = false;
    bool tutorialTransform = false;

    public int TransformationPoints
    {
        get { return _transformationPoints; }
    }

    //Components
    private PlayerController _playerController;

    public void Init(PlayerController pc)
    {
        //Set Player Controller
        _playerController = pc;

        //Set Holder Dummy
        _holderDummy = _animatorDummy.gameObject;
    }

    public void Tick(float fixedDeltaTime, bool swapDown)
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            _playerController.QuestPoints = TransformationPoints;
        }

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

        /*GENERIC BLOCKERS*/
        if (canTransform == false && tutorialTransform == false)
        {
            return;
        }

        if (_playerController.FirstTransform == true && _playerController.Transforming == false)
        {
            ActivateTransformation();
            return;
        }

        if(tutorialTransform == true)
        {
            if (_playerController.TouchingWater == true)
            {
                EndTutorialTransformation();
            }
            return;

        }

        /*PLAYER TICK*/
        if (_playerController.IsPlayerBig == true)
        {
            TickBeast(fixedDeltaTime, swapDown);
        }
        else
        {
            TickChicken(fixedDeltaTime, swapDown);
        }
    }

    void TickBeast(float fixedDeltaTime, bool swapDown)
    {  
        if (_playerController.TouchingWater == true)
        {
            ActivateTransformation();
            return;
        }

        if (_playerController.IsDoingAction == true)
        {
            return;
        }

        if(swapDown == true)
        {
            ActivateTransformation();
        }               

    }

    void TickChicken(float fixedDeltaTime, bool swapDown)
    {
        if (_playerController.IsDoingAction == true || _playerController.TouchingWater == true)
        {
            return;
        }

        if (swapDown == true)
        {
            ActivateTransformation();
        }
    }

    public void FirstTransformation()
    {
        if (canTransform == true) return;
        canTransform = true;
        _playerController.UpdateTransformUI(canTransform);
        _playerController.FirstTransform = true;
    }

    public void StartTutorialTransformation()
    {
        if (canTransform == true) return;
        tutorialTransform = true;
        _playerController.FirstTransform = true;
    }

    public void EndTutorialTransformation()
    {
        tutorialTransform = false;
        GameManager.instance.SetPlayerExperience(0, 1);
        ActivateTransformation();
    }

    public void ActivateTransformation()
    {
        //Activate Dummy Animation
        string animationToPlay = (_playerController.IsPlayerBig) ? "BeastToChicken" : (_playerController.FirstTransform) ? "FirstTransformation" : "ChickenToBeast";
        float invokeTime = (_playerController.IsPlayerBig) ? 1f : (_playerController.FirstTransform) ? 6.66f : 1f;
        _holderDummy.SetActive(true);
        _animatorDummy.Play("Base Layer." + animationToPlay);

        //Swap Character thingies
        _playerController.SwapCharacter();
        //Placeholder invoke
        Invoke("EndTransformation", invokeTime);

    }

    public void EndTransformation()
    {
        //Deactivate Dummy
        _holderDummy.SetActive(false);

        //End setting the character
        _playerController.FinishSetCharacter();
    }

}
