using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraPositionController : MonoBehaviour
{
    //SETTINGS
    [SerializeField] private SkateStateManager skateStateManager;
    [SerializeField] private LevelTileMonitor levelTileMonitor;

    //STATE
    private TerrainTileGenerator _terrainTileGenerator;
    private SkatePhysicsController _skatePhysController;
    private Camera _MainCamera;
    private Vector3 _cameraOffset;
    private float _cameraX;
    private float _cameraZ;
    private bool _isLosing;
    private bool _isCameraStopped;


    IEnumerator Start()
    {
        yield return new WaitForFixedUpdate();

        _MainCamera = GetComponent<Camera>();
        _terrainTileGenerator = levelTileMonitor.GetTerrainTileGenerator();
        _skatePhysController = skateStateManager.GetSkatePhysicsController();

        _cameraX = skateStateManager.transform.position.x;
        _cameraZ = skateStateManager.transform.position.z;
    }


    public void Init()
    {
        SetIsCameraStopped(true);
    }
    public void StartGame()
    {
        SetIsCameraStopped(false);
    }
    public void OnGameOver()
    {
        SetIsCameraStopped(true);
    }
    public void RestartGame()
    {
        SetIsCameraStopped(false);

        _cameraX = skateStateManager.transform.position.x;
        _cameraZ = skateStateManager.transform.position.z;

        _MainCamera.transform.position = new Vector3(_cameraX, _terrainTileGenerator.GetTerrainHeight(), _cameraZ);
    }

    void Update()
    {
        if (IsCameraStopped()) return;

        if (_terrainTileGenerator == null)
        {
            return;
        }

        if (!IsOnScreenY(skateStateManager.gameObject))
        {
            skateStateManager.SetGameOver();
        }

        if (!_isLosing)
        {
            _cameraX = skateStateManager.transform.position.x;
        }

        if (!_skatePhysController.IsMoving())
        {
            _isLosing = true;
            _cameraX = Mathf.Lerp(_cameraX, _cameraX + Time.deltaTime * _skatePhysController.GetMaxSpeed() * 5, 0.1f * (Time.deltaTime * 60));

            if (!IsOnScreenX(skateStateManager.gameObject))
            {
                skateStateManager.SetGameOver();
            }
        }

        if (_skatePhysController.IsMoving() && _isLosing)
        {
            var playerCameraXDistance = Mathf.Abs(_cameraX - skateStateManager.transform.position.x);
            if (_cameraX < skateStateManager.transform.position.x)
            {
                _isLosing = false;
            }
        }

        _MainCamera.transform.position = SetCameraPosition(
            new Vector3(_cameraX, _terrainTileGenerator.GetTerrainHeight(), _cameraZ)
        );
    }



    private Vector3 SetCameraPosition(Vector3 targetPos)
    {
        var camPos = _MainCamera.transform.position;

        var newPos = new Vector3(
            targetPos.x + _cameraOffset.x,
            Mathf.Lerp(camPos.y, targetPos.y + _cameraOffset.y, 0.01f * (Time.deltaTime * 60)),
            targetPos.z + _cameraOffset.z
        );

        return newPos;
    }



    public void SetCameraOffset(Vector3 value)
    {
        _cameraOffset = value;
    }
    public void SetCameraFOV(float value)
    {
        Camera.main.fieldOfView = (float)value;
    }



    private bool IsOnScreenX(GameObject gameObject)
    {
        Vector3 screenPoint = _MainCamera.WorldToViewportPoint(gameObject.transform.position);
        bool onScreen = screenPoint.x >= -0.2 && screenPoint.x <= 1.2;

        return onScreen;
    }

    private bool IsOnScreenY(GameObject gameObject)
    {
        Vector3 screenPoint = _MainCamera.WorldToViewportPoint(gameObject.transform.position);
        bool onScreen = screenPoint.y >= -1 && screenPoint.y <= 2;

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



}
