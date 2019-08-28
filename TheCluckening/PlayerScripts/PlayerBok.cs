using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBok : MonoBehaviour
{
    //Components
    private PlayerController _playerController;

    public void Init(PlayerController pc)
    {
        //Set Player Controller
        _playerController = pc;
    }

    public void Tick(bool bokDown)
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
        if (_playerController.IsDoingAction == true)
        {
            return;
        }

        /*PLAYER TICK*/
        if (_playerController.IsPlayerBig == true)
        {
            TickBeast(bokDown);
        }
        else
        {
            TickChicken(bokDown);
        }
    }

    void TickBeast(bool bokDown)
    {
        //Player in ground
        if (_playerController.TouchingGround == true)
        {
            if (bokDown == true)
            {
                _playerController.StartBokAttack(true);
                _playerController.StartPlayerAction(true);
                _playerController.SetAction("Bok");
            }
        }
    }

    void TickChicken(bool bokDown)
    {
        if (bokDown == true)
        {
            _playerController.StartBokAttack(_playerController.GetBokModifier());
            _playerController.StartPlayerAction(false);
            _playerController.SetAction("Bok");
        }
    }  
    
}
