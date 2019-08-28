using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEvents : MonoBehaviour
{
    //Components
    private PlayerController _playerController;

    public void Init(PlayerController pc)
    {
        //Set Player Controller
        _playerController = pc;
    }

    void ActionFinished()
    {
        _playerController.ActionFinished();
    }

    void GrabObject()
    {
        _playerController.GrabObject();
    }

    void GrabFinished()
    {
        _playerController.GrabFinished();
    }

    void AttackChickenFinished()
    {
        _playerController.AttackFinished(Variables.PlayerAttackTypes.Chicken);
    }

    void AttackWeakRightFinished()
    {
        _playerController.AttackFinished(Variables.PlayerAttackTypes.BeastWeakRight);
    }

    void ActivateWeakRightAttack()
    {
        _playerController.ActivateWeakAttack(false);
    }

    void AttackWeakLeftFinished()
    {
        _playerController.AttackFinished(Variables.PlayerAttackTypes.BeastWeakLeft);
    }

    void ActivateWeakLeftAttack()
    {
        _playerController.ActivateWeakAttack(true);
    }

    void AttackStrongFinished()
    {
        _playerController.AttackFinished(Variables.PlayerAttackTypes.BeastStrong);
    }

    void ActivateStrongAttack()
    {
        _playerController.ActivateStrongAttack(true);
    }

    void DeactivateStrongAttack()
    {
        _playerController.ActivateStrongAttack(false);
    }

    void EatObject()
    {
        _playerController.EatObject();
    }

    void MunchObject()
    {
        _playerController.MunchObject();
    }

    void ThrowObject()
    {
        _playerController.ThrowObject();
    }

    void ActivateBok()
    {
        _playerController.ActivateBok();
    }

    void ActivateRoarParticles()
    {
        _playerController.ActivateRoarParticles();
    }

    void BokFinished()
    {
        _playerController.AttackFinished((_playerController.IsPlayerBig) ? Variables.PlayerAttackTypes.BeastBok : Variables.PlayerAttackTypes.ChickenBok);
    }

    void EndDeath()
    {
        //_playerController.DestroyPlayer();
    }

    void EndDrown()
    {
       _playerController.RespawnDrownPlayer();
    }

    void EndChargeStrongAttack()
    {
        _playerController.TryStartStrongAttack();
    }

    public void StartFootStepLeft(AnimationEvent evt)
    {
        if(evt.animatorClipInfo.weight > .5f)
        {
            _playerController.SetFootCollision(true);
        }
    }

    public void StartFootStepRight(AnimationEvent evt)
    {
        if (evt.animatorClipInfo.weight > .5f)
        {
            _playerController.SetFootCollision(false);
        }
    }

    public void SwimStroke(AnimationEvent evt)
    {
        if (evt.animatorClipInfo.weight > .5f)
        {
            _playerController.SwimSound();
        }
    }

    public void NotEatSound()
    {
        _playerController.PlayNotEatSound();
    }

    public void MunchSound()
    {
        _playerController.TryEatObject();
    }

}
