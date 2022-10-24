using System;
using UnityEngine;

[Serializable]
public class TileSelection
{
    public TileSelection(string name, bool shouldBeNextTo)
    {
        this.name = name;
        this.shouldBeNextTo = shouldBeNextTo;
    }
    [HideInInspector] public string name;
    public bool shouldBeNextTo;
}