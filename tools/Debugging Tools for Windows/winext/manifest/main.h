// ---------------------------------------------------------------------------
// LogExts API Logging Manifest
//
// Created  05-Jan-2000     markder
// ---------------------------------------------------------------------------



// ---------------------------------------------------------------------------
//
// Built-In Types
//
// VOID                 -- 0 bytes
// BYTE                 -- 1 byte   (hex)
// WORD                 -- 2 bytes  (hex)
// DWORD                -- 4 bytes  (hex)
// LONG                 -- 4 bytes  (dec)
// BOOL                 -- Boolean
// FLOAT                -- Float
// DOUBLE               -- Double
// CHAR                 -- Character
// WCHAR                -- Wide character
// LPSTR                -- ANSI string
// LPWSTR               -- Wide character string
// GUID                 -- Globally Unique Identifier
// COM_INTERFACE_PTR    -- COM interface pointer
// SIZE_T               -- 32-Bit on x86/64-Bit on x64
//
// ---------------------------------------------------------------------------

// -------------------------------------- Basic type definitions
typedef SIZE_T          int;
typedef LONG            long;
typedef CHAR            char;
typedef WORD            short;
typedef LONG            INT;
typedef LONG            UINT;
typedef LPSTR           LPCSTR;
typedef LPSTR           *PSTR;
typedef LPWSTR          LPCWSTR;
typedef WORD            USHORT;
typedef WORD            *LPWORD;
typedef DWORD           ULONG;
typedef SIZE_T          LPVOID;
typedef LPVOID          PVOID;
typedef LPVOID          LPCVOID;
typedef BYTE            *LPBYTE;
typedef BYTE            *PBYTE;
typedef BOOL            *PBOOL;
typedef BOOL            *LPBOOL;
typedef DWORD           *LPDWORD;
typedef DWORD           *PDWORD;
typedef LONG            *LPLONG;
typedef LONG            *PLONG;
typedef LONG            *PULONG;
typedef BYTE            BOOLEAN;
typedef DWORD           HINF;
typedef FLOAT           float;
typedef VOID            void;

// -------------------------------------- Memory management definitions

value SIZE_T HANDLE
{
#define NULL                       0 [fail]
#define INVALID_HANDLE_VALUE      -1 [fail]
};

typedef HANDLE          HGLOBAL;
typedef HANDLE          HLOCAL;

// -------------------------------------- Windows definitions
typedef DWORD           ATOM;
typedef GUID            *LPGUID;
typedef GUID            *PGUID;
typedef HANDLE          HDEVNOTIFY;
typedef HANDLE          HDEVINFO;
typedef HANDLE          *LPHANDLE;
typedef HANDLE          HMETAFILE;
typedef DWORD           WPARAM;
typedef LPVOID          LPARAM;
typedef HANDLE          HKEY;
typedef HKEY            *PHKEY;
typedef LPVOID          LPTOP_LEVEL_EXCEPTION_FILTER;
typedef LPVOID          LPTHREAD_START_ROUTINE;
typedef LPVOID          LPFIBER_START_ROUTINE;
typedef DWORD           LRESULT;
typedef LPVOID          HOOKPROC;
typedef LPVOID          FARPROC;
typedef LPVOID          LPOVERLAPPED;

// -------------------------------------- 64-bit definitions
typedef SIZE_T          __int64;
typedef LPVOID          UINT_PTR;
typedef LPVOID          ULONG_PTR;
typedef LPVOID          DWORD_PTR;
typedef LPVOID          PULONG_PTR;

//
// Include winerror.h for the definition of HRESULT
//
#include "winerror.h"

// -------------------------------------- COM definitions
typedef HRESULT         STDAPI;
typedef LPWSTR	        BSTR;
typedef DWORD	        LCID;
typedef WCHAR	        LPOLECHAR;
typedef LPWSTR	        LPOLESTR;
typedef LPWSTR	        LPCOLESTR;
typedef DWORD	        DISPID;
typedef DWORD	        DISPPARAMS;
typedef DWORD	        VARIANT;
typedef DWORD	        EXCEPINFO;
typedef GUID	        *REFGUID;
typedef GUID	        *REFCLSID;
typedef GUID	        *REFIID;


typedef struct _CLSID
{
    DWORD x;
    WORD  s1;
    WORD  s2;
    BYTE  c[8];
} CLSID;


value DWORD ThreadId
{
#define NULL                    0  [fail]
};

value DWORD ProcessId
{
#define NULL                    0  [fail]
};


typedef struct _LONGLONG
{
    DWORD   Low;
    LONG   High;
} LONGLONG;


typedef struct _ULONGLONG
{
    DWORD   Low;
    DWORD   High;
} ULONGLONG;

typedef struct _ULARGE_INTEGER
{
    DWORD   Low;
    DWORD   High;
} ULARGE_INTEGER,*PULARGE_INTEGER;

typedef struct _LARGE_INTEGER
{
    ULONG   Low;
    LONG    High;
} LARGE_INTEGER,*PLARGE_INTEGER;

typedef struct _DWORDLONG
{
    DWORD   Low;
    DWORD   High;
} DWORDLONG;

typedef LPVOID          PVOID64;

typedef WORD        SHORT;
typedef int        *LPINT;
typedef float      *LPFLOAT;
typedef float      *PFLOAT;

// -------------------------------------- Basic error definitions
value BOOL FailOnFalse
{
#define FALSE                   0 [fail]
#define TRUE                    1
};

value LONG IntFailIfNeg1
{
#define NEGATIVE_ONE              -1 [fail]
};

value LONG IntFailIfZero
{
#define ZERO                      0 [fail]
};

value LONG LongFailIfNeg1
{
#define NEGATIVE_ONE              -1 [fail]
};

value LONG LongFailIfZero
{
#define ZERO                       0 [fail]
};

value WORD WordFailIfZero
{
#define ZERO                       0 [fail]
};

value DWORD DwordFailIfZero
{
#define ZERO                       0 [fail]
};

value LPVOID HMODULE
{
#define NULL                       0 [fail]
};

value LPVOID HINSTANCE
{
#define NULL                       0 [fail]
};

value DWORD UintFailIfZero
{
#define ZERO                       0 [fail]
};


value DWORD DwordFailIf0xFFFFFFFF
{
#define _0xFFFFFFFF                0xFFFFFFFF [fail]
};

value DWORD DwordFailIfNeg1
{
#define NEGATIVE_ONE              -1 [fail]
};

value LPSTR LpstrFailIfNull
{
#define NULL                       0 [fail]
};

value LPWSTR LpwstrFailIfNull
{
#define NULL                       0 [fail]
};

value LPVOID LpvoidFailIfNull
{
#define NULL                       0 [fail]
};

typedef HANDLE  *PHANDLE;
typedef HANDLE  HFILE;

value HANDLE HWND
{
#define NULL                       0  [fail]
#define HWND_BOTTOM                1
#define HWND_TOPMOST              -1
#define HWND_NOTOPMOST            -2
};

value int SpoolerError
{
/* Spooler Error Codes */
#define SP_NOTREPORTED               0x4000 [fail]
#define SP_ZERO                       0  [fail]
#define SP_ERROR                     -1 [fail]
#define SP_APPABORT                  -2 [fail]
#define SP_USERABORT                 -3 [fail]
#define SP_OUTOFDISK                 -4 [fail]
#define SP_OUTOFMEMORY               -5 [fail]
};

value LONG UnhandledException
{
#define EXCEPTION_EXECUTE_HANDLER       1
#define EXCEPTION_CONTINUE_SEARCH       0
};

mask DWORD FileFlagsAndAttributes
{
#define FILE_ATTRIBUTE_READONLY             0x00000001
#define FILE_ATTRIBUTE_HIDDEN               0x00000002
#define FILE_ATTRIBUTE_SYSTEM               0x00000004
#define FILE_ATTRIBUTE_DIRECTORY            0x00000010
#define FILE_ATTRIBUTE_ARCHIVE              0x00000020
#define FILE_ATTRIBUTE_ENCRYPTED            0x00000040
#define FILE_ATTRIBUTE_NORMAL               0x00000080
#define FILE_ATTRIBUTE_TEMPORARY            0x00000100
#define FILE_ATTRIBUTE_SPARSE_FILE          0x00000200
#define FILE_ATTRIBUTE_REPARSE_POINT        0x00000400
#define FILE_ATTRIBUTE_COMPRESSED           0x00000800
#define FILE_ATTRIBUTE_OFFLINE              0x00001000
#define FILE_ATTRIBUTE_NOT_CONTENT_INDEXED  0x00002000
};

typedef DWORD FTime;

alias FTime;

typedef struct _FILETIME {
    FTime dwLowDateTime;
    FTime dwHighDateTime;
} FILETIME, *LPFILETIME, *PFILETIME;

typedef struct _STARTUPINFOA {
    DWORD    cb;
    FILLER64 align1[4];
    LPSTR    lpReserved;
    LPSTR    lpDesktop;
    LPSTR    lpTitle;
    DWORD    dwX;
    DWORD    dwY;
    DWORD    dwXSize;
    DWORD    dwYSize;
    DWORD    dwXCountChars;
    DWORD    dwYCountChars;
    DWORD    dwFillAttribute;
    DWORD    dwFlags;
    WORD     wShowWindow;
    WORD     cbReserved2;
    FILLER64 align2[4];
    LPBYTE   lpReserved2;
    HANDLE   hStdInput;
    HANDLE   hStdOutput;
    HANDLE   hStdError;
} STARTUPINFOA, *LPSTARTUPINFOA;

typedef struct _STARTUPINFOW {
    DWORD    cb;
    FILLER64 align1[4];
    LPWSTR   lpReserved;
    LPWSTR   lpDesktop;
    LPWSTR   lpTitle;
    DWORD    dwX;
    DWORD    dwY;
    DWORD    dwXSize;
    DWORD    dwYSize;
    DWORD    dwXCountChars;
    DWORD    dwYCountChars;
    DWORD    dwFillAttribute;
    DWORD    dwFlags;
    WORD     wShowWindow;
    WORD     cbReserved2;
    FILLER64 align2[4];
    LPBYTE   lpReserved2;
    HANDLE   hStdInput;
    HANDLE   hStdOutput;
    HANDLE   hStdError;
} STARTUPINFOW, *LPSTARTUPINFOW;

typedef struct _PROCESS_INFORMATION {
    HANDLE    hProcess;
    HANDLE    hThread;
    ProcessId dwProcessId;
    ThreadId  dwThreadId;
} PROCESS_INFORMATION, *LPPROCESS_INFORMATION;

typedef struct _WIN32_FIND_DATAW {
    FileFlagsAndAttributes dwFileAttributes;
    FILETIME ftCreationTime;
    FILETIME ftLastAccessTime;
    FILETIME ftLastWriteTime;
    DWORD    nFileSizeHigh;
    DWORD    nFileSizeLow;
    DWORD    dwReserved0;
    DWORD    dwReserved1;
    WCHAR    cFileName[ 260 ];
    WCHAR    cAlternateFileName[ 14 ];
} WIN32_FIND_DATAW, *LPWIN32_FIND_DATAW;

typedef struct _WIN32_FIND_DATAA {
    FileFlagsAndAttributes dwFileAttributes;
    FILETIME ftCreationTime;
    FILETIME ftLastAccessTime;
    FILETIME ftLastWriteTime;
    DWORD    nFileSizeHigh;
    DWORD    nFileSizeLow;
    DWORD    dwReserved0;
    DWORD    dwReserved1;
    CHAR     cFileName[ 260 ];
    CHAR     cAlternateFileName[ 14 ];
} WIN32_FIND_DATAA, *LPWIN32_FIND_DATAA;

typedef struct tagPOINT {
  LONG x;
  LONG y;
} POINT, *PPOINT, *LPPOINT;

typedef struct _RECT {
  LONG left;
  LONG top;
  LONG right;
  LONG bottom;
} RECT, *PRECT, *LPRECT, *LPCRECT, *LPCRECTL;

typedef struct tagPALETTEENTRY {
  BYTE peRed;
  BYTE peGreen;
  BYTE peBlue;
  BYTE peFlags;
} PALETTEENTRY, *LPPALETTEENTRY;

typedef struct tagSIZE {
  LONG cx;
  LONG cy;
} SIZE, *PSIZE, *LPSIZE;

typedef struct  _RGNDATAHEADER
    {
    DWORD dwSize;
    DWORD iType;
    DWORD nCount;
    DWORD nRgnSize;
    RECT rcBound;
    }   RGNDATAHEADER;

typedef struct _RGNDATA {
    RGNDATAHEADER rdh;
    char          Buffer[1];
} RGNDATA, *PRGNDATA, *LPRGNDATA;

typedef struct _RECTL
{
    LONG    left;
    LONG    top;
    LONG    right;
    LONG    bottom;
} RECTL, *PRECTL, *LPRECTL;
typedef struct  tagSIZEL
    {
    LONG cx;
    LONG cy;
    }   SIZEL;
typedef SIZEL *PSIZEL;
typedef SIZEL *LPSIZEL;

value HANDLE HDC
{
#define NULL                0  [fail]
};

value HANDLE HRGN
{
#define ERROR               0  [fail]
#define NULLREGION          1
#define SIMPLEREGION        2
#define COMPLEXREGION       3
};

value HANDLE HMENU
{
#define NULL                    0  [fail]
};

value HANDLE HRSRC
{
#define NULL                    0  [fail]
};

value HANDLE HACCEL
{
#define NULL                    0  [fail]
};

value LPVOID WNDPROC
{
#define NULL                    0  [fail]
};

value HANDLE HKL
{
#define NULL                    0  [fail]
#define HKL_NEXT                1
};

value HANDLE HHOOK
{
#define NULL                    0  [fail]
};


value DWORD WaitReturnValues
{
#define WAIT_OBJECT_0                  0x00000000
#define WAIT_OBJECT_1                  0x00000001
#define WAIT_OBJECT_2                  0x00000002
#define WAIT_OBJECT_3                  0x00000003
#define WAIT_OBJECT_4                  0x00000004
#define WAIT_OBJECT_5                  0x00000005
#define WAIT_OBJECT_6                  0x00000006
#define WAIT_OBJECT_7                  0x00000007
#define WAIT_OBJECT_8                  0x00000008
#define WAIT_OBJECT_9                  0x00000009
#define WAIT_OBJECT_10                 0x0000000A
#define WAIT_OBJECT_11                 0x0000000B
#define WAIT_OBJECT_12                 0x0000000C
#define WAIT_ABANDONED_0               0x00000080
#define WAIT_ABANDONED_1               0x00000081
#define WAIT_ABANDONED_2               0x00000082
#define WAIT_ABANDONED_3               0x00000083
#define WAIT_ABANDONED_4               0x00000084
#define WAIT_ABANDONED_5               0x00000085
#define WAIT_ABANDONED_6               0x00000086
#define WAIT_ABANDONED_7               0x00000087
#define WAIT_ABANDONED_8               0x00000088
#define WAIT_ABANDONED_9               0x00000089
#define WAIT_ABANDONED_10              0x0000008A
#define WAIT_ABANDONED_11              0x0000008B
#define WAIT_ABANDONED_12              0x0000008C
#define WAIT_IO_COMPLETION             0x000000C0
#define WAIT_TIMEOUT                   0x00000102
#define WAIT_FAILED                    0xFFFFFFFF [fail]
};

value HANDLE HDESK
{
#define NULL                    0  [fail]
};

value HANDLE HWINSTA
{
#define NULL                    0  [fail]
};

value HANDLE HMONITOR
{
#define NULL                    0  [fail]
};

value HANDLE HPRINTER
{
#define NULL                    0  [fail]
};

typedef DWORD ACCESS_MASK;


typedef struct _SECURITY_ATTRIBUTES {
    DWORD  nLength;
    LPVOID lpSecurityDescriptor;
    BOOL   bInheritHandle;
} SECURITY_ATTRIBUTES, *LPSECURITY_ATTRIBUTES;




mask DWORD DMFieldFlags
{
/* field selection bits */
#define DM_ORIENTATION      0x00000001L
#define DM_PAPERSIZE        0x00000002L
#define DM_PAPERLENGTH      0x00000004L
#define DM_PAPERWIDTH       0x00000008L
#define DM_SCALE            0x00000010L
#define DM_POSITION         0x00000020L
#define DM_NUP              0x00000040L
#define DM_COPIES           0x00000100L
#define DM_DEFAULTSOURCE    0x00000200L
#define DM_PRINTQUALITY     0x00000400L
#define DM_COLOR            0x00000800L
#define DM_DUPLEX           0x00001000L
#define DM_YRESOLUTION      0x00002000L
#define DM_TTOPTION         0x00004000L
#define DM_COLLATE          0x00008000L
#define DM_FORMNAME         0x00010000L
#define DM_LOGPIXELS        0x00020000L
#define DM_BITSPERPEL       0x00040000L
#define DM_PELSWIDTH        0x00080000L
#define DM_PELSHEIGHT       0x00100000L
#define DM_DISPLAYFLAGS     0x00200000L
#define DM_DISPLAYFREQUENCY 0x00400000L
#define DM_ICMMETHOD        0x00800000L
#define DM_ICMINTENT        0x01000000L
#define DM_MEDIATYPE        0x02000000L
#define DM_DITHERTYPE       0x04000000L
#define DM_PANNINGWIDTH     0x08000000L
#define DM_PANNINGHEIGHT    0x10000000L
};

value WORD DMBinValues
{
/* bin selections */
#define DMBIN_UPPER         1
#define DMBIN_ONLYONE       1
#define DMBIN_LOWER         2
#define DMBIN_MIDDLE        3
#define DMBIN_MANUAL        4
#define DMBIN_ENVELOPE      5
#define DMBIN_ENVMANUAL     6
#define DMBIN_AUTO          7
#define DMBIN_TRACTOR       8
#define DMBIN_SMALLFMT      9
#define DMBIN_LARGEFMT      10
#define DMBIN_LARGECAPACITY 11
#define DMBIN_CASSETTE      14
#define DMBIN_FORMSOURCE    15
#define DMBIN_USER          256     /* device specific bins start here */
};


value WORD DMPrintQualityValues
{
/* print qualities */
#define DMRES_DRAFT         -1
#define DMRES_LOW           -2
#define DMRES_MEDIUM        -3
#define DMRES_HIGH          -4
};

value WORD DMColorEnableValues
{
/* color enable/disable for color printers */
#define DMCOLOR_MONOCHROME  1
#define DMCOLOR_COLOR       2
};

value WORD DMDuplexValues
{
/* duplex enable */
#define DMDUP_SIMPLEX    1
#define DMDUP_VERTICAL   2
#define DMDUP_HORIZONTAL 3
};

value WORD DMTrueTypeValues
{
/* TrueType options */
#define DMTT_BITMAP     1       /* print TT fonts as graphics */
#define DMTT_DOWNLOAD   2       /* download TT fonts as soft fonts */
#define DMTT_SUBDEV     3       /* substitute device fonts for TT fonts */
#define DMTT_DOWNLOAD_OUTLINE 4 /* download TT fonts as outline soft fonts */
};

value WORD DMCollateValues
{
/* Collation selections */
#define DMCOLLATE_FALSE  0
#define DMCOLLATE_TRUE   1
};

/* DEVMODE dmDisplayFlags flags */

value DWORD DMDisplayFlagFields
{
#define DM_GRAYSCALE            0x00000001 /* This flag is no longer valid */
#define DM_INTERLACED           0x00000002 /* This flag is no longer valid */
#define DMDISPLAYFLAGS_TEXTMODE 0x00000004
};

value DWORD DMLogicalPageValues
{
/* dmNup , multiple logical page per physical page options */
#define DMNUP_SYSTEM        1
#define DMNUP_ONEUP         2
};

value DWORD DMICMMethodValues
{
/* ICM methods */
#define DMICMMETHOD_NONE    1   /* ICM disabled */
#define DMICMMETHOD_SYSTEM  2   /* ICM handled by system */
#define DMICMMETHOD_DRIVER  3   /* ICM handled by driver */
#define DMICMMETHOD_DEVICE  4   /* ICM handled by device */

#define DMICMMETHOD_USER  256   /* Device-specific methods start here */
};
value DWORD DMICMIntentsValues
{
/* ICM Intents */
#define DMICM_SATURATE          1   /* Maximize color saturation */
#define DMICM_CONTRAST          2   /* Maximize color contrast */
#define DMICM_COLORIMETRIC       3   /* Use specific color metric */
#define DMICM_ABS_COLORIMETRIC   4   /* Use specific color metric */

#define DMICM_USER        256   /* Device-specific intents start here */
};
value DWORD DMMediaTypeValues
{
/* Media types */
#define DMMEDIA_STANDARD      1   /* Standard paper */
#define DMMEDIA_TRANSPARENCY  2   /* Transparency */
#define DMMEDIA_GLOSSY        3   /* Glossy paper */

#define DMMEDIA_USER        256   /* Device-specific media start here */
};
value DWORD DMDitherTypeValues
{
/* Dither types */
#define DMDITHER_NONE       1      /* No dithering */
#define DMDITHER_COARSE     2      /* Dither with a coarse brush */
#define DMDITHER_FINE       3      /* Dither with a fine brush */
#define DMDITHER_LINEART    4      /* LineArt dithering */
#define DMDITHER_ERRORDIFFUSION 5  /* LineArt dithering */
#define DMDITHER_RESERVED6      6      /* LineArt dithering */
#define DMDITHER_RESERVED7      7      /* LineArt dithering */
#define DMDITHER_RESERVED8      8      /* LineArt dithering */
#define DMDITHER_RESERVED9      9      /* LineArt dithering */
#define DMDITHER_GRAYSCALE  10     /* Device does grayscaling */

#define DMDITHER_USER     256   /* Device-specific dithers start here */
};


value WORD DMOrientationValues
{
/* orientation selections */
#define DMORIENT_PORTRAIT   1
#define DMORIENT_LANDSCAPE  2
};

value WORD DMPaperSelectionValues
{
/* paper selections */
#define DMPAPER_LETTER               1  /* Letter 8 1/2 x 11 in               */
#define DMPAPER_LETTERSMALL          2  /* Letter Small 8 1/2 x 11 in         */
#define DMPAPER_TABLOID              3  /* Tabloid 11 x 17 in                 */
#define DMPAPER_LEDGER               4  /* Ledger 17 x 11 in                  */
#define DMPAPER_LEGAL                5  /* Legal 8 1/2 x 14 in                */
#define DMPAPER_STATEMENT            6  /* Statement 5 1/2 x 8 1/2 in         */
#define DMPAPER_EXECUTIVE            7  /* Executive 7 1/4 x 10 1/2 in        */
#define DMPAPER_A3                   8  /* A3 297 x 420 mm                    */
#define DMPAPER_A4                   9  /* A4 210 x 297 mm                    */
#define DMPAPER_A4SMALL             10  /* A4 Small 210 x 297 mm              */
#define DMPAPER_A5                  11  /* A5 148 x 210 mm                    */
#define DMPAPER_B4                  12  /* B4 (JIS) 250 x 354                 */
#define DMPAPER_B5                  13  /* B5 (JIS) 182 x 257 mm              */
#define DMPAPER_FOLIO               14  /* Folio 8 1/2 x 13 in                */
#define DMPAPER_QUARTO              15  /* Quarto 215 x 275 mm                */
#define DMPAPER_10X14               16  /* 10x14 in                           */
#define DMPAPER_11X17               17  /* 11x17 in                           */
#define DMPAPER_NOTE                18  /* Note 8 1/2 x 11 in                 */
#define DMPAPER_ENV_9               19  /* Envelope #9 3 7/8 x 8 7/8          */
#define DMPAPER_ENV_10              20  /* Envelope #10 4 1/8 x 9 1/2         */
#define DMPAPER_ENV_11              21  /* Envelope #11 4 1/2 x 10 3/8        */
#define DMPAPER_ENV_12              22  /* Envelope #12 4 \276 x 11           */
#define DMPAPER_ENV_14              23  /* Envelope #14 5 x 11 1/2            */
#define DMPAPER_CSHEET              24  /* C size sheet                       */
#define DMPAPER_DSHEET              25  /* D size sheet                       */
#define DMPAPER_ESHEET              26  /* E size sheet                       */
#define DMPAPER_ENV_DL              27  /* Envelope DL 110 x 220mm            */
#define DMPAPER_ENV_C5              28  /* Envelope C5 162 x 229 mm           */
#define DMPAPER_ENV_C3              29  /* Envelope C3  324 x 458 mm          */
#define DMPAPER_ENV_C4              30  /* Envelope C4  229 x 324 mm          */
#define DMPAPER_ENV_C6              31  /* Envelope C6  114 x 162 mm          */
#define DMPAPER_ENV_C65             32  /* Envelope C65 114 x 229 mm          */
#define DMPAPER_ENV_B4              33  /* Envelope B4  250 x 353 mm          */
#define DMPAPER_ENV_B5              34  /* Envelope B5  176 x 250 mm          */
#define DMPAPER_ENV_B6              35  /* Envelope B6  176 x 125 mm          */
#define DMPAPER_ENV_ITALY           36  /* Envelope 110 x 230 mm              */
#define DMPAPER_ENV_MONARCH         37  /* Envelope Monarch 3.875 x 7.5 in    */
#define DMPAPER_ENV_PERSONAL        38  /* 6 3/4 Envelope 3 5/8 x 6 1/2 in    */
#define DMPAPER_FANFOLD_US          39  /* US Std Fanfold 14 7/8 x 11 in      */
#define DMPAPER_FANFOLD_STD_GERMAN  40  /* German Std Fanfold 8 1/2 x 12 in   */
#define DMPAPER_FANFOLD_LGL_GERMAN  41  /* German Legal Fanfold 8 1/2 x 13 in */
#define DMPAPER_ISO_B4              42  /* B4 (ISO) 250 x 353 mm              */
#define DMPAPER_JAPANESE_POSTCARD   43  /* Japanese Postcard 100 x 148 mm     */
#define DMPAPER_9X11                44  /* 9 x 11 in                          */
#define DMPAPER_10X11               45  /* 10 x 11 in                         */
#define DMPAPER_15X11               46  /* 15 x 11 in                         */
#define DMPAPER_ENV_INVITE          47  /* Envelope Invite 220 x 220 mm       */
#define DMPAPER_RESERVED_48         48  /* RESERVED--DO NOT USE               */
#define DMPAPER_RESERVED_49         49  /* RESERVED--DO NOT USE               */
#define DMPAPER_LETTER_EXTRA        50  /* Letter Extra 9 \275 x 12 in        */
#define DMPAPER_LEGAL_EXTRA         51  /* Legal Extra 9 \275 x 15 in         */
#define DMPAPER_TABLOID_EXTRA       52  /* Tabloid Extra 11.69 x 18 in        */
#define DMPAPER_A4_EXTRA            53  /* A4 Extra 9.27 x 12.69 in           */
#define DMPAPER_LETTER_TRANSVERSE   54  /* Letter Transverse 8 \275 x 11 in   */
#define DMPAPER_A4_TRANSVERSE       55  /* A4 Transverse 210 x 297 mm         */
#define DMPAPER_LETTER_EXTRA_TRANSVERSE 56 /* Letter Extra Transverse 9\275 x 12 in */
#define DMPAPER_A_PLUS              57  /* SuperA/SuperA/A4 227 x 356 mm      */
#define DMPAPER_B_PLUS              58  /* SuperB/SuperB/A3 305 x 487 mm      */
#define DMPAPER_LETTER_PLUS         59  /* Letter Plus 8.5 x 12.69 in         */
#define DMPAPER_A4_PLUS             60  /* A4 Plus 210 x 330 mm               */
#define DMPAPER_A5_TRANSVERSE       61  /* A5 Transverse 148 x 210 mm         */
#define DMPAPER_B5_TRANSVERSE       62  /* B5 (JIS) Transverse 182 x 257 mm   */
#define DMPAPER_A3_EXTRA            63  /* A3 Extra 322 x 445 mm              */
#define DMPAPER_A5_EXTRA            64  /* A5 Extra 174 x 235 mm              */
#define DMPAPER_B5_EXTRA            65  /* B5 (ISO) Extra 201 x 276 mm        */
#define DMPAPER_A2                  66  /* A2 420 x 594 mm                    */
#define DMPAPER_A3_TRANSVERSE       67  /* A3 Transverse 297 x 420 mm         */
#define DMPAPER_A3_EXTRA_TRANSVERSE 68  /* A3 Extra Transverse 322 x 445 mm   */

#define DMPAPER_DBL_JAPANESE_POSTCARD 69 /* Japanese Double Postcard 200 x 148 mm */
#define DMPAPER_A6                  70  /* A6 105 x 148 mm                 */
#define DMPAPER_JENV_KAKU2          71  /* Japanese Envelope Kaku #2       */
#define DMPAPER_JENV_KAKU3          72  /* Japanese Envelope Kaku #3       */
#define DMPAPER_JENV_CHOU3          73  /* Japanese Envelope Chou #3       */
#define DMPAPER_JENV_CHOU4          74  /* Japanese Envelope Chou #4       */
#define DMPAPER_LETTER_ROTATED      75  /* Letter Rotated 11 x 8 1/2 11 in */
#define DMPAPER_A3_ROTATED          76  /* A3 Rotated 420 x 297 mm         */
#define DMPAPER_A4_ROTATED          77  /* A4 Rotated 297 x 210 mm         */
#define DMPAPER_A5_ROTATED          78  /* A5 Rotated 210 x 148 mm         */
#define DMPAPER_B4_JIS_ROTATED      79  /* B4 (JIS) Rotated 364 x 257 mm   */
#define DMPAPER_B5_JIS_ROTATED      80  /* B5 (JIS) Rotated 257 x 182 mm   */
#define DMPAPER_JAPANESE_POSTCARD_ROTATED 81 /* Japanese Postcard Rotated 148 x 100 mm */
#define DMPAPER_DBL_JAPANESE_POSTCARD_ROTATED 82 /* Double Japanese Postcard Rotated 148 x 200 mm */
#define DMPAPER_A6_ROTATED          83  /* A6 Rotated 148 x 105 mm         */
#define DMPAPER_JENV_KAKU2_ROTATED  84  /* Japanese Envelope Kaku #2 Rotated */
#define DMPAPER_JENV_KAKU3_ROTATED  85  /* Japanese Envelope Kaku #3 Rotated */
#define DMPAPER_JENV_CHOU3_ROTATED  86  /* Japanese Envelope Chou #3 Rotated */
#define DMPAPER_JENV_CHOU4_ROTATED  87  /* Japanese Envelope Chou #4 Rotated */
#define DMPAPER_B6_JIS              88  /* B6 (JIS) 128 x 182 mm           */
#define DMPAPER_B6_JIS_ROTATED      89  /* B6 (JIS) Rotated 182 x 128 mm   */
#define DMPAPER_12X11               90  /* 12 x 11 in                      */
#define DMPAPER_JENV_YOU4           91  /* Japanese Envelope You #4        */
#define DMPAPER_JENV_YOU4_ROTATED   92  /* Japanese Envelope You #4 Rotated*/
#define DMPAPER_P16K                93  /* PRC 16K 146 x 215 mm            */
#define DMPAPER_P32K                94  /* PRC 32K 97 x 151 mm             */
#define DMPAPER_P32KBIG             95  /* PRC 32K(Big) 97 x 151 mm        */
#define DMPAPER_PENV_1              96  /* PRC Envelope #1 102 x 165 mm    */
#define DMPAPER_PENV_2              97  /* PRC Envelope #2 102 x 176 mm    */
#define DMPAPER_PENV_3              98  /* PRC Envelope #3 125 x 176 mm    */
#define DMPAPER_PENV_4              99  /* PRC Envelope #4 110 x 208 mm    */
#define DMPAPER_PENV_5              100 /* PRC Envelope #5 110 x 220 mm    */
#define DMPAPER_PENV_6              101 /* PRC Envelope #6 120 x 230 mm    */
#define DMPAPER_PENV_7              102 /* PRC Envelope #7 160 x 230 mm    */
#define DMPAPER_PENV_8              103 /* PRC Envelope #8 120 x 309 mm    */
#define DMPAPER_PENV_9              104 /* PRC Envelope #9 229 x 324 mm    */
#define DMPAPER_PENV_10             105 /* PRC Envelope #10 324 x 458 mm   */
#define DMPAPER_P16K_ROTATED        106 /* PRC 16K Rotated                 */
#define DMPAPER_P32K_ROTATED        107 /* PRC 32K Rotated                 */
#define DMPAPER_P32KBIG_ROTATED     108 /* PRC 32K(Big) Rotated            */
#define DMPAPER_PENV_1_ROTATED      109 /* PRC Envelope #1 Rotated 165 x 102 mm */
#define DMPAPER_PENV_2_ROTATED      110 /* PRC Envelope #2 Rotated 176 x 102 mm */
#define DMPAPER_PENV_3_ROTATED      111 /* PRC Envelope #3 Rotated 176 x 125 mm */
#define DMPAPER_PENV_4_ROTATED      112 /* PRC Envelope #4 Rotated 208 x 110 mm */
#define DMPAPER_PENV_5_ROTATED      113 /* PRC Envelope #5 Rotated 220 x 110 mm */
#define DMPAPER_PENV_6_ROTATED      114 /* PRC Envelope #6 Rotated 230 x 120 mm */
#define DMPAPER_PENV_7_ROTATED      115 /* PRC Envelope #7 Rotated 230 x 160 mm */
#define DMPAPER_PENV_8_ROTATED      116 /* PRC Envelope #8 Rotated 309 x 120 mm */
#define DMPAPER_PENV_9_ROTATED      117 /* PRC Envelope #9 Rotated 324 x 229 mm */
#define DMPAPER_PENV_10_ROTATED     118 /* PRC Envelope #10 Rotated 458 x 324 mm */

#define DMPAPER_USER                256
};


/* size of a device name string */
//#define CCHDEVICENAME 32

/* size of a form name string */
//#define CCHFORMNAME 32

typedef struct _devicemodeA {
    BYTE   dmDeviceName[/*CCHDEVICENAME */ 32];
    WORD dmSpecVersion;
    WORD dmDriverVersion;
    WORD dmSize;
    WORD dmDriverExtra;
    DMFieldFlags dmFields;
//    union {
//      struct {
        DMOrientationValues dmOrientation;
        DMPaperSelectionValues dmPaperSize;
        WORD dmPaperLength;
        WORD dmPaperWidth;
      //};
//      POINTL dmPosition;
//    };
    WORD dmScale;
    WORD dmCopies;
    DMBinValues dmDefaultSource;
    DMPrintQualityValues dmPrintQuality;
    DMColorEnableValues dmColor;
    DMDuplexValues dmDuplex;
    WORD dmYResolution;
    DMTrueTypeValues dmTTOption;
    DMCollateValues dmCollate;
    BYTE   dmFormName[/*CCHFORMNAME */ 32];
    WORD   dmLogPixels;
    DWORD  dmBitsPerPel;
    DWORD  dmPelsWidth;
    DWORD  dmPelsHeight;
    DMDisplayFlagFields  dmDisplayFlags_OR_Nup;
    DWORD  dmDisplayFrequency;
    DMICMMethodValues  dmICMMethod;
    DMICMIntentsValues  dmICMIntent;
    DMMediaTypeValues  dmMediaType;
    DMDitherTypeValues  dmDitherType;
    DWORD  dmReserved1;
    DWORD  dmReserved2;
    DWORD  dmPanningWidth;
    DWORD  dmPanningHeight;
} DEVMODEA, *PDEVMODEA, *NPDEVMODEA, *LPDEVMODEA;

typedef struct _devicemodeW {
    WCHAR  dmDeviceName[/*CCHDEVICENAME */ 32];
    WORD dmSpecVersion;
    WORD dmDriverVersion;
    WORD dmSize;
    WORD dmDriverExtra;
    DMFieldFlags dmFields;
//    union {
//      struct {
        DMOrientationValues dmOrientation;
        DMPaperSelectionValues dmPaperSize;
        WORD dmPaperLength;
        WORD dmPaperWidth;
//      };
//      POINTL dmPosition;
//    };
    WORD dmScale;
    WORD dmCopies;
    DMBinValues dmDefaultSource;
    DMPrintQualityValues dmPrintQuality;
    DMColorEnableValues dmColor;
    DMDuplexValues dmDuplex;
    WORD dmYResolution;
    DMTrueTypeValues dmTTOption;
    DMCollateValues dmCollate;
    WCHAR   dmFormName[/*CCHFORMNAME */ 32];
    WORD   dmLogPixels;
    DWORD  dmBitsPerPel;
    DWORD  dmPelsWidth;
    DWORD  dmPelsHeight;
    DMDisplayFlagFields  dmDisplayFlags_OR_Nup;
    DWORD  dmDisplayFrequency;
    DMICMMethodValues  dmICMMethod;
    DMICMIntentsValues  dmICMIntent;
    DMMediaTypeValues  dmMediaType;
    DMDitherTypeValues  dmDitherType;
    DWORD  dmReserved1;
    DWORD  dmReserved2;
    DWORD  dmPanningWidth;
    DWORD  dmPanningHeight;
} DEVMODEW, *PDEVMODEW, *NPDEVMODEW, *LPDEVMODEW;





value LONG GdiEscapeCode
{
#define NEWFRAME                     1
#define ABORTDOC                     2
#define NEXTBAND                     3
#define SETCOLORTABLE                4
#define GETCOLORTABLE                5
#define FLUSHOUTPUT                  6
#define DRAFTMODE                    7
#define QUERYESCSUPPORT              8
#define SETABORTPROC                 9
#define STARTDOC                     10
#define ENDDOC                       11
#define GETPHYSPAGESIZE              12
#define GETPRINTINGOFFSET            13
#define GETSCALINGFACTOR             14
#define MFCOMMENT                    15
#define GETPENWIDTH                  16
#define SETCOPYCOUNT                 17
#define SELECTPAPERSOURCE            18
#define DEVICEDATA                   19
#define PASSTHROUGH                  19
#define GETTECHNOLGY                 20
#define GETTECHNOLOGY                20
#define SETLINECAP                   21
#define SETLINEJOIN                  22
#define SETMITERLIMIT                23
#define BANDINFO                     24
#define DRAWPATTERNRECT              25
#define GETVECTORPENSIZE             26
#define GETVECTORBRUSHSIZE           27
#define ENABLEDUPLEX                 28
#define GETSETPAPERBINS              29
#define GETSETPRINTORIENT            30
#define ENUMPAPERBINS                31
#define SETDIBSCALING                32
#define EPSPRINTING                  33
#define ENUMPAPERMETRICS             34
#define GETSETPAPERMETRICS           35
#define POSTSCRIPT_DATA              37
#define POSTSCRIPT_IGNORE            38
#define MOUSETRAILS                  39
#define GETDEVICEUNITS               42

#define GETEXTENDEDTEXTMETRICS       256
#define GETEXTENTTABLE               257
#define GETPAIRKERNTABLE             258
#define GETTRACKKERNTABLE            259
#define EXTTEXTOUT                   512
#define GETFACENAME                  513
#define DOWNLOADFACE                 514
#define ENABLERELATIVEWIDTHS         768
#define ENABLEPAIRKERNING            769
#define SETKERNTRACK                 770
#define SETALLJUSTVALUES             771
#define SETCHARSET                   772

#define STRETCHBLT                   2048
#define METAFILE_DRIVER              2049
#define GETSETSCREENPARAMS           3072
#define QUERYDIBSUPPORT              3073
#define BEGIN_PATH                   4096
#define CLIP_TO_PATH                 4097
#define END_PATH                     4098
#define EXT_DEVICE_CAPS              4099
#define RESTORE_CTM                  4100
#define SAVE_CTM                     4101
#define SET_ARC_DIRECTION            4102
#define SET_BACKGROUND_COLOR         4103
#define SET_POLY_MODE                4104
#define SET_SCREEN_ANGLE             4105
#define SET_SPREAD                   4106
#define TRANSFORM_CTM                4107
#define SET_CLIP_BOX                 4108
#define SET_BOUNDS                   4109
#define SET_MIRROR_MODE              4110
#define OPENCHANNEL                  4110
#define DOWNLOADHEADER               4111
#define CLOSECHANNEL                 4112
#define POSTSCRIPT_PASSTHROUGH       4115
#define ENCAPSULATED_POSTSCRIPT      4116

#define POSTSCRIPT_IDENTIFY          4117   /* new escape for NT5 pscript driver */
#define POSTSCRIPT_INJECTION         4118   /* new escape for NT5 pscript driver */

#define CHECKJPEGFORMAT              4119
#define CHECKPNGFORMAT               4120

#define GET_PS_FEATURESETTING        4121   /* new escape for NT5 pscript driver */

#define SPCLPASSTHROUGH2             4568   /* new escape for NT5 pscript driver */
};

value LONG RegistryType
{
#define REG_NONE                        0   // No value type
#define REG_SZ                          1   // Unicode nul terminated string
#define REG_EXPAND_SZ                   2   // Unicode nul terminated string
                                            // (with environment variable references)
#define REG_BINARY                      3   // Free form binary
#define REG_DWORD                       4   // 32-bit number
#define REG_DWORD_BIG_ENDIAN            5   // 32-bit number
#define REG_LINK                        6   // Symbolic Link (unicode)
#define REG_MULTI_SZ                    7   // Multiple Unicode strings
#define REG_RESOURCE_LIST               8   // Resource list in the resource map
#define REG_FULL_RESOURCE_DESCRIPTOR    9  // Resource list in the hardware description
#define REG_RESOURCE_REQUIREMENTS_LIST  10
#define REG_QWORD                       11  // 64-bit number
};

value LONG ShowWindowCommand
{
#define SW_HIDE             0
#define SW_NORMAL           1
#define SW_SHOWMINIMIZED    2
#define SW_MAXIMIZE         3
#define SW_SHOWNOACTIVATE   4
#define SW_SHOW             5
#define SW_MINIMIZE         6
#define SW_SHOWMINNOACTIVE  7
#define SW_SHOWNA           8
#define SW_RESTORE          9
#define SW_SHOWDEFAULT      10
#define SW_FORCEMINIMIZE    11
};

mask DWORD AccessMode
{
#define PIPE_ACCESS_INBOUND             0x00000001
#define PIPE_ACCESS_OUTBOUND            0x00000002
#define PIPE_ACCESS_DUPLEX              0x00000003
#define FILE_FLAG_WRITE_THROUGH         0x80000000
#define FILE_FLAG_OVERLAPPED            0x40000000
#define ACCESS_SYSTEM_SECURITY          0x01000000

#define DELETE                          0x00010000
#define READ_CONTROL                    0x00020000
#define WRITE_DAC                       0x00040000
#define WRITE_OWNER                     0x00080000
#define SYNCHRONIZE                     0x00100000

//
// AccessSystemAcl access type
//

#define ACCESS_SYSTEM_SECURITY          0x01000000

//
// MaximumAllowed access type
//

#define MAXIMUM_ALLOWED                 0x02000000

//
//  These are the generic rights.
//

#define GENERIC_READ                    0x80000000
#define GENERIC_WRITE                   0x40000000
#define GENERIC_EXECUTE                 0x20000000
#define GENERIC_ALL                     0x10000000
};

alias HWND;
alias LPVOID;
alias HANDLE;
alias HDC;
alias HRGN;
alias HMENU;
alias HRSRC;
alias HACCEL;
alias HHOOK;
alias HINSTANCE;
alias HMODULE;
alias HKL;
alias HDESK;
alias HWINSTA;
alias HMONITOR;
alias WNDPROC;
alias HPRINTER;
alias ThreadId;
alias ProcessId;

#include "kernel32.h"
#include "user32.h"
#include "gdi32.h"
#include "winspool.h"
#include "version.h"
#include "winsock2.h"
#include "advapi32.h"

//
// uuids.h and com.h are needed for all the headers that have COM definitions.
// Those headers are: shell.h ole32.h ddraw.h dplay.h
//
#include "uuids.h"
#include "com.h"

#include "shell.h"
#include "ole32.h"

//
// ddraw is needed for all the headers that have DirectX definitions.
// Those headers are: d3d.h d3d8.h
//
#include "ddraw.h"
#include "winmm.h"
#include "avifile.h"
#include "dplay.h"
#include "d3d.h"
#include "d3d8.h"
#include "dsound.h"
