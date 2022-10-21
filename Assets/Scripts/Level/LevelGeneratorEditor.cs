using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_EDITOR
[CustomEditor(typeof(LevelGenerator))]
[CanEditMultipleObjects]
public class LevelGeneratorEditor : Editor
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
        (target as LevelGenerator).InitTiles();
    }
}
#endif
