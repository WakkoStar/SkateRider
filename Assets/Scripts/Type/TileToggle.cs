using System;
using UnityEngine;

[Serializable]
public class TileToggle
{
    public TileToggle(string name, bool canBeBefore)
    {
        this.tileName = name;
        this.canBeBefore = canBeBefore;
    }
    [HideInInspector] public string tileName;
    public bool canBeBefore;
}