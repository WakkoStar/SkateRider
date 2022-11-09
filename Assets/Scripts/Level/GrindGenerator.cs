using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GrindTileGenerator : MonoBehaviour
{
    // public GameObject Player;
    public TerrainTileGenerator terrainTileGenerator;
    public List<GameObject> GrindStarts = new List<GameObject>();
    public List<GameObject> GrindEnds = new List<GameObject>();
    private List<GameObject> _grind = new List<GameObject>();
    private UnityAction onTilePassedAction;

    public void Init(TerrainTileGenerator terrainTileGenerator, List<GameObject> GrindStarts, List<GameObject> GrindEnds)
    {
        this.terrainTileGenerator = terrainTileGenerator;
        this.GrindStarts = GrindStarts;
        this.GrindEnds = GrindEnds;
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
        var terrain = terrainTileGenerator.GetTerrain();
        var prevTile = terrain[terrain.Count - (terrainTileGenerator.tileAmount - 2)];

        if (GrindEnds.Find(grindObj => grindObj.name.Contains(prevTile.name)) != null)
        {
            Destroy(_grind[0]);
            _grind.RemoveAt(0);
        }
    }

    public void AddTileToGrind(GameObject baseTile, GameObject grindTile)
    {
        var grindTileCollider = grindTile.GetComponent<BoxCollider>();
        if (GrindStarts.Find(grindObj => grindObj.name.Contains(baseTile.name)) != null)
        {
            var grindSegment = new GameObject("Grind segment");
            grindSegment.transform.parent = transform;
            grindSegment.transform.position = grindTile.transform.position;
            grindSegment.transform.eulerAngles = ScaleEulerAngles(FormatEulerAngles(grindTile.transform.eulerAngles));
            grindSegment.transform.localScale = ScaleToTerrain(grindTile.transform.localScale);

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
        grindTile.GetComponent<BoxCollider>().enabled = false;

    }

    private Vector3 ScaleToTerrain(Vector3 vectorToScale)
    {
        var tileSize = terrainTileGenerator.tileSize;
        return new Vector3(vectorToScale.x * tileSize, vectorToScale.y * tileSize * terrainTileGenerator.yScale, vectorToScale.z * tileSize * terrainTileGenerator.zScale);
    }

    private Vector3 ScaleEulerAngles(Vector3 eulerAnglesToScale)
    {
        return new Vector3(eulerAnglesToScale.x, eulerAnglesToScale.y, eulerAnglesToScale.z * terrainTileGenerator.yScale);
    }

    private Vector3 FormatEulerAngles(Vector3 eulerAngles)
    {
        return new Vector3(
            eulerAngles.x > 180 ? eulerAngles.x - 360 : eulerAngles.x,
            eulerAngles.y > 180 ? eulerAngles.y - 360 : eulerAngles.y,
            eulerAngles.z > 180 ? eulerAngles.z - 360 : eulerAngles.z
        );
    }
}
