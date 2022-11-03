using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileMeshCombinerInfo : MonoBehaviour
{
    [Serializable]
    public struct TileComponent
    {

        public GameObject Tile;
        public bool shouldBeInMeshCombiner;
    }

    public TileComponent[] tileComponents;
}
