using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
#if !COMPILER_UDONSHARP && UNITY_EDITOR
using UnityEditor;
#endif

public class UdonBakeryVolumeAdaptor : UdonSharpBehaviour
{
    public bool enableBaking = true;
    public Bounds bounds = new Bounds(Vector3.zero, Vector3.one);
    public bool adaptiveRes = true;
    public float voxelsPerUnit = 0.5f;
    public int resolutionX = 16;
    public int resolutionY = 16;
    public int resolutionZ = 16;
    public Encoding encoding = Encoding.Half4;
    public ShadowmaskEncoding shadowmaskEncoding = ShadowmaskEncoding.RGBA8;
    public bool firstLightIsAlwaysAlpha = false;
    public bool denoise = false;
    public bool isGlobal = false;
    public Texture3D bakedTexture0, bakedTexture1, bakedTexture2, bakedTexture3, bakedMask;
    public bool supportRotationAfterBake;

    public Transform tform;

    // Store the IDs in instance variables instead of static fields
    public int _Volume0;
    public int _Volume1;
    public int _Volume2;
    public int _Volume3;
    public int _VolumeMask;
    public int _GlobalVolumeMin;
    public int _GlobalVolumeInvSize;
    public int _GlobalVolumeMatrix;
#if !COMPILER_UDONSHARP && UNITY_EDITOR
    [ExecuteInEditMode]
    private void Awake()
    {
        CopyVolumeData();
    }
#endif
    public void init()
    {

        // Initialize shader property IDs
        _Volume0 = VRCShader.PropertyToID("_Udon_Volume0");
        _Volume1 = VRCShader.PropertyToID("_Udon_Volume1");
        _Volume2 = VRCShader.PropertyToID("_Udon_Volume2");
        _Volume3 = VRCShader.PropertyToID("_Udon_Volume3");
        _VolumeMask = VRCShader.PropertyToID("_Udon_VolumeMask");
        _GlobalVolumeMin = VRCShader.PropertyToID("_Udon_GlobalVolumeMin");
        _GlobalVolumeInvSize = VRCShader.PropertyToID("_Udon_GlobalVolumeInvSize");
        _GlobalVolumeMatrix = VRCShader.PropertyToID("_Udon_GlobalVolumeMatrix");
        
    }
    private void Start()
    {
       
        // Initialize shader property IDs
        _Volume0 = VRCShader.PropertyToID("_Udon_Volume0");
        _Volume1 = VRCShader.PropertyToID("_Udon_Volume1");
        _Volume2 = VRCShader.PropertyToID("_Udon_Volume2");
        _Volume3 = VRCShader.PropertyToID("_Udon_Volume3");
        _VolumeMask = VRCShader.PropertyToID("_Udon_VolumeMask");
        _GlobalVolumeMin = VRCShader.PropertyToID("_Udon_GlobalVolumeMin");
        _GlobalVolumeInvSize = VRCShader.PropertyToID("_Udon_GlobalVolumeInvSize");
        _GlobalVolumeMatrix = VRCShader.PropertyToID("_Udon_GlobalVolumeMatrix");
        SetGlobalParams();
    }

    public Vector3 GetMin()
    {
        return bounds.min;
    }

    public Vector3 GetInvSize()
    {
        var b = bounds;
        return new Vector3(1.0f / b.size.x, 1.0f / b.size.y, 1.0f / b.size.z);
    }

    public Matrix4x4 GetMatrix()
    {
        if (tform == null) tform = transform;
        return Matrix4x4.TRS(tform.position, tform.rotation, Vector3.one).inverse;
    }

    public void SetGlobalParams()
    {
        VRCShader.SetGlobalTexture(VRCShader.PropertyToID("_Udon_Volume0"), bakedTexture0);
        VRCShader.SetGlobalTexture(VRCShader.PropertyToID("_Udon_Volume1"), bakedTexture1);
        VRCShader.SetGlobalTexture(VRCShader.PropertyToID("_Udon_Volume2"), bakedTexture2);
        if (bakedTexture3 != null) VRCShader.SetGlobalTexture(VRCShader.PropertyToID("_Udon_Volume3"), bakedTexture3);
        VRCShader.SetGlobalTexture(VRCShader.PropertyToID("_Udon_VolumeMask"), bakedMask);

        var b = bounds;
        var bmin = b.min;
        var bis = new Vector3(1.0f / b.size.x, 1.0f / b.size.y, 1.0f / b.size.z);

        VRCShader.SetGlobalVector(VRCShader.PropertyToID("_Udon_GlobalVolumeMin"), bmin);
        VRCShader.SetGlobalVector(VRCShader.PropertyToID("_Udon_GlobalVolumeInvSize"), bis);
        if (supportRotationAfterBake) VRCShader.SetGlobalMatrix(VRCShader.PropertyToID("_Udon_GlobalVolumeMatrix"), GetMatrix());
    }

    public void UpdateBounds()
    {
        var pos = transform.position;
        var size = bounds.size;
        bounds = new Bounds(pos, size);
    }

    private void OnEnable()
    {
        if (isGlobal)
        {
            SetGlobalParams();
        }
    }

#if !COMPILER_UDONSHARP && UNITY_EDITOR
    
    public void CopyVolumeData()
    {
        BakeryVolume other = this.gameObject.GetComponent<BakeryVolume>();   
        enableBaking = other.enableBaking;
        bounds = other.bounds;
        adaptiveRes = other.adaptiveRes;
        voxelsPerUnit = other.voxelsPerUnit;
        resolutionX = other.resolutionX;
        resolutionY = other.resolutionY;
        resolutionZ = other.resolutionZ;
       // encoding = other.encoding;
       // shadowmaskEncoding = other.shadowmaskEncoding;
        firstLightIsAlwaysAlpha = other.firstLightIsAlwaysAlpha;
        denoise = other.denoise;
        isGlobal = other.isGlobal;
        bakedTexture0 = other.bakedTexture0;
        bakedTexture1 = other.bakedTexture1;
        bakedTexture2 = other.bakedTexture2;
        bakedTexture3 = other.bakedTexture3;
        bakedMask = other.bakedMask;
        supportRotationAfterBake = other.supportRotationAfterBake;

        // If the other volume is global, update global params
        if (isGlobal)
        {
            SetGlobalParams();
        }
        EditorUtility.SetDirty(this.gameObject);
    }
#endif

}

public enum Encoding
{
    Half4,
    RGBA8,
    RGBA8Mono
}

public enum ShadowmaskEncoding
{
    RGBA8,
    A8
}



//made by ivaj
