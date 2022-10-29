using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SideTerrainTileGenerator : MonoBehaviour
{
    public TerrainTileGenerator terrainTileGenerator;
    public List<SideTile> sideTiles;
    public List<SideTile> backwardSideTiles;
    GameObject _GroundBackward;
    GameObject _GroundForward;
    private List<List<GameObject>> _forwardSideTerrain = new List<List<GameObject>>();
    List<GameObject> _forwardSideTerrainLayers = new List<GameObject>();
    private List<List<GameObject>> _backwardSideTerrain = new List<List<GameObject>>();
    List<GameObject> _backwardSideTerrainLayers = new List<GameObject>();
    TileSideSelector tileSideSelector;
    private UnityAction onTilePassedAction;

    public void Init(TerrainTileGenerator terrainTileGenerator, List<SideTile> sideTiles, List<SideTile> backwardSideTiles)
    {
        this.terrainTileGenerator = terrainTileGenerator;
        this.sideTiles = sideTiles;
        this.backwardSideTiles = backwardSideTiles;
    }

    private void Awake()
    {
        _GroundForward = InitLevelComponent("GroundForward");
        _GroundBackward = InitLevelComponent("GroundBackward");
        _GroundForward.transform.parent = transform;
        _GroundBackward.transform.parent = transform;
    }

    void Start()
    {
        tileSideSelector = new TileSideSelector(terrainTileGenerator.tiles, terrainTileGenerator.DefaultTile);
        onTilePassedAction += DeleteSideTerrainFirstTile;
        terrainTileGenerator.OnTilePassed.AddListener(onTilePassedAction);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void AddTileToTerrain(GameObject baseTile)
    {
        UpdateSideTerrainPosition(_GroundForward, -1);
        UpdateSideTerrainPosition(_GroundBackward, 1);

        AddSideTileToTerrain(baseTile, sideTiles, _forwardSideTerrainLayers, _forwardSideTerrain, _GroundForward);
        AddSideTileToTerrain(baseTile, sideTiles.Union(backwardSideTiles).ToList(), _backwardSideTerrainLayers, _backwardSideTerrain, _GroundBackward);
    }

    private void AddSideTileToTerrain(GameObject baseTile, List<SideTile> sideTiles, List<GameObject> sideTerrainLayers, List<List<GameObject>> sideTerrain, GameObject SideTerrainObj)
    {
        var sideChoosenTile = tileSideSelector != null
        ? tileSideSelector.ChooseSideTile(baseTile, sideTiles, GetSideBaseTerrain(sideTerrain))
        : terrainTileGenerator.DefaultTile;

        var tileGroup = new List<GameObject>();
        bool hasTileChild = sideChoosenTile.transform.childCount != 0;
        for (int i = 0; i < (hasTileChild ? sideChoosenTile.transform.childCount : 1); i++)
        {
            var tileComponent = hasTileChild ? sideChoosenTile.transform.GetChild(i).gameObject : sideChoosenTile;
            var currentLayer = terrainTileGenerator.GetLayer(sideTerrainLayers, tileComponent, SideTerrainObj.transform);

            var tileEuler = terrainTileGenerator.GetScaledEulerAngles(
               new Vector2(terrainTileGenerator.tileSize, terrainTileGenerator.tileSize * terrainTileGenerator.yScale * terrainTileGenerator.GetHeightOffset(baseTile.name) / 2)
               , tileComponent.transform.eulerAngles
           );

            var tile = terrainTileGenerator.InstantiateTile(
                tileComponent,
                baseTile.transform.position + SideTerrainObj.transform.position,
                tileEuler,
                new Vector2(terrainTileGenerator.yScale, terrainTileGenerator.zScale),
                currentLayer.transform,
                terrainTileGenerator.tileSize
            );
            tileGroup.Add(tile);
        }
        sideTerrain.Add(tileGroup);

        MeshCombiner.CombineLayers(sideTerrainLayers);
    }

    private void DeleteSideTerrainFirstTile()
    {
        terrainTileGenerator.DeleteTerrainFirstTile(_forwardSideTerrain);
        terrainTileGenerator.DeleteTerrainFirstTile(_backwardSideTerrain);
    }

    private List<GameObject> GetSideBaseTerrain(List<List<GameObject>> sideTerrain)
    {
        return sideTerrain.Select(tileGroup => tileGroup[0]).ToList();
    }

    private GameObject InitLevelComponent(string name)
    {
        var levelComponent = new GameObject(name);
        levelComponent.transform.parent = transform;

        return levelComponent;
    }

    private void UpdateSideTerrainPosition(GameObject SideTerrainObj, int direction)
    {
        SideTerrainObj.transform.position = Vector3.forward * terrainTileGenerator.zScale * terrainTileGenerator.tileSize * direction;

        for (int i = 0; i < SideTerrainObj.transform.childCount; i++)
        {
            var layer = SideTerrainObj.transform.GetChild(i);
            layer.transform.localPosition = Vector3.forward * terrainTileGenerator.zScale * terrainTileGenerator.tileSize * -direction;
        }
    }


}
