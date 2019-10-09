using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

[CustomEditor(typeof(NodeGenerator))]
public class NodeGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        EditorGUI.BeginChangeCheck();

        NodeGenerator nodeGenerator = (NodeGenerator)target;

        if (GUILayout.Button("Find Neighbours"))
        {
            nodeGenerator.GetEdges();
        }

        if (GUILayout.Button("Delete All Connections"))
        {
            nodeGenerator.DeleteAllConnections();
        }

        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            if (!Application.isPlaying)
            {
                EditorUtility.SetDirty(target);

                Node[] nodes = FindObjectsOfType<Node>();
                foreach (Node n in nodes)
                {
                    EditorUtility.SetDirty(n);
                }

                EditorApplication.MarkSceneDirty();
            }
        }
    }
}
