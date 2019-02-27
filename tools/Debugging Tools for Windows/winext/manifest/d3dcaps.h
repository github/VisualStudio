

//
// GUIDs
//



//
// Typedefs
//

typedef VOID * LPD3DENUMDEVICESCALLBACK;
typedef VOID * LPD3DENUMDEVICESCALLBACK7;


//
// Masks
//

mask DWORD d3ddevcapsFlags
{
	#define D3DDEVCAPS_FLOATTLVERTEX                 0x00000001L
	#define D3DDEVCAPS_SORTINCREASINGZ               0x00000002L
	#define D3DDEVCAPS_SORTDECREASINGZ               0X00000004L
	#define D3DDEVCAPS_SORTEXACT                     0x00000008L
	#define D3DDEVCAPS_EXECUTESYSTEMMEMORY           0x00000010L
	#define D3DDEVCAPS_EXECUTEVIDEOMEMORY            0x00000020L
	#define D3DDEVCAPS_TLVERTEXSYSTEMMEMORY          0x00000040L
	#define D3DDEVCAPS_TLVERTEXVIDEOMEMORY           0x00000080L
	#define D3DDEVCAPS_TEXTURESYSTEMMEMORY           0x00000100L
	#define D3DDEVCAPS_TEXTUREVIDEOMEMORY            0x00000200L
	#define D3DDEVCAPS_DRAWPRIMTLVERTEX              0x00000400L
	#define D3DDEVCAPS_CANRENDERAFTERFLIP            0x00000800L
	#define D3DDEVCAPS_TEXTURENONLOCALVIDMEM         0x00001000L
	#define D3DDEVCAPS_DRAWPRIMITIVES2               0x00002000L
	#define D3DDEVCAPS_SEPARATETEXTUREMEMORIES       0x00004000L
	#define D3DDEVCAPS_DRAWPRIMITIVES2EX             0x00008000L
	#define D3DDEVCAPS_HWTRANSFORMANDLIGHT           0x00010000L
	#define D3DDEVCAPS_CANBLTSYSTONONLOCAL           0x00020000L
	#define D3DDEVCAPS_HWRASTERIZATION               0x00080000L
};

mask DWORD d3dlightingmodelFlags
{
	#define D3DLIGHTINGMODEL_RGB                     0x00000001L
	#define D3DLIGHTINGMODEL_MONO                    0x00000002L
};

mask DWORD d3dddFlags
{
	#define D3DDD_COLORMODEL                         0x00000001L
	#define D3DDD_DEVCAPS                            0x00000002L
	#define D3DDD_TRANSFORMCAPS                      0x00000004L
	#define D3DDD_LIGHTINGCAPS                       0x00000008L
	#define D3DDD_BCLIPPING                          0x00000010L
	#define D3DDD_LINECAPS                           0x00000020L
	#define D3DDD_TRICAPS                            0x00000040L
	#define D3DDD_DEVICERENDERBITDEPTH               0x00000080L
	#define D3DDD_DEVICEZBUFFERBITDEPTH              0x00000100L
	#define D3DDD_MAXBUFFERSIZE                      0x00000200L
	#define D3DDD_MAXVERTEXCOUNT                     0x00000400L
};

mask DWORD d3dptexturecapsFlags
{
	#define D3DPTEXTURECAPS_PERSPECTIVE              0x00000001L
	#define D3DPTEXTURECAPS_POW2                     0x00000002L
	#define D3DPTEXTURECAPS_ALPHA                    0x00000004L
	#define D3DPTEXTURECAPS_TRANSPARENCY             0x00000008L
	#define D3DPTEXTURECAPS_BORDER                   0x00000010L
	#define D3DPTEXTURECAPS_SQUAREONLY               0x00000020L
	#define D3DPTEXTURECAPS_TEXREPEATNOTSCALEDBYSIZE 0x00000040L
	#define D3DPTEXTURECAPS_ALPHAPALETTE             0x00000080L
	#define D3DPTEXTURECAPS_NONPOW2CONDITIONAL       0x00000100L
	#define D3DPTEXTURECAPS_PROJECTED                0x00000400L
	#define D3DPTEXTURECAPS_CUBEMAP                  0x00000800L
	#define D3DPTEXTURECAPS_COLORKEYBLEND            0x00001000L
};

mask DWORD d3dprastercapsFlags
{
	#define D3DPRASTERCAPS_DITHER                    0x00000001L
	#define D3DPRASTERCAPS_ROP2                      0x00000002L
	#define D3DPRASTERCAPS_XOR                       0x00000004L
	#define D3DPRASTERCAPS_PAT                       0x00000008L
	#define D3DPRASTERCAPS_ZTEST                     0x00000010L
	#define D3DPRASTERCAPS_SUBPIXEL                  0x00000020L
	#define D3DPRASTERCAPS_SUBPIXELX                 0x00000040L
	#define D3DPRASTERCAPS_FOGVERTEX                 0x00000080L
	#define D3DPRASTERCAPS_FOGTABLE                  0x00000100L
	#define D3DPRASTERCAPS_STIPPLE                   0x00000200L
	#define D3DPRASTERCAPS_ANTIALIASSORTDEPENDENT    0x00000400L
	#define D3DPRASTERCAPS_ANTIALIASSORTINDEPENDENT  0x00000800L
	#define D3DPRASTERCAPS_ANTIALIASEDGES            0x00001000L
	#define D3DPRASTERCAPS_MIPMAPLODBIAS             0x00002000L
	#define D3DPRASTERCAPS_ZBIAS                     0x00004000L
	#define D3DPRASTERCAPS_ZBUFFERLESSHSR            0x00008000L
	#define D3DPRASTERCAPS_FOGRANGE                  0x00010000L
	#define D3DPRASTERCAPS_ANISOTROPY                0x00020000L
	#define D3DPRASTERCAPS_WBUFFER                   0x00040000L
	#define D3DPRASTERCAPS_TRANSLUCENTSORTINDEPENDENT 0x00080000L
	#define D3DPRASTERCAPS_WFOG                      0x00100000L
	#define D3DPRASTERCAPS_ZFOG                      0x00200000L
};

mask DWORD d3dfvfcapsFlags
{
	#define D3DFVFCAPS_TEXCOORDCOUNTMASK             0x0000ffffL
	#define D3DFVFCAPS_DONOTSTRIPELEMENTS            0x00080000L
};

mask DWORD d3ddebFlags
{
	#define D3DDEB_BUFSIZE                           0x00000001l
	#define D3DDEB_CAPS                              0x00000002l
	#define D3DDEB_LPDATA                            0x00000004l
};

mask DWORD d3dptblendcapsFlags
{
	#define D3DPTBLENDCAPS_DECAL                     0x00000001L
	#define D3DPTBLENDCAPS_MODULATE                  0x00000002L
	#define D3DPTBLENDCAPS_DECALALPHA                0x00000004L
	#define D3DPTBLENDCAPS_MODULATEALPHA             0x00000008L
	#define D3DPTBLENDCAPS_DECALMASK                 0x00000010L
	#define D3DPTBLENDCAPS_MODULATEMASK              0x00000020L
	#define D3DPTBLENDCAPS_COPY                      0x00000040L
	#define D3DPTBLENDCAPS_ADD                       0x00000080L
};

mask DWORD d3dptaddresscapsFlags
{
	#define D3DPTADDRESSCAPS_WRAP                    0x00000001L
	#define D3DPTADDRESSCAPS_MIRROR                  0x00000002L
	#define D3DPTADDRESSCAPS_CLAMP                   0x00000004L
	#define D3DPTADDRESSCAPS_BORDER                  0x00000008L
	#define D3DPTADDRESSCAPS_INDEPENDENTUV           0x00000010L
};

mask DWORD d3dptfiltercapsFlags
{
	#define D3DPTFILTERCAPS_NEAREST                  0x00000001L
	#define D3DPTFILTERCAPS_LINEAR                   0x00000002L
	#define D3DPTFILTERCAPS_MIPNEAREST               0x00000004L
	#define D3DPTFILTERCAPS_MIPLINEAR                0x00000008L
	#define D3DPTFILTERCAPS_LINEARMIPNEAREST         0x00000010L
	#define D3DPTFILTERCAPS_LINEARMIPLINEAR          0x00000020L
	#define D3DPTFILTERCAPS_MINFPOINT                0x00000100L
	#define D3DPTFILTERCAPS_MINFLINEAR               0x00000200L
	#define D3DPTFILTERCAPS_MINFANISOTROPIC          0x00000400L
	#define D3DPTFILTERCAPS_MIPFPOINT                0x00010000L
	#define D3DPTFILTERCAPS_MIPFLINEAR               0x00020000L
	#define D3DPTFILTERCAPS_MAGFPOINT                0x01000000L
	#define D3DPTFILTERCAPS_MAGFLINEAR               0x02000000L
	#define D3DPTFILTERCAPS_MAGFANISOTROPIC          0x04000000L
	#define D3DPTFILTERCAPS_MAGFAFLATCUBIC           0x08000000L
	#define D3DPTFILTERCAPS_MAGFGAUSSIANCUBIC        0x10000000L
};

mask DWORD d3ddebcapsFlags
{
	#define D3DDEBCAPS_SYSTEMMEMORY                  0x00000001l
	#define D3DDEBCAPS_VIDEOMEMORY                   0x00000002l
	#define D3DDEBCAPS_MEM                           3
};

mask DWORD d3dpcmpcapsFlags
{
	#define D3DPCMPCAPS_NEVER                        0x00000001L
	#define D3DPCMPCAPS_LESS                         0x00000002L
	#define D3DPCMPCAPS_EQUAL                        0x00000004L
	#define D3DPCMPCAPS_LESSEQUAL                    0x00000008L
	#define D3DPCMPCAPS_GREATER                      0x00000010L
	#define D3DPCMPCAPS_NOTEQUAL                     0x00000020L
	#define D3DPCMPCAPS_GREATEREQUAL                 0x00000040L
	#define D3DPCMPCAPS_ALWAYS                       0x00000080L
};

mask DWORD d3dtexopcapsFlags
{
	#define D3DTEXOPCAPS_DISABLE                     0x00000001L
	#define D3DTEXOPCAPS_SELECTARG1                  0x00000002L
	#define D3DTEXOPCAPS_SELECTARG2                  0x00000004L
	#define D3DTEXOPCAPS_MODULATE                    0x00000008L
	#define D3DTEXOPCAPS_MODULATE2X                  0x00000010L
	#define D3DTEXOPCAPS_MODULATE4X                  0x00000020L
	#define D3DTEXOPCAPS_ADD                         0x00000040L
	#define D3DTEXOPCAPS_ADDSIGNED                   0x00000080L
	#define D3DTEXOPCAPS_ADDSIGNED2X                 0x00000100L
	#define D3DTEXOPCAPS_SUBTRACT                    0x00000200L
	#define D3DTEXOPCAPS_ADDSMOOTH                   0x00000400L
	#define D3DTEXOPCAPS_BLENDDIFFUSEALPHA           0x00000800L
	#define D3DTEXOPCAPS_BLENDTEXTUREALPHA           0x00001000L
	#define D3DTEXOPCAPS_BLENDFACTORALPHA            0x00002000L
	#define D3DTEXOPCAPS_BLENDTEXTUREALPHAPM         0x00004000L
	#define D3DTEXOPCAPS_BLENDCURRENTALPHA           0x00008000L
	#define D3DTEXOPCAPS_PREMODULATE                 0x00010000L
	#define D3DTEXOPCAPS_MODULATEALPHA_ADDCOLOR      0x00020000L
	#define D3DTEXOPCAPS_MODULATECOLOR_ADDALPHA      0x00040000L
	#define D3DTEXOPCAPS_MODULATEINVALPHA_ADDCOLOR   0x00080000L
	#define D3DTEXOPCAPS_MODULATEINVCOLOR_ADDALPHA   0x00100000L
	#define D3DTEXOPCAPS_BUMPENVMAP                  0x00200000L
	#define D3DTEXOPCAPS_BUMPENVMAPLUMINANCE         0x00400000L
	#define D3DTEXOPCAPS_DOTPRODUCT3                 0x00800000L
};

mask DWORD d3dvtxpcapsFlags
{
	#define D3DVTXPCAPS_TEXGEN                       0x00000001L
	#define D3DVTXPCAPS_MATERIALSOURCE7              0x00000002L
	#define D3DVTXPCAPS_VERTEXFOG                    0x00000004L
	#define D3DVTXPCAPS_DIRECTIONALLIGHTS            0x00000008L
	#define D3DVTXPCAPS_POSITIONALLIGHTS             0x00000010L
	#define D3DVTXPCAPS_LOCALVIEWER                  0x00000020L
};

mask DWORD d3dstencilcapsFlags
{
	#define D3DSTENCILCAPS_KEEP                      0x00000001L
	#define D3DSTENCILCAPS_ZERO                      0x00000002L
	#define D3DSTENCILCAPS_REPLACE                   0x00000004L
	#define D3DSTENCILCAPS_INCRSAT                   0x00000008L
	#define D3DSTENCILCAPS_DECRSAT                   0x00000010L
	#define D3DSTENCILCAPS_INVERT                    0x00000020L
	#define D3DSTENCILCAPS_INCR                      0x00000040L
	#define D3DSTENCILCAPS_DECR                      0x00000080L
};

mask DWORD d3dpblendcapsFlags
{
	#define D3DPBLENDCAPS_ZERO                       0x00000001L
	#define D3DPBLENDCAPS_ONE                        0x00000002L
	#define D3DPBLENDCAPS_SRCCOLOR                   0x00000004L
	#define D3DPBLENDCAPS_INVSRCCOLOR                0x00000008L
	#define D3DPBLENDCAPS_SRCALPHA                   0x00000010L
	#define D3DPBLENDCAPS_INVSRCALPHA                0x00000020L
	#define D3DPBLENDCAPS_DESTALPHA                  0x00000040L
	#define D3DPBLENDCAPS_INVDESTALPHA               0x00000080L
	#define D3DPBLENDCAPS_DESTCOLOR                  0x00000100L
	#define D3DPBLENDCAPS_INVDESTCOLOR               0x00000200L
	#define D3DPBLENDCAPS_SRCALPHASAT                0x00000400L
	#define D3DPBLENDCAPS_BOTHSRCALPHA               0x00000800L
	#define D3DPBLENDCAPS_BOTHINVSRCALPHA            0x00001000L
};

mask DWORD d3dpshadecapsFlags
{
	#define D3DPSHADECAPS_COLORFLATMONO              0x00000001L
	#define D3DPSHADECAPS_COLORFLATRGB               0x00000002L
	#define D3DPSHADECAPS_COLORGOURAUDMONO           0x00000004L
	#define D3DPSHADECAPS_COLORGOURAUDRGB            0x00000008L
	#define D3DPSHADECAPS_COLORPHONGMONO             0x00000010L
	#define D3DPSHADECAPS_COLORPHONGRGB              0x00000020L
	#define D3DPSHADECAPS_SPECULARFLATMONO           0x00000040L
	#define D3DPSHADECAPS_SPECULARFLATRGB            0x00000080L
	#define D3DPSHADECAPS_SPECULARGOURAUDMONO        0x00000100L
	#define D3DPSHADECAPS_SPECULARGOURAUDRGB         0x00000200L
	#define D3DPSHADECAPS_SPECULARPHONGMONO          0x00000400L
	#define D3DPSHADECAPS_SPECULARPHONGRGB           0x00000800L
	#define D3DPSHADECAPS_ALPHAFLATBLEND             0x00001000L
	#define D3DPSHADECAPS_ALPHAFLATSTIPPLED          0x00002000L
	#define D3DPSHADECAPS_ALPHAGOURAUDBLEND          0x00004000L
	#define D3DPSHADECAPS_ALPHAGOURAUDSTIPPLED       0x00008000L
	#define D3DPSHADECAPS_ALPHAPHONGBLEND            0x00010000L
	#define D3DPSHADECAPS_ALPHAPHONGSTIPPLED         0x00020000L
	#define D3DPSHADECAPS_FOGFLAT                    0x00040000L
	#define D3DPSHADECAPS_FOGGOURAUD                 0x00080000L
	#define D3DPSHADECAPS_FOGPHONG                   0x00100000L
};

mask DWORD d3dfdsFlags
{
	#define D3DFDS_COLORMODEL                        0x00000001L
	#define D3DFDS_GUID                              0x00000002L
	#define D3DFDS_HARDWARE                          0x00000004L
	#define D3DFDS_TRIANGLES                         0x00000008L
	#define D3DFDS_LINES                             0x00000010L
	#define D3DFDS_MISCCAPS                          0x00000020L
	#define D3DFDS_RASTERCAPS                        0x00000040L
	#define D3DFDS_ZCMPCAPS                          0x00000080L
	#define D3DFDS_ALPHACMPCAPS                      0x00000100L
	#define D3DFDS_SRCBLENDCAPS                      0x00000200L
	#define D3DFDS_DSTBLENDCAPS                      0x00000400L
	#define D3DFDS_SHADECAPS                         0x00000800L
	#define D3DFDS_TEXTURECAPS                       0x00001000L
	#define D3DFDS_TEXTUREFILTERCAPS                 0x00002000L
	#define D3DFDS_TEXTUREBLENDCAPS                  0x00004000L
	#define D3DFDS_TEXTUREADDRESSCAPS                0x00008000L
};

mask DWORD d3dlightcapsFlags
{
	#define D3DLIGHTCAPS_POINT                       0x00000001L
	#define D3DLIGHTCAPS_SPOT                        0x00000002L
	#define D3DLIGHTCAPS_DIRECTIONAL                 0x00000004L
	#define D3DLIGHTCAPS_PARALLELPOINT               0x00000008L
	#define D3DLIGHTCAPS_GLSPOT                      0x00000010L
};

mask DWORD d3dpmisccapsFlags
{
	#define D3DPMISCCAPS_MASKPLANES                  0x00000001L
	#define D3DPMISCCAPS_MASKZ                       0x00000002L
	#define D3DPMISCCAPS_LINEPATTERNREP              0x00000004L
	#define D3DPMISCCAPS_CONFORMANT                  0x00000008L
	#define D3DPMISCCAPS_CULLNONE                    0x00000010L
	#define D3DPMISCCAPS_CULLCW                      0x00000020L
	#define D3DPMISCCAPS_CULLCCW                     0x00000040L
};

mask DWORD d3dtransformcapsFlags
{
	#define D3DTRANSFORMCAPS_CLIP                    0x00000001L
};



//
// Values
//



//
// Structs
//

typedef struct _D3DDEVINFO_TEXTUREMANAGER
{
    BOOL    bThrashing;                 /* indicates if thrashing */
    DWORD   dwApproxBytesDownloaded;    /* Approximate number of bytes downloaded by texture manager */
    DWORD   dwNumEvicts;                /* number of textures evicted */
    DWORD   dwNumVidCreates;            /* number of textures created in video memory */
    DWORD   dwNumTexturesUsed;          /* number of textures used */
    DWORD   dwNumUsedTexInVid;          /* number of used textures present in video memory */
    DWORD   dwWorkingSet;               /* number of textures in video memory */
    DWORD   dwWorkingSetBytes;          /* number of bytes in video memory */
    DWORD   dwTotalManaged;             /* total number of managed textures */
    DWORD   dwTotalBytes;               /* total number of bytes of managed textures */
    DWORD   dwLastPri;                  /* priority of last texture evicted */
} D3DDEVINFO_TEXTUREMANAGER, *LPD3DDEVINFO_TEXTUREMANAGER;

typedef struct _D3DExecuteBufferDesc
{
    DWORD               dwSize;         /* size of this structure */
    DWORD               dwFlags;        /* flags indicating which fields are valid */
    DWORD               dwCaps;         /* capabilities of execute buffer */
    DWORD               dwBufferSize;   /* size of execute buffer data */
    LPVOID              lpData;         /* pointer to actual data */
} D3DEXECUTEBUFFERDESC, *LPD3DEXECUTEBUFFERDESC;

typedef struct _D3DLIGHTINGCAPS
{
    DWORD dwSize;
    DWORD dwCaps;                   /* Lighting caps */
    DWORD dwLightingModel;          /* Lighting model - RGB or mono */
    DWORD dwNumLights;              /* Number of lights that can be handled */
} D3DLIGHTINGCAPS, *LPD3DLIGHTINGCAPS;

typedef struct _D3DPrimCaps
{
    DWORD dwSize;
    DWORD dwMiscCaps;                 /* Capability flags */
    DWORD dwRasterCaps;
    DWORD dwZCmpCaps;
    DWORD dwSrcBlendCaps;
    DWORD dwDestBlendCaps;
    DWORD dwAlphaCmpCaps;
    DWORD dwShadeCaps;
    DWORD dwTextureCaps;
    DWORD dwTextureFilterCaps;
    DWORD dwTextureBlendCaps;
    DWORD dwTextureAddressCaps;
    DWORD dwStippleWidth;             /* maximum width and height of */
    DWORD dwStippleHeight;            /* of supported stipple (up to 32x32) */
} D3DPRIMCAPS, *LPD3DPRIMCAPS;

typedef struct _D3DFINDDEVICESEARCH
{
    DWORD               dwSize;
    DWORD               dwFlags;
    BOOL                bHardware;
    D3DCOLORMODEL       dcmColorModel;
    GUID                guid;
    DWORD               dwCaps;
    D3DPRIMCAPS         dpcPrimCaps;
} D3DFINDDEVICESEARCH, *LPD3DFINDDEVICESEARCH;

typedef struct _D3DTRANSFORMCAPS
{
    DWORD dwSize;
    DWORD dwCaps;
} D3DTRANSFORMCAPS, *LPD3DTRANSFORMCAPS;

typedef struct _D3DDEVINFO_TEXTURING
{
    DWORD   dwNumLoads;                 /* counts Load() API calls */
    DWORD   dwApproxBytesLoaded;        /* Approximate number bytes loaded via Load() */
    DWORD   dwNumPreLoads;              /* counts PreLoad() API calls */
    DWORD   dwNumSet;                   /* counts SetTexture() API calls */
    DWORD   dwNumCreates;               /* counts texture creates */
    DWORD   dwNumDestroys;              /* counts texture destroys */
    DWORD   dwNumSetPriorities;         /* counts SetPriority() API calls */
    DWORD   dwNumSetLODs;               /* counts SetLOD() API calls */
    DWORD   dwNumLocks;                 /* counts number of texture locks */
    DWORD   dwNumGetDCs;                /* counts number of GetDCs to textures */
} D3DDEVINFO_TEXTURING, *LPD3DDEVINFO_TEXTURING;

typedef struct _D3DDeviceDesc
{
    DWORD            dwSize;                 /* Size of D3DDEVICEDESC structure */
    DWORD            dwFlags;                /* Indicates which fields have valid data */
    D3DCOLORMODEL    dcmColorModel;          /* Color model of device */
    DWORD            dwDevCaps;              /* Capabilities of device */
    D3DTRANSFORMCAPS dtcTransformCaps;       /* Capabilities of transform */
    BOOL             bClipping;              /* Device can do 3D clipping */
    D3DLIGHTINGCAPS  dlcLightingCaps;        /* Capabilities of lighting */
    D3DPRIMCAPS      dpcLineCaps;
    D3DPRIMCAPS      dpcTriCaps;
    DWORD            dwDeviceRenderBitDepth; /* One of DDBB_8, 16, etc.. */
    DWORD            dwDeviceZBufferBitDepth;/* One of DDBD_16, 32, etc.. */
    DWORD            dwMaxBufferSize;        /* Maximum execute buffer size */
    DWORD            dwMaxVertexCount;       /* Maximum vertex count */
    DWORD        dwMinTextureWidth;
    DWORD        dwMinTextureHeight;
    DWORD        dwMaxTextureWidth;
    DWORD        dwMaxTextureHeight;
    DWORD        dwMinStippleWidth;
    DWORD        dwMaxStippleWidth;
    DWORD        dwMinStippleHeight;
    DWORD        dwMaxStippleHeight;
    DWORD       dwMaxTextureRepeat;
    DWORD       dwMaxTextureAspectRatio;
    DWORD       dwMaxAnisotropy;

    // Guard band that the rasterizer can accommodate
    // Screen-space vertices inside this space but outside the viewport
    // will get clipped properly.
    D3DVALUE    dvGuardBandLeft;
    D3DVALUE    dvGuardBandTop;
    D3DVALUE    dvGuardBandRight;
    D3DVALUE    dvGuardBandBottom;

    D3DVALUE    dvExtentsAdjust;
    DWORD       dwStencilCaps;

    DWORD       dwFVFCaps;
    DWORD       dwTextureOpCaps;
    WORD        wMaxTextureBlendStages;
    WORD        wMaxSimultaneousTextures;
} D3DDEVICEDESC, *LPD3DDEVICEDESC;
typedef struct _D3DDeviceDesc7
{
    DWORD            dwDevCaps;              /* Capabilities of device */
    D3DPRIMCAPS      dpcLineCaps;
    D3DPRIMCAPS      dpcTriCaps;
    DWORD            dwDeviceRenderBitDepth; /* One of DDBB_8, 16, etc.. */
    DWORD            dwDeviceZBufferBitDepth;/* One of DDBD_16, 32, etc.. */

    DWORD       dwMinTextureWidth;
    DWORD       dwMinTextureHeight;
    DWORD       dwMaxTextureWidth;
    DWORD       dwMaxTextureHeight;

    DWORD       dwMaxTextureRepeat;
    DWORD       dwMaxTextureAspectRatio;
    DWORD       dwMaxAnisotropy;

    D3DVALUE    dvGuardBandLeft;
    D3DVALUE    dvGuardBandTop;
    D3DVALUE    dvGuardBandRight;
    D3DVALUE    dvGuardBandBottom;

    D3DVALUE    dvExtentsAdjust;
    DWORD       dwStencilCaps;

    DWORD       dwFVFCaps;
    DWORD       dwTextureOpCaps;
    WORD        wMaxTextureBlendStages;
    WORD        wMaxSimultaneousTextures;

    DWORD       dwMaxActiveLights;
    D3DVALUE    dvMaxVertexW;
    GUID        deviceGUID;

    WORD        wMaxUserClipPlanes;
    WORD        wMaxVertexBlendMatrices;

    DWORD       dwVertexProcessingCaps;

    DWORD       dwReserved1;
    DWORD       dwReserved2;
    DWORD       dwReserved3;
    DWORD       dwReserved4;
} D3DDEVICEDESC7, *LPD3DDEVICEDESC7;

typedef struct _D3DFINDDEVICERESULT
{
    DWORD               dwSize;
    GUID                guid;           /* guid which matched */
    D3DDEVICEDESC       ddHwDesc;       /* hardware D3DDEVICEDESC */
    D3DDEVICEDESC       ddSwDesc;       /* software D3DDEVICEDESC */
} D3DFINDDEVICERESULT, *LPD3DFINDDEVICERESULT;


//
// Interfaces
//
