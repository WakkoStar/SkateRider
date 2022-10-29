using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SideTile : IEquatable<SideTile>
{
    public GameObject obj;
    public bool shouldHaveSideSelection;
    public TileSelection[] sideSelection;
    public TileSelection[] selection;

    public bool Equals(SideTile other)
    {
        if (other is null)
            return false;

        return this.obj.name == other.obj.name;
    }

    public override bool Equals(object obj) => Equals(obj as SideTile);
    public override int GetHashCode() => (obj).GetHashCode();
}

