using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshCombiner
{
    public static List<CombineInstance> SetMeshCombine(List<GameObject> meshObjects)
    {
        var meshCombine = new List<CombineInstance>();

        foreach (var meshObject in meshObjects)
        {
            var combine = InitCombineInstance(meshObject.GetComponent<MeshFilter>());
            meshCombine.Add(combine);
            meshObject.GetComponent<MeshCollider>().enabled = false;
        }

        return meshCombine;
    }

    static CombineInstance InitCombineInstance(MeshFilter meshFilter)
    {
        var combineInstance = new CombineInstance();
        combineInstance.mesh = meshFilter.sharedMesh;
        combineInstance.transform = meshFilter.transform.localToWorldMatrix;

        return combineInstance;
    }

    public static void ApplyMeshCombine(GameObject gameObject, List<CombineInstance> meshCombine)
    {

        var meshFilter = gameObject.GetComponent<MeshFilter>();
        meshFilter.mesh.Clear();
        meshFilter.mesh = new Mesh();
        meshFilter.mesh.CombineMeshes(meshCombine.ToArray());

        gameObject.GetComponent<MeshCollider>().sharedMesh = meshFilter.mesh;
    }


    /*
    Combine layers is used to gameobject with childs named layers
    Each layers have its mesh, we have to combine child of each layers
    **/
    public static void CombineLayers(Transform meshCombinerTransform, IEnumerable<TileComponent[]> tiles)
    {
        UpdateLayers(meshCombinerTransform, tiles);
        //EACH LAYERS
        for (int i = 0; i < meshCombinerTransform.childCount; i++)
        {
            var layer = meshCombinerTransform.GetChild(i).gameObject;
            var layerComponents = new List<GameObject>();

            //EACH OBJECT IN LIST
            foreach (var tileComponents in tiles)
            {
                foreach (var tileComponent in tileComponents)
                {
                    if (!tileComponent.shouldBeInMeshCombiner) continue;
                    layerComponents.Add(tileComponent.Tile);
                }
            }

            List<CombineInstance> meshCombine = MeshCombiner.SetMeshCombine(layerComponents);
            MeshCombiner.ApplyMeshCombine(layer, meshCombine);
        }
    }


    private static void UpdateLayers(Transform meshCombinerTransform, IEnumerable<TileComponent[]> tiles)
    {
        //EACH OBJECT IN LIST
        foreach (var tileComponents in tiles)
        {
            foreach (var tileComponent in tileComponents)
            {
                if (!tileComponent.shouldBeInMeshCombiner) continue;

                bool isExistingLayer = false;
                GameObject baseItem = meshCombinerTransform.childCount == 0
                    ? tileComponent.Tile
                    : null;

                //EACH LAYERS
                for (int i = 0; i < meshCombinerTransform.childCount; i++)
                {
                    var layer = meshCombinerTransform.GetChild(i).gameObject;
                    if (layer.GetComponent<MeshCollider>().sharedMaterial.Equals(tileComponent.Tile.GetComponent<MeshCollider>().sharedMaterial))
                    {
                        isExistingLayer = true;
                    }
                    if (!isExistingLayer) baseItem = tileComponent.Tile;
                }

                if (!isExistingLayer)
                {
                    AddNewLayer(baseItem, meshCombinerTransform);
                }
            }
        }
    }

    private static void AddNewLayer(GameObject BaseItem, Transform meshCombinerTransform)
    {
        var layer = new GameObject("Layer " + meshCombinerTransform.childCount);
        layer.transform.parent = meshCombinerTransform;

        layer.AddComponent<MeshFilter>().name = "Layer " + meshCombinerTransform.childCount;
        layer.AddComponent<MeshCollider>().sharedMaterial = BaseItem.GetComponent<MeshCollider>().sharedMaterial;

    }
}