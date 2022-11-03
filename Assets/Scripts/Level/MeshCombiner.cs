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

        // var meshCollider = gameObject.GetComponent<MeshCollider>();
        // meshCollider.sharedMesh.Clear();
        // meshCollider.sharedMesh = new Mesh();
        // meshCollider.sharedMesh.CombineMeshes(meshCombine.ToArray());
    }


    /*
    Combine layers is used to gameobject with childs named layers
    Each layers have its mesh, we have to combine child of each layers
    **/
    public static void CombineLayers(Dictionary<string, List<GameObject>> meshCombiner, Transform transform)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            var layer = transform.GetChild(i).gameObject;
            var layerComponents = meshCombiner[layer.name];
            List<CombineInstance> meshCombine = MeshCombiner.SetMeshCombine(layerComponents);
            MeshCombiner.ApplyMeshCombine(layer, meshCombine);
        }
        // foreach (var layer in meshCombiner)
        // {

        //     var layerGameObjs = new List<GameObject>();
        //     for (int i = 0; i < layer.transform.childCount; i++)
        //     {
        //         var layerComp = layer.transform.GetChild(i).gameObject;
        //         layerGameObjs.Add(layerComp);
        //     }

        //     if (layerGameObjs.Count == 0) continue;

        //     List<CombineInstance> meshCombine = MeshCombiner.SetMeshCombine(layerGameObjs);
        //     MeshCombiner.ApplyMeshCombine(layer, meshCombine);
        // }
    }
}
