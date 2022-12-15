using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Linq;

public class TileSideSelector
{
    public List<Tile> tiles = new List<Tile>();
    GameObject DefaultTile;

    public TileSideSelector(List<Tile> tiles, GameObject DefaultTile)
    {
        this.tiles = tiles;
        this.DefaultTile = DefaultTile;
    }

    public GameObject ChooseSideTile(GameObject baseTile, List<SideTile> sideTiles, List<GameObject> previousSideTiles)
    {
        var selectedTiles = new List<GameObject>();
        var prevSideTile = previousSideTiles[previousSideTiles.Count - 1];

        //SIDE TILE IS SPECIFIED
        foreach (var tile in sideTiles)
        {
            var selection = SelectTile(tile, baseTile, prevSideTile);
            if (selection != null) selectedTiles.Add(selection);
        }

        if (selectedTiles.Count > 0)
            return selectedTiles[UnityEngine.Random.Range(0, selectedTiles.Count)]; ;

        return DefaultTile;
    }

    private GameObject SelectTile(SideTile tile, GameObject prevTile, GameObject prevSideTile)
    {
        //FIRST SELECTION
        foreach (var tileToSelect in tile.selection)
        {
            if (tileToSelect.shouldBeNextTo && prevTile.name.Contains(tileToSelect.name))
            {
                //NEED A SECOND SELECTION
                if (tile.shouldHaveSideSelection)
                {
                    foreach (var sideTileToSelect in tile.sideSelection)
                    {
                        if (sideTileToSelect.shouldBeNextTo && prevSideTile.name.Contains(sideTileToSelect.name))
                        {
                            return tile.obj;
                        }
                    }
                }
                else
                {
                    return tile.obj;
                }
            }
        }

        return null;
    }
}
