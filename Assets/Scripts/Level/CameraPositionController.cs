using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraPositionController : MonoBehaviour
{
    //SETTINGS
    [SerializeField] private SkateStateManager skateStateManager;
    [SerializeField] private LevelTileMonitor levelTileMonitor;

    [SerializeField] private Vector3 frontCameraOffset;
    [SerializeField] private Vector3 sideCameraOffset;
    //STATE
    private TerrainTileGenerator _terrainTileGenerator;
    private SkatePhysicsController _skatePhysController;
    private Camera _MainCamera;

    private float _cameraX;
    private float _cameraZ;
    private bool _isLosing;
    private bool _isCameraStopped;
    private bool _isFrontMode = false;
    private bool _isChangingMode = false;
    private float _oldCameraHeight, _oldTargetHeight;


    IEnumerator Start()
    {
        yield return new WaitForFixedUpdate();

        _MainCamera = GetComponent<Camera>();
        sideCameraOffset = transform.position;

        _terrainTileGenerator = levelTileMonitor.GetTerrainTileGenerator();
        _skatePhysController = skateStateManager.GetSkatePhysicsController();

        _cameraX = skateStateManager.transform.position.x;
        _cameraZ = skateStateManager.transform.position.z;
    }


    public void Init()
    {
        SetIsCameraStopped(true);
    }
    public void OnGameOver()
    {
        SetIsCameraStopped(true);
    }

    public void StartGame()
    {
        SetIsFrontMode(false, true);
        SetIsChangingMode(false);

        _cameraX = skateStateManager.transform.position.x;
        _cameraZ = skateStateManager.transform.position.z;

        _MainCamera.transform.forward = Vector3.forward;
        _MainCamera.transform.position = new Vector3(
            _cameraX + sideCameraOffset.x,
            sideCameraOffset.y,
            _cameraZ + sideCameraOffset.z);

        _oldTargetHeight = sideCameraOffset.y;
        _oldCameraHeight = sideCameraOffset.y;

        SetIsCameraStopped(false);
        SetIsLosing(false);
    }

    void Update()
    {
        if (IsCameraStopped() || IsChangingMode()) return;

        if (!IsOnScreen(skateStateManager.gameObject))
        {
            skateStateManager.SetGameOver();
        }

        if (IsFrontMode())
        {
            FollowSkateOnFrontView();
        }
        else
        {
            FollowSkateOnSideView();
        }


    }

    private void FollowSkateOnSideView()
    {
        _MainCamera.transform.forward = Vector3.forward;

        if (!IsLosing())
        {
            _cameraX = skateStateManager.transform.position.x;

            _MainCamera.transform.position = SetCameraPosition(
                new Vector3(_cameraX, _terrainTileGenerator.GetTerrainHeight(), _cameraZ)
            );

            if (!_skatePhysController.IsMoving()) SetIsLosing(true);
        }
        else
        {
            if (!_skatePhysController.IsMoving())
            {
                _cameraX += Time.deltaTime * _skatePhysController.GetMaxSpeed() / 2;

                _MainCamera.transform.position = SetCameraPosition(
                new Vector3(
                    _cameraX,
                    _terrainTileGenerator.GetTerrainHeight(),
                    _cameraZ
                    )
                );
            }
            else
            {
                if (_cameraX < skateStateManager.transform.position.x)
                {
                    _isLosing = false;
                }
            }
        }

        _oldCameraHeight = _MainCamera.transform.position.y;
        _oldTargetHeight = _terrainTileGenerator.GetTerrainHeight();
    }

    private void FollowSkateOnFrontView()
    {
        _MainCamera.transform.position = skateStateManager.transform.position - frontCameraOffset;
        _MainCamera.transform.forward = (skateStateManager.transform.position - _MainCamera.transform.position);
    }


    private Vector3 SetCameraPosition(Vector3 targetPos)
    {
        var camPos = _MainCamera.transform.position;

        var newPos = new Vector3(
            targetPos.x + sideCameraOffset.x,
            SmoothLerp(_oldCameraHeight, _oldTargetHeight, targetPos.y + sideCameraOffset.y, 1),
            targetPos.z + sideCameraOffset.z
        );

        return newPos;
    }



    private float SmoothLerp(float oldValue, float oldTargetValue, float targetValue, float speed)
    {
        float t = Time.fixedDeltaTime * speed;
        float v = (targetValue - oldTargetValue) / t;
        float f = oldValue - oldTargetValue + v;
        return targetValue - v + f * Mathf.Exp(-t);
    }



    public void SetCameraOffset(Vector3 value)
    {
        sideCameraOffset = value;
    }
    public void SetCameraFOV(float value)
    {
        Camera.main.fieldOfView = (float)value;
    }



    private bool IsOnScreen(GameObject gameObject)
    {
        Vector3 screenPoint = _MainCamera.WorldToViewportPoint(gameObject.transform.position);
        bool onScreen = screenPoint.y >= -0.1 && screenPoint.y <= 30 && screenPoint.x >= -0.2 && screenPoint.x <= 1.2;

        return onScreen;
    }



    public void SetIsCameraStopped(bool value)
    {
        _isCameraStopped = value;
    }

    public bool IsCameraStopped()
    {
        return _isCameraStopped;
    }



    private bool IsLosing()
    {
        return _isLosing;
    }

    private void SetIsLosing(bool value)
    {
        _isLosing = value;
    }



    public void SetIsFrontMode(bool value)
    {
        StartCoroutine(ChangeCameraMode(value));
        _isFrontMode = value;
    }
    public void SetIsFrontMode(bool value, bool shouldSkipChange)
    {
        if (!shouldSkipChange) StartCoroutine(ChangeCameraMode(value));
        _isFrontMode = value;
    }

    private bool IsFrontMode()
    {
        return _isFrontMode;
    }

    private IEnumerator ChangeCameraMode(bool shouldBeFrontMode)
    {
        SetIsChangingMode(true);

        var startCamPos = _MainCamera.transform.position;
        var startCamForward = _MainCamera.transform.forward;

        for (float a = 0; a < 1; a += Time.fixedDeltaTime / 0.5f)
        {
            //TARGET FOR SIDE MODE
            var targetSideCameraPos = new Vector3(
                skateStateManager.transform.position.x + sideCameraOffset.x,
                _terrainTileGenerator.GetTerrainHeight() + sideCameraOffset.y,
                skateStateManager.transform.position.z + sideCameraOffset.z
            );
            var targetSideCameraForward = Vector3.forward;

            //TARGET FOR FRONT MODE
            var targetFrontCameraPos = skateStateManager.transform.position - frontCameraOffset;
            var targetFrontCameraForward = skateStateManager.transform.position - _MainCamera.transform.position;

            //TARGET
            var targetCameraPos = shouldBeFrontMode ? targetFrontCameraPos : targetSideCameraPos;
            var targetCameraForward = shouldBeFrontMode ? targetFrontCameraForward : targetSideCameraForward;

            _MainCamera.transform.position = Vector3.Lerp(
                startCamPos,
                targetCameraPos,
                a
            );

            _MainCamera.transform.forward = Vector3.Lerp(
                startCamForward,
                targetCameraForward,
                a
            );

            _oldTargetHeight = targetCameraPos.y;
            _oldCameraHeight = targetCameraPos.y;

            yield return null;
        }

        SetIsChangingMode(false);
    }



    private void SetIsChangingMode(bool value)
    {
        _isChangingMode = value;
    }

    private bool IsChangingMode()
    {
        return _isChangingMode;
    }



}
