using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using System.Collections.Generic;

#if !COMPILER_UDONSHARP && UNITY_EDITOR
using UnityEditor;
#endif
public class BakeryAdapter : UdonSharpBehaviour
{
    private int[] globalIDs;
    private int[] lightmapIndices;
    private Vector4[] lightmapScaleOffsets;
    private Texture2D[] rnm0Textures;
    private Texture2D[] rnm1Textures;
    private Texture2D[] rnm2Textures;
    private float[] lightmapModes;
    private Renderer[] bakedRenderers;

    // Property to get the number of lightmaps
    

    [ExecuteInEditMode]
    private void OnEnable()
    {
#if !COMPILER_UDONSHARP && UNITY_EDITOR
        Setup();
#endif
    }

    private void Start()
    {
        ApplyRendererData();
    }

    public void ApplyRendererData()
    {
        for (int i = 0; i < bakedRenderers.Length; i++)
        {
            var renderer = bakedRenderers[i];
            int lightmapIndex = lightmapIndices[i];
            renderer.lightmapIndex = lightmapIndex;
            renderer.lightmapScaleOffset = lightmapScaleOffsets[i];

            if (lightmapIndex >= 0 && lightmapIndex < rnm0Textures.Length)
            {
                var prop = new MaterialPropertyBlock();
                if (rnm0Textures[lightmapIndex] != null && rnm1Textures[lightmapIndex] != null && rnm2Textures[lightmapIndex] != null)
                {
                    prop.SetTexture("_RNM0", rnm0Textures[lightmapIndex]);
                    prop.SetTexture("_RNM1", rnm1Textures[lightmapIndex]);
                    prop.SetTexture("_RNM2", rnm2Textures[lightmapIndex]);
                }
                prop.SetFloat("bakeryLightmapMode", lightmapModes[lightmapIndex]);
                renderer.SetPropertyBlock(prop);
            }
        }
    }

#if !COMPILER_UDONSHARP && UNITY_EDITOR
    private ftLightmapsStorage storage;
    public int LightmapCount
    {
        get
        {
            HashSet<int> uniqueLightmaps = new HashSet<int>();
            foreach (int index in lightmapIndices)
            {
                if (index >= 0)
                {
                    uniqueLightmaps.Add(index);
                }
            }
            return uniqueLightmaps.Count;
        }
    }
    public void Setup()
    {
        ftRenderLightmap bakery = ftRenderLightmap.instance != null ? ftRenderLightmap.instance : new ftRenderLightmap();
        bakery.LoadRenderSettings();
        storage = bakery.renderSettingsStorage;

        int count = storage.bakedRenderers.Count;
        globalIDs = new int[count];
        lightmapIndices = new int[count];
        lightmapScaleOffsets = new Vector4[count];
        rnm0Textures = new Texture2D[count];
        rnm1Textures = new Texture2D[count];
        rnm2Textures = new Texture2D[count];
        lightmapModes = new float[count];
        bakedRenderers = new Renderer[count];

        CollectRendererData(storage);
        EditorUtility.SetDirty(this.gameObject);
    }

    private void CollectRendererData(ftLightmapsStorage storage)
    {
        var emptyVec4 = new Vector4(1, 1, 0, 0);
        var lightmapDataDict = new Dictionary<int, int>();

        for (int i = 0; i < storage.bakedRenderers.Count; i++)
        {
            var renderer = storage.bakedRenderers[i];
            if (renderer == null) continue;

            bakedRenderers[i] = renderer;

            int id = storage.bakedIDs[i];
            int globalID = (id < 0 || id >= storage.idremap.Length) ? id : storage.idremap[id];
            globalIDs[i] = globalID;

            lightmapScaleOffsets[i] = id < 0 ? emptyVec4 : storage.bakedScaleOffset[i];
            lightmapIndices[i] = globalID;

            if (globalID >= 0 && globalID < storage.rnmMaps0.Count && !lightmapDataDict.ContainsKey(globalID))
            {
                lightmapDataDict[globalID] = globalID;
                rnm0Textures[globalID] = storage.rnmMaps0[globalID];
                rnm1Textures[globalID] = storage.rnmMaps1[globalID];
                rnm2Textures[globalID] = storage.rnmMaps2[globalID];
                lightmapModes[globalID] = storage.mapsMode[globalID];
            }
        }
    }

    public void CleanData()
    {
        for (int i = 0; i < bakedRenderers.Length; i++)
        {
            var renderer = bakedRenderers[i];
            renderer.SetPropertyBlock(null);
        }
    }
#endif
}


//made by ivaj