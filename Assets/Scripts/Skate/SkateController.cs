using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class SkateController : MonoBehaviour
{
    //SETTINGS
    [SerializeField] private SkateInput skateInput;
    [SerializeField] private GUIController guiController;
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private float maxStartSpeed = 10f;
    [SerializeField] private float maxEndSpeed = 10f;
    [SerializeField] private float jumpStartForce = 350f;
    [SerializeField] private float jumpEndForce = 350f;
    [SerializeField] private float speedForce = 4000f;
    [SerializeField] private float flipForce = 6;
    [SerializeField] private float rotateForce = 6;
    [SerializeField] private float maxAngle = 0.33f;
    [SerializeField] private float maxSkateOffset = 10f;
    [SerializeField] private UnityEvent<Vector2> _onJump;

    //STATE
    private BoxCollider _groundDetection;
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
    private bool _forceMaxSpeed = true;
    private float _jumpForce = 400f;
    private int _trickScore;
    private float _totalScore;
    private float _minSensitivity = 100;
    private int _collectibleCount = 0;


    //VALUES
    RigidbodyConstraints _normalConstraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezePositionZ;
    RigidbodyConstraints _onGrindConstraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionZ;

    void Start()
    {
        _maxSpeed = maxStartSpeed;
        _jumpForce = jumpStartForce;

        if (_onJump == null)
            _onJump = new UnityEvent<Vector2>();

        _groundDetection = GetComponent<BoxCollider>();


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
        if (_forceMaxSpeed)
        {
            var direction = _rb.velocity.x < _maxSpeed ? Vector3.right : -Vector3.right;
            _rb.AddForce(direction * speedForce, ForceMode.Acceleration);
        }


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
        //SCORE
        guiController.DisplayScore(_totalScore += 1);
        guiController.DisplayJumpScore(_trickScore);

        //GET SWITCH
        _isSwitch = transform.localEulerAngles.y > 90 && transform.localEulerAngles.y < 270;
        var isUpsideDown = (Mathf.Abs(transform.localEulerAngles.x) > 90 && Mathf.Abs(transform.localEulerAngles.x) < 270) || (Mathf.Abs(transform.localEulerAngles.z) > 90 && Mathf.Abs(transform.localEulerAngles.z) < 270);
        if (isUpsideDown) _isSwitch = !_isSwitch;

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

        var relativeEulerAngleZ = _isSwitch ? -(tEulerAng.z > 180 ? tEulerAng.z - 360 : tEulerAng.z) : tEulerAng.z;

        var groundAngleOffset = relativeEulerAngleZ - Vector3.SignedAngle(Vector3.up, _groundInclinaison, Vector3.forward);
        groundAngleOffset = groundAngleOffset > 180 ? groundAngleOffset - 360 : groundAngleOffset;

        if (_isNoseTouch && _isTailTouch)
        {
            if (groundAngleOffset > 2)
            {
                _rb.angularVelocity -= Vector3.forward / 2;
            }
            else if (groundAngleOffset < -2)
            {
                _rb.angularVelocity += Vector3.forward / 2;
            }
            else
            {
                _rb.angularVelocity = Vector3.Lerp(_rb.angularVelocity, Vector3.zero, 0.05f);
            }
        }


        transform.localEulerAngles = Vector3.Lerp(
            tEulerAng,
            Anchors.GetGroundAnchors(transform.localEulerAngles),
            0.1f * Time.deltaTime * 60
        );

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
        var tEulerAng = transform.localEulerAngles;

        var isUpsideDown = (Mathf.Abs(tEulerAng.x) > 90 && Mathf.Abs(tEulerAng.x) < 270) || (Mathf.Abs(tEulerAng.z) > 90 && Mathf.Abs(tEulerAng.z) < 270);

        if (_isNoseTouch && _isTailTouch)
        {
            Physics.gravity = Vector3.down * 60;

            //STOP TRICK
            if (_flipCoroutine != null) StopCoroutine(_flipCoroutine);

            //HANDLE SKATE INCLINAISON
            if (isUpsideDown)
            {
                tEulerAng.z = Anchors.GetNearestAnchor(tEulerAng.z);
            }
            else
            {
                tEulerAng.z = Vector3.SignedAngle(Vector3.up, _groundInclinaison, Vector3.forward);
                if (_isSwitch) tEulerAng.z *= -1;
            }

            GoToAnchors(
                _isGrindUnder
                ? Anchors.GetGrindAnchors(tEulerAng)
                : Anchors.GetAirAnchors(tEulerAng),
                !isUpsideDown
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
        _rb.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);

        _rotateAmount = 0;
        _flipAmount = 0;
        _trickScore = 0;

        var flipDirection = GetFlipFirection(isManual, skateInput.GetNoseOffset().y, skateInput.GetTailOffset().y);
        var rotateDirection = GetFlipFirection(isManual, skateInput.GetTailOffset().y, skateInput.GetNoseOffset().y);

        _flipCoroutine = StartCoroutine(DoAFlip(flipDirection, rotateDirection));
        StartCoroutine(SetIsJumping());

        skateInput.ResetNoseOffset();
        skateInput.ResetTailOffset();

        _onJump.Invoke(_rb.velocity);

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

    public void AddCollectible()
    {
        _collectibleCount += 1;
        guiController.DisplayCollectibleScore(_collectibleCount);
    }

    private void GetGroundInformations()
    {
        RaycastHit hitBack;
        RaycastHit hitFront;
        Vector3 pos = transform.position;

        Physics.Raycast(pos - transform.right, Vector3.down, out hitBack, Mathf.Infinity);
        Physics.Raycast(pos + transform.right, Vector3.down, out hitFront, Mathf.Infinity);

        bool isHitFront = hitFront.collider != null;
        bool isHitBack = hitBack.collider != null;

        _hitDistance = hitFront.distance;
        _isGrindUnder = isHitFront ? hitFront.collider.gameObject.tag == "Grind" : false;
        _isOnWheel = isHitBack && isHitFront ? (hitFront.distance < 1f || hitBack.distance < 1f) : false;

        var groundObj = hitFront.collider != null ? hitFront.collider.gameObject : null;
        if (groundObj == null) return;
        _groundInclinaison = groundObj.transform.InverseTransformVector(hitFront.normal);

    }

    private void GoToAnchors(Vector3 anchor, bool formatEulerAngle = false)
    {
        _rb.angularVelocity = Vector3.zero;

        var tEulerAng = transform.localEulerAngles;
        if (formatEulerAngle)
        {
            tEulerAng.z = tEulerAng.z > 180 ? tEulerAng.z - 360 : tEulerAng.z;
        }
        transform.localEulerAngles = Vector3.Lerp(tEulerAng, anchor, 0.1f * Time.deltaTime * 60);
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
            var rotateForceTotal = rotateForce * rotateDirection * Time.deltaTime * 60;
            var flipForceTotal = flipForce * flipDirection * Time.deltaTime * 60;

            _rotateAmount += rotateForceTotal;
            _flipAmount += flipForceTotal;
            transform.Rotate(new Vector3(0, rotateForceTotal, 0), Space.Self);
            transform.Rotate(new Vector3(flipForceTotal, 0, 0), Space.Self);

            _trickScore = (int)(Mathf.Abs(_flipAmount) / flipForce + Mathf.Abs(_rotateAmount) / rotateForce);

            yield return null;
        }
    }

    public float GetMaxSpeed()
    {
        return _maxSpeed;
    }

    public void ForceMaxSpeed(bool forceMaxSpeed)
    {
        _forceMaxSpeed = forceMaxSpeed;
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
            _maxSpeed = Mathf.Lerp(_maxSpeed, maxEndSpeed, (float)a);
            _jumpForce = Mathf.Lerp(_jumpForce, jumpEndForce, (float)a);

            yield return null;
        }
    }

    public void ForceMaxSpeedWithDelay(float delay)
    {
        StartCoroutine(ForceMaxSpeedWithDelayCoroutine(delay));
    }

    IEnumerator ForceMaxSpeedWithDelayCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        _forceMaxSpeed = true;
    }

}
