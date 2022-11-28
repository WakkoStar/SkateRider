using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

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
        var isPrevTileShouldBeCompleted = tiles.Find(t => t.shouldBeCompleted && prevTile.name.Contains(t.obj.name)) != null;

        //TILE SELECTION
        foreach (var tile in tiles)
        {
            GameObject selection;

            if (shouldBeInSafeZone && !tile.isSafe && !tile.isSafeFirst)
            {
                if (isPrevTileShouldBeCompleted)
                {
                    var completion = SelectTile(tile, prevTile, previousTiles);
                    if (completion != null) selectedTiles.Add(completion);
                }
                continue;
            }

            if (tile.shouldBeInSafeZone && IsSafeZone(previousTiles, 3) && !isPrevTileShouldBeCompleted)
            {
                selection = tile.obj;
            }
            else
            {
                selection = SelectTile(tile, prevTile, previousTiles);
            }

            if (selection != null)
            {
                selectedTiles.Add(selection);
            }
        }

        // FLATTEN CURVES
        if (currentLevelHeight < -4 && !isPrevTileShouldBeCompleted)
        {
            selectedTiles.RemoveAll(t => t.name.Contains(" -"));
        }
        else if (currentLevelHeight > 4)
        {
            selectedTiles.RemoveAll(t => t.name.Contains(" +"));
        }

        //SPREAD
        selectedTiles = tileOccurenceHandler.SpreadTileCandidate(selectedTiles);

        // var defaultTileIndex = selectedTiles.FindIndex(c => c.name.Contains(DefaultTile.name));
        // if (defaultTileIndex != -1 && UnityEngine.Random.Range(0, 100f) > 90)
        // {
        //     tileOccurenceHandler.AddOccurenceOnTile(selectedTiles[defaultTileIndex].name);
        //     return DefaultTile;
        // }

        //FALLBACK
        if (selectedTiles.Count == 0)
            selectedTiles.Add(DefaultTile);

        var choosenTile = selectedTiles[UnityEngine.Random.Range(0, selectedTiles.Count)];
        tileOccurenceHandler.AddOccurenceOnTile(choosenTile.name);
        return choosenTile;
    }



    public GameObject SelectTile(Tile newTile, GameObject prevTile, List<GameObject> previousTiles)
    {
        foreach (var tileToSelect in newTile.selection)
        {
            if (tileToSelect.shouldBeNextTo && prevTile.name.Contains(tileToSelect.name))
            {
                return newTile.obj;
            }
        }

        return null;
    }

    bool IsSafeZone(List<GameObject> previousTiles, int safeZoneLength)
    {
        bool[] isPrevTileSafeArray = new bool[safeZoneLength];
        for (int i = 0; i < safeZoneLength; i++)
        {
            var prevTile = previousTiles[previousTiles.Count - (1 + i)];
            var firstTileIndex = safeZoneLength - 1; //the oldest tile index

            foreach (var tile in tiles)
            {
                bool isFirstTileSafe = (tile.isSafeFirst || tile.isSafe) && prevTile.name.Contains(tile.obj.name);
                if (i == firstTileIndex && isFirstTileSafe)
                {
                    isPrevTileSafeArray[firstTileIndex] = true;
                }

                bool isNextTilesSafe = tile.isSafe && prevTile.name.Contains(tile.obj.name);
                if (i < firstTileIndex && isNextTilesSafe)
                {
                    isPrevTileSafeArray[i] = true;
                }
            }
        }
        return Array.TrueForAll(isPrevTileSafeArray, isPrevTileSafe => isPrevTileSafe);
    }
}
