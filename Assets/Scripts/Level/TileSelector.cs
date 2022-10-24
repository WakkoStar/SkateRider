using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileSelector
{
    public List<Tile> tiles = new List<Tile>();
    GameObject DefaultTile;
    TileOccurenceHandler tileOccurenceHandler;

    public TileSelector(List<Tile> tiles, GameObject DefaultTile)
    {
        this.tiles = tiles;
        this.DefaultTile = DefaultTile;
        tileOccurenceHandler = new TileOccurenceHandler(tiles);
    }

    /**
    Select tile under conditions: 
    - previous tiles
    - current level height
    - need to have a safe zone or not
    */
    public GameObject ChooseTile(List<GameObject> previousTiles, float currentLevelHeight, bool shouldBeInSafeZone)
    {
        var selectedTiles = new List<GameObject>();
        var prevTile = previousTiles[previousTiles.Count - 1];

        //TILE SELECTION
        foreach (var tile in tiles)
        {
            var selection = SelectTile(tile, prevTile, previousTiles);

            if (selection != null)
            {
                bool isSafeFirst = prevTile.name.Contains(tile.obj.name) && tile.isSafeFirst;
                bool isSafe = isSafeFirst || tile.isSafe;

                if (shouldBeInSafeZone && !isSafe)
                {
                    continue;
                }
                // A SEPARER
                // if (prevTileIndex != landingTileIndex - 2)
                // {
                //     selectedTiles.RemoveAll(t => t.name.Contains("grind-start") && t.name.Contains(" +") && t.name.Contains(" -"));
                // }

                selectedTiles.Add(selection);
            }
        }

        // FLATTEN CURVES
        if (currentLevelHeight < -4)
        {
            selectedTiles.RemoveAll(t => t.name.Contains(" -"));
        }
        else if (currentLevelHeight > 4)
        {
            selectedTiles.RemoveAll(t => t.name.Contains(" +"));
        }

        //SPREAD
        selectedTiles = tileOccurenceHandler.SpreadTileCandidate(selectedTiles);

        var defaultTileIndex = selectedTiles.FindIndex(c => c.name.Contains(DefaultTile.name));
        if (defaultTileIndex != -1 && UnityEngine.Random.Range(0, 100f) > 90)
        {
            tileOccurenceHandler.AddOccurenceOnTile(selectedTiles[defaultTileIndex].name);
            return DefaultTile;
        }

        //FALLBACK
        if (selectedTiles.Count == 0)
            selectedTiles.Add(DefaultTile);

        var choosenTile = selectedTiles[UnityEngine.Random.Range(0, selectedTiles.Count)];
        tileOccurenceHandler.AddOccurenceOnTile(choosenTile.name);
        return choosenTile;
    }



    public GameObject SelectTile(Tile newTile, GameObject prevTile, List<GameObject> previousTiles)
    {
        if (newTile.shouldBeInSafeZone && IsSafeZone(previousTiles, 3))
        {
            return newTile.obj;
        }

        foreach (var tileToSelect in newTile.selection)
        {
            if (tileToSelect.shouldBeNextTo && prevTile.name.Contains(tileToSelect.name))
            {
                return newTile.obj;
            }
        }

        return null;
    }

    /*
    Check if zone is safe depending on previous Tiles
    If one of them isn't safe, the zone too. Otherwise, this is safe;
    **/
    bool IsSafeZone(List<GameObject> previousTiles, int safeZoneLength)
    {
        for (int i = 0; i < safeZoneLength; i++)
        {
            bool isFirstTile = i == 0;
            var prevTile = previousTiles[previousTiles.Count - (1 + i)];
            foreach (var tile in tiles)
            {
                bool isFirstTileSafe = isFirstTile && (tile.isSafeFirst || tile.isSafe) && prevTile.name.Contains(tile.obj.name);
                bool isNextTilesSafe = !isFirstTile && tile.isSafe && prevTile.name.Contains(tile.obj.name);

                if (!isFirstTileSafe && !isNextTilesSafe)
                {
                    return false;
                }
            }
        }
        return true;
    }
}
