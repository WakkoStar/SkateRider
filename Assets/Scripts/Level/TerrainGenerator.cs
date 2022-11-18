using System.Linq;
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


    //STATE
    private bool _shouldBeInSafeZone;
    private bool _forceStop;
    private int _tileIndex = -1;
    private List<GameObject> _terrain = new List<GameObject>();
    private Dictionary<string, List<GameObject>> _tilesInMeshCombiner = new Dictionary<string, List<GameObject>>();

    private GameObject _meshCombiner;
    private float _heightOffset = 0;
    private int _startTerrainIndex = 5;
    TileSelector _tileSelector;

    public UnityEvent<GameObject> OnTileAdded = new UnityEvent<GameObject>();
    public UnityEvent OnTilePassed = new UnityEvent();

    // Start is called before the first frame update
    void Start()
    {
        _tileSelector = new TileSelector(tiles, DefaultTile);

        _meshCombiner = new GameObject("Mesh Combiner");
        _meshCombiner.transform.parent = transform;

        for (int i = 0; i < tileAmount - 1; i++)
        {
            AddTileToTerrain(DefaultTile);
        }
    }

    // Update is called once per frame
    void Update()
    {
        int currentTileIndex = (int)(Player.transform.position.x / tileSize);
        if (currentTileIndex != _tileIndex)
        {
            DeleteTileMeshCombiner(_terrain, _tilesInMeshCombiner);
            DeleteTerrainFirstTile(_terrain);
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

        var choosenTile = forceTile != null ? forceTile : _tileSelector.ChooseTile(GetTerrain(), _heightOffset, GetSafeZone());
        _heightOffset += GetHeightOffset(choosenTile.name);

        var instanceTile = InstantiateTile(choosenTile, playerPos + nextTileOffset, new Vector2(yScale, zScale), transform, tileSize);
        _terrain.Add(instanceTile);

        var tileComponents = instanceTile.GetComponent<TileMeshCombinerInfo>().tileComponents;
        foreach (var tileComponent in tileComponents)
        {
            if (tileComponent.shouldBeInMeshCombiner)
            {
                UpdateLayer(_tilesInMeshCombiner, tileComponent.Tile, _meshCombiner.transform);
            }
        }

        MeshCombiner.CombineLayers(_tilesInMeshCombiner, _meshCombiner.transform);
        OnTileAdded.Invoke(instanceTile);
    }

    //Get height offset by string, (must contains "+0.5" or "-1.0")
    public float GetHeightOffset(string stringToParse)
    {
        var match = Regex.Match(stringToParse, @"([-+]?[0-9]*\.?[0-9]+)");
        if (match.Success)
            return float.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture.NumberFormat);

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

    public GameObject InstantiateTile(GameObject tile, Vector3 pos, Vector2 scale, Transform parent, float tileSize)
    {
        var instanceTile = Instantiate(tile, pos, Quaternion.identity, parent);
        instanceTile.transform.localScale = new Vector3(
            tile.transform.localScale.x,
            tile.transform.localScale.y * scale.x,
            tile.transform.localScale.z * scale.y
        ) * tileSize;

        instanceTile.name = tile.name;

        return instanceTile;
    }

    public void DeleteTerrainFirstTile(List<GameObject> terrain)
    {
        if (terrain.Count == 0) return;
        Destroy(terrain[0]);
        terrain.RemoveAt(0);
    }

    public void DeleteTileMeshCombiner(List<GameObject> terrain, Dictionary<string, List<GameObject>> meshCombiner)
    {
        if (terrain.Count == 0 || meshCombiner.Count == 0) return;

        foreach (var layer in meshCombiner)
        {
            layer.Value.RemoveAll(t => t.transform.position == terrain[0].transform.position);
        }
    }

    public List<GameObject> UpdateLayer(Dictionary<string, List<GameObject>> meshCombiner, GameObject InspectedItem, Transform meshCombinerTransform)
    {
        List<GameObject> currentLayerComponents = null;

        foreach (var layer in meshCombiner)
        {
            var isLayerExist = layer.Value.Find(tileComponent =>
                tileComponent.GetComponent<MeshCollider>().sharedMaterial.name == InspectedItem.GetComponent<MeshCollider>().sharedMaterial.name
            ) != null;

            if (isLayerExist)
            {
                currentLayerComponents = meshCombiner[layer.Key];
            }
        }

        if (currentLayerComponents == null)
        {
            currentLayerComponents = AddNewLayer(meshCombiner, InspectedItem, meshCombinerTransform);
        }

        currentLayerComponents.Add(InspectedItem);

        return currentLayerComponents;
    }

    private List<GameObject> AddNewLayer(Dictionary<string, List<GameObject>> meshCombiner, GameObject BaseItem, Transform meshCombinerTransform)
    {
        var layer = new GameObject("Layer " + meshCombiner.Count);
        layer.transform.parent = meshCombinerTransform;

        layer.AddComponent<MeshFilter>().name = "Layer " + meshCombiner.Count;
        layer.AddComponent<MeshCollider>().sharedMaterial = BaseItem.GetComponent<MeshCollider>().sharedMaterial;
        meshCombiner.Add(layer.name, new List<GameObject>());

        return meshCombiner[layer.name];
    }

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

    public void SetSafeZone(float safePoint, int zoneLenght)
    {
        _shouldBeInSafeZone = Mathf.Abs(safePoint - _tileIndex) < zoneLenght;
    }

    private bool GetSafeZone()
    {
        return _shouldBeInSafeZone;
    }

    public void RestartGame()
    {
        _heightOffset = 0;
        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).gameObject.name == "Mesh Combiner") continue;
            Destroy(transform.GetChild(i).gameObject);
        }

        for (int i = 0; i < _meshCombiner.transform.childCount; i++)
        {
            Destroy(_meshCombiner.transform.GetChild(i).gameObject);
        }

        _tileIndex = -1;

        _terrain = new List<GameObject>();
        _tilesInMeshCombiner = new Dictionary<string, List<GameObject>>();

        for (int i = 0; i < tileAmount - 1; i++)
        {
            AddTileToTerrain(DefaultTile);
        }
    }
}
