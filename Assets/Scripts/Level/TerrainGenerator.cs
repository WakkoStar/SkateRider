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
    private int _landingTileIndex = -1;
    private List<List<GameObject>> _terrain = new List<List<GameObject>>();
    List<GameObject> _terrainLayers = new List<GameObject>();
    private float _heightOffset = 0;
    TileSelector _tileSelector;

    public UnityEvent<List<GameObject>> OnTileAdded = new UnityEvent<List<GameObject>>();
    public UnityEvent OnTilePassed = new UnityEvent();

    public void Init(GameObject Player, float baseHeightLevel, List<Tile> tiles, int tileAmount, int tileSize, float yScale, float zScale, GameObject DefaultTile)
    {
        this.Player = Player;
        this.baseHeightLevel = baseHeightLevel;
        this.tiles = tiles;

        this.tileAmount = tileAmount;
        this.tileSize = tileSize;

        this.yScale = yScale;
        this.zScale = zScale;

        this.DefaultTile = DefaultTile;
    }

    // Start is called before the first frame update
    void Start()
    {
        _tileSelector = new TileSelector(tiles, DefaultTile);

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
            OnTilePassed.Invoke();
            DeleteTerrainFirstTile(_terrain);
            if (!_forceStop) AddTileToTerrain();

            _tileIndex = currentTileIndex;
        }
    }

    public void AddTileToTerrain(GameObject forceTile = null)
    {
        var playerPos = new Vector3(_tileIndex * tileSize, (baseHeightLevel * tileSize * yScale / 2f), 0);
        var nextTileOffset = Vector3.right * tileSize * _terrain.Count + Vector3.up * _heightOffset * tileSize * yScale / 2;

        var choosenTile = forceTile ? forceTile : _tileSelector.ChooseTile(GetBaseTerrain(), _heightOffset, GetSafeZone());
        _heightOffset += GetHeightOffset(choosenTile.name);

        bool hasTileChild = choosenTile.transform.childCount != 0;
        var tileGroup = new List<GameObject>();
        for (int i = 0; i < (hasTileChild ? choosenTile.transform.childCount : 1); i++)
        {
            var tileComponent = hasTileChild ? choosenTile.transform.GetChild(i).gameObject : choosenTile;
            var currentLayer = GetLayer(_terrainLayers, tileComponent, transform);
            var tile = InstantiateTile(tileComponent, playerPos + nextTileOffset, new Vector2(yScale, zScale), currentLayer.transform, tileSize);
            tileGroup.Add(tile);
        }

        _terrain.Add(tileGroup);
        OnTileAdded.Invoke(tileGroup);

        MeshCombiner.CombineLayers(_terrainLayers);
        // foreach (var layer in _terrainLayers)
        // {
        //     var layerTiles = new List<GameObject>();
        //     for (int i = 0; i < layer.transform.childCount; i++)
        //     {
        //         var tileComponent = layer.transform.GetChild(i).gameObject;
        //         layerTiles.Add(tileComponent);
        //     }

        //     if (layerTiles.Count == 0) continue;

        //     List<CombineInstance> meshCombine = MeshCombiner.SetMeshCombine(layerTiles);
        //     MeshCombiner.ApplyMeshCombine(layer, meshCombine);
        // }
    }




    //Get height offset by string, (must contains "+0.5" or "-1.0")
    float GetHeightOffset(string stringToParse)
    {
        var match = Regex.Match(stringToParse, @"([-+]?[0-9]*\.?[0-9]+)");
        if (match.Success)
            return float.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture.NumberFormat);

        return 0;
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

    // static public void DeleteOldTile(List<GameObject> terrain)
    // {
    //     if (terrain.Count == 0) return;
    //     Destroy(terrain[0]);
    //     terrain.RemoveAt(0);
    // }

    public void DeleteTerrainFirstTile(List<List<GameObject>> terrain)
    {
        if (terrain.Count == 0) return;
        foreach (var tileComponent in terrain[0])
        {
            Destroy(tileComponent);
        }
        terrain.RemoveAt(0);
    }

    public GameObject GetLayer(List<GameObject> layers, GameObject InspectedItem, Transform transform)
    {
        var currentLayer = layers.Find(layer =>
                layer.GetComponent<MeshRenderer>().sharedMaterial.name == InspectedItem.GetComponent<MeshRenderer>().sharedMaterial.name
                && layer.GetComponent<MeshCollider>().sharedMaterial.name == InspectedItem.GetComponent<MeshCollider>().sharedMaterial.name
            );

        if (currentLayer == null)
        {
            currentLayer = AddNewLayer(layers, InspectedItem, transform);
        }

        return currentLayer;
    }

    private GameObject AddNewLayer(List<GameObject> layers, GameObject BaseItem, Transform transform)
    {
        var layer = new GameObject("Layer " + layers.Count);
        layer.transform.parent = transform;

        layer.AddComponent<MeshFilter>().name = "Layer " + layers.Count;
        layer.AddComponent<MeshRenderer>().sharedMaterial = BaseItem.GetComponent<MeshRenderer>().sharedMaterial;
        layer.AddComponent<MeshCollider>().sharedMaterial = BaseItem.GetComponent<MeshCollider>().sharedMaterial;

        layers.Add(layer);

        return layer;
    }

    public float GetTerrainHeight()
    {
        return _heightOffset * tileSize * yScale / 2;
    }

    public List<GameObject> GetBaseTerrain()
    {
        return _terrain.Select(tileGroup => tileGroup[0]).ToList();
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
}
