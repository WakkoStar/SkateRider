using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class SkateController : MonoBehaviour
{
    //SETTINGS
    [SerializeField] private SkateInput skateInput;
    [SerializeField] private GUIController guiController;
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private LevelGenerator levelGenerator;
    [SerializeField] private float maxStartSpeed = 10f;
    [SerializeField] private float maxEndSpeed = 10f;
    [SerializeField] private float jumpStartForce = 350f;
    [SerializeField] private float jumpEndForce = 350f;
    [SerializeField] private float speedForce = 4000f;
    [SerializeField] private float flipForce = 6;
    [SerializeField] private float rotateForce = 6;
    [SerializeField] private float maxAngle = 0.33f;
    [SerializeField] private Vector3 cameraStartPos = new Vector3(4, 3, -15);
    [SerializeField] private Vector3 cameraEndPos = new Vector3(4, 3, -15);
    [SerializeField] private float startFOV = 80;
    [SerializeField] private float endFOV = 80;
    [SerializeField] private float maxSkateOffset = 10f;
    [SerializeField] private UnityEvent _onJump;

    //STATE
    [HideInInspector] public float jumpForce = 400f;
    private BoxCollider _groundDetection;
    private GameObject _MainCamera;
    private Vector3 _cameraPos;
    private Rigidbody _rb;
    private bool _canJump = false;
    private bool _isOnWheel = false;
    private bool _isOnGrind = false;
    private bool _isJumping = false;
    private bool _isGrindUnder = false;
    private bool _isOllie;
    private bool _isSwitch;
    private bool _isNoseTouch;
    private bool _isTailTouch;
    private bool _canPlayLanding;
    private Vector3 _groundInclinaison;
    private float _hitDistance;
    private Coroutine _flipCoroutine;
    private float _rotateAmount = 0;
    private float _flipAmount = 0;
    private float _maxSpeed = 9f;
    private int _trickScore;
    private float _totalScore;
    private float _minSensitivity = 100;


    //VALUES
    RigidbodyConstraints _normalConstraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezePositionZ;
    RigidbodyConstraints _onGrindConstraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionZ;

    void Start()
    {
        Camera.main.fieldOfView = startFOV;
        _maxSpeed = maxStartSpeed;
        jumpForce = jumpStartForce;
        _cameraPos = cameraStartPos;

        if (_onJump == null)
            _onJump = new UnityEvent();

        _groundDetection = GetComponent<BoxCollider>();
        _MainCamera = Camera.main.gameObject;

        _rb = GetComponent<Rigidbody>();
        _rb.velocity = Vector3.right;

        Application.targetFrameRate = 60;

        StartCoroutine(IncreaseDifficulty());
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Grind")
        {
            guiController.AddTrickToDisplay(
                Tricks.GetGrindTrick(
                    Anchors.GetSimpleNearestAnchor(transform.localEulerAngles.z),
                    Anchors.GetFullNearestAnchor(transform.localEulerAngles.y),
                    _isSwitch
                ));
        }
    }

    private void OnTriggerStay(Collider other)
    {
        _isOnGrind = other.gameObject.tag == "Grind";
    }

    private void OnTriggerExit(Collider other)
    {
        _isOnGrind = false;
    }

    void FixedUpdate()
    {
        //VALUES
        Physics.gravity = Vector3.down * 20;
        _rb.constraints = _normalConstraints;

        // //DEFEAT CONDITON
        // if (_rb.velocity.x < 0.3) return;

        //SET DIRECTION
        var direction = _rb.velocity.x < _maxSpeed ? Vector3.right : -Vector3.right;
        _rb.AddForce(direction * speedForce, ForceMode.Acceleration);

        //JUMP HEIGHT
        if (_hitDistance > maxSkateOffset)
        {
            if (_rb.velocity.y > 20) _rb.AddForce(-Vector3.up * 400, ForceMode.Acceleration);
        }

        //TOUCHES
        _isNoseTouch = _isSwitch ? skateInput.GetIsTailTouch() : skateInput.GetIsNoseTouch();
        _isTailTouch = _isSwitch ? skateInput.GetIsNoseTouch() : skateInput.GetIsTailTouch();

        //GROUND INFOS
        GetGroundInformations();

        if (_isOnWheel)
        {
            //MANUAL AND OLLIE
            InclineAndJump(_isNoseTouch, _isTailTouch, true);
            //NOSE AND NOLLIE
            InclineAndJump(_isTailTouch, _isNoseTouch, false);

            if (_isNoseTouch && _isTailTouch) _canJump = true;

            if (_isOnGrind)
            {
                OnGrind();
            }
            else
            {
                OnGround();
            }
            if (_isJumping)
            {
                OnLanding();
                _isJumping = false;
            }

        }
        else
        {
            OnAir();
            _canJump = false;
            _canPlayLanding = true;
        }
    }

    void Update()
    {
        //SET CAMERA
        SetCameraMovement();

        //SCORE
        guiController.DisplayScore(_totalScore += 1);
        guiController.DisplayJumpScore(_trickScore);

        //GET SWITCH
        _isSwitch = transform.localEulerAngles.y > 90 && transform.localEulerAngles.y < 270;

        if (_canPlayLanding && _isOnWheel)
        {
            audioManager.Play("Landing", 1.5f);
            _canPlayLanding = false;
        }

    }

    void OnGrind()
    {
        _rb.constraints = _onGrindConstraints;
        GoToAnchors(Anchors.GetGrindAnchors(transform.localEulerAngles));
        _trickScore += 1;

        PlayOnly("onSlide");
    }

    void OnGround()
    {
        var tEulerAng = transform.localEulerAngles;
        var rotOffset = Mathf.Abs(tEulerAng.y) % 360;

        transform.localEulerAngles = Vector3.Lerp(
            tEulerAng,
            Anchors.GetGroundAnchors(transform.localEulerAngles),
            0.1f
        );

        if (_isNoseTouch && _isTailTouch)
        {
            _rb.angularVelocity = Vector3.Lerp(_rb.angularVelocity, Vector3.zero, 0.04f);
        }

        AddAndResetTrickScore();

        if (!GetIsInclined())
        {
            PlayOnly("onGround", 1.5f);
        }
        if ((rotOffset > 45 && rotOffset < 135) || (rotOffset > 235 && rotOffset < 315))
        {
            audioManager.Play("Drift", 1.05f);
        }
    }

    void OnLanding()
    {
        //STOP TRICK
        if (_flipCoroutine != null) StopCoroutine(_flipCoroutine);
        audioManager.Play("Landing", 1.5f);

        //GET TRICK NAME
        guiController.AddTrickToDisplay(Tricks.GetTrick(_isOllie, _flipAmount, _rotateAmount, _isSwitch));

        AddAndResetTrickScore();
    }

    void OnAir()
    {
        if (_isNoseTouch && _isTailTouch)
        {
            Physics.gravity = Vector3.down * 60;

            //STOP TRICK
            if (_flipCoroutine != null) StopCoroutine(_flipCoroutine);

            GoToAnchors(
                _isGrindUnder
                ? Anchors.GetGrindAnchors(transform.localEulerAngles)
                : Anchors.GetAirAnchors(transform.localEulerAngles)
            );
        }

        PlayOnly("WheelsOnAir", 1.8f);
    }

    private void InclineAndJump(bool isTouchOne, bool isTouchTwo, bool isManual)
    {
        var isExpectedJump = isManual ? skateInput.GetIsOllie() : skateInput.GetIsNollie();
        if (!isTouchOne)
        {
            if (!isTouchTwo && _canJump == true && isExpectedJump)
            {
                OnJump(isManual);
                _canJump = false;
            }
            else if (isTouchTwo)
            {
                OnIncline(isManual);
            }
        }
    }

    void OnJump(bool isManual)
    {
        _isOllie = isManual; //USED FOR TRICKS

        _rb.angularVelocity = Vector3.zero;
        _rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

        _rotateAmount = 0;
        _flipAmount = 0;
        _trickScore = 0;

        var flipDirection = GetFlipFirection(isManual, skateInput.GetNoseOffset().y, skateInput.GetTailOffset().y);
        var rotateDirection = GetFlipFirection(isManual, skateInput.GetTailOffset().y, skateInput.GetNoseOffset().y);

        _flipCoroutine = StartCoroutine(DoAFlip(flipDirection, rotateDirection));
        StartCoroutine(SetIsJumping());

        skateInput.ResetNoseOffset();
        skateInput.ResetTailOffset();

        _onJump.Invoke();

        audioManager.Play("Jump");
    }

    void OnIncline(bool isManual)
    {
        var direction = isManual ? Vector3.forward : -Vector3.forward;
        direction = _isSwitch ? -direction : direction;
        var currentMaxAngle = isManual ? maxAngle - transform.right.y : transform.right.y + maxAngle;

        if (currentMaxAngle > 0)
        {
            _rb.angularVelocity += direction * 50 * Time.deltaTime;
        }
        else if (currentMaxAngle > (maxAngle + 0.1f))
        {
            _rb.angularVelocity -= direction * 50 * Time.deltaTime;
        }
        else
        {
            var angVel = _rb.angularVelocity;
            _rb.angularVelocity = new Vector3(angVel.x, angVel.y, 0);
        }

        if (!_isOnGrind)
        {
            PlayOnly("onGround", 1.8f);
        }
    }

    private void SetCameraMovement()
    {
        //MAIN CAMERA
        var camPos = _MainCamera.transform.position;
        var pos = transform.position;

        _MainCamera.transform.position = Vector3.Lerp(
            camPos,
            new Vector3(pos.x + _cameraPos.x, Mathf.Lerp(camPos.y, levelGenerator.GetHeightTerrain() + _cameraPos.y, 0.01f), pos.z + _cameraPos.z),
            1f
        );
    }

    private void GetGroundInformations()
    {
        RaycastHit hitBack;
        RaycastHit hitFront;
        Vector3 pos = transform.position;

        Physics.Raycast(pos - Vector3.right, Vector3.down, out hitBack, Mathf.Infinity);
        Physics.Raycast(pos + Vector3.right, Vector3.down, out hitFront, Mathf.Infinity);

        _hitDistance = hitFront.distance;
        _isGrindUnder = hitFront.collider != null ? hitFront.collider.gameObject.tag == "Grind" : false;
        _isOnWheel = hitFront.distance < 1f || hitBack.distance < 1f;

    }

    private void GoToAnchors(Vector3 anchor)
    {
        _rb.angularVelocity = Vector3.zero;
        transform.localEulerAngles = Vector3.Lerp(transform.localEulerAngles, anchor, 0.1f);
    }

    private void AddAndResetTrickScore()
    {
        if (_trickScore > 5) _totalScore += _trickScore;
        _trickScore = 0;
    }

    private bool GetIsInclined()
    {
        if (_isOnWheel)
        {
            return (_isNoseTouch || _isTailTouch) && !(_isTailTouch && _isNoseTouch); //XOR
        }

        return false;
    }

    public float GetFlipFirection(bool isManual, float onManualDirection, float onNoseDirection)
    {
        var offset = isManual ? onManualDirection : onNoseDirection;
        return Mathf.Abs(offset) < _minSensitivity ? 0 : Mathf.Sign(offset);
    }

    private void PlayOnly(string trackName, float pitch = 1.0f)
    {
        string[] staticTracksNames = new string[] { "onSlide", "onGround", "WheelsOnAir" };

        foreach (var staticTrackName in staticTracksNames)
        {
            if (staticTrackName == trackName)
            {
                audioManager.Play(staticTrackName, pitch);
            }
            else
            {
                audioManager.Stop(staticTrackName);
            }
        }
    }

    //COROUTINES
    private IEnumerator DoAFlip(float flipDirection, float rotateDirection)
    {
        var rot = transform.rotation;

        for (float a = 0; a < 1; a += Time.deltaTime * 0.1f)
        {
            _rotateAmount += rotateForce * rotateDirection;
            _flipAmount += flipForce * flipDirection;
            transform.Rotate(new Vector3(0, rotateForce * rotateDirection, 0), Space.Self);
            transform.Rotate(new Vector3(flipForce * flipDirection, 0, 0), Space.Self);

            _trickScore = (int)(Mathf.Abs(_flipAmount) / flipForce + Mathf.Abs(_rotateAmount) / rotateForce);

            yield return null;
        }
    }

    private IEnumerator SetIsJumping()
    {
        yield return new WaitForSeconds(0.1f);
        _isJumping = true;
    }

    private IEnumerator IncreaseDifficulty()
    {

        for (double a = 0; a < 1; a += Time.deltaTime / 2000000)
        {
            Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, endFOV, (float)a);
            _maxSpeed = Mathf.Lerp(_maxSpeed, maxEndSpeed, (float)a);
            jumpForce = Mathf.Lerp(jumpForce, jumpEndForce, (float)a);
            _cameraPos = Vector3.Lerp(_cameraPos, cameraEndPos, (float)a);
            yield return null;
        }
    }

}
