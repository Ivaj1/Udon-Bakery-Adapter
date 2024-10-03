#if !COMPILER_UDONSHARP && UNITY_EDITOR
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using System.Collections.Generic;
using UdonSharpEditor;
using UnityEditor;


[ExecuteInEditMode]
[CustomEditor(typeof(BakeryAdapter))]
public class BakeryAdapterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target)) return;
        
       // DrawDefaultInspector();

        BakeryAdapter adapter = (BakeryAdapter)target;

        if (GUILayout.Button("Setup"))
        {
            adapter.Setup();
        }
        if (GUILayout.Button("Apply Data"))
        {
            adapter.ApplyRendererData();
        }
        if (GUILayout.Button("Clean Data"))
        {
            adapter.CleanData();
        }
        
    }
   
    private void OnEnable()
    {
        if (Application.isPlaying) return;
        BakeryAdapter adapter = (BakeryAdapter)target;
        adapter.Setup();
    }

    [MenuItem("Bakery/BakeryAdapter/AddBakeryAdapter")]
    private static void AddBakeryAdapter()
    {
        if (Application.isPlaying) return;
        GameObject obj = new GameObject("BakeryAdapter");
        obj.AddComponent<BakeryAdapter>();
        obj.GetComponent<BakeryAdapter>().Setup();
        EditorUtility.SetDirty(obj);
    }
}

public static class BakeryAdapterExtensions
{
    public static void Setup(this BakeryAdapter adapter)
    {
        if (Application.isPlaying) return;
        ftRenderLightmap bakery = ftRenderLightmap.instance != null ? ftRenderLightmap.instance : new ftRenderLightmap();
        bakery.LoadRenderSettings();
        var storage = bakery.renderSettingsStorage; // Variable local para el storage
        
        int count = storage.bakedRenderers.Count;
        adapter.SetGlobalIDs(new int[count]);
        adapter.SetLightmapIndices(new int[count]);
        adapter.SetLightmapScaleOffsets(new Vector4[count]);
        adapter.SetRnm0Textures(new Texture2D[count]);
        adapter.SetRnm1Textures(new Texture2D[count]);
        adapter.SetRnm2Textures(new Texture2D[count]);
        adapter.SetLightmapModes(new float[count]);
        adapter.SetBakedRenderers(new Renderer[count]);

        CollectRendererData(adapter, storage); // Pasar el storage como parámetro
        EditorUtility.SetDirty(adapter.gameObject);
        
       

    }

    private static void CollectRendererData(BakeryAdapter adapter, ftLightmapsStorage storage) 
    {
        var emptyVec4 = new Vector4(1, 1, 0, 0);
        var lightmapDataDict = new Dictionary<int, int>();

        for (int i = 0; i < storage.bakedRenderers.Count; i++)
        {
            var renderer = storage.bakedRenderers[i];
            if (renderer == null) continue;

            adapter.GetBakedRenderers()[i] = renderer;

            int id = storage.bakedIDs[i];
            int globalID = (id < 0 || id >= storage.idremap.Length) ? id : storage.idremap[id];
            adapter.GetGlobalIDs()[i] = globalID;

            adapter.GetLightmapScaleOffsets()[i] = id < 0 ? emptyVec4 : storage.bakedScaleOffset[i];
            adapter.GetLightmapIndices()[i] = globalID;

            if (globalID >= 0 && globalID < storage.rnmMaps0.Count && !lightmapDataDict.ContainsKey(globalID))
            {
                lightmapDataDict[globalID] = globalID;
                adapter.GetRnm0Textures()[globalID] = storage.rnmMaps0[globalID];
                adapter.GetRnm1Textures()[globalID] = storage.rnmMaps1[globalID];
                adapter.GetRnm2Textures()[globalID] = storage.rnmMaps2[globalID];
                adapter.GetLightmapModes()[globalID] = storage.mapsMode[globalID];
            }
        }
    }

    public static void CleanData(this BakeryAdapter adapter)
    {
        for (int i = 0; i < adapter.GetBakedRenderers().Length; i++)
        {
            var renderer = adapter.GetBakedRenderers()[i];
            renderer.SetPropertyBlock(null);
        }
    }
}




#endif

