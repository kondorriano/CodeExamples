using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerObjectHandler : MonoBehaviour
{
    [Header("Big Character Throwing Settings")]
    [SerializeField] private float _throwForceBig = 80;
    [Header("Small Character Throwing Settings")]
    [SerializeField] private float _throwForceSmall = 15;



    //Components
    private PlayerController _playerController;
    Rigidbody _rb;

    public void Init(PlayerController pc)
    {
        //Set Player Controller
        _playerController = pc;
        //Rigidbody
        _rb = GetComponent<Rigidbody>();
    }

    public void Tick(bool eatDown, bool bokDown, bool grabDown)
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

        /*GENERIC BLOCKERS*/

        //No item
        if (_playerController.GrabbedObject == null)
        {
            return;
        }

        //In an action
        if (_playerController.IsDoingAction == true)
        {
            return;
        }
        
        /*PLAYER TICK*/
        if (_playerController.IsPlayerBig == true)
        {
            TickBeast(eatDown, bokDown, grabDown);
        }
        else
        {
            TickChicken(eatDown, bokDown, grabDown);
        }        
    }

    public void TickChicken(bool eatDown, bool bokDown, bool grabDown)
    {
        if (bokDown == true)
        {
            DropObject();
        }
        else if (eatDown == true)
        {
            _playerController.StartPlayerAction(true);
            if (_playerController.GrabbedObject.IsEatable == true)
            {
                _playerController.SetAction("Eat");
            }
            else
            {
                _playerController.SetAction("NotEat");
                _playerController.PlayNotEatSound();
            }
        } else if(grabDown == true)
        {
            _playerController.SetThrowRotation();
            _playerController.StartPlayerAction(true);
            _playerController.SetAction("Throw");
        }
    }
    public void TickBeast(bool eatDown, bool bokDown, bool grabDown)
    {

        //Not in ground
        if (_playerController.TouchingGround == false)
        {
            return;
        }

        if (bokDown == true)
        {
            DropObject();
        }
        else if (eatDown == true)
        {
            _playerController.StartPlayerAction(true);
            if(_playerController.GrabbedObject.IsEatable == true)
            {
                _playerController.SetAction("Eat");
                _playerController.SetAction("EatArms");
            }
            else
            {
                _playerController.SetAction("NotEat");
                _playerController.SetAction("NotEatArms");
            }
        } else if(grabDown == true)
        {
            _playerController.SetThrowRotation();
            _playerController.StartPlayerAction(true);
            _playerController.SetAction("Throw");
            _playerController.SetAction("ThrowArms");
        }
    }

    public void SwapCharacters(bool swapToSmall)
    {
        if (_playerController.GrabbedObject != null)
        {
            DropObject();
        }
    }


    public void ThrowObject(Vector3 throwDirection)
    {
        float throwHeight = throwDirection.y;
        if (_playerController.IsPlayerBig == false) throwHeight = Mathf.Max(0, throwHeight);
        throwDirection = transform.forward;
        throwDirection.y = throwHeight + throwDirection.y * .5f;
        throwDirection.Normalize();

        /*
        if (_playerController.IsPlayerBig == false)
        {
            throwDirection.y = Mathf.Max(0, throwDirection.y);
            throwDirection += transform.forward*.5f;
            throwDirection.Normalize();            
        }
        */

        float throwModifier = _playerController.GetThrowModifier();
        Vector3 throwVelocity = throwDirection * ((_playerController.IsPlayerBig) ? _throwForceBig : _throwForceSmall*throwModifier);

        _playerController.GrabbedObject.Release(throwVelocity, false, _playerController.IsPlayerBig);
        _playerController.GrabbedObject = null;
    }

    void DropObject()
    {
        _playerController.GrabbedObject.Release(_rb.velocity, true, false);
        _playerController.GrabbedObject = null;
    }

    public void PlayerDied()
    {
        if (_playerController.GrabbedObject != null) DropObject();    
    }
}
