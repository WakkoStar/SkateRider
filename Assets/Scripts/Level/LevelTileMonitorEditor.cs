using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_EDITOR
[CustomEditor(typeof(LevelTileMonitor))]
[CanEditMultipleObjects]
public class LevelTileMonitorEditor : Editor
{
    SerializedProperty tiles;

    private void OnEnable()
    {
        tiles = serializedObject.FindProperty("tiles");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        serializedObject.Update();
        (target as LevelTileMonitor).InitAllTiles();

    }
}
#endif
