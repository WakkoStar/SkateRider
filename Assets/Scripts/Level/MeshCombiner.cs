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
            meshObject.gameObject.SetActive(false);
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
        gameObject.gameObject.SetActive(true);
    }


    /*
    Combine layers is used to gameobject with childs named layers
    Each layers have its mesh, we have to combine child of each layers
    **/
    public static void CombineLayers(List<GameObject> layers)
    {
        foreach (var layer in layers)
        {
            var layerGameObjs = new List<GameObject>();
            for (int i = 0; i < layer.transform.childCount; i++)
            {
                var layerComp = layer.transform.GetChild(i).gameObject;
                layerGameObjs.Add(layerComp);
            }

            if (layerGameObjs.Count == 0) continue;

            List<CombineInstance> meshCombine = MeshCombiner.SetMeshCombine(layerGameObjs);
            MeshCombiner.ApplyMeshCombine(layer, meshCombine);
        }
    }
}
