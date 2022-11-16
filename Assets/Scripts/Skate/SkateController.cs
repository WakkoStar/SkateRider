using UnityEngine;
using UnityEngine.Events;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class SkateController : MonoBehaviour
{
    //STATE
    private float _speedForce = 4000f;
    private float _flipForce = 6;
    private float _rotateForce = 6;
    private float _maxAngle = 0.33f;
    private float _maxSkateHeight = 10f;
    private Vector3 _TE;
    private SkateTerrainReader _skateTerrainReader;
    private SkateRotationReader _skateRotationReader;
    private SkateStateManager _skateState;
    private SkateInput _skateInput;
    private Rigidbody _rb;
    private Coroutine _flipCoroutine;
    private float _rotateAmount = 0;
    private float _flipAmount = 0;
    private float _maxSpeed = 9f;
    private bool _forceMaxSpeed = true;
    private bool _isStopped = false;
    private float _jumpForce = 400f;
    private float _minSensitivity = 100;

    //VALUES
    RigidbodyConstraints _normalConstraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezePositionZ;
    RigidbodyConstraints _onGrindConstraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionZ;

    public void Init(
        float speedForce,
        float flipForce,
        float rotateForce,
        float maxAngle,
        float maxSkateHeight,
        SkateTerrainReader skateTerrainReader,
        SkateRotationReader skateRotationReader,
        SkateInput skateInput,
        SkateStateManager skateStateManager
    )
    {
        _speedForce = speedForce;
        _flipForce = flipForce;
        _rotateForce = rotateForce;
        _maxAngle = maxAngle;
        _maxSkateHeight = maxSkateHeight;
        _skateTerrainReader = skateTerrainReader;
        _skateRotationReader = skateRotationReader;
        _skateInput = skateInput;
        _skateState = skateStateManager;
    }

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.velocity = Vector3.right;

        Application.targetFrameRate = 60;
    }

    void FixedUpdate()
    {
        //VALUES
        Physics.gravity = _skateState.IsPushLanding() ? Vector3.down * 60 : Vector3.down * 20;
        _rb.constraints = _skateState.IsOnGrind() ? _onGrindConstraints : _normalConstraints;

        _TE = transform.localEulerAngles;

        //SET DIRECTION
        if (GetForceMaxSpeed() && !IsStopped())
        {
            var direction = _rb.velocity.x < GetMaxSpeed() ? Vector3.right : -Vector3.right;
            _rb.AddForce(direction * _speedForce, ForceMode.Acceleration);
        }

        //JUMP HEIGHT
        if (_skateTerrainReader.GetTerrainDistance() > _maxSkateHeight)
        {
            if (_rb.velocity.y > 20) _rb.AddForce(-Vector3.up * 400, ForceMode.Acceleration);
        }

        SetIsStopped(_skateTerrainReader.IsOnTerrain() && !IsMoving());
    }

    public void OnGrind()
    {
        _rb.angularVelocity = Vector3.zero;

        transform.localEulerAngles = Vector3.Lerp(
            _TE, Anchors.GetGrindAnchors(_TE),
            0.1f * Time.deltaTime * 60
        );
    }

    public void OnGround()
    {
        var relativeEulerAngleZ = _skateRotationReader.IsSwitch() ? -AngleHelper.FormatAngle(_TE.z) : _TE.z;
        var groundAngleOffset = relativeEulerAngleZ - Vector3.SignedAngle(Vector3.up, _skateTerrainReader.GetTerrainInclination(), Vector3.forward);
        groundAngleOffset = AngleHelper.FormatAngle(groundAngleOffset);

        if (_skateState.GetIsNoseTouch() && _skateState.GetIsTailTouch())
        {
            if (groundAngleOffset > 2)
            {
                _rb.angularVelocity -= Vector3.forward / 3;
            }
            else if (groundAngleOffset < -2)
            {
                _rb.angularVelocity += Vector3.forward / 3;
            }
            else
            {
                _rb.angularVelocity = Vector3.Lerp(_rb.angularVelocity, Vector3.zero, 0.1f);
            }
        }


        transform.localEulerAngles = Vector3.Lerp(
            transform.localEulerAngles,
            Anchors.GetGroundAnchors(transform.localEulerAngles),
            0.1f * Time.deltaTime * 60
        );
    }

    public void OnLanding()
    {
        //STOP TRICK
        if (_flipCoroutine != null) StopCoroutine(_flipCoroutine);

        _rotateAmount = 0;
        _flipAmount = 0;
    }

    public void OnAir()
    {
        var currentTE = _TE;

        if (IsStopped() && _skateState.IsJumping())
        {
            _rb.velocity += Vector3.right;
            SetIsStopped(false);
        }

        if (_skateState.GetIsNoseTouch() && _skateState.GetIsTailTouch())
        {
            //STOP TRICK
            if (_flipCoroutine != null) StopCoroutine(_flipCoroutine);

            //HANDLE SKATE INCLINAISON
            if (_skateRotationReader.IsUpsideDown())
            {
                currentTE.z = Anchors.GetNearestAnchor(currentTE.z);
            }
            else
            {
                currentTE.z = Vector3.SignedAngle(Vector3.up, _skateTerrainReader.GetTerrainInclination(), Vector3.forward);
                if (_skateRotationReader.IsSwitch()) currentTE.z *= -1;
            }

            _rb.angularVelocity = Vector3.zero;

            var baseEulerAngles = new Vector3(
                _TE.x,
                _TE.y,
                !_skateRotationReader.IsUpsideDown() ? AngleHelper.FormatAngle(_TE.z) : _TE.z
            );

            var targetEulerAngles = _skateTerrainReader.IsGrindOnTerrain()
                ? Anchors.GetGrindAnchors(currentTE)
                : Anchors.GetAirAnchors(currentTE);

            transform.localEulerAngles = Vector3.Lerp(
                baseEulerAngles, targetEulerAngles
                , 0.1f * Time.deltaTime * 60
            );

        }
    }

    public void OnJump(bool isManual)
    {
        _rb.angularVelocity = Vector3.zero;
        _rb.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);

        _rotateAmount = 0;
        _flipAmount = 0;

        var flipDirection = GetFlipFirection(isManual, _skateInput.GetNoseOffset().y, _skateInput.GetTailOffset().y);
        var rotateDirection = GetFlipFirection(isManual, _skateInput.GetTailOffset().y, _skateInput.GetNoseOffset().y);

        _flipCoroutine = StartCoroutine(DoAFlip(flipDirection, rotateDirection));

        _skateState.OnJumpWithVelocity.Invoke(_rb.velocity);
    }

    public void OnIncline(bool isManual)
    {
        var direction = isManual ? Vector3.forward : -Vector3.forward;
        direction = _skateRotationReader.IsSwitch() ? -direction : direction;
        var currentMaxAngle = isManual ? _maxAngle - transform.right.y : transform.right.y + _maxAngle;

        if (currentMaxAngle > 0)
        {

            _rb.angularVelocity += direction * 50 * Time.deltaTime;
        }
        else if (currentMaxAngle > (_maxAngle + 0.1f))
        {
            _rb.angularVelocity -= direction * 50 * Time.deltaTime;
        }
        else
        {
            var angVel = _rb.angularVelocity;
            _rb.angularVelocity = new Vector3(angVel.x, angVel.y, 0);
        }
    }

    public void OnBoost()
    {
        ForceMaxSpeed(false);
        _rb.velocity = new Vector3(0, _rb.velocity.y, _rb.velocity.z);
        var skateBoostedSpeed = GetMaxSpeed() * 1.75f * _rb.mass;

        _rb.AddForce(Vector3.right * skateBoostedSpeed, ForceMode.Impulse);
    }

    public void StopBoost()
    {
        ForceMaxSpeedWithDelay(0.5f);
    }

    public float GetFlipFirection(bool isManual, float onManualDirection, float onNoseDirection)
    {
        var offset = isManual ? onManualDirection : onNoseDirection;
        return Mathf.Abs(offset) < _minSensitivity ? 0 : Mathf.Sign(offset);
    }

    public bool IsMoving()
    {
        if (_rb == null) return true;

        return Mathf.Abs(_rb.velocity.x) > 0.5f;
    }


    private IEnumerator DoAFlip(float flipDirection, float rotateDirection)
    {
        var rot = transform.rotation;


        for (float a = 0; a < 1; a += Time.deltaTime * 0.1f)
        {
            var rotateForceTotal = _rotateForce * rotateDirection * Time.deltaTime * 60;
            var flipForceTotal = _flipForce * flipDirection * Time.deltaTime * 60;

            IncrementRotatemount(rotateForceTotal);
            IncrementFlipAmount(flipForceTotal);
            transform.Rotate(new Vector3(0, rotateForceTotal, 0), Space.Self);
            transform.Rotate(new Vector3(flipForceTotal, 0, 0), Space.Self);

            // _trickScore = (int)(Mathf.Abs(_flipAmount) / flipForce + Mathf.Abs(_rotateAmount) / rotateForce);

            yield return null;
        }
    }

    public void SetMaxSpeed(float value)
    {
        _maxSpeed = value;
    }
    public void SetMaxSpeed(object value)
    {
        _maxSpeed = (float)value;
    }
    public float GetMaxSpeed()
    {
        return _maxSpeed;
    }



    public void SetJumpForce(float value)
    {
        _jumpForce = value;
    }
    public void SetJumpForce(object value)
    {
        _jumpForce = (float)value;
    }
    public float GetJumpForce()
    {
        return _jumpForce;
    }



    private void IncrementFlipAmount(float value)
    {
        _flipAmount += value;
    }
    public float GetFlipAmount()
    {
        return _flipAmount;
    }




    private void IncrementRotatemount(float value)
    {
        _rotateAmount += value;
    }
    public float GetRotateAmount()
    {
        return _rotateAmount;
    }



    public float GetFlipForce()
    {
        return _flipForce;
    }

    public float GetRotateForce()
    {
        return _rotateForce;
    }



    private bool GetForceMaxSpeed()
    {
        return _forceMaxSpeed;
    }
    public void ForceMaxSpeed(bool forceMaxSpeed)
    {
        _forceMaxSpeed = forceMaxSpeed;
    }
    public void ForceMaxSpeedWithDelay(float delay)
    {
        StartCoroutine(ForceMaxSpeedWithDelayCoroutine(delay));
    }



    public bool IsStopped()
    {
        return _isStopped;
    }

    public void SetIsStopped(bool value)
    {
        _isStopped = value;
    }


    IEnumerator ForceMaxSpeedWithDelayCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        _forceMaxSpeed = true;
    }

}
