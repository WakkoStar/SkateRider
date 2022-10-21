using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Globalization;

public class LevelGenerator : MonoBehaviour
{
    //SETTINGS
    [SerializeField] private GameObject Player;
    [SerializeField] private float baseHeightLevel = -1.47f;
    [SerializeField] private List<Tile> tiles = new List<Tile>();
    [SerializeField] private int tileAmount = 2;
    [SerializeField] private float tileSizeEnd = 10;
    [SerializeField] private float tileSizeStart = 5;
    [SerializeField] private float zStartScale = 1;
    [SerializeField] private float yStartScale = 1.5f;
    [SerializeField] private float grindStartYScale = 1.6f;
    [SerializeField] private float zEndScale = 1;
    [SerializeField] private float yEndScale = 1.5f;
    [SerializeField] private float grindEndYScale = 1.6f;
    [SerializeField] private PhysicMaterial grindPhysicMaterial;
    [SerializeField] private PhysicMaterial groundPhysicMaterial;
    [SerializeField] private Material grindMaterial;
    [SerializeField] private Material groundMaterial;

    //STATE
    private float _zScale = 5;
    private float _yScale = 5;
    private float _grindYScale = 5;
    private int _chunkIndex = -1;
    private int _landingChunkIndex = -1;
    private int _tileSize = 5;
    private float _currentileSize = 5;
    private List<GameObject> _terrain = new List<GameObject>();
    private List<GameObject> _sideTerrain = new List<GameObject>();
    private List<GameObject> _grind = new List<GameObject>();
    private float _heightOffset = 0;
    private GameObject _Ground;
    private GameObject _GroundSide;
    private GameObject _GroundForward;
    private GameObject _GroundBackward;

    private GameObject _Grind;
    private int _switchingIndex = 12;
    private bool _isSwitchingSize = false;
    public struct TileOccurence
    {
        public string tileName;
        public int occurence;
    }
    private List<TileOccurence> _tileOccurences = new List<TileOccurence>();

    void Start()
    {
        _tileSize = (int)tileSizeStart;
        _currentileSize = _tileSize;
        _yScale = yStartScale;
        _zScale = zStartScale;
        _grindYScale = grindStartYScale;

        _Ground = InitTerrain("Ground", groundPhysicMaterial, groundMaterial);
        _Grind = InitTerrain("Grind", grindPhysicMaterial, grindMaterial);
        _GroundForward = InitTerrain("GroundForward", groundPhysicMaterial, groundMaterial);
        _GroundBackward = InitTerrain("GroundBackward", groundPhysicMaterial, groundMaterial);
        _GroundSide = InitTerrain("GroundSide", groundPhysicMaterial, groundMaterial);

        _GroundForward.transform.parent = _GroundSide.transform;
        _GroundBackward.transform.parent = _GroundSide.transform;
        _GroundForward.transform.position = -Vector3.forward * _zScale * _tileSize;
        _GroundBackward.transform.position = Vector3.forward * _zScale * _tileSize;

        for (int i = 0; i < tileAmount - 1; i++)
        {
            AddTileToTerrain();
        }

        for (int i = 0; i < tiles.Count; i++)
        {
            var tileOccurence = new TileOccurence();
            tileOccurence.occurence = 0;
            tileOccurence.tileName = tiles[i].obj.name;
            _tileOccurences.Add(tileOccurence);
        }

        StartCoroutine(IncreaseDifficulty());
    }


    void FixedUpdate()
    {

        if ((int)Mathf.Round(_currentileSize) != _tileSize && !_isSwitchingSize)
        {
            _switchingIndex = 0;
            _isSwitchingSize = true;
        }

        int currentChunkIndex = (int)(Player.transform.position.x / _tileSize);
        if (currentChunkIndex != _chunkIndex)
        {
            _GroundForward.transform.position = Vector3.Lerp(_GroundForward.transform.position, -Vector3.forward * _zScale * _tileSize, 0.1f);
            _GroundBackward.transform.position = Vector3.Lerp(_GroundBackward.transform.position, Vector3.forward * _zScale * _tileSize, 0.1f);

            DeleteOldTile(_terrain);
            DeleteOldTile(_sideTerrain);

            if (_switchingIndex <= 11)
            {
                SwitchTileTerrain();
            }
            else
            {
                _isSwitchingSize = false;
                AddTileToTerrain();
            }

            _chunkIndex = currentChunkIndex;

        }
        DeleteOldGrind(_Grind, 10);
    }

    private GameObject InitTerrain(string name, PhysicMaterial physicMaterial, Material material)
    {
        var terrain = new GameObject(name);
        terrain.AddComponent<MeshFilter>();
        terrain.AddComponent<MeshCollider>().material = physicMaterial;
        terrain.AddComponent<MeshRenderer>().material = material;
        terrain.transform.parent = transform;

        return terrain;
    }

    public void InitTiles()
    {
        for (int i = 0; i < tiles.Count; i++)
        {
            var oldTileToggles = tiles[i].tileToggles;
            tiles[i].tileToggles = new TileToggle[tiles.Count];

            for (int j = 0; j < tiles.Count; j++)
            {
                if (oldTileToggles.Length > j)
                {
                    if (tiles[j].obj.name == oldTileToggles[j].tileName)
                    {
                        tiles[i].tileToggles[j] = oldTileToggles[j];
                        continue;
                    }
                }
                tiles[i].tileToggles[j] = new TileToggle(tiles[j].obj.name, false);
            }
        }
    }

    private void DeleteOldTile(List<GameObject> terrain)
    {
        if (terrain.Count == 0) return;
        Destroy(terrain[0]);
        terrain.RemoveAt(0);
    }

    private void DeleteOldGrind(GameObject Grind, float offset)
    {
        for (int i = 0; i < Grind.transform.childCount; i++)
        {
            var grindSegment = Grind.transform.GetChild(i);
            if (grindSegment.position.x < Player.transform.position.x - _tileSize * offset)
            {
                Destroy(grindSegment.gameObject);
            }
        }
    }

    private void SwitchTileTerrain()
    {
        if ((_switchingIndex >= 0 && _switchingIndex < 4)
        || (_switchingIndex >= 7 && _switchingIndex <= 11)
        )
        {
            AddTileToTerrain(tiles.Find(t => t.obj.name.Contains("plain 0.0")).obj);
        }

        if (_switchingIndex == 4)
        {
            AddTileToTerrain(tiles.Find(t => t.obj.name.Contains("bump-end -0.5")).obj);
        }

        if (_switchingIndex == 5)
        {
            AddTileToTerrain(tiles.Find(t => t.obj.name.Contains("void 0.0")).obj);

            var tileSizeGap = tileSizeEnd - tileSizeStart;
            _yScale += (yEndScale - yStartScale) / tileSizeGap;
            _zScale += (zEndScale - zStartScale) / tileSizeGap;
            _grindYScale += (grindEndYScale - grindStartYScale) / tileSizeGap;
            _tileSize = (int)Mathf.Round(_currentileSize);
        }

        if (_switchingIndex == 6)
        {
            AddTileToTerrain(tiles.Find(t => t.obj.name.Contains("bump-start +0.5")).obj);
        }
        _switchingIndex += 1;
    }

    void AddTileToTerrain(GameObject forcedTile = null)
    {
        var playerPos = new Vector3(_chunkIndex * _tileSize, (baseHeightLevel * _tileSize * _yScale / 2f), 0);
        var nextChunkOffset = Vector3.right * _tileSize * _terrain.Count + Vector3.up * _heightOffset * _tileSize * _yScale / 2;

        var choosenTile = forcedTile ? forcedTile : ChooseTile();
        var sideChoosenTile = forcedTile ? forcedTile : ChooseSideTile(choosenTile);
        _heightOffset += SetHeightOffset(choosenTile);

        GameObject tile;
        GameObject sideTile;

        if (choosenTile.name.Contains("grind"))
        {
            var baseGround = choosenTile.transform.GetChild(0).gameObject;
            baseGround.name = choosenTile.name;
            tile = InstantiateTile(baseGround, playerPos + nextChunkOffset, _yScale, _zScale, _Ground.transform);
            sideTile = tile;

            var grind = choosenTile.transform.GetChild(1).gameObject;
            var tileGrind = InstantiateTile(grind.gameObject, playerPos + nextChunkOffset, _grindYScale, 1, _Grind.transform);
            AddTileToGrind(choosenTile, tileGrind);
        }
        else
        {
            tile = InstantiateTile(choosenTile, playerPos + nextChunkOffset, _yScale, _zScale, _Ground.transform);
            sideTile = InstantiateTile(sideChoosenTile, playerPos + nextChunkOffset, _yScale, _zScale, _GroundSide.transform);
        }

        _terrain.Add(tile);
        _sideTerrain.Add(sideTile);

        List<CombineInstance> groundCombine = SetTerrainCombine(_terrain, _Ground.GetComponentsInChildren<MeshFilter>());
        ApplyMeshCombine(_Ground, groundCombine);

        List<CombineInstance> groundSideCombine = SetTerrainCombine(_sideTerrain, _GroundSide.GetComponentsInChildren<MeshFilter>());
        ApplyMeshCombine(_GroundForward, groundSideCombine);
        ApplyMeshCombine(_GroundBackward, groundSideCombine);
    }

    GameObject InstantiateTile(GameObject tile, Vector3 pos, float yScale, float zScale, Transform parent)
    {
        var instanceTile = Instantiate(tile, pos, Quaternion.identity, parent);

        instanceTile.transform.localScale = new Vector3(
            tile.transform.localScale.x,
            tile.transform.localScale.y * yScale,
            tile.transform.localScale.z * zScale
        ) * _tileSize;

        instanceTile.name = tile.name;

        return instanceTile;
    }

    List<CombineInstance> SetTerrainCombine(List<GameObject> terrain, MeshFilter[] terrainMeshFilters)
    {
        var terrrainCombine = new List<CombineInstance>();

        foreach (var tile in terrain)
        {
            var combine = SetTileCombine(tile.GetComponent<MeshFilter>());
            terrrainCombine.Add(combine);
            tile.gameObject.SetActive(false);
        }

        return terrrainCombine;
    }

    CombineInstance SetTileCombine(MeshFilter tileMeshFilter)
    {
        var combineTile = new CombineInstance();
        combineTile.mesh = tileMeshFilter.sharedMesh;
        combineTile.transform = tileMeshFilter.transform.localToWorldMatrix;

        return combineTile;
    }

    void ApplyMeshCombine(GameObject terrain, List<CombineInstance> terrainCombine)
    {
        var meshFilter = terrain.GetComponent<MeshFilter>();
        meshFilter.mesh.Clear();
        meshFilter.mesh = new Mesh();
        meshFilter.mesh.CombineMeshes(terrainCombine.ToArray());

        terrain.GetComponent<MeshCollider>().sharedMesh = meshFilter.mesh;
        terrain.gameObject.SetActive(true);
    }

    void AddTileToGrind(GameObject tile, GameObject subTile)
    {
        var subTileCollider = subTile.GetComponent<BoxCollider>();

        if (tile.name.Contains("grind-start"))
        {
            var grindSegment = new GameObject("Grind segment");
            grindSegment.transform.parent = _Grind.transform;
            grindSegment.transform.position = subTile.transform.position;
            grindSegment.transform.localScale = subTile.transform.localScale;
            grindSegment.tag = "Grind";

            var grindSegmentCollider = grindSegment.AddComponent<BoxCollider>();
            grindSegmentCollider.center = subTileCollider.center;
            grindSegmentCollider.size = subTileCollider.size;
            grindSegmentCollider.material = grindPhysicMaterial;

            _grind.Add(grindSegment);
        }
        else
        {
            var lastGrindSegmentCollider = _grind[_grind.Count - 1].GetComponent<BoxCollider>();

            lastGrindSegmentCollider.size += Vector3.right * subTileCollider.size.x;
            lastGrindSegmentCollider.center += Vector3.right * subTileCollider.size.x / 2;
        }

        Destroy(subTileCollider);
    }

    float SetHeightOffset(GameObject tile)
    {
        var match = Regex.Match(tile.name, @"([-+]?[0-9]*\.?[0-9]+)");
        if (match.Success)
            return float.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture.NumberFormat);

        return 0;
    }

    GameObject ChooseTile()
    {
        //START
        if (_chunkIndex <= tileAmount)
        {
            return tiles.Find(t => t.obj.name.Contains("plain 0.0")).obj;
        }

        var candidates = new List<GameObject>();
        var prevTile = _terrain[_terrain.Count - 1];
        int prevTileIndex = (int)(prevTile.transform.position.x / _tileSize);

        //TERRAIN GENERATION
        foreach (var tile in tiles)
        {
            var selection = SelectTile(tile, prevTile);
            if (selection != null) candidates.Add(selection);
        }

        // LANDING ZONE
        if (IsLandingZone(prevTileIndex))
        {
            // Debug.DrawRay(new Vector3((_landingChunkIndex - 1) * _tileSize, -4 * _tileSize * yScale, 0), Vector3.up * 100, Color.blue, 100);
            // Debug.DrawRay(new Vector3((_landingChunkIndex + 2) * _tileSize, -4 * _tileSize * yScale, 0), Vector3.up * 100, Color.blue, 100);

            foreach (var tile in tiles)
            {
                if (!tile.canLandingOn)
                    candidates.RemoveAll(t => t.name.Contains(tile.obj.name));
            }

            if (prevTileIndex != _landingChunkIndex - 2)
            {
                candidates.RemoveAll(t => t.name.Contains("grind-start") && t.name.Contains(" +") && t.name.Contains(" -"));
            }
        }

        // GRIND 
        // if (candidates.FindIndex(c => c.name.Contains("grind-start 0.0")) != -1 && UnityEngine.Random.Range(0, 100f) > 95)
        //     return tiles.Find(t => t.obj.name.Contains("grind-start 0.0")).obj;

        if (prevTile.name.Contains("grind-end 0.0"))
            _grind.RemoveAt(0);

        // FLATTEN CURVES
        if (_heightOffset < -4)
        {
            candidates.RemoveAll(t => t.name.Contains(" -"));
        }
        else if (_heightOffset > 4)
        {
            candidates.RemoveAll(t => t.name.Contains(" +"));
        }

        //SPREAD
        candidates = SpreadTileCandidate(candidates);

        var plainIndex = candidates.FindIndex(c => c.name.Contains("plain 0.0"));
        if (plainIndex != -1 && UnityEngine.Random.Range(0, 100f) > 90)
        {
            AddOccurenceOnTile(candidates[plainIndex].name);
            return candidates[plainIndex];
        }

        //FALLBACK
        if (candidates.Count == 0)
            candidates.Add(tiles.Find(t => t.obj.name.Contains("plain 0.0")).obj);

        var choosenTile = candidates[UnityEngine.Random.Range(0, candidates.Count)];
        AddOccurenceOnTile(choosenTile.name);
        return choosenTile;
    }

    GameObject ChooseSideTile(GameObject choosenTile)
    {
        if (_chunkIndex <= tileAmount)
        {
            return tiles.Find(t => t.obj.name.Contains("plain 0.0")).obj;
        }

        var match = Regex.Match(choosenTile.name, @"([-+]?[0-9]*\.?[0-9]+)");
        var height = "0.0";
        if (match.Success)
            height = match.Groups[1].Value;

        var prevTile = _sideTerrain[_sideTerrain.Count - 1];
        var candidates = new List<GameObject>();
        List<Tile> acceptedTiles = tiles.FindAll(t =>
            t.obj.name.Contains(height)
            && !t.obj.name.Contains("grind")
        );

        foreach (var tile in acceptedTiles)
        {
            var selection = SelectTile(tile, prevTile);
            if (selection != null) candidates.Add(selection);
        }

        if (choosenTile.name.Contains("hole"))
            return choosenTile;

        if (choosenTile.name.Contains("plain 0.0"))
            return choosenTile;

        if (candidates.Count == 0)
            candidates.Add(tiles.Find(t => t.obj.name.Contains("plain 0.0")).obj);

        return candidates[UnityEngine.Random.Range(0, candidates.Count)];
    }

    void AddOccurenceOnTile(string tileName)
    {
        var tileOccurenceIndex = _tileOccurences.FindIndex(t => t.tileName.Contains(tileName));
        var tileOccurence = _tileOccurences[tileOccurenceIndex];
        tileOccurence.occurence += 1;

        _tileOccurences[tileOccurenceIndex] = tileOccurence;
    }

    List<GameObject> SpreadTileCandidate(List<GameObject> candidates)
    {
        var tilesNotOccured = _tileOccurences.OrderBy(t => t.occurence);
        var notOccuredCandidates = new List<GameObject>();

        int count = 0;
        foreach (var tile in tilesNotOccured)
        {
            if (count >= 5) continue;

            var notOccuredCandidate = candidates.Find(c => c.name.Contains(tile.tileName));
            if (notOccuredCandidate)
            {
                notOccuredCandidates.Add(notOccuredCandidate);
                count++;
            }

        }

        return notOccuredCandidates;
    }

    bool IsSafeZoneOnTerrain(List<GameObject> terrain)
    {
        var prevTileOne = terrain[_terrain.Count - 1];
        var prevTileTwo = terrain[_terrain.Count - 2];
        var prevTileThree = terrain[_terrain.Count - 3];

        bool isPrevTileThreeSafe = false;
        foreach (var tile in tiles)
        {
            if (tile.isSafe && prevTileThree.name.Contains(tile.obj.name))
                isPrevTileThreeSafe = true;
        }

        bool isPrevTileTwoSafe =
        prevTileTwo.name.Contains("plain 0.0")
        || prevTileTwo.name.Contains(" +0.25")
        || prevTileTwo.name.Contains(" -0.25")
        || prevTileTwo.name.Contains(" +0.125")
        || prevTileTwo.name.Contains(" -0.125");

        bool isPrevTileOneSafe =
        prevTileOne.name.Contains("plain 0.0")
        || prevTileOne.name.Contains(" +0.25")
        || prevTileOne.name.Contains(" -0.25")
        || prevTileOne.name.Contains(" +0.125")
        || prevTileOne.name.Contains(" -0.125");

        return isPrevTileThreeSafe && isPrevTileTwoSafe && isPrevTileOneSafe;
    }

    bool IsLandingZone(int prevTileIndex)
    {
        return prevTileIndex >= _landingChunkIndex - 2
        && prevTileIndex <= _landingChunkIndex + 3;
    }

    GameObject SelectTile(Tile tile, GameObject prevTile)
    {
        foreach (var tileToggle in tile.tileToggles)
        {
            if (tileToggle.canBeBefore && prevTile.name.Contains(tileToggle.tileName))
            {
                return tile.obj;
            }
        }
        if (tile.shouldBeInSafeZone && IsSafeZoneOnTerrain(_terrain))
        {
            return tile.obj;
        }

        return null;
    }

    public void PredictGroundHit()
    {
        float h = Player.transform.position.y - ((baseHeightLevel * _tileSize * _yScale / 2f) + _heightOffset * _tileSize * _yScale / 2);
        float g = Physics.gravity.magnitude;
        float vy = Player.GetComponent<Rigidbody>().velocity.y + Player.GetComponent<SkateController>().jumpForce / Player.GetComponent<Rigidbody>().mass;
        float vx = 20;
        var totalTime = (vy + Mathf.Sqrt(vy * vy + 2 * g * h)) / g;
        var range = vx * totalTime;
        _landingChunkIndex = ((int)((Player.transform.position.x + range) / _tileSize));
    }

    public float GetHeightTerrain()
    {
        return _heightOffset * _tileSize * _yScale / 2;
    }

    private IEnumerator IncreaseDifficulty()
    {
        for (double a = 0; a < 1; a += Time.deltaTime / 2000000)
        {
            _currentileSize = Mathf.Lerp(_currentileSize, tileSizeEnd, (float)a);
            yield return null;
        }
    }


    Mesh CreateShape(int width, int length, Vector3 offset)
    {
        var vertices = new Vector3[(width + 2) * (length + 1)];

        for (int i = 0; i < (width + 1); i++)
        {
            for (int j = 0; j < (length + 1); j++)
            {
                vertices[(i * (length + 1)) + j] = new Vector3(j + offset.x, 0 + offset.y, i + offset.z);
            }
        }
        for (int i = 0; i < (width + 1); i++)
        {
            vertices[(width + 1) * (length + 1) + i] = new Vector3(i + offset.x, -10 + offset.y, offset.z);
        }

        var triangles = new int[((width + 1) * length * 2) * 3];
        var index = -2;
        for (int i = 0; i < width; i++)
        {
            index += 1;
            for (int j = 0; j < length; j++)
            {
                index += 1;
                var size = 6 * ((i * length) + j);
                triangles[0 + size] = 0 + index;
                triangles[1 + size] = length + 1 + index;
                triangles[2 + size] = 1 + index;
                triangles[3 + size] = 1 + index;
                triangles[4 + size] = length + 1 + index;
                triangles[5 + size] = length + 2 + index;
            }
        }
        for (int i = 0; i < width; i++)
        {
            var size = ((width * length * 2) * 3) + 6 * i;

            triangles[0 + size] = (length + 1) * (width + 1) + i;
            triangles[1 + size] = i;
            triangles[2 + size] = (length + 1) * (width + 1) + i + 1;
            triangles[3 + size] = (length + 1) * (width + 1) + i + 1;
            triangles[4 + size] = i;
            triangles[5 + size] = i + 1;
        }
        var mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        return mesh;
    }
}
