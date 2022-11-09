using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class LevelTileMonitor : MonoBehaviour
{
    //STATE
    private GameObject _Terrain;
    private GameObject _SideTerrain;
    private GameObject _GrindTerrain;

    private int _switchingTileIndex = 10000;
    bool _isSwitchingSize;

    private int _tileSize;
    private float _currentileSize;
    private float _yScale;
    private float _zScale;

    private TerrainTileGenerator _terrainTileGenerator;
    private SideTerrainTileGenerator _sideTerrainTileGenerator;
    private GrindTileGenerator _grindTileGenerator;

    private GameObject _MainCamera;
    private Vector3 _cameraOffset;
    private float _landingHit;


    //SETTINGS
    [SerializeField] private GameObject Player;
    [SerializeField] private float baseHeightLevel = -1.47f;

    [SerializeField] private List<Tile> tiles = new List<Tile>();
    [SerializeField] private List<SideTile> sideTiles = new List<SideTile>();
    [SerializeField] private List<SideTile> sideTilesBackward = new List<SideTile>();
    [SerializeField] private int tileAmount = 2;

    [SerializeField] private GameObject DefaultTile;
    [SerializeField] private GameObject VoidTile;
    [SerializeField] private GameObject StartSwitchTile;
    [SerializeField] private GameObject EndSwitchTile;
    [SerializeField] private List<GameObject> GrindStarts = new List<GameObject>();
    [SerializeField] private List<GameObject> GrindEnds = new List<GameObject>();

    [SerializeField] private Vector3 cameraStartOffset = new Vector3(4, 3, -15);
    [SerializeField] private Vector3 cameraEndOffset = new Vector3(4, 3, -15);

    [SerializeField] private DifficultyIncreaser difficultyIncreaser;
    [SerializeField] private float zStartScale = 1;
    [SerializeField] private float zEndScale = 1;
    [SerializeField] private float yStartScale = 1.5f;
    [SerializeField] private float yEndScale = 1.5f;
    [SerializeField] private float startFOV = 80;
    [SerializeField] private float endFOV = 80;
    [SerializeField] private float tileSizeStart = 5;
    [SerializeField] private float tileSizeEnd = 10;
    [SerializeField] private int switchTileTransitionLength = 4;

    private UnityAction<GameObject> onTileAddedAction;
    private UnityAction onTilePassedAction;

    void Start()
    {
        //ASSIGN INCREASERS
        _tileSize = (int)tileSizeStart;
        DifficultyIncreaser.DelValueModifier SetCurrentTileSizeCallback = SetCurrentTileSize;
        difficultyIncreaser.AddIncreaser<float>(tileSizeStart, tileSizeEnd, SetCurrentTileSizeCallback);

        DifficultyIncreaser.DelValueModifier SetCameraOffsetCallback = SetCameraOffset;
        difficultyIncreaser.AddIncreaser<Vector3>(cameraStartOffset, cameraEndOffset, SetCameraOffsetCallback);

        DifficultyIncreaser.DelValueModifier SetCameraFOVCallback = SetCameraFOV;
        difficultyIncreaser.AddIncreaser<float>(startFOV, endFOV, SetCameraFOVCallback);

        _yScale = yStartScale;
        _zScale = zStartScale;

        //INIT LEVEL STRUCTURE
        _Terrain = InitLevelComponent("BaseTerrain");
        _terrainTileGenerator = _Terrain.AddComponent<TerrainTileGenerator>();
        _terrainTileGenerator.Init(Player, baseHeightLevel, tiles, tileAmount, _tileSize, _yScale, _zScale, DefaultTile);

        _SideTerrain = InitLevelComponent("SideTerrain");
        _sideTerrainTileGenerator = _SideTerrain.AddComponent<SideTerrainTileGenerator>();
        _sideTerrainTileGenerator.Init(_terrainTileGenerator, sideTiles, sideTilesBackward);

        _GrindTerrain = InitLevelComponent("GrindTerrain");
        _grindTileGenerator = _GrindTerrain.AddComponent<GrindTileGenerator>();
        _grindTileGenerator.Init(_terrainTileGenerator, GrindStarts, GrindEnds);


        //EVENTS
        onTileAddedAction += AddTileToOtherLevelComponents;
        _terrainTileGenerator.OnTileAdded.AddListener(onTileAddedAction);

        onTilePassedAction += ForceTerrainSwitch;
        _terrainTileGenerator.OnTilePassed.AddListener(onTilePassedAction);

        _MainCamera = Camera.main.gameObject;
    }

    void Update()
    {
        if ((int)Mathf.Round(_currentileSize) != _tileSize && !_isSwitchingSize)
        {
            _switchingTileIndex = 0;
            _isSwitchingSize = true;
        }

        _MainCamera.transform.position = SetCameraPosition(_terrainTileGenerator.GetTerrainHeight());
        _terrainTileGenerator.SetSafeZone((int)(_landingHit / _tileSize) - 2, 2);

        _terrainTileGenerator.yScale = _yScale;
        _terrainTileGenerator.zScale = _zScale;
        _terrainTileGenerator.tileSize = _tileSize;
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
        int endSwitchTileIndex = switchTileTransitionLength * 2 + 3;
        if (_switchingTileIndex < endSwitchTileIndex && _isSwitchingSize)
        {
            _terrainTileGenerator.ForceStop(true);
            SwitchTileTerrain(switchTileTransitionLength);
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

    private Vector3 SetCameraPosition(float levelHeight)
    {
        var camPos = _MainCamera.transform.position;
        var pos = Player.transform.position;
        var targetPos = new Vector3(
            pos.x + _cameraOffset.x,
            Mathf.Lerp(camPos.y, levelHeight + _cameraOffset.y, 0.01f * (Time.deltaTime * 60)),
            pos.z + _cameraOffset.z
        );

        return targetPos;
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

    private void SwitchTileTerrain(int transitionLength)
    {
        int startSwitchIndex = transitionLength;
        int endSwitchIndex = transitionLength + 3;
        int finalEndIndex = transitionLength * 2 + 3;

        /*
        We need to recalculate currentTileIndex because tileSize is changing 
        between terrainTileGenerator update, when currentTileIndex isn't equal to _tileIndex
        and the invoke of OnTilePassed
        **/
        var forceCurrentTileIndex = (int)Player.transform.position.x / _tileSize;

        if ((_switchingTileIndex >= 0 && _switchingTileIndex < startSwitchIndex)
        || (_switchingTileIndex >= endSwitchIndex && _switchingTileIndex < finalEndIndex)
        )
        {
            _terrainTileGenerator.AddTileToTerrain(DefaultTile);
        }

        if (_switchingTileIndex == startSwitchIndex)
        {
            _terrainTileGenerator.AddTileToTerrain(EndSwitchTile);
        }

        if (_switchingTileIndex == startSwitchIndex + 1)
        {
            var tileSizeGap = tileSizeEnd - tileSizeStart;
            _tileSize = (int)Mathf.Round(_currentileSize);
            _yScale += (yEndScale - yStartScale) / tileSizeGap;
            _zScale += (zEndScale - zStartScale) / tileSizeGap;

            _terrainTileGenerator.AddTileToTerrain(VoidTile, forceCurrentTileIndex);
        }

        if (_switchingTileIndex == startSwitchIndex + 2)
        {

            _terrainTileGenerator.AddTileToTerrain(StartSwitchTile, forceCurrentTileIndex);
        }

        _switchingTileIndex += 1;
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


    void SetCurrentTileSize(object value)
    {
        _currentileSize = (float)value;
    }
    void SetCameraOffset(object value)
    {
        _cameraOffset = (Vector3)value;
    }
    void SetCameraFOV(object value)
    {
        Camera.main.fieldOfView = (float)value;
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
