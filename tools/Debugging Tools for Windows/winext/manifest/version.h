// +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
//
//                              Versioning Functions
//
// +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

category Version:
module VERSION.DLL:


/*
export VerFindFileA;
export VerFindFileW;
export VerInstallFileA;
export VerInstallFileW;
export VerQueryValueA;
export VerQueryValueIndexA;
export VerQueryValueIndexW;
export VerQueryValueW;
*/

DWORD
GetFileVersionInfoSizeA(
        LPSTR lptstrFilename, /* Filename of version stamped file */
        [out] LPDWORD lpdwHandle
        );                      /* Information for use by GetFileVersionInfo */
/* Returns size of version info in bytes */
DWORD
GetFileVersionInfoSizeW(
        LPWSTR lptstrFilename, /* Filename of version stamped file */
        [out] LPDWORD lpdwHandle
        );                      /* Information for use by GetFileVersionInfo */

/* Read version info into buffer */
FailOnFalse
GetFileVersionInfoA(
        LPSTR lptstrFilename, /* Filename of version stamped file */
        DWORD dwHandle,         /* Information from GetFileVersionSize */
        DWORD dwLen,            /* Length of buffer for info */
        [out] LPVOID lpData
        );                      /* Buffer to place the data structure */
/* Read version info into buffer */
FailOnFalse
GetFileVersionInfoW(
        LPWSTR lptstrFilename, /* Filename of version stamped file */
        DWORD dwHandle,         /* Information from GetFileVersionSize */
        DWORD dwLen,            /* Length of buffer for info */
        [out] LPVOID lpData
        );                      /* Buffer to place the data structure */


module KERNEL32.DLL:

value LONG PlatformId
{
#define VER_PLATFORM_WIN32s             0
#define VER_PLATFORM_WIN32_WINDOWS      1
#define VER_PLATFORM_WIN32_NT           2
};

typedef struct _OSVERSIONINFOA{
    DWORD dwOSVersionInfoSize;
    DWORD dwMajorVersion;
    DWORD dwMinorVersion;
    DWORD dwBuildNumber;
    PlatformId dwPlatformId;
    CHAR szCSDVersion[ 128 ];
} OSVERSIONINFOA, *LPOSVERSIONINFOA;

typedef struct _OSVERSIONINFOW{
    DWORD dwOSVersionInfoSize;
    DWORD dwMajorVersion;
    DWORD dwMinorVersion;
    DWORD dwBuildNumber;
    PlatformId dwPlatformId;
    WCHAR szCSDVersion[ 128 ];
} OSVERSIONINFOW, *LPOSVERSIONINFOW;

DWORD GetVersion();

FailOnFalse [gle] GetVersionExA(
                                [out] LPOSVERSIONINFOA lpVersionInfo
                                );

FailOnFalse [gle] GetVersionExW(
                                [out] LPOSVERSIONINFOW lpVersionInfo
                                );
