
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

#if !COMPILER_UDONSHARP && UNITY_EDITOR
using UnityEditor;
#endif

#if !COMPILER_UDONSHARP && UNITY_EDITOR
    [CustomEditor(typeof(BakeryAdapter))]
    public class BakeryAdapterEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            BakeryAdapter myScript = (BakeryAdapter)target;

            EditorGUILayout.LabelField("Number of Lightmaps", myScript.LightmapCount.ToString());

            if (GUILayout.Button("Setup"))
            {
                myScript.Setup();
                EditorUtility.SetDirty(myScript);
            }

            if (GUILayout.Button("Apply Renderer Data"))
            {
                myScript.ApplyRendererData();
            }

            if (GUILayout.Button("Clean Property Blocks"))
            {
                myScript.CleanData();
            }
        }

        [MenuItem("Bakery/BakeryAdapter/AddBakeryAdapter")]
        private static void AddBakeryAdapter()
        {
            GameObject obj = new GameObject("BakeryAdapter");
            obj.AddComponent<BakeryAdapter>();
            obj.GetComponent<BakeryAdapter>().Setup();
            EditorUtility.SetDirty(obj);
        }
    }
#endif

