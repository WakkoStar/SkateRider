using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Globalization;
using UnityEngine.Events;

public class TerrainTileGenerator : MonoBehaviour
{
    //SETTINGS
    public GameObject Player;
    public float baseHeightLevel = -1.47f;
    public List<Tile> tiles = new List<Tile>();
    public int tileAmount = 2;
    public int tileSize = 5;
    public float yScale = 5;
    public float zScale = 5;
    public GameObject DefaultTile;
    private ObjectPool _objectPool;


    //STATE
    private bool _shouldBeInSafeZone;
    private int _safeZoneCounter;
    private bool _forceStop;
    private int _tileIndex = 0;
    private List<GameObject> _terrain = new List<GameObject>();
    // private Dictionary<string, List<GameObject>> _tilesInMeshCombiner = new Dictionary<string, List<GameObject>>();

    private GameObject _meshCombiner;
    private float _heightOffset = 0;
    private int _startTerrainIndex = 5;
    private TileSelector _tileSelector;
    private bool _canIndexGoBack = false;

    public UnityEvent<GameObject> OnTileAdded = new UnityEvent<GameObject>();
    public UnityEvent OnTilePassed = new UnityEvent();

    // Start is called before the first frame update
    IEnumerator Start()
    {
        _tileSelector = new TileSelector(tiles, DefaultTile);

        _meshCombiner = new GameObject("Mesh Combiner");
        _meshCombiner.transform.parent = transform;

        _objectPool = CreateObjectPoolOnTerrain(tiles.Select(s => s.obj).ToList(), transform);

        yield return new WaitForFixedUpdate();
        for (int i = 0; i < tileAmount - 1; i++)
        {
            AddTileToTerrain(DefaultTile);
        }
    }

    // Update is called once per frame
    void Update()
    {
        int currentTileIndex = (int)(Player.transform.position.x / tileSize);
        var isTileIndexChanged = CanIndexGoBack() ? currentTileIndex != _tileIndex : currentTileIndex > _tileIndex;
        if (isTileIndexChanged)
        {
            // DeleteTileMeshCombiner(_terrain, _meshCombiner);
            DeleteTerrainFirstTile(_terrain, _objectPool);
            OnTilePassed.Invoke();
            if (!_forceStop) AddTileToTerrain(_tileIndex > _startTerrainIndex ? null : DefaultTile);

            _tileIndex = currentTileIndex;
        }
    }

    public void AddTileToTerrain(GameObject forceTile = null, int forceCurrentTileIndex = -1)
    {
        var newTileIndex = forceCurrentTileIndex != -1 ? forceCurrentTileIndex : _tileIndex;
        var playerPos = new Vector3(newTileIndex * tileSize, (baseHeightLevel * tileSize * yScale / 2f), 0);
        var nextTileOffset = Vector3.right * tileSize * _terrain.Count + Vector3.up * _heightOffset * tileSize * yScale / 2;

        SetSafeZoneCounter(ShouldBeInSafeZone());
        var choosenTile = forceTile != null ? forceTile : _tileSelector.ChooseTile(GetTerrain(), _heightOffset, GetSafeZone());
        _heightOffset += GetHeightOffset(choosenTile.name);

        var instanceTile = InstantiateTile(
            choosenTile, playerPos + nextTileOffset, new Vector2(yScale, zScale), transform, tileSize, _objectPool
        );

        MeshCombiner.CombineLayers(
            _meshCombiner.transform,
            _terrain.Select((tile) => tile.GetComponent<TileMeshCombinerInfo>().tileComponents)
        );

        _terrain.Add(instanceTile);
        OnTileAdded.Invoke(instanceTile);
    }

    //Get height offset by string, (must contains "+0.5" or "-1.0")
    public float GetHeightOffset(string stringToParse)
    {
        if (stringToParse.Contains("0.0")) return 0;
        if (stringToParse.Contains("+0.25")) return 0.25f;
        if (stringToParse.Contains("-0.25")) return -0.25f;
        if (stringToParse.Contains("+0.5")) return 0.5f;
        if (stringToParse.Contains("-0.5")) return -0.5f;
        if (stringToParse.Contains("+1.0")) return 1;
        if (stringToParse.Contains("-1.0")) return -1;
        if (stringToParse.Contains("+2.0")) return 2;
        if (stringToParse.Contains("-2.0")) return -2;
        if (stringToParse.Contains("-4.0")) return -4;

        Debug.LogWarning("Height offset not found on " + stringToParse);
        return 0;

    }

    public Vector3 GetScaledEulerAngles(Vector2 scale, Vector3 eulerAngles)
    {
        if (eulerAngles.z == 0) return eulerAngles;

        var scaledAngle = Mathf.Atan(scale.y / scale.x) * Mathf.Rad2Deg;
        var formatedEulerAngleZ = eulerAngles.z > 180 ? eulerAngles.z - 360 : eulerAngles.z;

        var scaledEulerAngles = new Vector3(0, 0, formatedEulerAngleZ * scaledAngle / formatedEulerAngleZ);

        return scaledEulerAngles;
    }

    public ObjectPool CreateObjectPoolOnTerrain(List<GameObject> tilesObjs, Transform baseTerrain)
    {
        var objectPoolObj = new GameObject("Object Pool");
        objectPoolObj.transform.parent = baseTerrain;
        var objectPool = objectPoolObj.AddComponent<ObjectPool>();

        for (int i = 0; i < tileAmount; i++)
        {
            foreach (var tileObj in tilesObjs)
            {
                objectPool.Create(tileObj, tileObj.name + "-" + i);
            }
        }

        return objectPool;
    }

    public GameObject InstantiateTile(GameObject tile, Vector3 pos, Vector2 scale, Transform parent, float tileSize, ObjectPool objectPool)
    {
        var instanceTile = objectPool.Instantiate(tile, pos, Quaternion.identity, parent);

        instanceTile.transform.localScale = new Vector3(
            tile.transform.localScale.x,
            tile.transform.localScale.y * scale.x,
            tile.transform.localScale.z * scale.y
        ) * tileSize;

        return instanceTile;
    }

    public void DeleteTerrainFirstTile(List<GameObject> terrain, ObjectPool objectPool)
    {
        if (terrain.Count == 0) return;
        objectPool.Destroy(terrain[0]);
        terrain.RemoveAt(0);
    }

    // public void DeleteTileMeshCombiner(List<GameObject> terrain, GameObject meshCombiner)
    // {
    //     // if (terrain.Count == 0 || meshCombiner.Count == 0) return;

    //     // foreach (var layer in meshCombiner)
    //     // {
    //     //     layer.Value.RemoveAll(t => t.transform.position == terrain[0].transform.position);
    //     // }
    // }

    // public List<GameObject> UpdateLayer(GameObject InspectedItem, Transform meshCombinerTransform)
    // {
    //     List<GameObject> currentLayerComponents = null;

    //     foreach (var layer in meshCombiner)
    //     {
    //         var isLayerExist = layer.Value.Find(tileComponent =>
    //             tileComponent.GetComponent<MeshCollider>().sharedMaterial.name == InspectedItem.GetComponent<MeshCollider>().sharedMaterial.name
    //         ) != null;

    //         if (isLayerExist)
    //         {
    //             currentLayerComponents = meshCombiner[layer.Key];
    //         }
    //     }

    //     if (currentLayerComponents == null)
    //     {
    //         currentLayerComponents = AddNewLayer(meshCombiner, InspectedItem, meshCombinerTransform);
    //     }

    //     currentLayerComponents.Add(InspectedItem);

    //     return currentLayerComponents;
    // }

    // private List<GameObject> AddNewLayer(Dictionary<string, List<GameObject>> meshCombiner, GameObject BaseItem, Transform meshCombinerTransform)
    // {
    //     var layer = new GameObject("Layer " + meshCombiner.Count);
    //     layer.transform.parent = meshCombinerTransform;

    //     layer.AddComponent<MeshFilter>().name = "Layer " + meshCombiner.Count;
    //     layer.AddComponent<MeshCollider>().sharedMaterial = BaseItem.GetComponent<MeshCollider>().sharedMaterial;
    //     meshCombiner.Add(layer.name, new List<GameObject>());

    //     return meshCombiner[layer.name];
    // }

    public float GetTerrainHeight()
    {
        return _heightOffset * tileSize * yScale / 2;
    }
    public float GetTerrainHeight(float heightAddition)
    {
        return heightAddition * tileSize * yScale / 2;
    }

    public List<GameObject> GetTerrain()
    {
        return _terrain;
    }

    public bool GetForceStop()
    {
        return _forceStop;
    }

    public void ForceStop(bool value)
    {
        _forceStop = value;
    }




    public bool CanIndexGoBack()
    {
        return _canIndexGoBack;
    }
    public void SetCanIndexGoBack(bool value)
    {
        _canIndexGoBack = value;
    }


    public void SetSafeZone(float safePoint, int zoneLenght)
    {
        _shouldBeInSafeZone = Mathf.Abs(safePoint - _tileIndex) < zoneLenght;
    }

    private bool ShouldBeInSafeZone()
    {
        return _shouldBeInSafeZone;
    }



    private int GetSafeZoneCounter()
    {
        return _safeZoneCounter;
    }

    private void SetSafeZoneCounter(bool isSafeZone)
    {
        _safeZoneCounter = isSafeZone ? _safeZoneCounter + 1 : 0;
    }


    private bool GetSafeZone()
    {
        if (GetSafeZoneCounter() >= 5) return false;
        return ShouldBeInSafeZone();
    }

    public void StartGame()
    {
        _heightOffset = 0;
        _tileIndex = 0;

        _terrain = new List<GameObject>();
        CleanGameObjectChilds(gameObject);

        _meshCombiner = new GameObject("Mesh Combiner");
        _meshCombiner.transform.parent = transform;

        _objectPool = CreateObjectPoolOnTerrain(tiles.Select(s => s.obj).ToList(), transform);

        for (int i = 0; i < tileAmount - 1; i++)
        {
            AddTileToTerrain(DefaultTile);
        }
    }

    public void CleanGameObjectChilds(GameObject gameObject)
    {
        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            Destroy(gameObject.transform.GetChild(i).gameObject);
        }
    }

    public int GetTileIndex()
    {
        return _tileIndex;
    }
    public int GetStartTerrainIndex()
    {
        return _startTerrainIndex;
    }
}
