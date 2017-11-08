// ---------------------------------------------------------------------------
// ---------------------------------------------------------------------------
//
//                              D3D8 Types
//
// ---------------------------------------------------------------------------
// ---------------------------------------------------------------------------

//
// Typedefs
//


//
// Masks
//

mask DWORD d3dUsage8
{
#define D3DUSAGE_RENDERTARGET          0x00000001
#define D3DUSAGE_DEPTHSTENCIL          0x00000002
#define D3DUSAGE_WRITEONLY             0x00000008
#define D3DUSAGE_SOFTWAREPROCESSING    0x00000010
#define D3DUSAGE_DONOTCLIP             0x00000020
#define D3DUSAGE_POINTS                0x00000040
#define D3DUSAGE_RTPATCHES             0x00000080
#define D3DUSAGE_NPATCHES              0x00000100
#define D3DUSAGE_DYNAMIC               0x00000200
};

mask DWORD d3dPresentParamsFlags8
{
#define D3DPRESENTFLAG_LOCKABLE_BACKBUFFER  0x00000001
};

mask DWORD d3dPresentationIntervals8
{
#define D3DPRESENT_INTERVAL_ONE        0x00000001
#define D3DPRESENT_INTERVAL_TWO        0x00000002
#define D3DPRESENT_INTERVAL_THREE      0x00000004
#define D3DPRESENT_INTERVAL_FOUR       0x00000008
#define D3DPRESENT_INTERVAL_IMMEDIATE  0x80000000
};

mask DWORD d3dLockFlags8
{
#define D3DLOCK_READONLY               0x00000010
#define D3DLOCK_DISCARD                0x00002000
#define D3DLOCK_NOOVERWRITE            0x00001000
#define D3DLOCK_NOSYSLOCK              0x00000800
#define D3DLOCK_NO_DIRTY_UPDATE        0x00008000
};

mask DWORD d3dFVF8
{
#define D3DFVF_RESERVED0               0x001
#define D3DFVF_XYZ                     0x002
#define D3DFVF_XYZRHW                  0x004
#define D3DFVF_XYZB2                   0x008
#define D3DFVF_NORMAL                  0x010
#define D3DFVF_PSIZE                   0x020
#define D3DFVF_DIFFUSE                 0x040
#define D3DFVF_SPECULAR                0x080
#define D3DFVF_TEX1                    0x100
#define D3DFVF_TEX2                    0x200
#define D3DFVF_TEX4                    0x400
#define D3DFVF_TEX8                    0x800
#define D3DFVF_LASTBETA_UBYTE4         0x1000
#define D3DFVF_RESERVED                0xFFFFE000
};

mask DWORD d3dBehaviorFlags8
{
#define D3DCREATE_FPU_PRESERVE              0x00000002
#define D3DCREATE_MULTITHREADED             0x00000004
#define D3DCREATE_PUREDEVICE                0x00000010
#define D3DCREATE_SOFTWARE_VERTEXPROCESSING 0x00000020
#define D3DCREATE_HARDWARE_VERTEXPROCESSING 0x00000040
#define D3DCREATE_MIXED_VERTEXPROCESSING    0x00000080
};

mask DWORD d3dClearFlags8
{
#define D3DCLEAR_TARGET                 0x00000001
#define D3DCLEAR_ZBUFFER                0x00000002
#define D3DCLEAR_STENCIL                0x00000004
};

mask DWORD d3dClipStatus8
{
#define D3DCS_LEFT                      0x00000001L
#define D3DCS_RIGHT                     0x00000002L
#define D3DCS_TOP                       0x00000004L
#define D3DCS_BOTTOM                    0x00000008L
#define D3DCS_FRONT                     0x00000010L
#define D3DCS_BACK                      0x00000020L
#define D3DCS_PLANE0                    0x00000040L
#define D3DCS_PLANE1                    0x00000080L
#define D3DCS_PLANE2                    0x00000100L
#define D3DCS_PLANE3                    0x00000200L
#define D3DCS_PLANE4                    0x00000400L
#define D3DCS_PLANE5                    0x00000800L
};

mask DWORD d3dProcessVerticesFlags
{
#define D3DPV_DONOTCOPYDATA             1
};

//
// Values
//


value UINT d3dAdapterID8
{
#define D3DADAPTER_DEFAULT              0
};

value DWORD D3DDEVTYPE
{
#define D3DDEVTYPE_HAL                  1
#define D3DDEVTYPE_REF                  2
#define D3DDEVTYPE_SW                   3
};

value DWORD D3DMULTISAMPLE_TYPE
{
#define D3DMULTISAMPLE_NONE             0
#define D3DMULTISAMPLE_2_SAMPLES        2
#define D3DMULTISAMPLE_3_SAMPLES        3
#define D3DMULTISAMPLE_4_SAMPLES        4
#define D3DMULTISAMPLE_5_SAMPLES        5
#define D3DMULTISAMPLE_6_SAMPLES        6
#define D3DMULTISAMPLE_7_SAMPLES        7
#define D3DMULTISAMPLE_8_SAMPLES        8
#define D3DMULTISAMPLE_9_SAMPLES        9
#define D3DMULTISAMPLE_10_SAMPLES      10
#define D3DMULTISAMPLE_11_SAMPLES      11
#define D3DMULTISAMPLE_12_SAMPLES      12
#define D3DMULTISAMPLE_13_SAMPLES      13
#define D3DMULTISAMPLE_14_SAMPLES      14
#define D3DMULTISAMPLE_15_SAMPLES      15
#define D3DMULTISAMPLE_16_SAMPLES      16
};

value DWORD D3DRESOURCETYPE
{
#define D3DRTYPE_SURFACE                1
#define D3DRTYPE_VOLUME                 2
#define D3DRTYPE_TEXTURE                3
#define D3DRTYPE_VOLUMETEXTURE          4
#define D3DRTYPE_CUBETEXTURE            5
#define D3DRTYPE_VERTEXBUFFER           6
#define D3DRTYPE_INDEXBUFFER            7
};

value DWORD D3DSWAPEFFECT
{
#define D3DSWAPEFFECT_DEFAULT           0
#define D3DSWAPEFFECT_DISCARD           1
#define D3DSWAPEFFECT_FLIP              2
#define D3DSWAPEFFECT_COPY              3
#define D3DSWAPEFFECT_COPY_VSYNC        4
};

value UINT d3dRefreshRate8
{
#define D3DPRESENT_RATE_DEFAULT         0x00000000
#define D3DPRESENT_RATE_UNLIMITED       0x7fffffff
};

value DWORD D3DPOOL
{
#define D3DPOOL_DEFAULT                 0
#define D3DPOOL_MANAGED                 1
#define D3DPOOL_SYSTEMMEM               2
};

value DWORD D3DCUBEMAP_FACES
{
#define D3DCUBEMAP_FACE_POSITIVE_X      0
#define D3DCUBEMAP_FACE_NEGATIVE_X      1
#define D3DCUBEMAP_FACE_POSITIVE_Y      2
#define D3DCUBEMAP_FACE_NEGATIVE_Y      3
#define D3DCUBEMAP_FACE_POSITIVE_Z      4
#define D3DCUBEMAP_FACE_NEGATIVE_Z      5
};

value DWORD D3DBACKBUFFER_TYPE
{
#define D3DBACKBUFFER_TYPE_MONO         0
#define D3DBACKBUFFER_TYPE_LEFT         1
#define D3DBACKBUFFER_TYPE_RIGHT        2
};

value DWORD D3DTRANSFORMSTATETYPE8
{
#define D3DTS_VIEW                      2
#define D3DTS_PROJECTION                3
#define D3DTS_TEXTURE0                 16
#define D3DTS_TEXTURE1                 17
#define D3DTS_TEXTURE2                 18
#define D3DTS_TEXTURE3                 19
#define D3DTS_TEXTURE4                 20
#define D3DTS_TEXTURE5                 21
#define D3DTS_TEXTURE6                 22
#define D3DTS_TEXTURE7                 23
#define D3DTS_WORLD                   256
#define D3DTS_WORLD1                  257
#define D3DTS_WORLD2                  258
#define D3DTS_WORLD3                  259
};

value DWORD D3DLIGHTTYPE8
{
#define D3DLIGHT_POINT                  1
#define D3DLIGHT_SPOT                   2
#define D3DLIGHT_DIRECTIONAL            3
};

value DWORD D3DRENDERSTATETYPE8
{
#define D3DRS_ZENABLE                    7
#define D3DRS_FILLMODE                   8
#define D3DRS_SHADEMODE                  9
#define D3DRS_LINEPATTERN               10
#define D3DRS_ZWRITEENABLE              14
#define D3DRS_ALPHATESTENABLE           15
#define D3DRS_LASTPIXEL                 16
#define D3DRS_SRCBLEND                  19
#define D3DRS_DESTBLEND                 20
#define D3DRS_CULLMODE                  22
#define D3DRS_ZFUNC                     23
#define D3DRS_ALPHAREF                  24
#define D3DRS_ALPHAFUNC                 25
#define D3DRS_DITHERENABLE              26
#define D3DRS_ALPHABLENDENABLE          27
#define D3DRS_FOGENABLE                 28
#define D3DRS_SPECULARENABLE            29
#define D3DRS_ZVISIBLE                  30
#define D3DRS_FOGCOLOR                  34
#define D3DRS_FOGTABLEMODE              35
#define D3DRS_FOGSTART                  36
#define D3DRS_FOGEND                    37
#define D3DRS_FOGDENSITY                38
#define D3DRS_EDGEANTIALIAS             40
#define D3DRS_ZBIAS                     47
#define D3DRS_RANGEFOGENABLE            48
#define D3DRS_STENCILENABLE             52
#define D3DRS_STENCILFAIL               53
#define D3DRS_STENCILZFAIL              54
#define D3DRS_STENCILPASS               55
#define D3DRS_STENCILFUNC               56
#define D3DRS_STENCILREF                57
#define D3DRS_STENCILMASK               58
#define D3DRS_STENCILWRITEMASK          59
#define D3DRS_TEXTUREFACTOR             60
#define D3DRS_WRAP0                    128
#define D3DRS_WRAP1                    129
#define D3DRS_WRAP2                    130
#define D3DRS_WRAP3                    131
#define D3DRS_WRAP4                    132
#define D3DRS_WRAP5                    133
#define D3DRS_WRAP6                    134
#define D3DRS_WRAP7                    135
#define D3DRS_CLIPPING                 136
#define D3DRS_LIGHTING                 137
#define D3DRS_AMBIENT                  139
#define D3DRS_FOGVERTEXMODE            140
#define D3DRS_COLORVERTEX              141
#define D3DRS_LOCALVIEWER              142
#define D3DRS_NORMALIZENORMALS         143
#define D3DRS_DIFFUSEMATERIALSOURCE    145
#define D3DRS_SPECULARMATERIALSOURCE   146
#define D3DRS_AMBIENTMATERIALSOURCE    147
#define D3DRS_EMISSIVEMATERIALSOURCE   148
#define D3DRS_VERTEXBLEND              151
#define D3DRS_CLIPPLANEENABLE          152
#define D3DRS_SOFTWAREVERTEXPROCESSING 153
#define D3DRS_POINTSIZE                154
#define D3DRS_POINTSIZE_MIN            155
#define D3DRS_POINTSPRITEENABLE        156
#define D3DRS_POINTSCALEENABLE         157
#define D3DRS_POINTSCALE_A             158
#define D3DRS_POINTSCALE_B             159
#define D3DRS_POINTSCALE_C             160
#define D3DRS_MULTISAMPLEANTIALIAS     161
#define D3DRS_MULTISAMPLEMASK          162
#define D3DRS_PATCHEDGESTYLE           163
#define D3DRS_PATCHSEGMENTS            164
#define D3DRS_DEBUGMONITORTOKEN        165
#define D3DRS_POINTSIZE_MAX            166
#define D3DRS_INDEXEDVERTEXBLENDENABLE 167
#define D3DRS_COLORWRITEENABLE         168
#define D3DRS_TWEENFACTOR              170
#define D3DRS_BLENDOP                  171
};

value DWORD D3DTEXTURESTAGESTATETYPE8
{
#define D3DTSS_COLOROP                  1
#define D3DTSS_COLORARG1                2
#define D3DTSS_COLORARG2                3
#define D3DTSS_ALPHAOP                  4
#define D3DTSS_ALPHAARG1                5
#define D3DTSS_ALPHAARG2                6
#define D3DTSS_BUMPENVMAT00             7
#define D3DTSS_BUMPENVMAT01             8
#define D3DTSS_BUMPENVMAT10             9
#define D3DTSS_BUMPENVMAT11            10
#define D3DTSS_TEXCOORDINDEX           11
#define D3DTSS_ADDRESSU                13
#define D3DTSS_ADDRESSV                14
#define D3DTSS_BORDERCOLOR             15
#define D3DTSS_MAGFILTER               16
#define D3DTSS_MINFILTER               17
#define D3DTSS_MIPFILTER               18
#define D3DTSS_MIPMAPLODBIAS           19
#define D3DTSS_MAXMIPLEVEL             20
#define D3DTSS_MAXANISOTROPY           21
#define D3DTSS_BUMPENVLSCALE           22
#define D3DTSS_BUMPENVLOFFSET          23
#define D3DTSS_TEXTURETRANSFORMFLAGS   24
#define D3DTSS_ADDRESSW                25
#define D3DTSS_COLORARG0               26
#define D3DTSS_ALPHAARG0               27
#define D3DTSS_RESULTARG               28
};

value DWORD D3DSTATEBLOCKTYPE8
{
#define D3DSBT_ALL                      1
#define D3DSBT_PIXELSTATE               2
#define D3DSBT_VERTEXSTATE              3
};

value DWORD D3DPRIMITIVETYPE8
{
#define D3DPT_POINTLIST                 1
#define D3DPT_LINELIST                  2
#define D3DPT_LINESTRIP                 3
#define D3DPT_TRIANGLELIST              4
#define D3DPT_TRIANGLESTRIP             5
#define D3DPT_TRIANGLEFAN               6
};

value DWORD D3DBASISTYPE
{
#define D3DBASIS_BEZIER                 0
#define D3DBASIS_BSPLINE                1
#define D3DBASIS_INTERPOLATE            2
};

value DWORD D3DORDERTYPE
{
#define D3DORDER_LINEAR                 1
#define D3DORDER_CUBIC                  3
#define D3DORDER_QUINTIC                5
};

value DWORD D3DFORMAT
{
#define D3DFMT_UNKNOWN                  0
#define D3DFMT_R8G8B8                  20
#define D3DFMT_A8R8G8B8                21
#define D3DFMT_X8R8G8B8                22
#define D3DFMT_R5G6B5                  23
#define D3DFMT_X1R5G5B5                24
#define D3DFMT_A1R5G5B5                25
#define D3DFMT_A4R4G4B4                26
#define D3DFMT_R3G3B2                  27
#define D3DFMT_A8                      28
#define D3DFMT_A8R3G3B2                29
#define D3DFMT_X4R4G4B4                30
#define D3DFMT_A8P8                    40
#define D3DFMT_P8                      41
#define D3DFMT_L8                      50
#define D3DFMT_A8L8                    51
#define D3DFMT_A4L4                    52
#define D3DFMT_V8U8                    60
#define D3DFMT_L6V5U5                  61
#define D3DFMT_X8L8V8U8                62
#define D3DFMT_Q8W8V8U8                63
#define D3DFMT_V16U16                  64
#define D3DFMT_W11V11U10               65
#define D3DFMT_UYVY                    0x59565955
#define D3DFMT_YUY2                    0x32595559
#define D3DFMT_DXT1                    0x31545844
#define D3DFMT_DXT2                    0x32545844
#define D3DFMT_DXT3                    0x33545844
#define D3DFMT_DXT4                    0x34545844
#define D3DFMT_DXT5                    0x35545844
#define D3DFMT_D16_LOCKABLE            70
#define D3DFMT_D32                     71
#define D3DFMT_D15S1                   73
#define D3DFMT_D24S8                   75
#define D3DFMT_D16                     80
#define D3DFMT_D24X8                   77
#define D3DFMT_D24X4S4                 79
#define D3DFMT_VERTEXDATA             100
#define D3DFMT_INDEX16                101
#define D3DFMT_INDEX32                102
};


//
// structs
//

typedef struct _D3DDISPLAYMODE
{
    UINT            Width;
    UINT            Height;
    UINT            RefreshRate;
    D3DFORMAT       Format;
} D3DDISPLAYMODE,*LPD3DDISPLAYMODE;

typedef struct _D3DADAPTER_IDENTIFIER8
{
    char            Driver[512];
    char            Description[512];
    DWORD           DriverVersionLowPart;
    DWORD           DriverVersionHighPart;
    DWORD           VendorId;
    DWORD           DeviceId;
    DWORD           SubSysId;
    DWORD           Revision;
    GUID            DeviceIdentifier;
    DWORD           WHQLLevel;
} D3DADAPTER_IDENTIFIER8,*LPD3DADAPTER_IDENTIFIER8;

typedef struct _D3DPRESENT_PARAMETERS_
{
    UINT                      BackBufferWidth;
    UINT                      BackBufferHeight;
    D3DFORMAT                 BackBufferFormat;
    UINT                      BackBufferCount;
    D3DMULTISAMPLE_TYPE       MultiSampleType;
    D3DSWAPEFFECT             SwapEffect;
    HWND                      hDeviceWindow;
    BOOL                      Windowed;
    BOOL                      EnableAutoDepthStencil;
    D3DFORMAT                 AutoDepthStencilFormat;
    d3dPresentParamsFlags8    Flags;
    d3dRefreshRate8           FullScreen_RefreshRateInHz;
    d3dPresentationIntervals8 FullScreen_PresentationInterval;
} D3DPRESENT_PARAMETERS,*LPD3DPRESENT_PARAMETERS;

typedef struct _D3DSURFACE_DESC
{
    D3DFORMAT           Format;
    D3DRESOURCETYPE     Type;
    d3dUsage8           Usage;
    D3DPOOL             Pool;
    UINT                Size;
    D3DMULTISAMPLE_TYPE MultiSampleType;
    UINT                Width;
    UINT                Height;
} D3DSURFACE_DESC,*LPD3DSURFACE_DESC;

typedef struct _D3DVOLUME_DESC
{
    D3DFORMAT           Format;
    D3DRESOURCETYPE     Type;
    d3dUsage8           Usage;
    D3DPOOL             Pool;
    UINT                Size;
    UINT                Width;
    UINT                Height;
    UINT                Depth;
} D3DVOLUME_DESC,*LPD3DVOLUME_DESC;

typedef struct _D3DLOCKED_RECT
{
    INT                 Pitch;
    LPVOID              pBits;
} D3DLOCKED_RECT,*LPD3DLOCKED_RECT;

typedef struct _D3DBOX
{
    UINT                Left;
    UINT                Top;
    UINT                Right;
    UINT                Bottom;
    UINT                Front;
    UINT                Back;
} D3DBOX,*LPD3DBOX;

typedef struct _D3DLOCKED_BOX
{
    INT                 RowPitch;
    INT                 SlicePitch;
    LPVOID              pBits;
} D3DLOCKED_BOX,*LPD3DLOCKED_BOX;

typedef struct _D3DVERTEXBUFFER_DESC
{
    D3DFORMAT           Format;
    D3DRESOURCETYPE     Type;
    d3dUsage8           Usage;
    D3DPOOL             Pool;
    UINT                Size;
    d3dFVF8             FVF;

} D3DVERTEXBUFFER_DESC,*LPD3DVERTEXBUFFER_DESC;

typedef struct _D3DINDEXBUFFER_DESC
{
    D3DFORMAT           Format;
    D3DRESOURCETYPE     Type;
    d3dUsage8           Usage;
    D3DPOOL             Pool;
    UINT                Size;
} D3DINDEXBUFFER_DESC,*LPD3DINDEXBUFFER_DESC;

typedef struct _D3DDEVICE_CREATION_PARAMETERS
{
    d3dAdapterID8       AdapterOrdinal;
    D3DDEVTYPE          DeviceType;
    HWND                hFocusWindow;
    d3dBehaviorFlags8   BehaviorFlags;
} D3DDEVICE_CREATION_PARAMETERS,*LPD3DDEVICE_CREATION_PARAMETERS;

typedef struct _D3DRASTER_STATUS
{
    BOOL            InVBlank;
    UINT            ScanLine;
} D3DRASTER_STATUS,*LPD3DRASTER_STATUS;

typedef struct _D3DMATRIX8
{
    float        _11;
    float        _12;
    float        _13;
    float        _14;
    float        _21;
    float        _22;
    float        _23;
    float        _24;
    float        _31;
    float        _32;
    float        _33;
    float        _34;
    float        _41;
    float        _42;
    float        _43;
    float        _44;
} D3DMATRIX8,*LPD3DMATRIX8;

typedef struct _D3DVIEWPORT8
{
    UINT        X;
    UINT        Y;
    UINT        Width;
    UINT        Height;
    float       MinZ;
    float       MaxZ;
} D3DVIEWPORT8,*LPD3DVIEWPORT8;

typedef struct _D3DMATERIAL8
{
    D3DCOLORVALUE   Diffuse;
    D3DCOLORVALUE   Ambient;
    D3DCOLORVALUE   Specular;
    D3DCOLORVALUE   Emissive;
    float           Power;
} D3DMATERIAL8,*LPD3DMATERIAL8;

typedef struct _D3DCLIPSTATUS8 {
    d3dClipStatus8  ClipUnion;
    d3dClipStatus8  ClipIntersection;
} D3DCLIPSTATUS8,*LPD3DCLIPSTATUS8;

typedef struct _D3DLIGHT8 {
    D3DLIGHTTYPE8   Type;
    D3DCOLORVALUE   Diffuse;
    D3DCOLORVALUE   Specular;
    D3DCOLORVALUE   Ambient;
    D3DVECTOR       Position;
    D3DVECTOR       Direction;
    float           Range;
    float           Falloff;
    float           Attenuation0;
    float           Attenuation1;
    float           Attenuation2;
    float           Theta;
    float           Phi;
} D3DLIGHT8,*LPD3DLIGHT8;

typedef struct _D3DRECTPATCH_INFO
{
    UINT                StartVertexOffsetWidth;
    UINT                StartVertexOffsetHeight;
    UINT                Width;
    UINT                Height;
    UINT                Stride;
    D3DBASISTYPE        Basis;
    D3DORDERTYPE        Order;
} D3DRECTPATCH_INFO,*LPD3DRECTPATCH_INFO;

typedef struct _D3DTRIPATCH_INFO
{
    UINT                StartVertexOffset;
    UINT                NumVertices;
    D3DBASISTYPE        Basis;
    D3DORDERTYPE        Order;
} D3DTRIPATCH_INFO,*LPD3DTRIPATCH_INFO;

typedef struct _D3DGAMMARAMP
{
    WORD                red  [256];
    WORD                green[256];
    WORD                blue [256];
} D3DGAMMARAMP,*LPD3DGAMMARAMP;

alias LPD3DDISPLAYMODE;
alias LPD3DADAPTER_IDENTIFIER8;
alias LPD3DPRESENT_PARAMETERS;
alias LPD3DSURFACE_DESC;
alias LPD3DVOLUME_DESC;
alias LPD3DLOCKED_RECT;
alias LPD3DBOX;
alias LPD3DLOCKED_BOX;
alias LPD3DVERTEXBUFFER_DESC;
alias LPD3DINDEXBUFFER_DESC;
alias LPD3DDEVICE_CREATION_PARAMETERS;
alias LPD3DRASTER_STATUS;
alias LPD3DMATRIX8;
alias LPD3DVIEWPORT8;
alias LPD3DVECTOR;
alias LPD3DCOLORVALUE;
alias LPD3DRECT;
alias LPD3DMATERIAL8;
alias LPD3DCLIPSTATUS8;
alias LPD3DLIGHT8;
alias LPD3DRECTPATCH_INFO;
alias LPD3DTRIPATCH_INFO;
alias LPD3DGAMMARAMP;
