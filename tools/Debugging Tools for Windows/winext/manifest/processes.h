// +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
//
//                          Processes and Threads
//
// +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
category ProcessesAndThreads:

mask DWORD CreateProcessCreationFlags
{
#define DEBUG_PROCESS               0x00000001
#define DEBUG_ONLY_THIS_PROCESS     0x00000002
#define CREATE_SUSPENDED            0x00000004
#define DETACHED_PROCESS            0x00000008
#define CREATE_NEW_CONSOLE          0x00000010
#define NORMAL_PRIORITY_CLASS       0x00000020
#define IDLE_PRIORITY_CLASS         0x00000040
#define HIGH_PRIORITY_CLASS         0x00000080
#define REALTIME_PRIORITY_CLASS     0x00000100
#define CREATE_NEW_PROCESS_GROUP    0x00000200
#define CREATE_UNICODE_ENVIRONMENT  0x00000400
#define CREATE_SEPARATE_WOW_VDM     0x00000800
#define CREATE_SHARED_WOW_VDM       0x00001000
#define CREATE_FORCEDOS             0x00002000
#define CREATE_DEFAULT_ERROR_MODE   0x04000000
#define CREATE_NO_WINDOW            0x08000000

};

value DWORD DLLReason
{
#define DLL_PROCESS_DETACH 0
#define DLL_PROCESS_ATTACH 1
#define DLL_THREAD_ATTACH  2
#define DLL_THREAD_DETACH  3
};

mask DWORD LoadLibFlags
{
#define DONT_RESOLVE_DLL_REFERENCES     0x00000001
#define LOAD_LIBRARY_AS_DATAFILE        0x00000002
#define LOAD_WITH_ALTERED_SEARCH_PATH   0x00000008
};


value DWORD GetGuiResourcesFlags
{
#define GR_GDIOBJECTS     0
#define GR_USEROBJECTS    1
};

typedef struct _IO_COUNTERS {
    ULONGLONG  ReadOperationCount;
    ULONGLONG  WriteOperationCount;
    ULONGLONG  OtherOperationCount;
    ULONGLONG ReadTransferCount;
    ULONGLONG WriteTransferCount;
    ULONGLONG OtherTransferCount;
} IO_COUNTERS;
typedef IO_COUNTERS *LPIO_COUNTERS;

value LPDWORD ProcessShutdownFlags
{
#define SHUTDOWN_NORETRY                0x00000001
};

/*
typedef struct _FILETIME {
    DWORD dwLowDateTime;
    DWORD dwHighDateTime;
} FILETIME, *LPFILETIME;
*/

mask DWORD JobObjectAccess
{
#define JOB_OBJECT_ASSIGN_PROCESS           0x0001
#define JOB_OBJECT_SET_ATTRIBUTES           0x0002
#define JOB_OBJECT_QUERY                    0x0004
#define JOB_OBJECT_TERMINATE                0x0008
#define JOB_OBJECT_SET_SECURITY_ATTRIBUTES  0x0010
};

mask DWORD ProcessAccess
{
#define PROCESS_CREATE_PROCESS    0x0080
#define PROCESS_CREATE_THREAD     0x0002
#define PROCESS_DUP_HANDLE        0x0040
#define PROCESS_QUERY_INFORMATION 0x0400
#define PROCESS_SET_QUOTA         0x0100
#define PROCESS_SET_INFORMATION   0x0200
#define PROCESS_TERMINATE         0x0001
#define PROCESS_VM_OPERATION      0x0008
#define PROCESS_VM_READ           0x0010
#define PROCESS_VM_WRITE          0x0020
#define SYNCHRONIZE               0x00100000L
};

mask DWORD ThreadAccess
{
#define SYNCHRONIZE               0x00100000L
#define THREAD_TERMINATE               0x0001
#define THREAD_SUSPEND_RESUME          0x0002
#define THREAD_GET_CONTEXT             0x0008
#define THREAD_SET_CONTEXT             0x0010
#define THREAD_SET_INFORMATION         0x0020
#define THREAD_QUERY_INFORMATION       0x0040
#define THREAD_SET_THREAD_TOKEN        0x0080
};

mask ULONG QueueUserWorkItemFlags
{
#define WT_EXECUTEDEFAULT      0x00000000
#define WT_EXECUTEINIOTHREAD   0x00000001
#define WT_EXECUTEINUITHREAD   0x00000002
#define WT_EXECUTEINWAITTHREAD 0x00000004
#define WT_EXECUTEDELETEWAIT   0x00000008
#define WT_EXECUTEINLONGTHREAD 0x00000010
};

value DWORD SleepExReturnValue
{
#define Interval_Expired            0
#define Returned_due_to_CallBack    0x000000C0
};

value DWORD ExceptionFlags
{
#define NULL                        0
#define EXCEPTION_NONCONTINUABLE    0x1
};

module KERNEL32.DLL:

FailOnFalse AssignProcessToJobObject(
                                     HANDLE hJob,
                                     HANDLE hProcess
                                     );

module SHELL32.DLL:
LpwstrFailIfNull * [gle] CommandLineToArgvW(
                                            LPCWSTR lpCmdLine,
                                            INT *pNumArgs
                                            );

module KERNEL32.DLL:
LpvoidFailIfNull [gle] ConvertThreadToFiber(
                                            LPVOID lpParameter
                                            );

LpvoidFailIfNull [gle] CreateFiber(
                                   DWORD dwStackSize,
                                   LPFIBER_START_ROUTINE lpStartAddress,
                                   LPVOID lpParameter
                                   );

HANDLE [gle] CreateJobObjectA(
                                        LPSECURITY_ATTRIBUTES lpJobAttributes,
                                        LPCSTR lpName
                                        );

HANDLE [gle] CreateJobObjectW(
                                        LPSECURITY_ATTRIBUTES lpJobAttributes,
                                        LPCWSTR lpName
                                        );

FailOnFalse [gle] CreateProcessA(
                                 LPCSTR lpApplicationName,
                                 LPSTR lpCommandLine,
                                 LPSECURITY_ATTRIBUTES lpProcessAttributes,
                                 LPSECURITY_ATTRIBUTES lpThreadAttributes,
                                 BOOL bInheritHandles,
                                 CreateProcessCreationFlags dwCreationFlags,
                                 LPVOID lpEnvironment,
                                 LPCSTR lpCurrentDirectory,
                                 [out] LPSTARTUPINFOA lpStartupInfo,
                                 [out] LPPROCESS_INFORMATION lpProcessInformation
                                 );

FailOnFalse [gle] CreateProcessW(
                                 LPCWSTR lpApplicationName,
                                 LPWSTR lpCommandLine,
                                 LPSECURITY_ATTRIBUTES lpProcessAttributes,
                                 LPSECURITY_ATTRIBUTES lpThreadAttributes,
                                 BOOL bInheritHandles,
                                 CreateProcessCreationFlags dwCreationFlags,
                                 LPVOID lpEnvironment,
                                 LPCWSTR lpCurrentDirectory,
                                 LPSTARTUPINFOW lpStartupInfo,
                                 LPPROCESS_INFORMATION lpProcessInformation
                                 );

module ADVAPI32.DLL:
FailOnFalse [gle] CreateProcessAsUserA (
                                        HANDLE hToken,
                                        LPCSTR lpApplicationName,
                                        LPSTR lpCommandLine,
                                        LPSECURITY_ATTRIBUTES lpProcessAttributes,
                                        LPSECURITY_ATTRIBUTES lpThreadAttributes,
                                        BOOL bInheritHandles,
                                        CreateProcessCreationFlags dwCreationFlags,
                                        LPVOID lpEnvironment,
                                        LPCSTR lpCurrentDirectory,
                                        LPSTARTUPINFOA lpStartupInfo,
                                        LPPROCESS_INFORMATION lpProcessInformation
                                        );

FailOnFalse [gle] CreateProcessAsUserW (
                                        HANDLE hToken,
                                        LPCWSTR lpApplicationName,
                                        LPWSTR lpCommandLine,
                                        LPSECURITY_ATTRIBUTES lpProcessAttributes,
                                        LPSECURITY_ATTRIBUTES lpThreadAttributes,
                                        BOOL bInheritHandles,
                                        CreateProcessCreationFlags dwCreationFlags,
                                        LPVOID lpEnvironment,
                                        LPCWSTR lpCurrentDirectory,
                                        LPSTARTUPINFOW lpStartupInfo,
                                        LPPROCESS_INFORMATION lpProcessInformation
                                        );

FailOnFalse [gle] CreateProcessWithLogonW(
                                          LPCWSTR lpUsername,
                                          LPCWSTR lpDomain,
                                          LPCWSTR lpPassword,
                                          LPCWSTR lpApplicationName,
                                          LPWSTR lpCommandLine,
                                          CreateProcessCreationFlags dwCreationFlags,
                                          LPVOID lpEnvironment,
                                          LPCWSTR lpCurrentDirectory,
                                          LPSTARTUPINFOW lpStartupInfo,
                                          LPPROCESS_INFORMATION lpProcessInformation
                                          );

module KERNEL32.DLL:
HANDLE [gle] CreateRemoteThread(
                                          HANDLE hProcess,
                                          LPSECURITY_ATTRIBUTES lpThreadAttributes,
                                          DWORD dwStackSize,
                                          LPTHREAD_START_ROUTINE lpStartAddress,
                                          LPVOID lpParameter,
                                          CreateProcessCreationFlags dwCreationFlags,
                                          LPDWORD lpThreadId
                                          );

HANDLE [gle] CreateThread(
                                    LPSECURITY_ATTRIBUTES lpThreadAttributes,
                                    DWORD dwStackSize,
                                    LPTHREAD_START_ROUTINE lpStartAddress,
                                    LPVOID lpParameter,
                                    CreateProcessCreationFlags dwCreationFlags,
                                    LPDWORD lpThreadId
                                    );

VOID DeleteFiber(
                 LPVOID lpFiber
                 );

VOID ExitProcess(
                 UINT uExitCode
                 );

VOID ExitThread(
                DWORD dwExitCode
                );

FailOnFalse [gle] FreeEnvironmentStringsA(
                                          LPSTR lpszEnvironmentBlock
                                          );

FailOnFalse [gle] FreeEnvironmentStringsW(
                                          LPWSTR lpszEnvironmentBlock
                                          );

LPSTR GetCommandLineA();

LPWSTR GetCommandLineW();

DWORD GetCurrentProcess();

ProcessId GetCurrentProcessId();
ThreadId GetCurrentThreadId();

HANDLE GetCurrentThread();

LPSTR GetEnvironmentStrings();

LPWSTR GetEnvironmentStringsW();

DwordFailIfZero GetEnvironmentVariableA(
                                        LPCSTR lpName,
                                        [out] LPSTR lpBuffer,
                                        DWORD nSize
                                        );

DwordFailIfZero GetEnvironmentVariableW(
                                        LPCWSTR lpName,
                                        [out] LPWSTR lpBuffer,
                                        DWORD nSize
                                        );

FailOnFalse [gle] GetExitCodeProcess(
                                     HANDLE hProcess,
                                     [out] LPDWORD lpExitCode
                                     );

FailOnFalse [gle] GetExitCodeThread(
                                    HANDLE hThread,
                                    [out] LPDWORD lpExitCode
                                    );

module USER32.DLL:
DwordFailIfZero GetGuiResources (
                                 HANDLE hProcess,
                                 GetGuiResourcesFlags uiFlags
                                 );

module KERNEL32.DLL:
DWORD GetPriorityClass(
                       HANDLE hProcess
                       );

FailOnFalse [gle] GetProcessAffinityMask(
                                         HANDLE hProcess,
                                         [out] LPDWORD lpProcessAffinityMask,
                                         [out] LPDWORD lpSystemAffinityMask
                                         );

FailOnFalse [gle] GetProcessIoCounters(
                                       HANDLE hProcess,
                                       [out] LPIO_COUNTERS lpIoCounters
                                       );

FailOnFalse [gle] GetProcessPriorityBoost(
                                          HANDLE hProcess,
                                          [out] PBOOL pDisablePriorityBoost
                                          );

FailOnFalse [gle] GetProcessShutdownParameters(
                                               [out] LPDWORD lpdwLevel,
                                               [out] ProcessShutdownFlags *lpdwFlags
                                               );

FailOnFalse [gle] GetProcessTimes(
                                  HANDLE hProcess,
                                  [out] LPFILETIME lpCreationTime,
                                  [out] LPFILETIME lpExitTime,
                                  [out] LPFILETIME lpKernelTime,
                                  [out] LPFILETIME lpUserTime
                                  );

DwordFailIfZero [gle] GetProcessVersion(
                                        ProcessId dwProcessId
                                        );

FailOnFalse [gle] GetProcessWorkingSetSize(
                                           HANDLE hProcess,
                                           [out] LPDWORD lpMinimumWorkingSetSize,
                                           [out] LPDWORD lpMaximumWorkingSetSize
                                           );

VOID GetStartupInfoA(
                    [out] LPSTARTUPINFOA lpStartupInfo
                    );

VOID GetStartupInfoW(
                    [out] LPSTARTUPINFOW lpStartupInfo
                    );

ThreadPriority [gle] GetThreadPriority(
                                                     HANDLE hThread
                                                     );

FailOnFalse [gle] GetThreadPriorityBoost(
                                         HANDLE hThread,
                                         [out] PBOOL pDisablePriorityBoost
                                         );

FailOnFalse [gle] GetThreadTimes(
                                 HANDLE hThread,
                                 [out] LPFILETIME lpCreationTime,
                                 [out] LPFILETIME lpExitTime,
                                 [out] LPFILETIME lpKernelTime,
                                 [out] LPFILETIME lpUserTime
                                 );

HANDLE [gle] OpenJobObjectA(
                                     JobObjectAccess dwDesiredAccess,
                                     BOOL bInheritHandles,
                                     LPCSTR lpName
                                     );

HANDLE [gle] OpenJobObjectW(
                                     JobObjectAccess dwDesiredAccess,
                                     BOOL bInheritHandles,
                                     LPCWSTR lpName
                                     );

HANDLE [gle] OpenProcess(
                                   ProcessAccess dwDesiredAccess,
                                   BOOL bInheritHandle,
                                   DWORD dwProcessId
                                   );

HANDLE [gle] OpenThread(
                                  ThreadAccess dwDesiredAccess,
                                  BOOL bInheritHandle,
                                  DWORD dwThreadId
                                  );

FailOnFalse [gle] QueryInformationJobObject(
                                            HANDLE hJob,
                                            DWORD JobObjectInformationClass,
                                            LPVOID lpJobObjectInformation,
                                            DWORD cbJobObjectInformationLength,
                                            [out] LPDWORD lpReturnLength
                                            );

FailOnFalse QueueUserWorkItem(
                              LPTHREAD_START_ROUTINE Function,
                              PVOID Context,
                              QueueUserWorkItemFlags Flags
                              );

VOID RaiseException(DWORD  dwExceptionCode,
                    ExceptionFlags dwExceptionFlags,
                    DWORD  nNumberOfArguments,
                    DWORD* lpArguments);

DwordFailIf0xFFFFFFFF [gle] ResumeThread(
                                         HANDLE hThread
                                         );

FailOnFalse [gle] SetEnvironmentVariableA(
                                          LPCSTR lpName,
                                          LPCSTR lpValue
                                          );

FailOnFalse [gle] SetEnvironmentVariableW(
                                          LPCWSTR lpName,
                                          LPCWSTR lpValue
                                          );

BOOL SetInformationJobObject(
                             HANDLE hJob,
                             DWORD JobObjectInformationClass,
                             LPVOID lpJobObjectInformation,
                             DWORD cbJobObjectInformationLength
                             );

FailOnFalse [gle] SetPriorityClass(
                                   HANDLE hProcess,
                                   CreateProcessCreationFlags dwPriorityClass
                                   );

FailOnFalse [gle] SetProcessAffinityMask(
                                         HANDLE hProcess,
                                         DWORD dwProcessAffinityMask
                                         );

FailOnFalse [gle] SetProcessPriorityBoost(
                                          HANDLE hProcess,
                                          BOOL DisablePriorityBoost
                                          );

FailOnFalse [gle] SetProcessShutdownParameters(
                                               DWORD dwLevel,
                                               ProcessShutdownFlags dwFlags
                                               );

FailOnFalse [gle] SetProcessWorkingSetSize(
                                           HANDLE hProcess,
                                           DWORD dwMinimumWorkingSetSize,
                                           DWORD dwMaximumWorkingSetSize
                                           );

DwordFailIfZero SetThreadAffinityMask (
                                       HANDLE hThread,
                                       DWORD dwThreadAffinityMask
                                       );

DwordFailIfNeg1 [gle] SetThreadIdealProcessor(
                                              HANDLE hThread,
                                              DWORD dwIdealProcessor
                                              );

FailOnFalse [gle] SetThreadPriority(
                                    HANDLE hThread,
                                    ThreadPriority nPriority
                                    );

FailOnFalse [gle] SetThreadPriorityBoost(
                                         HANDLE hThread,
                                         BOOL DisablePriorityBoost
                                         );

VOID Sleep(
           DWORD dwMilliseconds
           );

SleepExReturnValue SleepEx(
                           DWORD dwMilliseconds,
                           BOOL bAlertable
                           );

DwordFailIf0xFFFFFFFF [gle] SuspendThread(
                                          HANDLE hThread
                                          );

VOID SwitchToFiber(
                   LPVOID lpFiber
                   );

BOOL SwitchToThread();

FailOnFalse [gle] TerminateJobObject(
                                     HANDLE hJob,
                                     UINT uExitCode
                                     );

FailOnFalse [gle] TerminateProcess(
                                   HANDLE hProcess,
                                   UINT uExitCode
                                   );

FailOnFalse [gle] TerminateThread(
                                  HANDLE hThread,
                                  DWORD dwExitCode
                                  );

category ThreadLocalStorage:
DwordFailIf0xFFFFFFFF [gle] TlsAlloc();

FailOnFalse [gle] TlsFree(
                          DWORD dwTlsIndex
                          );

LpvoidFailIfNull [gle] TlsGetValue(DWORD dwTlsIndex);

FailOnFalse [gle] TlsSetValue(
                              DWORD dwTlsIndex,
                              LPVOID lpTlsValue
                              );

category ProcessesAndThreads:

module USER32.DLL:
FailOnFalse [gle] UserHandleGrantAccess(
                                        HANDLE hUserHandle,
                                        HANDLE hJob,
                                        BOOL bGrant
                                        );

module KERNEL32.DLL:
WinError WinExec(
                 LPCSTR lpCmdLine,
                 UINT uCmdShow
                 );



DwordFailIfZero [gle] DisableThreadLibraryCalls(HMODULE hLibModule);

DwordFailIfZero [gle] FreeLibrary(
                             HMODULE hLibModule
                             );

DwordFailIfZero [gle] GetModuleFileNameA(
                                  HMODULE hModule,
                                  [out] LPSTR lpFilename,
                                  DWORD nSize
                                  );

DwordFailIfZero [gle] GetModuleFileNameW(
                                  HMODULE hModule,
                                  [out] LPWSTR lpFilename,
                                  DWORD nSize
                                  );


VOID FreeLibraryAndExitThread(
                              HMODULE hLibModule,
                              DWORD dwExitCode
                             );



DwordFailIfZero [gle] GetModuleHandleA(
                                       LPCSTR lpModuleName
                                       );

DwordFailIfZero [gle] GetModuleHandleW(
                                       LPCWSTR lpModuleName
                                       );



LpvoidFailIfNull [gle] GetProcAddress(
                                      HMODULE hModule,
                                      LPCSTR lpProcName
                                      );

HMODULE [gle] LoadLibraryA(LPCSTR lpLibFileName);
HMODULE [gle] LoadLibraryW(LPCWSTR lpLibFileName);


HMODULE [gle] LoadLibraryExA(LPCSTR lpLibFileName,
                             HANDLE hFile,
                             LoadLibFlags dwFlags);

HMODULE [gle] LoadLibraryExW(LPCWSTR lpLibFileName,
                             HANDLE hFile,
                             LoadLibFlags dwFlags);

DWORD LoadModule(
                LPCSTR lpModuleName,
                LPVOID lpParameterBlock
                );
