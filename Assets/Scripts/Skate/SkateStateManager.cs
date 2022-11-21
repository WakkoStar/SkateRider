using System.Collections;
using System;
using UnityEngine;
using UnityEngine.Events;

public class SkateStateManager : MonoBehaviour
{


    //OUTSIDER COMPONENTS
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private SkateMainScreen skateMainScreen;
    [SerializeField] private SkateEndScreen skateEndScreen;

    //SKATE INPUT PROPS
    [SerializeField] private GameObject NoseZone;
    [SerializeField] private GameObject TailZone;
    [SerializeField] private Vector2 noseZoneUIOffset;
    [SerializeField] private Vector2 tailZoneUIOffset;
    [SerializeField] private SkateInputCollider noseZoneCollider;
    [SerializeField] private SkateInputCollider tailZoneCollider;


    //SKATE PHYSICS CONTROLLER PROPS
    [SerializeField] private float speedForce;
    [SerializeField] private float flipForce;
    [SerializeField] private float rotateForce;
    [SerializeField] private float maxAngle;
    [SerializeField] private float maxSkateHeight;


    //COMPONENTS
    private SkateTerrainReader _skateTerrainReader;
    private SkateRotationReader _skateRotationReader;
    private SkatePhysicsController _skatePhysController;
    private SkateScoreManager _skateScoreManager;
    private SkateCollectibleCounter _skateCollectibleCounter;
    private SkateInput _skateInput;
    private TimerTrigger _timerTrigger;


    //EVENTS
    public UnityEvent OnInit = new UnityEvent();
    public UnityEvent OnGameOver = new UnityEvent();
    public UnityEvent OnStartGame = new UnityEvent();
    public UnityEvent OnJumpLandingEvent = new UnityEvent();
    public UnityEvent OnJumpEvent = new UnityEvent();
    public UnityEvent<Vector2> OnJumpWithVelocity = new UnityEvent<Vector2>();
    public UnityEvent OnBoost = new UnityEvent();


    //ACTIONS
    private UnityAction _onBoostAction;


    //STATE
    private bool _isJumpAuthorized;
    private bool _isOnGrind;
    private bool _isJumping;
    private bool _isLanding;
    private bool _isOllie;
    private bool _isPushLanding;
    private bool _isSkateStateStopped;
    private bool _isSwitchOnJump;
    private bool _shouldDisplayGrindTrick;
    private bool _isTailTouchPressed;
    private bool _isNoseTouchPressed;


    //LOCAL VALUES
    private string[] staticTracksNames = new string[] { "onSlide", "onGround", "WheelsOnAir" };


    void Start()
    {
        Application.targetFrameRate = 60;

        _skateTerrainReader = gameObject.AddComponent<SkateTerrainReader>();
        _skateRotationReader = gameObject.AddComponent<SkateRotationReader>();
        _skatePhysController = gameObject.AddComponent<SkatePhysicsController>();
        _skateInput = gameObject.AddComponent<SkateInput>();
        _skateScoreManager = gameObject.AddComponent<SkateScoreManager>();
        _skateCollectibleCounter = new SkateCollectibleCounter();

        _timerTrigger = gameObject.AddComponent<TimerTrigger>();

        _skatePhysController.skateState = this;
        _skatePhysController.speedForce = speedForce;
        _skatePhysController.flipForce = flipForce;
        _skatePhysController.rotateForce = rotateForce;
        _skatePhysController.maxAngle = maxAngle;
        _skatePhysController.maxSkateHeight = maxSkateHeight;

        _skateInput.NoseZone = NoseZone;
        _skateInput.TailZone = TailZone;
        _skateInput.noseZoneUIOffset = noseZoneUIOffset;
        _skateInput.tailZoneUIOffset = tailZoneUIOffset;


        _timerTrigger.triggerTime = 1;
        _timerTrigger.onTimeTriggered = OnNoTouch;
        _timerTrigger.onResponse = OnTouchResponse;

        _skateScoreManager.skateState = this;
        _skateCollectibleCounter.skateState = this;

        _onBoostAction += OnBoostFunc;
        OnBoost.AddListener(_onBoostAction);

        InitState();
    }

    void FixedUpdate()
    {
        if (IsSkateStateStopped()) return;

        if (GetSkateTerrainReader().IsOnTerrain())
        {
            //MANUAL AND OLLIE
            InclineAndJump(GetIsNoseTouch(), GetIsTailTouch(), true);
            //NOSE AND NOLLIE
            InclineAndJump(GetIsTailTouch(), GetIsNoseTouch(), false);

            if (GetIsNoseTouch() && GetIsTailTouch()) AuthorizeJump(true);

            if (IsOnGrind())
            {
                OnGrind();
            }
            else
            {
                OnGround();
            }
            if (IsJumping())
            {
                OnJumpLanding();
                SetIsJumping(false);
            }

        }
        else
        {
            OnAir();
            AuthorizeJump(false);
            SetIsLanding(true);
        }

        if (IsLanding() && _skateTerrainReader.IsOnTerrain())
        {
            OnLanding();
            SetIsLanding(false);
        }
        SetIsPushLanding(!_skateTerrainReader.IsOnTerrain() && GetIsNoseTouch() && GetIsTailTouch());

        if (!IsOnGrind())
        {
            SetShouldDisplayGrindTrick(true);
            _skateScoreManager.AddAndResetGrindScore();
        }

        _skateScoreManager.SetShouldStopScore(!_skatePhysController.IsMoving());

        DisplayInputCollider(
            _skateInput.GetIsNoseTouch(),
            IsNoseTouchPressed(),
            Color.blue,
            SetIsNoseTouchPressed,
            false
        );
        DisplayInputCollider(
            _skateInput.GetIsTailTouch(),
            IsTailTouchPressed(),
            Color.red,
            SetIsTailTouchPressed,
            true
        );

        if (!_skatePhysController.IsMoving())
        {
            audioManager.StopAll(staticTracksNames);
        }
    }

    private void InclineAndJump(bool isTouchOne, bool isTouchTwo, bool isManual)
    {
        var isExpectedJump = isManual ? _skateInput.GetIsOllie() : _skateInput.GetIsNollie();
        if (!isTouchOne)
        {
            if (!isTouchTwo && IsJumpAuthorized() == true && isExpectedJump)
            {
                OnJump(isManual);
                SetIsOllie(isManual);
                SetIsJumpingWithDelay(true, 0.1f);
                AuthorizeJump(false);
            }
            else if (isTouchTwo)
            {
                OnIncline(isManual);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {

    }

    private void OnTriggerStay(Collider other)
    {
        SetIsOnGrind(other.gameObject.tag == "Grind");
    }

    private void OnTriggerExit(Collider other)
    {
        SetIsOnGrind(false);
    }

    public bool GetIsNoseTouch()
    {
        return _skateRotationReader.IsSwitch() ? _skateInput.GetIsTailTouch() : _skateInput.GetIsNoseTouch();
    }
    public bool GetIsTailTouch()
    {
        return _skateRotationReader.IsSwitch() ? _skateInput.GetIsNoseTouch() : _skateInput.GetIsTailTouch();
    }
    private bool GetIsInclined()
    {
        if (_skateTerrainReader.IsOnTerrain())
        {
            return (GetIsNoseTouch() || GetIsTailTouch()) && !(GetIsNoseTouch() && GetIsTailTouch()); //XOR
        }

        return false;
    }


    private void AuthorizeJump(bool value)
    {
        _isJumpAuthorized = value;
    }
    private bool IsJumpAuthorized()
    {
        return _isJumpAuthorized;
    }



    private void SetIsOnGrind(bool value)
    {
        _isOnGrind = value;
    }
    public bool IsOnGrind()
    {
        return _isOnGrind;
    }

    private void SetIsSwitchOnJump(bool value)
    {
        _isSwitchOnJump = value;
    }
    private bool IsSwitchOnJump()
    {
        return _isSwitchOnJump;
    }



    private void SetIsNoseTouchPressed(bool value)
    {
        _isNoseTouchPressed = value;
    }
    private bool IsNoseTouchPressed()
    {
        return _isNoseTouchPressed;
    }
    private void SetIsTailTouchPressed(bool value)
    {
        _isTailTouchPressed = value;
    }
    private bool IsTailTouchPressed()
    {
        return _isTailTouchPressed;
    }



    private void SetShouldDisplayGrindTrick(bool value)
    {
        _shouldDisplayGrindTrick = value;
    }

    private bool ShouldDisplayGrindTrick()
    {
        return _shouldDisplayGrindTrick;
    }



    private void SetIsJumping(bool value)
    {
        _isJumping = value;
    }
    private void SetIsJumpingWithDelay(bool value, float delay)
    {
        StartCoroutine(SetIsJumpingCoroutine(value, delay));
    }
    private IEnumerator SetIsJumpingCoroutine(bool value, float delay)
    {
        yield return new WaitForSeconds(delay);
        SetIsJumping(value);
    }
    public bool IsJumping()
    {
        return _isJumping;
    }



    private void SetIsLanding(bool value)
    {
        _isLanding = value;
    }
    private bool IsLanding()
    {
        return _isLanding;
    }


    public void SetIsPushLanding(bool value)
    {
        _isPushLanding = value;
    }
    public bool IsPushLanding()
    {
        return _isPushLanding;
    }



    /**
    A jump in skate, to get the difference between nollie and ollie
    Nollie : jump made with the nose of the skate
    Ollie : jump made with the tail of the skate
    */
    private void SetIsOllie(bool value)
    {
        _isOllie = value;
    }
    private bool IsOllie()
    {
        return _isOllie;
    }


    public SkateRotationReader GetSkateRotationReader()
    {
        return _skateRotationReader;
    }
    public SkateTerrainReader GetSkateTerrainReader()
    {
        return _skateTerrainReader;
    }
    public SkatePhysicsController GetSkatePhysicsController()
    {
        return _skatePhysController;
    }
    public SkateInput GetSkateInput()
    {
        return _skateInput;
    }
    public SkateMainScreen GetSkateMainScreen()
    {
        return skateMainScreen;
    }


    public void InitState()
    {
        _skateScoreManager.SetInit();
        _skatePhysController.SetInit();
        _isSkateStateStopped = true;

        OnInit.Invoke();
    }

    public void StartGame()
    {
        transform.localPosition = Vector3.zero;
        transform.localEulerAngles = Vector3.zero;

        _isSkateStateStopped = false;

        _timerTrigger.ResetTime();
        _skateScoreManager.SetStartGame();
        _skateCollectibleCounter.SetStartGame();
        _skatePhysController.SetStartGame();

        OnStartGame.Invoke();
    }

    public void SetGameOver()
    {
        audioManager.StopAll(staticTracksNames);

        var isBestScore = _skateScoreManager.GetTotalScore() > PlayerPrefs.GetFloat("bestScore");

        if (_skateScoreManager.GetTotalScore() > PlayerPrefs.GetFloat("bestScore"))
        {
            PlayerPrefs.SetFloat("bestScore", _skateScoreManager.GetTotalScore());
        }
        PlayerPrefs.SetInt("collectibleCount", PlayerPrefs.GetInt("collectibleCount") + _skateCollectibleCounter.GetCollectibleCount());

        skateEndScreen.DisplayBestScoreSign(isBestScore);
        skateEndScreen.DisplayBestScore(PlayerPrefs.GetFloat("bestScore"));
        skateEndScreen.DisplayScore(_skateScoreManager.GetTotalScore());
        skateEndScreen.DisplayCollectibleCount(_skateCollectibleCounter.GetCollectibleCount());
        skateEndScreen.DisplayAllCollectibleCount(PlayerPrefs.GetInt("collectibleCount"));

        _isSkateStateStopped = true;

        _skatePhysController.SetGameOver();

        OnGameOver.Invoke();
    }

    public void SetGameOverWithDelay(float delay)
    {
        StartCoroutine(SetGameOverWithDelayCoroutine(delay));
    }
    private IEnumerator SetGameOverWithDelayCoroutine(float delay)
    {
        _isSkateStateStopped = true;
        yield return new WaitForSeconds(delay);
        SetGameOver();
    }

    public bool IsSkateStateStopped()
    {
        return _isSkateStateStopped;
    }



    private void OnGround()
    {
        if (_skateRotationReader.IsTrueUpsideDown() && !_skateTerrainReader.IsGrindOnTerrain())
        {
            SetGameOverWithDelay(0.5f);
        }
        else if (!GetIsInclined())
        {
            audioManager.PlayOnly("onGround", staticTracksNames, (_skatePhysController.GetForwardingSpeed() / 20) + 1);
        }

        if ((_skateRotationReader.IsPerpendicular()))
        {
            audioManager.Play("Drift", 1.05f);
        }

        if (!GetIsNoseTouch() && !GetIsTailTouch())
        {
            _timerTrigger.IncrementTime(Time.deltaTime);
        }
        else
        {
            _timerTrigger.ResetTime();
        }


        _skatePhysController.OnGround();

    }

    private void OnGrind()
    {
        _skateScoreManager.SetGrindScore(_skateScoreManager.GetGrindScore() + 1);

        if (_skateScoreManager.GetGrindScore() > 5 && ShouldDisplayGrindTrick())
        {
            skateMainScreen.AddTrickToDisplay(
            Tricks.GetGrindTrick(
                Anchors.GetSimpleNearestAnchor(transform.localEulerAngles.z),
                Anchors.GetFullNearestAnchor(transform.localEulerAngles.y),
                _skateRotationReader.IsSwitch()
            ));

            SetShouldDisplayGrindTrick(false);
        }

        audioManager.PlayOnly("onSlide", staticTracksNames);

        _skatePhysController.OnGrind();
    }

    private void OnJumpLanding()
    {
        OnJumpLandingEvent.Invoke();

        skateMainScreen.AddTrickToDisplay(
            Tricks.GetTrick(
                IsOllie(),
                _skatePhysController.GetFlipAmount(),
                _skatePhysController.GetRotateAmount(),
                IsSwitchOnJump()
            )
        );
        audioManager.Play("Landing", 1.5f);

        _timerTrigger.ResetTime();

        _skatePhysController.OnLanding();
    }

    private void OnLanding()
    {
        _skateScoreManager.AddAndResetTrickScore();
        audioManager.Play("Landing", 1.5f);
    }

    private void OnAir()
    {
        audioManager.PlayOnly("WheelsOnAir", staticTracksNames, 1.8f);

        var totalFlipAmount = Mathf.Abs(_skatePhysController.GetFlipAmount()) / _skatePhysController.GetFlipForce();
        var totalRotateAmount = Mathf.Abs(_skatePhysController.GetRotateAmount()) / _skatePhysController.GetRotateForce();
        _skateScoreManager.SetTrickScore((int)(totalFlipAmount + totalRotateAmount));

        _skatePhysController.OnAir();
    }

    private void OnJump(bool isManual)
    {
        audioManager.Play("Jump");

        _skatePhysController.OnJump(isManual);
        SetIsSwitchOnJump(_skateRotationReader.IsSwitch());

        _skateInput.ResetNoseOffset();
        _skateInput.ResetTailOffset();

        OnJumpEvent.Invoke();

    }

    private void OnIncline(bool isManual)
    {
        if (!IsOnGrind())
        {
            audioManager.PlayOnly("onGround", staticTracksNames, 1.8f);
        }

        _skatePhysController.OnIncline(isManual);
    }

    private void OnBoostFunc()
    {
        audioManager.Play("Boost");
        _skatePhysController.OnBoost();
    }

    private void OnNoTouch()
    {
        skateMainScreen.DisplayNoTouchNotifier();
        _skatePhysController.OnNoTouch();
    }

    private void OnTouchResponse()
    {
        skateMainScreen.HideNoTouchNotifier();
        _skatePhysController.ForceMaxSpeed(true);
    }

    public void AddCollectible()
    {
        _skateCollectibleCounter.AddCollectible();
    }

    private void DisplayInputCollider(bool isInputTouch, bool isTouchPressed, Color color, Action<bool> SetTouchPress, bool isTailMode)
    {
        var normalCollider = isTailMode ? tailZoneCollider : noseZoneCollider;
        var switchCollider = isTailMode ? noseZoneCollider : tailZoneCollider;
        if (isInputTouch && isTouchPressed)
        {
            if (_skateRotationReader.IsSwitch())
            {
                switchCollider.Display(color);
            }
            else
            {
                normalCollider.Display(color);
            }
            SetTouchPress(false);
        }

        if (!isInputTouch)
        {
            SetTouchPress(true);
        }
    }

    public void SetMaxSpeed(float value)
    {
        _skatePhysController.SetMaxSpeed(value);
    }

    public void SetJumpForce(float value)
    {
        _skatePhysController.SetJumpForce(value);
    }
}
