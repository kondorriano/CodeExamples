using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerModelReposition : MonoBehaviour
{
    //Reposition Data
    private Vector3 _modelOffset;
    private Quaternion _modelRotation;
    private Quaternion _oldModelRotation;
    private float oldAngle;



    //Player Models
    private GameObject _playerBig;
    private GameObject _playerSmall;

    //Components
    private PlayerController _playerController;
    private Rigidbody _rb;

    public void Init(PlayerController pc, GameObject playerBig, GameObject playerSmall)
    {
        //Set Player Controller
        _playerController = pc;

        _modelOffset = transform.position;
        _modelRotation = transform.rotation;
        _oldModelRotation = _modelRotation;
        oldAngle = 0;

        //SetModels
        _playerBig = playerBig;
        _playerSmall = playerSmall;

        //Rig
        _rb = GetComponent<Rigidbody>();
    }

    public void Tick()
    {
        /*
        //InDialogue
        if (_playerController.InDialogue == true)
        {
            return;
        }
        */

        //PlayerDisabled
        if (_playerController.IsPlayerDisabled == true)
        {
            return;
        }

        /*GENERIC BLOCKERS*/

        /*PLAYER TICK*/
        if(_playerController.IsSliding == true)
        {
            if(_playerController.IsPlayerBig == true)
            {                
                if (oldAngle != _playerController.SlopeAngle)
                {
                    _oldModelRotation = _modelRotation;
                    oldAngle = _playerController.SlopeAngle;
                }
            }
            else
            {
                Vector3 forward = -_playerController.SlopeForward;
                forward.y = 0;
                _modelOffset = transform.position;
                _modelRotation = Quaternion.LookRotation(forward, _playerController.GroundNormal);
                _oldModelRotation = _modelRotation;
                oldAngle = _playerController.SlopeAngle;
            }         
        }
        else if (_playerController.TouchingGround == false)
        {
            _modelOffset = transform.position;
            _modelRotation = transform.rotation;
            _rb.velocity = Quaternion.FromToRotation(_oldModelRotation * Vector3.up, _modelRotation * Vector3.up) * _rb.velocity;
            _oldModelRotation = _modelRotation;
            oldAngle = _playerController.SlopeAngle;
            //Debug.DrawRay(transform.position, _rb.velocity.normalized, Color.yellow);
        }
        else if(oldAngle != _playerController.SlopeAngle)
        {
            //Change slope velocity
            //Debug.DrawRay(transform.position, _rb.velocity.normalized, Color.blue);
            _rb.velocity = Quaternion.FromToRotation(_oldModelRotation*Vector3.up, _modelRotation*Vector3.up) * _rb.velocity;
            _oldModelRotation = _modelRotation;
            oldAngle = _playerController.SlopeAngle;
            //Debug.DrawRay(transform.position, _rb.velocity.normalized, Color.white);
        }
        //else Debug.DrawRay(transform.position, _rb.velocity.normalized, Color.blue);

        _playerSmall.transform.position = Vector3.Lerp(_playerSmall.transform.position, _modelOffset, .2f);
        _playerSmall.transform.rotation = Quaternion.Slerp(_playerSmall.transform.rotation, _modelRotation, .2f);

        _playerBig.transform.position = _playerSmall.transform.position;
        _playerBig.transform.rotation = _playerSmall.transform.rotation;

    }

    public void InWallReposition(Quaternion targetRotation, float easing)
    {
        _playerSmall.transform.position = Vector3.Lerp(_playerSmall.transform.position, transform.position, easing);
        _playerSmall.transform.rotation = Quaternion.Slerp(_playerSmall.transform.rotation, targetRotation, easing);
        _playerBig.transform.position = _playerSmall.transform.position;
        _playerBig.transform.rotation = _playerSmall.transform.rotation;
    }

    public void CalculateGroundData(Vector3 hitNormal, Vector3 hitPoint, float hitCounter)
    {
        _modelOffset = transform.position;
        _modelRotation = transform.rotation;

        if (hitCounter == 0)
        {
            //SetSlope Data
            _playerController.SetSlopeData(0, transform.right, transform.forward);
        }
        else
        {
            //PrepareRotation
            Vector3 hitUp = hitNormal;
            Vector3 hitForward = Quaternion.FromToRotation(transform.up, hitUp) * transform.forward;
            _modelRotation = Quaternion.LookRotation(hitForward, hitUp);

            float slopeAngle = Vector3.Angle(Vector3.up, hitNormal);
            //SetSlope Data
            if (slopeAngle != 0)
            {
                Vector3 groundRight = Vector3.Cross(hitNormal, Vector3.up);
                Vector3 groundForward = Vector3.Cross(groundRight, hitNormal);
                _playerController.SetSlopeData(slopeAngle, groundRight.normalized, groundForward.normalized);

                _modelOffset = hitPoint;
            }
            else
            {
                _playerController.SetSlopeData(slopeAngle, transform.right, transform.forward);
            }
        }
    }

    public Vector3 GetModelUp()
    {
        return (_playerController.IsSliding) ? transform.up : _modelRotation*Vector3.up;
    }
}
