using System;
using UnityEngine;

[Serializable]
public class Tile
{
    public TileToggle[] tileToggles;
    public GameObject obj;
    public bool shouldBeInSafeZone;
    public bool canLandingOn;
    public bool isSafe;
}