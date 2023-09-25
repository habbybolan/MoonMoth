using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;
using Gyroscope = UnityEngine.InputSystem.Gyroscope;
using EnhancedTouch = UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.EventSystems;


/*
 * Deals with Player's Inputs, States and which methods to call each frame.
 * Acts as the central hub for interacting with all Player components.
 */
public class PlayerController : CharacterController<PlayerHealth>
{
    [SerializeField] private PlayerMovement m_PlayerMovement;
    [SerializeField] private PlayerParentMovement m_PlayerParentMovement;

    [Tooltip("CameraMovement component")]
    [SerializeField] private CameraMovement m_CameraMovement;
    [SerializeField] private PlayerWeapon m_Weapon;
    [SerializeField] private MoonBarAbility m_MoonBarAbility;

    [Header("Effects")]
    [SerializeField] private float m_SlowEffectDuration = 3f;
    [SerializeField] private float m_FogInTransitionDuration = 1f;
    [SerializeField] private float m_FogOutTransitionDuration = 3f;

    [Header("Lost Moth")]
    [SerializeField] private TextMeshProUGUI m_LostMothUI;
    [SerializeField] private GameObject m_LostMothContainer;

    [Header("Sound")]
    [SerializeField] private AudioSource m_AimModeStartSound;
    [SerializeField] private AudioSource m_AimModeEndSound;

    [Header("Dodge")]
    [SerializeField] private float m_DodgeCooldown = 1;

    [Header("Tutorial")]
    [SerializeField] private Checklist m_ChecklistPrefab;
    [SerializeField] private TutorialManager m_TutorialManager;
    [Min(0)]
    [SerializeField] private float m_ButtonHoldLengthToSkip = 3;
    [SerializeField] private ParticleSystem m_TutorialDustParticlesPrefab;

    [Header("Animation")]
    [SerializeField] private float m_GlideFlapDelayMin = .2f;
    [SerializeField] private float m_GlideFlapDelayMax = .5f;

    [Header("Mobile")]
    [Tooltip("Either uses Gyro controls, or Attitude + Accelerometer values")]
    [SerializeField] private bool m_IsJoystickMovement = false;
    [SerializeField] private float m_MinSwipeLengthToDodge = 500.0f;

    [Header("Gyroscope")]
    //[SerializeField] private float MaxMobileYawRotationFromOrigin = 25;
    [Min(1)]
    [SerializeField] private float m_GryoscopePitchMult = 17;
    [Min(1)]
    [SerializeField] private float m_GryoscopeYawMult = 10;

    [Header("Attitude")]

    [Header("Mobile UI")]
    [SerializeField] private GameObject m_MobileUI;
    [SerializeField] private TextMeshProUGUI m_MobileFPSTest;
    [SerializeField] private MobileActionButton m_AimModeButton;
    [SerializeField] private MobileActionButton m_DashButton;
    [SerializeField] private MobileActionButton m_FireButton;
    [SerializeField] private VirtualJoystick m_VirtualJoystick;

    public MobileActionButton AimModeButton
    {
        get { return m_AimModeButton; }
    }
    public MobileActionButton DashButton
    {
        get { return m_DashButton; }
    }
    public MobileActionButton FireButton
    {
        get { return m_FireButton; }
    }

    private PlayerInput m_PlayerInput; 

    private PLAYER_ACTION_STATE m_playerState;  // Current player state given the actions performed / effects applied

    private Vector2 m_MovementInput;            // Input object for moving player along x-y axis

    private DamageInfo.HIT_EFFECT m_CurrEffect; // Current hit effect applied to player
    private Coroutine m_SlowEffectCoroutine;

    private bool m_IsDodgeCooldown;
    private Animator m_Animator;
    private bool m_IsFirstUpdate = true;

    private float BasePhoneYawRotation;
    private float CurrYawRot = 0;
    private float CurrPitchRot = 0;
    private ParticleSystem m_SpawnedDustParticle;

    private Coroutine m_SkipCoroutine;

    public delegate void LostMothCollectedDelegate();
    public LostMothCollectedDelegate lostMothCollectedDelegate;

    private float currFpsDelay = 0.25f;
    private float avgFpsCount = 0;

    private Checklist m_Checklist;
    public Checklist Checklist

    {
        get { return m_Checklist; }
    }

    public bool IsJoystickMovement
    {
        get { return IsJoystickMovement; }
    }

    // Time that skip button was pressed and held. -1 if not pressed yet
    private float m_TimeSkipPressed = -1;
    
    public MoonBarAbility MoonBarAbility { get { return m_MoonBarAbility; }}

    // Add a new enemy's boost duration to list
    public void OnEnemyKilled()
    {
        m_MoonBarAbility.AddEnemyKilled();
    }

    private void Awake()
    {
        m_Animator = GetComponent<Animator>();
        m_CurrEffect = DamageInfo.HIT_EFFECT.NORMAL;
        m_PlayerInput = FindObjectOfType<PlayerInput>();
    }

    protected override void Start()
    {
        base.Start();
        m_playerState = PLAYER_ACTION_STATE.FLYING;

        m_MoonBarAbility.d_AimModeStartDelegate = AimModeStart;
        m_MoonBarAbility.d_AimModeEndDelegate = AimModeEnd;
        m_MoonBarAbility.d_DashStartDelegate = DashModeStart;
        m_MoonBarAbility.d_DashEndDelegate = DashModeEnd;

        m_Health.d_DamageDelegate = OnDamageTaken;
        UpdateLostMothText();

        GameState.PropertyInstance.d_GameTransitioningDelegate += StartTransition;

#if UNITY_ANDROID && !UNITY_EDITOR
                /*if (Accelerometer.current != null)
                {
                    InputSystem.EnableDevice(Accelerometer.current);
                    InputSystem.EnableDevice(AttitudeSensor.current);
                    InputSystem.EnableDevice(Gyroscope.current);

                    Quaternion q = AttitudeSensor.current.attitude.ReadValue();
                    BasePhoneYawRotation = (float)Math.Atan2(2.0 * (q.y * q.z + q.w * q.x), q.w * q.w - q.x * q.x - q.y * q.y + q.z * q.z);
                    //BasePhoneYawRotation = attitude.eulerAngles.y;
                }

                SetMobileMovementType(m_IsJoystickMovement);
                EnhancedTouch.EnhancedTouchSupport.Enable();
                // delegate for checking after swipe
                EnhancedTouch.Touch.onFingerUp += OnFingerUp;
                EnhancedTouch.Touch.onFingerDown += OnFingerDown;*/
                m_MobileUI.SetActive(true);
#endif

        Application.targetFrameRate = -1;
    }


    // Event on entering tutorial
    public void InitializeTutorialUI()
    {
        m_Checklist = Instantiate(m_ChecklistPrefab);
        m_Checklist.skipButtonDownDelegate += OnSkipStart;
        m_Checklist.skipButtonUpDelegate += OnSkipEnd;
        m_LostMothContainer.SetActive(false);

        m_SpawnedDustParticle = Instantiate(m_TutorialDustParticlesPrefab);
        m_SpawnedDustParticle.transform.parent = m_PlayerParentMovement.transform;
        m_SpawnedDustParticle.Play();

        m_TutorialManager.NewTutorialEnteredDelegate += NewTutorialEntered;
    }

    private void NewTutorialEntered(Tutorial tutorial)
    {
        if (tutorial == null) return;

        // If lost moth tutorial, show lost moth tutorial text
        LostMothTutorial lostMothTutorial = tutorial as LostMothTutorial;
        if (lostMothTutorial != null)
        {
            ActivateLostMothUI();
        }
    }

    // Event on tutorial finished or skipped
    public void TutorialEnded()
    {
        Destroy(m_Checklist.gameObject);
        m_Checklist = null;
        m_SpawnedDustParticle.Stop();
    }

    private void ActivateLostMothUI()
    {
        m_LostMothContainer.SetActive(true);
        UpdateLostMothText();
    }

    public Tutorial GetCurrTutorial()
    {
        if (m_TutorialManager != null)
        {
            return m_TutorialManager.CurrTutorial;
        }
        return null;
    }

    private void OnDamageTaken(DamageInfo damageInfo) 
    {
        m_CameraMovement.StartCameraShake();
        ApplyEffect(damageInfo.m_HitEffect);
    }

    private void AimModeStart() {
        m_AimModeStartSound.Play();
        m_PlayerMovement.AimModeEnter();
        m_CameraMovement.CameraAimModeZoom();
        UpdateAimModeButton();
    }
    private void AimModeEnd() {
        m_AimModeEndSound.Play();
        m_PlayerMovement.AimModeExit();
        m_CameraMovement.ResetZoom();
        UpdateAimModeButton();
    }
    private void DashModeStart() {
        m_PlayerParentMovement.DashStart();
        m_CameraMovement.CameraDashZoom();
        UpdateDashButton();
    }
    private void DashModeEnd() {
        m_PlayerParentMovement.DashEnd();
        m_CameraMovement.ResetZoom();
        UpdateDashButton();
    }

    public void OnMove(InputValue value)
    {
        RecordInput(value.Get<Vector2>());
    }

    public void RecordInput(Vector2 inputValue)
    {
        m_MovementInput = inputValue;

        if (GameState.PropertyInstance.GameStateEnum == GameStateEnum.TUTORIAL)
        {
            if (inputValue.y > .1) m_TutorialManager.ReceiveTutorialInput(TutorialInputs.UP);
            if (inputValue.y < -.1) m_TutorialManager.ReceiveTutorialInput(TutorialInputs.DOWN);
            if (inputValue.x > .1) m_TutorialManager.ReceiveTutorialInput(TutorialInputs.RIGHT);
            if (inputValue.x < -.1) m_TutorialManager.ReceiveTutorialInput(TutorialInputs.LEFT);
        }
    }

    // Main Update controller for all Player components, Dealing with actions/effects that happen each frame
    void Update()
    {
        m_Weapon.TryShoot();
        m_MoonBarAbility.UpdateAimModeEnemyKilledList();
        m_MoonBarAbility.UpdateAimModeReticleBar();

        m_Health.LosePassiveHealth();
        m_PlayerMovement.RotationLook();

        if (m_playerState == PLAYER_ACTION_STATE.FLYING || m_playerState == PLAYER_ACTION_STATE.DASHING)
        {
            m_PlayerMovement.HorizontalRotation(m_MovementInput.x);
        }

        if (m_IsFirstUpdate)
        {
            UIManager.PropertyInstance.FadeOut(m_FogOutTransitionDuration);
            m_CameraMovement.ResetPosition();
            m_IsFirstUpdate = !m_IsFirstUpdate;
        }
#if UNITY_ANDROID && !UNITY_EDITOR
        currFpsDelay += Time.deltaTime;
        avgFpsCount += 1;
        if (currFpsDelay > 0.25)
        {
            m_MobileFPSTest.SetText((1 / (currFpsDelay / avgFpsCount)).ToString());
            currFpsDelay = 0;
            avgFpsCount = 0;
        }
        
#endif
    }

    private void FixedUpdate()
    {
        // move parent along spline
        m_PlayerParentMovement.TryMove();

        if (m_playerState == PLAYER_ACTION_STATE.FLYING || m_playerState == PLAYER_ACTION_STATE.DASHING)
        {
            if (Accelerometer.current != null && Gyroscope.current != null && Accelerometer.current.enabled)
            {
                // TODO: Move Joystick/Gyro flag to settings class
                // Joystick controls
                if (m_IsJoystickMovement)
                {
                    // move player body along local x, y plane based on inputs
                    m_PlayerMovement.ControlPointXYMovement(m_MovementInput, false);
                }
                // Gyro controls
                else
                {
                    // TODO: Use m_MovementInput for gyro movement
                    // TODO: Fix drifting

                    Vector3 gyroscope = Gyroscope.current.angularVelocity.ReadValue();

                    // calculate the Y movement
                    CurrPitchRot += gyroscope.x / m_GryoscopePitchMult;
                    float YInput = CurrPitchRot;
                    // Clamp Pitch rotation
                    YInput = Math.Clamp(YInput, -1, 1);

                    // calculate the X movement
                    CurrYawRot += (gyroscope.y * -1) / m_GryoscopeYawMult;
                    float XInput = CurrYawRot;
                    // Clamp yaw rotation
                    XInput = Math.Clamp(XInput, -1, 1);

                    // move player body along local x, y plane based on inputs
                    m_PlayerMovement.ControlPointXYMovement(new Vector2(XInput, YInput), true);
                }
            } else
            {
                // move player body along local x, y plane based on inputs
                m_PlayerMovement.ControlPointXYMovement(m_MovementInput);
            }
            
        } else
        {
            // move player only along parent movement
            m_PlayerMovement.ControlPointXYMovement(new Vector2(0, 0));
            if (m_playerState == PLAYER_ACTION_STATE.DODGING)
            {
                m_PlayerMovement.PerformDodge();
            }
        }

        m_PlayerMovement.MothXYMovemnent();
    }

    private void OnFingerUp(EnhancedTouch.Finger finger)
    {
        CheckSwipeDodge(finger);
    }

    // Check if user tried performing dodge
    private void CheckSwipeDodge(EnhancedTouch.Finger finger)
    {
        EnhancedTouch.TouchHistory touchHistory = finger.touchHistory;
        // check for swipes, not single touches
        if (touchHistory.Count == 1) return;

        Vector2 firstTouchVec = finger.lastTouch.startScreenPosition;
        Vector2 lastTouchVec = finger.lastTouch.screenPosition;

        // If start or end of touch at left of screen, dont perform dodge
        if (firstTouchVec.x < Screen.width / 2 || lastTouchVec.x < Screen.width / 2)
        {
            return;
        }

        //float dotSwipe = Vector2.Dot(firstTouchVec, lastTouchVec);
        Vector2 DodgeDirection = lastTouchVec - firstTouchVec;
        // valid dodge if swipe in single direction and swiping left/right
        if (/*dotSwipe > 0 &&*/ DodgeDirection.magnitude > m_MinSwipeLengthToDodge)
        {
            StartDodge(DodgeDirection.normalized);
        }
    }
    
    private void OnFingerDown(EnhancedTouch.Finger finger)
    {
        // check if touching left side of screen
        if (finger.lastTouch.screenPosition.x < Screen.width / 2)
        {
            m_VirtualJoystick.StartJoystickTouch(finger);
        }
    }

    private void SetMobileMovementType(bool IsJoystickMovement)
    {
        m_IsJoystickMovement = IsJoystickMovement;
        m_VirtualJoystick.gameObject.SetActive(m_IsJoystickMovement);
    }

    public float DistanceFromPlayer(Vector3 pointToCompare)
    {
        return Vector3.Distance(pointToCompare, transform.position);
    }

    public void OnFireStart(InputValue value)
    {
        FireWeapon();
    }

    public void FireWeapon()
    {
        m_Weapon.IsShooting = true;
    }

    public void OnFireStop(InputValue value)
    {
        StopFiringWeapon();
    }

    public void StopFiringWeapon()
    {
        m_Weapon.IsShooting = false;
    }

    public void OnDodge(InputValue value)
    {
        StartDodge(m_MovementInput);
    }

    public void StartDodge(Vector2 DodgeDirection)
    {
        if (m_playerState == PLAYER_ACTION_STATE.FLYING && !m_IsDodgeCooldown)
        {
            if (GameState.PropertyInstance.GameStateEnum == GameStateEnum.TUTORIAL)
            {
                m_TutorialManager.ReceiveTutorialInput(TutorialInputs.DODGE);
            }
            
            m_playerState = PLAYER_ACTION_STATE.DODGING;
            m_Health.SetProjectileInvulnFrames(m_PlayerMovement.GetDodgeDuration());
            StartCoroutine(m_PlayerMovement.PlayerDodge(FinishAction, DodgeDirection));
        }
    }

    public void OnAimModeStart(InputValue value)
    {
        StartAimMode();
    }

    public void StartAimMode()
    {
        if (m_CurrEffect == DamageInfo.HIT_EFFECT.SLOW) return;
        m_MoonBarAbility.AimModeStartHelper();
    }

    public void OnAimModeEnd(InputValue value)
    {
        StopAimingMode();
    }

    public void StopAimingMode()
    {
        m_MoonBarAbility.AimModeEndHelper();
    }

    public void FlipAimMode()
    {
        if (m_MoonBarAbility.IsAimMode)
        {
            StopAimingMode();
        } else
        {
            StartAimMode();
        }
    }

    private void UpdateAimModeButton()
    {
#if UNITY_ANDROID
        // Update Aim mode button
#endif
    }

    public void OnDashStart(InputValue value)
    {
        StartDash();
    }

    public void StartDash()
    {
        if (m_CurrEffect == DamageInfo.HIT_EFFECT.SLOW) return;
        m_MoonBarAbility.OnDashStartHelper();
    }

    public void OnDashEnd(InputValue value)
    {
        StopDashing();
    }

    public void OnSkipStart()
    {
        if (GameState.PropertyInstance.GameStateEnum != GameStateEnum.TUTORIAL) return;

        m_TimeSkipPressed = Time.time;
        if (m_Checklist)
        {
            m_Checklist.StartSkipping(m_ButtonHoldLengthToSkip);
        }
        m_SkipCoroutine = StartCoroutine(SkipCoroutine());
    }

    private IEnumerator SkipCoroutine()
    {
        // Loop while full skip time not reached
        while (Time.time - m_TimeSkipPressed < m_ButtonHoldLengthToSkip)
        {
            yield return null;
        }
        OnSkipEnd();
    }

    public void OnSkipEnd()
    {
        if (GameState.PropertyInstance.GameStateEnum != GameStateEnum.TUTORIAL) return;

        // Check if skip button held for long enough
        if (Time.time - m_TimeSkipPressed >= m_ButtonHoldLengthToSkip)
        {
            PerformSkip();
        }

        m_TimeSkipPressed = 0;
        StopCoroutine(m_SkipCoroutine);

        // Stop Skipping graphic
        if (m_Checklist)
        {
            m_Checklist.StopSkipping();
        }
    }

    private void PerformSkip()
    {
        // If in the tutorial, skip tutorial
        if (GameState.PropertyInstance.GameStateEnum == GameStateEnum.TUTORIAL)
        {
            if (m_TutorialManager != null)
            {
                m_TutorialManager.SetTutorialsFinished();
            }
            
        }
    }

    public void StopDashing()
    {
        m_MoonBarAbility.OnDashEndHelper();
    }

    public void FlipDashing()
    {
        if (m_MoonBarAbility.IsDashing)
        {
            StopDashing();
        } else
        {
            StartDash();
        }
    }

    private void UpdateDashButton()
    {
#if UNITY_ANDROID
        // Update Dash button
#endif
    }

    public void LostMothCollected()
    {
        if (lostMothCollectedDelegate != null)
            lostMothCollectedDelegate();
        UpdateLostMothText();
    }

    private void UpdateLostMothText()
    {
        m_LostMothUI.text = GameManager.PropertyInstance.LostMothCount.ToString() + "/" + GameManager.PropertyInstance.CurrLostMothWinCondition();
    }

    public override void Death() 
    {
        // TODO: Fall down? add delay to next screen in GameManager?
    } 

    // Set current level as won, goto next or win game
    public void StartTransition()
    {
        StartCoroutine(TransitionPhase());
    }

    // Fogs up player's screen, and calls next logic for the transition
    private IEnumerator TransitionPhase()
    {
        // invincible during transition
        m_Health.SetAllInvulnFrames(3f);
        UIManager.PropertyInstance.FadeIn(m_FogInTransitionDuration);

        yield return new WaitForSeconds(m_FogInTransitionDuration);

        m_PlayerInput.enabled = false;
        // Call rest of transition logic
        m_PlayerParentMovement.DisconnectFromSpline();

        // Begin tile transition
        TileManager.PropertyInstance.TileSetFinished();
        
        // wait for transition state to be over to reconnect to spline
        while (GameState.PropertyInstance.GameStateEnum != GameStateEnum.RUNNING)
        {
            yield return null;
        }

        m_PlayerParentMovement.ConnectBackToSpline();
        m_CameraMovement.ResetPosition();

        float delay = 1f;
        while (delay > 0)
        {
            delay -= Time.deltaTime;
            yield return null;
        }
       

        UIManager.PropertyInstance.FadeOut(m_FogOutTransitionDuration);
        ActivateLostMothUI();
        m_PlayerInput.enabled = true;
    }

    private void FinishAction()
    {
        // if player was dodging, start dodge cooldown
        if (m_playerState == PLAYER_ACTION_STATE.DODGING)
        {
            StartCoroutine(DodgeCooldown());
        }

        m_playerState = PLAYER_ACTION_STATE.FLYING;
    }

    IEnumerator DodgeCooldown()
    {
        m_IsDodgeCooldown = true;
        yield return new WaitForSeconds(m_DodgeCooldown);
        m_IsDodgeCooldown = false;
    }

    protected override void ApplyEffect(DamageInfo.HIT_EFFECT effect)
    {
        if (effect == DamageInfo.HIT_EFFECT.SLOW)
        {
            ApplySlow();
            // Cancel any moon abilities that are currently being used
            m_MoonBarAbility.OnDashEndHelper();
            m_MoonBarAbility.AimModeEndHelper();
        }
    }

    private void ApplySlow()
    {
        // cancel currently running slow effect and reset if one running
        if (m_SlowEffectCoroutine != null)
        {
            StopCoroutine(m_SlowEffectCoroutine);
        }
        m_SlowEffectCoroutine = StartCoroutine(SlowEffectCoroutine());
    }

    IEnumerator SlowEffectCoroutine()
    {
        m_CurrEffect = DamageInfo.HIT_EFFECT.SLOW;
        m_PlayerParentMovement.SLowEffectStart();
        yield return new WaitForSeconds(m_SlowEffectDuration);
        m_CurrEffect = DamageInfo.HIT_EFFECT.NORMAL;
        m_PlayerParentMovement.SlowEffectStop();
    }

    // Converts a world position to local wrt player and returns z distance from the camera
    // Negative Z-distance means the positions is behind the player camera
    public float ZDistanceFromPlayerCamera(Vector3 position)
    {
        // COnvert firefly position to player's parent local space
        Vector3 localPlayerPos = PlayerParent.transform.InverseTransformPoint(position);
        return localPlayerPos.z;
    }

    public void OnGlideFlapAnimationPerformed() 
    {
        StartCoroutine(GlideDelay());
    }

    IEnumerator GlideDelay()
    {
        m_Animator.SetBool("isGlideDelay", true);
        float rand = UnityEngine.Random.Range(m_GlideFlapDelayMin, m_GlideFlapDelayMax);
        yield return new WaitForSeconds(rand);
        int randAnim = UnityEngine.Random.Range(0, 2);
        if (randAnim == 0)
            m_Animator.SetBool("isLargeFlap", true);
        else
            m_Animator.SetBool("isLargeFlap", false);
        m_Animator.SetBool("isGlideDelay", false);
    }

    public PlayerParentMovement PlayerParent { get { return m_PlayerParentMovement;  } }
    public PlayerMovement PlayerMovement { get { return m_PlayerMovement; } }
    public CameraMovement CameraMovement { get { return m_CameraMovement; } }

    enum PLAYER_ACTION_STATE
    {
        FLYING,
        DODGING,
        DASHING
    }
}
