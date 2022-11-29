using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TileComponent
{
    public GameObject Tile;
    public bool shouldBeInMeshCombiner;
}

public class TileMeshCombinerInfo : MonoBehaviour
{
    public TileComponent[] tileComponents;
}
