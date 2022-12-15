using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CollectibleGenerator : MonoBehaviour
{
    [SerializeField] private GameObject upSkewCollectible;
    [SerializeField] private GameObject upSkewThreeCollectible;
    [SerializeField] private GameObject jumpCollectible;
    [SerializeField] private GameObject jumpFiveCollectible;
    [SerializeField] private GameObject downSkewCollectible;
    [SerializeField] private GameObject downSkewThreeCollectible;
    [SerializeField] private GameObject cubeCollectible;
    [SerializeField] private GameObject followCollectible;
    [SerializeField] private GameObject followThreeCollectible;
    [SerializeField] private LevelTileMonitor levelTileMonitor;

    private UnityAction<GameObject> onTileAdded;
    private TerrainTileGenerator _terrainTileGenerator;
    private List<GameObject> _terrain;

    public class GameObjectWithPosition
    {
        public GameObject gameObject;
        public Vector3 position;

        public GameObjectWithPosition(GameObject gameObject, Vector3 position)
        {
            this.gameObject = gameObject;
            this.position = position;
        }
    }

    // Start is called before the first frame update
    IEnumerator Start()
    {
        yield return new WaitForFixedUpdate();
        _terrainTileGenerator = levelTileMonitor.GetTerrainTileGenerator();
        onTileAdded += AddCollectibleToTerrain;
        onTileAdded += DeleteCollectibleToTerrain;
        _terrainTileGenerator.OnTileAdded.AddListener(onTileAdded);
    }

    public void StartGame()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
    }

    void AddCollectibleToTerrain(GameObject terrainTile)
    {
        if (_terrainTileGenerator.GetTileIndex() <= _terrainTileGenerator.GetStartTerrainIndex())
        {
            return;
        }

        if (Random.Range(0, 1f) > 0f)
        {
            var collectible = ChooseCollectible();
            if (collectible != null)
            {
                var instance = Instantiate(collectible.gameObject, collectible.position, Quaternion.identity, transform);
            }
        }
    }

    void DeleteCollectibleToTerrain(GameObject terrainTile)
    {
        _terrain = _terrainTileGenerator.GetTerrain();

        if (_terrain.Count < 4) return;
        for (int i = 0; i < transform.childCount; i++)
        {
            var collectibleInstance = transform.GetChild(i);
            if (collectibleInstance.transform.position.x < _terrain[0].transform.position.x)
            {
                Destroy(collectibleInstance.gameObject);
            }
        }
    }

#nullable enable
    GameObjectWithPosition? ChooseCollectible()
    {
        var selectedCollectibles = new List<GameObjectWithPosition>();

        _terrain = _terrainTileGenerator.GetTerrain();
        if (_terrain.Count < 4) return null;

        var prevTileOne = _terrain[_terrain.Count - 1];
        var prevTileTwo = _terrain[_terrain.Count - 2];
        var prevTileThree = _terrain[_terrain.Count - 3];
        var prevTileFour = _terrain[_terrain.Count - 4];

        var prevTileHeightOne = _terrainTileGenerator.GetHeightOffset(prevTileOne.name);
        var prevTileHeightTwo = _terrainTileGenerator.GetHeightOffset(prevTileTwo.name);
        var prevTileHeightThree = _terrainTileGenerator.GetHeightOffset(prevTileThree.name);
        var prevTileHeightFour = _terrainTileGenerator.GetHeightOffset(prevTileFour.name);

        for (int i = 0; i < transform.childCount; i++)
        {
            var collectibleInstance = transform.GetChild(i);
            if (Vector3.Distance(collectibleInstance.transform.position, prevTileOne.transform.position) < 10)
            {
                return null;
            }
        }

        //CROSS COLLECTIBLE
        // if (prevTileHeightOne <= 0 && prevTileHeightTwo == 0 && prevTileHeightThree == 0)
        // {
        //     if (prevTileOne.name.Contains("grind")
        //     || prevTileTwo.name.Contains("up-start 0.0")
        //     || prevTileOne.name.Contains("fence")
        //     )
        //     {
        //         prevTileHeightTwo -= 0.5f;
        //     }

        //     var pos = prevTileOne.transform.position;

        //     pos = new Vector3(pos.x, pos.y + _terrainTileGenerator.GetTerrainHeight(2 - prevTileHeightTwo), pos.z);
        //     selectedCollectibles.Add(new GameObjectWithPosition(crossCollectible, pos));
        // }

        //UP SKEW COLLECTIBLE
        if ((((prevTileTwo.name.Contains("bump-end") || prevTileTwo.name.Contains("up-start 0.0")))
            || (prevTileOne.name.Contains("hole"))
            || (prevTileTwo.name.Contains("up-start 0.0"))
            || (prevTileOne.name.Contains("downstairs") && prevTileHeightTwo == 0))
            && prevTileHeightTwo >= 0
            && prevTileHeightThree >= 0
            && prevTileHeightFour >= 0
        )
        {
            float heightVariation = -prevTileHeightTwo;

            if (prevTileTwo.name.Contains("up-start 0.0"))
            {
                heightVariation = 0.5f;
            }

            if ((prevTileOne.name.Contains("hole")))
            {
                heightVariation += prevTileHeightTwo;
            }

            var pos = prevTileOne.transform.position;

            pos = new Vector3(pos.x, pos.y + _terrainTileGenerator.GetTerrainHeight(1.5f + heightVariation), pos.z);
            selectedCollectibles.Add(new GameObjectWithPosition(upSkewCollectible, pos));
            selectedCollectibles.Add(new GameObjectWithPosition(upSkewThreeCollectible, pos));
            selectedCollectibles.Add(new GameObjectWithPosition(cubeCollectible, pos));
        }

        //JUMP COLLECTIBLE
        if ((prevTileOne.name.Contains("hole-end")
        || prevTileOne.name.Contains("fence")
        || prevTileOne.name.Contains("grind-start")
        || prevTileOne.name.Contains("bump-start")
        || prevTileOne.name.Contains("down-end 0.0"))
        && prevTileHeightTwo >= 0
        )
        {
            var pos = prevTileOne.transform.position;
            var posTwo = prevTileTwo.transform.position;

            if (prevTileOne.name.Contains("fence") || prevTileOne.name.Contains("hole-end"))
            {
                posTwo.x = pos.x;
            }

            pos = new Vector3((pos.x + posTwo.x) / 2, pos.y + _terrainTileGenerator.GetTerrainHeight(2f), pos.z);
            selectedCollectibles.Add(new GameObjectWithPosition(jumpCollectible, pos));
            selectedCollectibles.Add(new GameObjectWithPosition(jumpFiveCollectible, pos));
        }

        //LINE COLLECTIBLE
        // if (prevTileTwo.name.Contains("plain 0.0") && prevTileOne.name.Contains("plain 0.0"))
        // {
        //     var pos = prevTileOne.transform.position;
        //     pos = new Vector3(pos.x, pos.y + _terrainTileGenerator.GetTerrainHeight(1.11f), pos.z);
        //     selectedCollectibles.Add(new GameObjectWithPosition(triangleCollectible, pos));
        // }

        //FOLLOW COLLECTIBLE
        if (prevTileTwo.name.Contains("plain 0.0") && prevTileOne.name.Contains("plain 0.0") && prevTileThree.name.Contains("plain 0.0") && prevTileFour.name.Contains("plain 0.0"))
        {
            var pos = prevTileTwo.transform.position;
            // if (prevTileOne.name.Contains("grind")) prevTileHeightOne -= 1.0f;

            pos = new Vector3(pos.x, pos.y + _terrainTileGenerator.GetTerrainHeight(1.11f), pos.z);
            selectedCollectibles.Add(new GameObjectWithPosition(followCollectible, pos));
            selectedCollectibles.Add(new GameObjectWithPosition(followThreeCollectible, pos));
        }

        //DOWN SKEW COLLECTIBLE
        if (((prevTileTwo.name.Contains("hole-start") && prevTileHeightTwo <= -1f)
        || (prevTileThree.name.Contains("bump-end") && prevTileHeightThree <= -1f)
        || (prevTileTwo.name.Contains("hole ") && prevTileHeightThree <= -1f)
        )
        && !prevTileOne.name.Contains("hole")
        && prevTileHeightOne <= 0
        )
        {
            var pos = prevTileOne.transform.position;

            float heightOffsetvariation = 0;
            if (prevTileOne.name.Contains("grind"))
            {
                heightOffsetvariation = 1.5f;
            }

            pos = new Vector3(pos.x, pos.y + _terrainTileGenerator.GetTerrainHeight(1.5f) + heightOffsetvariation, pos.z);
            selectedCollectibles.Add(new GameObjectWithPosition(downSkewCollectible, pos));
            selectedCollectibles.Add(new GameObjectWithPosition(downSkewThreeCollectible, pos));
        }

        if (selectedCollectibles.Count == 0) return null;

        return selectedCollectibles[Random.Range(0, selectedCollectibles.Count)];
    }
#nullable disable

}
