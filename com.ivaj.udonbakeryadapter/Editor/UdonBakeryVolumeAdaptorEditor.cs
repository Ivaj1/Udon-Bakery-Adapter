using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if !COMPILER_UDONSHARP && UNITY_EDITOR
using UnityEditor;
#endif




[CustomEditor(typeof(UdonBakeryVolumeAdaptor))]
public class UdonBakeryVolumeAdaptorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        UdonBakeryVolumeAdaptor script = (UdonBakeryVolumeAdaptor)target;

        EditorGUILayout.Space();

        if (GUILayout.Button("Copy Volume Data"))
        {
            //  UdonBakeryVolumeAdaptor source = null;
            //  source = (UdonBakeryVolumeAdaptor)EditorGUILayout.ObjectField("Source Volume", source, typeof(UdonBakeryVolumeAdaptor), true);

            script.init();
            script.CopyVolumeData();
            
        }
    }

      [MenuItem("Bakery/BakeryAdapter/AddVolumenAdapter")]
        private static void AddBakeryAdapter()
        {
               
    
         
          GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();

           foreach (var obj in allObjects)
           {
              var behaviors = obj.GetComponents<MonoBehaviour>();

             foreach (var behavior in behaviors)
             {
                if (behavior.GetType().Name == "BakeryVolume")
                {
                    Debug.Log($"Found {"BakeryVolume"} in GameObject '{obj.name}'");
                    if (obj.GetComponent<UdonBakeryVolumeAdaptor>() == null)
                    {
                        obj.AddComponent<UdonBakeryVolumeAdaptor>();
                        obj.GetComponent<UdonBakeryVolumeAdaptor>().CopyVolumeData();
                    }
                }
             }
           }


    
        }

}



