using SO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


//Basic Components
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
//Player Scripts
[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerJump))]
[RequireComponent(typeof(PlayerGrab))]
[RequireComponent(typeof(PlayerModelReposition))]
[RequireComponent(typeof(PlayerObjectHandler))]
[RequireComponent(typeof(PlayerTransformation))]
[RequireComponent(typeof(PlayerStats))]
[RequireComponent(typeof(PlayerAudio))]
[RequireComponent(typeof(PlayerExtraBehaviours))]
[RequireComponent(typeof(PlayerBok))]
[RequireComponent(typeof(PlayerInteractions))]
[RequireComponent(typeof(PlayerParticles))]
public class PlayerController : MonoBehaviour
{
    [System.Serializable]
    public class InputSettings
    {
        public bool isXBox = true;

        public Variables.InputJoystickList MOVE = Variables.InputJoystickList.LJoystick;
        public Variables.InputJoystickList CAMERA = Variables.InputJoystickList.RJoystick;

        public bool cameraInvertedVertical = false;
        public bool cameraInvertedHorizontal = false;

        public Variables.InputButtonList JUMP = Variables.InputButtonList.A;
        public Variables.InputButtonList ATTACK = Variables.InputButtonList.X;
        public Variables.InputButtonList GRAB = Variables.InputButtonList.RTrigger;
        public Variables.InputButtonList THROW = Variables.InputButtonList.RTrigger;
        public Variables.InputButtonList DROP = Variables.InputButtonList.Y;
        public Variables.InputButtonList EAT = Variables.InputButtonList.B;
        public Variables.InputButtonList BOK = Variables.InputButtonList.Y;
        public Variables.InputButtonList TRANSFORM = Variables.InputButtonList.L;
        public Variables.InputButtonList INTERACT = Variables.InputButtonList.A;
        public Variables.InputButtonList[] ACCEPT =
        {
            Variables.InputButtonList.A,
            Variables.InputButtonList.B,
            Variables.InputButtonList.X,
            Variables.InputButtonList.Y,
        };
        public Variables.InputButtonList PAUSE = Variables.InputButtonList.Start;

    }

    class InputData
    {
        public string controller = ""; 
        public float valueLTrigger = 0;
        public float valueRTrigger = 0;

        public Vector2[] joystickInput;
        public bool[] downInput;
        public bool[] upInput;
        public bool[] stayInput;


        public int moveIndex;
        public int cameraIndex;
        public bool camInvV;
        public bool camInvH;
        public int jumpIndex;
        public int attackIndex;
        public int grabIndex;
        public int throwIndex;
        public int dropIndex;
        public int eatIndex;
        public int bokIndex;
        public int transformIndex;
        public int interactIndex;
        public int[] acceptIndex;
        public int pauseIndex;
    }

    class PlayerData
    {
        //Ground
        public bool touchingGround = false;
        public bool jumping = false;
        public bool landed = false;

        //Water
        public int waterLayer = 0;
        public bool touchingWater = false;

        //Action
        public bool doingAction = false;
        public bool actionIsLocking = false;
        public bool actionIsDisabling = false;
        
        //Movement
        public Vector3 movementInput = Vector3.zero;
        public float momentum = 1;
        //SlopeData
        public Vector3 groundPoint = Vector3.zero;
        public Vector3 groundNormal = Vector3.zero;
        public float hitCounter = 0;

        public float slopeAngle = 0;
        public Vector3 slopeRight = Vector3.zero;
        public Vector3 slopeForward = Vector3.zero;
        //Sliding
        public bool isSliding = false;
        //Grabbing
        public BasicGrabbableScript grabbedObject;
        //Power ups
        public Variables.PlayerModifier activeModifier = Variables.PlayerModifier.None;
        public int modifierLevel = 1;

        //Combat
        public bool chargingAttack = false;

        //Transformation
        public int questPoints = 0;
        public int tutorialPoints = 0;
        public bool firstTransformation = false;
        public bool transforming = false;

        //Music State
        public int intensityState = 0;
        public Variables.Health healthState = Variables.Health.Full;

        //Gliding
        public bool isGliding = false;

        //Dialogue
        public bool inDialogue = false;

        //Beast Jump
        public bool beastCanJump = false;

        //EndGame
        public bool inEndLockState = false;
        public bool inCredits = false;
        public float creditsTransition = 0;
        public float creditsAnimationTime = 0;
    }

    class PlayerScripts
    {
        public PlayerMovement playerMovement;
        public PlayerJump playerJump;
        public PlayerGrab playerGrab;
        public PlayerModelReposition playerModelReposition;
        public PlayerObjectHandler playerObjectHandler;
        public PlayerTransformation playerTransformation;
        public PlayerStats playerStats;
        public PlayerAudio playerAudio;
        public PlayerExtraBehaviours playerExtraBehaviours;
        public PlayerBok playerBok;
        public PlayerInteractions playerInteractions;
        public PlayerParticles playerParticles;
        public CameraController cameraController;
    }

    [Header("Player Input Data")]
    [SerializeField] private InputSettings _inputSettings = null;
    private InputData _inputData;
    private PlayerData _playerData;
    private PlayerScripts _playerScripts;

    //Player Animation Data
    [Header("Player Animation Data")]
    [SerializeField] private GameObject _playerModelHolderBig = null;
    [SerializeField] private GameObject _playerModelHolderSmall = null;
    [SerializeField] private SkinnedMeshRenderer[] _skinnedRenderers = null;
    private Animator _playerAnimatorBig;
    private Animator _playerAnimatorSmall;

    [Header("Slope Data")]
    [SerializeField] private float _maxWalkAngleSmall = 65;
    [SerializeField] private float _maxWalkAngleBig = 50;

    //Player Collision Data
    [Header("Player Collision Data")]
    [SerializeField] private CapsuleCollider _capsuleColliderBig = null;
    [SerializeField] private CapsuleCollider _capsuleColliderSmall = null;
    [SerializeField] private string _playerLayerNameBig = "BossBody";
    [SerializeField] private string _playerLayerNameSmall = "ChickenBody";

    //Camera Data
    public Transform playerCamera;

    //Player Stats
    private bool startAsBeast = false;

    //Rigidbody
    private Rigidbody _rb;
    //Ground Check Data
    private float _colliderBottomOffset;
    private float _groundSideOffset;

    [SerializeField] private float _characterScaling = .125f;
    private float _characterScale = 1;
    [SerializeField] private LayerMask _groundMaskBig = 0;
    [SerializeField] private LayerMask _groundMaskSmall = 0;


    

    


    [Header("Player Events")]
    [SerializeField] private GameEvent _deathEvent = null;
    [SerializeField] private GameEvent _bokEvent = null;

    #region Player Data Checks
    #region GROUNDED, JUMPING, LANDING

    public bool TouchingGround
    {
        get { return _playerData.touchingGround; }
        set { _playerData.touchingGround = value; }
    }

    public bool TouchingWater
    {
        get { return _playerData.touchingWater; }
        private set { _playerData.touchingWater = value; }
    }

    public bool Jumping
    {
        get { return _playerData.jumping; }
        set
        {
            _playerData.jumping = value;
            _playerAnimatorSmall.SetBool("Jumping", value);
            _playerAnimatorBig.SetBool("Jumping", value);
        }
    }

    public bool Landed
    {
        get { return _playerData.landed; }
    }

    public Vector3 JumpDirection
    {
        get { return _playerScripts.playerJump.JumpDirectionUsed; }
    }
    #endregion

    #region ACTIONS
    public bool IsDoingAction
    {
        get { return _playerData.doingAction; }
    }

    public bool IsPlayerLocked
    {
        get { return _playerData.actionIsLocking; }
    }

    public bool IsPlayerEndLocked
    {
        get { return _playerData.inEndLockState; }
    }

    public bool InCredits
    {
        get { return _playerData.inCredits; }
    }

    public bool IsPlayerDisabled
    {
        get { return _playerData.actionIsDisabling; }
    }

    public bool IsGliding
    {
        get { return _playerData.isGliding; }
        private set { _playerData.isGliding = value; }
    }

    public bool BeastCanJump
    {
        get { return _playerData.beastCanJump; }
        set { _playerData.beastCanJump = value; }
    }
    #endregion

    #region DEATH
    public bool IsDead { get; private set; } = false;

    #endregion

    #region PLAYER TRANSFORMATION
    public bool IsPlayerBig { get; private set; } = false;

    public float CharacterScale
    {
        get { return _characterScale; }
    }

    public int TutorialPoints
    {
        get { return _playerData.tutorialPoints; }
        set
        {
            if (_playerData.tutorialPoints < _playerScripts.playerTransformation.TransformationPoints && value >= _playerScripts.playerTransformation.TransformationPoints)
            {
                _playerScripts.playerTransformation.StartTutorialTransformation();
            }
            if (_playerData.tutorialPoints < _playerScripts.playerTransformation.TransformationPoints) GameManager.instance.SetPlayerExperience(value, _playerScripts.playerTransformation.TransformationPoints);
            _playerData.tutorialPoints = value;
        }
    }

    public int QuestPoints
    {
        get { return _playerData.questPoints; }
        set {
            if(_playerData.questPoints < _playerScripts.playerTransformation.TransformationPoints && value >= _playerScripts.playerTransformation.TransformationPoints)
            {
                _playerScripts.playerTransformation.FirstTransformation();
            }
            _playerData.questPoints = value;
            GameManager.instance.SetPlayerExperience(_playerData.questPoints, _playerScripts.playerTransformation.TransformationPoints);
        }
    }
    #endregion

    #region MOVEMENT
    public bool IsPlayerMoving
    {
        get { return _rb.velocity.sqrMagnitude > .64f; }
    }

    public Vector3 MovementInput
    {
        get { return _playerData.movementInput; }
        set { _playerData.movementInput = value; }
    }

    public float Momentum
    {
        get { return _playerData.momentum; }
        set { _playerData.momentum = Mathf.Clamp(value, Variables.PLAYER_MIN_MOMENTUM ,1f); }
    }

    public float Speed
    {
        get
        {
            return _inputData.joystickInput[_inputData.moveIndex].magnitude * GetSpeedModifier() * Momentum;
        }
    }
    #endregion

    #region SLOPES AND SLIDING
    public Vector3 PlayerUp
    {
        get { return _playerScripts.playerModelReposition.GetModelUp(); }
    }

    public float SlopeAngle
    {
        get { return _playerData.slopeAngle; }
    }

    public Vector3 SlopeForward
    {
        get { return _playerData.slopeForward; }
    }

    public bool AirWallColliding
    {
        get { return TouchingGround == false && _playerData.hitCounter > 0; }
    }

    public Vector3 GroundNormal
    {
        get { return _playerData.groundNormal; }
    }

    public void SetSlopeData(float slopeAngle, Vector3 slopeRight, Vector3 slopeForward)
    {
        _playerData.slopeAngle = slopeAngle;
        _playerData.slopeRight = slopeRight;
        _playerData.slopeForward = slopeForward;
    }

    public bool IsSliding
    {
        get { return _playerData.isSliding; }
        set
        {
            if (_playerData.isSliding == value) return;
            _playerData.isSliding = value;
            _playerAnimatorSmall.SetBool("Sliding", value);
            _playerAnimatorBig.SetBool("Sliding", value);
        }
    }
    #endregion
       
    #region GRABBED OBJECTS

    public BasicGrabbableScript GrabbedObject
    {
        get { return _playerData.grabbedObject; }
        set { _playerData.grabbedObject = value; }
    }

    public Variables.InteractableObject GrabbedObjectType
    {
        get
        {
            if (GrabbedObject == null) return Variables.InteractableObject.Select_Object;
            return GrabbedObject.GetObjectId();
        }
    }

    public void DestroyGrabbedObject()
    {
        if (GrabbedObject == null) return;
        GrabbedObject.DestroyQuestObject();
    }

    #endregion

    #region COMBAT AND HEALTH
    public int GetPlayerHealth
    {
        get { return _playerScripts.playerStats.CurrentHealth; }
    }

    public bool ChargingAttack
    {
        get { return _playerData.chargingAttack; }
        set
        {
            if(_playerData.chargingAttack != value)
            {
                _playerAnimatorSmall.SetBool("ChargingAttack", value);
                _playerAnimatorBig.SetBool("ChargingAttack", value);
            }
            _playerData.chargingAttack = value;
        }
    }
    #endregion

    #region MISC
    public float GetPlayerCenterOffset
    {
        get { return _colliderBottomOffset; }
    }
    #endregion

    #region FEAR
    public Variables.FearLevel GetFearLevel
    {
        get
        {
            if(IsPlayerBig == false)
            {
                if (GrabbedObject == null) return Variables.FearLevel.Harmless;
                return GrabbedObject.GetFearLevel();
            }
            return Variables.FearLevel.Kokoroko;
        }
    }
    #endregion

    #region MUSIC
    public int MusicIntensity
    {
        get { return _playerData.intensityState; }
        set
        {
            if(_playerData.intensityState != value)
            {
                //Call game manager
                _playerData.intensityState = value;
            }
        }
    }

    public Variables.Health AudioHealth
    {
        get { return _playerData.healthState; }
        set
        {
            if (_playerData.healthState != value)
            {
                GameManager.instance.SetHealthAudioState(value);
                _playerData.healthState = value;
            }
        }
    }
    #endregion

    #region DIALOGUE
    public bool InDialogue
    {
        get { return _playerData.inDialogue; }
        set
        {
            _playerData.inDialogue = value;
            GameManager.instance.SetDialogueAudioState(value);
        }
    }
    #endregion

    #region TRANSFORM
    public bool FirstTransform
    {
        get { return _playerData.firstTransformation; }
        set { _playerData.firstTransformation = value; }
    }

    public bool Transforming
    {
        get { return _playerData.transforming; }
        set { _playerData.transforming = value; }
    }
    #endregion

    #region MODIFIERS
    public int ModifierLevel
    {
        get { return _playerData.modifierLevel; }
    }

    public Variables.PlayerModifier ActiveModifier
    {
        get { return _playerData.activeModifier; }
    }
    #endregion
    #endregion

    private void Start()
    {
        //GameManager
        if (GameManager.instance == null)
        {
            Instantiate(Resources.Load("GameManager"), transform.position, transform.rotation);
        }

        GameManager.instance.SetPlayerController(this);

        //Rigidbody
        _rb = GetComponent<Rigidbody>();

        //Prepare player start with
        IsPlayerBig = startAsBeast;

        //Initialize Animation Data
        _playerAnimatorBig = _playerModelHolderBig.GetComponent<Animator>();
        _playerModelHolderBig.GetComponent<PlayerEvents>().Init(this);

        _playerAnimatorSmall = _playerModelHolderSmall.GetComponent<Animator>();
        _playerModelHolderSmall.GetComponent<PlayerEvents>().Init(this);

        //Initialize Classes
        _inputData = new InputData();
        _playerData = new PlayerData();
        _playerScripts = new PlayerScripts();

        //Initialize Input
        InitializeInput();

        //Set PS4 Input
        if (_inputSettings.isXBox == false)
        {
            _inputData.controller = "PS4";
        }

        //Set PlayerData variables
        _playerData.momentum = Variables.PLAYER_MIN_MOMENTUM;
        _playerData.waterLayer = LayerMask.NameToLayer("Water");

        /*Component Initialization*/
        //Player Bok
        _playerScripts.playerBok = GetComponent<PlayerBok>();
        _playerScripts.playerBok.Init(this);
        //Player Interactions
        _playerScripts.playerParticles = GetComponent<PlayerParticles>();
        _playerScripts.playerParticles.Init(this);
        //Player Interactions
        _playerScripts.playerInteractions = GetComponent<PlayerInteractions>();
        _playerScripts.playerInteractions.Init(this);
        //Player Extra Behaviours
        _playerScripts.playerExtraBehaviours = GetComponent<PlayerExtraBehaviours>();
        _playerScripts.playerExtraBehaviours.Init(this);
        //Player Audio
        _playerScripts.playerAudio = GetComponent<PlayerAudio>();
        _playerScripts.playerAudio.Init(this, _playerModelHolderBig, _playerModelHolderSmall);
        //Player Stats
        _playerScripts.playerStats = GetComponent<PlayerStats>();
        _playerScripts.playerStats.Init(this);
        //Camera
        if (playerCamera == null)
        {
            playerCamera = Camera.main.transform;
        }
        _playerScripts.cameraController = playerCamera.parent.GetComponent<CameraController>();
        _playerScripts.cameraController.Init(this);
        //Movement
        _playerScripts.playerMovement = GetComponent<PlayerMovement>();
        _playerScripts.playerMovement.Init(this);
        //Jump
        _playerScripts.playerJump = GetComponent<PlayerJump>();
        _playerScripts.playerJump.Init(this);

        //Grab
        _playerScripts.playerGrab = GetComponent<PlayerGrab>();
        _playerScripts.playerGrab.Init(this);

        //ObjectHandler
        _playerScripts.playerObjectHandler = GetComponent<PlayerObjectHandler>();
        _playerScripts.playerObjectHandler.Init(this);

        //Transform
        _playerScripts.playerTransformation = GetComponent<PlayerTransformation>();
        _playerScripts.playerTransformation.Init(this);

        //Model Reposition
        _playerScripts.playerModelReposition = GetComponent<PlayerModelReposition>();
        _playerScripts.playerModelReposition.Init(this, _playerModelHolderBig, _playerModelHolderSmall);

        //ResizeCharacter
        SetCharacter();
    }

    #region Input
    void InitializeInput()
    {
        _inputData.joystickInput = new Vector2[(int)Variables.InputJoystickList.Length];
        for (int i = 0; i < _inputData.joystickInput.Length; i++) _inputData.joystickInput[i] = Vector2.zero;

        _inputData.downInput = new bool[(int)Variables.InputButtonList.Length];
        _inputData.upInput = new bool[(int)Variables.InputButtonList.Length];
        _inputData.stayInput = new bool[(int)Variables.InputButtonList.Length];
        for (int i = 0; i < _inputData.downInput.Length; i++)
        {
            _inputData.downInput[i] = false;
            _inputData.upInput[i] = false;
            _inputData.stayInput[i] = false;
        }

        _inputData.moveIndex = (int)_inputSettings.MOVE;
        _inputData.cameraIndex = (int)_inputSettings.CAMERA;
        _inputData.camInvV = _inputSettings.cameraInvertedHorizontal;
        _inputData.camInvH = _inputSettings.cameraInvertedVertical;
        _inputData.jumpIndex = (int)_inputSettings.JUMP;
        _inputData.attackIndex = (int)_inputSettings.ATTACK;
        _inputData.grabIndex = (int)_inputSettings.GRAB;
        _inputData.throwIndex = (int)_inputSettings.THROW;
        _inputData.dropIndex = (int)_inputSettings.DROP;
        _inputData.eatIndex = (int)_inputSettings.EAT;

        _inputData.bokIndex = (int)_inputSettings.BOK;
        _inputData.transformIndex = (int)_inputSettings.TRANSFORM;
        _inputData.interactIndex = (int)_inputSettings.INTERACT;
        _inputData.pauseIndex = (int)_inputSettings.PAUSE;

        _inputData.acceptIndex = new int[_inputSettings.ACCEPT.Length];

        for (int i = 0; i < _inputData.acceptIndex.Length; i++)
        {
            _inputData.acceptIndex[i] = (int)_inputSettings.ACCEPT[i];
        } 
    }

    void GetUpdateInput()
    {
        //A = 0
        _inputData.stayInput[0] = Input.GetButton(_inputData.controller + Variables.ABUTTON);
        if (Input.GetButtonUp(_inputData.controller + Variables.ABUTTON)) _inputData.upInput[0] = true;
        if (Input.GetButtonDown(_inputData.controller + Variables.ABUTTON)) _inputData.downInput[0] = true;
        //B = 1
        _inputData.stayInput[1] = Input.GetButton(_inputData.controller + Variables.BBUTTON);
        if (Input.GetButtonUp(_inputData.controller + Variables.BBUTTON)) _inputData.upInput[1] = true;
        if (Input.GetButtonDown(_inputData.controller + Variables.BBUTTON)) _inputData.downInput[1] = true;
        //X = 2
        _inputData.stayInput[2] = Input.GetButton(_inputData.controller + Variables.XBUTTON);
        if (Input.GetButtonUp(_inputData.controller + Variables.XBUTTON)) _inputData.upInput[2] = true;
        if (Input.GetButtonDown(_inputData.controller + Variables.XBUTTON)) _inputData.downInput[2] = true;
        //Y = 3
        _inputData.stayInput[3] = Input.GetButton(_inputData.controller + Variables.YBUTTON);
        if (Input.GetButtonUp(_inputData.controller + Variables.YBUTTON)) _inputData.upInput[3] = true;
        if (Input.GetButtonDown(_inputData.controller + Variables.YBUTTON)) _inputData.downInput[3] = true;
        //L = 4
        _inputData.stayInput[4] = Input.GetButton(_inputData.controller + Variables.LBUTTON);
        if (Input.GetButtonUp(_inputData.controller + Variables.LBUTTON)) _inputData.upInput[4] = true;
        if (Input.GetButtonDown(_inputData.controller + Variables.LBUTTON)) _inputData.downInput[4] = true;
        //R = 5
        _inputData.stayInput[5] = Input.GetButton(_inputData.controller + Variables.RBUTTON);
        if (Input.GetButtonUp(_inputData.controller + Variables.RBUTTON)) _inputData.upInput[5] = true;
        if (Input.GetButtonDown(_inputData.controller + Variables.RBUTTON)) _inputData.downInput[5] = true;
        //Start = 6
        _inputData.stayInput[6] = Input.GetButton(_inputData.controller + Variables.START);
        if (Input.GetButtonUp(_inputData.controller + Variables.START)) _inputData.upInput[6] = true;
        if (Input.GetButtonDown(_inputData.controller + Variables.START)) _inputData.downInput[6] = true;
        //Select = 7
        _inputData.stayInput[7] = Input.GetButton(_inputData.controller + Variables.SELECT);
        if (Input.GetButtonUp(_inputData.controller + Variables.SELECT)) _inputData.upInput[7] = true;
        if (Input.GetButtonDown(_inputData.controller + Variables.SELECT)) _inputData.downInput[7] = true;
        //L3 = 8
        _inputData.stayInput[8] = Input.GetButton(_inputData.controller + Variables.L3);
        if (Input.GetButtonUp(_inputData.controller + Variables.L3)) _inputData.upInput[8] = true;
        if (Input.GetButtonDown(_inputData.controller + Variables.L3)) _inputData.downInput[8] = true;
        //R3 = 9
        _inputData.stayInput[9] = Input.GetButton(_inputData.controller + Variables.R3);
        if (Input.GetButtonUp(_inputData.controller + Variables.R3)) _inputData.upInput[9] = true;
        if (Input.GetButtonDown(_inputData.controller + Variables.R3)) _inputData.downInput[9] = true;
    }

    void EndUpdateInput()
    {
        int maxIndex = (int)Variables.InputButtonList.LTrigger;
        for (int i = 0; i < maxIndex; i++)
        {
            _inputData.upInput[i] = false;
            _inputData.downInput[i] = false;
        }
    }

    void GetFixedInput()
    {
        //LJoystick = 0
        _inputData.joystickInput[0].x = Input.GetAxisRaw(_inputData.controller + Variables.LJOYSTICKH);
        _inputData.joystickInput[0].y = Input.GetAxisRaw(_inputData.controller + Variables.LJOYSTICKV);
        //RJoystick = 1
        _inputData.joystickInput[1].x = Input.GetAxisRaw(_inputData.controller + Variables.RJOYSTICKH);
        _inputData.joystickInput[1].y = Input.GetAxisRaw(_inputData.controller + Variables.RJOYSTICKV);

        //Set values to move speed
        Vector2 moveSpeed = _inputData.joystickInput[_inputData.moveIndex];
        if (moveSpeed.magnitude < .05f) moveSpeed = Vector2.zero;
        else if (moveSpeed.magnitude < .25f) moveSpeed = moveSpeed.normalized * .25f;
        _inputData.joystickInput[_inputData.moveIndex] = moveSpeed;

        //Set inverted values to camera
        Vector2 cameraInput = _inputData.joystickInput[_inputData.cameraIndex];
        cameraInput.x *= (_inputData.camInvH) ? -1f : 1f;
        cameraInput.y *= (_inputData.camInvV) ? -1f : 1f;
        _inputData.joystickInput[_inputData.cameraIndex] = cameraInput;

        //LTrigger = 10
        float oldValueTrigger = _inputData.valueLTrigger;
        _inputData.valueLTrigger = Input.GetAxis(_inputData.controller + Variables.LTRIGGER);
        _inputData.downInput[10] = oldValueTrigger < .8f && _inputData.valueLTrigger >= .8f;
        _inputData.upInput[10] = oldValueTrigger >= .8f && _inputData.valueLTrigger < .8f;
        _inputData.stayInput[10] = oldValueTrigger >= .8f && _inputData.valueLTrigger >= .8f;

        //RTrigger = 11
        oldValueTrigger = _inputData.valueRTrigger;
        _inputData.valueRTrigger = Input.GetAxis(_inputData.controller + Variables.RTRIGGER);
        _inputData.downInput[11] = oldValueTrigger < .8f && _inputData.valueRTrigger >= .8f;
        _inputData.upInput[11] = oldValueTrigger >= .8f && _inputData.valueRTrigger < .8f;
        _inputData.stayInput[11] = oldValueTrigger >= .8f && _inputData.valueRTrigger >= .8f;
    }
    #endregion

    bool basicControls = false;
    bool groundControls = false;
    bool airControls = false;

    private void Update()
    {
        GetUpdateInput();
        DebugSystem();

        if (InCredits)
        {
            if (GameManager.instance.IsPaused) GameManager.instance.UnPauseGame();
            return;
        }
        if (_inputData.downInput[_inputData.pauseIndex])
        {
            _inputData.downInput[_inputData.pauseIndex] = false;
            GameManager.instance.PauseButtonPressed();
        }
    }    

    void DebugSystem()
    {
        //Hurt Player
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _playerScripts.playerStats.ApplyDamage(0, 40, transform.position, Variables.SourceType.Object, Variables.DamageType.Damage);
        }

        //Respawn
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            _playerScripts.playerStats.KillPlayer();
        }

        //Restart
        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            AkSoundEngine.PostEvent("Stop_All", gameObject);
            //AkSoundEngine.ClearPreparedEvents();
            Scene scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.name, LoadSceneMode.Single);
        }

        //Reset character
        if (Input.GetKeyDown(KeyCode.Alpha8))
        {

            DisablePlayer();
            EnablePlayer();

        }
    }

    private void FixedUpdate()
    {
        GetFixedInput();
        float fixedDeltaTime = Time.fixedDeltaTime;

        //Transformation
        _playerScripts.playerTransformation.Tick(fixedDeltaTime, _inputData.downInput[_inputData.transformIndex]);
        
        //Ground Check
        bool groundCheck = GroundCheck();

        //Set Ground Normals
        _playerScripts.playerModelReposition.CalculateGroundData(_playerData.groundNormal, _playerData.groundPoint, _playerData.hitCounter);

        //Set Grounding depending on angle and jumping
        float maxAngle = (IsPlayerBig) ? _maxWalkAngleBig : _maxWalkAngleSmall;
        bool newTouchingGround = groundCheck;
        //Debug.Log("G_" + newTouchingGround + " A_" + _playerData.slopeAngle + " J_" + Jumping);
        if (_playerData.slopeAngle >= maxAngle || Jumping) newTouchingGround = false;
        _playerData.landed = !TouchingGround && newTouchingGround;
        TouchingGround = newTouchingGround;
        
        //Apply Model Reposition
        _playerScripts.playerModelReposition.Tick();

        //Apply Interactions
        _playerScripts.playerInteractions.EarlyTick(_inputData.downInput[_inputData.interactIndex]);

        //Apply Grab
        _playerScripts.playerGrab.Tick(fixedDeltaTime, _inputData.downInput[_inputData.grabIndex], _inputData.stayInput[_inputData.grabIndex]);
        //Apply Eat or Throw
        _playerScripts.playerObjectHandler.Tick(_inputData.downInput[_inputData.eatIndex], _inputData.downInput[_inputData.dropIndex], _inputData.downInput[_inputData.throwIndex]);
        //Apply Bok
        _playerScripts.playerBok.Tick(_inputData.downInput[_inputData.bokIndex]);
        //Apply Attack
        _playerScripts.playerStats.Tick(fixedDeltaTime, _inputData.downInput[_inputData.attackIndex], _inputData.upInput[_inputData.attackIndex], _inputData.stayInput[_inputData.attackIndex]);

        
        //Apply Jump
        _playerScripts.playerJump.Tick(fixedDeltaTime, _inputData.downInput[_inputData.jumpIndex], _inputData.upInput[_inputData.jumpIndex], _inputData.stayInput[_inputData.jumpIndex]);
        //PrepareMovementInput
        _playerScripts.playerMovement.SetMovementInput(_inputData.joystickInput[_inputData.moveIndex]);
        //Apply Movement
        _playerScripts.playerMovement.Tick(fixedDeltaTime);

        //Apply Camera Movement
        _playerScripts.cameraController.Tick(fixedDeltaTime, _inputData.joystickInput[_inputData.cameraIndex].y, _inputData.joystickInput[_inputData.cameraIndex].x);

        //Apply Sound
        _playerScripts.playerAudio.Tick();

        //Apply More Interaction
        bool acceptDown = false;
        for(int i = 0; i < _inputData.acceptIndex.Length; i++)
        {
            acceptDown = acceptDown || _inputData.downInput[_inputData.acceptIndex[i]];
        }
        _playerScripts.playerInteractions.LateTick(acceptDown);

        //Camera Shake
        if (Landed && IsPlayerBig)
        {
            int shakeId = (int)Variables.ShakeTypes.Land;
            _playerScripts.cameraController.StartShake(Variables.SHAKEDURATION[shakeId], Variables.SHAKEAMPLITUDE[shakeId], Variables.SHAKEFREQUENCY[shakeId]);
        } 

        //Animator
        //Set groun animation depending on jumping and angle  again (jumping may have finished this frame)
        if (_playerData.slopeAngle >= maxAngle || Jumping) groundCheck = false;
        SetAnimatorValues(groundCheck, fixedDeltaTime);

        //Particles Update
        _playerScripts.playerParticles.Tick();

        //UpdateUI
        UpdateUI();

        EndUpdateInput();
    }

    void UpdateUI()
    {
        //Grabbed Object
        GameManager.instance.SetObjectControls(_playerData.grabbedObject != null && !_playerData.grabbedObject.IsEatable);
        GameManager.instance.SetObjectEatControls(_playerData.grabbedObject != null && _playerData.grabbedObject.IsEatable);
        GameManager.instance.SetEatPrompt(_playerData.grabbedObject != null && _playerData.grabbedObject.IsEatable);

        //Grab
        Transform nearestGrabTransform = null;
        if(_playerData.grabbedObject == null && IsPlayerDisabled == false)
        {
            BasicGrabbableScript nearestGrab = _playerScripts.playerGrab.GetNearestGrab();
            if (nearestGrab != null) nearestGrabTransform = nearestGrab.transform;
        }        
        GameManager.instance.SetNearestGrabbableUI(nearestGrabTransform);

        //Interactions
        string interaction = "";
        Transform nearesInteractable = null;
        if (IsPlayerDisabled == false)
        {
            nearesInteractable = _playerScripts.playerInteractions.GetNearestInteractable(out interaction);
        }
        GameManager.instance.SetNearestInteractableUI(nearesInteractable, interaction);
    }

    private void SetAnimatorValues(bool groundCheck, float fixedDeltaTime)
    {
        //Touching ground
        _playerAnimatorBig.SetBool("IsGrounded", groundCheck);
        _playerAnimatorSmall.SetBool("IsGrounded", groundCheck);

        //Touching water
        _playerAnimatorSmall.SetBool("IsWet", TouchingWater);

        //Landing
        if (_playerData.landed == true) _playerAnimatorSmall.SetTrigger("Land");

        //Grabbing
        _playerAnimatorSmall.SetBool("IsGrabbing", GrabbedObject != null);
        _playerAnimatorBig.SetBool("IsGrabbing", GrabbedObject != null);

        //Movement
        Vector2 moveSpeed = _inputData.joystickInput[_inputData.moveIndex];
        if (InDialogue == true || IsPlayerLocked == true || IsPlayerEndLocked == true) moveSpeed = Vector2.zero;
        if (moveSpeed.magnitude > 1) moveSpeed.Normalize();
        _playerAnimatorBig.SetFloat("Speed", moveSpeed.magnitude);
        moveSpeed *= Momentum;
        _inputData.joystickInput[_inputData.moveIndex] = moveSpeed;
        _playerAnimatorSmall.SetFloat("Speed", moveSpeed.magnitude);
        _playerAnimatorSmall.SetFloat("SpeedMomentum", Momentum);

        //Credits
        if(InCredits)
        {
            _playerData.creditsAnimationTime += fixedDeltaTime;
            float toTransition = (moveSpeed.magnitude == 0) ? 1 : 0;
            _playerData.creditsTransition = Mathf.Lerp(_playerData.creditsTransition, toTransition, .1f);
            _playerAnimatorBig.SetLayerWeight(_playerAnimatorBig.GetLayerIndex("DanceLayer"), _playerData.creditsTransition);
        }
    }


    #region Ground Check Functions
    private void UpdateColliderBottomOffset()
    {
        _colliderBottomOffset = (IsPlayerBig == true) ?_capsuleColliderBig.center.y : _capsuleColliderSmall.center.y;
        _groundSideOffset = .9f*((IsPlayerBig == true) ? _capsuleColliderBig.radius : _capsuleColliderSmall.radius);
    }

    // Check if bottom of character is touching ground
    private bool GroundCheck()
    {
        bool result = false;
        float groundCheck;
        if(IsPlayerBig == true)
        {
            groundCheck = (TouchingGround && !Jumping) ? Variables.GROUND_CHECK_DISTANCE_BIG_GROUND : Variables.GROUND_CHECK_DISTANCE_BIG_AIR;
        }
        else
        {
            groundCheck = (TouchingGround && !Jumping) ? Variables.GROUND_CHECK_DISTANCE_SMALL_GROUND : Variables.GROUND_CHECK_DISTANCE_SMALL_AIR;
        }

        _playerData.groundNormal = Vector3.zero;
        _playerData.groundPoint = Vector3.zero;
        _playerData.hitCounter = 0;

        TouchingWater = true;

        float distance  = _colliderBottomOffset +  groundCheck * _characterScale;
        result = GroundCheck(transform.position + Quaternion.FromToRotation(Vector3.forward, transform.forward) * (Vector3.forward * _groundSideOffset * .4f + Vector3.right * _groundSideOffset) + Vector3.up * _colliderBottomOffset, Vector3.down, distance) || result;
        result = GroundCheck(transform.position + Quaternion.FromToRotation(Vector3.forward, transform.forward) * (Vector3.forward * _groundSideOffset * .4f - Vector3.right * _groundSideOffset) + Vector3.up * _colliderBottomOffset, Vector3.down, distance) || result;
        result = GroundCheck(transform.position + Quaternion.FromToRotation(Vector3.forward, transform.forward) * (Vector3.forward * _groundSideOffset) + Vector3.up * _colliderBottomOffset, Vector3.down, distance) || result;
        result = GroundCheck(transform.position + Quaternion.FromToRotation(Vector3.forward, transform.forward) * (-Vector3.forward * _groundSideOffset) + Vector3.up * _colliderBottomOffset, Vector3.down, distance) || result;
        if(TouchingGround == true) result = GroundCheck(transform.position + Quaternion.FromToRotation(Vector3.forward, transform.forward) * (Vector3.forward * _groundSideOffset) + Vector3.up * _colliderBottomOffset, Quaternion.FromToRotation(Vector3.up, PlayerUp) * Vector3.down, Variables.GROUND_CHECK_SLOPE_DISTANCE) || result;

        TouchingWater = TouchingWater && result;
        if (_playerData.hitCounter != 0)
        {
            _playerData.groundNormal = _playerData.groundNormal / _playerData.hitCounter;
            _playerData.groundPoint = _playerData.groundPoint / _playerData.hitCounter;
        }

        return result;
    }


    //Raycasting down to check if grounded
    private bool GroundCheck(Vector3 origin, Vector3 direction, float distance)
    {
        LayerMask groundMask = (IsPlayerBig == true) ? _groundMaskBig : _groundMaskSmall;
        RaycastHit hit;
        if (Physics.Raycast(origin, direction, out hit,  distance, groundMask) == true)
        {
            _playerData.groundNormal += hit.normal;
            _playerData.groundPoint += hit.point;
            _playerData.hitCounter++;
            Debug.DrawRay(origin, direction * distance, Color.black);
            TouchingWater = TouchingWater && hit.collider.gameObject.layer == _playerData.waterLayer;
            return true;
        }

        Debug.DrawRay(origin, direction * distance, Color.red);
        return false;
    }
    #endregion

    #region Events
    #region Quest Points
    public void AddQuestPoints(int points)
    {
        _playerScripts.playerAudio.PlayQuestPointSound();
        QuestPoints += points;
    }

    public void AddTutorialPoints(int points)
    {
        _playerScripts.playerAudio.PlayQuestPointSound();
        TutorialPoints += points;
    }
    #endregion

    #region UI
    public void UpdateHealthUI(float healthPercentage, Variables.Health healthState)
    {
        GameManager.instance.SetPlayerHealth(healthPercentage, healthState);
    }

    public void UpdateTransformUI(bool activate)
    {
        GameManager.instance.SetTransformPrompt(activate);
    }

    #endregion

    #region Bridge Events
    public void LockCameraSmoothly(Transform lookTransform)
    {
        _playerScripts.cameraController.LockCameraSmoothly(lookTransform);
    }

    public void UnlockCamera()
    {
        _playerScripts.cameraController.UnlockCamera();
    }

    public void SetCreditsState(bool start)
    {
        if (!start) _playerData.creditsAnimationTime = 0;
        _playerData.inCredits = start;
        _playerAnimatorBig.SetBool("InCredits", start);
        _playerScripts.cameraController.SetCreditsTransition(start);
    }

    public void StartBokAttack(bool poweredUp)
    {
        _playerScripts.playerStats.StartBokAttack(poweredUp);
    }

    public void TryDummyJump()
    {
        _playerScripts.playerExtraBehaviours.TryDummyJump();
    }

    public void KickPlayer(Vector3 direction)
    {
        _playerScripts.playerExtraBehaviours.KickPlayer(direction);
    }

    public void SetExtraTransition(Variables.ExtraCameraTransition extraTransition, Variables.CameraTransition transition)
    {
        _playerScripts.cameraController.SetExtraTransition(extraTransition, transition);
    }

    public void SetDummyCameraTarget(Transform target)
    {
        _playerScripts.cameraController.SetDummyCameraTarget(target);
    }

    public void InWallReposition(Quaternion targetRotation, float easing)
    {
        _playerScripts.playerModelReposition.InWallReposition(targetRotation, easing);
    }

    public void SetThrowRotation()
    {
        _playerScripts.cameraController.AimPlayerToCamera();
    }
    #endregion

    #region Transformation
    public void SwapCharacter()
    {
        //Set player bool
        IsPlayerBig = !IsPlayerBig;
        _playerScripts.playerAudio.PlayTransform(FirstTransform);
        StartPlayerAction(true);
        StartSetCharacter();
    }

    void StartSetCharacter()
    {
        Transforming = true;

        if (IsPlayerBig == true)
        {
            //Update Scripts
            _playerScripts.playerStats.SwapCharacters();
            _playerScripts.playerJump.SwapCharacters(IsPlayerBig);
            _playerScripts.cameraController.SwapCharacters(IsPlayerBig, FirstTransform);
            _playerScripts.playerGrab.SwapCharacters(!IsPlayerBig);
            _playerScripts.playerObjectHandler.SwapCharacters(!IsPlayerBig);

            //Set character scale
            _characterScale = 1;

            //Set capsules
            _capsuleColliderBig.enabled = IsPlayerBig;
            _capsuleColliderSmall.enabled = !IsPlayerBig;

            //Update Collider
            UpdateColliderBottomOffset();

            //Change player layer
            gameObject.layer = LayerMask.NameToLayer(_playerLayerNameBig);
        } else
        {
            _playerScripts.cameraController.SwapCharacters(IsPlayerBig, false);
        }
        _playerScripts.playerParticles.SwapCharacters(IsPlayerBig);

        //Disable All Models
        _playerModelHolderBig.SetActive(false);
        _playerModelHolderSmall.SetActive(false);
    }



    public void FinishSetCharacter()
    {
        if(IsPlayerBig == false)
        {
            //Update Scripts
            _playerScripts.playerStats.SwapCharacters();
            _playerScripts.playerJump.SwapCharacters(IsPlayerBig);
            _playerScripts.playerGrab.SwapCharacters(!IsPlayerBig);
            _playerScripts.playerObjectHandler.SwapCharacters(!IsPlayerBig);

            //Set character scale
            _characterScale = _characterScaling;

            //Set capsules
            _capsuleColliderBig.enabled = IsPlayerBig;
            _capsuleColliderSmall.enabled = !IsPlayerBig;

            //Update Collider
            UpdateColliderBottomOffset();

            //Change player layer
            gameObject.layer = LayerMask.NameToLayer(_playerLayerNameSmall);
        }

        //Enable Proper Model
        _playerModelHolderBig.SetActive(IsPlayerBig);
        _playerModelHolderSmall.SetActive(!IsPlayerBig);

        //Animator Behaviours
        TemplateSMB[] behaviours = ((IsPlayerBig) ? _playerAnimatorBig : _playerAnimatorSmall).GetBehaviours<TemplateSMB>();
        for (int i = 0; i < behaviours.Length; i++)
        {
            behaviours[i].Init(_playerScripts.playerAudio);
        }

        if (IsPlayerBig == true)
        {
            string state = "Base Layer.ChickenToBeastTransformation";

            if (FirstTransform == true)
            {
                FirstTransform = false;
                state = "Base Layer.FirstTransformation";
            }

            _playerAnimatorBig.Play(state);
            if(InCredits)
            {
                _playerAnimatorBig.SetBool("InCredits", true);
                _playerAnimatorBig.Play("Dance", _playerAnimatorBig.GetLayerIndex("DanceLayer"), _playerData.creditsAnimationTime/68.5f);

            }
        }
        else
        {
            _playerAnimatorSmall.Play("Base Layer.Idle");
            UpdateModifier(_playerData.activeModifier, _playerData.modifierLevel);
        }

        ActionFinished();
        Transforming = false;

        if (IsPlayerBig == false && _playerScripts.playerStats.CurrentHealth == 0) PlayerDied();
    }

    void SetCharacter()
    {
        //Update Scripts
        _playerScripts.playerStats.SwapCharacters();
        _playerScripts.playerJump.SwapCharacters(IsPlayerBig);
        _playerScripts.cameraController.SwapFast(IsPlayerBig);
        _playerScripts.playerGrab.SwapCharacters(!IsPlayerBig);
        _playerScripts.playerObjectHandler.SwapCharacters(!IsPlayerBig);
        _playerScripts.playerParticles.SwapCharacters(IsPlayerBig);

        //Set character scale
        _characterScale = (IsPlayerBig == true) ? 1 : _characterScaling;

        //Set capsules
        _capsuleColliderBig.enabled = IsPlayerBig;
        _capsuleColliderSmall.enabled = !IsPlayerBig;

        //Enable Proper Model
        _playerModelHolderBig.SetActive(IsPlayerBig);
        _playerModelHolderSmall.SetActive(!IsPlayerBig);

        //Animator Behaviours
        TemplateSMB[] behaviours = ((IsPlayerBig) ? _playerAnimatorBig : _playerAnimatorSmall).GetBehaviours<TemplateSMB>();
        for (int i = 0; i < behaviours.Length; i++)
        {
            behaviours[i].Init(_playerScripts.playerAudio);
        }

        //Change player layer
        gameObject.layer = LayerMask.NameToLayer((IsPlayerBig == true) ? _playerLayerNameBig : _playerLayerNameSmall);

        //Update Collider
        UpdateColliderBottomOffset();

        if (IsPlayerBig == true)
        {
            _playerAnimatorBig.Play("Base Layer.Idle");
        }
        else
        {
            _playerAnimatorSmall.Play("Base Layer.Idle");
            UpdateModifier(_playerData.activeModifier, _playerData.modifierLevel);
        }

        ActionFinished();
    }
    #endregion

    #region Respawn
    public void PlayerDied()
    {
        if(IsPlayerBig == true)
        {
            _playerScripts.playerTransformation.ActivateTransformation();
            return;
        }
        if (IsDead) return;
        _playerScripts.playerObjectHandler.PlayerDied();
        _playerScripts.playerStats.PlayerDied();
        InterruptDialogue();

        ResetModifiers();

        _deathEvent.Raise();
        IsDead = true;

        StartPlayerAction(true);
        SetAction("Death");

        _playerScripts.playerAudio.PlayDeathSound();
        Invoke("DestroyPlayer", 4f);
    }

    public void PlayerDrown()
    {
        if (IsDead) return;
        _playerScripts.playerObjectHandler.PlayerDied();
        _playerScripts.playerStats.PlayerDied();
        InterruptDialogue();

        IsDead = true;

        StartPlayerAction(true);
        SetAction("Drown");

        _playerScripts.playerAudio.PlayDrownSound();
    }

    public void RespawnDrownPlayer()
    {
        _playerScripts.playerGrab.PlayerRespawn();
        _playerScripts.playerInteractions.PlayerRespawn();
        IsDead = false;
        ActionFinished();
        _rb.velocity = Vector3.zero;
        transform.position = GameManager.instance.GetWaterRespawn(transform.position);
    }

    public void DestroyPlayer()
    {
        _playerAnimatorSmall.Play("Base Layer.Idle");
        _playerScripts.playerGrab.PlayerRespawn();
        _playerScripts.playerInteractions.PlayerRespawn();
        IsDead = false;
        ActionFinished();
        _rb.velocity = Vector3.zero;
        transform.position = GameManager.instance.GetGroundRespawn(transform.position);
        _playerScripts.playerStats.ReHealPlayer();
    }
    #endregion

    #region Dialogue Events
    public void StartDialogue(string npcName, string[] dialogueText)
    {

        GameManager.instance.StartDialogue(npcName, dialogueText);
        InDialogue = true;
    }

    public void NextLine()
    {
        GameManager.instance.NextLine();
    }

    void InterruptDialogue()
    {
        if (InDialogue == false) return;
        GameManager.instance.InterruptDialogue();
    }

    public void DialogueUpdated(int id)
    {
        //Set new camera transition
        _playerScripts.playerInteractions.SetCameraTransition(id);
        //Set new sound
        string sound = _playerScripts.playerInteractions.GetInteractionSound(id);
        if(sound != "") _playerScripts.cameraController.PlayQuestSound(sound);
    }

    public void FinishDialogue()
    {
        InDialogue = false;
        _playerScripts.playerInteractions.FinishingDialogue();
    }
    #endregion

    #region Combat Events
    public void StartHitFlash()
    {
        for (int i = 0; i < _skinnedRenderers.Length; i++)
        {
            _skinnedRenderers[i].material.color = Color.red;
        }
        Invoke("EndHitFlash", .15f);
    }

    void EndHitFlash()
    {
        for (int i = 0; i < _skinnedRenderers.Length; i++)
        {
            _skinnedRenderers[i].material.color = Color.white;
        }
    }
    #endregion

    #region Jump Events
    public void ApplyJumpBurst()
    {
        if (IsPlayerBig)
        {
            _playerScripts.playerParticles.SetDashParticles();
        }
        else
        {
            if (GetJumpModifier() == 1) return;
            _playerScripts.playerParticles.SetDustJumpGroundParticles();
        }

    }
    #endregion

    #endregion

    #region Sound Set
    public void PlayAttackSound()
    {
        _playerScripts.playerAudio.PlayAttack();
    }

    public void PlayRecoverSound()
    {
        _playerScripts.playerAudio.PlayRecoverSound();
    }

    public void PlayNotEatSound()
    {
        _playerScripts.playerAudio.PlayNotEat();
    }

    public void PlayNotAIHitSound(int hitState)
    {
        _playerScripts.playerAudio.PlayNotAIHitSound(hitState);
    }
    #endregion

    #region Animation Set

    public void SetAction(string actionName)
    {
        _playerAnimatorBig.SetTrigger(actionName);
        _playerAnimatorSmall.SetTrigger(actionName);
    }

    public void EnterChargeJumpMode()
    {
        _playerAnimatorBig.SetBool("ChargingJump", true);

    }

    public void ExitChargeJumpMode()
    {
        _playerAnimatorBig.SetBool("ChargingJump", false);

    }

    public void SetGliding(bool gliding)
    {
        IsGliding = gliding;
        _playerAnimatorSmall.SetBool("Gliding", gliding);
        _playerAnimatorBig.SetBool("Gliding", gliding);
    }
    #endregion

    #region Animation Events
    public void StartPlayerAction(bool isLocking)
    {
        _playerData.doingAction = true;
        _playerData.actionIsLocking = isLocking;
        BeastCanJump = false;
    }

    public void ActionFinished()
    {
        if (IsDead) return;
        _playerData.doingAction = false;
        _playerData.actionIsLocking = false;
        BeastCanJump = true;
    }

    public void GrabFinished()
    {
        ActionFinished();
    }

    public void AttackFinished(Variables.PlayerAttackTypes attackType)
    {
        ActionFinished();
        _playerScripts.playerStats.AttackFinished(attackType);
    }

    public void ActivateStrongAttack(bool active)
    {
        _playerScripts.playerStats.StrongAttackActivation(active);
        if(active == true)
        {
            int shakeId = (int)Variables.ShakeTypes.StrongAttack;
            _playerScripts.cameraController.StartShake(Variables.SHAKEDURATION[shakeId], Variables.SHAKEAMPLITUDE[shakeId], Variables.SHAKEFREQUENCY[shakeId]);
            _playerScripts.playerParticles.SetShockWaveParticles();
            _playerScripts.playerAudio.PlayStrongAttack();
        }
    }

    public void ActivateWeakAttack(bool isLeft)
    {
        _playerScripts.playerStats.WeakAttackActivation(isLeft);
    }

    public void GrabObject()
    {
        _playerScripts.playerGrab.GrabObject();
        if(GrabbedObject != null)
        {
            _playerScripts.playerStats.PrepareItemDealers(GrabbedObject.GetDamageTrigger(), GrabbedObject.GetDamage());
        }
    }

    public void TryEatObject()
    { 
        _playerScripts.playerAudio.PlayEat(Variables.ObjectType.HP);
    }

    public void EatObject()
    {
        Variables.ObjectType newModifier = GrabbedObject.GetModifierType();
        _playerScripts.playerAudio.PlayEat(newModifier);
        SetModifiers(newModifier);
        GrabbedObject.EatObject();
        //Destroy object or pooling
        //Destroy(GrabbedObject.gameObject);
    }

    public void MunchObject()
    {
        _playerScripts.playerAudio.PlayEat(Variables.ObjectType.HP);
        GrabbedObject.MunchObject();
    }

    public void ThrowObject()
    {
        _playerScripts.playerObjectHandler.ThrowObject(playerCamera.forward);
    }

    public void SetFootCollision(bool isLeft)
    {
        _playerScripts.playerAudio.PlayFootStep(isLeft);
        if (IsPlayerBig == true)
        {
            int shakeId = (int)Variables.ShakeTypes.Stomp;
            _playerScripts.cameraController.StartShake(Variables.SHAKEDURATION[shakeId], Variables.SHAKEAMPLITUDE[shakeId], Variables.SHAKEFREQUENCY[shakeId]);
        }
    }

    public void SwimSound()
    {
        _playerScripts.playerAudio.PlaySwim();
    }

    public void ActivateBok()
    {
        _playerScripts.playerParticles.SetBokRadar();
        if (IsPlayerBig == true)
        {
            int shakeId = (int)Variables.ShakeTypes.Scream;
            _playerScripts.cameraController.StartShake(Variables.SHAKEDURATION[shakeId], Variables.SHAKEAMPLITUDE[shakeId], Variables.SHAKEFREQUENCY[shakeId]);
        }
        if (GetBokModifier() == true && IsPlayerBig == false)
        {
            _playerScripts.playerParticles.SetScreamParticles();
        }
        _playerScripts.playerStats.ActivateBokAttack();
        //AUDIO HERE
        _playerScripts.playerAudio.PlayBok();
        //INVOKE BOK BACK
        _bokEvent.Raise(transform);
    }

    public void ActivateRoarParticles()
    {
        _playerScripts.playerParticles.SetRoarParticles();
    }

    public void TryStartStrongAttack()
    {
        _playerScripts.playerStats.TryStartStrongAttack();
    }
    #endregion

    #region PowerUp functions
    void SetModifiers(Variables.ObjectType newModifier)
    {
        //INMEDIATE CHANGES
        //Heal Player
        if (newModifier == Variables.ObjectType.HP)
        {
            _playerScripts.playerStats.HealDamage(Variables.OBJECT_HEALING_AMOUNT);
            return;
        }

        if(newModifier == Variables.ObjectType.Bronze)
        {
            QuestPoints += Variables.BRONZE_POINTS;
            return;
        }

        if (newModifier == Variables.ObjectType.Silver)
        {
            QuestPoints += Variables.SILVER_POINTS;
            return;
        }

        if (newModifier == Variables.ObjectType.Gold)
        {
            QuestPoints += Variables.GOLD_POINTS;
            return;
        }

        //PERMANENT MODIFIERS
        int modifierType = (int)newModifier;

        //Not a slot modifier (non-slot modifiers are smaller or equal to 0)
        if (modifierType <= 0)
        {
            return;
        }

        int playerModifier = (int)_playerData.activeModifier;

        //Already has modifier, update level
        if (playerModifier == modifierType)
        {
            /*
            _playerData.modifierLevel = Mathf.Clamp(_playerData.modifierLevel + 1, 1, Variables.MAX_MODIFIER_LEVEL);
            UpdateModifier(_playerData.activeModifier, _playerData.modifierLevel);
            */
            return;
        }

        //CHECK IF PLAYER IS CHICKEN???
        //Remove things from previous modifier
        UpdateModifier(_playerData.activeModifier, 0);
        //Update New Modifier
        _playerData.modifierLevel = 1;
        _playerData.activeModifier = (Variables.PlayerModifier)modifierType;
        _playerScripts.playerParticles.SetModifierParticles(_playerData.activeModifier);
        UpdateModifier(_playerData.activeModifier, _playerData.modifierLevel);
        GameManager.instance.SetPlayerAbility(_playerData.activeModifier);
    }

    void ResetModifiers()
    {
        //Remove things from previous modifier
        UpdateModifier(_playerData.activeModifier, 0);
        _playerData.modifierLevel = 0;
        _playerData.activeModifier = Variables.PlayerModifier.None;
        GameManager.instance.SetPlayerAbility(_playerData.activeModifier);
    }


    void UpdateModifier(Variables.PlayerModifier modifierType, int modifierLevel)
    {
        switch (modifierType)
        {
            case Variables.PlayerModifier.Speed:
                //UPDATE ANIMATION SPEED
                _playerAnimatorSmall.SetFloat("LocomotionMultiplier", 1f + Variables.SPEED_MODIFIER * modifierLevel);
                if (modifierLevel == 0)
                {
                    _playerScripts.playerParticles.StopSpeedParticles();
                }

                break;
            case Variables.PlayerModifier.Jump:
                if (modifierLevel == 0)
                {
                    _playerScripts.playerParticles.StopJumpTrail();
                }
                break;
        }
        //UPDATE UI MODIFIER
    }

    public float GetSpeedModifier()
    {
        if (_playerData.activeModifier == Variables.PlayerModifier.Speed) return 1f + Variables.SPEED_MODIFIER * _playerData.modifierLevel;
        return 1;
    }

    public float GetJumpModifier()
    {
        if (_playerData.activeModifier == Variables.PlayerModifier.Jump) return 1f + Variables.JUMP_MODIFIER * _playerData.modifierLevel;
        return 1;
    }

    public float GetGlideModifier()
    {
        if (_playerData.activeModifier == Variables.PlayerModifier.Glide) return 1f/(Variables.GLIDE_MODIFIER * _playerData.modifierLevel);
        return 1;
    }

    public bool GetGrabModifier()
    {
        return _playerData.activeModifier == Variables.PlayerModifier.GrabThrow;
    }

    public float GetThrowModifier()
    {
        if (_playerData.activeModifier == Variables.PlayerModifier.GrabThrow) return 1f + Variables.THROW_MODIFIER * _playerData.modifierLevel;
        return 1f;
    }

    public float GetAttackModifier()
    {
        if (_playerData.activeModifier == Variables.PlayerModifier.Attack) return 1f + Variables.ATTACK_MODIFIER * _playerData.modifierLevel;
        return 1f;
    }

    public bool GetBokModifier()
    {
        return _playerData.activeModifier == Variables.PlayerModifier.Scream;

    }

    #endregion

    #region Player Disabled
    public void LockPlayer()
    {
        _playerData.inEndLockState = true;
    }

    public void UnlockPlayer()
    {
        _playerData.inEndLockState = false;
    }

    public void DisablePlayer()
    {
        _capsuleColliderBig.enabled = false;
        _capsuleColliderSmall.enabled = false;
        _playerModelHolderBig.SetActive(false);
        _playerModelHolderSmall.SetActive(false);
        _playerData.actionIsDisabling = true;
        _rb.isKinematic = true;

        _playerScripts.playerAudio.StopAllSounds();
        _playerScripts.playerGrab.ClearGrabbableObjects();
        _playerScripts.playerInteractions.ClearNPChickens();
        _playerScripts.playerParticles.DisablePlayer(IsPlayerBig);

        _playerData.doingAction = false;
        _playerData.actionIsLocking = false;
    }

    public void EnablePlayer()
    {
        _capsuleColliderBig.enabled = IsPlayerBig;
        _capsuleColliderSmall.enabled = !IsPlayerBig;
        _playerModelHolderBig.SetActive(IsPlayerBig);
        _playerModelHolderSmall.SetActive(!IsPlayerBig);
        //Animator Behaviours
        TemplateSMB[] behaviours = ((IsPlayerBig) ? _playerAnimatorBig : _playerAnimatorSmall).GetBehaviours<TemplateSMB>();
        for (int i = 0; i < behaviours.Length; i++)
        {
            behaviours[i].Init(_playerScripts.playerAudio);
        }
        _playerData.actionIsDisabling = false;
        _rb.isKinematic = false;

        if (IsPlayerBig == true)
        {
            _playerAnimatorBig.Play("Base Layer.Idle");
        }
        else
        {
            _playerAnimatorSmall.Play("Base Layer.Idle");
            UpdateModifier(_playerData.activeModifier, _playerData.modifierLevel);
        }
    }
    #endregion

}