using System;
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

    private List<GameObject> _forwardSideTerrain = new List<GameObject>();
    private List<GameObject> _backwardSideTerrain = new List<GameObject>();
    private List<SideTile> _allTiles = new List<SideTile>();

    // private GameObject _forwardMeshCombiner;
    // private GameObject _backwardMeshCombiner;

    // Dictionary<string, List<GameObject>> _forwardSideTilesInMeshCombiner = new Dictionary<string, List<GameObject>>();
    // Dictionary<string, List<GameObject>> _backwardSideTilesInMeshCombiner = new Dictionary<string, List<GameObject>>();

    TileSideSelector tileSideSelector;
    private UnityAction onTilePassedAction;

    private void Awake()
    {
        _GroundForward = InitLevelComponent("GroundForward", transform);
        _GroundBackward = InitLevelComponent("GroundBackward", transform);

        // _forwardMeshCombiner = InitLevelComponent("Mesh Combiner", _GroundForward.transform, typeof(MeshCollider), typeof(MeshRenderer));
        // _backwardMeshCombiner = InitLevelComponent("Mesh Combiner", _GroundBackward.transform, typeof(MeshCollider), typeof(MeshRenderer));
    }

    void Start()
    {
        _allTiles = sideTiles.Union(backwardSideTiles).ToList();

        tileSideSelector = new TileSideSelector(terrainTileGenerator.tiles, terrainTileGenerator.DefaultTile);
        onTilePassedAction += DeleteSideTerrainFirstTile;
        terrainTileGenerator.OnTilePassed.AddListener(onTilePassedAction);
    }

    public void AddTileToTerrain(GameObject baseTile)
    {
        UpdateSideTerrainPosition(_GroundForward, -1);
        UpdateSideTerrainPosition(_GroundBackward, 1);

        AddSideTileToTerrain(baseTile, sideTiles, _forwardSideTerrain, _GroundForward);
        AddSideTileToTerrain(baseTile, _allTiles, _backwardSideTerrain, _GroundBackward);
    }

    private void AddSideTileToTerrain(
        GameObject baseTile,
        List<SideTile> sideTiles,
        // Dictionary<string, List<GameObject>> sideTilesInMeshCombiner,
        // GameObject meshCombiner,
        List<GameObject> sideTerrain,
        GameObject SideTerrainObj
    )
    {
        var sideChoosenTile = tileSideSelector != null && sideTerrain.Count > 1
        ? tileSideSelector.ChooseSideTile(baseTile, sideTiles, sideTerrain)
        : terrainTileGenerator.DefaultTile;

        var instanceTile = terrainTileGenerator.InstantiateTile(
            sideChoosenTile,
            baseTile.transform.position + SideTerrainObj.transform.position,
            new Vector2(terrainTileGenerator.yScale, terrainTileGenerator.zScale),
            SideTerrainObj.transform,
            terrainTileGenerator.tileSize
        );

        sideTerrain.Add(instanceTile);

        // var tileComponents = instanceTile.GetComponent<TileMeshCombinerInfo>().tileComponents;
        // foreach (var tileComponent in tileComponents)
        // {
        //     if (tileComponent.shouldBeInMeshCombiner)
        //     {
        //         terrainTileGenerator.UpdateLayer(sideTilesInMeshCombiner, tileComponent.Tile, meshCombiner.transform);
        //     }
        // }

        // MeshCombiner.CombineLayers(sideTilesInMeshCombiner, meshCombiner.transform);
    }

    private void DeleteSideTerrainFirstTile()
    {
        // terrainTileGenerator.DeleteTileMeshCombiner(_forwardSideTerrain, _forwardSideTilesInMeshCombiner);
        terrainTileGenerator.DeleteTerrainFirstTile(_forwardSideTerrain);

        // terrainTileGenerator.DeleteTileMeshCombiner(_backwardSideTerrain, _backwardSideTilesInMeshCombiner);
        terrainTileGenerator.DeleteTerrainFirstTile(_backwardSideTerrain);
    }

    private GameObject InitLevelComponent(string name, Transform parent, params Type[] components)
    {
        var levelComponent = new GameObject(name, components);
        levelComponent.transform.parent = parent;

        return levelComponent;
    }

    private void UpdateSideTerrainPosition(GameObject SideTerrainObj, int direction)
    {
        SideTerrainObj.transform.position = Vector3.forward * terrainTileGenerator.zScale * terrainTileGenerator.tileSize * direction;
    }


    public void StartGame()
    {
        _forwardSideTerrain = new List<GameObject>();
        // _forwardSideTilesInMeshCombiner = new Dictionary<string, List<GameObject>>();
        // terrainTileGenerator.CleanGameObjectChilds(_forwardMeshCombiner);

        _backwardSideTerrain = new List<GameObject>();
        // _backwardSideTilesInMeshCombiner = new Dictionary<string, List<GameObject>>();
        // terrainTileGenerator.CleanGameObjectChilds(_backwardMeshCombiner);
    }

}
