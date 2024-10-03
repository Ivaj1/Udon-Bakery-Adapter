using UdonSharp;
using UnityEditor;
using UnityEngine;
using VRC;
using VRC.SDKBase;
using VRC.Udon;

public class BakeryAdapter : UdonSharpBehaviour
{
    [SerializeField] private int[] globalIDs;
    [SerializeField] private int[] lightmapIndices;
    [SerializeField] private Vector4[] lightmapScaleOffsets;
    [SerializeField] private Texture2D[] rnm0Textures;
    [SerializeField] private Texture2D[] rnm1Textures;
    [SerializeField] private Texture2D[] rnm2Textures;
    [SerializeField] private float[] lightmapModes;
    [SerializeField] private Renderer[] bakedRenderers;


#if !COMPILER_UDONSHARP && UNITY_EDITOR
    public int[] GetGlobalIDs() => globalIDs;
    public void SetGlobalIDs(int[] value) => globalIDs = value;

    public int[] GetLightmapIndices() => lightmapIndices;
    public void SetLightmapIndices(int[] value) => lightmapIndices = value;

    public Vector4[] GetLightmapScaleOffsets() => lightmapScaleOffsets;
    public void SetLightmapScaleOffsets(Vector4[] value) => lightmapScaleOffsets = value;

    public Texture2D[] GetRnm0Textures() => rnm0Textures;
    public void SetRnm0Textures(Texture2D[] value) => rnm0Textures = value;

    public Texture2D[] GetRnm1Textures() => rnm1Textures;
    public void SetRnm1Textures(Texture2D[] value) => rnm1Textures = value;

    public Texture2D[] GetRnm2Textures() => rnm2Textures;
    public void SetRnm2Textures(Texture2D[] value) => rnm2Textures = value;

    public float[] GetLightmapModes() => lightmapModes;
    public void SetLightmapModes(float[] value) => lightmapModes = value;

    public Renderer[] GetBakedRenderers() => bakedRenderers;
    public void SetBakedRenderers(Renderer[] value) => bakedRenderers = value;

#endif


    private void Start()
    {
          ApplyRendererData();
    }

    public void ApplyRendererData()
    {
        MaterialPropertyBlock prop = new MaterialPropertyBlock();
        for (int i = 0; i < bakedRenderers.Length; i++)
        {
            var renderer = bakedRenderers[i];
            int lightmapIndex = lightmapIndices[i];
            renderer.lightmapIndex = lightmapIndex;
            renderer.lightmapScaleOffset = lightmapScaleOffsets[i];

            if (lightmapIndex >= 0 && lightmapIndex < rnm0Textures.Length)
            {
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
}


//made by ivaj