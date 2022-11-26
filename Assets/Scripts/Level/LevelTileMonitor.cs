using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class LevelTileMonitor : MonoBehaviour
{
    //INNER STRUCTS
    [Serializable]
    public class IndexedEvent
    {
        public int index;
        public UnityEvent onIndexReached = new UnityEvent();
    }


    //STATE
    private GameObject _Terrain;
    private GameObject _SideTerrain;
    private GameObject _GrindTerrain;

    private int _switchingTileIndex = 10000;

    /*
      We need to recalculate currentTileIndex because tileSize is changing 
      between terrainTileGenerator update, when currentTileIndex isn't equal to _tileIndex
      and the invoke of OnTilePassed
      **/
    private int _forceCurrentTileIndex = -1;
    bool _isSwitchingSize;

    private int _tileSize;
    private float _currentileSize;
    private float _yScale;
    private float _zScale;

    private TerrainTileGenerator _terrainTileGenerator;
    private SideTerrainTileGenerator _sideTerrainTileGenerator;
    private GrindTileGenerator _grindTileGenerator;
    private GameObject _objectPoolObj;
    private ObjectPool _objectPool;

    private Vector3 _cameraOffset;
    private float _landingHit;


    //SETTINGS
    [SerializeField] private GameObject Player;
    [SerializeField] private float baseHeightLevel = -1.47f;

    [SerializeField] private List<Tile> tiles = new List<Tile>();
    [SerializeField] private List<SideTile> sideTiles = new List<SideTile>();
    [SerializeField] private List<SideTile> sideTilesBackward = new List<SideTile>();
    [SerializeField] private List<GameObject> switchTiles = new List<GameObject>();

    [SerializeField] private List<IndexedEvent> switchTileEvents = new List<IndexedEvent>();
    [SerializeField] private int tileAmount = 2;

    [SerializeField] private GameObject DefaultTile;
    // [SerializeField] private GameObject VoidTile;
    // [SerializeField] private GameObject StartSwitchTile;
    // [SerializeField] private GameObject EndSwitchTile;
    [SerializeField] private List<GameObject> GrindStarts = new List<GameObject>();
    [SerializeField] private List<GameObject> GrindEnds = new List<GameObject>();
    [SerializeField] private float zStartScale = 1;
    [SerializeField] private float zEndScale = 1;
    [SerializeField] private float yStartScale = 1.5f;
    [SerializeField] private float yEndScale = 1.5f;
    [SerializeField] private float tileSizeStart = 5;
    [SerializeField] private float tileSizeEnd = 10;
    // [SerializeField] private int switchTileTransitionLength = 4;

    private UnityAction<GameObject> onTileAddedAction;
    private UnityAction onTilePassedAction;

    void Start()
    {
        //ASSIGN INCREASERS
        _tileSize = (int)tileSizeStart;
        _currentileSize = (int)tileSizeStart;

        _yScale = yStartScale;
        _zScale = zStartScale;


        //CREATE TILE POOL 
        _objectPoolObj = new GameObject("Object Pool");
        _objectPoolObj.transform.parent = transform;
        _objectPool = _objectPoolObj.AddComponent<ObjectPool>();

        for (int i = 0; i < tileAmount * 3; i++)
        {
            foreach (var tile in GetAllTiles())
            {
                _objectPool.Create(tile.obj, tile.obj.name + "-" + i);
            }
        }

        //INIT LEVEL STRUCTURE
        _Terrain = InitLevelComponent("BaseTerrain");
        _terrainTileGenerator = _Terrain.AddComponent<TerrainTileGenerator>();

        _terrainTileGenerator.Player = Player;
        _terrainTileGenerator.baseHeightLevel = baseHeightLevel;
        _terrainTileGenerator.tiles = tiles;
        _terrainTileGenerator.tileAmount = tileAmount;
        _terrainTileGenerator.tileSize = (int)tileSizeStart;
        _terrainTileGenerator.yScale = yStartScale;
        _terrainTileGenerator.zScale = zStartScale;
        _terrainTileGenerator.DefaultTile = DefaultTile;
        _terrainTileGenerator.objectPool = _objectPool;


        _SideTerrain = InitLevelComponent("SideTerrain");
        _sideTerrainTileGenerator = _SideTerrain.AddComponent<SideTerrainTileGenerator>();

        _sideTerrainTileGenerator.terrainTileGenerator = _terrainTileGenerator;
        _sideTerrainTileGenerator.sideTiles = sideTiles;
        _sideTerrainTileGenerator.backwardSideTiles = sideTilesBackward;


        _GrindTerrain = InitLevelComponent("GrindTerrain");
        _grindTileGenerator = _GrindTerrain.AddComponent<GrindTileGenerator>();

        _grindTileGenerator.terrainTileGenerator = _terrainTileGenerator;
        _grindTileGenerator.GrindStarts = GrindStarts;
        _grindTileGenerator.GrindEnds = GrindEnds;


        //EVENTS
        onTileAddedAction += AddTileToOtherLevelComponents;
        _terrainTileGenerator.OnTileAdded.AddListener(onTileAddedAction);

        onTilePassedAction += ForceTerrainSwitch;
        _terrainTileGenerator.OnTilePassed.AddListener(onTilePassedAction);
    }

    public void StartGame()
    {
        _tileSize = (int)tileSizeStart;
        _currentileSize = (int)tileSizeStart;

        _yScale = yStartScale;
        _zScale = zStartScale;

        _terrainTileGenerator.yScale = _yScale;
        _terrainTileGenerator.zScale = _zScale;
        _terrainTileGenerator.tileSize = _tileSize;

        _sideTerrainTileGenerator.StartGame();
        _terrainTileGenerator.StartGame();
        _grindTileGenerator.StartGame();
    }

    void Update()
    {
        if ((int)Mathf.Round(_currentileSize) != _tileSize && !_isSwitchingSize)
        {
            _switchingTileIndex = 0;
            _isSwitchingSize = true;
        }

        _terrainTileGenerator.SetSafeZone((int)(_landingHit / _tileSize) - 2, 2);
    }

    void AddTileToOtherLevelComponents(GameObject instanceTile)
    {
        if (instanceTile.tag == "Grind")
        {
            _grindTileGenerator.AddTileToGrind(
                instanceTile.transform.GetChild(0).gameObject,
                instanceTile.transform.GetChild(1).gameObject
            );
        }

        _sideTerrainTileGenerator.AddTileToTerrain(instanceTile);
    }

    void ForceTerrainSwitch()
    {
        int endSwitchTileIndex = switchTiles.Count - 1;
        if (_switchingTileIndex < endSwitchTileIndex && _isSwitchingSize)
        {
            _terrainTileGenerator.ForceStop(true);
            SwitchTileTerrain(_switchingTileIndex);
            _switchingTileIndex += 1;
        }
        else
        {
            _isSwitchingSize = false;
            _terrainTileGenerator.ForceStop(false);
        }
    }

    private GameObject InitLevelComponent(string name)
    {
        var levelComponent = new GameObject(name);
        levelComponent.transform.parent = transform;

        return levelComponent;
    }

    public void SetLandingHit(Vector2 playerVelocity)
    {
        float vx = playerVelocity.x;
        float vy = playerVelocity.y;
        float g = Physics.gravity.magnitude;
        var currentPosition = Player.transform.position;
        float jumpCurrentHeight = Mathf.Infinity;
        int i = 2; //AVOID TO START TOO EARLY

        while (jumpCurrentHeight > _terrainTileGenerator.GetTerrainHeight())
        {

            jumpCurrentHeight = vy * i / vx - g * Mathf.Pow(i / vx, 2) / 2f;

            // var y0 = vy * (i - 1) / vx - g * Mathf.Pow((i - 1) / vx, 2) / 2f;
            // currentPosition = new Vector3(Player.transform.position.x + (i - 1), y0, 0);

            // Debug.DrawLine(currentPosition, new Vector3(Player.transform.position.x + i, jumpCurrentHeight, 0), Color.black, 2f);

            i++;
        }
        // Instantiate(GameObject.Find("cars.001"), new Vector3(Player.transform.position.x + i, _terrainTileGenerator.GetTerrainHeight(), 0), Quaternion.identity);
        _landingHit = Player.transform.position.x + i;
    }

    private void SwitchTileTerrain(int switchingTileIndex)
    {
        _forceCurrentTileIndex = -1;

        var existingEventOnIndex = switchTileEvents.Find(indexedEvent => indexedEvent.index == switchingTileIndex);
        if (existingEventOnIndex != null)
        {
            existingEventOnIndex.onIndexReached.Invoke();
        }

        _terrainTileGenerator.AddTileToTerrain(switchTiles[_switchingTileIndex], _forceCurrentTileIndex);
    }

    public void ChangeLevelScale()
    {
        var tileSizeGap = tileSizeEnd - tileSizeStart;
        _tileSize = (int)Mathf.Round(_currentileSize);
        _yScale += (yEndScale - yStartScale) / tileSizeGap;
        _zScale += (zEndScale - zStartScale) / tileSizeGap;

        _terrainTileGenerator.yScale = _yScale;
        _terrainTileGenerator.zScale = _zScale;
        _terrainTileGenerator.tileSize = _tileSize;

        _forceCurrentTileIndex = (int)Player.transform.position.x / _tileSize;
    }


    public void InitAllTiles()
    {
        InitTiles(tiles);
        InitSideTiles(sideTiles, tiles);
        InitSideTiles(sideTilesBackward, tiles);
    }

    private void InitTiles(List<Tile> tiles)
    {
        for (int i = 0; i < tiles.Count; i++)
        {
            var oldSelection = tiles[i].selection;
            tiles[i].selection = new TileSelection[tiles.Count];

            for (int j = 0; j < tiles.Count; j++)
            {
                if (oldSelection.Length > j)
                {
                    if (tiles[j].obj.name == oldSelection[j].name)
                    {
                        tiles[i].selection[j] = oldSelection[j];
                        continue;
                    }
                }
                tiles[i].selection[j] = new TileSelection(tiles[j].obj.name, false);
            }
        }
    }

    private void InitSideTiles(List<SideTile> sideTiles, List<Tile> tiles)
    {
        for (int i = 0; i < sideTiles.Count; i++)
        {
            //SELECTION
            var oldSelection = sideTiles[i].selection;
            sideTiles[i].selection = new TileSelection[tiles.Count];

            for (int j = 0; j < tiles.Count; j++)
            {
                if (oldSelection.Length > j)
                {
                    if (tiles[j].obj.name == oldSelection[j].name)
                    {
                        sideTiles[i].selection[j] = oldSelection[j];
                        continue;
                    }
                }
                sideTiles[i].selection[j] = new TileSelection(tiles[j].obj.name, false);
            }

            //SIDE SELECTION
            if (!sideTiles[i].shouldHaveSideSelection)
            {
                sideTiles[i].sideSelection = new TileSelection[0];
                continue;
            }

            var formattedTiles = tiles.Select((t) =>
            {
                var sT = new SideTile();
                sT.obj = t.obj;
                sT.selection = t.selection;
                return sT;
            });

            var allTiles = formattedTiles.Union<SideTile>(sideTiles).ToList();

            var oldSideSelection = sideTiles[i].sideSelection;
            sideTiles[i].sideSelection = new TileSelection[allTiles.Count];

            for (int j = 0; j < allTiles.Count; j++)
            {
                if (oldSideSelection.Length > j)
                {
                    if (allTiles[j].obj.name == oldSideSelection[j].name)
                    {
                        sideTiles[i].sideSelection[j] = oldSideSelection[j];
                        continue;
                    }
                }
                sideTiles[i].sideSelection[j] = new TileSelection(allTiles[j].obj.name, false);
            }
        }
    }


    public void SetCurrentTileSize(float value)
    {
        _currentileSize = value;
    }


    public TerrainTileGenerator GetTerrainTileGenerator()
    {
        return _terrainTileGenerator;
    }

    public List<SideTile> GetAllTiles()
    {
        var formattedTiles = tiles.Select((t) =>
            {
                var sT = new SideTile();
                sT.obj = t.obj;
                sT.selection = t.selection;
                return sT;
            });

        var allTiles = formattedTiles.Union<SideTile>(sideTiles.Union<SideTile>(sideTilesBackward).ToList()).ToList();

        return allTiles;
    }
}
