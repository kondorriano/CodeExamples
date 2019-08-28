using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerExtraBehaviours : MonoBehaviour, ICannonAmmo
{
    [SerializeField] private CannonDummyBehaviour _cannonDummy = null;

    //Components
    private PlayerController _playerController;

    public void Init(PlayerController pc)
    {
        //Set Player Controller
        _playerController = pc;
        _cannonDummy.Init(this);
    }

    public void KickPlayer(Vector3 direction)
    {
        if (_playerController.IsPlayerBig == true) return;

        if (_playerController.GrabbedObject != null)
        {
            //DROP
            _playerController.GrabbedObject.Release(Vector3.zero, true, false);
            _playerController.GrabbedObject = null;
        }

        //transform.position += Vector3.up;
        _cannonDummy.LoadDummy(transform.position + Vector3.up*_playerController.GetPlayerCenterOffset, transform.rotation);
        _playerController.DisablePlayer();
        _playerController.SetDummyCameraTarget(_cannonDummy.transform);
        _cannonDummy.ShootDummy(direction);
    }

    #region Cannon
    public ICannonAmmo LoadAmmo(Transform newLocation)
    {        

        if(_playerController.GrabbedObject != null)
        {            
            ICannonAmmo objectCA = _playerController.GrabbedObject.GetComponent<ICannonAmmo>();
            //DROP
            _playerController.GrabbedObject.Release(Vector3.zero, true, false);
            _playerController.GrabbedObject = null;
            return objectCA.LoadAmmo(newLocation);
        }

        if (_playerController.IsPlayerBig == true)
        {
            return null;
        }

        _cannonDummy.LoadDummy(newLocation.position, newLocation.rotation);
        _playerController.DisablePlayer();
        _playerController.SetDummyCameraTarget(_cannonDummy.transform);
        return this;
    }

    public void ShootAmmo(Vector3 direction)
    {
        _cannonDummy.ShootDummy(direction);
    }

    public bool IsAmmoPlayer()
    {
        return true;
    }

    public void DummyCollision()
    {
        _cannonDummy.gameObject.SetActive(false);
        transform.position = _cannonDummy.transform.position - Vector3.up * _playerController.GetPlayerCenterOffset*.5f;
        _playerController.EnablePlayer();
        _playerController.PlayRecoverSound();
        _playerController.SetDummyCameraTarget(null);

    }

    public void TryDummyJump()
    {
        _cannonDummy.TryJump();
    }
    #endregion
}
