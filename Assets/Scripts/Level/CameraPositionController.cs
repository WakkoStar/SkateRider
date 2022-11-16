using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraPositionController : MonoBehaviour
{
    [SerializeField] private SkateStateManager skateStateManager;
    [SerializeField] private LevelTileMonitor levelTileMonitor;
    [SerializeField] private DifficultyIncreaser difficultyIncreaser;

    [SerializeField] private Vector3 cameraStartOffset = new Vector3(4, 3, -15);
    [SerializeField] private Vector3 cameraEndOffset = new Vector3(4, 3, -15);

    [SerializeField] private float startFOV = 80;
    [SerializeField] private float endFOV = 80;


    private TerrainTileGenerator _terrainTileGenerator;
    private SkateController _skateController;
    private Camera _MainCamera;

    private Vector3 _cameraOffset;

    private float _cameraX;
    private float _cameraZ;

    private bool _isLosing;


    void Start()
    {
        difficultyIncreaser.AddIncreaser<Vector3>(cameraStartOffset, cameraEndOffset, SetCameraOffset);
        difficultyIncreaser.AddIncreaser<float>(startFOV, endFOV, SetCameraFOV);

        StartCoroutine(OnStart());
    }

    IEnumerator OnStart()
    {
        yield return new WaitForSeconds(0.1f);

        _MainCamera = GetComponent<Camera>();
        _terrainTileGenerator = levelTileMonitor.GetTerrainTileGenerator();
        _skateController = skateStateManager.GetSkateController();

        _cameraX = skateStateManager.transform.position.x;
        _cameraZ = skateStateManager.transform.position.z;
    }




    void Update()
    {
        if (skateStateManager.IsGameOver() || _terrainTileGenerator == null)
        {
            return;
        }

        if (!_isLosing)
        {
            _cameraX = skateStateManager.transform.position.x;
        }

        if (!_skateController.IsMoving())
        {
            _isLosing = true;
            _cameraX = Mathf.Lerp(_cameraX, _cameraX + Time.deltaTime * _skateController.GetMaxSpeed() * 5, 0.1f * (Time.deltaTime * 60));

            if (!IsOnScreen(skateStateManager.gameObject))
            {
                skateStateManager.SetGameOver();
            }
        }

        if (_skateController.IsMoving() && _isLosing)
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



    private void SetCameraOffset(object value)
    {
        _cameraOffset = (Vector3)value;
    }
    private void SetCameraFOV(object value)
    {
        Camera.main.fieldOfView = (float)value;
    }



    private bool IsOnScreen(GameObject gameObject)
    {
        Vector3 screenPoint = _MainCamera.WorldToViewportPoint(gameObject.transform.position);
        bool onScreen = screenPoint.x >= -0.2 && screenPoint.x <= 1.2;

        return onScreen;
    }

}
