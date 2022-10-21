using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets;

public class ModuleChunkSystem : MonoBehaviour
{

    //OPERATION
    public struct Operation
    {
        public Operation(GameObject module, AsyncOperationHandle<GameObject> operationHandle)
        {
            this.module = module;
            this.operationHandle = operationHandle;
        }
        public GameObject module { get; set; }
        public AsyncOperationHandle<GameObject> operationHandle { get; set; }
    }


    //SETTINGS
    [SerializeField] private List<AssetReference> assetReferences;
    [SerializeField] private List<GameObject> modules;
    [SerializeField] private GameObject Player;
    [SerializeField] private float chunkLength = 109.81f;

    //STATE
    private int _chunkIndex = -1;
    private List<Operation> _currentModulesOperation = new List<Operation>();

    private void Update()
    {
        if (_currentModulesOperation.Count >= 3)
        {
            ReleaseModuleAsset(_currentModulesOperation[0]);
            _currentModulesOperation.RemoveAt(0);
        }

        int currentChunkIndex = (int)Mathf.Round(Player.transform.position.x / chunkLength);
        if (currentChunkIndex != _chunkIndex)
        {
            InstantiateChunk(chunkLength, currentChunkIndex);
            _chunkIndex = currentChunkIndex;
        }
    }

    public async void InstantiateChunk(float moduleLength, int chunkIndex)
    {
        Vector3 pos = new Vector3(moduleLength * chunkIndex, 0, 0);
        int moduleIndex = (int)Random.Range(0, assetReferences.Count);
        var operationHandle = await InstantiateModuleAsset(moduleIndex, pos);
        _currentModulesOperation.Add(new Operation(operationHandle.Result, operationHandle));
    }

    public async Task<AsyncOperationHandle<GameObject>> InstantiateModuleAsset(int moduleIndex, Vector3 pos)
    {
        var operationHandle = assetReferences[moduleIndex].InstantiateAsync(pos + transform.position, modules[moduleIndex].transform.rotation, transform);
        await operationHandle.Task;
        return operationHandle;
    }

    public void ReleaseModuleAsset(Operation moduleOperation)
    {
        Addressables.ReleaseInstance(moduleOperation.operationHandle);
        Addressables.ReleaseInstance(moduleOperation.module);
    }
}
