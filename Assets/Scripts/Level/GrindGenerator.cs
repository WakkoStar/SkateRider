using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GrindTileGenerator : MonoBehaviour
{
    // public GameObject Player;
    public TerrainTileGenerator terrainTileGenerator;
    public GameObject GrindStart;
    public GameObject GrindEnd;
    private List<GameObject> _grind = new List<GameObject>();
    private UnityAction onTilePassedAction;

    public void Init(TerrainTileGenerator terrainTileGenerator, GameObject GrindStart, GameObject GrindEnd)
    {
        this.terrainTileGenerator = terrainTileGenerator;
        this.GrindStart = GrindStart;
        this.GrindEnd = GrindEnd;
    }
    // Start is called before the first frame update
    void Start()
    {
        onTilePassedAction += DeleteGrindFirstTile;
        terrainTileGenerator.OnTilePassed.AddListener(onTilePassedAction);
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void DeleteGrindFirstTile()
    {
        var terrain = terrainTileGenerator.GetBaseTerrain();
        var prevTile = terrain[terrain.Count - (terrainTileGenerator.tileAmount - 1)];

        if (prevTile.name.Contains(GrindEnd.name))
        {
            Destroy(_grind[0]);
            _grind.RemoveAt(0);
        }
    }

    public void AddTileToGrind(GameObject baseTile, GameObject grindTile)
    {
        var grindTileCollider = grindTile.GetComponent<BoxCollider>();

        if (baseTile.name.Contains(GrindStart.name))
        {
            var grindSegment = new GameObject("Grind segment");
            grindSegment.transform.parent = transform;
            grindSegment.transform.position = grindTile.transform.position;
            grindSegment.transform.localScale = grindTile.transform.localScale;
            grindSegment.tag = "Grind";

            var grindSegmentCollider = grindSegment.AddComponent<BoxCollider>();
            grindSegmentCollider.center = grindTileCollider.center;
            grindSegmentCollider.size = grindTileCollider.size;
            grindSegmentCollider.material = grindTileCollider.GetComponent<Collider>().material;

            _grind.Add(grindSegment);
        }
        else
        {
            var lastGrindSegmentCollider = _grind[_grind.Count - 1].GetComponent<BoxCollider>();

            lastGrindSegmentCollider.size += Vector3.right * grindTileCollider.size.x;
            lastGrindSegmentCollider.center += Vector3.right * grindTileCollider.size.x / 2;
        }
    }
}
