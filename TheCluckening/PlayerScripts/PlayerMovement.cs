using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerMovement : MonoBehaviour
{
    //Movement Data
    [Header("Big Character Movement Settings")]
    [SerializeField] private float _movementSpeedBig = 8;
    [SerializeField] private float _accelerationBig = 10;
    [SerializeField] private float _decelerationBig = 10;

    //Air Movement
    [SerializeField] private float _airSpeedMultiplierBig = .5f;

    [Header("Small Character Movement Settings")]
    [SerializeField] private float _movementSpeedSmall = 8;
    [SerializeField] private float _accelerationSmall = 8;
    [SerializeField] private float _decelerationSmall = 10;

    //Air Movement
    [SerializeField] private float _airSpeedMultiplierSmall = .25f;

    [Header("General Movement Settings")]
    [SerializeField] private float _momentumBoostSpeed = .25f;

    //Components
    private PlayerController _playerController;
    private Rigidbody _rb;

    public void Init(PlayerController pc)
    {
        //Set Player Controller
        _playerController = pc;
        //Rigidbody
        _rb = GetComponent<Rigidbody>();
        _rb.useGravity = false;        
    }

    

    public void Tick(float fixedDeltaTime)
    {
        //PlayerDisabled
        if (_playerController.IsPlayerDisabled == true)
        {
            return;
        }

        /*GENERIC BLOCKERS*/

        Vector3 movement = _playerController.MovementInput;
        //Not moving if action being done
        if (_playerController.IsPlayerLocked == true || _playerController.InDialogue == true || _playerController.IsPlayerEndLocked == true)
        {
            movement = Vector3.zero;
        }

        //Momentum check
        if(movement.magnitude == 0)
        {
            _playerController.Momentum -= fixedDeltaTime*_momentumBoostSpeed;
        }
        else
        {
            _playerController.Momentum += fixedDeltaTime * _momentumBoostSpeed;
        }       
        
        /*PLAYER TICK*/
        if(_playerController.IsPlayerBig == true)
        {
            TickBeast(fixedDeltaTime, movement);
        }
        else
        {
            TickChicken(fixedDeltaTime, movement);
        }
    }

    public void TickChicken(float fixedDeltaTime, Vector3 movement)
    {
        //_rb.velocity = normalVelocity;

        //Prepare Movement Input
        float speedPowerUp = _playerController.GetSpeedModifier();
        //ApplyMovement
        AccelerateToVelocity(movement, _movementSpeedSmall*speedPowerUp*_playerController.Momentum, _accelerationSmall, _decelerationSmall, _airSpeedMultiplierSmall);
        //SetRotation
        ApplyRotation(movement);
    }

    public void TickBeast(float fixedDeltaTime, Vector3 movement)
    {
        if(_playerController.Jumping == false)
        {
            //Prepare Movement Input
            //ApplyMovement
            AccelerateToVelocity(movement, _movementSpeedBig, _accelerationBig, _decelerationBig, _airSpeedMultiplierBig);

            //SetRotation
            ApplyRotation(movement);
        } else
        {
            AccelerateToVelocity(transform.forward, _movementSpeedBig, _accelerationBig, _decelerationBig, _airSpeedMultiplierBig*5);
        }

    }

    public void SetMovementInput(Vector2 moveInput)
    {
        //Prepare camera directions
        Vector2 frontDir = new Vector2(_playerController.playerCamera.forward.x, _playerController.playerCamera.forward.z);
        frontDir.Normalize();
        Vector2 rightDir = new Vector2(_playerController.playerCamera.right.x, _playerController.playerCamera.right.z);
        rightDir.Normalize();

        //Create direction vector with camera and input
        Vector2 dir = rightDir * moveInput.x + frontDir * moveInput.y;
        if (dir.magnitude > 1)
        {
            dir.Normalize();
        }

        //Set movement Input vector
        Vector3 movementInput = new Vector3(dir.x, 0, dir.y);

        //Check if jumping
        if(_playerController.Jumping == true)
        {
            Vector3 jumpDir = _playerController.JumpDirection;
            jumpDir.y = 0;
            float deficit = Vector3.Dot(movementInput.normalized, jumpDir.normalized);
            if (deficit < 0) movementInput -= jumpDir.normalized * deficit * movementInput.magnitude;
            _playerController.IsSliding = false;
        }
        //Chcek if on air and colliding onto wall
        else if (_playerController.AirWallColliding == true)
        {
            Vector3 wallNormal = _playerController.GroundNormal;
            wallNormal.y = 0;
            float deficit = Vector3.Dot(movementInput.normalized, wallNormal.normalized);
            if (deficit < 0) movementInput -= wallNormal.normalized * deficit * movementInput.magnitude;
            _playerController.IsSliding = true;
        }
        else _playerController.IsSliding = false;

        //Check if movement is negligible
        if (movementInput.sqrMagnitude < .1)
        {
            //movementInput = Vector3.zero;
        }

        _playerController.MovementInput = movementInput;
        //if (Input.GetButtonDown("A1")) isPressed = !isPressed;
        //if(isPressed)_playerController.MovementInput = new Vector3(0, 0, -1);
    }

    void AccelerateToVelocity(Vector3 movement, float speed, float acceleration, float deceleration, float airSpeedMultiplier)
    {

        //Get difference of velocity from what you have and the one you want to get
        float deltaX;
        float deltaZ;
        float accX;
        float accZ;
        Quaternion offsetRotation = Quaternion.FromToRotation(Vector3.up, _playerController.PlayerUp);

        //Air Movement
        if (_playerController.TouchingGround == false)
        {
            deltaX = (movement.x * speed * airSpeedMultiplier) - _rb.velocity.x;
            deltaZ = (movement.z * speed * airSpeedMultiplier) - _rb.velocity.z;
            accX = (CheckDecelerate(deltaX, _rb.velocity.x) == true) ? deceleration : acceleration;
            accZ = (CheckDecelerate(deltaZ, _rb.velocity.z) == true) ? deceleration : acceleration;
            _rb.AddForce(offsetRotation * new Vector3(deltaX*accX, 0, deltaZ*accZ));
        }
        //Ground Movement
        else
        {
            deltaX = (movement.x * speed) - _rb.velocity.x;
            deltaZ = (movement.z * speed) - _rb.velocity.z;
            accX = (CheckDecelerate(deltaX, _rb.velocity.x) == true) ? deceleration : acceleration;
            accZ = (CheckDecelerate(deltaZ, _rb.velocity.z) == true) ? deceleration : acceleration;
            _rb.AddForce(offsetRotation * new Vector3(deltaX * accX, 0, deltaZ * accZ), ForceMode.Acceleration);
        }
    }

    bool CheckDecelerate(float delta, float velocity) 
    {
        bool same = delta > 0 && velocity > 0 || delta < 0 && velocity < 0;
        //Check zero 
        if(same == false)
        {
            if (delta == 0) return true;
            if (velocity != 0) return true;
        }
        return false;
    }

    void ApplyRotation(Vector3 movement)
    {
        if(_playerController.IsSliding == true)
        {
            Vector3 forward = -_playerController.SlopeForward;
            forward.y = 0;
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(forward, Vector3.up), 0.25f);
        }
        else
        {
            if (movement.sqrMagnitude > 0)
            {
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(movement, Vector3.up), 0.25f);
            }
            else
            {
                Vector3 forwardDir = transform.forward;
                forwardDir.y = 0;
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(forwardDir, Vector3.up), 0.25f);
            }
        }        
    }
}
