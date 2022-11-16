using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SkateStateManager : MonoBehaviour
{

    [SerializeField] private AudioManager audioManager;
    [SerializeField] private GUIController guiController;
    [SerializeField] private SkateInput skateInput;
    [SerializeField] private DifficultyIncreaser difficultyIncreaser;


    [SerializeField] private SkateInputCollider noseZoneCollider;
    [SerializeField] private SkateInputCollider tailZoneCollider;

    [SerializeField] private float speedForce;
    [SerializeField] private float flipForce;
    [SerializeField] private float rotateForce;
    [SerializeField] private float maxAngle;
    [SerializeField] private float maxSkateHeight;

    [SerializeField] private float maxStartSpeed = 10f;
    [SerializeField] private float maxEndSpeed = 10f;
    [SerializeField] private float jumpStartForce = 350f;
    [SerializeField] private float jumpEndForce = 350f;

    private SkateTerrainReader _skateTerrainReader;
    private SkateRotationReader _skateRotationReader;
    private SkateController _skateController;
    private SkateScoreManager _skateScoreManager;
    private SkateCollectibleCounter _skateCollectibleCounter;

    public UnityEvent OnJumpLandingEvent = new UnityEvent();
    public UnityEvent OnJumpEvent = new UnityEvent();
    public UnityEvent<Vector2> OnJumpWithVelocity = new UnityEvent<Vector2>();
    public UnityEvent OnBoost = new UnityEvent();

    private bool _isJumpAuthorized;
    private bool _isOnGrind;
    private bool _isJumping;
    private bool _isLanding;
    private bool _isOllie;
    private bool _isPushLanding;
    private bool _isGameOver;
    private bool _isSwitchOnJump;
    private bool _shouldDisplayGrindTrick;
    private bool _isTailTouchPressed;
    private bool _isNoseTouchPressed;

    public delegate void TouchPressure(bool value);
    private UnityAction _onBoostAction;

    //LOCAL VALUES
    private string[] staticTracksNames = new string[] { "onSlide", "onGround", "WheelsOnAir" };

    void Start()
    {
        _skateTerrainReader = gameObject.AddComponent<SkateTerrainReader>();
        _skateRotationReader = gameObject.AddComponent<SkateRotationReader>();
        _skateController = gameObject.AddComponent<SkateController>();
        _skateScoreManager = gameObject.AddComponent<SkateScoreManager>();
        _skateCollectibleCounter = new SkateCollectibleCounter();

        _skateController.Init(
            speedForce,
            flipForce,
            rotateForce,
            maxAngle,
            maxSkateHeight,
            _skateTerrainReader,
            _skateRotationReader,
            skateInput,
            this
        );

        _skateScoreManager.guiController = guiController;

        _skateCollectibleCounter.guiController = guiController;

        _onBoostAction += OnBoostFunc;
        OnBoost.AddListener(_onBoostAction);

        difficultyIncreaser.AddIncreaser<float>(maxStartSpeed, maxEndSpeed, _skateController.SetMaxSpeed);
        difficultyIncreaser.AddIncreaser<float>(jumpStartForce, jumpEndForce, _skateController.SetJumpForce);
    }

    void FixedUpdate()
    {
        if (IsGameOver())
        {
            return;
        }

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

        _skateScoreManager.SetShouldStopScore(!_skateController.IsMoving());

        DisplayInputCollider(
            skateInput.GetIsNoseTouch(),
            IsNoseTouchPressed(),
            Color.blue,
            SetIsNoseTouchPressed,
            false
        );
        DisplayInputCollider(
            skateInput.GetIsTailTouch(),
            IsTailTouchPressed(),
            Color.red,
            SetIsTailTouchPressed,
            true
        );

        if (!_skateController.IsMoving())
        {
            audioManager.StopAll(staticTracksNames);
        }
    }

    private void InclineAndJump(bool isTouchOne, bool isTouchTwo, bool isManual)
    {
        var isExpectedJump = isManual ? skateInput.GetIsOllie() : skateInput.GetIsNollie();
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
        return _skateRotationReader.IsSwitch() ? skateInput.GetIsTailTouch() : skateInput.GetIsNoseTouch();
    }
    public bool GetIsTailTouch()
    {
        return _skateRotationReader.IsSwitch() ? skateInput.GetIsNoseTouch() : skateInput.GetIsTailTouch();
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
    public SkateController GetSkateController()
    {
        return _skateController;
    }


    public void SetGameOver()
    {
        audioManager.StopAll(staticTracksNames);
        _skateScoreManager.SetShouldStopScore(true);
        Debug.Log("you loose !");

        _isGameOver = true;
    }

    public bool IsGameOver()
    {
        return _isGameOver;
    }



    private void OnGround()
    {
        if (_skateRotationReader.IsUpsideDown())
        {
            SetGameOver();
        }
        else if (!GetIsInclined())
        {
            audioManager.PlayOnly("onGround", staticTracksNames, 1.5f);
        }

        if ((_skateRotationReader.IsPerpendicular()))
        {
            audioManager.Play("Drift", 1.05f);
        }

        _skateController.OnGround();

    }

    private void OnGrind()
    {
        _skateScoreManager.SetGrindScore(_skateScoreManager.GetGrindScore() + 1);

        if (_skateScoreManager.GetGrindScore() > 5 && ShouldDisplayGrindTrick())
        {
            guiController.AddTrickToDisplay(
            Tricks.GetGrindTrick(
                Anchors.GetSimpleNearestAnchor(transform.localEulerAngles.z),
                Anchors.GetFullNearestAnchor(transform.localEulerAngles.y),
                _skateRotationReader.IsSwitch()
            ));

            SetShouldDisplayGrindTrick(false);
        }

        audioManager.PlayOnly("onSlide", staticTracksNames);

        _skateController.OnGrind();
    }

    private void OnJumpLanding()
    {
        OnJumpLandingEvent.Invoke();

        guiController.AddTrickToDisplay(
            Tricks.GetTrick(
                IsOllie(),
                _skateController.GetFlipAmount(),
                _skateController.GetRotateAmount(),
                IsSwitchOnJump()
            )
        );
        audioManager.Play("Landing", 1.5f);

        _skateController.OnLanding();
    }

    private void OnLanding()
    {
        _skateScoreManager.AddAndResetTrickScore();
        audioManager.Play("Landing", 1.5f);
    }

    private void OnAir()
    {
        audioManager.PlayOnly("WheelsOnAir", staticTracksNames, 1.8f);

        var totalFlipAmount = Mathf.Abs(_skateController.GetFlipAmount()) / _skateController.GetFlipForce();
        var totalRotateAmount = Mathf.Abs(_skateController.GetRotateAmount()) / _skateController.GetRotateForce();
        _skateScoreManager.SetTrickScore((int)(totalFlipAmount + totalRotateAmount));

        _skateController.OnAir();
    }

    private void OnJump(bool isManual)
    {
        audioManager.Play("Jump");

        _skateController.OnJump(isManual);
        SetIsSwitchOnJump(_skateRotationReader.IsSwitch());

        skateInput.ResetNoseOffset();
        skateInput.ResetTailOffset();

        OnJumpEvent.Invoke();

    }

    private void OnIncline(bool isManual)
    {
        if (!IsOnGrind())
        {
            audioManager.PlayOnly("onGround", staticTracksNames, 1.8f);
        }

        _skateController.OnIncline(isManual);
    }

    private void OnBoostFunc()
    {
        audioManager.Play("Boost");

        _skateController.OnBoost();
    }

    public void AddCollectible()
    {
        _skateCollectibleCounter.AddCollectible();
    }

    private void DisplayInputCollider(bool isInputTouch, bool isTouchPressed, Color color, TouchPressure SetTouchPress, bool isTailMode)
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
}
