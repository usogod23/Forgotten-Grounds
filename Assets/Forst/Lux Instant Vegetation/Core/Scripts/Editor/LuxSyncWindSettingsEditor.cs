using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace LuxInstantVegetation
{

    [CustomEditor(typeof(LuxSyncWindSettings))]
    public class LuxSyncWindSettingsEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            LuxSyncWindSettings luxSyncWindSettings = (LuxSyncWindSettings)target;

            var materials = serializedObject.FindProperty("materials");
            if (materials.arraySize == 0)
            {
                // EditorGUILayout.PropertyField(serializedObject.FindProperty("materials")); // works
                if (GUILayout.Button("Step 1: Get Materials"))
                {
                    luxSyncWindSettings.GetMaterials();
                }
            }

            else
            {
                if (!luxSyncWindSettings._DontFetchValues)
                {
                    EditorGUILayout.BeginHorizontal();
                        if (GUILayout.Button("Step 2: Get values from Master Material"))
                        {
                            luxSyncWindSettings.GetValuesFromMaterialEdior();
                            luxSyncWindSettings._DontFetchValues = true;
                        }

                        if (GUILayout.Button("Skip this step"))
                        {
                            luxSyncWindSettings._DontFetchValues = true;
                        }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.PropertyField(serializedObject.FindProperty("_MasterMaterial"));
                    GUILayout.Space(4);
                }
                
                else
                {
                    EditorGUILayout.BeginHorizontal();
                        if (GUILayout.Button("Get values from Master Material"))
                        {
                            luxSyncWindSettings.GetValuesFromMaterialEdior();
                            luxSyncWindSettings._DontFetchValues = true;
                        }
                        if (GUILayout.Button("Sync Shaders"))
                        {
                            luxSyncWindSettings.SyncShaders();
                        }
                    EditorGUILayout.EndHorizontal();
                }

                if (luxSyncWindSettings._DontFetchValues)
                {

                    DrawDefaultInspector();

                    if (GUILayout.Button("Sync"))
                    {
                        luxSyncWindSettings.SyncMaterials();
                    }

                }

            }

            serializedObject.ApplyModifiedProperties();
        }
    }

}