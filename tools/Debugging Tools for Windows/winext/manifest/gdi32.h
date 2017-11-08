module GDI32.DLL:
category GDI:


typedef HANDLE HGDIOBJ;
typedef HANDLE HFONT;
typedef HANDLE HPALETTE;
typedef HANDLE HBITMAP;
typedef HANDLE HBRUSH;
typedef HANDLE HPEN;
typedef HANDLE HENHMETAFILE;
typedef HANDLE HCOLORSPACE;
typedef HANDLE HGLRC;

typedef DWORD COLORREF;
typedef DWORD *LPCOLORREF;

value int _ODD_FAILURE
{
#define ODD_FAILURE 0x80000000 [fail]
};

/* Binary raster ops */
value DWORD _BinaryDrawMode
{
#define R2_BLACK            1   /*  0       */
#define R2_NOTMERGEPEN      2   /* DPon     */
#define R2_MASKNOTPEN       3   /* DPna     */
#define R2_NOTCOPYPEN       4   /* PN       */
#define R2_MASKPENNOT       5   /* PDna     */
#define R2_NOT              6   /* Dn       */
#define R2_XORPEN           7   /* DPx      */
#define R2_NOTMASKPEN       8   /* DPan     */
#define R2_MASKPEN          9   /* DPa      */
#define R2_NOTXORPEN        10  /* DPxn     */
#define R2_NOP              11  /* D        */
#define R2_MERGENOTPEN      12  /* DPno     */
#define R2_COPYPEN          13  /* P        */
#define R2_MERGEPENNOT      14  /* PDno     */
#define R2_MERGEPEN         15  /* DPo      */
#define R2_WHITE            16  /*  1       */
#define R2_LAST             16
};

value DWORD _TernaryDrawMode
{
/* Ternary raster operations */
#define SRCCOPY             0x00CC0020 /* dest = source                   */
#define SRCPAINT            0x00EE0086 /* dest = source OR dest           */
#define SRCAND              0x008800C6 /* dest = source AND dest          */
#define SRCINVERT           0x00660046 /* dest = source XOR dest          */
#define SRCERASE            0x00440328 /* dest = source AND (NOT dest )   */
#define NOTSRCCOPY          0x00330008 /* dest = (NOT source)             */
#define NOTSRCERASE         0x001100A6 /* dest = (NOT src) AND (NOT dest) */
#define MERGECOPY           0x00C000CA /* dest = (source AND pattern)     */
#define MERGEPAINT          0x00BB0226 /* dest = (NOT source) OR dest     */
#define PATCOPY             0x00F00021 /* dest = pattern                  */
#define PATPAINT            0x00FB0A09 /* dest = DPSnoo                   */
#define PATINVERT           0x005A0049 /* dest = pattern XOR dest         */
#define DSTINVERT           0x00550009 /* dest = (NOT dest)               */
#define BLACKNESS           0x00000042 /* dest = BLACK                    */
#define WHITENESS           0x00FF0062 /* dest = WHITE                    */
#define NOMIRRORBITMAP      0x80000000 /* Do not Mirror the bitmap in this call */
#define CAPTUREBLT          0x40000000 /* Include layered windows */
};



value DWORD _GDI_ERROR
{
#define GDI_ERROR 0xFFFFFFFFL [fail]
};
value DWORD _HGDI_ERROR
{
#define HGDI_ERROR 0xFFFFFFFFL [fail]
};

value DWORD _RegionFlags
{
/* Region Flags */
#define ERROR               0 [fail]
#define NULLREGION          1
#define SIMPLEREGION        2
#define COMPLEXREGION       3
};

value int _CombineRgn
{
/* CombineRgn() Styles */
#define RGN_AND             1
#define RGN_OR              2
#define RGN_XOR             3
#define RGN_DIFF            4
#define RGN_COPY            5
};

value DWORD _COMBINRGN_STYLE
{
/* CombineRgn() Styles */
/* StretchBlt() Modes */
#define BLACKONWHITE                 1
#define WHITEONBLACK                 2
#define COLORONCOLOR                 3
#define HALFTONE                     4
};

value DWORD _PolyFill
{
/* PolyFill() Modes */
#define ALTERNATE                    1
#define WINDING                      2
#define POLYFILL_LAST                2
};

mask DWORD _LAYOUT
{
#define LAYOUT_RTL                         0x00000001 // Right to left
#define LAYOUT_BTT                         0x00000002 // Bottom to top
#define LAYOUT_VBH                         0x00000004 // Vertical before horizontal
//#define LAYOUT_ORIENTATIONMASK             (LAYOUT_RTL | LAYOUT_BTT | LAYOUT_VBH)
#define LAYOUT_BITMAPORIENTATIONPRESERVED  0x00000008
};

mask DWORD _TextAlignmentOptions
{
/* Text Alignment Options */
//#define TA_NOUPDATECP                0
//#define TA_UPDATECP                  1
#define TA_LEFT                      0
#define TA_RIGHT                     2
#define TA_CENTER                    6
#define TA_TOP                       0
#define TA_BOTTOM                    8
#define TA_BASELINE                  24
#define TA_RTLREADING                256
};

mask DWORD _ETO
{
#define ETO_OPAQUE                   0x0002
#define ETO_CLIPPED                  0x0004
#define ETO_GLYPH_INDEX              0x0010
#define ETO_RTLREADING               0x0080
#define ETO_NUMERICSLOCAL            0x0400
#define ETO_NUMERICSLATIN            0x0800
#define ETO_IGNORELANGUAGE           0x1000
#define ETO_PDY                      0x2000
};

mask DWORD _AspectFiltering
{
#define ASPECT_FILTERING             0x0001
};


mask DWORD _DCB
{
/* Bounds Accumulation APIs */
#define DCB_ERROR       0           //[fail]
#define DCB_RESET       0x0001
#define DCB_ACCUMULATE  0x0002
#define DCB_ENABLE      0x0004
#define DCB_DISABLE     0x0008
};

value DWORD _Meta
{
/* Metafile Functions */
#define META_SETBKCOLOR              0x0201
#define META_SETBKMODE               0x0102
#define META_SETMAPMODE              0x0103
#define META_SETROP2                 0x0104
#define META_SETRELABS               0x0105
#define META_SETPOLYFILLMODE         0x0106
#define META_SETSTRETCHBLTMODE       0x0107
#define META_SETTEXTCHAREXTRA        0x0108
#define META_SETTEXTCOLOR            0x0209
#define META_SETTEXTJUSTIFICATION    0x020A
#define META_SETWINDOWORG            0x020B
#define META_SETWINDOWEXT            0x020C
#define META_SETVIEWPORTORG          0x020D
#define META_SETVIEWPORTEXT          0x020E
#define META_OFFSETWINDOWORG         0x020F
#define META_SCALEWINDOWEXT          0x0410
#define META_OFFSETVIEWPORTORG       0x0211
#define META_SCALEVIEWPORTEXT        0x0412
#define META_LINETO                  0x0213
#define META_MOVETO                  0x0214
#define META_EXCLUDECLIPRECT         0x0415
#define META_INTERSECTCLIPRECT       0x0416
#define META_ARC                     0x0817
#define META_ELLIPSE                 0x0418
#define META_FLOODFILL               0x0419
#define META_PIE                     0x081A
#define META_RECTANGLE               0x041B
#define META_ROUNDRECT               0x061C
#define META_PATBLT                  0x061D
#define META_SAVEDC                  0x001E
#define META_SETPIXEL                0x041F
#define META_OFFSETCLIPRGN           0x0220
#define META_TEXTOUT                 0x0521
#define META_BITBLT                  0x0922
#define META_STRETCHBLT              0x0B23
#define META_POLYGON                 0x0324
#define META_POLYLINE                0x0325
#define META_ESCAPE                  0x0626
#define META_RESTOREDC               0x0127
#define META_FILLREGION              0x0228
#define META_FRAMEREGION             0x0429
#define META_INVERTREGION            0x012A
#define META_PAINTREGION             0x012B
#define META_SELECTCLIPREGION        0x012C
#define META_SELECTOBJECT            0x012D
#define META_SETTEXTALIGN            0x012E
#define META_CHORD                   0x0830
#define META_SETMAPPERFLAGS          0x0231
#define META_EXTTEXTOUT              0x0a32
#define META_SETDIBTODEV             0x0d33
#define META_SELECTPALETTE           0x0234
#define META_REALIZEPALETTE          0x0035
#define META_ANIMATEPALETTE          0x0436
#define META_SETPALENTRIES           0x0037
#define META_POLYPOLYGON             0x0538
#define META_RESIZEPALETTE           0x0139
#define META_DIBBITBLT               0x0940
#define META_DIBSTRETCHBLT           0x0b41
#define META_DIBCREATEPATTERNBRUSH   0x0142
#define META_STRETCHDIB              0x0f43
#define META_EXTFLOODFILL            0x0548
#define META_SETLAYOUT               0x0149
#define META_DELETEOBJECT            0x01f0
#define META_CREATEPALETTE           0x00f7
#define META_CREATEPATTERNBRUSH      0x01F9
#define META_CREATEPENINDIRECT       0x02FA
#define META_CREATEFONTINDIRECT      0x02FB
#define META_CREATEBRUSHINDIRECT     0x02FC
#define META_CREATEREGION            0x06FF
};

//#define ELF_VERSION         0
//#define ELF_CULTURE_LATIN   0

mask DWORD _EnumFontsMask
{
/* EnumFonts Masks */
#define RASTER_FONTTYPE     0x0001
#define DEVICE_FONTTYPE     0x002
#define TRUETYPE_FONTTYPE   0x004
};


/* palette entry flags */
mask BYTE _PaletteEntryFlag
{
#define PC_RESERVED     0x01    /* palette index used for animation */
#define PC_EXPLICIT     0x02    /* palette index is explicit to device */
#define PC_NOCOLLAPSE   0x04    /* do not match color to system palette */
};

value DWORD _BK_Mode
{
/* Background Modes */
#define TRANSPARENT         1
#define OPAQUE              2
};

value DWORD _GM
{
/* Graphics Modes */
#define GM_COMPATIBLE       1
#define GM_ADVANCED         2
};

mask DWORD _PT
{
/* PolyDraw and GetPath point types */
#define PT_CLOSEFIGURE      0x01
#define PT_LINETO           0x02
#define PT_BEZIERTO         0x04
#define PT_MOVETO           0x06
};

value DWORD _MM
{
/* Mapping Modes */
#define MM_TEXT             1
#define MM_LOMETRIC         2
#define MM_HIMETRIC         3
#define MM_LOENGLISH        4
#define MM_HIENGLISH        5
#define MM_TWIPS            6
#define MM_ISOTROPIC        7
#define MM_ANISOTROPIC      8

};

value DWORD _Coordinate_Mode
{
/* Coordinate Modes */
#define ABSOLUTE            1
#define RELATIVE            2
};

value DWORD _StockObject
{

/* Stock Logical Objects */
#define WHITE_BRUSH         0
#define LTGRAY_BRUSH        1
#define GRAY_BRUSH          2
#define DKGRAY_BRUSH        3
#define BLACK_BRUSH         4
#define NULL_BRUSH          5
//#define HOLLOW_BRUSH        NULL_BRUSH
#define WHITE_PEN           6
#define BLACK_PEN           7
#define NULL_PEN            8
#define OEM_FIXED_FONT      10
#define ANSI_FIXED_FONT     11
#define ANSI_VAR_FONT       12
#define SYSTEM_FONT         13
#define DEVICE_DEFAULT_FONT 14
#define DEFAULT_PALETTE     15
#define SYSTEM_FIXED_FONT   16

#define DEFAULT_GUI_FONT    17

#define DC_BRUSH            18
#define DC_PEN              19

};

value DWORD COLORREF_RETURN
{
#define CLR_INVALID     0xFFFFFFFF [fail]
};

value DWORD _BrushStyles
{
/* Brush Styles */
#define BS_SOLID            0
#define BS_NULL             1
#define BS_HATCHED          2
#define BS_PATTERN          3
#define BS_INDEXED          4
#define BS_DIBPATTERN       5
#define BS_DIBPATTERNPT     6
#define BS_PATTERN8X8       7
#define BS_DIBPATTERN8X8    8
#define BS_MONOPATTERN      9
};

value ULONG_PTR _HatchStyle
{
/* Hatch Styles */
#define HS_HORIZONTAL       0       /* ----- */
#define HS_VERTICAL         1       /* ||||| */
#define HS_FDIAGONAL        2       /* \\\\\ */
#define HS_BDIAGONAL        3       /* ///// */
#define HS_CROSS            4       /* +++++ */
#define HS_DIAGCROSS        5       /* xxxxx */
};

mask int _PS
{
/* Pen Styles */
#define PS_SOLID            0
#define PS_DASH             1       /* -------  */
#define PS_DOT              2       /* .......  */
//#define PS_DASHDOT          3       /* _._._._  */
#define PS_DASHDOTDOT       4       /* _.._.._  */
//#define PS_NULL             5
//#define PS_INSIDEFRAME      6
//#define PS_USERSTYLE        7
#define PS_ALTERNATE        8

#define PS_ENDCAP_ROUND     0x00000000
#define PS_ENDCAP_SQUARE    0x00000100
#define PS_ENDCAP_FLAT      0x00000200
#define PS_ENDCAP_MASK      0x00000F00

#define PS_JOIN_ROUND       0x00000000
#define PS_JOIN_BEVEL       0x00001000
#define PS_JOIN_MITER       0x00002000
#define PS_JOIN_MASK        0x0000F000

#define PS_COSMETIC         0x00000000
#define PS_GEOMETRIC        0x00010000
#define PS_TYPE_MASK        0x000F0000
};

value DWORD _AD
{
#define AD_COUNTERCLOCKWISE 1
#define AD_CLOCKWISE        2
};

value DWORD _DeviceParameters
{
/* Device Parameters for GetDeviceCaps() */
#define DRIVERVERSION 0     /* Device driver version                    */
#define TECHNOLOGY    2     /* Device classification                    */
#define HORZSIZE      4     /* Horizontal size in millimeters           */
#define VERTSIZE      6     /* Vertical size in millimeters             */
#define HORZRES       8     /* Horizontal width in pixels               */
#define VERTRES       10    /* Vertical height in pixels                */
#define BITSPIXEL     12    /* Number of bits per pixel                 */
#define PLANES        14    /* Number of planes                         */
#define NUMBRUSHES    16    /* Number of brushes the device has         */
#define NUMPENS       18    /* Number of pens the device has            */
#define NUMMARKERS    20    /* Number of markers the device has         */
#define NUMFONTS      22    /* Number of fonts the device has           */
#define NUMCOLORS     24    /* Number of colors the device supports     */
#define PDEVICESIZE   26    /* Size required for device descriptor      */
#define CURVECAPS     28    /* Curve capabilities                       */
#define LINECAPS      30    /* Line capabilities                        */
#define POLYGONALCAPS 32    /* Polygonal capabilities                   */
#define TEXTCAPS      34    /* Text capabilities                        */
#define CLIPCAPS      36    /* Clipping capabilities                    */
#define RASTERCAPS    38    /* Bitblt capabilities                      */
#define ASPECTX       40    /* Length of the X leg                      */
#define ASPECTY       42    /* Length of the Y leg                      */
#define ASPECTXY      44    /* Length of the hypotenuse                 */


#define LOGPIXELSX    88    /* Logical pixels/inch in X                 */
#define LOGPIXELSY    90    /* Logical pixels/inch in Y                 */

#define SIZEPALETTE  104    /* Number of entries in physical palette    */
#define NUMRESERVED  106    /* Number of reserved entries in palette    */
#define COLORRES     108    /* Actual color resolution                  */


// Printing related DeviceCaps. These replace the appropriate Escapes

#define PHYSICALWIDTH   110 /* Physical Width in device units           */
#define PHYSICALHEIGHT  111 /* Physical Height in device units          */
#define PHYSICALOFFSETX 112 /* Physical Printable Area x margin         */
#define PHYSICALOFFSETY 113 /* Physical Printable Area y margin         */
#define SCALINGFACTORX  114 /* Scaling factor x                         */
#define SCALINGFACTORY  115 /* Scaling factor y                         */

// Display driver specific

#define VREFRESH        116  /* Current vertical refresh rate of the    */
                             /* display device (for displays only) in Hz*/
#define DESKTOPVERTRES  117  /* Horizontal width of entire desktop in   */
                             /* pixels                                  */
#define DESKTOPHORZRES  118  /* Vertical height of entire desktop in    */
                             /* pixels                                  */
#define BLTALIGNMENT    119  /* Preferred blt alignment                 */
#define SHADEBLENDCAPS  120  /* Shading and blending caps               */
#define COLORMGMTCAPS   121  /* Color Management caps                   */
};

mask DWORD _DeviceCapabilityDT
{

/* Device Capability Masks: */

/* Device Technologies */
#define DT_PLOTTER          0   /* Vector plotter                   */
#define DT_RASDISPLAY       1   /* Raster display                   */
#define DT_RASPRINTER       2   /* Raster printer                   */
#define DT_RASCAMERA        3   /* Raster camera                    */
#define DT_CHARSTREAM       4   /* Character-stream, PLP            */
#define DT_METAFILE         5   /* Metafile, VDM                    */
#define DT_DISPFILE         6   /* Display-file                     */
};

mask DWORD _DeviceCapabilityCC
{
/* Curve Capabilities */
#define CC_NONE             0   /* Curves not supported             */
#define CC_CIRCLES          1   /* Can do circles                   */
#define CC_PIE              2   /* Can do pie wedges                */
#define CC_CHORD            4   /* Can do chord arcs                */
#define CC_ELLIPSES         8   /* Can do ellipese                  */
#define CC_WIDE             16  /* Can do wide lines                */
#define CC_STYLED           32  /* Can do styled lines              */
#define CC_WIDESTYLED       64  /* Can do wide styled lines         */
#define CC_INTERIORS        128 /* Can do interiors                 */
#define CC_ROUNDRECT        256 /*                                  */
};

mask DWORD _DeviceCapabilityLC
{
/* Line Capabilities */
#define LC_NONE             0   /* Lines not supported              */
#define LC_POLYLINE         2   /* Can do polylines                 */
#define LC_MARKER           4   /* Can do markers                   */
#define LC_POLYMARKER       8   /* Can do polymarkers               */
#define LC_WIDE             16  /* Can do wide lines                */
#define LC_STYLED           32  /* Can do styled lines              */
#define LC_WIDESTYLED       64  /* Can do wide styled lines         */
#define LC_INTERIORS        128 /* Can do interiors                 */
};

mask DWORD _DeviceCapabilityPC
{
/* Polygonal Capabilities */
#define PC_NONE             0   /* Polygonals not supported         */
#define PC_POLYGON          1   /* Can do polygons                  */
#define PC_RECTANGLE        2   /* Can do rectangles                */
#define PC_WINDPOLYGON      4   /* Can do winding polygons          */
#define PC_TRAPEZOID        4   /* Can do trapezoids                */
#define PC_SCANLINE         8   /* Can do scanlines                 */
#define PC_WIDE             16  /* Can do wide borders              */
#define PC_STYLED           32  /* Can do styled borders            */
#define PC_WIDESTYLED       64  /* Can do wide styled borders       */
#define PC_INTERIORS        128 /* Can do interiors                 */
#define PC_POLYPOLYGON      256 /* Can do polypolygons              */
#define PC_PATHS            512 /* Can do paths                     */
};

mask DWORD _DeviceCapabilityCP
{
/* Clipping Capabilities */
#define CP_NONE             0   /* No clipping of output            */
#define CP_RECTANGLE        1   /* Output clipped to rects          */
#define CP_REGION           2   /* obsolete                         */
};

mask DWORD _DeviceCapabilityTC
{
/* Text Capabilities */
#define TC_OP_CHARACTER     0x00000001  /* Can do OutputPrecision   CHARACTER      */
#define TC_OP_STROKE        0x00000002  /* Can do OutputPrecision   STROKE         */
#define TC_CP_STROKE        0x00000004  /* Can do ClipPrecision     STROKE         */
#define TC_CR_90            0x00000008  /* Can do CharRotAbility    90             */
#define TC_CR_ANY           0x00000010  /* Can do CharRotAbility    ANY            */
#define TC_SF_X_YINDEP      0x00000020  /* Can do ScaleFreedom      X_YINDEPENDENT */
#define TC_SA_DOUBLE        0x00000040  /* Can do ScaleAbility      DOUBLE         */
#define TC_SA_INTEGER       0x00000080  /* Can do ScaleAbility      INTEGER        */
#define TC_SA_CONTIN        0x00000100  /* Can do ScaleAbility      CONTINUOUS     */
#define TC_EA_DOUBLE        0x00000200  /* Can do EmboldenAbility   DOUBLE         */
#define TC_IA_ABLE          0x00000400  /* Can do ItalisizeAbility  ABLE           */
#define TC_UA_ABLE          0x00000800  /* Can do UnderlineAbility  ABLE           */
#define TC_SO_ABLE          0x00001000  /* Can do StrikeOutAbility  ABLE           */
#define TC_RA_ABLE          0x00002000  /* Can do RasterFontAble    ABLE           */
#define TC_VA_ABLE          0x00004000  /* Can do VectorFontAble    ABLE           */
#define TC_RESERVED         0x00008000
#define TC_SCROLLBLT        0x00010000  /* Don't do text scroll with blt           */

};

mask DWORD _DeviceCapabilityRC
{
/* Raster Capabilities */
#define RC_NONE             0
#define RC_BITBLT           1       /* Can do standard BLT.             */
#define RC_BANDING          2       /* Device requires banding support  */
#define RC_SCALING          4       /* Device requires scaling support  */
#define RC_BITMAP64         8       /* Device can support >64K bitmap   */
#define RC_GDI20_OUTPUT     0x0010      /* has 2.0 output calls         */
#define RC_GDI20_STATE      0x0020
#define RC_SAVEBITMAP       0x0040
#define RC_DI_BITMAP        0x0080      /* supports DIB to memory       */
#define RC_PALETTE          0x0100      /* supports a palette           */
#define RC_DIBTODEV         0x0200      /* supports DIBitsToDevice      */
#define RC_BIGFONT          0x0400      /* supports >64K fonts          */
#define RC_STRETCHBLT       0x0800      /* supports StretchBlt          */
#define RC_FLOODFILL        0x1000      /* supports FloodFill           */
#define RC_STRETCHDIB       0x2000      /* supports StretchDIBits       */
#define RC_OP_DX_OUTPUT     0x4000
#define RC_DEVBITS          0x8000

};

mask DWORD _DeviceCapabilitySB
{
/* Shading and blending caps                */
#define SB_NONE             0x00000000
#define SB_CONST_ALPHA      0x00000001
#define SB_PIXEL_ALPHA      0x00000002
#define SB_PREMULT_ALPHA    0x00000004

#define SB_GRAD_RECT        0x00000010
#define SB_GRAD_TRI         0x00000020
};

mask DWORD _ColorManagementCaps
{
/* Color Management caps */
#define CM_NONE             0x00000000
#define CM_DEVICE_ICM       0x00000001
#define CM_GAMMA_RAMP       0x00000002
#define CM_CMYK_COLOR       0x00000004
};


/* DIB color table identifiers */
value DWORD _DIB_Color
{
#define DIB_RGB_COLORS      0 /* color table in RGBs */
#define DIB_PAL_COLORS      1 /* color table in palette indices */
};

value DWORD _SYSPAL
{
/* constants for Get/SetSystemPaletteUse() */
#define SYSPAL_ERROR        0 [fail]
#define SYSPAL_STATIC       1
#define SYSPAL_NOSTATIC     2
#define SYSPAL_NOSTATIC256  3
};

value DWORD _CreateDIBitmap
{
/* constants for CreateDIBitmap */
#define CBM_INIT        0x04L   /* initialize bitmap */
};

value DWORD _FLOODFILL
{
/* ExtFloodFill style flags */
#define  FLOODFILLBORDER   0
#define  FLOODFILLSURFACE  1
};
/* current version of specification */
//#define DM_SPECVERSION 0x0401



value DWORD _PSIDENT
{
/*
 * Parameters for POSTSCRIPT_IDENTIFY escape
 */

#define PSIDENT_GDICENTRIC    0
#define PSIDENT_PSCENTRIC     1
};
value DWORD _PSINJECTMode
{

/*
 * Constants for PSINJECTDATA.Flags field
 */

#define   PSINJECT_APPEND       0
#define   PSINJECT_REPLACE      1
};

/*
 * Constants for PSINJECTDATA.InjectionPoint field
 */

/*
 * The data injected at these points coexist with the output emitted
 * by the driver for the same points.
 */

value WORD _PSINJECT
{
#define PSINJECT_BEGINSTREAM                1
#define PSINJECT_PSADOBE                    2
#define PSINJECT_PAGESATEND                 3
#define PSINJECT_PAGES                      4

#define PSINJECT_DOCNEEDEDRES               5
#define PSINJECT_DOCSUPPLIEDRES             6
#define PSINJECT_PAGEORDER                  7
#define PSINJECT_ORIENTATION                8
#define PSINJECT_BOUNDINGBOX                9
#define PSINJECT_DOCUMENTPROCESSCOLORS      10

#define PSINJECT_COMMENTS                   11
#define PSINJECT_BEGINDEFAULTS              12
#define PSINJECT_ENDDEFAULTS                13
#define PSINJECT_BEGINPROLOG                14
#define PSINJECT_ENDPROLOG                  15
#define PSINJECT_BEGINSETUP                 16
#define PSINJECT_ENDSETUP                   17
#define PSINJECT_TRAILER                    18
#define PSINJECT_EOF                        19
#define PSINJECT_ENDSTREAM                  20
#define PSINJECT_DOCUMENTPROCESSCOLORSATEND 21

#define PSINJECT_PAGENUMBER                 100
#define PSINJECT_BEGINPAGESETUP             101
#define PSINJECT_ENDPAGESETUP               102
#define PSINJECT_PAGETRAILER                103
#define PSINJECT_PLATECOLOR                 104

#define PSINJECT_SHOWPAGE                   105
#define PSINJECT_PAGEBBOX                   106
#define PSINJECT_ENDPAGECOMMENTS            107

#define PSINJECT_VMSAVE                     200
#define PSINJECT_VMRESTORE                  201

};

value DWORD _PSPROTOCOL
{
/* Value returned for FEATURESETTING_PROTOCOL */
#define PSPROTOCOL_ASCII             0
#define PSPROTOCOL_BCP               1
#define PSPROTOCOL_TBCP              2
#define PSPROTOCOL_BINARY            3
};

mask DWORD _QDI
{
/* Flag returned from QUERYDIBSUPPORT */
#define QDI_SETDIBITS                1
#define QDI_GETDIBITS                2
#define QDI_DIBTOSCREEN              4
#define QDI_STRETCHDIB               8
};

value DWORD _FEATURESETTING
{
/*
 * Parameter for GET_PS_FEATURESETTING escape
 */

#define FEATURESETTING_NUP         0
#define FEATURESETTING_OUTPUT      1
#define FEATURESETTING_PSLEVEL     2
#define FEATURESETTING_CUSTPAPER   3
#define FEATURESETTING_MIRROR      4
#define FEATURESETTING_NEGATIVE    5
#define FEATURESETTING_PROTOCOL    6
};

value DWORD _PR_JOBSTATUS
{
#define PR_JOBSTATUS                 0x0000
};

value DWORD _OBJ
{
#define OBJ_ERROR           0   [fail]
/* Object Definitions for EnumObjects() */
#define OBJ_PEN             1
#define OBJ_BRUSH           2
#define OBJ_DC              3
#define OBJ_METADC          4
#define OBJ_PAL             5
#define OBJ_FONT            6
#define OBJ_BITMAP          7
#define OBJ_REGION          8
#define OBJ_METAFILE        9
#define OBJ_MEMDC           10
#define OBJ_EXTPEN          11
#define OBJ_ENHMETADC       12
#define OBJ_ENHMETAFILE     13
#define OBJ_COLORSPACE      14
};

value DWORD _MWT
{
/* xform stuff */
#define MWT_IDENTITY        1
#define MWT_LEFTMULTIPLY    2
#define MWT_RIGHTMULTIPLY   3
};


/* Image Color Matching color definitions */

value DWORD _CS
{
#define CS_ENABLE                       0x00000001L
#define CS_DISABLE                      0x00000002L
#define CS_DELETE_TRANSFORM             0x00000003L
};


value DWORD _OUT
{
#define OUT_DEFAULT_PRECIS          0
#define OUT_STRING_PRECIS           1
#define OUT_CHARACTER_PRECIS        2
#define OUT_STROKE_PRECIS           3
#define OUT_TT_PRECIS               4
#define OUT_DEVICE_PRECIS           5
#define OUT_RASTER_PRECIS           6
#define OUT_TT_ONLY_PRECIS          7
#define OUT_OUTLINE_PRECIS          8
#define OUT_SCREEN_OUTLINE_PRECIS   9
#define OUT_PS_ONLY_PRECIS          10
};

value BYTE _OUTBYTE
{
#define OUT_DEFAULT_PRECIS          0
#define OUT_STRING_PRECIS           1
#define OUT_CHARACTER_PRECIS        2
#define OUT_STROKE_PRECIS           3
#define OUT_TT_PRECIS               4
#define OUT_DEVICE_PRECIS           5
#define OUT_RASTER_PRECIS           6
#define OUT_TT_ONLY_PRECIS          7
#define OUT_OUTLINE_PRECIS          8
#define OUT_SCREEN_OUTLINE_PRECIS   9
#define OUT_PS_ONLY_PRECIS          10
};

mask DWORD _CLIP
{
#define CLIP_DEFAULT_PRECIS     0
#define CLIP_CHARACTER_PRECIS   1
#define CLIP_STROKE_PRECIS      2
#define CLIP_MASK               0xf
#define CLIP_LH_ANGLES          0x10
#define CLIP_TT_ALWAYS          0x20
#define CLIP_EMBEDDED           0x80
};

mask BYTE _CLIPBYTE
{
#define CLIP_DEFAULT_PRECIS     0
#define CLIP_CHARACTER_PRECIS   1
#define CLIP_STROKE_PRECIS      2
#define CLIP_MASK               0xf
#define CLIP_LH_ANGLES          0x10
#define CLIP_TT_ALWAYS          0x20
#define CLIP_EMBEDDED           0x80
};

value DWORD _QUALITY
{
#define DEFAULT_QUALITY         0
#define DRAFT_QUALITY           1
#define PROOF_QUALITY           2
#define NONANTIALIASED_QUALITY  3
#define ANTIALIASED_QUALITY     4
#define CLEARTYPE_QUALITY       5
};

value BYTE _QUALITYBYTE
{
#define DEFAULT_QUALITY         0
#define DRAFT_QUALITY           1
#define PROOF_QUALITY           2
#define NONANTIALIASED_QUALITY  3
#define ANTIALIASED_QUALITY     4
};

value DWORD _PITCH
{
#define DEFAULT_PITCH           0
#define FIXED_PITCH             1
#define VARIABLE_PITCH          2
#define MONO_FONT               8
};

value DWORD _CHARSET
{
#define ANSI_CHARSET            0
#define DEFAULT_CHARSET         1 [fail]
#define SYMBOL_CHARSET          2
#define SHIFTJIS_CHARSET        128
#define HANGEUL_CHARSET         129
#define HANGUL_CHARSET          129
#define GB2312_CHARSET          134
#define CHINESEBIG5_CHARSET     136
#define OEM_CHARSET             255
#define JOHAB_CHARSET           130
#define HEBREW_CHARSET          177
#define ARABIC_CHARSET          178
#define GREEK_CHARSET           161
#define TURKISH_CHARSET         162
#define VIETNAMESE_CHARSET      163
#define THAI_CHARSET            222
#define EASTEUROPE_CHARSET      238
#define RUSSIAN_CHARSET         204

#define MAC_CHARSET             77
#define BALTIC_CHARSET          186
};

value BYTE _CHARSETBYTE
{
#define ANSI_CHARSET            0
#define DEFAULT_CHARSET         1
#define SYMBOL_CHARSET          2
#define SHIFTJIS_CHARSET        128
#define HANGEUL_CHARSET         129
#define HANGUL_CHARSET          129
#define GB2312_CHARSET          134
#define CHINESEBIG5_CHARSET     136
#define OEM_CHARSET             255
#define JOHAB_CHARSET           130
#define HEBREW_CHARSET          177
#define ARABIC_CHARSET          178
#define GREEK_CHARSET           161
#define TURKISH_CHARSET         162
#define VIETNAMESE_CHARSET      163
#define THAI_CHARSET            222
#define EASTEUROPE_CHARSET      238
#define RUSSIAN_CHARSET         204

#define MAC_CHARSET             77
#define BALTIC_CHARSET          186
};

mask DWORD _FS
{

#define FS_LATIN1               0x00000001L
#define FS_LATIN2               0x00000002L
#define FS_CYRILLIC             0x00000004L
#define FS_GREEK                0x00000008L
#define FS_TURKISH              0x00000010L
#define FS_HEBREW               0x00000020L
#define FS_ARABIC               0x00000040L
#define FS_BALTIC               0x00000080L
#define FS_VIETNAMESE           0x00000100L
#define FS_THAI                 0x00010000L
#define FS_JISJAPAN             0x00020000L
#define FS_CHINESESIMP          0x00040000L
#define FS_WANSUNG              0x00080000L
#define FS_CHINESETRAD          0x00100000L
#define FS_JOHAB                0x00200000L
#define FS_SYMBOL               0x80000000L
};

mask DWORD _FF
{

/* Font Families */
#define FF_DONTCARE         0x00  /* Don't care or don't know. */
#define FF_ROMAN            0x10  /* Variable stroke width, serifed.Times Roman, Century Schoolbook, etc. */
#define FF_SWISS            0x20  /* Variable stroke width, sans-serifed.Helvetica, Swiss, etc. */
#define FF_MODERN           0x30  /* Constant stroke width, serifed or sans-serifed. Pica, Elite, Courier, etc. */
#define FF_SCRIPT           0x40  /* Cursive, etc. */
#define FF_DECORATIVE       0x50  /* Old English, etc. */

};

mask BYTE _FFBYTE
{

/* Font Families */
#define FF_DONTCARE         0x00  /* Don't care or don't know. */
#define FF_ROMAN            0x10  /* Variable stroke width, serifed.Times Roman, Century Schoolbook, etc. */
#define FF_SWISS            0x20  /* Variable stroke width, sans-serifed.Helvetica, Swiss, etc. */
#define FF_MODERN           0x30  /* Constant stroke width, serifed or sans-serifed. Pica, Elite, Courier, etc. */
#define FF_SCRIPT           0x40  /* Cursive, etc. */
#define FF_DECORATIVE       0x50  /* Old English, etc. */

};

mask int _FW
{
/* Font Weights */
#define FW_DONTCARE         0
#define FW_THIN             100
#define FW_EXTRALIGHT       200
#define FW_LIGHT            300
#define FW_NORMAL           400
#define FW_MEDIUM           500
#define FW_SEMIBOLD         600
#define FW_BOLD             700
#define FW_EXTRABOLD        800
#define FW_HEAVY            900

};

value DWORD _PAN
{
#define PAN_FAMILYTYPE_INDEX        0
#define PAN_SERIFSTYLE_INDEX        1
#define PAN_WEIGHT_INDEX            2
#define PAN_PROPORTION_INDEX        3
#define PAN_CONTRAST_INDEX          4
#define PAN_STROKEVARIATION_INDEX   5
#define PAN_ARMSTYLE_INDEX          6
#define PAN_LETTERFORM_INDEX        7
#define PAN_MIDLINE_INDEX           8
#define PAN_XHEIGHT_INDEX           9
};

value DWORD _PAN_CULTURE
{
#define PAN_CULTURE_LATIN           0
};

value DWORD _PAN_FAMILY
{
#define PAN_FAMILY_ANY                         0 /* Any                            */
#define PAN_FAMILY_NO_FIT                      1 /* No Fit                         */

#define PAN_FAMILY_TEXT_DISPLAY         2 /* Text and Display               */
#define PAN_FAMILY_SCRIPT               3 /* Script                         */
#define PAN_FAMILY_DECORATIVE           4 /* Decorative                     */
#define PAN_FAMILY_PICTORIAL            5 /* Pictorial                      */
};
value DWORD _PAN_SERIF
{
#define PAN_SERIF_ANY                         0 /* Any                            */
#define PAN_SERIF_NO_FIT                      1 /* No Fit                         */
#define PAN_SERIF_COVE                  2 /* Cove                           */
#define PAN_SERIF_OBTUSE_COVE           3 /* Obtuse Cove                    */
#define PAN_SERIF_SQUARE_COVE           4 /* Square Cove                    */
#define PAN_SERIF_OBTUSE_SQUARE_COVE    5 /* Obtuse Square Cove             */
#define PAN_SERIF_SQUARE                6 /* Square                         */
#define PAN_SERIF_THIN                  7 /* Thin                           */
#define PAN_SERIF_BONE                  8 /* Bone                           */
#define PAN_SERIF_EXAGGERATED           9 /* Exaggerated                    */
#define PAN_SERIF_TRIANGLE             10 /* Triangle                       */
#define PAN_SERIF_NORMAL_SANS          11 /* Normal Sans                    */
#define PAN_SERIF_OBTUSE_SANS          12 /* Obtuse Sans                    */
#define PAN_SERIF_PERP_SANS            13 /* Prep Sans                      */
#define PAN_SERIF_FLARED               14 /* Flared                         */
#define PAN_SERIF_ROUNDED              15 /* Rounded                        */
};
value DWORD _PAN_WEIGHT_CULTURE
{
#define PAN_WEIGHT_ANY                         0 /* Any                            */
#define PAN_WEIGHT_NO_FIT                      1 /* No Fit                         */
#define PAN_WEIGHT_VERY_LIGHT           2 /* Very Light                     */
#define PAN_WEIGHT_LIGHT                3 /* Light                          */
#define PAN_WEIGHT_THIN                 4 /* Thin                           */
#define PAN_WEIGHT_BOOK                 5 /* Book                           */
#define PAN_WEIGHT_MEDIUM               6 /* Medium                         */
#define PAN_WEIGHT_DEMI                 7 /* Demi                           */
#define PAN_WEIGHT_BOLD                 8 /* Bold                           */
#define PAN_WEIGHT_HEAVY                9 /* Heavy                          */
#define PAN_WEIGHT_BLACK               10 /* Black                          */
#define PAN_WEIGHT_NORD                11 /* Nord                           */
};
value DWORD _PAN_PROP
{
#define PAN_PROP_ANY                         0 /* Any                            */
#define PAN_PROP_NO_FIT                      1 /* No Fit                         */
#define PAN_PROP_OLD_STYLE              2 /* Old Style                      */
#define PAN_PROP_MODERN                 3 /* Modern                         */
#define PAN_PROP_EVEN_WIDTH             4 /* Even Width                     */
#define PAN_PROP_EXPANDED               5 /* Expanded                       */
#define PAN_PROP_CONDENSED              6 /* Condensed                      */
#define PAN_PROP_VERY_EXPANDED          7 /* Very Expanded                  */
#define PAN_PROP_VERY_CONDENSED         8 /* Very Condensed                 */
#define PAN_PROP_MONOSPACED             9 /* Monospaced                     */
};
value DWORD _PAN_CONTRAST
{
#define PAN_CONTRAST_ANY                         0 /* Any                            */
#define PAN_CONTRAST_NO_FIT                      1 /* No Fit                         */
#define PAN_CONTRAST_NONE               2 /* None                           */
#define PAN_CONTRAST_VERY_LOW           3 /* Very Low                       */
#define PAN_CONTRAST_LOW                4 /* Low                            */
#define PAN_CONTRAST_MEDIUM_LOW         5 /* Medium Low                     */
#define PAN_CONTRAST_MEDIUM             6 /* Medium                         */
#define PAN_CONTRAST_MEDIUM_HIGH        7 /* Mediim High                    */
#define PAN_CONTRAST_HIGH               8 /* High                           */
#define PAN_CONTRAST_VERY_HIGH          9 /* Very High                      */
};
value DWORD _PAN_STROKE
{
#define PAN_STROKE_ANY                         0 /* Any                            */
#define PAN_STROKE_NO_FIT                      1 /* No Fit                         */
#define PAN_STROKE_GRADUAL_DIAG         2 /* Gradual/Diagonal               */
#define PAN_STROKE_GRADUAL_TRAN         3 /* Gradual/Transitional           */
#define PAN_STROKE_GRADUAL_VERT         4 /* Gradual/Vertical               */
#define PAN_STROKE_GRADUAL_HORZ         5 /* Gradual/Horizontal             */
#define PAN_STROKE_RAPID_VERT           6 /* Rapid/Vertical                 */
#define PAN_STROKE_RAPID_HORZ           7 /* Rapid/Horizontal               */
#define PAN_STROKE_INSTANT_VERT         8 /* Instant/Vertical               */
};
value DWORD _PAN_ARMS
{
#define PAN_ARMS_ANY                         0 /* Any                            */
#define PAN_ARMS_NO_FIT                      1 /* No Fit                         */
#define PAN_STRAIGHT_ARMS_HORZ          2 /* Straight Arms/Horizontal       */
#define PAN_STRAIGHT_ARMS_WEDGE         3 /* Straight Arms/Wedge            */
#define PAN_STRAIGHT_ARMS_VERT          4 /* Straight Arms/Vertical         */
#define PAN_STRAIGHT_ARMS_SINGLE_SERIF  5 /* Straight Arms/Single-Serif     */
#define PAN_STRAIGHT_ARMS_DOUBLE_SERIF  6 /* Straight Arms/Double-Serif     */
#define PAN_BENT_ARMS_HORZ              7 /* Non-Straight Arms/Horizontal   */
#define PAN_BENT_ARMS_WEDGE             8 /* Non-Straight Arms/Wedge        */
#define PAN_BENT_ARMS_VERT              9 /* Non-Straight Arms/Vertical     */
#define PAN_BENT_ARMS_SINGLE_SERIF     10 /* Non-Straight Arms/Single-Serif */
#define PAN_BENT_ARMS_DOUBLE_SERIF     11 /* Non-Straight Arms/Double-Serif */
};
value DWORD _PAN_LETT
{
#define PAN_LETT_ANY                         0 /* Any                            */
#define PAN_LETT_NO_FIT                      1 /* No Fit                         */
#define PAN_LETT_NORMAL_CONTACT         2 /* Normal/Contact                 */
#define PAN_LETT_NORMAL_WEIGHTED        3 /* Normal/Weighted                */
#define PAN_LETT_NORMAL_BOXED           4 /* Normal/Boxed                   */
#define PAN_LETT_NORMAL_FLATTENED       5 /* Normal/Flattened               */
#define PAN_LETT_NORMAL_ROUNDED         6 /* Normal/Rounded                 */
#define PAN_LETT_NORMAL_OFF_CENTER      7 /* Normal/Off Center              */
#define PAN_LETT_NORMAL_SQUARE          8 /* Normal/Square                  */
#define PAN_LETT_OBLIQUE_CONTACT        9 /* Oblique/Contact                */
#define PAN_LETT_OBLIQUE_WEIGHTED      10 /* Oblique/Weighted               */
#define PAN_LETT_OBLIQUE_BOXED         11 /* Oblique/Boxed                  */
#define PAN_LETT_OBLIQUE_FLATTENED     12 /* Oblique/Flattened              */
#define PAN_LETT_OBLIQUE_ROUNDED       13 /* Oblique/Rounded                */
#define PAN_LETT_OBLIQUE_OFF_CENTER    14 /* Oblique/Off Center             */
#define PAN_LETT_OBLIQUE_SQUARE        15 /* Oblique/Square                 */
};
value DWORD _PAN_MIDLINE
{
#define PAN_MIDLINE_ANY                         0 /* Any                            */
#define PAN_MIDLINE_NO_FIT                      1 /* No Fit                         */
#define PAN_MIDLINE_STANDARD_TRIMMED    2 /* Standard/Trimmed               */
#define PAN_MIDLINE_STANDARD_POINTED    3 /* Standard/Pointed               */
#define PAN_MIDLINE_STANDARD_SERIFED    4 /* Standard/Serifed               */
#define PAN_MIDLINE_HIGH_TRIMMED        5 /* High/Trimmed                   */
#define PAN_MIDLINE_HIGH_POINTED        6 /* High/Pointed                   */
#define PAN_MIDLINE_HIGH_SERIFED        7 /* High/Serifed                   */
#define PAN_MIDLINE_CONSTANT_TRIMMED    8 /* Constant/Trimmed               */
#define PAN_MIDLINE_CONSTANT_POINTED    9 /* Constant/Pointed               */
#define PAN_MIDLINE_CONSTANT_SERIFED   10 /* Constant/Serifed               */
#define PAN_MIDLINE_LOW_TRIMMED        11 /* Low/Trimmed                    */
#define PAN_MIDLINE_LOW_POINTED        12 /* Low/Pointed                    */
#define PAN_MIDLINE_LOW_SERIFED        13 /* Low/Serifed                    */
};
value DWORD _PAN_XHEIGHT
{
#define PAN_XHEIGHT_ANY                         0 /* Any                            */
#define PAN_XHEIGHT_NO_FIT                      1 /* No Fit                         */
#define PAN_XHEIGHT_CONSTANT_SMALL      2 /* Constant/Small                 */
#define PAN_XHEIGHT_CONSTANT_STD        3 /* Constant/Standard              */
#define PAN_XHEIGHT_CONSTANT_LARGE      4 /* Constant/Large                 */
#define PAN_XHEIGHT_DUCKING_SMALL       5 /* Ducking/Small                  */
#define PAN_XHEIGHT_DUCKING_STD         6 /* Ducking/Standard               */
#define PAN_XHEIGHT_DUCKING_LARGE       7 /* Ducking/Large                  */
};

mask DWORD _DISPLAY_DEVICE
{
#define DISPLAY_DEVICE_ATTACHED_TO_DESKTOP 0x00000001
#define DISPLAY_DEVICE_MULTI_DRIVER        0x00000002
#define DISPLAY_DEVICE_PRIMARY_DEVICE      0x00000004
#define DISPLAY_DEVICE_MIRRORING_DRIVER    0x00000008
#define DISPLAY_DEVICE_VGA_COMPATIBLE      0x00000010
#define DISPLAY_DEVICE_REMOVABLE           0x00000020
#define DISPLAY_DEVICE_MODESPRUNED         0x08000000
#define DISPLAY_DEVICE_REMOTE              0x04000000
#define DISPLAY_DEVICE_DISCONNECT          0x02000000
};
mask DWORD _DISPLAY_DEVICE_STATE
{
/* Child device state */
#define DISPLAY_DEVICE_ACTIVE              0x00000001
#define DISPLAY_DEVICE_ATTACHED            0x00000002
};


value DWORD _RDH
{
#define RDH_RECTANGLES  1
};
//  GetGlyphOutline constants

value DWORD _GGO
{
#define GGO_METRICS        0
#define GGO_BITMAP         1
#define GGO_NATIVE         2
#define GGO_BEZIER         3
#define  GGO_GRAY2_BITMAP   4
#define  GGO_GRAY4_BITMAP   5
#define  GGO_GRAY8_BITMAP   6
#define  GGO_GLYPH_INDEX    0x0080
#define  GGO_UNHINTED       0x0100
};

value DWORD _TT_POLYGON
{
#define TT_POLYGON_TYPE   24
};

value WORD _TT_PRIM
{
#define TT_PRIM_LINE       1
#define TT_PRIM_QSPLINE    2
#define TT_PRIM_CSPLINE    3
};

typedef struct tagMETAFILEPICT {
    LONG      mm;
    LONG      xExt;
    LONG      yExt;
    HMETAFILE hMF;
} METAFILEPICT, *LPMETAFILEPICT;


/* Logcolorspace signature */

// #define LCS_SIGNATURE           'PSOC'

/* Logcolorspace lcsType values */

// #define LCS_sRGB                'sRGB'
// #define LCS_WINDOWS_COLOR_SPACE 'Win '  // Windows default color space

typedef LONG   LCSCSTYPE;
value DWORD _LCSCSTYPE
{
#define LCS_CALIBRATED_RGB              0x00000000L
#define LCS_DEVICE_RGB                  0x00000001L
#define LCS_DEVICE_CMYK                 0x00000002L
};

mask DWORD _GCP
{
#define GCP_DBCS           0x0001
#define GCP_REORDER        0x0002
#define GCP_USEKERNING     0x0008
#define GCP_GLYPHSHAPE     0x0010
#define GCP_LIGATE         0x0020
////#define GCP_GLYPHINDEXING  0x0080
#define GCP_DIACRITIC      0x0100
#define GCP_KASHIDA        0x0400
#define GCP_ERROR          0x8000
//#define FLI_MASK           0x103B

#define GCP_JUSTIFY        0x00010000L
////#define GCP_NODIACRITICS   0x00020000L
#define FLI_GLYPHS         0x00040000L
#define GCP_CLASSIN        0x00080000L
#define GCP_MAXEXTENT      0x00100000L
#define GCP_JUSTIFYIN      0x00200000L
#define GCP_DISPLAYZWG      0x00400000L
#define GCP_SYMSWAPOFF      0x00800000L
#define GCP_NUMERICOVERRIDE 0x01000000L
#define GCP_NEUTRALOVERRIDE 0x02000000L
#define GCP_NUMERICSLATIN   0x04000000L
#define GCP_NUMERICSLOCAL   0x08000000L
};
mask DWORD _GCPCLASS
{
#define GCPCLASS_LATIN                  1
#define GCPCLASS_HEBREW                 2
//#define GCPCLASS_ARABIC                 2
#define GCPCLASS_NEUTRAL                3
#define GCPCLASS_LOCALNUMBER            4
#define GCPCLASS_LATINNUMBER            5
#define GCPCLASS_LATINNUMERICTERMINATOR 6
#define GCPCLASS_LATINNUMERICSEPARATOR  7
#define GCPCLASS_NUMERICSEPARATOR       8
#define GCPCLASS_PREBOUNDLTR         0x80
#define GCPCLASS_PREBOUNDRTL         0x40
#define GCPCLASS_POSTBOUNDLTR        0x20
#define GCPCLASS_POSTBOUNDRTL        0x10

#define GCPGLYPH_LINKBEFORE          0x8000
#define GCPGLYPH_LINKAFTER           0x4000
};

typedef LONG    LCSGAMUTMATCH;
value DWORD _LCSGAMUTMATCH
{
#define LCS_GM_BUSINESS                 0x00000001L
#define LCS_GM_GRAPHICS                 0x00000002L
#define LCS_GM_IMAGES                   0x00000004L
#define LCS_GM_ABS_COLORIMETRIC         0x00000008L
};


value UINT _UpdateICMRegKey
{
/* UpdateICMRegKey Constants               */
#define ICM_ADDPROFILE                  1
#define ICM_DELETEPROFILE               2
#define ICM_QUERYPROFILE                3
#define ICM_SETDEFAULTPROFILE           4
#define ICM_REGISTERICMATCHER           5
#define ICM_UNREGISTERICMATCHER         6
#define ICM_QUERYMATCH                  7
};


value DWORD _biCompression
{
/* constants for the biCompression field */
#define BI_RGB        0L
#define BI_RLE8       1L
#define BI_RLE4       2L
#define BI_BITFIELDS  3L
#define BI_JPEG       4L
#define BI_PNG        5L
};

value DWORD _TCI_SRC
{
#define TCI_SRCCHARSET  1
#define TCI_SRCCODEPAGE 2
#define TCI_SRCFONTSIG  3
};


mask WORD _RASTERIZER_STATUS_Flag
{
/* bits defined in wFlags of RASTERIZER_STATUS */
#define TT_AVAILABLE    0x0001
#define TT_ENABLED      0x0002
};


mask BYTE _PFD
{
/* pixel types */
#define PFD_TYPE_RGBA        0
#define PFD_TYPE_COLORINDEX  1
};

mask BYTE _PFD_LAYER
{
/* layer types */
#define PFD_MAIN_PLANE       0
#define PFD_OVERLAY_PLANE    1
#define PFD_UNDERLAY_PLANE   -1
};
mask DWORD _PIXELFORMATDESCRIPTOR
{
/* PIXELFORMATDESCRIPTOR flags */
#define PFD_DOUBLEBUFFER            0x00000001
#define PFD_STEREO                  0x00000002
#define PFD_DRAW_TO_WINDOW          0x00000004
#define PFD_DRAW_TO_BITMAP          0x00000008
#define PFD_SUPPORT_GDI             0x00000010
#define PFD_SUPPORT_OPENGL          0x00000020
#define PFD_GENERIC_FORMAT          0x00000040
#define PFD_NEED_PALETTE            0x00000080
#define PFD_NEED_SYSTEM_PALETTE     0x00000100
#define PFD_SWAP_EXCHANGE           0x00000200
#define PFD_SWAP_COPY               0x00000400
#define PFD_SWAP_LAYER_BUFFERS      0x00000800
#define PFD_GENERIC_ACCELERATED     0x00001000
#define PFD_SUPPORT_DIRECTDRAW      0x00002000

/* PIXELFORMATDESCRIPTOR flags for use in ChoosePixelFormat only */
#define PFD_DEPTH_DONTCARE          0x20000000
#define PFD_DOUBLEBUFFER_DONTCARE   0x40000000
#define PFD_STEREO_DONTCARE         0x80000000
};



mask DWORD _DeviceMode
{
/* mode selections for the device mode function */
#define DM_UPDATE           1
#define DM_COPY             2
#define DM_PROMPT           4
#define DM_MODIFY           8
};

value DWORD _DC_PRINTRATEUNIT
{
#define   PRINTRATEUNIT_PPM     1
#define   PRINTRATEUNIT_CPS     2
#define   PRINTRATEUNIT_LPM     3
#define   PRINTRATEUNIT_IPM     4
};
mask DWORD _DCTT
{
/* bit fields of the return value (DWORD) for DC_TRUETYPE */
#define DCTT_BITMAP             0x0000001L
#define DCTT_DOWNLOAD           0x0000002L
#define DCTT_SUBDEV             0x0000004L
#define DCTT_DOWNLOAD_OUTLINE   0x0000008L
};
value DWORD _DCBA
{
/* return values for DC_BINADJUST */
#define DCBA_FACEUPNONE       0x0000
#define DCBA_FACEUPCENTER     0x0001
#define DCBA_FACEUPLEFT       0x0002
#define DCBA_FACEUPRIGHT      0x0003
#define DCBA_FACEDOWNNONE     0x0100
#define DCBA_FACEDOWNCENTER   0x0101
#define DCBA_FACEDOWNLEFT     0x0102
#define DCBA_FACEDOWNRIGHT    0x0103
};

/* flAccel flags for the GLYPHSET structure above */
value DWORD _GS_8BIT_INDICES
{
#define GS_8BIT_INDICES     0x00000001
};

/* flags for GetGlyphIndices */
value DWORD _GGI_MARK_NONEXISTING_GLYPHS
{

#define GGI_MARK_NONEXISTING_GLYPHS  0X0001
};
value DWORD _FR
{

#define FR_PRIVATE     0x10
#define FR_NOT_ENUM    0x20
};
value BYTE _AC_SRC_OVER
{
//
// currentlly defined blend function
//
#define AC_SRC_OVER                 0x00
};
value BYTE _AC_SRC_ALPHA
{
//
// currentlly defined blend function
//
#define AC_SRC_ALPHA                 0x01
};
value ULONG _GRADIENT_FILL
{
//
// gradient drawing modes
//

#define GRADIENT_FILL_RECT_H    0x00000000
#define GRADIENT_FILL_RECT_V    0x00000001
#define GRADIENT_FILL_TRIANGLE  0x00000002
#define GRADIENT_FILL_OP_FLAG   0x000000ff
};
value WORD _COLORADJUSTMENTValue
{

/* Flags value for COLORADJUSTMENT */
#define CA_NEGATIVE                 0x0001
#define CA_LOG_FILTER               0x0002
};
value WORD _IlluminantIndexValue
{

/* IlluminantIndex values */
#define ILLUMINANT_DEVICE_DEFAULT   0
#define ILLUMINANT_A                1
#define ILLUMINANT_B                2
#define ILLUMINANT_C                3
#define ILLUMINANT_D50              4
#define ILLUMINANT_D55              5
#define ILLUMINANT_D65              6
#define ILLUMINANT_D75              7
#define ILLUMINANT_F2               8
};

value DWORD _ICM
{
#define ICM_OFF               1
#define ICM_ON                2
#define ICM_QUERY             3
#define ICM_DONE_OUTSIDEDC    4
};

value DWORD _EMR
{

// Enhanced metafile record types.

#define EMR_HEADER                      1
#define EMR_POLYBEZIER                  2
#define EMR_POLYGON                     3
#define EMR_POLYLINE                    4
#define EMR_POLYBEZIERTO                5
#define EMR_POLYLINETO                  6
#define EMR_POLYPOLYLINE                7
#define EMR_POLYPOLYGON                 8
#define EMR_SETWINDOWEXTEX              9
#define EMR_SETWINDOWORGEX              10
#define EMR_SETVIEWPORTEXTEX            11
#define EMR_SETVIEWPORTORGEX            12
#define EMR_SETBRUSHORGEX               13
#define EMR_EOF                         14
#define EMR_SETPIXELV                   15
#define EMR_SETMAPPERFLAGS              16
#define EMR_SETMAPMODE                  17
#define EMR_SETBKMODE                   18
#define EMR_SETPOLYFILLMODE             19
#define EMR_SETROP2                     20
#define EMR_SETSTRETCHBLTMODE           21
#define EMR_SETTEXTALIGN                22
#define EMR_SETCOLORADJUSTMENT          23
#define EMR_SETTEXTCOLOR                24
#define EMR_SETBKCOLOR                  25
#define EMR_OFFSETCLIPRGN               26
#define EMR_MOVETOEX                    27
#define EMR_SETMETARGN                  28
#define EMR_EXCLUDECLIPRECT             29
#define EMR_INTERSECTCLIPRECT           30
#define EMR_SCALEVIEWPORTEXTEX          31
#define EMR_SCALEWINDOWEXTEX            32
#define EMR_SAVEDC                      33
#define EMR_RESTOREDC                   34
#define EMR_SETWORLDTRANSFORM           35
#define EMR_MODIFYWORLDTRANSFORM        36
#define EMR_SELECTOBJECT                37
#define EMR_CREATEPEN                   38
#define EMR_CREATEBRUSHINDIRECT         39
#define EMR_DELETEOBJECT                40
#define EMR_ANGLEARC                    41
#define EMR_ELLIPSE                     42
#define EMR_RECTANGLE                   43
#define EMR_ROUNDRECT                   44
#define EMR_ARC                         45
#define EMR_CHORD                       46
#define EMR_PIE                         47
#define EMR_SELECTPALETTE               48
#define EMR_CREATEPALETTE               49
#define EMR_SETPALETTEENTRIES           50
#define EMR_RESIZEPALETTE               51
#define EMR_REALIZEPALETTE              52
#define EMR_EXTFLOODFILL                53
#define EMR_LINETO                      54
#define EMR_ARCTO                       55
#define EMR_POLYDRAW                    56
#define EMR_SETARCDIRECTION             57
#define EMR_SETMITERLIMIT               58
#define EMR_BEGINPATH                   59
#define EMR_ENDPATH                     60
#define EMR_CLOSEFIGURE                 61
#define EMR_FILLPATH                    62
#define EMR_STROKEANDFILLPATH           63
#define EMR_STROKEPATH                  64
#define EMR_FLATTENPATH                 65
#define EMR_WIDENPATH                   66
#define EMR_SELECTCLIPPATH              67
#define EMR_ABORTPATH                   68

#define EMR_GDICOMMENT                  70
#define EMR_FILLRGN                     71
#define EMR_FRAMERGN                    72
#define EMR_INVERTRGN                   73
#define EMR_PAINTRGN                    74
#define EMR_EXTSELECTCLIPRGN            75
#define EMR_BITBLT                      76
#define EMR_STRETCHBLT                  77
#define EMR_MASKBLT                     78
#define EMR_PLGBLT                      79
#define EMR_SETDIBITSTODEVICE           80
#define EMR_STRETCHDIBITS               81
#define EMR_EXTCREATEFONTINDIRECTW      82
#define EMR_EXTTEXTOUTA                 83
#define EMR_EXTTEXTOUTW                 84
#define EMR_POLYBEZIER16                85
#define EMR_POLYGON16                   86
#define EMR_POLYLINE16                  87
#define EMR_POLYBEZIERTO16              88
#define EMR_POLYLINETO16                89
#define EMR_POLYPOLYLINE16              90
#define EMR_POLYPOLYGON16               91
#define EMR_POLYDRAW16                  92
#define EMR_CREATEMONOBRUSH             93
#define EMR_CREATEDIBPATTERNBRUSHPT     94
#define EMR_EXTCREATEPEN                95
#define EMR_POLYTEXTOUTA                96
#define EMR_POLYTEXTOUTW                97

#define EMR_SETICMMODE                  98
#define EMR_CREATECOLORSPACE            99
#define EMR_SETCOLORSPACE              100
#define EMR_DELETECOLORSPACE           101
#define EMR_GLSRECORD                  102
#define EMR_GLSBOUNDEDRECORD           103
#define EMR_PIXELFORMAT                104

#define EMR_RESERVED_105               105
#define EMR_RESERVED_106               106
#define EMR_RESERVED_107               107
#define EMR_RESERVED_108               108
#define EMR_RESERVED_109               109
#define EMR_RESERVED_110               110
#define EMR_COLORCORRECTPALETTE        111
#define EMR_SETICMPROFILEA             112
#define EMR_SETICMPROFILEW             113
#define EMR_ALPHABLEND                 114
#define EMR_SETLAYOUT                  115
#define EMR_TRANSPARENTBLT             116
#define EMR_RESERVED_117               117
#define EMR_GRADIENTFILL               118
#define EMR_RESERVED_119               119
#define EMR_RESERVED_120               120
#define EMR_COLORMATCHTOTARGETW        121
#define EMR_CREATECOLORSPACEW          122
};
value DWORD _SETICMPROFILE_EMBEDED
{

#define SETICMPROFILE_EMBEDED           0x00000001
};
value DWORD _GDICOMMENT
{
#define GDICOMMENT_IDENTIFIER           0x43494447
#define GDICOMMENT_WINDOWS_METAFILE     0x80000001
#define GDICOMMENT_BEGINGROUP           0x00000002
#define GDICOMMENT_ENDGROUP             0x00000003
#define GDICOMMENT_MULTIFORMATS         0x40000004
#define GDICOMMENT_UNICODE_STRING       0x00000040
#define GDICOMMENT_UNICODE_END          0x00000080
};

value DWORD _WGL_FONT
{

#define WGL_FONT_LINES      0
#define WGL_FONT_POLYGONS   1
};
value DWORD _LAYERPLANEDESCRIPTOR
{

/* LAYERPLANEDESCRIPTOR flags */
#define LPD_DOUBLEBUFFER        0x00000001
#define LPD_STEREO              0x00000002
#define LPD_SUPPORT_GDI         0x00000010
#define LPD_SUPPORT_OPENGL      0x00000020
#define LPD_SHARE_DEPTH         0x00000040
#define LPD_SHARE_STENCIL       0x00000080
#define LPD_SHARE_ACCUM         0x00000100
#define LPD_SWAP_EXCHANGE       0x00000200
#define LPD_SWAP_COPY           0x00000400
#define LPD_TRANSPARENT         0x00001000
};
value BYTE _LPD_TYPE
{

#define LPD_TYPE_RGBA        0
#define LPD_TYPE_COLORINDEX  1
};
value DWORD _WGL_SWAP
{

/* wglSwapLayerBuffers flags */
#define WGL_SWAP_MAIN_PLANE     0x00000001
#define WGL_SWAP_OVERLAY1       0x00000002
#define WGL_SWAP_OVERLAY2       0x00000004
#define WGL_SWAP_OVERLAY3       0x00000008
#define WGL_SWAP_OVERLAY4       0x00000010
#define WGL_SWAP_OVERLAY5       0x00000020
#define WGL_SWAP_OVERLAY6       0x00000040
#define WGL_SWAP_OVERLAY7       0x00000080
#define WGL_SWAP_OVERLAY8       0x00000100
#define WGL_SWAP_OVERLAY9       0x00000200
#define WGL_SWAP_OVERLAY10      0x00000400
#define WGL_SWAP_OVERLAY11      0x00000800
#define WGL_SWAP_OVERLAY12      0x00001000
#define WGL_SWAP_OVERLAY13      0x00002000
#define WGL_SWAP_OVERLAY14      0x00004000
#define WGL_SWAP_OVERLAY15      0x00008000
#define WGL_SWAP_UNDERLAY1      0x00010000
#define WGL_SWAP_UNDERLAY2      0x00020000
#define WGL_SWAP_UNDERLAY3      0x00040000
#define WGL_SWAP_UNDERLAY4      0x00080000
#define WGL_SWAP_UNDERLAY5      0x00100000
#define WGL_SWAP_UNDERLAY6      0x00200000
#define WGL_SWAP_UNDERLAY7      0x00400000
#define WGL_SWAP_UNDERLAY8      0x00800000
#define WGL_SWAP_UNDERLAY9      0x01000000
#define WGL_SWAP_UNDERLAY10     0x02000000
#define WGL_SWAP_UNDERLAY11     0x04000000
#define WGL_SWAP_UNDERLAY12     0x08000000
#define WGL_SWAP_UNDERLAY13     0x10000000
#define WGL_SWAP_UNDERLAY14     0x20000000
#define WGL_SWAP_UNDERLAY15     0x40000000
};

mask DWORD _otmfsSelection
{
#define Italic          0x00
#define Underscore      0x01
#define Negative        0x02
#define Outline         0x04
#define Strikeout       0x08
#define Bold            0x10
};

value WORD RectangleStyleValue
{
#define BlackRectangle     0
#define WhiteRectangle     1
#define GrayRectangle      2
};

mask BYTE _TMPF
{
/* tmPitchAndFamily flags */
#define TMPF_FIXED_PITCH    0x01
#define TMPF_VECTOR         0x02
#define TMPF_DEVICE         0x08
#define TMPF_TRUETYPE       0x04
};

mask DWORD _NTMFlags
{
/* ntmFlags field flags */
#define NTM_REGULAR     0x00000040L
#define NTM_BOLD        0x00000020L
#define NTM_ITALIC      0x00000001L

#define NTM_NONNEGATIVE_AC  0x00010000
#define NTM_PS_OPENTYPE     0x00020000
#define NTM_TT_OPENTYPE     0x00040000
#define NTM_MULTIPLEMASTER  0x00080000
#define NTM_TYPE1           0x00100000
#define NTM_DSIG            0x00200000
};



value BYTE PanFamilyType
{
#define PAN_ANY                         0 /* Any                            */
#define PAN_NO_FIT                      1 /* No Fit                         */

#define PAN_FAMILY_TEXT_DISPLAY         2 /* Text and Display               */
#define PAN_FAMILY_SCRIPT               3 /* Script                         */
#define PAN_FAMILY_DECORATIVE           4 /* Decorative                     */
#define PAN_FAMILY_PICTORIAL            5 /* Pictorial                      */
};
value BYTE PanSerifType
{
#define PAN_SERIF_COVE                  2 /* Cove                           */
#define PAN_SERIF_OBTUSE_COVE           3 /* Obtuse Cove                    */
#define PAN_SERIF_SQUARE_COVE           4 /* Square Cove                    */
#define PAN_SERIF_OBTUSE_SQUARE_COVE    5 /* Obtuse Square Cove             */
#define PAN_SERIF_SQUARE                6 /* Square                         */
#define PAN_SERIF_THIN                  7 /* Thin                           */
#define PAN_SERIF_BONE                  8 /* Bone                           */
#define PAN_SERIF_EXAGGERATED           9 /* Exaggerated                    */
#define PAN_SERIF_TRIANGLE             10 /* Triangle                       */
#define PAN_SERIF_NORMAL_SANS          11 /* Normal Sans                    */
#define PAN_SERIF_OBTUSE_SANS          12 /* Obtuse Sans                    */
#define PAN_SERIF_PERP_SANS            13 /* Prep Sans                      */
#define PAN_SERIF_FLARED               14 /* Flared                         */
#define PAN_SERIF_ROUNDED              15 /* Rounded                        */
};
value BYTE PanWeightType
{
#define PAN_WEIGHT_VERY_LIGHT           2 /* Very Light                     */
#define PAN_WEIGHT_LIGHT                3 /* Light                          */
#define PAN_WEIGHT_THIN                 4 /* Thin                           */
#define PAN_WEIGHT_BOOK                 5 /* Book                           */
#define PAN_WEIGHT_MEDIUM               6 /* Medium                         */
#define PAN_WEIGHT_DEMI                 7 /* Demi                           */
#define PAN_WEIGHT_BOLD                 8 /* Bold                           */
#define PAN_WEIGHT_HEAVY                9 /* Heavy                          */
#define PAN_WEIGHT_BLACK               10 /* Black                          */
#define PAN_WEIGHT_NORD                11 /* Nord                           */
};
value BYTE PanPropType
{
#define PAN_PROP_OLD_STYLE              2 /* Old Style                      */
#define PAN_PROP_MODERN                 3 /* Modern                         */
#define PAN_PROP_EVEN_WIDTH             4 /* Even Width                     */
#define PAN_PROP_EXPANDED               5 /* Expanded                       */
#define PAN_PROP_CONDENSED              6 /* Condensed                      */
#define PAN_PROP_VERY_EXPANDED          7 /* Very Expanded                  */
#define PAN_PROP_VERY_CONDENSED         8 /* Very Condensed                 */
#define PAN_PROP_MONOSPACED             9 /* Monospaced                     */
};
value BYTE PanConstrastType
{
#define PAN_CONTRAST_NONE               2 /* None                           */
#define PAN_CONTRAST_VERY_LOW           3 /* Very Low                       */
#define PAN_CONTRAST_LOW                4 /* Low                            */
#define PAN_CONTRAST_MEDIUM_LOW         5 /* Medium Low                     */
#define PAN_CONTRAST_MEDIUM             6 /* Medium                         */
#define PAN_CONTRAST_MEDIUM_HIGH        7 /* Mediim High                    */
#define PAN_CONTRAST_HIGH               8 /* High                           */
#define PAN_CONTRAST_VERY_HIGH          9 /* Very High                      */
};
value BYTE PanStrokeType
{
#define PAN_STROKE_GRADUAL_DIAG         2 /* Gradual/Diagonal               */
#define PAN_STROKE_GRADUAL_TRAN         3 /* Gradual/Transitional           */
#define PAN_STROKE_GRADUAL_VERT         4 /* Gradual/Vertical               */
#define PAN_STROKE_GRADUAL_HORZ         5 /* Gradual/Horizontal             */
#define PAN_STROKE_RAPID_VERT           6 /* Rapid/Vertical                 */
#define PAN_STROKE_RAPID_HORZ           7 /* Rapid/Horizontal               */
#define PAN_STROKE_INSTANT_VERT         8 /* Instant/Vertical               */
};
value BYTE PanArmsType
{
#define PAN_STRAIGHT_ARMS_HORZ          2 /* Straight Arms/Horizontal       */
#define PAN_STRAIGHT_ARMS_WEDGE         3 /* Straight Arms/Wedge            */
#define PAN_STRAIGHT_ARMS_VERT          4 /* Straight Arms/Vertical         */
#define PAN_STRAIGHT_ARMS_SINGLE_SERIF  5 /* Straight Arms/Single-Serif     */
#define PAN_STRAIGHT_ARMS_DOUBLE_SERIF  6 /* Straight Arms/Double-Serif     */
#define PAN_BENT_ARMS_HORZ              7 /* Non-Straight Arms/Horizontal   */
#define PAN_BENT_ARMS_WEDGE             8 /* Non-Straight Arms/Wedge        */
#define PAN_BENT_ARMS_VERT              9 /* Non-Straight Arms/Vertical     */
#define PAN_BENT_ARMS_SINGLE_SERIF     10 /* Non-Straight Arms/Single-Serif */
#define PAN_BENT_ARMS_DOUBLE_SERIF     11 /* Non-Straight Arms/Double-Serif */
};
value BYTE PanLettType
{
#define PAN_LETT_NORMAL_CONTACT         2 /* Normal/Contact                 */
#define PAN_LETT_NORMAL_WEIGHTED        3 /* Normal/Weighted                */
#define PAN_LETT_NORMAL_BOXED           4 /* Normal/Boxed                   */
#define PAN_LETT_NORMAL_FLATTENED       5 /* Normal/Flattened               */
#define PAN_LETT_NORMAL_ROUNDED         6 /* Normal/Rounded                 */
#define PAN_LETT_NORMAL_OFF_CENTER      7 /* Normal/Off Center              */
#define PAN_LETT_NORMAL_SQUARE          8 /* Normal/Square                  */
#define PAN_LETT_OBLIQUE_CONTACT        9 /* Oblique/Contact                */
#define PAN_LETT_OBLIQUE_WEIGHTED      10 /* Oblique/Weighted               */
#define PAN_LETT_OBLIQUE_BOXED         11 /* Oblique/Boxed                  */
#define PAN_LETT_OBLIQUE_FLATTENED     12 /* Oblique/Flattened              */
#define PAN_LETT_OBLIQUE_ROUNDED       13 /* Oblique/Rounded                */
#define PAN_LETT_OBLIQUE_OFF_CENTER    14 /* Oblique/Off Center             */
#define PAN_LETT_OBLIQUE_SQUARE        15 /* Oblique/Square                 */
};
value BYTE PanMidlineType
{
#define PAN_MIDLINE_STANDARD_TRIMMED    2 /* Standard/Trimmed               */
#define PAN_MIDLINE_STANDARD_POINTED    3 /* Standard/Pointed               */
#define PAN_MIDLINE_STANDARD_SERIFED    4 /* Standard/Serifed               */
#define PAN_MIDLINE_HIGH_TRIMMED        5 /* High/Trimmed                   */
#define PAN_MIDLINE_HIGH_POINTED        6 /* High/Pointed                   */
#define PAN_MIDLINE_HIGH_SERIFED        7 /* High/Serifed                   */
#define PAN_MIDLINE_CONSTANT_TRIMMED    8 /* Constant/Trimmed               */
#define PAN_MIDLINE_CONSTANT_POINTED    9 /* Constant/Pointed               */
#define PAN_MIDLINE_CONSTANT_SERIFED   10 /* Constant/Serifed               */
#define PAN_MIDLINE_LOW_TRIMMED        11 /* Low/Trimmed                    */
#define PAN_MIDLINE_LOW_POINTED        12 /* Low/Pointed                    */
#define PAN_MIDLINE_LOW_SERIFED        13 /* Low/Serifed                    */
};
value BYTE PanXHeightType
{
#define PAN_XHEIGHT_CONSTANT_SMALL      2 /* Constant/Small                 */
#define PAN_XHEIGHT_CONSTANT_STD        3 /* Constant/Standard              */
#define PAN_XHEIGHT_CONSTANT_LARGE      4 /* Constant/Large                 */
#define PAN_XHEIGHT_DUCKING_SMALL       5 /* Ducking/Small                  */
#define PAN_XHEIGHT_DUCKING_STD         6 /* Ducking/Standard               */
#define PAN_XHEIGHT_DUCKING_LARGE       7 /* Ducking/Large                  */
};

value DWORD EMRSignature
{
#define ENHMETA_SIGNATURE       0x464D4520
#define EPS_SIGNATURE                   0x46535045
};

value DWORD EMRColorSpaceFlagMask
{
#define CREATECOLORSPACE_EMBEDED        0x00000001
};

value DWORD EMRColorMatchFlagMask
{
#define COLORMATCHTOTARGET_EMBEDED      0x00000001
};



//==================================================================================
//==================================================================================
//==================================================================================
typedef struct  tagCOLORADJUSTMENT {
  WORD  caSize;
  _COLORADJUSTMENTValue  caFlags;
  _IlluminantIndexValue  caIlluminantIndex;
  WORD  caRedGamma;
  WORD  caGreenGamma;
  WORD  caBlueGamma;
  WORD  caReferenceBlack;
  WORD  caReferenceWhite;
  SHORT caContrast;
  SHORT caBrightness;
  SHORT caColorfulness;
  SHORT caRedGreenTint;
} COLORADJUSTMENT, *PCOLORADJUSTMENT,  *LPCOLORADJUSTMENT;

typedef struct _POINTL {
  LONG x;
  LONG y;
} POINTL, *PPOINTL;

typedef struct tagPOINTS {
  SHORT x;
  SHORT y;
} POINTS, *PPOINTS;

typedef struct _DRAWPATRECT {
        POINT ptPosition;
        POINT ptSize;
        RectangleStyleValue wStyle;
        WORD wPattern;
} DRAWPATRECT, *PDRAWPATRECT;

/*
 * Information about output options
 */

typedef struct _PSFEATURE_OUTPUT {

    BOOL bPageIndependent;
    BOOL bSetPageDevice;

} PSFEATURE_OUTPUT, *PPSFEATURE_OUTPUT;

/*
 * Information about custom paper size
 */

typedef struct _PSFEATURE_CUSTPAPER {

    LONG lOrientation;
    LONG lWidth;
    LONG lHeight;
    LONG lWidthOffset;
    LONG lHeightOffset;

} PSFEATURE_CUSTPAPER, *PPSFEATURE_CUSTPAPER;

/*
 * Header structure for the input buffer to POSTSCRIPT_INJECTION escape
 */

typedef struct _PSINJECTDATA {

    DWORD   DataBytes;          /* number of raw data bytes */
    _PSINJECT   InjectionPoint;     /* injection point */
    WORD   Flags;              /* flags */

    /* Followed by raw data to be injected */

} PSINJECTDATA, *PPSINJECTDATA;


typedef struct  tagXFORM
  {
    FLOAT   eM11;
    FLOAT   eM12;
    FLOAT   eM21;
    FLOAT   eM22;
    FLOAT   eDx;
    FLOAT   eDy;
  } XFORM, *PXFORM,  *LPXFORM;

/* Bitmap Header Definition */
typedef struct tagBITMAP
  {
    LONG        bmType;
    LONG        bmWidth;
    LONG        bmHeight;
    LONG        bmWidthBytes;
    WORD        bmPlanes;
    WORD        bmBitsPixel;
    LPVOID      bmBits;
  } BITMAP, *PBITMAP,  *NPBITMAP,  *LPBITMAP;

typedef struct tagRGBTRIPLE {
        BYTE    rgbtBlue;
        BYTE    rgbtGreen;
        BYTE    rgbtRed;
} RGBTRIPLE;

typedef struct tagRGBQUAD {
        BYTE    rgbBlue;
        BYTE    rgbGreen;
        BYTE    rgbRed;
        BYTE    rgbReserved;
} RGBQUAD;
typedef RGBQUAD * LPRGBQUAD;


typedef long            FXPT16DOT16;
typedef long            *LPFXPT16DOT16;

typedef long            FXPT2DOT30;
typedef long            *LPFXPT2DOT30;

/* ICM Color Definitions */
// The following two structures are used for defining RGB's in terms of CIEXYZ.

typedef struct tagCIEXYZ
{
        FXPT2DOT30 ciexyzX;
        FXPT2DOT30 ciexyzY;
        FXPT2DOT30 ciexyzZ;
} CIEXYZ;
typedef CIEXYZ   *LPCIEXYZ;

typedef struct tagICEXYZTRIPLE
{
        CIEXYZ  ciexyzRed;
        CIEXYZ  ciexyzGreen;
        CIEXYZ  ciexyzBlue;
} CIEXYZTRIPLE;
typedef CIEXYZTRIPLE     *LPCIEXYZTRIPLE;

// The next structures the logical color space. Unlike pens and brushes,
// but like palettes, there is only one way to create a LogColorSpace.
// A pointer to it must be passed, its elements can't be pushed as
// arguments.


typedef struct tagLOGCOLORSPACEA {
    DWORD lcsSignature;
    DWORD lcsVersion;
    DWORD lcsSize;
    _LCSCSTYPE lcsCSType;
    _LCSGAMUTMATCH lcsIntent;
    CIEXYZTRIPLE lcsEndpoints;
    DWORD lcsGammaRed;
    DWORD lcsGammaGreen;
    DWORD lcsGammaBlue;
    CHAR   lcsFilename[260];
} LOGCOLORSPACEA, *LPLOGCOLORSPACEA;
typedef struct tagLOGCOLORSPACEW {
    DWORD lcsSignature;
    DWORD lcsVersion;
    DWORD lcsSize;
    _LCSCSTYPE lcsCSType;
    _LCSGAMUTMATCH lcsIntent;
    CIEXYZTRIPLE lcsEndpoints;
    DWORD lcsGammaRed;
    DWORD lcsGammaGreen;
    DWORD lcsGammaBlue;
    WCHAR  lcsFilename[260];
} LOGCOLORSPACEW, *LPLOGCOLORSPACEW;



/* structures for defining DIBs */
typedef struct tagBITMAPCOREHEADER {
        DWORD   bcSize;                 /* used to get to color table */
        WORD    bcWidth;
        WORD    bcHeight;
        WORD    bcPlanes;
        WORD    bcBitCount;
} BITMAPCOREHEADER,  *LPBITMAPCOREHEADER, *PBITMAPCOREHEADER;

typedef struct tagBITMAPINFOHEADER{
        DWORD      biSize;
        LONG       biWidth;
        LONG       biHeight;
        WORD       biPlanes;
        WORD       biBitCount;
        _biCompression      biCompression;
        DWORD      biSizeImage;
        LONG       biXPelsPerMeter;
        LONG       biYPelsPerMeter;
        DWORD      biClrUsed;
        DWORD      biClrImportant;
} BITMAPINFOHEADER,  *LPBITMAPINFOHEADER, *PBITMAPINFOHEADER;

typedef struct tagBITMAPV4HEADER {
        DWORD        bV4Size;
        LONG         bV4Width;
        LONG         bV4Height;
        WORD         bV4Planes;
        WORD         bV4BitCount;
        _biCompression        bV4V4Compression;
        DWORD        bV4SizeImage;
        LONG         bV4XPelsPerMeter;
        LONG         bV4YPelsPerMeter;
        DWORD        bV4ClrUsed;
        DWORD        bV4ClrImportant;
        DWORD        bV4RedMask;
        DWORD        bV4GreenMask;
        DWORD        bV4BlueMask;
        DWORD        bV4AlphaMask;
        DWORD        bV4CSType;
        CIEXYZTRIPLE bV4Endpoints;
        DWORD        bV4GammaRed;
        DWORD        bV4GammaGreen;
        DWORD        bV4GammaBlue;
} BITMAPV4HEADER,  *LPBITMAPV4HEADER, *PBITMAPV4HEADER;

typedef struct tagBITMAPV5HEADER {
        DWORD        bV5Size;
        LONG         bV5Width;
        LONG         bV5Height;
        WORD         bV5Planes;
        WORD         bV5BitCount;
        _biCompression        bV5Compression;
        DWORD        bV5SizeImage;
        LONG         bV5XPelsPerMeter;
        LONG         bV5YPelsPerMeter;
        DWORD        bV5ClrUsed;
        DWORD        bV5ClrImportant;
        DWORD        bV5RedMask;
        DWORD        bV5GreenMask;
        DWORD        bV5BlueMask;
        DWORD        bV5AlphaMask;
        DWORD        bV5CSType;
        CIEXYZTRIPLE bV5Endpoints;
        DWORD        bV5GammaRed;
        DWORD        bV5GammaGreen;
        DWORD        bV5GammaBlue;
        DWORD        bV5Intent;
        DWORD        bV5ProfileData;
        DWORD        bV5ProfileSize;
        DWORD        bV5Reserved;
} BITMAPV5HEADER,  *LPBITMAPV5HEADER, *PBITMAPV5HEADER;

// Values for bV5CSType
// #define PROFILE_LINKED          'LINK'
// #define PROFILE_EMBEDDED        'MBED'

typedef struct tagBITMAPINFO {
    BITMAPINFOHEADER    bmiHeader;
    RGBQUAD             bmiColors[1];
} BITMAPINFO,  *LPBITMAPINFO, *PBITMAPINFO;

typedef struct tagBITMAPCOREINFO {
    BITMAPCOREHEADER    bmciHeader;
    RGBTRIPLE           bmciColors[1];
} BITMAPCOREINFO,  *LPBITMAPCOREINFO, *PBITMAPCOREINFO;

typedef struct tagBITMAPFILEHEADER {
        WORD    bfType;
        DWORD   bfSize;
        WORD    bfReserved1;
        WORD    bfReserved2;
        DWORD   bfOffBits;
} BITMAPFILEHEADER,  *LPBITMAPFILEHEADER, *PBITMAPFILEHEADER;


typedef struct tagFONTSIGNATURE
{
    DWORD fsUsb[4];
    DWORD fsCsb[2];
} FONTSIGNATURE, *PFONTSIGNATURE, *LPFONTSIGNATURE;

typedef struct tagCHARSETINFO
{
    UINT ciCharset;
    UINT ciACP;
    FONTSIGNATURE fs;
} CHARSETINFO, *PCHARSETINFO,  *NPCHARSETINFO,  *LPCHARSETINFO;

typedef struct tagLOCALESIGNATURE
{
    DWORD lsUsb[4];
    DWORD lsCsbDefault[2];
    DWORD lsCsbSupported[2];
} LOCALESIGNATURE, *PLOCALESIGNATURE, *LPLOCALESIGNATURE;




/* Clipboard Metafile Picture Structure */
typedef struct tagHANDLETABLE
  {
    HGDIOBJ     objectHandle[1];
  } HANDLETABLE, *PHANDLETABLE,  *LPHANDLETABLE;

typedef struct tagMETARECORD
  {
    DWORD       rdSize;
    WORD        rdFunction;
    WORD        rdParm[1];
  } METARECORD, *PMETARECORD,  *LPMETARECORD;

typedef struct tagMETAHEADER
{
    WORD        mtType;
    WORD        mtHeaderSize;
    WORD        mtVersion;
    DWORD       mtSize;
    WORD        mtNoObjects;
    DWORD       mtMaxRecord;
    WORD        mtNoParameters;
} METAHEADER, *PMETAHEADER,   *LPMETAHEADER;


/* Enhanced Metafile structures */
typedef struct tagENHMETARECORD
{
    DWORD   iType;              // Record type EMR_XXX
    DWORD   nSize;              // Record size in bytes
    DWORD   dParm[1];           // Parameters
} ENHMETARECORD, *PENHMETARECORD, *LPENHMETARECORD;

typedef struct tagENHMETAHEADER
{
    DWORD   iType;              // Record type EMR_HEADER
    DWORD   nSize;              // Record size in bytes.  This may be greater
                                // than the sizeof(ENHMETAHEADER).
    RECTL   rclBounds;          // Inclusive-inclusive bounds in device units
    RECTL   rclFrame;           // Inclusive-inclusive Picture Frame of metafile in .01 mm units
    DWORD   dSignature;         // Signature.  Must be ENHMETA_SIGNATURE.
    DWORD   nVersion;           // Version number
    DWORD   nBytes;             // Size of the metafile in bytes
    DWORD   nRecords;           // Number of records in the metafile
    WORD    nHandles;           // Number of handles in the handle table
                                // Handle index zero is reserved.
    WORD    sReserved;          // Reserved.  Must be zero.
    DWORD   nDescription;       // Number of chars in the unicode description string
                                // This is 0 if there is no description string
    DWORD   offDescription;     // Offset to the metafile description record.
                                // This is 0 if there is no description string
    DWORD   nPalEntries;        // Number of entries in the metafile palette.
    SIZEL   szlDevice;          // Size of the reference device in pels
    SIZEL   szlMillimeters;     // Size of the reference device in millimeters
    DWORD   cbPixelFormat;      // Size of PIXELFORMATDESCRIPTOR information
                                // This is 0 if no pixel format is set
    DWORD   offPixelFormat;     // Offset to PIXELFORMATDESCRIPTOR
                                // This is 0 if no pixel format is set
    DWORD   bOpenGL;            // TRUE if OpenGL commands are present in
                                // the metafile, otherwise FALSE
    SIZEL   szlMicrometers;     // Size of the reference device in micrometers

} ENHMETAHEADER, *PENHMETAHEADER, *LPENHMETAHEADER;


//
// BCHAR definition for APPs
//

typedef struct tagTEXTMETRICA
{
    LONG        tmHeight;
    LONG        tmAscent;
    LONG        tmDescent;
    LONG        tmInternalLeading;
    LONG        tmExternalLeading;
    LONG        tmAveCharWidth;
    LONG        tmMaxCharWidth;
    LONG        tmWeight;
    LONG        tmOverhang;
    LONG        tmDigitizedAspectX;
    LONG        tmDigitizedAspectY;
    BYTE        tmFirstChar;
    BYTE        tmLastChar;
    BYTE        tmDefaultChar;
    BYTE        tmBreakChar;
    BYTE        tmItalic;
    BYTE        tmUnderlined;
    BYTE        tmStruckOut;
    _TMPF        tmPitchAndFamily;
    _CHARSETBYTE        tmCharSet;
} TEXTMETRICA, *PTEXTMETRICA,  *NPTEXTMETRICA,  *LPTEXTMETRICA;

typedef struct tagTEXTMETRICW
{
    LONG        tmHeight;
    LONG        tmAscent;
    LONG        tmDescent;
    LONG        tmInternalLeading;
    LONG        tmExternalLeading;
    LONG        tmAveCharWidth;
    LONG        tmMaxCharWidth;
    LONG        tmWeight;
    LONG        tmOverhang;
    LONG        tmDigitizedAspectX;
    LONG        tmDigitizedAspectY;
    WCHAR       tmFirstChar;
    WCHAR       tmLastChar;
    WCHAR       tmDefaultChar;
    WCHAR       tmBreakChar;
    BYTE        tmItalic;
    BYTE        tmUnderlined;
    BYTE        tmStruckOut;
    _TMPF        tmPitchAndFamily;
    _CHARSETBYTE        tmCharSet;
} TEXTMETRICW, *PTEXTMETRICW,  *NPTEXTMETRICW,  *LPTEXTMETRICW;

typedef struct tagNEWTEXTMETRICA
{
    LONG        tmHeight;
    LONG        tmAscent;
    LONG        tmDescent;
    LONG        tmInternalLeading;
    LONG        tmExternalLeading;
    LONG        tmAveCharWidth;
    LONG        tmMaxCharWidth;
    LONG        tmWeight;
    LONG        tmOverhang;
    LONG        tmDigitizedAspectX;
    LONG        tmDigitizedAspectY;
    BYTE        tmFirstChar;
    BYTE        tmLastChar;
    BYTE        tmDefaultChar;
    BYTE        tmBreakChar;
    BYTE        tmItalic;
    BYTE        tmUnderlined;
    BYTE        tmStruckOut;
    _TMPF        tmPitchAndFamily;
    _CHARSETBYTE        tmCharSet;
    DWORD   ntmFlags;
    UINT    ntmSizeEM;
    UINT    ntmCellHeight;
    UINT    ntmAvgWidth;
} NEWTEXTMETRICA, *PNEWTEXTMETRICA,  *NPNEWTEXTMETRICA,  *LPNEWTEXTMETRICA;
typedef struct tagNEWTEXTMETRICW
{
    LONG        tmHeight;
    LONG        tmAscent;
    LONG        tmDescent;
    LONG        tmInternalLeading;
    LONG        tmExternalLeading;
    LONG        tmAveCharWidth;
    LONG        tmMaxCharWidth;
    LONG        tmWeight;
    LONG        tmOverhang;
    LONG        tmDigitizedAspectX;
    LONG        tmDigitizedAspectY;
    WCHAR       tmFirstChar;
    WCHAR       tmLastChar;
    WCHAR       tmDefaultChar;
    WCHAR       tmBreakChar;
    BYTE        tmItalic;
    BYTE        tmUnderlined;
    BYTE        tmStruckOut;
    _TMPF        tmPitchAndFamily;
    _CHARSETBYTE        tmCharSet;
    DWORD   ntmFlags;
    UINT    ntmSizeEM;
    UINT    ntmCellHeight;
    UINT    ntmAvgWidth;
} NEWTEXTMETRICW, *PNEWTEXTMETRICW,  *NPNEWTEXTMETRICW,  *LPNEWTEXTMETRICW;

typedef struct tagNEWTEXTMETRICEXA
{
    NEWTEXTMETRICA  ntmTm;
    FONTSIGNATURE   ntmFontSig;
}NEWTEXTMETRICEXA;
typedef struct tagNEWTEXTMETRICEXW
{
    NEWTEXTMETRICW  ntmTm;
    FONTSIGNATURE   ntmFontSig;
}NEWTEXTMETRICEXW;

/* GDI Logical Objects: */

/* Pel Array */
typedef struct tagPELARRAY
  {
    LONG        paXCount;
    LONG        paYCount;
    LONG        paXExt;
    LONG        paYExt;
    BYTE        paRGBs;
  } PELARRAY, *PPELARRAY,  *NPPELARRAY,  *LPPELARRAY;

/* Logical Brush (or Pattern) */
typedef struct tagLOGBRUSH
  {
    _BrushStyles        lbStyle;
    COLORREF    lbColor;
    _HatchStyle        lbHatch;
  } LOGBRUSH, *PLOGBRUSH,  *NPLOGBRUSH,  *LPLOGBRUSH;

typedef struct tagLOGBRUSH32
  {
    _BrushStyles        lbStyle;
    COLORREF    lbColor;
    _HatchStyle       lbHatch;
  } LOGBRUSH32, *PLOGBRUSH32,  *NPLOGBRUSH32,  *LPLOGBRUSH32;

typedef LOGBRUSH            PATTERN;
typedef PATTERN             *PPATTERN;
typedef PATTERN         *NPPATTERN;
typedef PATTERN          *LPPATTERN;

/* Logical Pen */
typedef struct tagLOGPEN
  {
    _PS        lopnStyle;
    POINT       lopnWidth;
    COLORREF    lopnColor;
  } LOGPEN, *PLOGPEN,  *NPLOGPEN,  *LPLOGPEN;

typedef struct tagEXTLOGPEN {
    _PS       elpPenStyle;
    DWORD       elpWidth;
    _BrushStyles        elpBrushStyle;
    COLORREF    elpColor;
    _HatchStyle        elpHatch;
    DWORD       elpNumEntries;
    DWORD       elpStyleEntry[1];
} EXTLOGPEN, *PEXTLOGPEN,  *NPEXTLOGPEN,  *LPEXTLOGPEN;

/* Logical Palette */
typedef struct tagLOGPALETTE {
    WORD        palVersion;
    WORD        palNumEntries;
    PALETTEENTRY        palPalEntry[1];
} LOGPALETTE, *PLOGPALETTE,  *NPLOGPALETTE,  *LPLOGPALETTE;


/* Logical Font */
//#define LF_FACESIZE         32

typedef struct tagLOGFONTA
{
    LONG      lfHeight;
    LONG      lfWidth;
    LONG      lfEscapement;
    LONG      lfOrientation;
    _FW      lfWeight;
    BYTE      lfItalic;
    BYTE      lfUnderline;
    BYTE      lfStrikeOut;
    _CHARSETBYTE      lfCharSet;
    _OUTBYTE      lfOutPrecision;
    _CLIPBYTE      lfClipPrecision;
    _QUALITYBYTE      lfQuality;
    _FFBYTE      lfPitchAndFamily;
    CHAR      lfFaceName[/*LF_FACESIZE*/  32];
} LOGFONTA, *PLOGFONTA,  *NPLOGFONTA,  *LPLOGFONTA;
typedef struct tagLOGFONTW
{
    LONG      lfHeight;
    LONG      lfWidth;
    LONG      lfEscapement;
    LONG      lfOrientation;
    _FW      lfWeight;
    BYTE      lfItalic;
    BYTE      lfUnderline;
    BYTE      lfStrikeOut;
    _CHARSETBYTE      lfCharSet;
    _OUTBYTE      lfOutPrecision;
    _CLIPBYTE      lfClipPrecision;
    _QUALITYBYTE      lfQuality;
    _FFBYTE      lfPitchAndFamily;
    WCHAR     lfFaceName[/*LF_FACESIZE*/ 32];
} LOGFONTW, *PLOGFONTW,  *NPLOGFONTW,  *LPLOGFONTW;

//#define LF_FULLFACESIZE     64

/* Structure passed to FONTENUMPROC */
typedef struct tagENUMLOGFONTA
{
    LOGFONTA elfLogFont;
    BYTE     elfFullName[/*LF_FULLFACESIZE*/ 64];
    BYTE     elfStyle[/*LF_FACESIZE*/ 32];
} ENUMLOGFONTA, * LPENUMLOGFONTA;
/* Structure passed to FONTENUMPROC */
typedef struct tagENUMLOGFONTW
{
    LOGFONTW elfLogFont;
    WCHAR    elfFullName[/*LF_FULLFACESIZE*/ 64];
    WCHAR    elfStyle[/*LF_FACESIZE*/ 32];
} ENUMLOGFONTW, * LPENUMLOGFONTW;

typedef struct tagENUMLOGFONTEXA
{
    LOGFONTA    elfLogFont;
    BYTE        elfFullName[/*LF_FULLFACESIZE*/ 64];
    BYTE        elfStyle[/*LF_FACESIZE*/ 32];
    BYTE        elfScript[/*LF_FACESIZE*/ 32];
} ENUMLOGFONTEXA,  *LPENUMLOGFONTEXA;
typedef struct tagENUMLOGFONTEXW
{
    LOGFONTW    elfLogFont;
    WCHAR       elfFullName[/*LF_FULLFACESIZE*/ 64];
    WCHAR       elfStyle[/*LF_FACESIZE*/ 32];
    WCHAR       elfScript[/*LF_FACESIZE*/ 32];
} ENUMLOGFONTEXW,  *LPENUMLOGFONTEXW;

typedef struct tagPANOSE
{
    PanFamilyType    bFamilyType;
    PanSerifType    bSerifStyle;
    PanWeightType    bWeight;
    PanPropType    bProportion;
    PanConstrastType    bContrast;
    PanStrokeType    bStrokeVariation;
    PanArmsType    bArmStyle;
    PanLettType    bLetterform;
    PanMidlineType    bMidline;
    PanXHeightType    bXHeight;
} PANOSE, * LPPANOSE;

/* The extended logical font       */
/* An extension of the ENUMLOGFONT */

typedef struct tagEXTLOGFONTA {
    LOGFONTA    elfLogFont;
    BYTE        elfFullName[/*LF_FULLFACESIZE*/ 64];
    BYTE        elfStyle[/*LF_FACESIZE*/ 32];
    DWORD       elfVersion;     /* 0 for the first release of NT */
    DWORD       elfStyleSize;
    DWORD       elfMatch;
    DWORD       elfReserved;
    BYTE        elfVendorId[/*ELF_VENDOR_SIZE*/ 4];
    DWORD       elfCulture;     /* 0 for Latin                   */
    PANOSE      elfPanose;
} EXTLOGFONTA, *PEXTLOGFONTA,  *NPEXTLOGFONTA,  *LPEXTLOGFONTA;
typedef struct tagEXTLOGFONTW {
    LOGFONTW    elfLogFont;
    WCHAR       elfFullName[/*LF_FULLFACESIZE*/ 64];
    WCHAR       elfStyle[/*LF_FACESIZE*/ 32];
    DWORD       elfVersion;     /* 0 for the first release of NT */
    DWORD       elfStyleSize;
    DWORD       elfMatch;
    DWORD       elfReserved;
    BYTE        elfVendorId[/*ELF_VENDOR_SIZE*/ 4];
    DWORD       elfCulture;     /* 0 for Latin                   */
    PANOSE      elfPanose;
} EXTLOGFONTW, *PEXTLOGFONTW,  *NPEXTLOGFONTW,  *LPEXTLOGFONTW;

/* GetRegionData/ExtCreateRegion */

typedef struct _ABC {
    int     abcA;
    UINT    abcB;
    int     abcC;
} ABC, *PABC,  *NPABC,  *LPABC;

typedef struct _ABCFLOAT {
    FLOAT   abcfA;
    FLOAT   abcfB;
    FLOAT   abcfC;
} ABCFLOAT, *PABCFLOAT,  *NPABCFLOAT,  *LPABCFLOAT;


typedef struct _OUTLINETEXTMETRICA {
    UINT    otmSize;
    TEXTMETRICA otmTextMetrics;
    BYTE    otmFiller;
    PANOSE  otmPanoseNumber;
    _otmfsSelection    otmfsSelection;
    UINT    otmfsType;
     int    otmsCharSlopeRise;
     int    otmsCharSlopeRun;
     int    otmItalicAngle;
    UINT    otmEMSquare;
     int    otmAscent;
     int    otmDescent;
    UINT    otmLineGap;
    UINT    otmsCapEmHeight;
    UINT    otmsXHeight;
    RECT    otmrcFontBox;
     int    otmMacAscent;
     int    otmMacDescent;
    UINT    otmMacLineGap;
    UINT    otmusMinimumPPEM;
    POINT   otmptSubscriptSize;
    POINT   otmptSubscriptOffset;
    POINT   otmptSuperscriptSize;
    POINT   otmptSuperscriptOffset;
    UINT    otmsStrikeoutSize;
     int    otmsStrikeoutPosition;
     int    otmsUnderscoreSize;
     int    otmsUnderscorePosition;
    PSTR    otmpFamilyName;
    PSTR    otmpFaceName;
    PSTR    otmpStyleName;
    PSTR    otmpFullName;
} OUTLINETEXTMETRICA, *POUTLINETEXTMETRICA,  *NPOUTLINETEXTMETRICA,  *LPOUTLINETEXTMETRICA;
typedef struct _OUTLINETEXTMETRICW {
    UINT    otmSize;
    TEXTMETRICW otmTextMetrics;
    BYTE    otmFiller;
    PANOSE  otmPanoseNumber;
    _otmfsSelection    otmfsSelection;
    UINT    otmfsType;
     int    otmsCharSlopeRise;
     int    otmsCharSlopeRun;
     int    otmItalicAngle;
    UINT    otmEMSquare;
     int    otmAscent;
     int    otmDescent;
    UINT    otmLineGap;
    UINT    otmsCapEmHeight;
    UINT    otmsXHeight;
    RECT    otmrcFontBox;
     int    otmMacAscent;
     int    otmMacDescent;
    UINT    otmMacLineGap;
    UINT    otmusMinimumPPEM;
    POINT   otmptSubscriptSize;
    POINT   otmptSubscriptOffset;
    POINT   otmptSuperscriptSize;
    POINT   otmptSuperscriptOffset;
    UINT    otmsStrikeoutSize;
     int    otmsStrikeoutPosition;
     int    otmsUnderscoreSize;
     int    otmsUnderscorePosition;
    PSTR    otmpFamilyName;
    PSTR    otmpFaceName;
    PSTR    otmpStyleName;
    PSTR    otmpFullName;
} OUTLINETEXTMETRICW, *POUTLINETEXTMETRICW,  *NPOUTLINETEXTMETRICW,  *LPOUTLINETEXTMETRICW;



typedef struct tagPOLYTEXTA
{
    int       x;
    int       y;
    UINT      n;
    LPCSTR    lpstr;
    _ETO      uiFlags;
    RECT      rcl;
    int      *pdx;
} POLYTEXTA, *PPOLYTEXTA,  *NPPOLYTEXTA,  *LPPOLYTEXTA;
typedef struct tagPOLYTEXTW
{
    int       x;
    int       y;
    UINT      n;
    LPCWSTR   lpstr;
    _ETO      uiFlags;
    RECT      rcl;
    int      *pdx;
} POLYTEXTW, *PPOLYTEXTW,  *NPPOLYTEXTW,  *LPPOLYTEXTW;

typedef struct _FIXED {
    WORD    fract;
    WORD   _value;
} FIXED;


typedef struct _MAT2 {
     FIXED  eM11;
     FIXED  eM12;
     FIXED  eM21;
     FIXED  eM22;
} MAT2,  *LPMAT2;



typedef struct _GLYPHMETRICS {
    UINT    gmBlackBoxX;
    UINT    gmBlackBoxY;
    POINT   gmptGlyphOrigin;
    short   gmCellIncX;
    short   gmCellIncY;
} GLYPHMETRICS,  *LPGLYPHMETRICS;

typedef struct tagPOINTFX
{
    FIXED x;
    FIXED y;
} POINTFX, * LPPOINTFX;

typedef struct tagTTPOLYCURVE
{
    _TT_PRIM    wType;
    WORD    cpfx;
    POINTFX apfx[1];
} TTPOLYCURVE, * LPTTPOLYCURVE;

typedef struct tagTTPOLYGONHEADER
{
    DWORD   cb;
    _TT_POLYGON   dwType;
    POINTFX pfxStart;
} TTPOLYGONHEADER, * LPTTPOLYGONHEADER;


typedef struct tagGCP_RESULTSA
    {
    DWORD   lStructSize;
    LPSTR     lpOutString;
    UINT  *lpOrder;
    int   *lpDx;
    int   *lpCaretPos;
    _GCPCLASS *   lpClass;
    LPWSTR  lpGlyphs;
    UINT    nGlyphs;
    int     nMaxFit;
    } GCP_RESULTSA, * LPGCP_RESULTSA;
typedef struct tagGCP_RESULTSW
    {
    DWORD   lStructSize;
    LPWSTR    lpOutString;
    UINT  *lpOrder;
    int   *lpDx;
    int   *lpCaretPos;
    _GCPCLASS *   lpClass;
    LPWSTR  lpGlyphs;
    UINT    nGlyphs;
    int     nMaxFit;
    } GCP_RESULTSW, * LPGCP_RESULTSW;

typedef struct _RASTERIZER_STATUS {
    short   nSize;
    _RASTERIZER_STATUS_Flag   wFlags;
    short   nLanguageID;
} RASTERIZER_STATUS,  *LPRASTERIZER_STATUS;

/* Pixel format descriptor */
typedef struct tagPIXELFORMATDESCRIPTOR
{
    WORD  nSize;
    WORD  nVersion;
    _PIXELFORMATDESCRIPTOR dwFlags;
    _PFD  iPixelType;
    BYTE  cColorBits;
    BYTE  cRedBits;
    BYTE  cRedShift;
    BYTE  cGreenBits;
    BYTE  cGreenShift;
    BYTE  cBlueBits;
    BYTE  cBlueShift;
    BYTE  cAlphaBits;
    BYTE  cAlphaShift;
    BYTE  cAccumBits;
    BYTE  cAccumRedBits;
    BYTE  cAccumGreenBits;
    BYTE  cAccumBlueBits;
    BYTE  cAccumAlphaBits;
    BYTE  cDepthBits;
    BYTE  cStencilBits;
    BYTE  cAuxBuffers;
    _PFD_LAYER  iLayerType;
    BYTE  bReserved;
    DWORD dwLayerMask;
    DWORD dwVisibleMask;
    DWORD dwDamageMask;
} PIXELFORMATDESCRIPTOR, *PPIXELFORMATDESCRIPTOR,  *LPPIXELFORMATDESCRIPTOR;

typedef OLDFONTENUMPROCA    FONTENUMPROCA;
typedef OLDFONTENUMPROCW    FONTENUMPROCW;


/* define types of pointers to ExtDeviceMode() and DeviceCapabilities()
 * functions for Win 3.1 compatibility
 */
typedef LPVOID OLDFONTENUMPROCA;
typedef LPVOID OLDFONTENUMPROCW;
typedef LPVOID GOBJENUMPROC;
typedef LPVOID LINEDDAPROC;

typedef LPVOID LPFNDEVMODEA;
typedef LPVOID LPFNDEVMODEW;

typedef LPVOID LPFNDEVCAPSA;
typedef LPVOID LPFNDEVCAPSW;

typedef struct tagWCRANGE
{
    WCHAR  wcLow;
    USHORT cGlyphs;
} WCRANGE, *PWCRANGE, *LPWCRANGE;


typedef struct tagGLYPHSET
{
    DWORD    cbThis;
    _GS_8BIT_INDICES    flAccel;
    DWORD    cGlyphsSupported;
    DWORD    cRanges;
    WCRANGE  ranges[1];
} GLYPHSET, *PGLYPHSET,  *LPGLYPHSET;

typedef struct tagDESIGNVECTOR
{
    DWORD  dvReserved;
    DWORD  dvNumAxes;
    LONG   dvValues[/*MM_MAX_NUMAXES*/16];
} DESIGNVECTOR, *PDESIGNVECTOR,  *LPDESIGNVECTOR;
// The actual size of the DESIGNVECTOR and ENUMLOGFONTEXDV structures
// is determined by dvNumAxes,
// MM_MAX_NUMAXES only detemines the maximal size allowed

//#define MM_MAX_AXES_NAMELEN 16

typedef struct tagAXISINFOA
{
    LONG   axMinValue;
    LONG   axMaxValue;
    BYTE   axAxisName[/*MM_MAX_NUMAXES*/ 16];
} AXISINFOA, *PAXISINFOA,  *LPAXISINFOA;
typedef struct tagAXISINFOW
{
    LONG   axMinValue;
    LONG   axMaxValue;
    WCHAR  axAxisName[/*MM_MAX_NUMAXES*/ 16];
} AXISINFOW, *PAXISINFOW,  *LPAXISINFOW;

typedef struct tagAXESLISTA
{
    DWORD     axlReserved;
    DWORD     axlNumAxes;
    AXISINFOA axlAxisInfo[/*MM_MAX_NUMAXES*/ 16];
} AXESLISTA, *PAXESLISTA,  *LPAXESLISTA;
typedef struct tagAXESLISTW
{
    DWORD     axlReserved;
    DWORD     axlNumAxes;
    AXISINFOW axlAxisInfo[/*MM_MAX_NUMAXES*/ 16];
} AXESLISTW, *PAXESLISTW,  *LPAXESLISTW;

// The actual size of the AXESLIST and ENUMTEXTMETRIC structure is
// determined by axlNumAxes,
// MM_MAX_NUMAXES only detemines the maximal size allowed

typedef struct tagENUMLOGFONTEXDVA
{
    ENUMLOGFONTEXA elfEnumLogfontEx;
    DESIGNVECTOR   elfDesignVector;
} ENUMLOGFONTEXDVA, *PENUMLOGFONTEXDVA,  *LPENUMLOGFONTEXDVA;
typedef struct tagENUMLOGFONTEXDVW
{
    ENUMLOGFONTEXW elfEnumLogfontEx;
    DESIGNVECTOR   elfDesignVector;
} ENUMLOGFONTEXDVW, *PENUMLOGFONTEXDVW,  *LPENUMLOGFONTEXDVW;


typedef struct tagENUMTEXTMETRICA
{
    NEWTEXTMETRICEXA etmNewTextMetricEx;
    AXESLISTA        etmAxesList;
} ENUMTEXTMETRICA, *PENUMTEXTMETRICA,  *LPENUMTEXTMETRICA;
typedef struct tagENUMTEXTMETRICW
{
    NEWTEXTMETRICEXW etmNewTextMetricEx;
    AXESLISTW        etmAxesList;
} ENUMTEXTMETRICW, *PENUMTEXTMETRICW,  *LPENUMTEXTMETRICW;

//
// image blt
//

typedef USHORT COLOR16;

typedef struct _TRIVERTEX
{
    LONG    x;
    LONG    y;
    COLOR16 Red;
    COLOR16 Green;
    COLOR16 Blue;
    COLOR16 Alpha;
}TRIVERTEX,*PTRIVERTEX,*LPTRIVERTEX;

typedef struct _GRADIENT_TRIANGLE
{
    ULONG Vertex1;
    ULONG Vertex2;
    ULONG Vertex3;
} GRADIENT_TRIANGLE,*PGRADIENT_TRIANGLE,*LPGRADIENT_TRIANGLE;

typedef struct _GRADIENT_RECT
{
    ULONG UpperLeft;
    ULONG LowerRight;
}GRADIENT_RECT,*PGRADIENT_RECT,*LPGRADIENT_RECT;

typedef struct _BLENDFUNCTION
{
    _AC_SRC_OVER   BlendOp;
    BYTE   BlendFlags;
    BYTE   SourceConstantAlpha;
    _AC_SRC_ALPHA   AlphaFormat;
}BLENDFUNCTION,*PBLENDFUNCTION;

typedef LPVOID MFENUMPROC;
typedef LPVOID ENHMFENUMPROC;


/* new GDI */

typedef struct tagDIBSECTION {
    BITMAP              dsBm;
    BITMAPINFOHEADER    dsBmih;
    DWORD               dsBitfields[3];
    HANDLE              dshSection;
    DWORD               dsOffset;
} DIBSECTION,  *LPDIBSECTION, *PDIBSECTION;

typedef struct tagKERNINGPAIR {
   WORD wFirst;
   WORD wSecond;
   int  iKernAmount;
} KERNINGPAIR, *LPKERNINGPAIR;

typedef LPVOID ICMENUMPROCA;
typedef LPVOID ICMENUMPROCW;

// Base record type for the enhanced metafile.

typedef struct tagEMR
{
    _EMR   iType;              // Enhanced metafile record type
    DWORD   nSize;              // Length of the record in bytes.
                                // This must be a multiple of 4.
} EMR, *PEMR;

// Base text record type for the enhanced metafile.

typedef struct tagEMRTEXT
{
    POINTL  ptlReference;
    DWORD   nChars;
    DWORD   offString;          // Offset to the string
    _ETO   fOptions;
    RECTL   rcl;
    DWORD   offDx;              // Offset to the inter-character spacing array.
                                // This is always given.
} EMRTEXT, *PEMRTEXT;

// Record structures for the enhanced metafile.

typedef struct tagABORTPATH
{
    EMR     emr;
} EMRABORTPATH,      *PEMRABORTPATH,
  EMRBEGINPATH,      *PEMRBEGINPATH,
  EMRENDPATH,        *PEMRENDPATH,
  EMRCLOSEFIGURE,    *PEMRCLOSEFIGURE,
  EMRFLATTENPATH,    *PEMRFLATTENPATH,
  EMRWIDENPATH,      *PEMRWIDENPATH,
  EMRSETMETARGN,     *PEMRSETMETARGN,
  EMRSAVEDC,         *PEMRSAVEDC,
  EMRREALIZEPALETTE, *PEMRREALIZEPALETTE;

typedef struct tagEMRSELECTCLIPPATH
{
    EMR     emr;
    DWORD   iMode;
} EMRSELECTCLIPPATH,    *PEMRSELECTCLIPPATH,
  EMRSETBKMODE,         *PEMRSETBKMODE,
  EMRSETMAPMODE,        *PEMRSETMAPMODE,
  EMRSETLAYOUT,         *PEMRSETLAYOUT,
  EMRSETPOLYFILLMODE,   *PEMRSETPOLYFILLMODE,
  EMRSETROP2,           *PEMRSETROP2,
  EMRSETSTRETCHBLTMODE, *PEMRSETSTRETCHBLTMODE,
  EMRSETICMMODE,        *PEMRSETICMMODE,
  EMRSETTEXTALIGN,      *PEMRSETTEXTALIGN;

typedef struct tagEMRSETMITERLIMIT
{
    EMR     emr;
    FLOAT   eMiterLimit;
} EMRSETMITERLIMIT, *PEMRSETMITERLIMIT;

typedef struct tagEMRRESTOREDC
{
    EMR     emr;
    LONG    iRelative;          // Specifies a relative instance
} EMRRESTOREDC, *PEMRRESTOREDC;

typedef struct tagEMRSETARCDIRECTION
{
    EMR     emr;
    DWORD   iArcDirection;      // Specifies the arc direction in the
                                // advanced graphics mode.
} EMRSETARCDIRECTION, *PEMRSETARCDIRECTION;

typedef struct tagEMRSETMAPPERFLAGS
{
    EMR     emr;
    DWORD   dwFlags;
} EMRSETMAPPERFLAGS, *PEMRSETMAPPERFLAGS;

typedef struct tagEMRSETTEXTCOLOR
{
    EMR     emr;
    COLORREF crColor;
} EMRSETBKCOLOR,   *PEMRSETBKCOLOR,
  EMRSETTEXTCOLOR, *PEMRSETTEXTCOLOR;

typedef struct tagEMRSELECTOBJECT
{
    EMR     emr;
    DWORD   ihObject;           // Object handle index
} EMRSELECTOBJECT, *PEMRSELECTOBJECT,
  EMRDELETEOBJECT, *PEMRDELETEOBJECT;

typedef struct tagEMRSELECTPALETTE
{
    EMR     emr;
    DWORD   ihPal;              // Palette handle index, background mode only
} EMRSELECTPALETTE, *PEMRSELECTPALETTE;

typedef struct tagEMRRESIZEPALETTE
{
    EMR     emr;
    DWORD   ihPal;              // Palette handle index
    DWORD   cEntries;
} EMRRESIZEPALETTE, *PEMRRESIZEPALETTE;

typedef struct tagEMRSETPALETTEENTRIES
{
    EMR     emr;
    DWORD   ihPal;              // Palette handle index
    DWORD   iStart;
    DWORD   cEntries;
    PALETTEENTRY aPalEntries[1];// The peFlags fields do not contain any flags
} EMRSETPALETTEENTRIES, *PEMRSETPALETTEENTRIES;

typedef struct tagEMRSETCOLORADJUSTMENT
{
    EMR     emr;
    COLORADJUSTMENT ColorAdjustment;
} EMRSETCOLORADJUSTMENT, *PEMRSETCOLORADJUSTMENT;

typedef struct tagEMRGDICOMMENT
{
    EMR     emr;
    DWORD   cbData;             // Size of data in bytes
    BYTE    Data[1];
} EMRGDICOMMENT, *PEMRGDICOMMENT;

typedef struct tagEMREOF
{
    EMR     emr;
    DWORD   nPalEntries;        // Number of palette entries
    DWORD   offPalEntries;      // Offset to the palette entries
    DWORD   nSizeLast;          // Same as nSize and must be the last DWORD
                                // of the record.  The palette entries,
                                // if exist, precede this field.
} EMREOF, *PEMREOF;

typedef struct tagEMRLINETO
{
    EMR     emr;
    POINTL  ptl;
} EMRLINETO,   *PEMRLINETO,
  EMRMOVETOEX, *PEMRMOVETOEX;

typedef struct tagEMROFFSETCLIPRGN
{
    EMR     emr;
    POINTL  ptlOffset;
} EMROFFSETCLIPRGN, *PEMROFFSETCLIPRGN;

typedef struct tagEMRFILLPATH
{
    EMR     emr;
    RECTL   rclBounds;          // Inclusive-inclusive bounds in device units
} EMRFILLPATH,          *PEMRFILLPATH,
  EMRSTROKEANDFILLPATH, *PEMRSTROKEANDFILLPATH,
  EMRSTROKEPATH,        *PEMRSTROKEPATH;

typedef struct tagEMREXCLUDECLIPRECT
{
    EMR     emr;
    RECTL   rclClip;
} EMREXCLUDECLIPRECT,   *PEMREXCLUDECLIPRECT,
  EMRINTERSECTCLIPRECT, *PEMRINTERSECTCLIPRECT;

typedef struct tagEMRSETVIEWPORTORGEX
{
    EMR     emr;
    POINTL  ptlOrigin;
} EMRSETVIEWPORTORGEX, *PEMRSETVIEWPORTORGEX,
  EMRSETWINDOWORGEX,   *PEMRSETWINDOWORGEX,
  EMRSETBRUSHORGEX,    *PEMRSETBRUSHORGEX;

typedef struct tagEMRSETVIEWPORTEXTEX
{
    EMR     emr;
    SIZEL   szlExtent;
} EMRSETVIEWPORTEXTEX, *PEMRSETVIEWPORTEXTEX,
  EMRSETWINDOWEXTEX,   *PEMRSETWINDOWEXTEX;

typedef struct tagEMRSCALEVIEWPORTEXTEX
{
    EMR     emr;
    LONG    xNum;
    LONG    xDenom;
    LONG    yNum;
    LONG    yDenom;
} EMRSCALEVIEWPORTEXTEX, *PEMRSCALEVIEWPORTEXTEX,
  EMRSCALEWINDOWEXTEX,   *PEMRSCALEWINDOWEXTEX;

typedef struct tagEMRSETWORLDTRANSFORM
{
    EMR     emr;
    XFORM   xform;
} EMRSETWORLDTRANSFORM, *PEMRSETWORLDTRANSFORM;

typedef struct tagEMRMODIFYWORLDTRANSFORM
{
    EMR     emr;
    XFORM   xform;
    _MWT   iMode;
} EMRMODIFYWORLDTRANSFORM, *PEMRMODIFYWORLDTRANSFORM;

typedef struct tagEMRSETPIXELV
{
    EMR     emr;
    POINTL  ptlPixel;
    COLORREF crColor;
} EMRSETPIXELV, *PEMRSETPIXELV;

typedef struct tagEMREXTFLOODFILL
{
    EMR     emr;
    POINTL  ptlStart;
    COLORREF crColor;
    _FLOODFILL   iMode;
} EMREXTFLOODFILL, *PEMREXTFLOODFILL;

typedef struct tagEMRELLIPSE
{
    EMR     emr;
    RECTL   rclBox;             // Inclusive-inclusive bounding rectangle
} EMRELLIPSE,  *PEMRELLIPSE,
  EMRRECTANGLE, *PEMRRECTANGLE;

typedef struct tagEMRROUNDRECT
{
    EMR     emr;
    RECTL   rclBox;             // Inclusive-inclusive bounding rectangle
    SIZEL   szlCorner;
} EMRROUNDRECT, *PEMRROUNDRECT;

typedef struct tagEMRARC
{
    EMR     emr;
    RECTL   rclBox;             // Inclusive-inclusive bounding rectangle
    POINTL  ptlStart;
    POINTL  ptlEnd;
} EMRARC,   *PEMRARC,
  EMRARCTO, *PEMRARCTO,
  EMRCHORD, *PEMRCHORD,
  EMRPIE,   *PEMRPIE;

typedef struct tagEMRANGLEARC
{
    EMR     emr;
    POINTL  ptlCenter;
    DWORD   nRadius;
    FLOAT   eStartAngle;
    FLOAT   eSweepAngle;
} EMRANGLEARC, *PEMRANGLEARC;

typedef struct tagEMRPOLYLINE
{
    EMR     emr;
    RECTL   rclBounds;          // Inclusive-inclusive bounds in device units
    DWORD   cptl;
    POINTL  aptl[1];
} EMRPOLYLINE,     *PEMRPOLYLINE,
  EMRPOLYBEZIER,   *PEMRPOLYBEZIER,
  EMRPOLYGON,      *PEMRPOLYGON,
  EMRPOLYBEZIERTO, *PEMRPOLYBEZIERTO,
  EMRPOLYLINETO,   *PEMRPOLYLINETO;

typedef struct tagEMRPOLYLINE16
{
    EMR     emr;
    RECTL   rclBounds;          // Inclusive-inclusive bounds in device units
    DWORD   cpts;
    POINTS  apts[1];
} EMRPOLYLINE16,     *PEMRPOLYLINE16,
  EMRPOLYBEZIER16,   *PEMRPOLYBEZIER16,
  EMRPOLYGON16,      *PEMRPOLYGON16,
  EMRPOLYBEZIERTO16, *PEMRPOLYBEZIERTO16,
  EMRPOLYLINETO16,   *PEMRPOLYLINETO16;

typedef struct tagEMRPOLYDRAW
{
    EMR     emr;
    RECTL   rclBounds;          // Inclusive-inclusive bounds in device units
    DWORD   cptl;               // Number of points
    POINTL  aptl[1];            // Array of points
    BYTE    abTypes[1];         // Array of point types
} EMRPOLYDRAW, *PEMRPOLYDRAW;

typedef struct tagEMRPOLYDRAW16
{
    EMR     emr;
    RECTL   rclBounds;          // Inclusive-inclusive bounds in device units
    DWORD   cpts;               // Number of points
    POINTS  apts[1];            // Array of points
    BYTE    abTypes[1];         // Array of point types
} EMRPOLYDRAW16, *PEMRPOLYDRAW16;

typedef struct tagEMRPOLYPOLYLINE
{
    EMR     emr;
    RECTL   rclBounds;          // Inclusive-inclusive bounds in device units
    DWORD   nPolys;             // Number of polys
    DWORD   cptl;               // Total number of points in all polys
    DWORD   aPolyCounts[1];     // Array of point counts for each poly
    POINTL  aptl[1];            // Array of points
} EMRPOLYPOLYLINE, *PEMRPOLYPOLYLINE,
  EMRPOLYPOLYGON,  *PEMRPOLYPOLYGON;

typedef struct tagEMRPOLYPOLYLINE16
{
    EMR     emr;
    RECTL   rclBounds;          // Inclusive-inclusive bounds in device units
    DWORD   nPolys;             // Number of polys
    DWORD   cpts;               // Total number of points in all polys
    DWORD   aPolyCounts[1];     // Array of point counts for each poly
    POINTS  apts[1];            // Array of points
} EMRPOLYPOLYLINE16, *PEMRPOLYPOLYLINE16,
  EMRPOLYPOLYGON16,  *PEMRPOLYPOLYGON16;

typedef struct tagEMRINVERTRGN
{
    EMR     emr;
    RECTL   rclBounds;          // Inclusive-inclusive bounds in device units
    DWORD   cbRgnData;          // Size of region data in bytes
    BYTE    RgnData[1];
} EMRINVERTRGN, *PEMRINVERTRGN,
  EMRPAINTRGN,  *PEMRPAINTRGN;

typedef struct tagEMRFILLRGN
{
    EMR     emr;
    RECTL   rclBounds;          // Inclusive-inclusive bounds in device units
    DWORD   cbRgnData;          // Size of region data in bytes
    DWORD   ihBrush;            // Brush handle index
    BYTE    RgnData[1];
} EMRFILLRGN, *PEMRFILLRGN;

typedef struct tagEMRFRAMERGN
{
    EMR     emr;
    RECTL   rclBounds;          // Inclusive-inclusive bounds in device units
    DWORD   cbRgnData;          // Size of region data in bytes
    DWORD   ihBrush;            // Brush handle index
    SIZEL   szlStroke;
    BYTE    RgnData[1];
} EMRFRAMERGN, *PEMRFRAMERGN;


typedef struct tagEMREXTSELECTCLIPRGN
{
    EMR     emr;
    DWORD   cbRgnData;          // Size of region data in bytes
    DWORD   iMode;
    BYTE    RgnData[1];
} EMREXTSELECTCLIPRGN, *PEMREXTSELECTCLIPRGN;

typedef struct tagEMREXTTEXTOUTA
{
    EMR     emr;
    RECTL   rclBounds;          // Inclusive-inclusive bounds in device units
    _GM   iGraphicsMode;      // Current graphics mode
    FLOAT   exScale;            // X and Y scales from Page units to .01mm units
    FLOAT   eyScale;            //   if graphics mode is GM_COMPATIBLE.
    EMRTEXT emrtext;            // This is followed by the string and spacing
                                // array
} EMREXTTEXTOUTA, *PEMREXTTEXTOUTA,
  EMREXTTEXTOUTW, *PEMREXTTEXTOUTW;

typedef struct tagEMRPOLYTEXTOUTA
{
    EMR     emr;
    RECTL   rclBounds;          // Inclusive-inclusive bounds in device units
    _GM   iGraphicsMode;      // Current graphics mode
    FLOAT   exScale;            // X and Y scales from Page units to .01mm units
    FLOAT   eyScale;            //   if graphics mode is GM_COMPATIBLE.
    LONG    cStrings;
    EMRTEXT aemrtext[1];        // Array of EMRTEXT structures.  This is
                                // followed by the strings and spacing arrays.
} EMRPOLYTEXTOUTA, *PEMRPOLYTEXTOUTA,
  EMRPOLYTEXTOUTW, *PEMRPOLYTEXTOUTW;

typedef struct tagEMRBITBLT
{
    EMR     emr;
    RECTL   rclBounds;          // Inclusive-inclusive bounds in device units
    LONG    xDest;
    LONG    yDest;
    LONG    cxDest;
    LONG    cyDest;
    _TernaryDrawMode   dwRop;
    LONG    xSrc;
    LONG    ySrc;
    XFORM   xformSrc;           // Source DC transform
    COLORREF crBkColorSrc;      // Source DC BkColor in RGB
    _DIB_Color   iUsageSrc;          // Source bitmap info color table usage
                                // (DIB_RGB_COLORS)
    DWORD   offBmiSrc;          // Offset to the source BITMAPINFO structure
    DWORD   cbBmiSrc;           // Size of the source BITMAPINFO structure
    DWORD   offBitsSrc;         // Offset to the source bitmap bits
    DWORD   cbBitsSrc;          // Size of the source bitmap bits
} EMRBITBLT, *PEMRBITBLT;

typedef struct tagEMRSTRETCHBLT
{
    EMR     emr;
    RECTL   rclBounds;          // Inclusive-inclusive bounds in device units
    LONG    xDest;
    LONG    yDest;
    LONG    cxDest;
    LONG    cyDest;
    _TernaryDrawMode   dwRop;
    LONG    xSrc;
    LONG    ySrc;
    XFORM   xformSrc;           // Source DC transform
    COLORREF crBkColorSrc;      // Source DC BkColor in RGB
    _DIB_Color   iUsageSrc;          // Source bitmap info color table usage
                                // (DIB_RGB_COLORS)
    DWORD   offBmiSrc;          // Offset to the source BITMAPINFO structure
    DWORD   cbBmiSrc;           // Size of the source BITMAPINFO structure
    DWORD   offBitsSrc;         // Offset to the source bitmap bits
    DWORD   cbBitsSrc;          // Size of the source bitmap bits
    LONG    cxSrc;
    LONG    cySrc;
} EMRSTRETCHBLT, *PEMRSTRETCHBLT;

typedef struct tagEMRMASKBLT
{
    EMR     emr;
    RECTL   rclBounds;          // Inclusive-inclusive bounds in device units
    LONG    xDest;
    LONG    yDest;
    LONG    cxDest;
    LONG    cyDest;
    _TernaryDrawMode   dwRop;
    LONG    xSrc;
    LONG    ySrc;
    XFORM   xformSrc;           // Source DC transform
    COLORREF crBkColorSrc;      // Source DC BkColor in RGB
    _DIB_Color   iUsageSrc;          // Source bitmap info color table usage
                                // (DIB_RGB_COLORS)
    DWORD   offBmiSrc;          // Offset to the source BITMAPINFO structure
    DWORD   cbBmiSrc;           // Size of the source BITMAPINFO structure
    DWORD   offBitsSrc;         // Offset to the source bitmap bits
    DWORD   cbBitsSrc;          // Size of the source bitmap bits
    LONG    xMask;
    LONG    yMask;
    DWORD   iUsageMask;         // Mask bitmap info color table usage
    DWORD   offBmiMask;         // Offset to the mask BITMAPINFO structure if any
    DWORD   cbBmiMask;          // Size of the mask BITMAPINFO structure if any
    DWORD   offBitsMask;        // Offset to the mask bitmap bits if any
    DWORD   cbBitsMask;         // Size of the mask bitmap bits if any
} EMRMASKBLT, *PEMRMASKBLT;

typedef struct tagEMRPLGBLT
{
    EMR     emr;
    RECTL   rclBounds;          // Inclusive-inclusive bounds in device units
    POINTL  aptlDest[3];
    LONG    xSrc;
    LONG    ySrc;
    LONG    cxSrc;
    LONG    cySrc;
    XFORM   xformSrc;           // Source DC transform
    COLORREF crBkColorSrc;      // Source DC BkColor in RGB
    _DIB_Color   iUsageSrc;          // Source bitmap info color table usage
                                // (DIB_RGB_COLORS)
    DWORD   offBmiSrc;          // Offset to the source BITMAPINFO structure
    DWORD   cbBmiSrc;           // Size of the source BITMAPINFO structure
    DWORD   offBitsSrc;         // Offset to the source bitmap bits
    DWORD   cbBitsSrc;          // Size of the source bitmap bits
    LONG    xMask;
    LONG    yMask;
    DWORD   iUsageMask;         // Mask bitmap info color table usage
    DWORD   offBmiMask;         // Offset to the mask BITMAPINFO structure if any
    DWORD   cbBmiMask;          // Size of the mask BITMAPINFO structure if any
    DWORD   offBitsMask;        // Offset to the mask bitmap bits if any
    DWORD   cbBitsMask;         // Size of the mask bitmap bits if any
} EMRPLGBLT, *PEMRPLGBLT;

typedef struct tagEMRSETDIBITSTODEVICE
{
    EMR     emr;
    RECTL   rclBounds;          // Inclusive-inclusive bounds in device units
    LONG    xDest;
    LONG    yDest;
    LONG    xSrc;
    LONG    ySrc;
    LONG    cxSrc;
    LONG    cySrc;
    DWORD   offBmiSrc;          // Offset to the source BITMAPINFO structure
    DWORD   cbBmiSrc;           // Size of the source BITMAPINFO structure
    DWORD   offBitsSrc;         // Offset to the source bitmap bits
    DWORD   cbBitsSrc;          // Size of the source bitmap bits
    _DIB_Color   iUsageSrc;          // Source bitmap info color table usage
    DWORD   iStartScan;
    DWORD   cScans;
} EMRSETDIBITSTODEVICE, *PEMRSETDIBITSTODEVICE;

typedef struct tagEMRSTRETCHDIBITS
{
    EMR     emr;
    RECTL   rclBounds;          // Inclusive-inclusive bounds in device units
    LONG    xDest;
    LONG    yDest;
    LONG    xSrc;
    LONG    ySrc;
    LONG    cxSrc;
    LONG    cySrc;
    DWORD   offBmiSrc;          // Offset to the source BITMAPINFO structure
    DWORD   cbBmiSrc;           // Size of the source BITMAPINFO structure
    DWORD   offBitsSrc;         // Offset to the source bitmap bits
    DWORD   cbBitsSrc;          // Size of the source bitmap bits
    _DIB_Color   iUsageSrc;          // Source bitmap info color table usage
    _TernaryDrawMode   dwRop;
    LONG    cxDest;
    LONG    cyDest;
} EMRSTRETCHDIBITS, *PEMRSTRETCHDIBITS;

typedef struct tagEMREXTCREATEFONTINDIRECTW
{
    EMR     emr;
    DWORD   ihFont;             // Font handle index
    EXTLOGFONTW elfw;
} EMREXTCREATEFONTINDIRECTW, *PEMREXTCREATEFONTINDIRECTW;


typedef struct tagEMRCREATEPALETTE
{
    EMR     emr;
    DWORD   ihPal;              // Palette handle index
    LOGPALETTE lgpl;            // The peFlags fields in the palette entries
                                // do not contain any flags
} EMRCREATEPALETTE, *PEMRCREATEPALETTE;

typedef struct tagEMRCREATEPEN
{
    EMR     emr;
    DWORD   ihPen;              // Pen handle index
    LOGPEN  lopn;
} EMRCREATEPEN, *PEMRCREATEPEN;

typedef struct tagEMREXTCREATEPEN
{
    EMR     emr;
    DWORD   ihPen;              // Pen handle index
    DWORD   offBmi;             // Offset to the BITMAPINFO structure if any
    DWORD   cbBmi;              // Size of the BITMAPINFO structure if any
                                // The bitmap info is followed by the bitmap
                                // bits to form a packed DIB.
    DWORD   offBits;            // Offset to the brush bitmap bits if any
    DWORD   cbBits;             // Size of the brush bitmap bits if any
    EXTLOGPEN elp;              // The extended pen with the style array.
} EMREXTCREATEPEN, *PEMREXTCREATEPEN;

typedef struct tagEMRCREATEBRUSHINDIRECT
{
    EMR     emr;
    DWORD   ihBrush;            // Brush handle index
    LOGBRUSH32 lb;                // The style must be BS_SOLID, BS_HOLLOW,
                                // BS_NULL or BS_HATCHED.
} EMRCREATEBRUSHINDIRECT, *PEMRCREATEBRUSHINDIRECT;

typedef struct tagEMRCREATEMONOBRUSH
{
    EMR     emr;
    DWORD   ihBrush;            // Brush handle index
    DWORD   iUsage;             // Bitmap info color table usage
    DWORD   offBmi;             // Offset to the BITMAPINFO structure
    DWORD   cbBmi;              // Size of the BITMAPINFO structure
    DWORD   offBits;            // Offset to the bitmap bits
    DWORD   cbBits;             // Size of the bitmap bits
} EMRCREATEMONOBRUSH, *PEMRCREATEMONOBRUSH;

typedef struct tagEMRCREATEDIBPATTERNBRUSHPT
{
    EMR     emr;
    DWORD   ihBrush;            // Brush handle index
    DWORD   iUsage;             // Bitmap info color table usage
    DWORD   offBmi;             // Offset to the BITMAPINFO structure
    DWORD   cbBmi;              // Size of the BITMAPINFO structure
                                // The bitmap info is followed by the bitmap
                                // bits to form a packed DIB.
    DWORD   offBits;            // Offset to the bitmap bits
    DWORD   cbBits;             // Size of the bitmap bits
} EMRCREATEDIBPATTERNBRUSHPT, *PEMRCREATEDIBPATTERNBRUSHPT;

typedef struct tagEMRFORMAT
{
    EMRSignature   dSignature;         // Format signature, e.g. ENHMETA_SIGNATURE.
    DWORD   nVersion;           // Format version number.
    DWORD   cbData;             // Size of data in bytes.
    DWORD   offData;            // Offset to data from GDICOMMENT_IDENTIFIER.
                                // It must begin at a DWORD offset.
} EMRFORMAT, *PEMRFORMAT;

typedef struct tagEMRGLSRECORD
{
    EMR     emr;
    DWORD   cbData;             // Size of data in bytes
    BYTE    Data[1];
} EMRGLSRECORD, *PEMRGLSRECORD;

typedef struct tagEMRGLSBOUNDEDRECORD
{
    EMR     emr;
    RECTL   rclBounds;          // Bounds in recording coordinates
    DWORD   cbData;             // Size of data in bytes
    BYTE    Data[1];
} EMRGLSBOUNDEDRECORD, *PEMRGLSBOUNDEDRECORD;

typedef struct tagEMRPIXELFORMAT
{
    EMR     emr;
    PIXELFORMATDESCRIPTOR pfd;
} EMRPIXELFORMAT, *PEMRPIXELFORMAT;


typedef struct tagEMRCREATECOLORSPACE
{
    EMR             emr;
    DWORD           ihCS;       // ColorSpace handle index
    LOGCOLORSPACEA  lcs;        // Ansi version of LOGCOLORSPACE
} EMRCREATECOLORSPACE, *PEMRCREATECOLORSPACE;

typedef struct tagEMRSETCOLORSPACE
{
    EMR     emr;
    DWORD   ihCS;               // ColorSpace handle index
} EMRSETCOLORSPACE,    *PEMRSETCOLORSPACE,
  EMRSELECTCOLORSPACE, *PEMRSELECTCOLORSPACE,
  EMRDELETECOLORSPACE, *PEMRDELETECOLORSPACE;


typedef struct tagEMREXTESCAPE
{
    EMR     emr;
    INT     iEscape;            // Escape code
    INT     cbEscData;          // Size of escape data
    BYTE    EscData[1];         // Escape data
} EMREXTESCAPE,  *PEMREXTESCAPE,
  EMRDRAWESCAPE, *PEMRDRAWESCAPE;

typedef struct tagEMRNAMEDESCAPE
{
    EMR     emr;
    INT     iEscape;            // Escape code
    INT     cbDriver;           // Size of driver name
    INT     cbEscData;          // Size of escape data
    BYTE    EscData[1];         // Driver name and Escape data
} EMRNAMEDESCAPE, *PEMRNAMEDESCAPE,
  EMRSETICMPROFILEA, *PEMRSETICMPROFILEA,
  EMRSETICMPROFILEW, *PEMRSETICMPROFILEW;


typedef struct tagEMRCREATECOLORSPACEA
{
    EMR             emr;
    DWORD           ihCS;       // ColorSpace handle index
    LOGCOLORSPACEA  lcs;        // Unicode version of logical color space structure
    EMRColorSpaceFlagMask           dwFlags;    // flags
    DWORD           cbData;     // size of raw source profile data if attached
    BYTE            Data[1];    // Array size is cbData
} EMRCREATECOLORSPACEA, *PEMRCREATECOLORSPACEA;

typedef struct tagEMRCREATECOLORSPACEW
{
    EMR             emr;
    DWORD           ihCS;       // ColorSpace handle index
    LOGCOLORSPACEW  lcs;        // Unicode version of logical color space structure
    EMRColorSpaceFlagMask           dwFlags;    // flags
    DWORD           cbData;     // size of raw source profile data if attached
    BYTE            Data[1];    // Array size is cbData
} EMRCREATECOLORSPACEW, *PEMRCREATECOLORSPACEW;

//#define COLORMATCHTOTARGET_EMBEDED      0x00000001

typedef struct tagCOLORMATCHTOTARGET
{
    EMR     emr;
    _CS   dwAction;           // CS_ENABLE, CS_DISABLE or CS_DELETE_TRANSFORM
    EMRColorMatchFlagMask   dwFlags;            // flags
    DWORD   cbName;             // Size of desired target profile name
    DWORD   cbData;             // Size of raw target profile data if attached
    BYTE    Data[1];            // Array size is cbName + cbData
} EMRCOLORMATCHTOTARGET, *PEMRCOLORMATCHTOTARGET;

typedef struct tagCOLORCORRECTPALETTE
{
    EMR     emr;
    DWORD   ihPalette;          // Palette handle index
    DWORD   nFirstEntry;        // Index of first entry to correct
    DWORD   nPalEntries;        // Number of palette entries to correct
    DWORD   nReserved;          // Reserved
} EMRCOLORCORRECTPALETTE, *PEMRCOLORCORRECTPALETTE;

typedef struct tagEMRALPHABLEND
{
    EMR     emr;
    RECTL   rclBounds;          // Inclusive-inclusive bounds in device units
    LONG    xDest;
    LONG    yDest;
    LONG    cxDest;
    LONG    cyDest;
    _TernaryDrawMode   dwRop;
    LONG    xSrc;
    LONG    ySrc;
    XFORM   xformSrc;           // Source DC transform
    COLORREF crBkColorSrc;      // Source DC BkColor in RGB
    _DIB_Color   iUsageSrc;          // Source bitmap info color table usage
                                // (DIB_RGB_COLORS)
    DWORD   offBmiSrc;          // Offset to the source BITMAPINFO structure
    DWORD   cbBmiSrc;           // Size of the source BITMAPINFO structure
    DWORD   offBitsSrc;         // Offset to the source bitmap bits
    DWORD   cbBitsSrc;          // Size of the source bitmap bits
    LONG    cxSrc;
    LONG    cySrc;
} EMRALPHABLEND, *PEMRALPHABLEND;

typedef struct tagEMRGRADIENTFILL
{
    EMR       emr;
    RECTL     rclBounds;          // Inclusive-inclusive bounds in device units
    DWORD     nVer;
    DWORD     nTri;
    _GRADIENT_FILL     ulMode;
    TRIVERTEX Ver[1];
}EMRGRADIENTFILL,*PEMRGRADIENTFILL;

typedef struct tagEMRTRANSPARENTBLT
{
    EMR     emr;
    RECTL   rclBounds;          // Inclusive-inclusive bounds in device units
    LONG    xDest;
    LONG    yDest;
    LONG    cxDest;
    LONG    cyDest;
    _TernaryDrawMode   dwRop;
    LONG    xSrc;
    LONG    ySrc;
    XFORM   xformSrc;           // Source DC transform
    COLORREF crBkColorSrc;      // Source DC BkColor in RGB
    _DIB_Color   iUsageSrc;          // Source bitmap info color table usage
                                // (DIB_RGB_COLORS)
    DWORD   offBmiSrc;          // Offset to the source BITMAPINFO structure
    DWORD   cbBmiSrc;           // Size of the source BITMAPINFO structure
    DWORD   offBitsSrc;         // Offset to the source bitmap bits
    DWORD   cbBitsSrc;          // Size of the source bitmap bits
    LONG    cxSrc;
    LONG    cySrc;
} EMRTRANSPARENTBLT, *PEMRTRANSPARENTBLT;

typedef struct tagEMRSETICMPROFILE
{
    EMR     emr;
    DWORD   dwFlags;            // flags
    DWORD   cbName;             // Size of desired profile name
    DWORD   cbData;             // Size of raw profile data if attached
    BYTE    Data[1];            // Array size is cbName and cbData
} EMRSETICMPROFILE, *PEMRSETICMPROFILE;

typedef struct _POINTFLOAT {
    FLOAT   x;
    FLOAT   y;
} POINTFLOAT, *PPOINTFLOAT;

typedef struct _GLYPHMETRICSFLOAT {
    FLOAT       gmfBlackBoxX;
    FLOAT       gmfBlackBoxY;
    POINTFLOAT  gmfptGlyphOrigin;
    FLOAT       gmfCellIncX;
    FLOAT       gmfCellIncY;
} GLYPHMETRICSFLOAT, *PGLYPHMETRICSFLOAT,  *LPGLYPHMETRICSFLOAT;

/* Layer plane descriptor */
typedef struct tagLAYERPLANEDESCRIPTOR { // lpd
    WORD  nSize;
    WORD  nVersion;
    _LAYERPLANEDESCRIPTOR dwFlags;
    _LPD_TYPE  iPixelType;
    BYTE  cColorBits;
    BYTE  cRedBits;
    BYTE  cRedShift;
    BYTE  cGreenBits;
    BYTE  cGreenShift;
    BYTE  cBlueBits;
    BYTE  cBlueShift;
    BYTE  cAlphaBits;
    BYTE  cAlphaShift;
    BYTE  cAccumBits;
    BYTE  cAccumRedBits;
    BYTE  cAccumGreenBits;
    BYTE  cAccumBlueBits;
    BYTE  cAccumAlphaBits;
    BYTE  cDepthBits;
    BYTE  cStencilBits;
    BYTE  cAuxBuffers;
    BYTE  iLayerPlane;
    BYTE  bReserved;
    COLORREF crTransparent;
} LAYERPLANEDESCRIPTOR, *PLAYERPLANEDESCRIPTOR,  *LPLAYERPLANEDESCRIPTOR;

typedef struct _WGLSWAP
{
    HDC hdc;
    UINT uiFlags;
} WGLSWAP, *PWGLSWAP,  *LPWGLSWAP;

//=========================================================================================================
//=========================================================================================================
//=========================================================================================================
//=========================================================================================================


IntFailIfZero AddFontResourceA(
    [in] LPCSTR lpszFilename
);
IntFailIfZero AddFontResourceW(
    [in] LPCWSTR lpszFilename
);

FailOnFalse [gle] AnimatePalette(
    [in] HPALETTE hpal,           // handle to logical palette
    [in] UINT iStartIndex,        // first entry in logical palette
    [in] UINT cEntries,           // number of entries
    [in] PALETTEENTRY *ppe  // first replacement
);
FailOnFalse [gle]  Arc(
    [in] HDC hdc,         // handle to device context
    [in] int nLeftRect,   // x-coord of rectangle's upper-left corner
    [in] int nTopRect,    // y-coord of rectangle's upper-left corner
    [in] int nRightRect,  // x-coord of rectangle's lower-right corner
    [in] int nBottomRect, // y-coord of rectangle's lower-right corner
    [in] int nXStartArc,  // x-coord of first radial ending poIN int
    [in] int nYStartArc,  // y-coord of first radial ending poIN int
    [in] int nXEndArc,    // x-coord of second radial ending poIN int
    [in] int nYEndArc     // y-coord of second radial ending poIN int
);
FailOnFalse  [gle] BitBlt(
    [in] HDC hdcDest, // handle to destination DC
    [in] int nXDest,  // x-coord of destination upper-left corner
    [in] int nYDest,  // y-coord of destination upper-left corner
    [in] int nWidth,  // width of destination rectangle
    [in] int nHeight, // height of destination rectangle
    [in] HDC hdcSrc,  // handle to source DC
    [in] int nXSrc,   // x-coordinate of source upper-left corner
    [in] int nYSrc,   // y-coordinate of source upper-left corner
    [in] _TernaryDrawMode dwRop  // raster operation code
);
FailOnFalse [gle]  CancelDC(
    [in] HDC hdc   // handle to DC
);
FailOnFalse  [gle] Chord(
    [in] HDC hdc,         // handle to DC
    [in] int nLeftRect,   // x-coord of upper-left corner of rectangle
    [in] int nTopRect,    // y-coord of upper-left corner of rectangle
    [in] int nRightRect,  // x-coord of lower-right corner of rectangle
    [in] int nBottomRect, // y-coord of lower-right corner of rectangle
    [in] int nXRadial1,   // x-coord of first radial's endpoIN int
    [in] int nYRadial1,   // y-coord of first radial's endpoIN int
    [in] int nXRadial2,   // x-coord of second radial's endpoIN int
    [in] int nYRadial2    // y-coord of second radial's endpoIN int
);
IntFailIfZero  [gle] ChoosePixelFormat(
    [in] HDC  hdc,  // device context to search for a best pixel format
               // match
    [in] PIXELFORMATDESCRIPTOR *  ppfd
               // pixel format for which a best match is sought
);

HMETAFILE  [gle] CloseMetaFile(
    [in] HDC hdc   // handle to Windows-metafile DC
);
_RegionFlags CombineRgn(
    [in] HRGN hrgnDest,      // handle to destination region
    [in] HRGN hrgnSrc1,      // handle to source region
    [in] HRGN hrgnSrc2,      // handle to source region
    [in] _CombineRgn fnCombineMode   // region combining mode
);
HMETAFILE  [gle] CopyMetaFileA(
    [in] HMETAFILE hmfSrc,  // handle to Windows-format metafile
    [in] LPCSTR lpszFile   // file name
);
HMETAFILE  [gle] CopyMetaFileW(
    [in] HMETAFILE hmfSrc,  // handle to Windows-format metafile
    [in] LPCWSTR lpszFile   // file name
);
HBITMAP  [gle] CreateBitmap(
    [in] int nWidth,         // bitmap width, in pixels
    [in] int nHeight,        // bitmap height, in pixels
    [in] UINT cPlanes,       // number of color planes
    [in] UINT cBitsPerPel,   // number of bits to identify color
    [in] VOID *lpvBits // color data array
);
HBITMAP  [gle] CreateBitmapIndirect(
     [in] BITMAP *lpbm    // bitmap data
);
HBRUSH  [gle] CreateBrushIndirect(
     [in] LOGBRUSH *lplb   // brush information
);
HBITMAP  [gle] CreateCompatibleBitmap(
    [in] HDC hdc,        // handle to DC
    [in] int nWidth,     // width of bitmap, in pixels
    [in] int nHeight     // height of bitmap, in pixels
);
HBITMAP  [gle] CreateDiscardableBitmap(
    [in] HDC hdc,     // handle to DC
    [in] int nWidth,  // bitmap width
    [in] int nHeight  // bitmap height
);
HDC  [gle] CreateCompatibleDC(
    [in] HDC hdc   // handle to DC
);
HDC  [gle] CreateDCA(
    [in] LPCSTR lpszDriver,        // driver name
    [in] LPCSTR lpszDevice,        // device name
    [in] LPCSTR lpszOutput,        // not used; should be NULL
    [in] DEVMODEA *lpInitData  // optional prIN inter data
);
HDC  [gle] CreateDCW(
    [in] LPCWSTR lpszDriver,        // driver name
    [in] LPCWSTR lpszDevice,        // device name
    [in] LPCWSTR lpszOutput,        // not used; should be NULL
    [in] DEVMODEW *lpInitData  // optional prIN inter data
);
HBITMAP  [gle] CreateDIBitmap(
    [in] HDC hdc,                        // handle to DC
    [in] BITMAPV5HEADER *lpbmih, // bitmap data
    [in] _CreateDIBitmap fdwInit,                  // initialization option
    [in] VOID *lpbInit,            // initialization data
    [in] BITMAPINFO *lpbmi,        // color-format data
    [in] _DIB_Color fuUsage                    // color-data usage
);

HBRUSH  [gle] CreateDIBPatternBrush(
    [in] HGLOBAL hglbDIBPacked,  // handle to DIB
    [in] _DIB_Color fuColorSpec        // color table data
);

HBRUSH  [gle] CreateDIBPatternBrushPt(
    [in] VOID *lpPackedDIB,  // bitmap bits
    [in] _DIB_Color iUsage               // usage
);
HRGN  [gle] CreateEllipticRgn(
    [in] int nLeftRect,   // x-coord of upper-left corner of rectangle
    [in] int nTopRect,    // y-coord of upper-left corner of rectangle
    [in] int nRightRect,  // x-coord of lower-right corner of rectangle
    [in] int nBottomRect  // y-coord of lower-right corner of rectangle
);
HRGN  [gle] CreateEllipticRgnIndirect(
     [in] RECT *lprc   // bounding rectangle
);
HFONT    [gle]  CreateFontIndirectA(
    [in]  LOGFONTA *lplf
);
HFONT  [gle]   CreateFontIndirectW(
    [in]  LOGFONTW *lplf
);

HFONT  [gle] CreateFontA(
    [in] int nHeight,               // height of font
    [in] int nWidth,                // average character width
    [in] int nEscapement,           // angle of escapement
    [in] int nOrientation,          // base-line orientation angle
    [in] _FW fnWeight,              // font weight
    [in] DWORD fdwItalic,           // italic attribute option
    [in] DWORD fdwUnderline,        // underline attribute option
    [in] DWORD fdwStrikeOut,        // strikeout attribute option
    [in] _CHARSET fdwCharSet,          // character set identifier
    [in] _OUT fdwOutputPrecision,  // output precision
    [in] _CLIP fdwClipPrecision,    // clipping precision
    [in] _OUT fdwQuality,          // output quality
    [in] _FF fdwPitchAndFamily,   // pitch and family
    [in] LPCSTR lpszFace           // typeface name
);

HFONT  [gle] CreateFontW(
    [in] int nHeight,               // height of font
    [in] int nWidth,                // average character width
    [in] int nEscapement,           // angle of escapement
    [in] int nOrientation,          // base-line orientation angle
    [in] _FW fnWeight,              // font weight
    [in] DWORD fdwItalic,           // italic attribute option
    [in] DWORD fdwUnderline,        // underline attribute option
    [in] DWORD fdwStrikeOut,        // strikeout attribute option
    [in] _CHARSET fdwCharSet,          // character set identifier
    [in] _OUT fdwOutputPrecision,  // output precision
    [in] _CLIP fdwClipPrecision,    // clipping precision
    [in] _OUT fdwQuality,          // output quality
    [in] _FF fdwPitchAndFamily,   // pitch and family
    [in] LPCWSTR lpszFace           // typeface name
);

HBRUSH  [gle] CreateHatchBrush(
    [in] int fnStyle,      // hatch style
    [in] COLORREF clrref   // foreground color
);
HDC  [gle] CreateICA(
    [in] LPCSTR lpszDriver,       // driver name
    [in] LPCSTR lpszDevice,       // device name
    [in] LPCSTR lpszOutput,       // port or file name
    [in] DEVMODEA *lpdvmInit  // optional initialization data
);
HDC  [gle] CreateICW(
    [in] LPCWSTR lpszDriver,       // driver name
    [in] LPCWSTR lpszDevice,       // device name
    [in] LPCWSTR lpszOutput,       // port or file name
    [in] DEVMODEW *lpdvmInit  // optional initialization data
);
HDC  [gle] CreateMetaFileA(
    [in] LPCSTR lpszFile
);
HDC  [gle] CreateMetaFileW(
    [in] LPCWSTR lpszFile
);

HPALETTE  [gle]  CreatePalette(
    [in]  LOGPALETTE * lplgpl
);
HPEN  [gle] CreatePen(
    [in] _PS fnPenStyle,    // pen style
    [in] int nWidth,        // pen width
    [in] COLORREF crColor   // pen color
);
HPEN    [gle]   CreatePenIndirect(
    [in]  LOGPEN *lplgpn
);
HRGN CreatePolyPolygonRgn(
    [in] POINT *lppt,        // poIN inter to array of poIN ints
    [in] INT *lpPolyCounts,  // poIN inter to count of vertices
    [in] int nCount,               // number of [in] integers in array
    [in] _PolyFill fnPolyFillMode        // polygon fill mode
);
HBRUSH  [gle] CreatePatternBrush(
    [in] HBITMAP hbmp   // handle to bitmap
);
HRGN CreateRectRgn(
    [in] int nLeftRect,   // x-coordinate of upper-left corner
    [in] int nTopRect,    // y-coordinate of upper-left corner
    [in] int nRightRect,  // x-coordinate of lower-right corner
    [in] int nBottomRect  // y-coordinate of lower-right corner
);
HRGN  [gle] CreateRectRgnIndirect(
    [in] RECT *lprc   // rectangle
);
HRGN  [gle] CreateRoundRectRgn(
    [in] int nLeftRect,      // x-coordinate of upper-left corner
    [in] int nTopRect,       // y-coordinate of upper-left corner
    [in] int nRightRect,     // x-coordinate of lower-right corner
    [in] int nBottomRect,    // y-coordinate of lower-right corner
    [in] int nWidthEllipse,  // height of ellipse
    [in] int nHeightEllipse  // width of ellipse
);
FailOnFalse  [gle] CreateScalableFontResourceA(
    DWORD fdwHidden,          // read-only option
    [in] LPCSTR lpszFontRes,      // font file name
    [in] LPCSTR lpszFontFile,     // scalable font file name
    [in] LPCSTR lpszCurrentPath   // scalable font file path
);
FailOnFalse  [gle] CreateScalableFontResourceW(
    DWORD fdwHidden,          // read-only option
    [in] LPCWSTR lpszFontRes,      // font file name
    [in] LPCWSTR lpszFontFile,     // scalable font file name
    [in] LPCWSTR lpszCurrentPath   // scalable font file path
);
HBRUSH  [gle] CreateSolidBrush(
    [in] COLORREF crColor   // brush color value
);

FailOnFalse  [gle]  DeleteDC(
    [in] HDC hdc
);
FailOnFalse  [gle]  DeleteMetaFile(
    [in] HMETAFILE hmf
);
FailOnFalse   [gle] DeleteObject(
    [in] HGDIOBJ hobj
);

IntFailIfZero  [gle] DescribePixelFormat(
    [in] HDC  hdc,           // device context of interest
    [in] int  iPixelFormat,  // pixel format selector
    [in] UINT  nBytes,       // size of buffer pointed to by ppfd
    [out] LPPIXELFORMATDESCRIPTOR  ppfd
                        // pointer to structure to receive pixel
                        // format data
);

SpoolerError [gle]  DrawEscape(
	[in] HDC hdc,            // handle to DC
	[in] int nEscape,        // escape function
	[in] int cbInput,        // size of structure for input
	[in] LPCSTR lpszInData   // structure for input
);
FailOnFalse  [gle] Ellipse(
	[in] HDC hdc,        // handle to DC
	[in] int nLeftRect,  // x-coord of upper-left corner of rectangle
	[in] int nTopRect,   // y-coord of upper-left corner of rectangle
	[in] int nRightRect, // x-coord of lower-right corner of rectangle
	[in] int nBottomRect // y-coord of lower-right corner of rectangle
);

int EnumFontFamiliesExA(
	[in] HDC hdc,                          // handle to DC
	[in] LPLOGFONTA lpLogfont,              // font information
	[in] FONTENUMPROCA lpEnumFontFamExProc, // callback function
	[in] LPARAM lParam,                    // additional data
	[in] DWORD dwFlags                     // not used; must be 0
);
int EnumFontFamiliesExW(
	[in] HDC hdc,                          // handle to DC
	[in] LPLOGFONTW lpLogfont,              // font information
	[in] FONTENUMPROCW lpEnumFontFamExProc, // callback function
	[in] LPARAM lParam,                    // additional data
	[in] DWORD dwFlags                     // not used; must be 0
);
int EnumFontFamiliesA(
	[in] HDC hdc,                        // handle to DC
	[in] LPCSTR lpszFamily,             // font family
	[in] FONTENUMPROCA lpEnumFontFamProc, // callback function
	[in] LPARAM lParam                   // additional data
);
int EnumFontFamiliesW(
	[in] HDC hdc,                        // handle to DC
	[in] LPCWSTR lpszFamily,             // font family
	[in] FONTENUMPROCW lpEnumFontFamProc, // callback function
	[in] LPARAM lParam                   // additional data
);

int EnumFontsA(
	[in] HDC hdc,                  // handle to DC
	[in] LPCSTR lpFaceName,       // font typeface name
	[in] FONTENUMPROCA lpFontFunc,  // callback function
	[in] LPARAM lParam             // application-supplied data
);
int EnumFontsW(
	[in] HDC hdc,                  // handle to DC
	[in] LPCWSTR lpFaceName,       // font typeface name
	[in] FONTENUMPROCW lpFontFunc,  // callback function
	[in] LPARAM lParam             // application-supplied data
);

int EnumObjects(
	[in] HDC hdc,                    // handle to DC
	[in] int nObjectType,            // object-type identifier
	[in] GOBJENUMPROC lpObjectFunc,  // callback function
	[in] LPARAM lParam               // application-supplied data
);


FailOnFalse EqualRgn(
	[in] HRGN hSrcRgn1,  // handle to first region
	[in] HRGN hSrcRgn2   // handle to second region
);
_RegionFlags ExcludeClipRect(
	[in] HDC hdc,         // handle to DC
	[in] int nLeftRect,   // x-coord of upper-left corner
	[in] int nTopRect,    // y-coord of upper-left corner
	[in] int nRightRect,  // x-coord of lower-right corner
	[in] int nBottomRect  // y-coord of lower-right corner
);
HRGN  [gle] ExtCreateRegion(
	[in] XFORM *lpXform,     // transformation data
	[in] DWORD nCount,             // size of region data
	[in] RGNDATA *lpRgnData  // region data buffer
);

FailOnFalse  [gle] ExtFloodFill(
	[in] HDC hdc,          // handle to DC
	[in] int nXStart,      // starting x-coordinate
	[in] int nYStart,      // starting y-coordinate
	[in] COLORREF crColor, // fill color
	[in] _FLOODFILL fuFillType   // fill type
);
FailOnFalse  [gle] FillRgn(
	[in] HDC hdc,    // handle to device context
	[in] HRGN hrgn,  // handle to region to be filled
	[in] HBRUSH hbr  // handle to brush used to fill the region
);

FailOnFalse  [gle] FloodFill(
	[in] HDC hdc,          // handle to DC
	[in] int nXStart,      // starting x-coordinate
	[in] int nYStart,      // starting y-coordinate
	[in] COLORREF crFill   // fill color
);
FailOnFalse  [gle] FrameRgn(
	[in] HDC hdc,     // handle to device context
	[in] HRGN hrgn,   // handle to region to be framed
	[in] HBRUSH hbr,  // handle to brush used to draw border
	[in] int nWidth,  // width of region frame
	[in] int nHeight  // height of region frame
);
_BinaryDrawMode [gle]  GetROP2(
	[in] HDC hdc   // handle to device context
);
FailOnFalse  [gle] GetAspectRatioFilterEx(
	[in] HDC hdc,               // handle to DC
	[out] LPSIZE lpAspectRatio   // aspect-ratio filter
);
COLORREF_RETURN GetBkColor(
    HDC hdc
);

COLORREF_RETURN GetDCBrushColor(
    HDC hdc
);
COLORREF_RETURN GetDCPenColor(
    HDC hdc
);

IntFailIfZero    GetBkMode(
	[in] HDC hdc
);
LONG [gle]  GetBitmapBits(
	[in] HBITMAP hbmp,      // handle to bitmap
	[in] LONG cbBuffer,     // number of bytes to copy
	[out] LPVOID lpvBits     // buffer to receive bits
);

FailOnFalse [gle]  GetBitmapDimensionEx(
	[in] HBITMAP hBitmap,     // handle to bitmap
	[out] LPSIZE lpDimension   // dimensions
);

_DCB GetBoundsRect(
	[in] HDC hdc,            // handle to device context
	[out] LPRECT lprcBounds,  // bounding rectangle
	[in] _DCB flags          // function options
);

FailOnFalse  [gle] GetBrushOrgEx(
	[in] HDC hdc,       // handle to DC
	[out] LPPOINT lppt   // coordinates of origin
);

FailOnFalse  [gle] GetCharWidthA(
	[in] HDC hdc,         // handle to DC
	[in] UINT iFirstChar, // first character in range
	[in] UINT iLastChar,  // last character in range
	[in] LPINT lpBuffer   // buffer for widths
);
FailOnFalse  [gle] GetCharWidthW(
	[in] HDC hdc,         // handle to DC
	[in] UINT iFirstChar, // first character in range
	[in] UINT iLastChar,  // last character in range
	[out] LPINT lpBuffer   // buffer for widths
);

FailOnFalse  [gle] GetCharWidth32A(
	[in] HDC hdc,          // handle to DC
	[in] UINT iFirstChar,  // first character in range
	[in] UINT iLastChar,   // last character in range
	[out] LPINT lpBuffer    // buffer for widths
);
FailOnFalse [gle]  GetCharWidth32W(
	[in] HDC hdc,          // handle to DC
	[in] UINT iFirstChar,  // first character in range
	[in] UINT iLastChar,   // last character in range
	[out] LPINT lpBuffer    // buffer for widths
);
FailOnFalse [gle]  GetCharWidthFloatA(
	[in] HDC hdc,          // handle to DC
	[in] UINT iFirstChar,  // first-character code point
	[in] UINT iLastChar,   // last-character code point
	[out] PFLOAT pxBuffer   // buffer for widths
);
FailOnFalse [gle]  GetCharWidthFloatW(
	[in] HDC hdc,          // handle to DC
	[in] UINT iFirstChar,  // first-character code point
	[in] UINT iLastChar,   // last-character code point
	[out] PFLOAT pxBuffer   // buffer for widths
);
FailOnFalse [gle]  GetCharABCWidthsA(
	[in] HDC hdc,         // handle to DC
	[in] UINT uFirstChar, // first character in range
	[in] UINT uLastChar,  // last character in range
	[out] LPABC lpabc      // array of character widths
);
FailOnFalse [gle]  GetCharABCWidthsW(
	[in] HDC hdc,         // handle to DC
	[in] UINT uFirstChar, // first character in range
	[in] UINT uLastChar,  // last character in range
	[out] LPABC lpabc      // array of character widths
);
FailOnFalse  [gle] GetCharABCWidthsFloatA(
	[in] HDC hdc,            // handle to DC
	[in] UINT iFirstChar,    // first character in range
	[in] UINT iLastChar,     // last character in range
	[out] LPABCFLOAT lpABCF   // array of character widths
);
FailOnFalse  [gle] GetCharABCWidthsFloatW(
	[in] HDC hdc,            // handle to DC
	[in] UINT iFirstChar,    // first character in range
	[in] UINT iLastChar,     // last character in range
	[out] LPABCFLOAT lpABCF   // array of character widths
);
_RegionFlags  [gle] GetClipBox(
	[in] HDC hdc,      // handle to DC
	[out] LPRECT lprc   // rectangle
);
IntFailIfNeg1  [gle] GetClipRgn(
	[in] HDC hdc,           // handle to DC
	[in] HRGN hrgn          // handle to region
);
IntFailIfZero  [gle] GetMetaRgn(
	[in] HDC hdc,    // handle to DC
	[in] HRGN hrgn   // handle to region
);
HGDIOBJ  [gle] GetCurrentObject(
	[in] HDC hdc,           // handle to DC
	[in] _OBJ uObjectType   // object type
);
FailOnFalse  [gle] GetCurrentPositionEx(
	[in] HDC hdc,        // handle to device context
	[out] LPPOINT lpPoint // current position
);
int GetDeviceCaps(
	[in] HDC hdc,     // handle to DC
	[in] _DeviceParameters nIndex   // index of capability
);

IntFailIfZero GetDIBits(
	[in] HDC hdc,           // handle to DC
	[in] HBITMAP hbmp,      // handle to bitmap
	[in] UINT uStartScan,   // first scan line to set
	[in] UINT cScanLines,   // number of scan lines to copy
	[out] LPVOID lpvBits,    // array for bitmap bits
	      LPBITMAPINFO lpbi, // bitmap data buffer
	[in] _DIB_Color uUsage        // RGB or palette index
);
_GDI_ERROR  [gle] GetFontData(
	[in] HDC hdc,           // handle to DC
	[in] DWORD dwTable,     // metric table name
	[in] DWORD dwOffset,    // offset into table
	[in] LPVOID lpvBuffer,  // buffer for returned data
	[in] DWORD cbData       // length of data
);

DWORD  [gle] GetGlyphOutlineA(
	[in] HDC hdc,             // handle to DC
	[in] UINT uChar,          // character to query
	[in] _GGO uFormat,        // data format
	[out] LPGLYPHMETRICS lpgm, // glyph metrics
	[in] DWORD cbBuffer,      // size of data buffer
	[out] LPVOID lpvBuffer,    // data buffer
	[in] MAT2 *lpmat2   // transformation matrix
);
DWORD  [gle] GetGlyphOutlineW(
	[in] HDC hdc,             // handle to DC
	[in] UINT uChar,          // character to query
	[in] _GGO uFormat,        // data format
	[out] LPGLYPHMETRICS lpgm, // glyph metrics
	[in] DWORD cbBuffer,      // size of data buffer
	[out] LPVOID lpvBuffer,    // data buffer
	[in] MAT2 *lpmat2   // transformation matrix
);

_GM    [gle]  GetGraphicsMode(
    [in] HDC hdc
);
_MM  [gle] GetMapMode(
	[in] HDC hdc   // handle to device context
);

UintFailIfZero  [gle] GetMetaFileBitsEx(
    [in] HMETAFILE hmf,
    [in] UINT nSize,
    [out] LPVOID lpvData
);

HMETAFILE    GetMetaFileA(
    [in] LPCSTR lpszMetaFile
);
HMETAFILE    GetMetaFileW(
    [in] LPCWSTR lpszMetaFile
);
COLORREF_RETURN  [gle] GetNearestColor(
	[in] HDC hdc,           // handle to DC
	[in] COLORREF crColor   // color to be matched
);

COLORREF_RETURN [gle] GetNearestPaletteIndex(
	[in] HPALETTE hpal,     // handle to logical palette
	[in] COLORREF crColor   // color to be matched
);

_OBJ  [gle] GetObjectType(
	[in] HGDIOBJ h   // handle to graphics object
);

UintFailIfZero [gle]  GetOutlineTextMetricsA(
	[in] HDC hdc,                    // handle to DC
	[in] UINT cbData,                // size of metric data array
	[out] LPOUTLINETEXTMETRICA lpOTM   // metric data array
);
UintFailIfZero [gle]  GetOutlineTextMetricsW(
	[in] HDC hdc,                    // handle to DC
	[in] UINT cbData,                // size of metric data array
	[out] LPOUTLINETEXTMETRICW lpOTM   // metric data array
);

UintFailIfZero [gle]  GetPaletteEntries(
	[in] HPALETTE hpal,        // handle to logical palette
	[in] UINT iStartIndex,     // first entry to retrieve
	[in] UINT nEntries,        // number of entries to retrieve
	[out] LPPALETTEENTRY lppe   // array that receives entries
);
COLORREF_RETURN GetPixel(
	[in] HDC hdc,    // handle to DC
	[in] int nXPos,  // x-coordinate of pixel
	[in] int nYPos   // y-coordinate of pixel
);

IntFailIfZero  [gle] GetPixelFormat(
    HDC hdc
);

_PolyFill  [gle] GetPolyFillMode(
	[in] HDC hdc   // handle to device context
);

FailOnFalse  [gle]  GetRasterizerCaps(
	[out] LPRASTERIZER_STATUS lprs ,
	[in]  UINT cb
);
IntFailIfNeg1   GetRandomRgn (
	[in]  HDC hdc ,
	[in] HRGN hrgn,
	[in]  INT iNum
);

IntFailIfZero GetRegionData(
	[in] HRGN hRgn,            // handle to region
	[in] DWORD dwCount,        // size of region data buffer
	[out] LPRGNDATA lpRgnData   // region data buffer
);
_RegionFlags GetRgnBox(
	[in] HRGN hrgn,   // handle to a region
	[out] LPRECT lprc  // bounding rectangle
);
HGDIOBJ  [gle] GetStockObject(
	[in] _StockObject fnObject   // stock object type
);
IntFailIfZero  [gle] GetStretchBltMode(
	[in] HDC hdc   // handle to DC
);

UINT  [gle] GetSystemPaletteEntries(
	[in] HDC hdc,              // handle to DC
	[in] UINT iStartIndex,     // first entry to be retrieved
	[in] UINT nEntries,        // number of entries to be retrieved
	[out] LPPALETTEENTRY lppe   // array that receives entries
);

_SYSPAL  [gle] GetSystemPaletteUse(
	[in] HDC hdc   // handle to DC
);

_ODD_FAILURE  [gle] GetTextCharacterExtra(
	[in] HDC hdc   // handle to DC
);
_TextAlignmentOptions  [gle] GetTextAlign(
	[in] HDC hdc   // handle to DC
);
COLORREF_RETURN GetTextColor(
    HDC hdc
);

FailOnFalse  [gle] GetTextExtentPointA(
	[in] HDC hdc,           // handle to DC
	[in] LPCSTR lpString,  // text string
	[in] int cbString,      // number of characters in string
	[out] LPSIZE lpSize      // string size
);
FailOnFalse  [gle] GetTextExtentPointW(
	[in] HDC hdc,           // handle to DC
	[in] LPCWSTR lpString,  // text string
	[in] int cbString,      // number of characters in string
	[out] LPSIZE lpSize      // string size
);

FailOnFalse  [gle] GetTextExtentPoint32A(
	[in] HDC hdc,           // handle to DC
	[in] LPCSTR lpString,  // text string
	[in] int cbString,      // characters in string
	[out] LPSIZE lpSize      // string size
);
FailOnFalse  [gle] GetTextExtentPoint32W(
	[in] HDC hdc,           // handle to DC
	[in] LPCWSTR lpString,  // text string
	[in] int cbString,      // characters in string
	[out] LPSIZE lpSize      // string size
);

FailOnFalse  [gle] GetTextExtentExPointA(
	[in] HDC hdc,         // handle to DC
	[in] LPCSTR lpszStr, // character string
	[in] int cchString,   // number of characters
	[in] int nMaxExtent,  // maximum width of formatted string
	[out] LPINT lpnFit,    // maximum number of characters
	[out] LPINT alpDx,     // array of partial string widths
	[out] LPSIZE lpSize    // string dimensions
);
FailOnFalse  [gle] GetTextExtentExPointW(
	[in] HDC hdc,         // handle to DC
	[in] LPCWSTR lpszStr, // character string
	[in] int cchString,   // number of characters
	[in] int nMaxExtent,  // maximum width of formatted string
	[out] LPINT lpnFit,    // maximum number of characters
	[out] LPINT alpDx,     // array of partial string widths
	[out] LPSIZE lpSize    // string dimensions
);
_CHARSET GetTextCharset(
	[in] HDC hdc  // handle to DC
);

_CHARSET GetTextCharsetInfo(
	[in] HDC hdc,                // handle to DC
	[out] LPFONTSIGNATURE lpSig,  // data buffer
	[in] DWORD dwFlags           // reserved; must be zero
);
FailOnFalse  [gle] TranslateCharsetInfo(
	[out] DWORD *lpSrc,        // information
	[out] LPCHARSETINFO lpCs,  // character set information
	[in] _TCI_SRC dwFlags        // translation option
);

_GCP  [gle] GetFontLanguageInfo(
	[in] HDC hdc  // handle to DC
);
IntFailIfZero  [gle] GetCharacterPlacementA(
	[in] HDC hdc,                  // handle to DC
	[in] LPCSTR lpString,         // character string
	[in] int nCount,               // number of characters
	[in] int nMaxExtent,           // maximum extent for string
	     LPGCP_RESULTSA lpResults,  // placement result
	[in] _GCP dwFlags             // placement options
);
IntFailIfZero  [gle] GetCharacterPlacementW(
	[in] HDC hdc,                  // handle to DC
	[in] LPCWSTR lpString,         // character string
	[in] int nCount,               // number of characters
	[in] int nMaxExtent,           // maximum extent for string
	     LPGCP_RESULTSW lpResults,  // placement result
	[in] _GCP dwFlags             // placement options
);


IntFailIfZero GetFontUnicodeRanges(
	[in] HDC hdc,         // handle to DC
	[out] LPGLYPHSET lpgs  // glyph set
);
DwordFailIfZero  [gle] GetGlyphIndicesA(
	[in] HDC hdc,       // handle to DC
	[in] LPCSTR lpstr, // string to convert
	[in] int c,         // number of characters in string
	[out] LPWORD pgi,    // array of glyph indices
	[in] _GGI_MARK_NONEXISTING_GLYPHS fl       // glyph options
);
DwordFailIfZero  [gle] GetGlyphIndicesW(
	[in] HDC hdc,       // handle to DC
	[in] LPCWSTR lpstr, // string to convert
	[in] int c,         // number of characters in string
	[out] LPWORD pgi,    // array of glyph indices
	[in] _GGI_MARK_NONEXISTING_GLYPHS fl       // glyph options
);
FailOnFalse  [gle] GetTextExtentPointI(
	[in] HDC hdc,           // handle to DC
	[in] LPWORD pgiIn,      // glyph indices
	[in] int cgi,           // number of indices in array
	[out] LPSIZE lpSize      // string size
);
FailOnFalse  [gle] GetTextExtentExPointI(
	[in] HDC hdc,         // handle to DC
	[in] LPWORD pgiIn,    // array of glyph indices
	[in] int cgi,         // number of glyphs in array
	[in] int nMaxExtent,  // maximum width of formatted string
	[out] LPINT lpnFit,    // maximum number of characters
	[out] LPINT alpDx,     // array of partial string widths
	[out] LPSIZE lpSize    // string dimensions
);
FailOnFalse  [gle] GetCharWidthI(
	[in] HDC hdc,         // handle to DC
	[in] UINT giFirst,    // first glyph index in range
	[in] UINT cgi,        // number of glyph indicies in range
	[in] LPWORD pgi,      // array of glyph indices
	[out] LPINT lpBuffer   // buffer for widths
);
FailOnFalse  [gle] GetCharABCWidthsI(
	[in] HDC hdc,         // handle to DC
	[in] UINT giFirst,    // first glyph index in range
	[in] UINT cgi,        // count of glyph indices in range
	[in] LPWORD pgi,      // array of glyph indices
	[out] LPABC lpabc      // array of character widths
);


IntFailIfZero AddFontResourceExA(
	[in] LPCSTR lpszFilename, // font file name
	[in] _FR fl,             // font characteristics
	[in] DESIGNVECTOR * pdv             // reserved
);

IntFailIfZero AddFontResourceExW(
	[in] LPCWSTR lpszFilename, // font file name
	[in] _FR fl,             // font characteristics
	[in] DESIGNVECTOR * pdv             // reserved
);

FailOnFalse RemoveFontResourceExA(
	[in] LPCSTR lpFileName,  // name of font file
	[in] _FR fl,            // font characteristics
	[in] DESIGNVECTOR * pdv             // reserved
);
FailOnFalse RemoveFontResourceExW(
	[in] LPCWSTR lpFileName,  // name of font file
	[in] _FR fl,            // font characteristics
	[in] DESIGNVECTOR * pdv             // reserved
);
HANDLE AddFontMemResourceEx(
	[in] PVOID pbFont,       // font resource
	[in] DWORD cbFont,       // number of bytes in font resource
	[in] PVOID pdv,          // Reserved. Must be 0.
	[in] DWORD *pcFonts      // number of fonts installed
);

FailOnFalse RemoveFontMemResourceEx(
	[in] HANDLE fh   // handle to the font resource
);


HFONT CreateFontIndirectExA(
	[in] ENUMLOGFONTEXDVA *penumlfex   // characteristiccs
);

HFONT CreateFontIndirectExW(
	[in] ENUMLOGFONTEXDVW *penumlfex   // characteristiccs
);


FailOnFalse  [gle] GetViewportExtEx(
	[in] HDC hdc,      // handle to device context
	[out] LPSIZE lpSize // viewport dimensions
);
FailOnFalse  [gle] GetViewportOrgEx(
	[in] HDC hdc,        // handle to device context
	[out] LPPOINT lpPoint // viewport origin
);
FailOnFalse  [gle] GetWindowExtEx(
	[in] HDC hdc,        // handle to device context
	[out] LPSIZE lpSize   // window extents
);
FailOnFalse  [gle] GetWindowOrgEx(
	[in] HDC hdc,         // handle to device context
	[out] LPPOINT lpPoint  // window origin
);
_RegionFlags IntersectClipRect(
	[in] HDC hdc,         // handle to DC
	[in] int nLeftRect,   // x-coord of upper-left corner
	[in] int nTopRect,    // y-coord of upper-left corner
	[in] int nRightRect,  // x-coord of lower-right corner
	[in] int nBottomRect  // y-coord of lower-right corner
);
FailOnFalse  [gle] InvertRgn(
	[in] HDC hdc,    // handle to device context
	[in] HRGN hrgn   // handle to region to be inverted
);

FailOnFalse  [gle] LineDDA(
	[in] int nXStart,             // x-coordinate of starting point
	[in] int nYStart,             // y-coordinate of starting point
	[in] int nXEnd,               // x-coordinate of ending point
	[in] int nYEnd,               // y-coordinate of ending point
	[in] LINEDDAPROC lpLineFunc,  // callback function
	[in] LPARAM lpData            // application-defined data
);
FailOnFalse  [gle] LineTo(
	[in] HDC hdc,    // device context handle
	[in] int nXEnd,  // x-coordinate of ending point
	[in] int nYEnd   // y-coordinate of ending point
);
FailOnFalse  [gle] MaskBlt(
	[in] HDC hdcDest,     // handle to destination DC
	[in] int nXDest,      // x-coord of destination upper-left corner
	[in] int nYDest,      // y-coord of destination upper-left corner
	[in] int nWidth,      // width of source and destination
	[in] int nHeight,     // height of source and destination
	[in] HDC hdcSrc,      // handle to source DC
	[in] int nXSrc,       // x-coord of upper-left corner of source
	[in] int nYSrc,       // y-coord of upper-left corner of source
	[in] HBITMAP hbmMask, // handle to monochrome bit mask
	[in] int xMask,       // horizontal offset into mask bitmap
	[in] int yMask,       // vertical offset into mask bitmap
	[in] _TernaryDrawMode dwRop      // raster operation code
);
FailOnFalse   [gle] PlgBlt(
	[in] HDC hdcDest,          // handle to destination DC
	[in] POINT *lpPoint, // destination vertices
	[in] HDC hdcSrc,           // handle to source DC
	[in] int nXSrc,            // x-coord of source upper-left corner
	[in] int nYSrc,            // y-coord of source upper-left corner
	[in] int nWidth,           // width of source rectangle
	[in] int nHeight,          // height of source rectangle
	[in] HBITMAP hbmMask,      // handle to bitmask
	[in] int xMask,            // x-coord of bitmask upper-left corner
	[in] int yMask             // y-coord of bitmask upper-left corner
);
_RegionFlags OffsetClipRgn(
	[in] HDC hdc,       // handle to DC
	[in] int nXOffset,  // offset along x-axis
	[in] int nYOffset   // offset along y-axis
);
_RegionFlags OffsetRgn(
	[in] HRGN hrgn,     // handle to region
	[in] int nXOffset,  // offset along x-axis
	[in] int nYOffset   // offset along y-axis
);
FailOnFalse   [gle] PatBlt(
	[in] HDC hdc,      // handle to DC
	[in] int nXLeft,   // x-coord of upper-left rectangle corner
	[in] int nYLeft,   // y-coord of upper-left rectangle corner
	[in] int nWidth,   // width of rectangle
	[in] int nHeight,  // height of rectangle
	[in] _TernaryDrawMode dwRop   // raster operation code
);
FailOnFalse   [gle] Pie(
	[in] HDC hdc,         // handle to DC
	[in] int nLeftRect,   // x-coord of upper-left corner of rectangle
	[in] int nTopRect,    // y-coord of upper-left corner of rectangle
	[in] int nRightRect,  // x-coord of lower-right corner of rectangle
	[in] int nBottomRect, // y-coord of lower-right corner of rectangle
	[in] int nXRadial1,   // x-coord of first radial's endpoint
	[in] int nYRadial1,   // y-coord of first radial's endpoint
	[in] int nXRadial2,   // x-coord of second radial's endpoint
	[in] int nYRadial2    // y-coord of second radial's endpoint
);
FailOnFalse   [gle] PlayMetaFile(
	[in] HDC hdc,        // handle to DC
	[in] HMETAFILE hmf   // handle to metafile
);
FailOnFalse  PaintRgn(
	[in] HDC hdc,    // handle to device context
	[in] HRGN hrgn   // handle to region to be painted
);


FailOnFalse   [gle] PolyPolygon(
	[in] HDC hdc,                  // handle to DC
	[in] POINT *lpPoints,    // array of vertices
	[in] INT *lpPolyCounts,  // array of count of vertices
	[in] int nCount                // count of polygons
);
BOOL  PtInRegion(
	[in] HRGN hrgn,  // handle to region
	[in] int X,      // x-coordinate of point
	[in] int Y       // y-coordinate of point
);
BOOL  PtVisible(
	[in] HDC hdc,  // handle to DC
	[in] int X,    // x-coordinate of point
	[in] int Y     // y-coordinate of point
);

BOOL  RectInRegion(
	[in] HRGN hrgn,         // handle to region
	[in] RECT *lprc   // pointer to rectangle
);
BOOL  RectVisible(
	[in] HDC hdc,           // handle to DC
	[in] RECT *lprc   // rectangle
);
FailOnFalse   [gle] Rectangle(
	[in] HDC hdc,         // handle to DC
	[in] int nLeftRect,   // x-coord of upper-left corner of rectangle
	[in] int nTopRect,    // y-coord of upper-left corner of rectangle
	[in] int nRightRect,  // x-coord of lower-right corner of rectangle
	[in] int nBottomRect  // y-coord of lower-right corner of rectangle
);

FailOnFalse   [gle] RestoreDC(
	[in] HDC hdc,       // handle to DC
	[in] int nSavedDC   // restore state
);
HDC  [gle]  ResetDCA(
	[in] HDC hdc,
	[in] DEVMODEA *lpInitData
);
HDC  [gle]  ResetDCW(
	[in] HDC hdc,
	[in] DEVMODEW *lpInitData
);

_GDI_ERROR [gle] RealizePalette(
	[in] HDC hdc   // handle to DC
);
FailOnFalse   [gle] RemoveFontResourceA(
	[in] LPCSTR lpFileName
);
FailOnFalse   [gle] RemoveFontResourceW(
	[in] LPCWSTR lpFileName
);
FailOnFalse   [gle] RoundRect(
	[in] HDC hdc,         // handle to DC
	[in] int nLeftRect,   // x-coord of upper-left corner of rectangle
	[in] int nTopRect,    // y-coord of upper-left corner of rectangle
	[in] int nRightRect,  // x-coord of lower-right corner of rectangle
	[in] int nBottomRect, // y-coord of lower-right corner of rectangle
	[in] int nWidth,      // width of ellipse
	[in] int nHeight      // height of ellipse
);

FailOnFalse   [gle] ResizePalette(
	[in] HPALETTE hpal, // handle to logical palette
	[in] UINT nEntries  // number of entries in logical palette
);

IntFailIfZero  [gle] SaveDC(
	[in] HDC hdc   // handle to DC
);
_RegionFlags  [gle] SelectClipRgn(
	[in] HDC hdc,    // handle to DC
	[in] HRGN hrgn   // handle to region
);
_RegionFlags  [gle] ExtSelectClipRgn(
	[in] HDC hdc,          // handle to DC
	[in] HRGN hrgn,        // handle to region
	[in] _CombineRgn fnMode        // region-selection mode
);
_RegionFlags   SetMetaRgn(
    HDC hdc
);

HGDIOBJ SelectObject(
	[in] HDC hdc,          // handle to DC
	[in] HGDIOBJ hgdiobj   // handle to object
);

HPALETTE  [gle] SelectPalette(
	[in] HDC hdc,                // handle to DC
	[in] HPALETTE hpal,          // handle to logical palette
	[in] BOOL bForceBackground   // foreground or background mode
);
COLORREF_RETURN   [gle] SetBkColor(
	[in] HDC hdc,           // handle to DC
	[in] COLORREF crColor   // background color value
);

COLORREF_RETURN  SetDCBrushColor(
	[in] HDC hdc,          // handle to DC
	[in] COLORREF crColor  // new brush color
);
COLORREF_RETURN  SetDCPenColor(
	[in] HDC hdc,          // handle to DC
	[in] COLORREF crColor  // new pen color
);

IntFailIfZero    [gle]  SetBkMode(
	[in] HDC hdc,      // handle to DC
	[in] int iBkMode   // background mode
);
LongFailIfZero  [gle] SetBitmapBits(
	[in] HBITMAP hbmp,        // handle to bitmap
	[in] DWORD cBytes,        // number of bytes in bitmap array
	[in] VOID *lpBits   // array with bitmap bits
);
_DCB  [gle] SetBoundsRect(
	[in] HDC hdc,                 // handle to DC
	[in] RECT *lprcBounds,  // bounding rectangle
	[in] _DCB flags               // rectangle combination option
);

IntFailIfZero  [gle] SetDIBits(
	[in] HDC hdc,                  // handle to DC
	[in] HBITMAP hbmp,             // handle to bitmap
	[in] UINT uStartScan,          // starting scan line
	[in] UINT cScanLines,          // number of scan lines
	[in] VOID *lpvBits,      // array of bitmap bits
	[in] BITMAPINFO *lpbmi,  // bitmap data
	[in] _DIB_Color fuColorUse           // type of color indexes to use
);

IntFailIfZero  [gle] SetDIBitsToDevice(
	[in] HDC hdc,                 // handle to DC
	[in] int XDest,               // x-coord of destination upper-left corner
	[in] int YDest,               // y-coord of destination upper-left corner
	[in] DWORD dwWidth,           // source rectangle width
	[in] DWORD dwHeight,          // source rectangle height
	[in] int XSrc,                // x-coord of source lower-left corner
	[in] int YSrc,                // y-coord of source lower-left corner
	[in] UINT uStartScan,         // first scan line in array
	[in] UINT cScanLines,         // number of scan lines
	[in] VOID *lpvBits,     // array of DIB bits
	[in] BITMAPINFO *lpbmi, // bitmap information
	[in] _DIB_Color fuColorUse          // RGB or palette indexes
);

_GDI_ERROR  [gle] SetMapperFlags(
	[in] HDC hdc,       // handle to DC
	[in] DWORD dwFlag   // font-mapper option
);
IntFailIfZero  [gle] SetGraphicsMode(
	[in] HDC hdc,    // handle to device context
	[in] _GM iMode   // graphics mode
);

IntFailIfZero  [gle] SetMapMode(
	[in] HDC hdc,        // handle to device context
	[in] _MM fnMapMode   // new mapping mode
);

_GDI_ERROR  [gle] SetLayout(
	[in] HDC hdc,
	[in] _LAYOUT dwLayout
);
_GDI_ERROR  [gle] GetLayout(
	[in] HDC hdc
);

HMETAFILE  [gle] SetMetaFileBitsEx(
	[in] UINT nSize,          // size of Windows-format metafile
	[in] BYTE *lpData   // metafile data
);

UintFailIfZero  [gle] SetPaletteEntries(
	[in] HPALETTE hpal,             // handle to logical palette
	[in] UINT iStart,               // index of first entry to set
	[in] UINT cEntries,             // number of entries to set
	[in] PALETTEENTRY *lppe   // array of palette entries
);

COLORREF_RETURN   [gle] SetPixel(
	[in] HDC hdc,           // handle to DC
	[in] int X,             // x-coordinate of pixel
	[in] int Y,             // y-coordinate of pixel
	[in] COLORREF crColor   // pixel color
);

FailOnFalse    [gle]  SetPixelV(
	[in] HDC hdc,           // handle to device context
	[in] int X,             // x-coordinate of pixel
	[in] int Y,             // y-coordinate of pixel
	[in] COLORREF crColor   // new pixel color
);

FailOnFalse  [gle]  SetPixelFormat(
	[in] HDC  hdc,  // device context whose pixel format the function
	                // attempts to set
	[in] int  iPixelFormat,
	                // pixel format index (one-based)
	[in] PIXELFORMATDESCRIPTOR *  ppfd
	              // pointer to logical pixel format specification
);

FailOnFalse  [gle] SetPolyFillMode(
	[in] HDC hdc,            // handle to device context
	[in] _PolyFill iPolyFillMode   // polygon fill mode
);
FailOnFalse  [gle] StretchBlt(
	[in] HDC hdcDest,      // handle to destination DC
	[in] int nXOriginDest, // x-coord of destination upper-left corner
	[in] int nYOriginDest, // y-coord of destination upper-left corner
	[in] int nWidthDest,   // width of destination rectangle
	[in] int nHeightDest,  // height of destination rectangle
	[in] HDC hdcSrc,       // handle to source DC
	[in] int nXOriginSrc,  // x-coord of source upper-left corner
	[in] int nYOriginSrc,  // y-coord of source upper-left corner
	[in] int nWidthSrc,    // width of source rectangle
	[in] int nHeightSrc,   // height of source rectangle
	[in] _TernaryDrawMode dwRop       // raster operation code
);


FailOnFalse   [gle]   SetRectRgn(
	[in] HRGN hrgn,       // handle to region
	[in] int nLeftRect,   // x-coordinate of upper-left corner of rectangle
	[in] int nTopRect,    // y-coordinate of upper-left corner of rectangle
	[in] int nRightRect,  // x-coordinate of lower-right corner of rectangle
	[in] int nBottomRect  // y-coordinate of lower-right corner of rectangle
);

_GDI_ERROR [gle]  StretchDIBits(
	[in] HDC hdc,                      // handle to DC
	[in] int XDest,                    // x-coord of destination upper-left corner
	[in] int YDest,                    // y-coord of destination upper-left corner
	[in] int nDestWidth,               // width of destination rectangle
	[in] int nDestHeight,              // height of destination rectangle
	[in] int XSrc,                     // x-coord of source upper-left corner
	[in] int YSrc,                     // y-coord of source upper-left corner
	[in] int nSrcWidth,                // width of source rectangle
	[in] int nSrcHeight,               // height of source rectangle
	[in] VOID *lpBits,           // bitmap bits
	[in] BITMAPINFO *lpBitsInfo, // bitmap data
	[in] _DIB_Color iUsage,                  // usage options
	[in] _TernaryDrawMode dwRop                   // raster operation code
);

IntFailIfZero  [gle] SetROP2(
	[in] HDC hdc,         // handle to DC
	[in] _BinaryDrawMode fnDrawMode   // drawing mode
);
IntFailIfZero  [gle] SetStretchBltMode(
	[in] HDC hdc,           // handle to DC
	[in] _COMBINRGN_STYLE iStretchMode   // bitmap stretching mode
);

_SYSPAL  [gle] SetSystemPaletteUse(
	[in] HDC hdc,      // handle to DC
	[in] _SYSPAL uUsage   // palette usage
);

_ODD_FAILURE  [gle] SetTextCharacterExtra(
	[in] HDC hdc,         // handle to DC
	[in] int nCharExtra   // extra-space value
);

COLORREF_RETURN  [gle]  SetTextColor(
	[in] HDC hdc,           // handle to DC
	[in] COLORREF crColor   // text color
);

UINT  [gle] SetTextAlign(
	[in] HDC hdc,     // handle to DC
	[in] _TextAlignmentOptions fMode   // text-alignment option
);

FailOnFalse  [gle]   SetTextJustification(
	[in] HDC hdc,          // handle to DC
	[in] int nBreakExtra,  // length of extra space
	[in] int nBreakCount   // count of space characters
);
FailOnFalse   [gle]  UpdateColors(
	[in] HDC hdc   // handle to DC
);

module MSIMG32.DLL:
FailOnFalse   [gle]  AlphaBlend(
	[in] HDC hdcDest,                 // handle to destination DC
	[in] int nXOriginDest,            // x-coord of upper-left corner
	[in] int nYOriginDest,            // y-coord of upper-left corner
	[in] int nWidthDest,              // destination width
	[in] int nHeightDest,             // destination height
	[in] HDC hdcSrc,                  // handle to source DC
	[in] int nXOriginSrc,             // x-coord of upper-left corner
	[in] int nYOriginSrc,             // y-coord of upper-left corner
	[in] int nWidthSrc,               // source width
	[in] int nHeightSrc,              // source height
	[in] BLENDFUNCTION blendFunction  // alpha-blending function
);

FailOnFalse   [gle]  TransparentBlt(
	[in] HDC hdcDest,        // handle to destination DC
	[in] int nXOriginDest,   // x-coord of destination upper-left corner
	[in] int nYOriginDest,   // y-coord of destination upper-left corner
	[in] int nWidthDest,     // width of destination rectangle
	[in] int hHeightDest,    // height of destination rectangle
	[in] HDC hdcSrc,         // handle to source DC
	[in] int nXOriginSrc,    // x-coord of source upper-left corner
	[in] int nYOriginSrc,    // y-coord of source upper-left corner
	[in] int nWidthSrc,      // width of source rectangle
	[in] int nHeightSrc,     // height of source rectangle
	[in] UINT crTransparent  // color to make transparent
);


FailOnFalse  [gle] GradientFill(
	[in] HDC hdc,                   // handle to DC
	[in] PTRIVERTEX pVertex,        // array of vertices
	[in] ULONG dwNumVertex,         // number of vertices
	[in] PVOID pMesh,               // array of gradients
	[in] ULONG dwNumMesh,           // size of gradient array
	[in] _GRADIENT_FILL dwMode               // gradient fill mode
);

module GDI32.DLL:
FailOnFalse   [gle]  PlayMetaFileRecord(
	[in] HDC hdc,                      // handle to DC
	[in] LPHANDLETABLE lpHandletable,  // metafile handle table
	[in] LPMETARECORD lpMetaRecord,    // metafile record
	[in] UINT nHandles                 // count of handles
);

FailOnFalse    [gle] EnumMetaFile(
	[in] HDC hdc,                // handle to DC
	[in] HMETAFILE hmf,          // handle to Windows-format metafile
	[in] MFENUMPROC lpMetaFunc,  // callback function
	[in] LPARAM lParam           // optional data
);

// Enhanced Metafile Function Declarations

HENHMETAFILE  [gle]  CloseEnhMetaFile(
	[in] HDC hdc   // handle to enhanced-metafile DC
);
HENHMETAFILE  [gle] CopyEnhMetaFileA(
	[in] HENHMETAFILE hemfSrc,  // handle to enhanced metafile
	[in] LPCSTR lpszFile       // file name
);
HENHMETAFILE  [gle] CopyEnhMetaFileW(
	[in] HENHMETAFILE hemfSrc,  // handle to enhanced metafile
	[in] LPCWSTR lpszFile       // file name
);

HDC  [gle] CreateEnhMetaFileA(
	[in] HDC hdcRef,            // handle to reference DC
	[in] LPCSTR lpFilename,    // file name
	[in] RECT *lpRect,    // bounding rectangle
	[in] LPCSTR lpDescription  // description string
);
HDC  [gle] CreateEnhMetaFileW(
	[in] HDC hdcRef,            // handle to reference DC
	[in] LPCWSTR lpFilename,    // file name
	[in] RECT *lpRect,    // bounding rectangle
	[in] LPCWSTR lpDescription  // description string
);

FailOnFalse   [gle] DeleteEnhMetaFile(
	[in] HENHMETAFILE hemf   // handle to an enhanced metafile
);
FailOnFalse    [gle] EnumEnhMetaFile(
	[in] HDC hdc,                     // handle to DC
	[in] HENHMETAFILE hemf,           // handle to enhanced metafile
	[in] ENHMFENUMPROC lpEnhMetaFunc, // callback function
	[in] LPVOID lpData,               // callback-function data
	[in] RECT *lpRect           // bounding rectangle
);
HENHMETAFILE  [gle] GetEnhMetaFileA(
	[in] LPCSTR lpszMetaFile   // file name
);
HENHMETAFILE  [gle] GetEnhMetaFileW(
	[in] LPCWSTR lpszMetaFile   // file name
);
UintFailIfZero   [gle]  GetEnhMetaFileBits(
	[in] HENHMETAFILE hemf, // handle to metafile
	[in] UINT cbBuffer,     // size of data buffer
	[out] LPBYTE lpbBuffer   // data buffer
);
UintFailIfZero  [gle]  GetEnhMetaFileDescriptionA(
	[in] HENHMETAFILE hemf,       // handle to enhanced metafile
	[in] UINT cchBuffer,          // size of text buffer
	[out] LPSTR lpszDescription   // text buffer
);
UintFailIfZero  [gle]  GetEnhMetaFileDescriptionW(
	[in] HENHMETAFILE hemf,       // handle to enhanced metafile
	[in] UINT cchBuffer,          // size of text buffer
	[out] LPWSTR lpszDescription   // text buffer
);
UintFailIfZero   [gle]  GetEnhMetaFileHeader(
	[in] HENHMETAFILE hemf,      // handle to enhanced metafile
	[in] UINT cbBuffer,          // size of buffer
	[out] LPENHMETAHEADER lpemh   // data buffer
);

UintFailIfZero  [gle]   GetEnhMetaFilePaletteEntries(
	[in] HENHMETAFILE hemf,    // handle to enhanced metafile
	[in] UINT cEntries,        // count of palette entries
	[out] LPPALETTEENTRY lppe   // array of palette entries
);

UintFailIfZero  [gle]   GetEnhMetaFilePixelFormat(
	[in] HENHMETAFILE  hemf, // handle to an enhanced metafile
	[in] DWORD cbBuffer,  // buffer size
	[out] PIXELFORMATDESCRIPTOR *  ppfd
	                    // pointer to logical pixel format specification
);

UintFailIfZero  [gle]   GetWinMetaFileBits(
	[in] HENHMETAFILE hemf, // handle to the enhanced metafile
	[in] UINT cbBuffer,     // buffer size
	[out] LPBYTE lpbBuffer,  // records buffer
	[in] INT fnMapMode,     // mapping mode
	[in] HDC hdcRef         // handle to reference DC
);
FailOnFalse   [gle]  PlayEnhMetaFile(
	[in] HDC hdc,            // handle to DC
	[in] HENHMETAFILE hemf,  // handle to an enhanced metafile
	[in] RECT *lpRect  // bounding rectangle
);

FailOnFalse  [gle]   PlayEnhMetaFileRecord(
	[in] HDC hdc,                              // handle to DC
	[in] LPHANDLETABLE lpHandletable,          // metafile handle table
	[in] ENHMETARECORD *lpEnhMetaRecord, // metafile record
	[in] UINT nHandles                         // count of handles
);

HENHMETAFILE  [gle]  SetEnhMetaFileBits(
	[in] UINT cbBuffer,      // buffer size
	[in] BYTE *lpData  // enhanced metafile data buffer
);


HENHMETAFILE  [gle] SetWinMetaFileBits(
	[in] UINT cbBuffer,              // size of buffer
	[in] BYTE *lpbBuffer,      // metafile data buffer
	[in] HDC hdcRef,                 // handle to reference DC
	[in] METAFILEPICT *lpmfp   // size of metafile picture
);
FailOnFalse   [gle]  GdiComment(
	[in] HDC hdc,             // handle to a device context
	[in] UINT cbSize,         // size of text buffer
	[in] BYTE *lpData   // text buffer
);

FailOnFalse  [gle]  GetTextMetricsA(
	[in] HDC hdc,            // handle to DC
	[out] LPTEXTMETRICA lptm   // text metrics
);
FailOnFalse  [gle]  GetTextMetricsW(
	[in] HDC hdc,            // handle to DC
	[out] LPTEXTMETRICW lptm   // text metrics
);

FailOnFalse  [gle]  AngleArc(
	[in] HDC hdc,            // handle to device context
	[in] int X,              // x-coordinate of circle's center
	[in] int Y,              // y-coordinate of circle's center
	[in] DWORD dwRadius,     // circle's radius
	[in] FLOAT eStartAngle,  // arc's start angle
	[in] FLOAT eSweepAngle   // arc's sweep angle
);
FailOnFalse  [gle]  PolyPolyline(
	[in] HDC hdc,                      // handle to device context
	[in] POINT *lppt,            // array of points
	[in] DWORD *lpdwPolyPoints,  // array of values
	[in] DWORD cCount                  // number of entries in values array
);
FailOnFalse  [gle]  GetWorldTransform(
	[in] HDC hdc,         // handle to device context
	[out] LPXFORM lpXform  // transformation
);
FailOnFalse  [gle]  SetWorldTransform(
	[in] HDC hdc,               // handle to device context
	[in] XFORM *lpXform   // transformation data
);
FailOnFalse  [gle]  ModifyWorldTransform(
	[in] HDC hdc,               // handle to device context
	[in] XFORM *lpXform,  // transformation data
	[in] DWORD iMode            // modification mode
);
FailOnFalse  [gle]  CombineTransform(
	[out] LPXFORM lpxformResult,  // combined transformation
	[in] XFORM *lpxform1,  // first transformation
	[in] XFORM *lpxform2   // second transformation
);
HBITMAP  [gle] CreateDIBSection(
	[in] HDC hdc,                 // handle to DC
	[in] BITMAPINFO *pbmi,  // bitmap data
	[in] UINT iUsage,             // data type indicator
	[out] VOID **ppvBits,          // bit values
	[in] HANDLE hSection,         // handle to file mapping object
	[in] DWORD dwOffset           // offset to bitmap bit values
);
UintFailIfZero  [gle] GetDIBColorTable(
	[in] HDC hdc,           // handle to DC
	[in] UINT uStartIndex,  // color table index of first entry
	[in] UINT cEntries,     // number of entries to retrieve
	[out] RGBQUAD *pColors   // array of color table entries
);
UintFailIfZero  [gle]  SetDIBColorTable(
	[in] HDC hdc,               // handle to DC
	[in] UINT uStartIndex,      // color table index of first entry
	[in] UINT cEntries,         // number of color table entries
	[in] RGBQUAD *pColors // array of color table entries
);

FailOnFalse  [gle]  SetColorAdjustment(
	[in] HDC hdc,                     // handle to DC
	[in] COLORADJUSTMENT *lpca  // color adjustment values
);
FailOnFalse  [gle]  GetColorAdjustment(
	[in] HDC hdc,                 // handle to DC
	[out] LPCOLORADJUSTMENT lpca   // color adjustment values
);

HPALETTE  [gle] CreateHalftonePalette(
	[in] HDC hdc   // handle to DC
);

FailOnFalse   [gle] AbortPath(
	[in] HDC hdc   // handle to DC
);
FailOnFalse   [gle] ArcTo(
	[in] HDC hdc,          // handle to device context
	[in] int nLeftRect,    // x-coord of rectangle's upper-left corner
	[in] int nTopRect,     // y-coord of rectangle's upper-left corner
	[in] int nRightRect,   // x-coord of rectangle's lower-right corner
	[in] int nBottomRect,  // y-coord of rectangle's lower-right corner
	[in] int nXRadial1,    // x-coord of first radial ending point
	[in] int nYRadial1,    // y-coord of first radial ending point
	[in] int nXRadial2,    // x-coord of second radial ending point
	[in] int nYRadial2     // y-coord of second radial ending point
);
FailOnFalse  [gle]  BeginPath(
	[in] HDC hdc   // handle to DC
);
FailOnFalse  [gle]  CloseFigure(
	[in] HDC hdc   // handle to DC
);
FailOnFalse   [gle] EndPath(
	[in] HDC hdc   // handle to DC
);
FailOnFalse  [gle]  FillPath(
	[in] HDC hdc   // handle to DC
);
FailOnFalse   [gle] FlattenPath(
	[in] HDC hdc   // handle to DC
);
IntFailIfNeg1   [gle]  GetPath(
	[in] HDC hdc,           // handle to DC
	[out] LPPOINT lpPoints,  // path vertices
	[out] _PT * lpTypes,    // array of path vertex types
	[in] int nSize          // count of points defining path
);
HRGN  [gle] PathToRegion(
	[in] HDC hdc   // handle to DC
);

FailOnFalse  [gle]  PolyDraw(
	[in] HDC hdc,               // handle to device context
	[in] POINT *lppt,     // array of points
	[in] _PT *lpbTypes,  // line and curve identifiers
	[in] int cCount             // count of points
);
FailOnFalse   [gle] SelectClipPath(
	[in] HDC hdc,    // handle to DC
	[in] _CombineRgn iMode   // clipping mode
);
IntFailIfZero  [gle] SetArcDirection(
	[in] HDC hdc,           // handle to device context
	[in] _AD ArcDirection   // new arc direction
);

FailOnFalse [gle] SetMiterLimit(
	[in] HDC hdc,            // handle to DC
	[in] FLOAT eNewLimit,    // new miter limit
	[out] PFLOAT peOldLimit   // previous miter limit
);
FailOnFalse [gle] StrokeAndFillPath(
	[in] HDC hdc   // handle to DC
);
FailOnFalse [gle] StrokePath(
	[in] HDC hdc   // handle to DC
);
FailOnFalse [gle] WidenPath(
	[in] HDC hdc   // handle to DC
);
HPEN  [gle] ExtCreatePen(
	[in] _PS dwPenStyle,      // pen style
	[in] DWORD dwWidth,         // pen width
	[in] LOGBRUSH *lplb,  // brush attributes
	[in] DWORD dwStyleCount,    // length of custom style array
	[in] DWORD *lpStyle   // custom style array
);

FailOnFalse  [gle]  GetMiterLimit(
	[in] HDC hdc,         // handle to DC
	[out] PFLOAT peLimit   // miter limit
);
_AD   GetArcDirection(
	[in] HDC hdc   // handle to device context
);

IntFailIfZero     [gle] GetObjectA(
	[in] HGDIOBJ hgdiobj,  // handle to graphics object
	[in] int cbBuffer,     // size of buffer for object information
	[out] LPVOID lpvObject  // buffer for object information
);
IntFailIfZero   [gle]  GetObjectW(
	[in] HGDIOBJ hgdiobj,  // handle to graphics object
	[in] int cbBuffer,     // size of buffer for object information
	[out] LPVOID lpvObject  // buffer for object information
);
FailOnFalse   [gle]  MoveToEx(
	[in] HDC hdc,          // handle to device context
	[in] int X,            // x-coordinate of new current position
	[in] int Y,            // y-coordinate of new current position
	[out] LPPOINT lpPoint   // old current position
);
FailOnFalse   [gle]  TextOutA(
	[in] HDC hdc,           // handle to DC
	[in] int nXStart,       // x-coordinate of starting position
	[in] int nYStart,       // y-coordinate of starting position
	[in] LPCSTR lpString,  // character string
	[in] int cbString       // number of characters
);
FailOnFalse   [gle]  TextOutW(
	[in] HDC hdc,           // handle to DC
	[in] int nXStart,       // x-coordinate of starting position
	[in] int nYStart,       // y-coordinate of starting position
	[in] LPCWSTR lpString,  // character string
	[in] int cbString       // number of characters
);
FailOnFalse   [gle]  ExtTextOutA(
	[in] HDC hdc,          // handle to DC
	[in] int X,            // x-coordinate of reference point
	[in] int Y,            // y-coordinate of reference point
	[in] _ETO fuOptions,   // text-output options
	[in] RECT *lprc, // optional dimensions
	[in] LPCSTR lpString, // string
	[in] UINT cbCount,     // number of characters in string
	[in] INT *lpDx   // array of spacing values
);

FailOnFalse   [gle]  ExtTextOutW(
	[in] HDC hdc,          // handle to DC
	[in] int X,            // x-coordinate of reference point
	[in] int Y,            // y-coordinate of reference point
	[in] _ETO fuOptions,   // text-output options
	[in] RECT *lprc, // optional dimensions
	[in] LPCWSTR lpString, // string
	[in] UINT cbCount,     // number of characters in string
	[in] INT *lpDx   // array of spacing values
);

FailOnFalse   [gle]  PolyTextOutA(
	[in] HDC hdc,                // handle to DC
	[in] POLYTEXTA *pptxt,  // array of strings
	[in] int cStrings            // number of strings in array
);
FailOnFalse   [gle]  PolyTextOutW(
	[in] HDC hdc,                // handle to DC
	[in] POLYTEXTW *pptxt,  // array of strings
	[in] int cStrings            // number of strings in array
);

HRGN  [gle] CreatePolygonRgn(
	[in] POINT *lppt,  // array of points
	[in] int cPoints,        // number of points in array
	[in] _PolyFill fnPolyFillMode  // polygon-filling mode
);
FailOnFalse   [gle]  DPtoLP(
	[in] HDC hdc,           // handle to device context
	     LPPOINT lpPoints,  // array of points
	[in] int nCount         // count of points in array
);
FailOnFalse   [gle]  LPtoDP(
	[in] HDC hdc,           // handle to device context
	     LPPOINT lpPoints,  // array of points
	[in] int nCount         // count of points in array
);
FailOnFalse   [gle]  Polygon(
	[in] HDC hdc,                // handle to DC
	[in] POINT *lpPoints,  // polygon vertices
	[in] int nCount              // count of polygon vertices
);
FailOnFalse   [gle]  Polyline(
	[in] HDC hdc,            // handle to device context
	[in] POINT *lppt,  // array of endpoints
	[in] int cPoints         // number of points in array
);

FailOnFalse   [gle]  PolyBezier(
	[in] HDC hdc,            // handle to device context
	[in] POINT *lppt,  // endpoints and control points
	[in] DWORD cPoints       // count of endpoints and control points
);
FailOnFalse  [gle]   PolyBezierTo(
	[in] HDC hdc,            // handle to device context
	[in] POINT *lppt,  // endpoints and control points
	[in] DWORD cCount        // count of endpoints and control points
);
FailOnFalse   PolylineTo(
	[in] HDC hdc,            // handle to device context
	[in] POINT *lppt,  // array of points
	[in] DWORD cCount        // number of points in array
);

FailOnFalse  [gle]   SetViewportExtEx(
	[in] HDC hdc,       // handle to device context
	[in] int nXExtent,  // new horizontal viewport extent
	[in] int nYExtent,  // new vertical viewport extent
	[out] LPSIZE lpSize  // original viewport extent
);
FailOnFalse  [gle]   SetViewportOrgEx(
	[in] HDC hdc,        // handle to device context
	[in] int X,          // new x-coordinate of viewport origin
	[in] int Y,          // new y-coordinate of viewport origin
	[out] LPPOINT lpPoint // original viewport origin
);
FailOnFalse  [gle]   SetWindowExtEx(
	[in] HDC hdc,       // handle to device context
	[in] int nXExtent,  // new horizontal window extent
	[in] int nYExtent,  // new vertical window extent
	[out] LPSIZE lpSize  // original window extent
);
FailOnFalse  [gle]   SetWindowOrgEx(
	[in] HDC hdc,        // handle to device context
	[in] int X,          // new x-coordinate of window origin
	[in] int Y,          // new y-coordinate of window origin
	[out] LPPOINT lpPoint // original window origin
);

FailOnFalse  [gle]   OffsetViewportOrgEx(
	[in] HDC hdc,         // handle to device context
	[in] int nXOffset,    // horizontal offset
	[in] int nYOffset,    // vertical offset
	[out] LPPOINT lpPoint  // original origin
);
FailOnFalse   [gle]  OffsetWindowOrgEx(
	[in] HDC hdc,          // handle to device context
	[in] int nXOffset,     // horizontal offset
	[in] int nYOffset,     // vertical offset
	[out] LPPOINT lpPoint   // original origin
);
FailOnFalse  [gle]   ScaleViewportExtEx(
	[in] HDC hdc,        // handle to device context
	[in] int Xnum,       // horizontal multiplicand
	[in] int Xdenom,     // horizontal divisor
	[in] int Ynum,       // vertical multiplicand
	[in] int Ydenom,     // vertical divisor
	[out] LPSIZE lpSize   // previous viewport extents
);
FailOnFalse  [gle]   ScaleWindowExtEx(
	[in] HDC hdc,        // handle to device context
	[in] int Xnum,       // horizontal multiplicand
	[in] int Xdenom,     // horizontal divisor
	[in] int Ynum,       // vertical multiplicand
	[in] int Ydenom,     // vertical divisor
	[out] LPSIZE lpSize   // previous window extents
);
FailOnFalse   [gle]  SetBitmapDimensionEx(
	[in] HBITMAP hBitmap,  // handle to bitmap
	[in] int nWidth,       // bitmap width in .01-mm units
	[in] int nHeight,      // bitmap height in .01-mm units
	[out] LPSIZE lpSize     // original dimensions
);
FailOnFalse   [gle]  SetBrushOrgEx(
	[in] HDC hdc,       // handle to device context
	[in] int nXOrg,     // x-coord of new origin
	[in] int nYOrg,     // y-coord of new origin
	[out] LPPOINT lppt   // points to previous brush origin
);

IntFailIfZero  [gle] GetTextFaceA(
	[in] HDC hdc,            // handle to DC
	[in] int nCount,         // length of typeface name buffer
	[out] LPSTR lpFaceName   // typeface name buffer
);
IntFailIfZero [gle]  GetTextFaceW(
	[in] HDC hdc,            // handle to DC
	[in] int nCount,         // length of typeface name buffer
	[out] LPWSTR lpFaceName   // typeface name buffer
);

DwordFailIfZero  [gle]  GetKerningPairsA(
	[in] HDC hdc,                 // handle to DC
	[in] DWORD nNumPairs,         // number of kerning pairs
	[out] LPKERNINGPAIR lpkrnpair  // array of kerning pairs
);
DwordFailIfZero  [gle]  GetKerningPairsW(
	[in] HDC hdc,                 // handle to DC
	[in] DWORD nNumPairs,         // number of kerning pairs
	[out] LPKERNINGPAIR lpkrnpair  // array of kerning pairs
);
FailOnFalse   [gle]  GetDCOrgEx(
	[in] HDC hdc,          // handle to a DC
	[out] LPPOINT lpPoint   // translation origin
);
FailOnFalse  [gle]   FixBrushOrgEx(
    HDC hdc,
    int arg1,
    int arg2,
    LPPOINT point
);
FailOnFalse   [gle]  UnrealizeObject(
	[in] HGDIOBJ hgdiobj   // handle to logical palette
);

FailOnFalse   GdiFlush();

DwordFailIfZero   [gle] GdiSetBatchLimit(
	[in] DWORD dwLimit   // batch limit
);

DwordFailIfZero [gle]  GdiGetBatchLimit();


IntFailIfZero SetICMMode(
	[in] HDC hDC,
	[in] _ICM iEnableICM
);

FailOnFalse  CheckColorsInGamut(
	[in] HDC hDC,              // device context handle
	[in] LPVOID lpRGBTriples,  // array of RGB triples
	[out] LPVOID lpBuffer,      // buffer for results
	[in] UINT nCount           // number of triples
);

HCOLORSPACE  GetColorSpace(
	[in] HDC hDC
);
FailOnFalse         GetLogColorSpaceA(
	[in] HCOLORSPACE hColorSpace,
	[out] LPLOGCOLORSPACEA lpBuffer,
	[in] DWORD nSize
);
FailOnFalse         GetLogColorSpaceW(
	[in] HCOLORSPACE hColorSpace,
	[out] LPLOGCOLORSPACEW lpBuffer,
	[in] DWORD nSize
);
HCOLORSPACE  CreateColorSpaceA(
	[in] LPLOGCOLORSPACEA lpLogColorSpace
);
HCOLORSPACE  CreateColorSpaceW(
	[in] LPLOGCOLORSPACEW lpLogColorSpace
);
HCOLORSPACE  SetColorSpace(
	[in] HDC hDC,
	[in] HCOLORSPACE hColorSpace
);

FailOnFalse         DeleteColorSpace(
	[in] HCOLORSPACE hColorSpace
);
FailOnFalse         GetICMProfileA(
	[in] HDC hDC,
	[out] LPDWORD lpcbName,
	[out] LPSTR lpszFilename
);
FailOnFalse         GetICMProfileW(
	[in] HDC hDC,
	[out] LPDWORD lpcbName,
	[out] LPWSTR lpszFilename
);
FailOnFalse         SetICMProfileA(
	[in] HDC hDC,
	[in] LPSTR lpFileName
);
FailOnFalse         SetICMProfileW(
	[in] HDC hDC,
	[in] LPWSTR lpFileName
);
FailOnFalse         GetDeviceGammaRamp(
	[in] HDC hDC,
	[out] LPVOID lpRamp
);
FailOnFalse         SetDeviceGammaRamp(
	[in] HDC hDC,
	[in] LPVOID lpRamp
);
FailOnFalse         ColorMatchToTarget(
	[in] HDC hDC,
	[in] HDC hdcTarget,
	[in] DWORD uiAction
);
int          EnumICMProfilesA(
	[in] HDC hDC,
	[in] ICMENUMPROCA lpEnumICMProfilesFunc,
	[in] LPARAM lParam
);
int          EnumICMProfilesW(
	[in] HDC hDC,
	[in] ICMENUMPROCW lpEnumICMProfilesFunc,
	[in] LPARAM lParam
);
FailOnFalse         UpdateICMRegKeyA(
	[in] DWORD dwReserved,
	[in] LPSTR lpszCMID,
	[in] LPSTR lpszFileName,
	[in] _UpdateICMRegKey nCommand
);
FailOnFalse         UpdateICMRegKeyW(
	[in] DWORD dwReserved,
	[in] LPWSTR lpszCMID,
	[in] LPWSTR lpszFileName,
	[in] _UpdateICMRegKey nCommand
);

FailOnFalse ColorCorrectPalette(
	[in] HDC hDC,
	[in] HPALETTE hPalette,
	[in] DWORD dwFirstEntry,
	[in] DWORD dwNumOfEntries
);


// OpenGL wgl prototypes
/*
FailOnFalse   wglCopyContext(HGLRC, HGLRC, UINT);
HGLRC  wglCreateContext(HDC);
HGLRC  wglCreateLayerContext(HDC, int);
FailOnFalse   wglDeleteContext(HGLRC);
HGLRC  wglGetCurrentContext();
HDC    wglGetCurrentDC();
PROC   wglGetProcAddress(LPCSTR);
FailOnFalse   wglMakeCurrent(HDC, HGLRC);
FailOnFalse   wglShareLists(HGLRC, HGLRC);
FailOnFalse   wglUseFontBitmapsA(HDC, DWORD, DWORD, DWORD);
FailOnFalse   wglUseFontBitmapsW(HDC, DWORD, DWORD, DWORD);
FailOnFalse   SwapBuffers(HDC);

FailOnFalse   wglUseFontOutlinesA(HDC, DWORD, DWORD, DWORD, FLOAT,
                                         FLOAT, int, LPGLYPHMETRICSFLOAT);
FailOnFalse   wglUseFontOutlinesW(HDC, DWORD, DWORD, DWORD, FLOAT,
FailOnFalse   wglDescribeLayerPlane(HDC, int, int, UINT,
                                           LPLAYERPLANEDESCRIPTOR);
IntFailIfZero    wglSetLayerPaletteEntries(HDC, int, int, int,
                                                COLORREF *);
IntFailIfZero    wglGetLayerPaletteEntries(HDC, int, int, int,
                                               COLORREF *);
FailOnFalse   wglRealizeLayerPalette(HDC, int, BOOL);
FailOnFalse   wglSwapLayerBuffers(HDC, UINT);


FailOnFalse  wglSwapMultipleBuffers(UINT,  WGLSWAP *);

*/
