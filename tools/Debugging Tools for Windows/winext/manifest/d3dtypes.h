

//
// GUIDs
//



//
// Typedefs
//

typedef float D3DVALUE;
typedef float *LPD3DVALUE;
typedef LONG D3DFIXED;
typedef VOID *LPD3DVALIDATECALLBACK;
typedef VOID *LPD3DENUMTEXTUREFORMATSCALLBACK;
typedef VOID *LPD3DENUMPIXELFORMATSCALLBACK;
typedef DWORD D3DMATERIALHANDLE;
typedef DWORD *LPD3DMATERIALHANDLE;
typedef DWORD D3DTEXTUREHANDLE;
typedef DWORD *LPD3DTEXTUREHANDLE;
typedef DWORD D3DMATRIXHANDLE;
typedef DWORD *LPD3DMATRIXHANDLE;
typedef DWORD D3DCOLORMODEL;
typedef DWORD D3DCOLOR;

typedef struct _D3DCOLORVALUE
{
    D3DVALUE dvR;
    D3DVALUE dvG;
    D3DVALUE dvB;
    D3DVALUE dvA;
} D3DCOLORVALUE, *LPD3DCOLORVALUE;

typedef struct _D3DVECTOR
{
    D3DVALUE dvX;
    D3DVALUE dvY;
    D3DVALUE dvZ;
} D3DVECTOR, *LPD3DVECTOR;

typedef struct _D3DRECT
{
    LONG lX1;
    LONG lY1;
    LONG lX2;
    LONG lY2;
} D3DRECT, *LPD3DRECT;


//
// Masks
//

mask DWORD d3dpvFlags
{
    #define D3DPV_DONOTCOPYDATA                      1
};

mask DWORD d3dclearFlags
{
    #define D3DCLEAR_TARGET                          0x00000001l
    #define D3DCLEAR_ZBUFFER                         0x00000002l
    #define D3DCLEAR_STENCIL                         0x00000004l
};

mask DWORD d3dclipFlags
{
    #define D3DCLIP_LEFT                             0x00000001L
    #define D3DCLIP_RIGHT                            0x00000002L
    #define D3DCLIP_TOP                              0x00000004L
    #define D3DCLIP_BOTTOM                           0x00000008L
    #define D3DCLIP_FRONT                            0x00000010L
    #define D3DCLIP_BACK                             0x00000020L
    #define D3DCLIP_GEN0                             0x00000040L
    #define D3DCLIP_GEN1                             0x00000080L
    #define D3DCLIP_GEN2                             0x00000100L
    #define D3DCLIP_GEN3                             0x00000200L
    #define D3DCLIP_GEN4                             0x00000400L
    #define D3DCLIP_GEN5                             0x00000800L
};

mask DWORD d3dtriflagFlags
{
    #define D3DTRIFLAG_START                         0x00000000L
    #define D3DTRIFLAG_ODD                           0x0000001eL
    #define D3DTRIFLAG_EVEN                          0x0000001fL
    #define D3DTRIFLAG_EDGEENABLE1                   0x00000100L
    #define D3DTRIFLAG_EDGEENABLE2                   0x00000200L
    #define D3DTRIFLAG_EDGEENABLE3                   0x00000400L
};

mask DWORD d3dclipstatusFlags
{
    #define D3DCLIPSTATUS_STATUS                     0x00000001L
    #define D3DCLIPSTATUS_EXTENTS2                   0x00000002L
    #define D3DCLIPSTATUS_EXTENTS3                   0x00000004L
};

mask DWORD d3dsetstatusFlags
{
    #define D3DSETSTATUS_STATUS                      0x00000001L
    #define D3DSETSTATUS_EXTENTS                     0x00000002L
};

mask DWORD d3dwrapcoordFlags
{
    #define D3DWRAPCOORD_0                           0x00000001L
    #define D3DWRAPCOORD_1                           0x00000002L
    #define D3DWRAPCOORD_2                           0x00000004L
    #define D3DWRAPCOORD_3                           0x00000008L
};

mask DWORD d3dvisFlags
{
    #define D3DVIS_INSIDE_FRUSTUM                    0
    #define D3DVIS_INTERSECT_FRUSTUM                 1
    #define D3DVIS_OUTSIDE_FRUSTUM                   2
    #define D3DVIS_INSIDE_LEFT                       0
    #define D3DVIS_INTERSECT_LEFT                    4
    #define D3DVIS_OUTSIDE_LEFT                      8
    #define D3DVIS_INSIDE_RIGHT                      0
    #define D3DVIS_INTERSECT_RIGHT                   16
    #define D3DVIS_OUTSIDE_RIGHT                     32
    #define D3DVIS_INSIDE_TOP                        0
    #define D3DVIS_INTERSECT_TOP                     64
    #define D3DVIS_OUTSIDE_TOP                       128
    #define D3DVIS_INSIDE_BOTTOM                     0
    #define D3DVIS_INTERSECT_BOTTOM                  256
    #define D3DVIS_OUTSIDE_BOTTOM                    512
    #define D3DVIS_INSIDE_NEAR                       0
    #define D3DVIS_INTERSECT_NEAR                    1024
    #define D3DVIS_OUTSIDE_NEAR                      2048
    #define D3DVIS_INSIDE_FAR                        0
    #define D3DVIS_INTERSECT_FAR                     4096
    #define D3DVIS_OUTSIDE_FAR                       8192
};

mask DWORD d3dexecuteFlags
{
    #define D3DEXECUTE_CLIPPED                       0x00000001l
    #define D3DEXECUTE_UNCLIPPED                     0x00000002l
};

mask DWORD d3dvopFlags
{
    #define D3DVOP_LIGHT                             1024
    #define D3DVOP_TRANSFORM                         1
    #define D3DVOP_CLIP                              4
    #define D3DVOP_EXTENTS                           8
};

mask DWORD d3dtaFlags
{
    #define D3DTA_SELECTMASK                         0x0000000f
    #define D3DTA_DIFFUSE                            0x00000000
    #define D3DTA_CURRENT                            0x00000001
    #define D3DTA_TEXTURE                            0x00000002
    #define D3DTA_TFACTOR                            0x00000003
    #define D3DTA_SPECULAR                           0x00000004
    #define D3DTA_COMPLEMENT                         0x00000010
    #define D3DTA_ALPHAREPLICATE                     0x00000020
};

mask DWORD d3ddevinfoidFlags
{
    #define D3DDEVINFOID_TEXTUREMANAGER              1
    #define D3DDEVINFOID_D3DTEXTUREMANAGER           2
    #define D3DDEVINFOID_TEXTURING                   3
};

mask DWORD d3dlightFlags
{
    #define D3DLIGHT_ACTIVE                          0x00000001
    #define D3DLIGHT_NO_SPECULAR                     0x00000002
    #define D3DLIGHT_ALL                             3
};

mask DWORD d3dwrapFlags
{
    #define D3DWRAP_U                                0x00000001L
    #define D3DWRAP_V                                0x00000002L
};

mask DWORD d3dtssFlags
{
    #define D3DTSS_TCI_PASSTHRU                      0x00000000
    #define D3DTSS_TCI_CAMERASPACENORMAL             0x00010000
    #define D3DTSS_TCI_CAMERASPACEPOSITION           0x00020000
    #define D3DTSS_TCI_CAMERASPACEREFLECTIONVECTOR   0x00030000
};

mask DWORD d3dtransformFlags
{
    #define D3DTRANSFORM_CLIPPED                     0x00000001l
    #define D3DTRANSFORM_UNCLIPPED                   0x00000002l
};

mask DWORD d3dpalFlags
{
    #define D3DPAL_FREE                              0x00
    #define D3DPAL_READONLY                          0x40
    #define D3DPAL_RESERVED                          0x80
};

mask DWORD d3dvbcapsFlags
{
    #define D3DVBCAPS_SYSTEMMEMORY                   0x00000800l
    #define D3DVBCAPS_WRITEONLY                      0x00010000l
    #define D3DVBCAPS_OPTIMIZED                      0x80000000l
    #define D3DVBCAPS_DONOTCLIP                      0x00000001l
};

mask DWORD d3dcolorFlags
{
    #define D3DCOLOR_MONO                            1
    #define D3DCOLOR_RGB                             2
};

mask DWORD d3denumretFlags
{
    #define D3DENUMRET_CANCEL                        1
    #define D3DENUMRET_OK                            0
};

mask DWORD d3dstateFlags
{
    #define D3DSTATE_OVERRIDE_BIAS                   256
};

mask DWORD d3drenderstateFlags
{
    #define D3DRENDERSTATE_BLENDENABLE               27
    #define D3DRENDERSTATE_WRAPBIAS                  128UL
};

mask DWORD d3dfvfFlags
{
    #define D3DFVF_RESERVED0                         0x001
    #define D3DFVF_POSITION_MASK                     0x00E
    #define D3DFVF_XYZ                               0x002
    #define D3DFVF_XYZRHW                            0x004
    #define D3DFVF_XYZB1                             0x006
    #define D3DFVF_XYZB2                             0x008
    #define D3DFVF_XYZB3                             0x00a
    #define D3DFVF_XYZB4                             0x00c
    #define D3DFVF_XYZB5                             0x00e
    #define D3DFVF_NORMAL                            0x010
    #define D3DFVF_RESERVED1                         0x020
    #define D3DFVF_DIFFUSE                           0x040
    #define D3DFVF_SPECULAR                          0x080
    #define D3DFVF_TEXCOUNT_MASK                     0xf00
    #define D3DFVF_TEXCOUNT_SHIFT                    8
    #define D3DFVF_TEX0                              0x000
    #define D3DFVF_TEX1                              0x100
    #define D3DFVF_TEX2                              0x200
    #define D3DFVF_TEX3                              0x300
    #define D3DFVF_TEX4                              0x400
    #define D3DFVF_TEX5                              0x500
    #define D3DFVF_TEX6                              0x600
    #define D3DFVF_TEX7                              0x700
    #define D3DFVF_TEX8                              0x800
    #define D3DFVF_RESERVED2                         0xf000
    #define D3DFVF_VERTEX                            0x00000112
    #define D3DFVF_LVERTEX                           0x000001E2
    #define D3DFVF_TLVERTEX                          0x000001C4
    #define D3DFVF_TEXTUREFORMAT2                    0
    #define D3DFVF_TEXTUREFORMAT1                    3
    #define D3DFVF_TEXTUREFORMAT3                    1
    #define D3DFVF_TEXTUREFORMAT4                    2
};

mask DWORD d3dstatusFlags
{
    #define D3DSTATUS_CLIPUNIONLEFT                  1
    #define D3DSTATUS_CLIPUNIONRIGHT                 2
    #define D3DSTATUS_CLIPUNIONTOP                   4
    #define D3DSTATUS_CLIPUNIONBOTTOM                8
    #define D3DSTATUS_CLIPUNIONFRONT                 16
    #define D3DSTATUS_CLIPUNIONBACK                  32
    #define D3DSTATUS_CLIPUNIONGEN0                  64
    #define D3DSTATUS_CLIPUNIONGEN1                  128
    #define D3DSTATUS_CLIPUNIONGEN2                  256
    #define D3DSTATUS_CLIPUNIONGEN3                  512
    #define D3DSTATUS_CLIPUNIONGEN4                  1024
    #define D3DSTATUS_CLIPUNIONGEN5                  2048
    #define D3DSTATUS_CLIPINTERSECTIONLEFT           0x00001000L
    #define D3DSTATUS_CLIPINTERSECTIONRIGHT          0x00002000L
    #define D3DSTATUS_CLIPINTERSECTIONTOP            0x00004000L
    #define D3DSTATUS_CLIPINTERSECTIONBOTTOM         0x00008000L
    #define D3DSTATUS_CLIPINTERSECTIONFRONT          0x00010000L
    #define D3DSTATUS_CLIPINTERSECTIONBACK           0x00020000L
    #define D3DSTATUS_CLIPINTERSECTIONGEN0           0x00040000L
    #define D3DSTATUS_CLIPINTERSECTIONGEN1           0x00080000L
    #define D3DSTATUS_CLIPINTERSECTIONGEN2           0x00100000L
    #define D3DSTATUS_CLIPINTERSECTIONGEN3           0x00200000L
    #define D3DSTATUS_CLIPINTERSECTIONGEN4           0x00400000L
    #define D3DSTATUS_CLIPINTERSECTIONGEN5           0x00800000L
    #define D3DSTATUS_ZNOTVISIBLE                    0x01000000L
    #define D3DSTATUS_CLIPUNIONALL                   0x00000FFFL
    #define D3DSTATUS_CLIPINTERSECTIONALL            0x00FFF000L
    #define D3DSTATUS_DEFAULT                        0x01FFF000L
};

mask DWORD d3dprocessverticesFlags
{
    #define D3DPROCESSVERTICES_TRANSFORMLIGHT        0x00000000L
    #define D3DPROCESSVERTICES_TRANSFORM             0x00000001L
    #define D3DPROCESSVERTICES_COPY                  0x00000002L
    #define D3DPROCESSVERTICES_OPMASK                0x00000007L
    #define D3DPROCESSVERTICES_UPDATEEXTENTS         0x00000008L
    #define D3DPROCESSVERTICES_NOCOLOR               0x00000010L
};



//
// Values
//

value DWORD D3DBLEND
{
    #define D3DBLEND_ZERO                            1
    #define D3DBLEND_ONE                             2
    #define D3DBLEND_SRCCOLOR                        3
    #define D3DBLEND_INVSRCCOLOR                     4
    #define D3DBLEND_SRCALPHA                        5
    #define D3DBLEND_INVSRCALPHA                     6
    #define D3DBLEND_DESTALPHA                       7
    #define D3DBLEND_INVDESTALPHA                    8
    #define D3DBLEND_DESTCOLOR                       9
    #define D3DBLEND_INVDESTCOLOR                    10
    #define D3DBLEND_SRCALPHASAT                     11
    #define D3DBLEND_BOTHSRCALPHA                    12
    #define D3DBLEND_BOTHINVSRCALPHA                 13
    #define D3DBLEND_FORCE_DWORD                     0x7fffffff
};

value DWORD D3DTEXTURESTAGESTATETYPE
{
    #define D3DTSS_COLOROP                           1
    #define D3DTSS_COLORARG1                         2
    #define D3DTSS_COLORARG2                         3
    #define D3DTSS_ALPHAOP                           4
    #define D3DTSS_ALPHAARG1                         5
    #define D3DTSS_ALPHAARG2                         6
    #define D3DTSS_BUMPENVMAT00                      7
    #define D3DTSS_BUMPENVMAT01                      8
    #define D3DTSS_BUMPENVMAT10                      9
    #define D3DTSS_BUMPENVMAT11                      10
    #define D3DTSS_TEXCOORDINDEX                     11
    #define D3DTSS_ADDRESS                           12
    #define D3DTSS_ADDRESSU                          13
    #define D3DTSS_ADDRESSV                          14
    #define D3DTSS_BORDERCOLOR                       15
    #define D3DTSS_MAGFILTER                         16
    #define D3DTSS_MINFILTER                         17
    #define D3DTSS_MIPFILTER                         18
    #define D3DTSS_MIPMAPLODBIAS                     19
    #define D3DTSS_MAXMIPLEVEL                       20
    #define D3DTSS_MAXANISOTROPY                     21
    #define D3DTSS_BUMPENVLSCALE                     22
    #define D3DTSS_BUMPENVLOFFSET                    23
    #define D3DTSS_TEXTURETRANSFORMFLAGS             24
    #define D3DTSS_ADDRESSW                          25
    #define D3DTSS_COLORARG0                         26
    #define D3DTSS_ALPHAARG0                         27
    #define D3DTSS_RESULTARG                         28
    #define D3DTSS_FORCE_DWORD                       0x7fffffff
};

value DWORD D3DSHADEMODE
{
    #define D3DSHADE_FLAT                            1
    #define D3DSHADE_GOURAUD                         2
    #define D3DSHADE_PHONG                           3
    #define D3DSHADE_FORCE_DWORD                     0x7fffffff
};

value DWORD D3DTEXTUREMAGFILTER
{
    #define D3DTFG_POINT                             1
    #define D3DTFG_LINEAR                            2
    #define D3DTFG_FLATCUBIC                         3
    #define D3DTFG_GAUSSIANCUBIC                     4
    #define D3DTFG_ANISOTROPIC                       5
    #define D3DTFG_FORCE_DWORD                       0x7fffffff
};

value DWORD D3DCMPFUNC
{
    #define D3DCMP_NEVER                             1
    #define D3DCMP_LESS                              2
    #define D3DCMP_EQUAL                             3
    #define D3DCMP_LESSEQUAL                         4
    #define D3DCMP_GREATER                           5
    #define D3DCMP_NOTEQUAL                          6
    #define D3DCMP_GREATEREQUAL                      7
    #define D3DCMP_ALWAYS                            8
    #define D3DCMP_FORCE_DWORD                       0x7fffffff
};

value DWORD D3DFILLMODE
{
    #define D3DFILL_POINT                            1
    #define D3DFILL_WIREFRAME                        2
    #define D3DFILL_SOLID                            3
    #define D3DFILL_FORCE_DWORD                      0x7fffffff
};

value DWORD D3DSTATEBLOCKTYPE
{
    #define D3DSBT_ALL                               1
    #define D3DSBT_PIXELSTATE                        2
    #define D3DSBT_VERTEXSTATE                       3
    #define D3DSBT_FORCE_DWORD                       0xffffffff
};

value DWORD D3DVERTEXBLENDFLAGS
{
    #define D3DVBLEND_DISABLE                        0
    #define D3DVBLEND_1WEIGHT                        1
    #define D3DVBLEND_2WEIGHTS                       2
    #define D3DVBLEND_3WEIGHTS                       3
};

value DWORD D3DTEXTUREOP
{
    #define D3DTOP_DISABLE                           1
    #define D3DTOP_SELECTARG1                        2
    #define D3DTOP_SELECTARG2                        3
    #define D3DTOP_MODULATE                          4
    #define D3DTOP_MODULATE2X                        5
    #define D3DTOP_MODULATE4X                        6
    #define D3DTOP_ADD                               7
    #define D3DTOP_ADDSIGNED                         8
    #define D3DTOP_ADDSIGNED2X                       9
    #define D3DTOP_SUBTRACT                          10
    #define D3DTOP_ADDSMOOTH                         11
    #define D3DTOP_BLENDDIFFUSEALPHA                 12
    #define D3DTOP_BLENDTEXTUREALPHA                 13
    #define D3DTOP_BLENDFACTORALPHA                  14
    #define D3DTOP_BLENDTEXTUREALPHAPM               15
    #define D3DTOP_BLENDCURRENTALPHA                 16
    #define D3DTOP_PREMODULATE                       17
    #define D3DTOP_MODULATEALPHA_ADDCOLOR            18
    #define D3DTOP_MODULATECOLOR_ADDALPHA            19
    #define D3DTOP_MODULATEINVALPHA_ADDCOLOR         20
    #define D3DTOP_MODULATEINVCOLOR_ADDALPHA         21
    #define D3DTOP_BUMPENVMAP                        22
    #define D3DTOP_BUMPENVMAPLUMINANCE               23
    #define D3DTOP_DOTPRODUCT3                       24
    #define D3DTOP_FORCE_DWORD                       0x7fffffff
};

value DWORD D3DCULL
{
    #define D3DCULL_NONE                             1
    #define D3DCULL_CW                               2
    #define D3DCULL_CCW                              3
    #define D3DCULL_FORCE_DWORD                      0x7fffffff
};

value DWORD D3DSTENCILOP
{
    #define D3DSTENCILOP_KEEP                        1
    #define D3DSTENCILOP_ZERO                        2
    #define D3DSTENCILOP_REPLACE                     3
    #define D3DSTENCILOP_INCRSAT                     4
    #define D3DSTENCILOP_DECRSAT                     5
    #define D3DSTENCILOP_INVERT                      6
    #define D3DSTENCILOP_INCR                        7
    #define D3DSTENCILOP_DECR                        8
    #define D3DSTENCILOP_FORCE_DWORD                 0x7fffffff
};

value DWORD D3DTEXTUREBLEND
{
    #define D3DTBLEND_DECAL                          1
    #define D3DTBLEND_MODULATE                       2
    #define D3DTBLEND_DECALALPHA                     3
    #define D3DTBLEND_MODULATEALPHA                  4
    #define D3DTBLEND_DECALMASK                      5
    #define D3DTBLEND_MODULATEMASK                   6
    #define D3DTBLEND_COPY                           7
    #define D3DTBLEND_ADD                            8
    #define D3DTBLEND_FORCE_DWORD                    0x7fffffff
};

value DWORD D3DZBUFFERTYPE
{
    #define D3DZB_FALSE                              0
    #define D3DZB_TRUE                               1
    #define D3DZB_USEW                               2
    #define D3DZB_FORCE_DWORD                        0x7fffffff
};

value DWORD D3DTEXTUREFILTER
{
    #define D3DFILTER_NEAREST                        1
    #define D3DFILTER_LINEAR                         2
    #define D3DFILTER_MIPNEAREST                     3
    #define D3DFILTER_MIPLINEAR                      4
    #define D3DFILTER_LINEARMIPNEAREST               5
    #define D3DFILTER_LINEARMIPLINEAR                6
    #define D3DFILTER_FORCE_DWORD                    0x7fffffff
};

value DWORD D3DTEXTUREADDRESS
{
    #define D3DTADDRESS_WRAP                         1
    #define D3DTADDRESS_MIRROR                       2
    #define D3DTADDRESS_CLAMP                        3
    #define D3DTADDRESS_BORDER                       4
    #define D3DTADDRESS_FORCE_DWORD                  0x7fffffff
};

value DWORD D3DTEXTURETRANSFORMFLAGS
{
    #define D3DTTFF_DISABLE                          0
    #define D3DTTFF_COUNT1                           1
    #define D3DTTFF_COUNT2                           2
    #define D3DTTFF_COUNT3                           3
    #define D3DTTFF_COUNT4                           4
    #define D3DTTFF_PROJECTED                        256
    #define D3DTTFF_FORCE_DWORD                      0x7fffffff
};

value DWORD D3DLIGHTTYPE
{
    #define D3DLIGHT_POINT                           1
    #define D3DLIGHT_SPOT                            2
    #define D3DLIGHT_DIRECTIONAL                     3
    #define D3DLIGHT_PARALLELPOINT                   4
    #define D3DLIGHT_GLSPOT                          5
    #define D3DLIGHT_FORCE_DWORD                     0x7fffffff
};

value DWORD D3DOPCODE
{
    #define D3DOP_POINT                              1
    #define D3DOP_LINE                               2
    #define D3DOP_TRIANGLE                           3
    #define D3DOP_MATRIXLOAD                         4
    #define D3DOP_MATRIXMULTIPLY                     5
    #define D3DOP_STATETRANSFORM                     6
    #define D3DOP_STATELIGHT                         7
    #define D3DOP_STATERENDER                        8
    #define D3DOP_PROCESSVERTICES                    9
    #define D3DOP_TEXTURELOAD                        10
    #define D3DOP_EXIT                               11
    #define D3DOP_BRANCHFORWARD                      12
    #define D3DOP_SPAN                               13
    #define D3DOP_SETSTATUS                          14
    #define D3DOP_FORCE_DWORD                        0x7fffffff
};

value DWORD D3DTEXTUREMINFILTER
{
    #define D3DTFN_POINT                             1
    #define D3DTFN_LINEAR                            2
    #define D3DTFN_ANISOTROPIC                       3
    #define D3DTFN_FORCE_DWORD                       0x7fffffff
};

value DWORD D3DLIGHTSTATETYPE
{
    #define D3DLIGHTSTATE_MATERIAL                   1
    #define D3DLIGHTSTATE_AMBIENT                    2
    #define D3DLIGHTSTATE_COLORMODEL                 3
    #define D3DLIGHTSTATE_FOGMODE                    4
    #define D3DLIGHTSTATE_FOGSTART                   5
    #define D3DLIGHTSTATE_FOGEND                     6
    #define D3DLIGHTSTATE_FOGDENSITY                 7
    #define D3DLIGHTSTATE_COLORVERTEX                8
    #define D3DLIGHTSTATE_FORCE_DWORD                0x7fffffff
};

value DWORD D3DANTIALIASMODE
{
    #define D3DANTIALIAS_NONE                        0
    #define D3DANTIALIAS_SORTDEPENDENT               1
    #define D3DANTIALIAS_SORTINDEPENDENT             2
    #define D3DANTIALIAS_FORCE_DWORD                 0x7fffffff
};

value DWORD D3DPRIMITIVETYPE
{
    #define D3DPT_POINTLIST                          1
    #define D3DPT_LINELIST                           2
    #define D3DPT_LINESTRIP                          3
    #define D3DPT_TRIANGLELIST                       4
    #define D3DPT_TRIANGLESTRIP                      5
    #define D3DPT_TRIANGLEFAN                        6
    #define D3DPT_FORCE_DWORD                        0x7fffffff
};

value DWORD D3DTEXTUREMIPFILTER
{
    #define D3DTFP_NONE                              1
    #define D3DTFP_POINT                             2
    #define D3DTFP_LINEAR                            3
    #define D3DTFP_FORCE_DWORD                       0x7fffffff
};

value DWORD D3DMATERIALCOLORSOURCE
{
    #define D3DMCS_MATERIAL                          0
    #define D3DMCS_COLOR1                            1
    #define D3DMCS_COLOR2                            2
    #define D3DMCS_FORCE_DWORD                       0x7fffffff
};

value DWORD D3DTRANSFORMSTATETYPE
{
    #define D3DTRANSFORMSTATE_WORLD                  1
    #define D3DTRANSFORMSTATE_VIEW                   2
    #define D3DTRANSFORMSTATE_PROJECTION             3
    #define D3DTRANSFORMSTATE_WORLD1                 4
    #define D3DTRANSFORMSTATE_WORLD2                 5
    #define D3DTRANSFORMSTATE_WORLD3                 6
    #define D3DTRANSFORMSTATE_TEXTURE0               16
    #define D3DTRANSFORMSTATE_TEXTURE1               17
    #define D3DTRANSFORMSTATE_TEXTURE2               18
    #define D3DTRANSFORMSTATE_TEXTURE3               19
    #define D3DTRANSFORMSTATE_TEXTURE4               20
    #define D3DTRANSFORMSTATE_TEXTURE5               21
    #define D3DTRANSFORMSTATE_TEXTURE6               22
    #define D3DTRANSFORMSTATE_TEXTURE7               23
    #define D3DTRANSFORMSTATE_FORCE_DWORD            0x7fffffff
};

value DWORD D3DVERTEXTYPE
{
    #define D3DVT_VERTEX                             1
    #define D3DVT_LVERTEX                            2
    #define D3DVT_TLVERTEX                           3
    #define D3DVT_FORCE_DWORD                        0x7fffffff
};

value DWORD D3DFOGMODE
{
    #define D3DFOG_NONE                              0
    #define D3DFOG_EXP                               1
    #define D3DFOG_EXP2                              2
    #define D3DFOG_LINEAR                            3
    #define D3DFOG_FORCE_DWORD                       0x7fffffff
};

value DWORD D3DRENDERSTATETYPE
{
    #define D3DRENDERSTATE_ANTIALIAS                 2
    #define D3DRENDERSTATE_TEXTUREPERSPECTIVE        4
    #define D3DRENDERSTATE_ZENABLE                   7
    #define D3DRENDERSTATE_FILLMODE                  8
    #define D3DRENDERSTATE_SHADEMODE                 9
    #define D3DRENDERSTATE_LINEPATTERN               10
    #define D3DRENDERSTATE_ZWRITEENABLE              14
    #define D3DRENDERSTATE_ALPHATESTENABLE           15
    #define D3DRENDERSTATE_LASTPIXEL                 16
    #define D3DRENDERSTATE_SRCBLEND                  19
    #define D3DRENDERSTATE_DESTBLEND                 20
    #define D3DRENDERSTATE_CULLMODE                  22
    #define D3DRENDERSTATE_ZFUNC                     23
    #define D3DRENDERSTATE_ALPHAREF                  24
    #define D3DRENDERSTATE_ALPHAFUNC                 25
    #define D3DRENDERSTATE_DITHERENABLE              26
    #define D3DRENDERSTATE_ALPHABLENDENABLE          27
    #define D3DRENDERSTATE_FOGENABLE                 28
    #define D3DRENDERSTATE_SPECULARENABLE            29
    #define D3DRENDERSTATE_ZVISIBLE                  30
    #define D3DRENDERSTATE_STIPPLEDALPHA             33
    #define D3DRENDERSTATE_FOGCOLOR                  34
    #define D3DRENDERSTATE_FOGTABLEMODE              35
    #define D3DRENDERSTATE_FOGSTART                  36
    #define D3DRENDERSTATE_FOGEND                    37
    #define D3DRENDERSTATE_FOGDENSITY                38
    #define D3DRENDERSTATE_EDGEANTIALIAS             40
    #define D3DRENDERSTATE_COLORKEYENABLE            41
    #define D3DRENDERSTATE_ZBIAS                     47
    #define D3DRENDERSTATE_RANGEFOGENABLE            48
    #define D3DRENDERSTATE_STENCILENABLE             52
    #define D3DRENDERSTATE_STENCILFAIL               53
    #define D3DRENDERSTATE_STENCILZFAIL              54
    #define D3DRENDERSTATE_STENCILPASS               55
    #define D3DRENDERSTATE_STENCILFUNC               56
    #define D3DRENDERSTATE_STENCILREF                57
    #define D3DRENDERSTATE_STENCILMASK               58
    #define D3DRENDERSTATE_STENCILWRITEMASK          59
    #define D3DRENDERSTATE_TEXTUREFACTOR             60
    #define D3DRENDERSTATE_WRAP0                     128
    #define D3DRENDERSTATE_WRAP1                     129
    #define D3DRENDERSTATE_WRAP2                     130
    #define D3DRENDERSTATE_WRAP3                     131
    #define D3DRENDERSTATE_WRAP4                     132
    #define D3DRENDERSTATE_WRAP5                     133
    #define D3DRENDERSTATE_WRAP6                     134
    #define D3DRENDERSTATE_WRAP7                     135
    #define D3DRENDERSTATE_CLIPPING                  136
    #define D3DRENDERSTATE_LIGHTING                  137
    #define D3DRENDERSTATE_EXTENTS                   138
    #define D3DRENDERSTATE_AMBIENT                   139
    #define D3DRENDERSTATE_FOGVERTEXMODE             140
    #define D3DRENDERSTATE_COLORVERTEX               141
    #define D3DRENDERSTATE_LOCALVIEWER               142
    #define D3DRENDERSTATE_NORMALIZENORMALS          143
    #define D3DRENDERSTATE_COLORKEYBLENDENABLE       144
    #define D3DRENDERSTATE_DIFFUSEMATERIALSOURCE     145
    #define D3DRENDERSTATE_SPECULARMATERIALSOURCE    146
    #define D3DRENDERSTATE_AMBIENTMATERIALSOURCE     147
    #define D3DRENDERSTATE_EMISSIVEMATERIALSOURCE    148
    #define D3DRENDERSTATE_VERTEXBLEND               151
    #define D3DRENDERSTATE_CLIPPLANEENABLE           152
    #define D3DRENDERSTATE_TEXTUREHANDLE             1
    #define D3DRENDERSTATE_TEXTUREADDRESS            3
    #define D3DRENDERSTATE_WRAPU                     5
    #define D3DRENDERSTATE_WRAPV                     6
    #define D3DRENDERSTATE_MONOENABLE                11
    #define D3DRENDERSTATE_ROP2                      12
    #define D3DRENDERSTATE_PLANEMASK                 13
    #define D3DRENDERSTATE_TEXTUREMAG                17
    #define D3DRENDERSTATE_TEXTUREMIN                18
    #define D3DRENDERSTATE_TEXTUREMAPBLEND           21
    #define D3DRENDERSTATE_SUBPIXEL                  31
    #define D3DRENDERSTATE_SUBPIXELX                 32
    #define D3DRENDERSTATE_STIPPLEENABLE             39
    #define D3DRENDERSTATE_BORDERCOLOR               43
    #define D3DRENDERSTATE_TEXTUREADDRESSU           44
    #define D3DRENDERSTATE_TEXTUREADDRESSV           45
    #define D3DRENDERSTATE_MIPMAPLODBIAS             46
    #define D3DRENDERSTATE_ANISOTROPY                49
    #define D3DRENDERSTATE_FLUSHBATCH                50
    #define D3DRENDERSTATE_STIPPLEPATTERN00          64
    #define D3DRENDERSTATE_STIPPLEPATTERN01          65
    #define D3DRENDERSTATE_STIPPLEPATTERN02          66
    #define D3DRENDERSTATE_STIPPLEPATTERN03          67
    #define D3DRENDERSTATE_STIPPLEPATTERN04          68
    #define D3DRENDERSTATE_STIPPLEPATTERN05          69
    #define D3DRENDERSTATE_STIPPLEPATTERN06          70
    #define D3DRENDERSTATE_STIPPLEPATTERN07          71
    #define D3DRENDERSTATE_STIPPLEPATTERN08          72
    #define D3DRENDERSTATE_STIPPLEPATTERN09          73
    #define D3DRENDERSTATE_STIPPLEPATTERN10          74
    #define D3DRENDERSTATE_STIPPLEPATTERN11          75
    #define D3DRENDERSTATE_STIPPLEPATTERN12          76
    #define D3DRENDERSTATE_STIPPLEPATTERN13          77
    #define D3DRENDERSTATE_STIPPLEPATTERN14          78
    #define D3DRENDERSTATE_STIPPLEPATTERN15          79
    #define D3DRENDERSTATE_STIPPLEPATTERN16          80
    #define D3DRENDERSTATE_STIPPLEPATTERN17          81
    #define D3DRENDERSTATE_STIPPLEPATTERN18          82
    #define D3DRENDERSTATE_STIPPLEPATTERN19          83
    #define D3DRENDERSTATE_STIPPLEPATTERN20          84
    #define D3DRENDERSTATE_STIPPLEPATTERN21          85
    #define D3DRENDERSTATE_STIPPLEPATTERN22          86
    #define D3DRENDERSTATE_STIPPLEPATTERN23          87
    #define D3DRENDERSTATE_STIPPLEPATTERN24          88
    #define D3DRENDERSTATE_STIPPLEPATTERN25          89
    #define D3DRENDERSTATE_STIPPLEPATTERN26          90
    #define D3DRENDERSTATE_STIPPLEPATTERN27          91
    #define D3DRENDERSTATE_STIPPLEPATTERN28          92
    #define D3DRENDERSTATE_STIPPLEPATTERN29          93
    #define D3DRENDERSTATE_STIPPLEPATTERN30          94
    #define D3DRENDERSTATE_STIPPLEPATTERN31          95
    #define D3DRENDERSTATE_FOGTABLESTART             36
    #define D3DRENDERSTATE_FOGTABLEEND               37
    #define D3DRENDERSTATE_FOGTABLEDENSITY           38
    #define D3DRENDERSTATE_FORCE_DWORD               0x7fffffff
};



//
// Structs
//

typedef struct _D3DLINEPATTERN
{
    WORD    wRepeatFactor;
    WORD    wLinePattern;
} D3DLINEPATTERN;

typedef struct _D3DPICKRECORD
{
    BYTE     bOpcode;
    BYTE     bPad;
    DWORD    dwOffset;
    D3DVALUE dvZ;
} D3DPICKRECORD, *LPD3DPICKRECORD;

typedef struct _D3DMATERIAL7
{
    D3DCOLORVALUE   dcvDiffuse;
    D3DCOLORVALUE   dcvAmbient;
    D3DCOLORVALUE   dcvSpecular;
    D3DCOLORVALUE   dcvEmissive;
    D3DVALUE        dvPower;
} D3DMATERIAL7, *LPD3DMATERIAL7;

typedef struct _D3DLINE
{
    WORD    wV1;
    WORD    wV2;
} D3DLINE, *LPD3DLINE;

typedef struct _D3DPOINT
{
    WORD    wCount;     /* number of points     */
    WORD    wFirst;     /* index to first vertex    */
} D3DPOINT, *LPD3DPOINT;

typedef struct _D3DCLIPSTATUS
{
    DWORD dwFlags; /* Do we set 2d extents, 3D extents or status */
    DWORD dwStatus; /* Clip status */
    float minx;
    float maxx; /* X extents */
    float miny;
    float maxy; /* Y extents */
    float minz;
    float maxz; /* Z extents */
} D3DCLIPSTATUS, *LPD3DCLIPSTATUS;

typedef struct _D3DBRANCH
{
    DWORD   dwMask;     /* Bitmask against D3D status */
    DWORD   dwValue;
    BOOL    bNegate;        /* TRUE to negate comparison */
    DWORD   dwOffset;   /* How far to branch forward (0 for exit)*/
} D3DBRANCH, *LPD3DBRANCH;

typedef struct _D3DDP_PTRSTRIDE
{
    LPVOID lpvData;
    DWORD  dwStride;
} D3DDP_PTRSTRIDE;

typedef struct _D3DDRAWPRIMITIVESTRIDEDDATA
{
    D3DDP_PTRSTRIDE position;
    D3DDP_PTRSTRIDE normal;
    D3DDP_PTRSTRIDE diffuse;
    D3DDP_PTRSTRIDE specular;
    D3DDP_PTRSTRIDE textureCoords[8];
} D3DDRAWPRIMITIVESTRIDEDDATA, *LPD3DDRAWPRIMITIVESTRIDEDDATA;

typedef struct _D3DSPAN
{
    WORD    wCount; /* Number of spans */
    WORD    wFirst; /* Index to first vertex */
} D3DSPAN, *LPD3DSPAN;

typedef struct _D3DLIGHTINGELEMENT
{
    D3DVECTOR dvPosition;           /* Lightable point in model space */
    D3DVECTOR dvNormal;             /* Normalised unit vector */
} D3DLIGHTINGELEMENT, *LPD3DLIGHTINGELEMENT;

typedef struct _D3DTRIANGLE
{
    WORD    wV1;
    WORD    wV2;
    WORD    wV3;
    WORD        wFlags;       /* Edge (and other) flags */
} D3DTRIANGLE, *LPD3DTRIANGLE;

typedef struct _D3DVERTEX
{
    D3DVALUE     dvX;
    D3DVALUE     dvY;
    D3DVALUE     dvZ;
    D3DVALUE     dvNX;
    D3DVALUE     dvNY;
    D3DVALUE     dvNZ;
    D3DVALUE     dvTU;
    D3DVALUE     dvTV;
} D3DVERTEX, *LPD3DVERTEX;

typedef struct _D3DSTATE
{
    DWORD   dwStateType;
    D3DVALUE        dvArg[1];
} D3DSTATE, *LPD3DSTATE;

typedef struct _D3DSTATUS
{
    DWORD       dwFlags;    /* Do we set extents or status */
    DWORD   dwStatus;   /* D3D status */
    D3DRECT drExtent;
} D3DSTATUS, *LPD3DSTATUS;

typedef struct _D3DPROCESSVERTICES
{
    DWORD        dwFlags;    /* Do we transform or light or just copy? */
    WORD         wStart;     /* Index to first vertex in source */
    WORD         wDest;      /* Index to first vertex in local buffer */
    DWORD        dwCount;    /* Number of vertices to be processed */
    DWORD    dwReserved; /* Must be zero */
} D3DPROCESSVERTICES, *LPD3DPROCESSVERTICES;

typedef struct _D3DHVERTEX
{
    DWORD           dwFlags;        /* Homogeneous clipping flags */
    D3DVALUE    dvHX;
    D3DVALUE    dvHY;
    D3DVALUE    dvHZ;
} D3DHVERTEX, *LPD3DHVERTEX;

typedef struct _D3DLVERTEX
{
    D3DVALUE     dvX;
    D3DVALUE     dvY;
    D3DVALUE     dvZ;
    DWORD        dwReserved;
    D3DCOLOR     dcColor;
    D3DCOLOR     dcSpecular;
    D3DVALUE     dvTU;
    D3DVALUE     dvTV;
} D3DLVERTEX, *LPD3DLVERTEX;

typedef struct _D3DLIGHT2
{
    DWORD           dwSize;
    D3DLIGHTTYPE    dltType;        /* Type of light source */
    D3DCOLORVALUE   dcvColor;       /* Color of light */
    D3DVECTOR       dvPosition;     /* Position in world space */
    D3DVECTOR       dvDirection;    /* Direction in world space */
    D3DVALUE        dvRange;        /* Cutoff range */
    D3DVALUE        dvFalloff;      /* Falloff */
    D3DVALUE        dvAttenuation0; /* Constant attenuation */
    D3DVALUE        dvAttenuation1; /* Linear attenuation */
    D3DVALUE        dvAttenuation2; /* Quadratic attenuation */
    D3DVALUE        dvTheta;        /* Inner angle of spotlight cone */
    D3DVALUE        dvPhi;          /* Outer angle of spotlight cone */
    DWORD           dwFlags;
} D3DLIGHT2, *LPD3DLIGHT2;

typedef struct _D3DEXECUTEDATA
{
    DWORD       dwSize;
    DWORD       dwVertexOffset;
    DWORD       dwVertexCount;
    DWORD       dwInstructionOffset;
    DWORD       dwInstructionLength;
    DWORD       dwHVertexOffset;
    D3DSTATUS   dsStatus;   /* Status after execute */
} D3DEXECUTEDATA, *LPD3DEXECUTEDATA;

typedef struct _D3DSTATS
{
    DWORD        dwSize;
    DWORD        dwTrianglesDrawn;
    DWORD        dwLinesDrawn;
    DWORD        dwPointsDrawn;
    DWORD        dwSpansDrawn;
    DWORD        dwVerticesProcessed;
} D3DSTATS, *LPD3DSTATS;

typedef struct _D3DMATERIAL
{
    DWORD           dwSize;
    D3DCOLORVALUE   dcvDiffuse;
    D3DCOLORVALUE   dcvAmbient;
    D3DCOLORVALUE   dcvSpecular;
    D3DCOLORVALUE   dcvEmissive;
    D3DVALUE        dvPower;
    D3DTEXTUREHANDLE    hTexture;       /* Handle to texture map */
    DWORD           dwRampSize;
} D3DMATERIAL, *LPD3DMATERIAL;

typedef struct _D3DLIGHT7
{
    D3DLIGHTTYPE    dltType;            /* Type of light source */
    D3DCOLORVALUE   dcvDiffuse;         /* Diffuse color of light */
    D3DCOLORVALUE   dcvSpecular;        /* Specular color of light */
    D3DCOLORVALUE   dcvAmbient;         /* Ambient color of light */
    D3DVECTOR       dvPosition;         /* Position in world space */
    D3DVECTOR       dvDirection;        /* Direction in world space */
    D3DVALUE        dvRange;            /* Cutoff range */
    D3DVALUE        dvFalloff;          /* Falloff */
    D3DVALUE        dvAttenuation0;     /* Constant attenuation */
    D3DVALUE        dvAttenuation1;     /* Linear attenuation */
    D3DVALUE        dvAttenuation2;     /* Quadratic attenuation */
    D3DVALUE        dvTheta;            /* Inner angle of spotlight cone */
    D3DVALUE        dvPhi;              /* Outer angle of spotlight cone */
} D3DLIGHT7, *LPD3DLIGHT7;

typedef struct _D3DTEXTURELOAD
{
    D3DTEXTUREHANDLE hDestTexture;
    D3DTEXTUREHANDLE hSrcTexture;
} D3DTEXTURELOAD, *LPD3DTEXTURELOAD;

typedef struct _D3DVIEWPORT2
{
    DWORD       dwSize;
    DWORD       dwX;
    DWORD       dwY;        /* Viewport Top left */
    DWORD       dwWidth;
    DWORD       dwHeight;   /* Viewport Dimensions */
    D3DVALUE    dvClipX;        /* Top left of clip volume */
    D3DVALUE    dvClipY;
    D3DVALUE    dvClipWidth;    /* Clip Volume Dimensions */
    D3DVALUE    dvClipHeight;
    D3DVALUE    dvMinZ;         /* Min/max of clip Volume */
    D3DVALUE    dvMaxZ;
} D3DVIEWPORT2, *LPD3DVIEWPORT2;

typedef struct _D3DMATRIX
{
    D3DVALUE        _11;
    D3DVALUE        _12;
    D3DVALUE        _13;
    D3DVALUE        _14;
    D3DVALUE        _21;
    D3DVALUE        _22;
    D3DVALUE        _23;
    D3DVALUE        _24;
    D3DVALUE        _31;
    D3DVALUE        _32;
    D3DVALUE        _33;
    D3DVALUE        _34;
    D3DVALUE        _41;
    D3DVALUE        _42;
    D3DVALUE        _43;
    D3DVALUE        _44;
} D3DMATRIX, *LPD3DMATRIX;

typedef struct _D3DMATRIXLOAD
{
    D3DMATRIXHANDLE hDestMatrix;   /* Destination matrix */
    D3DMATRIXHANDLE hSrcMatrix;   /* Source matrix */
} D3DMATRIXLOAD, *LPD3DMATRIXLOAD;

typedef struct _D3DMATRIXMULTIPLY
{
    D3DMATRIXHANDLE hDestMatrix;   /* Destination matrix */
    D3DMATRIXHANDLE hSrcMatrix1;  /* First source matrix */
    D3DMATRIXHANDLE hSrcMatrix2;  /* Second source matrix */
} D3DMATRIXMULTIPLY, *LPD3DMATRIXMULTIPLY;

typedef struct _D3DTRANSFORMDATA
{
    DWORD           dwSize;
    LPVOID      lpIn;           /* Input vertices */
    DWORD           dwInSize;       /* Stride of input vertices */
    LPVOID      lpOut;          /* Output vertices */
    DWORD           dwOutSize;      /* Stride of output vertices */
    LPD3DHVERTEX    lpHOut;         /* Output homogeneous vertices */
    DWORD           dwClip;         /* Clipping hint */
    DWORD           dwClipIntersection;
    DWORD           dwClipUnion;    /* Union of all clip flags */
    D3DRECT         drExtent;       /* Extent of transformed vertices */
} D3DTRANSFORMDATA, *LPD3DTRANSFORMDATA;

typedef struct _D3DVERTEXBUFFERDESC
{
    DWORD dwSize;
    DWORD dwCaps;
    DWORD dwFVF;
    DWORD dwNumVertices;
} D3DVERTEXBUFFERDESC, *LPD3DVERTEXBUFFERDESC;

typedef struct _D3DVIEWPORT7
{
    DWORD       dwX;
    DWORD       dwY;            /* Viewport Top left */
    DWORD       dwWidth;
    DWORD       dwHeight;       /* Viewport Dimensions */
    D3DVALUE    dvMinZ;         /* Min/max of clip Volume */
    D3DVALUE    dvMaxZ;
} D3DVIEWPORT7, *LPD3DVIEWPORT7;

typedef struct _D3DLIGHT
{
    DWORD           dwSize;
    D3DLIGHTTYPE    dltType;            /* Type of light source */
    D3DCOLORVALUE   dcvColor;           /* Color of light */
    D3DVECTOR       dvPosition;         /* Position in world space */
    D3DVECTOR       dvDirection;        /* Direction in world space */
    D3DVALUE        dvRange;            /* Cutoff range */
    D3DVALUE        dvFalloff;          /* Falloff */
    D3DVALUE        dvAttenuation0;     /* Constant attenuation */
    D3DVALUE        dvAttenuation1;     /* Linear attenuation */
    D3DVALUE        dvAttenuation2;     /* Quadratic attenuation */
    D3DVALUE        dvTheta;            /* Inner angle of spotlight cone */
    D3DVALUE        dvPhi;              /* Outer angle of spotlight cone */
} D3DLIGHT, *LPD3DLIGHT;

typedef struct _D3DTLVERTEX
{
    D3DVALUE    dvSX;
    D3DVALUE    dvSY;
    D3DVALUE    dvSZ;
    D3DVALUE    dvRHW;
    D3DCOLOR    dcColor;
    D3DCOLOR    dcSpecular;
    D3DVALUE    dvTU;
    D3DVALUE    dvTV;
} D3DTLVERTEX, *LPD3DTLVERTEX;

typedef struct _D3DVIEWPORT
{
    DWORD       dwSize;
    DWORD       dwX;
    DWORD       dwY;        /* Top left */
    DWORD       dwWidth;
    DWORD       dwHeight;   /* Dimensions */
    D3DVALUE    dvScaleX;   /* Scale homogeneous to screen */
    D3DVALUE    dvScaleY;   /* Scale homogeneous to screen */
    D3DVALUE    dvMaxX;     /* Min/max homogeneous x coord */
    D3DVALUE    dvMaxY;     /* Min/max homogeneous y coord */
    D3DVALUE    dvMinZ;
    D3DVALUE    dvMaxZ;     /* Min/max homogeneous z coord */
} D3DVIEWPORT, *LPD3DVIEWPORT;

typedef struct _D3DINSTRUCTION
{
    BYTE bOpcode;   /* Instruction opcode */
    BYTE bSize;     /* Size of each instruction data unit */
    WORD wCount;    /* Count of instruction data units to follow */
} D3DINSTRUCTION, *LPD3DINSTRUCTION;

typedef struct _D3DLIGHTDATA
{
    DWORD                dwSize;
    LPD3DLIGHTINGELEMENT lpIn;      /* Input positions and normals */
    DWORD                dwInSize;  /* Stride of input elements */
    LPD3DTLVERTEX        lpOut;     /* Output colors */
    DWORD                dwOutSize; /* Stride of output colors */
} D3DLIGHTDATA, *LPD3DLIGHTDATA;



//
// Interfaces
//
