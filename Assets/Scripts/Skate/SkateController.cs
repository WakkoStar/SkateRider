using UnityEngine;
using UnityEngine.Events;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class SkatePhysicsController : MonoBehaviour
{
    //SETTINGS
    public float speedForce = 4000f;
    public float flipForce = 6;
    public float rotateForce = 6;
    public float maxAngle = 0.33f;
    public float maxSkateHeight = 10f;
    public SkateStateManager skateState;

    //STATE
    private Vector3 _TE;
    private Rigidbody _rb;
    private Coroutine _flipCoroutine;
    private float _rotateAmount = 0;
    private float _flipAmount = 0;
    private float _maxSpeed = 9f;
    private bool _forceMaxSpeed = true;
    private bool _isStopped = false;
    private float _jumpForce = 400f;
    private float _minSensitivity = 100;
    private Coroutine _stopForceMaxSpeed;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        //VALUES
        Physics.gravity = skateState.IsPushLanding() && !skateState.IsInFrontView() ? Vector3.down * 60 : Vector3.down * 20;

        _TE = transform.localEulerAngles;

        //SET DIRECTION
        if (GetForceMaxSpeed() && !IsStopped())
        {
            var direction = _rb.velocity.x < GetMaxSpeed() ? Vector3.right : -Vector3.right;
            _rb.AddForce(direction * speedForce, ForceMode.Acceleration);
        }

        //JUMP HEIGHT
        if (skateState.GetSkateTerrainReader().GetTerrainDistance() > maxSkateHeight)
        {
            if (_rb.velocity.y > 20) _rb.AddForce(-Vector3.up * 400, ForceMode.Acceleration);
        }

        SetIsStopped(skateState.GetSkateTerrainReader().IsOnTerrain() && !IsMoving());
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
        var relativeEulerAngleZ = skateState.GetSkateRotationReader().IsSwitch() ? -AngleHelper.FormatAngle(_TE.z) : _TE.z;
        var groundAngleOffset = relativeEulerAngleZ - Vector3.SignedAngle(Vector3.up, skateState.GetSkateTerrainReader().GetTerrainInclination(), Vector3.forward);
        groundAngleOffset = AngleHelper.FormatAngle(groundAngleOffset);

        if (skateState.GetIsNoseTouch() && skateState.GetIsTailTouch())
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

        if (IsStopped() && skateState.IsJumping())
        {
            _rb.velocity += Vector3.right;
            SetIsStopped(false);
        }

        if (skateState.GetIsNoseTouch() && skateState.GetIsTailTouch())
        {
            //STOP TRICK
            if (_flipCoroutine != null) StopCoroutine(_flipCoroutine);

            //HANDLE SKATE INCLINAISON
            if (skateState.GetSkateRotationReader().IsUpsideDown())
            {
                currentTE.z = Anchors.GetNearestAnchor(currentTE.z);
            }
            else
            {
                currentTE.z = Vector3.SignedAngle(Vector3.up, skateState.GetSkateTerrainReader().GetTerrainInclination(), Vector3.forward);
                if (skateState.GetSkateRotationReader().IsSwitch()) currentTE.z *= -1;
            }

            _rb.angularVelocity = Vector3.zero;

            var baseEulerAngles = new Vector3(
                _TE.x,
                _TE.y,
                !skateState.GetSkateRotationReader().IsUpsideDown() ? AngleHelper.FormatAngle(_TE.z) : _TE.z
            );

            var targetEulerAngles = skateState.GetSkateTerrainReader().IsGrindOnTerrain()
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

        var flipDirection = GetFlipFirection(isManual, skateState.GetSkateInput().GetNoseOffset().y, skateState.GetSkateInput().GetTailOffset().y);
        var rotateDirection = GetFlipFirection(isManual, skateState.GetSkateInput().GetTailOffset().y, skateState.GetSkateInput().GetNoseOffset().y);

        _flipCoroutine = StartCoroutine(DoAFlip(flipDirection, rotateDirection));

        skateState.OnJumpWithVelocity.Invoke(_rb.velocity);
    }

    public void OnIncline(bool isManual)
    {
        var direction = isManual ? Vector3.forward : -Vector3.forward;
        direction = skateState.GetSkateRotationReader().IsSwitch() ? -direction : direction;
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
    }

    public void OnBoost(float boostValue = 1.75f)
    {
        if (_stopForceMaxSpeed != null) StopCoroutine(_stopForceMaxSpeed);
        ForceMaxSpeed(false);
        _rb.velocity = new Vector3(0, _rb.velocity.y, _rb.velocity.z);
        var skateBoostedSpeed = GetMaxSpeed() * boostValue * _rb.mass;

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

    public void OnNoTouch()
    {
        if (_rb.velocity.x > 0)
        {
            _rb.velocity = Vector3.Lerp(_rb.velocity, Vector3.zero, 0.03f);
        }
        ForceMaxSpeed(false);
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
            var rotateForceTotal = rotateForce * rotateDirection * Time.deltaTime * 60;
            var flipForceTotal = flipForce * flipDirection * Time.deltaTime * 60;

            IncrementRotatemount(rotateForceTotal);
            IncrementFlipAmount(flipForceTotal);
            _rb.AddTorque(transform.up * rotateForceTotal * 0.5f, ForceMode.VelocityChange);
            _rb.AddTorque(transform.right * flipForceTotal * 0.5f, ForceMode.VelocityChange);
            // transform.Rotate();
            // transform.Rotate(new Vector3(flipForceTotal, 0, 0), Space.Self);

            yield return null;
        }
    }
    private IEnumerator Decelerate()
    {
        Vector3 startVel = _rb.velocity;

        for (float a = 0; a < 1; a += Time.deltaTime * 2)
        {
            _rb.velocity = Vector3.Lerp(startVel, Vector3.zero, a);
            yield return null;
        }
    }

    public void SetRigidBodyConstraints(RigidbodyConstraints constraints)
    {
        _rb.constraints = constraints;
    }

    public void SetMaxSpeed(float value)
    {
        _maxSpeed = value;
    }
    public float GetMaxSpeed()
    {
        return _maxSpeed;
    }

    public float GetForwardingSpeed()
    {
        return Mathf.Abs(_rb.velocity.x);
    }



    public void SetJumpForce(float value)
    {
        _jumpForce = value;
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
        return flipForce;
    }

    public float GetRotateForce()
    {
        return rotateForce;
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
        _stopForceMaxSpeed = StartCoroutine(ForceMaxSpeedWithDelayCoroutine(delay));
    }
    public void StopSkate()
    {
        ForceMaxSpeed(false);
        StartCoroutine(Decelerate());
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


    public void SetInit()
    {
        _rb.isKinematic = true;
        _rb.detectCollisions = false;
    }

    public void SetGameOver()
    {
        _rb.isKinematic = true;
        _rb.detectCollisions = false;
    }

    public void SetStartGame()
    {
        ForceMaxSpeed(true);
        _rb.angularVelocity = Vector3.zero;
        _rb.velocity = Vector3.right * 10;
        _rb.isKinematic = false;
        _rb.detectCollisions = true;

        if (_flipCoroutine != null) StopCoroutine(_flipCoroutine);
    }

}
