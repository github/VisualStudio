// +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
//
//                       Memory Management Functions
//
// +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
category MemoryManagementFunctions:

mask DWORD AccessProtectionType
{
#define PAGE_NOACCESS          0x01
#define PAGE_READONLY          0x02
#define PAGE_READWRITE         0x04
#define PAGE_WRITECOPY         0x08
#define PAGE_EXECUTE           0x10
#define PAGE_EXECUTE_READ      0x20
#define PAGE_EXECUTE_READWRITE 0x40
#define PAGE_EXECUTE_WRITECOPY 0x80
#define PAGE_GUARD            0x100
#define PAGE_NOCACHE          0x200
#define PAGE_WRITECOMBINE     0x400
#define SEC_FILE           0x800000
#define SEC_IMAGE         0x1000000
#define SEC_VLM           0x2000000
#define SEC_RESERVE       0x4000000
#define SEC_COMMIT        0x8000000
#define SEC_NOCACHE      0x10000000
};


value UINT UintFailIfGlobalMemInvalid
{
#define GMEM_INVALID_HANDLE 0x8000 [fail]
};

value UINT UintFailIfLocalMemInvalid
{
#define LMEM_INVALID_HANDLE 0x8000     [fail]
};

value PVOID64 Pvoid64FailIfNull
{
#define NULL                0    [fail]
};

value LPVOID HeapAllocFailureReturns
{
#define NULL                             0           [fail]
#define STATUS_NO_MEMORY                 0xC0000017L    [fail]
#define STATUS_ACCESS_VIOLATION          0xC0000005L    [fail]
};

mask UINT AllocationAttributes
{
#define GMEM_FIXED          0x0000
#define GMEM_MOVEABLE       0x0002
// #define GMEM_DDESHARE       0x2000
#define GMEM_SHARE          0x2000
#define GMEM_DISCARDABLE    0x0100
#define GMEM_LOWER          0x1000
#define GMEM_NOCOMPACT      0x0010
#define GMEM_NODISCARD      0x0020
#define GMEM_NOT_BANKED     0x1000
#define GMEM_NOTIFY         0x4000
#define GMEM_ZEROINIT       0x0040
// #define GMEM_MODIFY         0x0080
// #define GMEM_VALID_FLAGS    0x7F72
// #define GMEM_INVALID_HANDLE 0x8000
};

mask UINT LocalAllocationAttributes
{
#define LMEM_FIXED          0x0000
#define LMEM_MOVEABLE       0x0002
#define LMEM_DISCARDABLE    0x0F00
#define LMEM_NOCOMPACT      0x0010
#define LMEM_NODISCARD      0x0020
#define LMEM_ZEROINIT       0x0040
// #define LMEM_MODIFY         0x0080
// #define LMEM_VALID_FLAGS    0x0F72
// #define LMEM_INVALID_HANDLE 0x8000
};

typedef struct _MEMORYSTATUS {
    DWORD dwLength;
    DWORD dwMemoryLoad;
    SIZE_T dwTotalPhys;
    SIZE_T dwAvailPhys;
    SIZE_T dwTotalPageFile;
    SIZE_T dwAvailPageFile;
    SIZE_T dwTotalVirtual;
    SIZE_T dwAvailVirtual;
} MEMORYSTATUS, *LPMEMORYSTATUS;

typedef struct _MEMORYSTATUSEX {
    DWORD dwLength;
    DWORD dwMemoryLoad;
    DWORDLONG ullTotalPhys;
    DWORDLONG ullAvailPhys;
    DWORDLONG ullTotalPageFile;
    DWORDLONG ullAvailPageFile;
    DWORDLONG ullTotalVirtual;
    DWORDLONG ullAvailVirtual;
    DWORDLONG ullAvailExtendedVirtual;
} MEMORYSTATUSEX, *LPMEMORYSTATUSEX;

mask DWORD HeapAllocationControl
{
#define HEAP_GENERATE_EXCEPTIONS        0x00000004
#define HEAP_NO_SERIALIZE               0x00000001
#define HEAP_ZERO_MEMORY                0x00000008
};

typedef struct _Region {
        DWORD dwCommittedSize;
        DWORD dwUnCommittedSize;
        LPVOID lpFirstBlock;
        LPVOID lpLastBlock;
    } Region;

typedef struct _PROCESS_HEAP_ENTRY {
    PVOID lpData;
    DWORD cbData;
    BYTE cbOverhead;
    BYTE iRegionIndex;
    WORD wFlags;
    Region Region;
} PROCESS_HEAP_ENTRY, *LPPROCESS_HEAP_ENTRY;

value DWORD FileAccessMode
{
#define FILE_MAP_COPY       0x0001
#define FILE_MAP_WRITE      0x0002
#define FILE_MAP_READ       0x0004
};

mask DWORD AllocationType
{
#define MEM_COMMIT           0x1000
#define MEM_RESERVE          0x2000
#define MEM_DECOMMIT         0x4000
#define MEM_RELEASE          0x8000
#define MEM_FREE            0x10000
#define MEM_PRIVATE         0x20000
#define MEM_MAPPED          0x40000
#define MEM_RESET           0x80000
#define MEM_TOP_DOWN       0x100000
#define MEM_4MB_PAGES    0x80000000
//#define MEM_IMAGE         SEC_IMAGE
};

typedef struct _MEMORY_BASIC_INFORMATION {
    PVOID BaseAddress;
    PVOID AllocationBase;
    AccessProtectionType AllocationProtect;
    SIZE_T RegionSize;
    AllocationType State;
    AllocationType Protect;
    DWORD Type;
} MEMORY_BASIC_INFORMATION, *PMEMORY_BASIC_INFORMATION;

typedef struct _MEMORY_BASIC_INFORMATION_VLM {
    PVOID64 BaseAddress;
    // ULONGLONG BaseAddressAsUlongLong;        Part of a Union
    PVOID64 AllocationBase;
    // ULONGLONG AllocationBaseAsUlongLong;     Part of a Union
    ULONGLONG RegionSize;
    AccessProtectionType AllocationProtect;
    AllocationType State;
    AllocationType Protect;
    DWORD Type;
} MEMORY_BASIC_INFORMATION_VLM, *PMEMORY_BASIC_INFORMATION_VLM;

typedef struct _SYSTEM_INFO {
  DWORD  dwOemId;
  DWORD  dwPageSize;
  LPVOID lpMinimumApplicationAddress;
  LPVOID lpMaximumApplicationAddress;
  DWORD_PTR dwActiveProcessorMask;
  DWORD dwNumberOfProcessors;
  DWORD dwProcessorType;
  DWORD dwAllocationGranularity;
  WORD wProcessorLevel;
  WORD wProcessorRevision;
} SYSTEM_INFO;

typedef HANDLE HeapHandle;

alias HeapHandle;


module KERNEL32.DLL:

FailOnFalse       [gle] AllocateUserPhysicalPages(HANDLE hProcess,[out] PULONG_PTR NumberOfPages,[out] PULONG_PTR UserPfnArray);
FailOnFalse       [gle] FreeUserPhysicalPages(HANDLE hProcess,[out] PULONG_PTR NumberOfPages,PULONG_PTR UserPfnArray);
HANDLE            [gle] GetProcessHeap();
DwordFailIfZero   [gle] GetProcessHeaps(DWORD NumberOfHeaps,[out] PHANDLE ProcessHeaps);
UINT                    GetWriteWatch(DWORD dwFlags,PVOID lpBaseAddress,SIZE_T dwRegionSize,[out] PVOID  *lpAddresses,[out] PULONG_PTR lpdwCount,[out] PULONG lpdwGranularity);
HGLOBAL           [gle] GlobalAlloc(AllocationAttributes uFlags,SIZE_T dwBytes);
UintFailIfGlobalMemInvalid [gle] GlobalFlags(HGLOBAL hMem);
HGLOBAL           [gle] GlobalFree(HGLOBAL hMem);
HGLOBAL           [gle] GlobalHandle(LPCVOID pMem);
LpvoidFailIfNull  [gle] GlobalLock(HGLOBAL hMem);
VOID                    GlobalMemoryStatus([out] LPMEMORYSTATUS lpBuffer);
FailOnFalse       [gle] GlobalMemoryStatusEx([out] LPMEMORYSTATUSEX lpBuffer);
HGLOBAL           [gle] GlobalReAlloc(HGLOBAL hMem,SIZE_T dwBytes,AllocationAttributes uFlags);
DwordFailIfZero   [gle] GlobalSize(HGLOBAL hMem);
FailOnFalse       [gle] GlobalUnlock(HGLOBAL hMem);

HeapAllocFailureReturns HeapAlloc(HeapHandle hHeap,HeapAllocationControl dwFlags,SIZE_T dwBytes);
LongFailIfZero    [gle] HeapCompact(HeapHandle hHeap,HeapAllocationControl dwFlags);
HeapHandle        [gle] HeapCreate(HeapAllocationControl flOptions,SIZE_T dwInitialSize,SIZE_T dwMaximumSize);
FailOnFalse       [gle] HeapDestroy( [da] HeapHandle hHeap);
FailOnFalse       [gle] HeapFree(HeapHandle hHeap,HeapAllocationControl dwFlags,LPVOID lpMem);
FailOnFalse       [gle] HeapLock(HeapHandle hHeap);
HeapAllocFailureReturns HeapReAlloc(HeapHandle hHeap,HeapAllocationControl dwFlags,LPVOID lpMem,SIZE_T dwBytes);
DwordFailIfNeg1         HeapSize(HeapHandle hHeap,HeapAllocationControl dwFlags,LPCVOID lpMem);
FailOnFalse       [gle] HeapUnlock(HeapHandle hHeap);
FailOnFalse             HeapValidate(HeapHandle hHeap,HeapAllocationControl dwFlags,LPCVOID lpMem);
FailOnFalse       [gle] HeapWalk(HeapHandle hHeap,[out] LPPROCESS_HEAP_ENTRY lpEntry);

HLOCAL            [gle] LocalAlloc(LocalAllocationAttributes uFlags,SIZE_T uBytes);
UintFailIfLocalMemInvalid LocalFlags(HLOCAL hMem);
HLOCAL            [gle] LocalFree(HLOCAL hMem);
HLOCAL            [gle] LocalHandle(LPCVOID pMem);
LpvoidFailIfNull  [gle] LocalLock(HLOCAL hMem);
HLOCAL            [gle] LocalReAlloc(HLOCAL hMem,SIZE_T uBytes,LocalAllocationAttributes uFlags);
LongFailIfZero    [gle] LocalSize(HLOCAL  hMem);
FailOnFalse       [gle] LocalUnlock(HLOCAL hMem);
FailOnFalse       [gle] MapUserPhysicalPages(PVOID lpAddress,ULONG_PTR NumberOfPages,PULONG_PTR UserPfnArray);
FailOnFalse       [gle] MapUserPhysicalPagesScatter(PVOID *VirtualAddresses,ULONG_PTR NumberOfPages,PULONG_PTR PageArray);
UINT                    ResetWriteWatch(LPVOID lpBaseAddress,SIZE_T dwRegionSize);
LpvoidFailIfNull  [gle] VirtualAlloc(LPVOID lpAddress,SIZE_T dwSize,AllocationType flAllocationType,AccessProtectionType flProtect);
LpvoidFailIfNull  [gle] VirtualAllocEx(HANDLE hProcess,LPVOID lpAddress,SIZE_T dwSize,AllocationType flAllocationType,AccessProtectionType flProtect);
FailOnFalse       [gle] VirtualFree(LPVOID lpAddress,SIZE_T dwSize,AllocationType dwFreeType);
FailOnFalse       [gle] VirtualFreeEx(HANDLE hProcess,LPVOID lpAddress,SIZE_T dwSize,AllocationType dwFreeType);
FailOnFalse       [gle] VirtualLock(LPVOID lpAddress,SIZE_T dwSize);
FailOnFalse       [gle] VirtualProtect(LPVOID lpAddress,SIZE_T dwSize,AccessProtectionType flNewProtect,[out] PDWORD lpflOldProtect);
FailOnFalse       [gle] VirtualProtectEx(HANDLE hProcess,LPVOID lpAddress,SIZE_T dwSize,AccessProtectionType flNewProtect,[out] PDWORD lpflOldProtect);
DWORD                   VirtualQuery(LPCVOID lpAddress,[out] PMEMORY_BASIC_INFORMATION lpBuffer,DWORD dwLength);
DWORD                   VirtualQueryEx(HANDLE hProcess,LPCVOID lpAddress,[out] PMEMORY_BASIC_INFORMATION lpBuffer,DWORD dwLength);
FailOnFalse       [gle] VirtualUnlock(LPVOID lpAddress,SIZE_T dwSize);

// File mapping functions
HANDLE            [gle] CreateFileMappingA(HANDLE hFile,LPSECURITY_ATTRIBUTES lpAttributes,AccessProtectionType flProtect,DWORD dwMaximumSizeHigh,DWORD dwMaximumSizeLow,LPCSTR lpName);
HANDLE            [gle] CreateFileMappingW(HANDLE hFile,LPSECURITY_ATTRIBUTES lpFileMappingAttributes,AccessProtectionType flProtect,DWORD dwMaximumSizeHigh,DWORD dwMaximumSizeLow,LPCWSTR lpName);
FailOnFalse       [gle] FlushViewOfFile (LPCVOID lpBaseAddress,SIZE_T dwNumberOfBytesToFlush);
LpvoidFailIfNull  [gle] MapViewOfFile(HANDLE hFileMappingObject,FileAccessMode dwDesiredAccess,DWORD dwFileOffsetHigh,DWORD dwFileOffsetLow,SIZE_T dwNumberOfBytesToMap);
LpvoidFailIfNull  [gle] MapViewOfFileEx(HANDLE hFileMappingObject,FileAccessMode dwDesiredAccess,DWORD dwFileOffsetHigh,DWORD dwFileOffsetLow,SIZE_T dwNumberOfBytesToMap,LPVOID lpBaseAddress);
HANDLE            [gle] OpenFileMappingA(FileAccessMode dwDesiredAccess,BOOL bInheritHandle,LPCSTR lpName);
HANDLE            [gle] OpenFileMappingW(FileAccessMode dwDesiredAccess,BOOL bInheritHandle,LPCWSTR lpName);
FailOnFalse       [gle] UnmapViewOfFile(LPCVOID lpBaseAddress);
