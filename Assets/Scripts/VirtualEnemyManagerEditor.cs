#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(VirtualEnemyManager))]
public class VirtualEnemyManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        VirtualEnemyManager spawner = (VirtualEnemyManager)target;

        DrawDefaultInspector();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Debug Info", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Active Enemies: " + spawner.GetActiveEnemyCount());
        EditorGUILayout.LabelField("Current Wave: " + spawner.GetCurrentWave());
    }
}
#endif