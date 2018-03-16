category AVIFileExports:

typedef struct _AVIFILEINFOA {
    DWORD       dwMaxBytesPerSec;   // max. transfer rate
    DWORD       dwFlags;        // the ever-present flags
    DWORD       dwCaps;
    DWORD       dwStreams;
    DWORD       dwSuggestedBufferSize;

    DWORD       dwWidth;
    DWORD       dwHeight;

    DWORD       dwScale;
    DWORD       dwRate; /* dwRate / dwScale == samples/second */
    DWORD       dwLength;

    DWORD       dwEditCount;

    char        szFileType[64];     // descriptive string for file type?
} AVIFILEINFOA;
typedef AVIFILEINFOA  * LPAVIFILEINFOA;

typedef struct _AVIFILEINFOW {
    DWORD       dwMaxBytesPerSec;   // max. transfer rate
    DWORD       dwFlags;        // the ever-present flags
    DWORD       dwCaps;
    DWORD       dwStreams;
    DWORD       dwSuggestedBufferSize;

    DWORD       dwWidth;
    DWORD       dwHeight;

    DWORD       dwScale;
    DWORD       dwRate; /* dwRate / dwScale == samples/second */
    DWORD       dwLength;

    DWORD       dwEditCount;

    WCHAR       szFileType[64];     // descriptive string for file type?
} AVIFILEINFOW;
typedef AVIFILEINFOW  * LPAVIFILEINFOW;

typedef DWORD AVIFILE;
typedef DWORD* PAVIFILE;

VOID AVIFileInit();
VOID AVIFileExit();

ULONG AVIFileAddRef(PAVIFILE pfile);
ULONG AVIFileRelease(PAVIFILE pfile);

STDAPI AVIFileOpenA(PAVIFILE* ppfile,  LPCSTR szFile, UINT mode, CLSID pclsidHandler);
STDAPI AVIFileOpenW(PAVIFILE* ppfile,  LPCWSTR szFile, UINT mode, CLSID pclsidHandler);

STDAPI AVIFileInfoA(PAVIFILE pfile, LPAVIFILEINFOA pfi, LONG lSize);
STDAPI AVIFileInfoW(PAVIFILE pfile, LPAVIFILEINFOW pfi, LONG lSize);
