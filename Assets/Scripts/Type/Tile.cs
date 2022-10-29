using System;
using UnityEngine;

[Serializable]
public class Tile
{
    public GameObject obj;
    public bool shouldBeInSafeZone;
    public bool shouldBeCompleted;
    public bool isSafe;
    public bool isSafeFirst;
    public TileSelection[] selection;
}