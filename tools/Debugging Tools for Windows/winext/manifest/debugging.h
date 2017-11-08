// +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
//
//                Debugging Functions
//
// +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
module KERNEL32.DLL:
category DebuggingAndErrorHandling:

typedef LPVOID LPDEBUG_EVENT;

FailOnFalse [gle] ContinueDebugEvent(
                                     DWORD dwProcessId,
                                     DWORD dwThreadId,
                                     DWORD dwContinueStatus
                                     );

FailOnFalse [gle] DebugActiveProcess(
                                     DWORD dwProcessId
                                     );

VOID DebugBreak();

VOID FatalExit(
               INT ExitCode
               );

FailOnFalse [gle] FlushInstructionCache(
                                        HANDLE hProcess,
                                        LPCVOID lpBaseAddress,
                                        DWORD dwSize
                                        );

FailOnFalse [gle] GetThreadContext(
                                   HANDLE hThread,
                                   LPCONTEXT lpContext
                                   );

FailOnFalse [gle] GetThreadSelectorEntry(
                                         HANDLE hThread,
                                         DWORD dwSelector,
                                         [out] LPVOID lpSelectorEntry
                                         );

BOOL IsDebuggerPresent();

VOID OutputDebugStringA(
                       LPCSTR lpOutputString
                       );

VOID OutputDebugStringW(
                       LPCWSTR lpOutputString
                       );

FailOnFalse [gle] SetThreadContext(
                                   HANDLE hThread,
                                   LPCONTEXT lpContext
                                   );

FailOnFalse [gle] WaitForDebugEvent(
                                    LPDEBUG_EVENT lpDebugEvent,
                                    DWORD dwMilliseconds
                                    );

// +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
//
//                          Error Handling Functions
//
// +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

FailOnFalse [gle] Beep(
                       DWORD dwFreq,
                       DWORD dwDuration
                       );

VOID FatalAppExitA(
                  UINT uAction,
                  LPCSTR lpMessageText
                  );

VOID FatalAppExitW(
                  UINT uAction,
                  LPCWSTR lpMessageText
                  );

module KERNEL32.DLL:

mask DWORD FormatMessageFlags
{
#define FORMAT_MESSAGE_ALLOCATE_BUFFER 0x00000100
#define FORMAT_MESSAGE_IGNORE_INSERTS  0x00000200
#define FORMAT_MESSAGE_FROM_STRING     0x00000400
#define FORMAT_MESSAGE_FROM_HMODULE    0x00000800
#define FORMAT_MESSAGE_FROM_SYSTEM     0x00001000
#define FORMAT_MESSAGE_ARGUMENT_ARRAY  0x00002000
#define FORMAT_MESSAGE_MAX_WIDTH_MASK  0x000000FF
};

DwordFailIfZero [gle] FormatMessageA(
                                    FormatMessageFlags dwFlags,
                                    LPCVOID lpSource,
                                    DWORD dwMessageId,
                                    DWORD dwLanguageId,
                                    [out] LPSTR lpBuffer,
                                    DWORD nSize,
                                    PVOID Arguments
                                    );

DwordFailIfZero [gle] FormatMessageW(
                                    FormatMessageFlags dwFlags,
                                    LPCVOID lpSource,
                                    DWORD dwMessageId,
                                    DWORD dwLanguageId,
                                    [out] LPWSTR lpBuffer,
                                    DWORD nSize,
                                    PVOID Arguments
                                    );


DWORD GetLastError();


value UINT MessageBeepType
{
#define MB_OK                       0x00000000L
#define MB_ICONHAND                 0x00000010L
#define MB_ICONQUESTION             0x00000020L
#define MB_ICONEXCLAMATION          0x00000030L
#define MB_ICONASTERISK             0x00000040L
};

module USER32.DLL:
FailOnFalse [gle] MessageBeep(
                              MessageBeepType uType
                              );

value UINT SetErrorModeType
{
#define SEM_FAILCRITICALERRORS      0x0001
#define SEM_NOGPFAULTERRORBOX       0x0002
#define SEM_NOALIGNMENTFAULTEXCEPT  0x0004
#define SEM_NOOPENFILEERRORBOX      0x8000
};

module KERNEL32.DLL:
UINT SetErrorMode(
                  SetErrorModeType uMode
                  );

VOID SetLastError(
                  DWORD dwErrCode
                  );

module USER32.DLL:
VOID SetLastErrorEx(
                    DWORD dwErrCode,
                    DWORD dwType
                    );



// +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
//
//                              Toolhelp32 Functions
//
// +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++


module KERNEL32.DLL:
mask DWORD CreateToolhelp32SnapshotFlags
{
#define TH32CS_SNAPHEAPLIST 0x00000001
#define TH32CS_SNAPPROCESS  0x00000002
#define TH32CS_SNAPTHREAD   0x00000004
#define TH32CS_SNAPMODULE   0x00000008
#define TH32CS_INHERIT      0x80000000
};

value DWORD HeapList32Type
{
#define HF32_DEFAULT      1  // process's default heap
#define HF32_SHARED       2  // is shared heap
};

typedef struct tagHEAPLIST32
{
    SIZE_T dwSize;
    DWORD  th32ProcessID;   // owning process
    ULONG_PTR  th32HeapID;      // heap (in owning process's context!)
    HeapList32Type  dwFlags;
} HEAPLIST32;
typedef HEAPLIST32 *  PHEAPLIST32;
typedef HEAPLIST32 *  LPHEAPLIST32;

mask DWORD HeapEntry32Flags
{
#define LF32_FIXED    0x00000001
#define LF32_FREE     0x00000002
#define LF32_MOVEABLE 0x00000004
};

typedef struct tagHEAPENTRY32
{
    SIZE_T dwSize;
    HANDLE hHandle;     // Handle of this heap block
    ULONG_PTR dwAddress;   // Linear address of start of block
    SIZE_T dwBlockSize; // Size of block in bytes
    HeapEntry32Flags  dwFlags;
    DWORD  dwLockCount;
    DWORD  dwResvd;
    DWORD  th32ProcessID;   // owning process
    ULONG_PTR  th32HeapID;      // heap block is in
} HEAPENTRY32;
typedef HEAPENTRY32 *  PHEAPENTRY32;
typedef HEAPENTRY32 *  LPHEAPENTRY32;

HANDLE [gle]
CreateToolhelp32Snapshot(
    CreateToolhelp32SnapshotFlags dwFlags,
    DWORD th32ProcessID
    );


FailOnFalse [gle]
Heap32ListFirst(
    HANDLE hSnapshot,
    [out] LPHEAPLIST32 lphl
    );

FailOnFalse [gle]
Heap32ListNext(
    HANDLE hSnapshot,
    [out] LPHEAPLIST32 lphl
    );

FailOnFalse [gle]
Heap32First(
    [out] LPHEAPENTRY32 lphe,
    DWORD th32ProcessID,
    ULONG_PTR th32HeapID
    );

FailOnFalse [gle]
Heap32Next(
    [out] LPHEAPENTRY32 lphe
    );

FailOnFalse [gle]
Toolhelp32ReadProcessMemory(
    DWORD   th32ProcessID,
    LPCVOID lpBaseAddress,
    LPVOID  lpBuffer,
    SIZE_T  cbRead,
    [out] SIZE_T *lpNumberOfBytesRead
    );

typedef struct tagPROCESSENTRY32W
{
    DWORD   dwSize;
    DWORD   cntUsage;
    DWORD   th32ProcessID;          // this process
    ULONG_PTR th32DefaultHeapID;
    DWORD   th32ModuleID;           // associated exe
    DWORD   cntThreads;
    DWORD   th32ParentProcessID;    // this process's parent process
    LONG    pcPriClassBase;         // Base priority of process's threads
    DWORD   dwFlags;
    WCHAR   szExeFile[260];    // Path
} PROCESSENTRY32W;
typedef PROCESSENTRY32W *  PPROCESSENTRY32W;
typedef PROCESSENTRY32W *  LPPROCESSENTRY32W;

typedef struct tagPROCESSENTRY32
{
    DWORD   dwSize;
    DWORD   cntUsage;
    DWORD   th32ProcessID;          // this process
    ULONG_PTR th32DefaultHeapID;
    DWORD   th32ModuleID;           // associated exe
    DWORD   cntThreads;
    DWORD   th32ParentProcessID;    // this process's parent process
    LONG    pcPriClassBase;         // Base priority of process's threads
    DWORD   dwFlags;
    CHAR    szExeFile[260];    // Path
} PROCESSENTRY32;
typedef PROCESSENTRY32 *  PPROCESSENTRY32;
typedef PROCESSENTRY32 *  LPPROCESSENTRY32;

FailOnFalse [gle]
Process32FirstW(
    HANDLE hSnapshot,
    [out] LPPROCESSENTRY32W lppe
    );

FailOnFalse [gle]
Process32NextW(
    HANDLE hSnapshot,
    [out] LPPROCESSENTRY32W lppe
    );

FailOnFalse [gle]
Process32First(
    HANDLE hSnapshot,
    [out] LPPROCESSENTRY32 lppe
    );

FailOnFalse [gle]
Process32Next(
    HANDLE hSnapshot,
    [out] LPPROCESSENTRY32 lppe
    );

typedef struct tagTHREADENTRY32
{
    DWORD   dwSize;
    DWORD   cntUsage;
    DWORD   th32ThreadID;       // this thread
    DWORD   th32OwnerProcessID; // Process this thread is associated with
    LONG    tpBasePri;
    LONG    tpDeltaPri;
    DWORD   dwFlags;
} THREADENTRY32;
typedef THREADENTRY32 *  PTHREADENTRY32;
typedef THREADENTRY32 *  LPTHREADENTRY32;

FailOnFalse [gle]
Thread32First(
    HANDLE hSnapshot,
    [out] LPTHREADENTRY32 lpte
    );

FailOnFalse [gle]
Thread32Next(
    HANDLE hSnapshot,
    [out] LPTHREADENTRY32 lpte
    );

typedef struct tagMODULEENTRY32W
{
    DWORD   dwSize;
    DWORD   th32ModuleID;       // This module
    DWORD   th32ProcessID;      // owning process
    DWORD   GlblcntUsage;       // Global usage count on the module
    DWORD   ProccntUsage;       // Module usage count in th32ProcessID's context
    BYTE  * modBaseAddr;        // Base address of module in th32ProcessID's context
    DWORD   modBaseSize;        // Size in bytes of module starting at modBaseAddr
    HMODULE hModule;            // The hModule of this module in th32ProcessID's context
    WCHAR   szModule[256];
    WCHAR   szExePath[260];
} MODULEENTRY32W;
typedef MODULEENTRY32W *  PMODULEENTRY32W;
typedef MODULEENTRY32W *  LPMODULEENTRY32W;

FailOnFalse [gle]
Module32FirstW(
    HANDLE hSnapshot,
    [out] LPMODULEENTRY32W lpme
    );

FailOnFalse [gle]
Module32NextW(
    HANDLE hSnapshot,
    [out] LPMODULEENTRY32W lpme
    );


typedef struct tagMODULEENTRY32
{
    DWORD   dwSize;
    DWORD   th32ModuleID;       // This module
    DWORD   th32ProcessID;      // owning process
    DWORD   GlblcntUsage;       // Global usage count on the module
    DWORD   ProccntUsage;       // Module usage count in th32ProcessID's context
    BYTE  * modBaseAddr;        // Base address of module in th32ProcessID's context
    DWORD   modBaseSize;        // Size in bytes of module starting at modBaseAddr
    HMODULE hModule;            // The hModule of this module in th32ProcessID's context
    char    szModule[256];
    char    szExePath[260];
} MODULEENTRY32;
typedef MODULEENTRY32 *  PMODULEENTRY32;
typedef MODULEENTRY32 *  LPMODULEENTRY32;

//
// NOTE CAREFULLY that the modBaseAddr and hModule fields are valid ONLY
// in th32ProcessID's process context.
//

FailOnFalse [gle]
Module32First(
    HANDLE hSnapshot,
    [out] LPMODULEENTRY32 lpme
    );

FailOnFalse [gle]
Module32Next(
    HANDLE hSnapshot,
    [out] LPMODULEENTRY32 lpme
    );
