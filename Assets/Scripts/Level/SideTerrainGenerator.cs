using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SideTerrainTileGenerator : MonoBehaviour
{
    public TerrainTileGenerator terrainTileGenerator;
    GameObject _GroundBackward;
    GameObject _GroundForward;
    private List<List<GameObject>> _forwardSideTerrain = new List<List<GameObject>>();
    List<GameObject> _forwardSideTerrainLayers = new List<GameObject>();
    private List<List<GameObject>> _backwardSideTerrain = new List<List<GameObject>>();
    List<GameObject> _backwardSideTerrainLayers = new List<GameObject>();
    TileSideSelector tileSideSelector;
    private UnityAction onTilePassedAction;

    public void Init(TerrainTileGenerator terrainTileGenerator)
    {
        this.terrainTileGenerator = terrainTileGenerator;
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
        _GroundForward.transform.position = -Vector3.forward * terrainTileGenerator.zScale * terrainTileGenerator.tileSize;
        _GroundBackward.transform.position = Vector3.forward * terrainTileGenerator.zScale * terrainTileGenerator.tileSize;
    }

    public void AddTileToTerrain(GameObject baseTile)
    {
        AddSideTileToTerrain(baseTile, _forwardSideTerrainLayers, _forwardSideTerrain, _GroundForward);
        AddSideTileToTerrain(baseTile, _backwardSideTerrainLayers, _backwardSideTerrain, _GroundBackward);
    }

    private void AddSideTileToTerrain(GameObject baseTile, List<GameObject> sideTerrainLayers, List<List<GameObject>> sideTerrain, GameObject SideTerrainObj)
    {

        var sideChoosenTile = tileSideSelector != null
        ? tileSideSelector.ChooseSideTile(baseTile, GetSideBaseTerrain(sideTerrain))
        : terrainTileGenerator.DefaultTile;

        var tileGroup = new List<GameObject>();
        bool hasTileChild = sideChoosenTile.transform.childCount != 0;
        for (int i = 0; i < (hasTileChild ? sideChoosenTile.transform.childCount : 1); i++)
        {
            var tileComponent = hasTileChild ? sideChoosenTile.transform.GetChild(i).gameObject : sideChoosenTile;

            var currentLayer = terrainTileGenerator.GetLayer(sideTerrainLayers, tileComponent, SideTerrainObj.transform);
            var tile = terrainTileGenerator.InstantiateTile(
                tileComponent,
                baseTile.transform.position,
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
        levelComponent.AddComponent<MeshFilter>();
        levelComponent.transform.parent = transform;

        return levelComponent;
    }


}
