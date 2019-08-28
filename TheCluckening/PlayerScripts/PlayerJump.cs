using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJump : MonoBehaviour
{
    //Jump
    [Header("Big Character Jump Settings")]
    [SerializeField] private float _leapForceBig = 6;
    [SerializeField] private float _leapUpTimeBig = .25f;
    [SerializeField] private float _leapDownTimeBig = .125f;


    [Header("Small Character Jump Settings")]
    [SerializeField] private float _jumpForceSmall = 4f;
    [SerializeField] private float _jumpMinTimeSmall = .15f;
    [SerializeField] private float _jumpMaxTimeSmall = .25f;

    private float _jumpTime;
    private float _jumpCounter;
    private bool _leapUp = false;

    public Vector3 JumpDirectionUsed
    {
        get;
        private set;
    }

    //Gravity
    [Header("Big Character Gravity Settings")]
    [SerializeField] private float _playerGravityBig = -40f;
    [SerializeField] private float _downVelocityGlideBig = -10f;
    [SerializeField] private float _floorGravityDragMultiplierBig = .4f;


    [Header("Small Character Gravity Settings")]
    [SerializeField] private float _playerGravitySmall = -20f;
    [SerializeField] private float _downVelocityGlideSmall = -7.5f;
    [SerializeField] private float _floorGravityDragMultiplierSmall = .05f;

    //Components
    private PlayerController _playerController;
    private Rigidbody _rb;

    public void Init(PlayerController pc)
    {
        //Set Player Controller
        _playerController = pc;
        //Rigidbody
        _rb = GetComponent<Rigidbody>();
    }

    public void Tick(float fixedDeltaTime, bool jumpDown, bool jumpUp, bool jumpStay)
    {
        /*PLAYER TICK*/
        if (_playerController.IsPlayerBig == true)
        {
            TickBeast(fixedDeltaTime, jumpDown, jumpUp, jumpStay);
        }
        else
        {
            TickChicken(fixedDeltaTime,jumpDown, jumpUp, jumpStay);
        }
    }

    void TickBeast(float fixedDeltaTime, bool jumpDown, bool jumpUp, bool jumpStay)
    {
        //InDialogue
        if (_playerController.InDialogue == true || _playerController.IsPlayerEndLocked == true)
        {
            ApplyGravity(_playerGravityBig, 1f);
            return;
        }

        //PlayerDisabled
        if (_playerController.IsPlayerDisabled == true)
        {
            return;
        }
        
        //Handle Leap
        HandleLeap(fixedDeltaTime);

        //ON AIR
        if (_playerController.TouchingGround == false || _leapUp == true)
        {
            //JUMPING
            if (_leapUp == true)
            {
                ApplyJump();
            }
            //SLIDING???
            //FALLING
            else
            {
                ApplyGravity(_playerGravityBig, 1f);
                //HANDLE GLIDE
                if (jumpStay == true)
                {
                    HandleDownVelocity(_downVelocityGlideBig);
                }

                _playerController.SetGliding(jumpStay);
            }
            return;
        }

        //ON GROUND, NOT JUMPING  AND NOT DOING AN ACTION
        _playerController.SetGliding(false);

        //Beast Jump
        if (jumpDown == true && _playerController.BeastCanJump == true)
        {
            StartJump(_leapForceBig, _leapUpTimeBig);
            _leapUp = true;
        }
        //Apply Ground Gravity
        else
        {
            ApplyGravity(_playerGravityBig, _floorGravityDragMultiplierBig);
        }
    }

    void TickChicken(float fixedDeltaTime, bool jumpDown, bool jumpUp, bool jumpStay)
    {
        //InDialogue
        if (_playerController.InDialogue == true || _playerController.IsPlayerEndLocked == true)
        {
            ApplyGravity(_playerGravitySmall, 1f);
            return;
        }

        //PlayerDisabled
        if (_playerController.IsPlayerDisabled == true)
        {
            if (jumpDown == true)
            {
                _playerController.TryDummyJump();
                if(_playerController.IsPlayerDisabled == false)
                {
                    float jumpPowerUp = _playerController.GetJumpModifier();
                    StartJump(_jumpForceSmall * jumpPowerUp, _jumpMaxTimeSmall);
                }
            }
            return;
        }

        //Handle Release jump button
        if (_playerController.Jumping == true && jumpUp == true)
        {
            _jumpTime = _jumpMinTimeSmall;
        }
        //Handle jumping
        HandleJumping(fixedDeltaTime);

        //ON AIR
        if (_playerController.TouchingGround == false || _playerController.Jumping == true)
        {
            //JUMPING
            if (_playerController.Jumping == true)
            {
                ApplyJump();
            }
            //SLIDING
            else if(_playerController.IsSliding == true)
            {
                //Chicken Slide Jump
                if (jumpDown == true && _playerController.IsPlayerLocked == false)
                {
                    float jumpPowerUp = _playerController.GetJumpModifier();
                    StartSlideJump(_jumpForceSmall * jumpPowerUp, _jumpMaxTimeSmall);
                }
                else
                {
                    ApplyGravity(_playerGravitySmall, 1f);
                    HandleDownVelocity(_downVelocityGlideSmall);
                }
            }
            //FALLING
            else
            {
                ApplyGravity(_playerGravitySmall, 1f);
                //HANDLE GLIDE
                if (jumpStay == true)
                {
                    //GLIDING
                    float glidePowerUp = _playerController.GetGlideModifier();
                    HandleDownVelocity(_downVelocityGlideSmall * glidePowerUp);
                }

                _playerController.SetGliding(jumpStay);
            }
            return;
        }

        //ON GROUND, NOT JUMPING  AND NOT DOING AN ACTION
        _playerController.SetGliding(false);
        //Chicken Jump
        if (jumpDown == true && _playerController.IsPlayerLocked == false && _playerController.TouchingWater == false)
        {
            float jumpPowerUp = _playerController.GetJumpModifier();
            StartJump(_jumpForceSmall * jumpPowerUp, _jumpMaxTimeSmall);
        }
        //Apply Ground Gravity
        else
        {
            if(jumpDown == true && _playerController.TouchingWater == true && _playerController.IsDoingAction == false)
            {
                _playerController.StartPlayerAction(true);
                _playerController.SetAction("WaterJump");
            }
            ApplyGravity(_playerGravitySmall, _floorGravityDragMultiplierSmall);
        }
    }

    void HandleLeap(float fixedDeltaTime)
    {
        if (_playerController.Jumping == false)
        {
            return;
        }

        _jumpCounter += fixedDeltaTime;
        if(_jumpCounter >= _jumpTime)
        {
            if (_leapUp == true)
            {
                _leapUp = false;
                _jumpCounter -= _jumpTime;
                _jumpTime = _leapDownTimeBig;
            }
            else
            {
                _playerController.Jumping = false;
            }
        }
    }

    void HandleJumping(float fixedDeltaTime)
    {
        if (_playerController.Jumping == false)
        {
            return;
        }

        _jumpCounter += fixedDeltaTime;
        _playerController.Jumping = (_jumpCounter < _jumpTime);
    }

    void ApplyJump()
    {
        
        Vector3 velocity = _rb.velocity;
        float deficit = Vector3.Dot(velocity.normalized, JumpDirectionUsed.normalized);
        velocity -= JumpDirectionUsed.normalized * deficit * velocity.magnitude;
        velocity += JumpDirectionUsed;
        _rb.velocity = velocity;
        
        //_rb.velocity = new Vector3(_rb.velocity.x, _jumpDirectionUsed.magnitude, _rb.velocity.z);
    }

    void StartJump(float jumpVelocity, float playerJumpTime)
    {
        JumpDirectionUsed = Vector3.up * jumpVelocity;
        ApplyJump();
        _playerController.Jumping = true;
        _jumpTime = playerJumpTime;
        _jumpCounter = 0;
        _playerController.ApplyJumpBurst();
    }

    void StartSlideJump(float jumpVelocity, float playerJumpTime)
    {
        JumpDirectionUsed = _playerController.GroundNormal * jumpVelocity;
        ApplyJump();
        _playerController.Jumping = true;
        _jumpTime = playerJumpTime;
        _jumpCounter = 0;
        _playerController.ApplyJumpBurst();
    }

    void ApplyGravity(float playerGravity, float playerDrag)
    {
        Quaternion offsetRotation = Quaternion.FromToRotation(Vector3.up, _playerController.PlayerUp);

        Vector3 gravity = playerGravity /** playerDrag*/ * Vector3.up;        
        _rb.AddForce(offsetRotation*gravity, ForceMode.Acceleration);
    }

    void HandleDownVelocity(float downVelocity)
    {
        Vector3 velocity = _rb.velocity;
        if (velocity.y < downVelocity) velocity.y = downVelocity;
        _rb.velocity = velocity;
    }

    public void SwapCharacters(bool isPlayerBig)
    {
        _playerController.Jumping = false;
        _leapUp = false;
    }
}
