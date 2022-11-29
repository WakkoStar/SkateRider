using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TileOccurenceHandler
{
    public TileOccurenceHandler(List<Tile> tiles)
    {
        InitOccurenceOnTile(tiles);
    }

    private List<TileOccurence> _tileOccurences = new List<TileOccurence>();

    private void InitOccurenceOnTile(List<Tile> tiles)
    {
        for (int i = 0; i < tiles.Count; i++)
        {
            var tileOccurence = new TileOccurence();
            tileOccurence.occurence = 0;
            tileOccurence.tileName = tiles[i].obj.name;
            _tileOccurences.Add(tileOccurence);
        }
    }

    public void AddOccurenceOnTile(string tileName)
    {
        var tileOccurenceIndex = _tileOccurences.FindIndex(t => t.tileName.Contains(tileName));
        var tileOccurence = _tileOccurences[tileOccurenceIndex];
        tileOccurence.occurence += 1;

        _tileOccurences[tileOccurenceIndex] = tileOccurence;
    }

    public List<GameObject> SpreadTileSelection(List<GameObject> selection)
    {

        var tilesNotOccured = _tileOccurences.OrderBy(t => t.occurence);
        var notOccuredCandidates = new List<GameObject>();

        int count = 0;
        foreach (var tile in tilesNotOccured)
        {
            if (count >= 5) continue;

            for (int i = 0; i < selection.Count; i++)
            {
                var selectedTile = selection[i];
                if (selectedTile.name.Contains(tile.tileName))
                {
                    notOccuredCandidates.Add(selectedTile);
                    count++;
                }
            }
            // var notOccuredCandidate = candidates.Find(c => c.name.Contains(tile.tileName));
            // if (notOccuredCandidate)
            // {

            // }

        }

        return notOccuredCandidates;
    }
}
