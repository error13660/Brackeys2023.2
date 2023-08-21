using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MeshHelper))]
public class MeshHelperInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        MeshHelper meshHelper = (MeshHelper)target;

        if (GUILayout.Button("Find mesh"))
        {
            EditorGUI.BeginChangeCheck();
            meshHelper.FindMesh();
            EditorGUI.EndChangeCheck();
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
        }

        if (GUILayout.Button("Bake"))
        {
            EditorGUI.BeginChangeCheck();
            meshHelper.Bake();
            EditorGUI.EndChangeCheck();
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
        }
    }
}
