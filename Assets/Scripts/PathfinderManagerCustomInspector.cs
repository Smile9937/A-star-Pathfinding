using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
[CustomEditor(typeof(PathfinderManager))]
public class PathfinderManagerCustomInspector : Editor
{
    PathfinderManager pathfinderManager;

    private void OnEnable()
    {
        pathfinderManager = target as PathfinderManager;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUILayout.Space(EditorGUIUtility.singleLineHeight);

        if(GUILayout.Button("Generate Grid"))
        {
            PathfinderManager.InvokeGenerateGrid();
        }
    }
}
#endif