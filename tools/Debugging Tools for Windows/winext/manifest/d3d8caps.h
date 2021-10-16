// ---------------------------------------------------------------------------
// ---------------------------------------------------------------------------
//
//                              D3D8 Caps
//
// ---------------------------------------------------------------------------
// ---------------------------------------------------------------------------

//
// Masks
//

//
// Caps
//
mask DWORD d3dCaps8
{
#define D3DCAPS_READ_SCANLINE           0x00020000L
};

//
// Caps2
//
mask DWORD d3dCaps28
{
#define D3DCAPS2_NO2DDURING3DSCENE      0x00000002L
#define D3DCAPS2_FULLSCREENGAMMA        0x00020000L
#define D3DCAPS2_CANRENDERWINDOWED      0x00080000L
#define D3DCAPS2_CANCALIBRATEGAMMA      0x00100000L
#define D3DCAPS2_RESERVED               0x02000000L
};

//
// Caps3
//
mask DWORD d3dCaps38
{
#define D3DCAPS3_RESERVED               0x8000001fL
};

//
// CursorCaps
//
mask DWORD d3dCursorCaps8
{
#define D3DCURSORCAPS_COLOR             0x00000001L
#define D3DCURSORCAPS_LOWRES            0x00000002L
};

//
// DevCaps
//
mask DWORD d3dDevCaps8
{
#define D3DDEVCAPS_EXECUTESYSTEMMEMORY  0x00000010L
#define D3DDEVCAPS_EXECUTEVIDEOMEMORY   0x00000020L
#define D3DDEVCAPS_TLVERTEXSYSTEMMEMORY 0x00000040L
#define D3DDEVCAPS_TLVERTEXVIDEOMEMORY  0x00000080L
#define D3DDEVCAPS_TEXTURESYSTEMMEMORY  0x00000100L
#define D3DDEVCAPS_TEXTUREVIDEOMEMORY   0x00000200L
#define D3DDEVCAPS_DRAWPRIMTLVERTEX     0x00000400L
#define D3DDEVCAPS_CANRENDERAFTERFLIP   0x00000800L
#define D3DDEVCAPS_TEXTURENONLOCALVIDMEM 0x00001000L
#define D3DDEVCAPS_DRAWPRIMITIVES2      0x00002000L
#define D3DDEVCAPS_SEPARATETEXTUREMEMORIES 0x00004000L
#define D3DDEVCAPS_DRAWPRIMITIVES2EX    0x00008000L
#define D3DDEVCAPS_HWTRANSFORMANDLIGHT  0x00010000L
#define D3DDEVCAPS_CANBLTSYSTONONLOCAL  0x00020000L
#define D3DDEVCAPS_HWRASTERIZATION      0x00080000L
#define D3DDEVCAPS_PUREDEVICE           0x00100000L
#define D3DDEVCAPS_QUINTICRTPATCHES     0x00200000L
#define D3DDEVCAPS_RTPATCHES            0x00400000L
#define D3DDEVCAPS_RTPATCHHANDLEZERO    0x00800000L
#define D3DDEVCAPS_NPATCHES             0x01000000L
};

//
// PrimitiveMiscCaps
//
mask DWORD d3dPrimitiveMiscCaps8
{
#define D3DPMISCCAPS_MASKZ              0x00000002L
#define D3DPMISCCAPS_LINEPATTERNREP     0x00000004L
#define D3DPMISCCAPS_CULLNONE           0x00000010L
#define D3DPMISCCAPS_CULLCW             0x00000020L
#define D3DPMISCCAPS_CULLCCW            0x00000040L
#define D3DPMISCCAPS_COLORWRITEENABLE   0x00000080L
#define D3DPMISCCAPS_CLIPPLANESCALEDPOINTS 0x00000100L
#define D3DPMISCCAPS_CLIPTLVERTS        0x00000200L
#define D3DPMISCCAPS_TSSARGTEMP         0x00000400L
#define D3DPMISCCAPS_BLENDOP            0x00000800L
};

//
// LineCaps
//
mask DWORD d3dLineCaps8
{
#define D3DLINECAPS_TEXTURE             0x00000001L
#define D3DLINECAPS_ZTEST               0x00000002L
#define D3DLINECAPS_BLEND               0x00000004L
#define D3DLINECAPS_ALPHACMP            0x00000008L
#define D3DLINECAPS_FOG                 0x00000010L
};

//
// RasterCaps
//
mask DWORD d3dRasterCaps8
{
#define D3DPRASTERCAPS_DITHER           0x00000001L
#define D3DPRASTERCAPS_PAT              0x00000008L
#define D3DPRASTERCAPS_ZTEST            0x00000010L
#define D3DPRASTERCAPS_FOGVERTEX        0x00000080L
#define D3DPRASTERCAPS_FOGTABLE         0x00000100L
#define D3DPRASTERCAPS_ANTIALIASEDGES   0x00001000L
#define D3DPRASTERCAPS_MIPMAPLODBIAS    0x00002000L
#define D3DPRASTERCAPS_ZBIAS            0x00004000L
#define D3DPRASTERCAPS_ZBUFFERLESSHSR   0x00008000L
#define D3DPRASTERCAPS_FOGRANGE         0x00010000L
#define D3DPRASTERCAPS_ANISOTROPY       0x00020000L
#define D3DPRASTERCAPS_WBUFFER          0x00040000L
#define D3DPRASTERCAPS_WFOG             0x00100000L
#define D3DPRASTERCAPS_ZFOG             0x00200000L
#define D3DPRASTERCAPS_COLORPERSPECTIVE 0x00400000L
#define D3DPRASTERCAPS_STRETCHBLTMULTISAMPLE  0x00800000L
};

//
// ZCmpCaps, AlphaCmpCaps
//
mask DWORD d3dCmpCaps8
{
#define D3DPCMPCAPS_NEVER               0x00000001L
#define D3DPCMPCAPS_LESS                0x00000002L
#define D3DPCMPCAPS_EQUAL               0x00000004L
#define D3DPCMPCAPS_LESSEQUAL           0x00000008L
#define D3DPCMPCAPS_GREATER             0x00000010L
#define D3DPCMPCAPS_NOTEQUAL            0x00000020L
#define D3DPCMPCAPS_GREATEREQUAL        0x00000040L
#define D3DPCMPCAPS_ALWAYS              0x00000080L
};

//
// SourceBlendCaps, DestBlendCaps
//
mask DWORD d3dBlendCaps8
{
#define D3DPBLENDCAPS_ZERO              0x00000001L
#define D3DPBLENDCAPS_ONE               0x00000002L
#define D3DPBLENDCAPS_SRCCOLOR          0x00000004L
#define D3DPBLENDCAPS_INVSRCCOLOR       0x00000008L
#define D3DPBLENDCAPS_SRCALPHA          0x00000010L
#define D3DPBLENDCAPS_INVSRCALPHA       0x00000020L
#define D3DPBLENDCAPS_DESTALPHA         0x00000040L
#define D3DPBLENDCAPS_INVDESTALPHA      0x00000080L
#define D3DPBLENDCAPS_DESTCOLOR         0x00000100L
#define D3DPBLENDCAPS_INVDESTCOLOR      0x00000200L
#define D3DPBLENDCAPS_SRCALPHASAT       0x00000400L
#define D3DPBLENDCAPS_BOTHSRCALPHA      0x00000800L
#define D3DPBLENDCAPS_BOTHINVSRCALPHA   0x00001000L
};

//
// ShadeCaps
//
mask DWORD d3dShadeCaps8
{
#define D3DPSHADECAPS_COLORGOURAUDRGB       0x00000008L
#define D3DPSHADECAPS_SPECULARGOURAUDRGB    0x00000200L
#define D3DPSHADECAPS_ALPHAGOURAUDBLEND     0x00004000L
#define D3DPSHADECAPS_FOGGOURAUD            0x00080000L
};

//
// TextureCaps
//
mask DWORD d3dTextureCaps8
{
#define D3DPTEXTURECAPS_PERSPECTIVE         0x00000001L
#define D3DPTEXTURECAPS_POW2                0x00000002L
#define D3DPTEXTURECAPS_ALPHA               0x00000004L
#define D3DPTEXTURECAPS_SQUAREONLY          0x00000020L
#define D3DPTEXTURECAPS_TEXREPEATNOTSCALEDBYSIZE 0x00000040L
#define D3DPTEXTURECAPS_ALPHAPALETTE        0x00000080L
#define D3DPTEXTURECAPS_NONPOW2CONDITIONAL  0x00000100L
#define D3DPTEXTURECAPS_PROJECTED           0x00000400L
#define D3DPTEXTURECAPS_CUBEMAP             0x00000800L
#define D3DPTEXTURECAPS_VOLUMEMAP           0x00002000L
#define D3DPTEXTURECAPS_MIPMAP              0x00004000L
#define D3DPTEXTURECAPS_MIPVOLUMEMAP        0x00008000L
#define D3DPTEXTURECAPS_MIPCUBEMAP          0x00010000L
#define D3DPTEXTURECAPS_CUBEMAP_POW2        0x00020000L
#define D3DPTEXTURECAPS_VOLUMEMAP_POW2      0x00040000L
};

//
// TextureFilterCaps
//
mask DWORD d3dTextureFilterCaps8
{
#define D3DPTFILTERCAPS_MINFPOINT           0x00000100L
#define D3DPTFILTERCAPS_MINFLINEAR          0x00000200L
#define D3DPTFILTERCAPS_MINFANISOTROPIC     0x00000400L
#define D3DPTFILTERCAPS_MIPFPOINT           0x00010000L
#define D3DPTFILTERCAPS_MIPFLINEAR          0x00020000L
#define D3DPTFILTERCAPS_MAGFPOINT           0x01000000L
#define D3DPTFILTERCAPS_MAGFLINEAR          0x02000000L
#define D3DPTFILTERCAPS_MAGFANISOTROPIC     0x04000000L
#define D3DPTFILTERCAPS_MAGFAFLATCUBIC      0x08000000L
#define D3DPTFILTERCAPS_MAGFGAUSSIANCUBIC   0x10000000L
};

//
// TextureAddressCaps
//
mask DWORD d3dTextureAddressCaps8
{
#define D3DPTADDRESSCAPS_WRAP           0x00000001L
#define D3DPTADDRESSCAPS_MIRROR         0x00000002L
#define D3DPTADDRESSCAPS_CLAMP          0x00000004L
#define D3DPTADDRESSCAPS_BORDER         0x00000008L
#define D3DPTADDRESSCAPS_INDEPENDENTUV  0x00000010L
#define D3DPTADDRESSCAPS_MIRRORONCE     0x00000020L
};

//
// StencilCaps
//
mask DWORD d3dStencilCaps8
{
#define D3DSTENCILCAPS_KEEP             0x00000001L
#define D3DSTENCILCAPS_ZERO             0x00000002L
#define D3DSTENCILCAPS_REPLACE          0x00000004L
#define D3DSTENCILCAPS_INCRSAT          0x00000008L
#define D3DSTENCILCAPS_DECRSAT          0x00000010L
#define D3DSTENCILCAPS_INVERT           0x00000020L
#define D3DSTENCILCAPS_INCR             0x00000040L
#define D3DSTENCILCAPS_DECR             0x00000080L
};

//
// TextureOpCaps
//
mask DWORD d3dTextureOpCaps8
{
#define D3DTEXOPCAPS_DISABLE                    0x00000001L
#define D3DTEXOPCAPS_SELECTARG1                 0x00000002L
#define D3DTEXOPCAPS_SELECTARG2                 0x00000004L
#define D3DTEXOPCAPS_MODULATE                   0x00000008L
#define D3DTEXOPCAPS_MODULATE2X                 0x00000010L
#define D3DTEXOPCAPS_MODULATE4X                 0x00000020L
#define D3DTEXOPCAPS_ADD                        0x00000040L
#define D3DTEXOPCAPS_ADDSIGNED                  0x00000080L
#define D3DTEXOPCAPS_ADDSIGNED2X                0x00000100L
#define D3DTEXOPCAPS_SUBTRACT                   0x00000200L
#define D3DTEXOPCAPS_ADDSMOOTH                  0x00000400L
#define D3DTEXOPCAPS_BLENDDIFFUSEALPHA          0x00000800L
#define D3DTEXOPCAPS_BLENDTEXTUREALPHA          0x00001000L
#define D3DTEXOPCAPS_BLENDFACTORALPHA           0x00002000L
#define D3DTEXOPCAPS_BLENDTEXTUREALPHAPM        0x00004000L
#define D3DTEXOPCAPS_BLENDCURRENTALPHA          0x00008000L
#define D3DTEXOPCAPS_PREMODULATE                0x00010000L
#define D3DTEXOPCAPS_MODULATEALPHA_ADDCOLOR     0x00020000L
#define D3DTEXOPCAPS_MODULATECOLOR_ADDALPHA     0x00040000L
#define D3DTEXOPCAPS_MODULATEINVALPHA_ADDCOLOR  0x00080000L
#define D3DTEXOPCAPS_MODULATEINVCOLOR_ADDALPHA  0x00100000L
#define D3DTEXOPCAPS_BUMPENVMAP                 0x00200000L
#define D3DTEXOPCAPS_BUMPENVMAPLUMINANCE        0x00400000L
#define D3DTEXOPCAPS_DOTPRODUCT3                0x00800000L
#define D3DTEXOPCAPS_MULTIPLYADD                0x01000000L
#define D3DTEXOPCAPS_LERP                       0x02000000L
};

//
// FVFCaps
//
mask DWORD d3dFVFCaps8
{
#define D3DFVFCAPS_TEXCOORDCOUNTMASK    0x0000ffffL
#define D3DFVFCAPS_DONOTSTRIPELEMENTS   0x00080000L
#define D3DFVFCAPS_PSIZE                0x00100000L
};

//
// VertexProcessingCaps
//
mask DWORD d3dVertexProcessingCaps8
{
#define D3DVTXPCAPS_TEXGEN              0x00000001L
#define D3DVTXPCAPS_MATERIALSOURCE7     0x00000002L
#define D3DVTXPCAPS_DIRECTIONALLIGHTS   0x00000008L
#define D3DVTXPCAPS_POSITIONALLIGHTS    0x00000010L
#define D3DVTXPCAPS_LOCALVIEWER         0x00000020L
#define D3DVTXPCAPS_TWEENING            0x00000040L
#define D3DVTXPCAPS_NO_VSDT_UBYTE4      0x00000080L
};


//
// Structs
//

typedef struct _D3DCAPS8
{
    D3DDEVTYPE                DeviceType;
    d3dAdapterID8             AdapterOrdinal;
    d3dCaps8                  Caps;
    d3dCaps28                 Caps2;
    d3dCaps38                 Caps3;
    d3dPresentationIntervals8 PresentationIntervals;
    d3dCursorCaps8            CursorCaps;
    d3dDevCaps8               DevCaps;
    d3dPrimitiveMiscCaps8     PrimitiveMiscCaps;
    d3dRasterCaps8            RasterCaps;
    d3dCmpCaps8               ZCmpCaps;
    d3dBlendCaps8             SrcBlendCaps;
    d3dBlendCaps8             DestBlendCaps;
    d3dCmpCaps8               AlphaCmpCaps;
    d3dShadeCaps8             ShadeCaps;
    d3dTextureCaps8           TextureCaps;
    d3dTextureFilterCaps8     TextureFilterCaps;
    d3dTextureFilterCaps8     CubeTextureFilterCaps;
    d3dTextureFilterCaps8     VolumeTextureFilterCaps;
    d3dTextureAddressCaps8    TextureAddressCaps;
    d3dTextureAddressCaps8    VolumeTextureAddressCaps;
    d3dLineCaps8              LineCaps;
    DWORD                     MaxTextureWidth;
	DWORD                     MaxTextureHeight;
    DWORD                     MaxVolumeExtent;
    DWORD                     MaxTextureRepeat;
    DWORD                     MaxTextureAspectRatio;
    DWORD                     MaxAnisotropy;
    float                     MaxVertexW;
    float                     GuardBandLeft;
    float                     GuardBandTop;
    float                     GuardBandRight;
    float                     GuardBandBottom;
    float                     ExtentsAdjust;
    d3dStencilCaps8           StencilCaps;
    d3dFVFCaps8               FVFCaps;
    d3dTextureOpCaps8         TextureOpCaps;
    DWORD                     MaxTextureBlendStages;
    DWORD                     MaxSimultaneousTextures;
    d3dVertexProcessingCaps8  VertexProcessingCaps;
    DWORD                     MaxActiveLights;
    DWORD                     MaxUserClipPlanes;
    DWORD                     MaxVertexBlendMatrices;
    DWORD                     MaxVertexBlendMatrixIndex;
    float                     MaxPointSize;
    DWORD                     MaxPrimitiveCount;
    DWORD                     MaxVertexIndex;
    DWORD                     MaxStreams;
    DWORD                     MaxStreamStride;
    DWORD                     VertexShaderVersion;
    DWORD                     MaxVertexShaderConst;
    DWORD                     PixelShaderVersion;
    float                     MaxPixelShaderValue;
} D3DCAPS8, *LPD3DCAPS8;

alias LPD3DCAPS8;
