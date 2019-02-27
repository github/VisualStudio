// ---------------------------------------------------------------------------
// ---------------------------------------------------------------------------
//
//                              KERNEL32 API Set
//
// ---------------------------------------------------------------------------
// ---------------------------------------------------------------------------
typedef LPVOID LPCONTEXT;

mask DWORD DesiredSecurityAccess
{
#define KEY_QUERY_VALUE         0x0001
#define KEY_SET_VALUE           0x0002
#define KEY_CREATE_SUB_KEY      0x0004
#define KEY_ENUMERATE_SUB_KEYS  0x0008
#define KEY_NOTIFY              0x0010
#define KEY_CREATE_LINK         0x0020
};

mask DWORD ServiceTypes
{
#define SERVICE_WIN32_OWN_PROCESS      0x00000010
#define SERVICE_WIN32_SHARE_PROCESS    0x00000020
#define SERVICE_KERNEL_DRIVER          0x00000001
#define SERVICE_FILE_SYSTEM_DRIVER     0x00000002
#define SERVICE_INTERACTIVE_PROCESS    0x00000100
};

value DWORD StartTypes
{
#define SERVICE_BOOT_START             0x00000000
#define SERVICE_SYSTEM_START           0x00000001
#define SERVICE_AUTO_START             0x00000002
#define SERVICE_DEMAND_START           0x00000003
#define SERVICE_DISABLED               0x00000004
};

value DWORD ErrorControls
{
#define SERVICE_ERROR_IGNORE           0x00000000
#define SERVICE_ERROR_NORMAL           0x00000001
#define SERVICE_ERROR_SEVERE           0x00000002
#define SERVICE_ERROR_CRITICAL         0x00000003
};

value DWORD ControlCodes
{
#define SERVICE_CONTROL_STOP           0x00000001
#define SERVICE_CONTROL_PAUSE          0x00000002
#define SERVICE_CONTROL_CONTINUE       0x00000003
#define SERVICE_CONTROL_INTERROGATE    0x00000004
#define SERVICE_CONTROL_SHUTDOWN       0x00000005
#define SERVICE_CONTROL_PARAMCHANGE    0x00000006
#define SERVICE_CONTROL_NETBINDADD     0x00000007
#define SERVICE_CONTROL_NETBINDREMOVE  0x00000008
#define SERVICE_CONTROL_NETBINDENABLE  0x00000009
#define SERVICE_CONTROL_NETBINDDISABLE 0x0000000A
};

mask DWORD DesiredAccessTypes
{
#define SC_MANAGER_CONNECT             0x0001
#define SC_MANAGER_CREATE_SERVICE      0x0002
#define SC_MANAGER_ENUMERATE_SERVICE   0x0004
#define SC_MANAGER_LOCK                0x0008
#define SC_MANAGER_QUERY_LOCK_STATUS   0x0010
#define SC_MANAGER_MODIFY_BOOT_CONFIG  0x0020
};

value DWORD InfoLevels
{
#define SERVICE_CONFIG_DESCRIPTION     1
#define SERVICE_CONFIG_FAILURE_ACTIONS 2
};

value DWORD Status
{
#define STATUS_WAIT_0                    0x00000000L
#define STATUS_ABANDONED_WAIT_0          0x00000080L
#define STATUS_USER_APC                  0x000000C0L
#define STATUS_TIMEOUT                   0x00000102L
#define STATUS_PENDING                   0x00000103L
#define STATUS_SEGMENT_NOTIFICATION      0x40000005L
#define STATUS_GUARD_PAGE_VIOLATION      0x80000001L
#define STATUS_DATATYPE_MISALIGNMENT     0x80000002L
#define STATUS_BREAKPOINT                0x80000003L
#define STATUS_SINGLE_STEP               0x80000004L
#define STATUS_ACCESS_VIOLATION          0xC0000005L
#define STATUS_IN_PAGE_ERROR             0xC0000006L
#define STATUS_INVALID_HANDLE            0xC0000008L
#define STATUS_NO_MEMORY                 0xC0000017L
#define STATUS_ILLEGAL_INSTRUCTION       0xC000001DL
#define STATUS_NONCONTINUABLE_EXCEPTION  0xC0000025L
#define STATUS_INVALID_DISPOSITION       0xC0000026L
#define STATUS_ARRAY_BOUNDS_EXCEEDED     0xC000008CL
#define STATUS_FLOAT_DENORMAL_OPERAND    0xC000008DL
#define STATUS_FLOAT_DIVIDE_BY_ZERO      0xC000008EL
#define STATUS_FLOAT_INEXACT_RESULT      0xC000008FL
#define STATUS_FLOAT_INVALID_OPERATION   0xC0000090L
#define STATUS_FLOAT_OVERFLOW            0xC0000091L
#define STATUS_FLOAT_STACK_CHECK         0xC0000092L
#define STATUS_FLOAT_UNDERFLOW           0xC0000093L
#define STATUS_INTEGER_DIVIDE_BY_ZERO    0xC0000094L
#define STATUS_INTEGER_OVERFLOW          0xC0000095L
#define STATUS_PRIVILEGED_INSTRUCTION    0xC0000096L
#define STATUS_STACK_OVERFLOW            0xC00000FDL
#define STATUS_CONTROL_C_EXIT            0xC000013AL
#define STATUS_FLOAT_MULTIPLE_FAULTS     0xC00002B4L
#define STATUS_FLOAT_MULTIPLE_TRAPS      0xC00002B5L
#define STATUS_ILLEGAL_VLM_REFERENCE     0xC00002C0L
};

mask DWORD ControlEvents
{
#define CTRL_C_EVENT        0
#define CTRL_BREAK_EVENT    1
#define CTRL_CLOSE_EVENT    2
#define CTRL_LOGOFF_EVENT   5
#define CTRL_SHUTDOWN_EVENT 6
};

mask DWORD GenericAccessRights
{
#define GENERIC_READ                    0x80000000L
#define GENERIC_WRITE                   0x40000000L
#define GENERIC_EXECUTE                 0x20000000L
#define GENERIC_ALL                     0x10000000L
};

mask DWORD ShareRights
{
#define FILE_SHARE_READ                 0x00000001
#define FILE_SHARE_WRITE                0x00000002
#define FILE_SHARE_DELETE               0x00000004
};

value DWORD CreationActions
{
#define CREATE_NEW          1
#define CREATE_ALWAYS       2
#define OPEN_EXISTING       3
#define OPEN_ALWAYS         4
#define TRUNCATE_EXISTING   5
};

typedef struct _OVERLAPPED {
    DWORD  Internal;
    DWORD  InternalHigh;
    DWORD  Offset;
    DWORD  OffsetHigh;
    HANDLE hEvent;
} OVERLAPPED;

value INT FilePointerStartingPosition
{
#define FILE_BEGIN           0
#define FILE_CURRENT         1
#define FILE_END             2
};

value LONG ThreadBasePriority
{
#define THREAD_BASE_PRIORITY_LOWRT  15  // value that gets a thread to LowRealtime-1
#define THREAD_BASE_PRIORITY_MAX    2   // maximum thread base priority boost
#define THREAD_BASE_PRIORITY_MIN    -2  // minimum thread base priority boost
#define THREAD_BASE_PRIORITY_IDLE   -15 // value that gets a thread to idle
};

value LONG ThreadPriority
{
#define THREAD_PRIORITY_LOWEST          -2
#define THREAD_PRIORITY_BELOW_NORMAL    -1
#define THREAD_PRIORITY_NORMAL          0
#define THREAD_PRIORITY_HIGHEST         2
#define THREAD_PRIORITY_ABOVE_NORMAL    1
#define THREAD_PRIORITY_ERROR_RETURN    0x7FFFFFFF
#define THREAD_PRIORITY_TIME_CRITICAL   15
#define THREAD_PRIORITY_IDLE            -15
};

#include "debugging.h"
#include "processes.h"
#include "memory.h"
#include "registry.h"
#include "fileio.h"
#include "strings.h"
