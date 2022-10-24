using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Globalization;

public class TileSideSelector
{
    public List<Tile> tiles = new List<Tile>();
    GameObject DefaultTile;

    public TileSideSelector(List<Tile> tiles, GameObject DefaultTile)
    {
        this.tiles = tiles;
        this.DefaultTile = DefaultTile;
    }

    public GameObject ChooseSideTile(GameObject baseTile, List<GameObject> previousTiles)
    {
        var match = Regex.Match(baseTile.name, @"([-+]?[0-9]*\.?[0-9]+)");
        var height = "0.0";
        if (match.Success)
            height = match.Groups[1].Value;

        var prevTile = previousTiles[previousTiles.Count - 1];
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

        if (baseTile.name.Contains("hole"))
            return DefaultTile;

        if (baseTile.name.Contains("plain 0.0"))
            return DefaultTile;

        if (candidates.Count == 0)
            candidates.Add(tiles.Find(t => t.obj.name.Contains("plain 0.0")).obj);

        return candidates[UnityEngine.Random.Range(0, candidates.Count)];
    }

    public GameObject SelectTile(Tile tile, GameObject prevTile)
    {
        foreach (var tileToSelect in tile.selection)
        {
            if (tileToSelect.shouldBeNextTo && prevTile.name.Contains(tileToSelect.name))
            {
                return tile.obj;
            }
        }

        return null;

    }
}
