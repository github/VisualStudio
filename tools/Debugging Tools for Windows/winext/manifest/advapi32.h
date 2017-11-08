// +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
//
//                              AdvApi32 Functions
//
// +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
category AdvApi32:


module ADVAPI32.DLL:

typedef PVOID PSID;

typedef ULONG SECURITY_INFORMATION;
typedef SECURITY_INFORMATION *PSECURITY_INFORMATION;
typedef PVOID PSECURITY_DESCRIPTOR;
typedef PVOID PGENERIC_MAPPING;
typedef PVOID PPRIVILEGE_SET;

value int SidNameUse
{
#define SidTypeUser                     1
#define SidTypeGroup                    2
#define SidTypeDomain                   3
#define SidTypeAlias                    4
#define SidTypeWellKnownGroup           5
#define SidTypeDeletedAccount           6
#define SidTypeInvalid                  7
#define SidTypeUnknown                  8
#define SidTypeComputer                 9
};

typedef struct _SID_IDENTIFIER_AUTHORITY {
    BYTE Value[6];
} SID_IDENTIFIER_AUTHORITY, *PSID_IDENTIFIER_AUTHORITY;

FailOnFalse [gle] AccessCheck([in] PSECURITY_DESCRIPTOR pSecurityDescriptor,
                              [in] HANDLE ClientToken,
                              [in] DWORD DesiredAccess,
                              [in] PGENERIC_MAPPING GenericMapping,
                              [out] PPRIVILEGE_SET PrivilegeSet,
                              [out] LPDWORD PrivilegeSetLength,
                              [out] LPDWORD GrantedAccess,
                              [out] LPBOOL AccessStatus);

FailOnFalse [gle] AllocateAndInitializeSid(PSID_IDENTIFIER_AUTHORITY pIdentifierAuthority,
                                           BYTE nSubAuthorityCount,
                                           DWORD nSubAuthority0,
                                           DWORD nSubAuthority1,
                                           DWORD nSubAuthority2,
                                           DWORD nSubAuthority3,
                                           DWORD nSubAuthority4,
                                           DWORD nSubAuthority5,
                                           DWORD nSubAuthority6,
                                           DWORD nSubAuthority7,
                                           [out] PSID* pSid);

FailOnFalse [gle] EqualSid(PSID pSid1,
                           PSID pSid2);

FailOnFalse [gle] GetFileSecurityA(LPCSTR lpFileName,
                                   SECURITY_INFORMATION RequestedInformation,
                                   [out] PSECURITY_DESCRIPTOR pSecurityDescriptor,
                                   DWORD nLength,
                                   [out] LPDWORD lpnLengthNeeded);

FailOnFalse [gle] GetFileSecurityW(LPCWSTR lpFileName,
                                   SECURITY_INFORMATION RequestedInformation,
                                   [out] PSECURITY_DESCRIPTOR pSecurityDescriptor,
                                   DWORD nLength,
                                   [out] LPDWORD lpnLengthNeeded);

FailOnFalse [gle] GetKernelObjectSecurity(HANDLE Handle,
                                          SECURITY_INFORMATION RequestedInformation,
                                          [out] PSECURITY_DESCRIPTOR pSecurityDescriptor,
                                          DWORD nLength,
                                          [out] LPDWORD lpnLengthNeeded);

DWORD GetLengthSid(PSID pSid);

FailOnFalse [gle] GetSecurityDescriptorGroup(PSECURITY_DESCRIPTOR pSecurityDescriptor,
                                             [out] PSID *pGroup,
                                             [out] LPBOOL lpbGroupDefaulted);

FailOnFalse [gle] GetSecurityDescriptorOwner(PSECURITY_DESCRIPTOR pSecurityDescriptor,
                                             [out] PSID *pOwner,
                                             [out] LPBOOL lpbOwnerDefaulted);

PSID_IDENTIFIER_AUTHORITY GetSidIdentifierAuthority(PSID pSid);

PDWORD GetSidSubAuthority(PSID pSid,
                          DWORD nSubAuthority);

PBYTE GetSidSubAuthorityCount(PSID pSid);

FailOnFalse IsValidSid(PSID pSid);

FailOnFalse [gle] LookupAccountNameA(LPCSTR lpSystemName,
                                     LPCSTR lpAccountName,
                                     [out] PSID Sid,
                                     [out] LPDWORD cbSid,
                                     [out] LPSTR ReferencedDomainName,
                                     [out] LPDWORD cbReferencedDomainName,
                                     [out] SidNameUse* peUse);

FailOnFalse [gle] LookupAccountNameW(LPCWSTR lpSystemName,
                                     LPCWSTR lpAccountName,
                                     [out] PSID Sid,
                                     [out] LPDWORD cbSid,
                                     [out] LPWSTR ReferencedDomainName,
                                     [out] LPDWORD cbReferencedDomainName,
                                     [out] SidNameUse* peUse);

FailOnFalse [gle] LookupAccountSidA(LPCSTR lpSystemName,
                                    PSID Sid,
                                    [out] LPSTR Name,
                                    [out] LPDWORD cbName,
                                    [out] LPSTR ReferencedDomainName,
                                    [out] LPDWORD cbReferencedDomainName,
                                    [out] SidNameUse* peUse);

FailOnFalse [gle] LookupAccountSidW(LPCWSTR lpSystemName,
                                    PSID Sid,
                                    [out] LPWSTR Name,
                                    [out] LPDWORD cbName,
                                    [out] LPWSTR ReferencedDomainName,
                                    [out] LPDWORD cbReferencedDomainName,
                                    [out] SidNameUse* peUse);

// ------------------------------------------------------------
//
//                       Services functions
//
// ------------------------------------------------------------

//
// Value to indicate no change to an optional parameter
//


//
// Service Types (Bit Mask)
//
mask DWORD ServiceType
{
#define SERVICE_KERNEL_DRIVER          0x00000001
#define SERVICE_FILE_SYSTEM_DRIVER     0x00000002
#define SERVICE_ADAPTER                0x00000004
#define SERVICE_RECOGNIZER_DRIVER      0x00000008

#define SERVICE_WIN32_OWN_PROCESS      0x00000010
#define SERVICE_WIN32_SHARE_PROCESS    0x00000020

#define SERVICE_INTERACTIVE_PROCESS    0x00000100

};


//
// Start Type
//

value DWORD ServiceStartType
{
#define SERVICE_BOOT_START             0x00000000
#define SERVICE_SYSTEM_START           0x00000001
#define SERVICE_AUTO_START             0x00000002
#define SERVICE_DEMAND_START           0x00000003
#define SERVICE_DISABLED               0x00000004
};

//
// Error control type
//
value DWORD ServiceErrorControlType
{
#define SERVICE_ERROR_IGNORE           0x00000000
#define SERVICE_ERROR_NORMAL           0x00000001
#define SERVICE_ERROR_SEVERE           0x00000002
#define SERVICE_ERROR_CRITICAL         0x00000003
};

//
// Service State -- for Enum Requests (Bit Mask)
//

value DWORD ServiceState
{
#define SERVICE_ACTIVE                 0x00000001
#define SERVICE_INACTIVE               0x00000002
#define SERVICE_STATE_ALL              0x00000003
#define SERVICE_NO_CHANGE              0xffffffff
};

//
// Controls
//
value DWORD ServiceControl
{
#define SERVICE_CONTROL_STOP                   0x00000001
#define SERVICE_CONTROL_PAUSE                  0x00000002
#define SERVICE_CONTROL_CONTINUE               0x00000003
#define SERVICE_CONTROL_INTERROGATE            0x00000004
#define SERVICE_CONTROL_SHUTDOWN               0x00000005
#define SERVICE_CONTROL_PARAMCHANGE            0x00000006
#define SERVICE_CONTROL_NETBINDADD             0x00000007
#define SERVICE_CONTROL_NETBINDREMOVE          0x00000008
#define SERVICE_CONTROL_NETBINDENABLE          0x00000009
#define SERVICE_CONTROL_NETBINDDISABLE         0x0000000A
#define SERVICE_CONTROL_DEVICEEVENT            0x0000000B
#define SERVICE_CONTROL_HARDWAREPROFILECHANGE  0x0000000C
#define SERVICE_CONTROL_POWEREVENT             0x0000000D
#define SERVICE_CONTROL_SESSIONCHANGE          0x0000000E
};

//
// Service State -- for CurrentState
//
value DWORD ServiceCurrentState
{
#define SERVICE_STOPPED                        0x00000001
#define SERVICE_START_PENDING                  0x00000002
#define SERVICE_STOP_PENDING                   0x00000003
#define SERVICE_RUNNING                        0x00000004
#define SERVICE_CONTINUE_PENDING               0x00000005
#define SERVICE_PAUSE_PENDING                  0x00000006
#define SERVICE_PAUSED                         0x00000007
};

//
// Controls Accepted  (Bit Mask)
//
mask DWORD ServiceControlsAccepted
{
#define SERVICE_ACCEPT_STOP                    0x00000001
#define SERVICE_ACCEPT_PAUSE_CONTINUE          0x00000002
#define SERVICE_ACCEPT_SHUTDOWN                0x00000004
#define SERVICE_ACCEPT_PARAMCHANGE             0x00000008
#define SERVICE_ACCEPT_NETBINDCHANGE           0x00000010
#define SERVICE_ACCEPT_HARDWAREPROFILECHANGE   0x00000020
#define SERVICE_ACCEPT_POWEREVENT              0x00000040
#define SERVICE_ACCEPT_SESSIONCHANGE           0x00000080
};

//
// Service Control Manager object specific access types
//
mask DWORD SCManagerAccess
{
#define SC_MANAGER_CONNECT             0x0001
#define SC_MANAGER_CREATE_SERVICE      0x0002
#define SC_MANAGER_ENUMERATE_SERVICE   0x0004
#define SC_MANAGER_LOCK                0x0008
#define SC_MANAGER_QUERY_LOCK_STATUS   0x0010
#define SC_MANAGER_MODIFY_BOOT_CONFIG  0x0020
};

//
// Service object specific access type
//
mask DWORD ServiceObjectAccess
{
#define SERVICE_QUERY_CONFIG           0x0001
#define SERVICE_CHANGE_CONFIG          0x0002
#define SERVICE_QUERY_STATUS           0x0004
#define SERVICE_ENUMERATE_DEPENDENTS   0x0008
#define SERVICE_START                  0x0010
#define SERVICE_STOP                   0x0020
#define SERVICE_PAUSE_CONTINUE         0x0040
#define SERVICE_INTERROGATE            0x0080
#define SERVICE_USER_DEFINED_CONTROL   0x0100
};

//
// Service flags for QueryServiceStatusEx
//
mask DWORD QueryServiceStatusExFlags
{
#define SERVICE_RUNS_IN_SYSTEM_PROCESS  0x00000001
};

//
// Info levels for ChangeServiceConfig2 and QueryServiceConfig2
//
value LONG ServiceConfig2Values
{
#define SERVICE_CONFIG_DESCRIPTION     1
#define SERVICE_CONFIG_FAILURE_ACTIONS 2
};

//
// Service description string
//
typedef struct _SERVICE_DESCRIPTIONA {
    LPSTR       lpDescription;
} SERVICE_DESCRIPTIONA, *LPSERVICE_DESCRIPTIONA;

//
// Service description string
//
typedef struct _SERVICE_DESCRIPTIONW {
    LPWSTR      lpDescription;
} SERVICE_DESCRIPTIONW, *LPSERVICE_DESCRIPTIONW;

//
// Actions to take on service failure
//
value LONG SC_ACTION_TYPE
{
#define SC_ACTION_NONE          0
#define SC_ACTION_RESTART       1
#define SC_ACTION_REBOOT        2
#define SC_ACTION_RUN_COMMAND   3
};

typedef struct _SC_ACTION {
    SC_ACTION_TYPE  Type;
    DWORD           Delay;
} SC_ACTION, *LPSC_ACTION;

typedef struct _SERVICE_FAILURE_ACTIONSA {
    DWORD       dwResetPeriod;
    LPSTR       lpRebootMsg;
    LPSTR       lpCommand;
    DWORD       cActions;
    SC_ACTION * lpsaActions;
} SERVICE_FAILURE_ACTIONSA, *LPSERVICE_FAILURE_ACTIONSA;

typedef struct _SERVICE_FAILURE_ACTIONSW {
    DWORD       dwResetPeriod;
    LPWSTR      lpRebootMsg;
    LPWSTR      lpCommand;
    DWORD       cActions;
    SC_ACTION * lpsaActions;
} SERVICE_FAILURE_ACTIONSW, *LPSERVICE_FAILURE_ACTIONSW;

typedef    HANDLE SC_HANDLE;
typedef    SC_HANDLE *LPSC_HANDLE ;
typedef  HANDLE SERVICE_STATUS_HANDLE;

//
// Info levels for QueryServiceStatusEx
//

value LONG SC_STATUS_TYPE
{
#define SC_STATUS_PROCESS_INFO      0
};

//
// Info levels for EnumServicesStatusEx
//
value LONG SC_ENUM_TYPE
{
#define SC_ENUM_PROCESS_INFO      0
};

//
// Service Status Structures
//

typedef struct _SERVICE_STATUS {
    ServiceType   dwServiceType;
    ServiceCurrentState   dwCurrentState;
    ServiceControlsAccepted   dwControlsAccepted;
    DWORD   dwWin32ExitCode;
    DWORD   dwServiceSpecificExitCode;
    DWORD   dwCheckPoint;
    DWORD   dwWaitHint;
} SERVICE_STATUS, *LPSERVICE_STATUS;

typedef struct _SERVICE_STATUS_PROCESS {
    ServiceType   dwServiceType;
    ServiceCurrentState   dwCurrentState;
    ServiceControlsAccepted   dwControlsAccepted;
    DWORD   dwWin32ExitCode;
    DWORD   dwServiceSpecificExitCode;
    DWORD   dwCheckPoint;
    DWORD   dwWaitHint;
    DWORD   dwProcessId;
    DWORD   dwServiceFlags;
} SERVICE_STATUS_PROCESS, *LPSERVICE_STATUS_PROCESS;

//
// Service Status Enumeration Structure
//

typedef struct _ENUM_SERVICE_STATUSA {
    LPSTR             lpServiceName;
    LPSTR             lpDisplayName;
    SERVICE_STATUS    ServiceStatus;
} ENUM_SERVICE_STATUSA, *LPENUM_SERVICE_STATUSA;
typedef struct _ENUM_SERVICE_STATUSW {
    LPWSTR            lpServiceName;
    LPWSTR            lpDisplayName;
    SERVICE_STATUS    ServiceStatus;
} ENUM_SERVICE_STATUSW, *LPENUM_SERVICE_STATUSW;

typedef struct _ENUM_SERVICE_STATUS_PROCESSA {
    LPSTR                     lpServiceName;
    LPSTR                     lpDisplayName;
    SERVICE_STATUS_PROCESS    ServiceStatusProcess;
} ENUM_SERVICE_STATUS_PROCESSA, *LPENUM_SERVICE_STATUS_PROCESSA;
typedef struct _ENUM_SERVICE_STATUS_PROCESSW {
    LPWSTR                    lpServiceName;
    LPWSTR                    lpDisplayName;
    SERVICE_STATUS_PROCESS    ServiceStatusProcess;
} ENUM_SERVICE_STATUS_PROCESSW, *LPENUM_SERVICE_STATUS_PROCESSW;

//
// Structures for the Lock API functions
//

typedef LPVOID  SC_LOCK;

typedef struct _QUERY_SERVICE_LOCK_STATUSA {
    DWORD   fIsLocked;
    LPSTR   lpLockOwner;
    DWORD   dwLockDuration;
} QUERY_SERVICE_LOCK_STATUSA, *LPQUERY_SERVICE_LOCK_STATUSA;
typedef struct _QUERY_SERVICE_LOCK_STATUSW {
    DWORD   fIsLocked;
    LPWSTR  lpLockOwner;
    DWORD   dwLockDuration;
} QUERY_SERVICE_LOCK_STATUSW, *LPQUERY_SERVICE_LOCK_STATUSW;

//
// Query Service Configuration Structure
//

typedef struct _QUERY_SERVICE_CONFIGA {
    ServiceType   dwServiceType;
    ServiceStartType   dwStartType;
    ServiceErrorControlType   dwErrorControl;
    LPSTR   lpBinaryPathName;
    LPSTR   lpLoadOrderGroup;
    DWORD   dwTagId;
    LPSTR   lpDependencies;
    LPSTR   lpServiceStartName;
    LPSTR   lpDisplayName;
} QUERY_SERVICE_CONFIGA, *LPQUERY_SERVICE_CONFIGA;
typedef struct _QUERY_SERVICE_CONFIGW {
    ServiceType   dwServiceType;
    ServiceStartType   dwStartType;
    ServiceErrorControlType   dwErrorControl;
    LPWSTR  lpBinaryPathName;
    LPWSTR  lpLoadOrderGroup;
    DWORD   dwTagId;
    LPWSTR  lpDependencies;
    LPWSTR  lpServiceStartName;
    LPWSTR  lpDisplayName;
} QUERY_SERVICE_CONFIGW, *LPQUERY_SERVICE_CONFIGW;

//
// Service Start Table
//

typedef struct _SERVICE_TABLE_ENTRYA {
    LPSTR                       lpServiceName;
    LPVOID    lpServiceProc;
}SERVICE_TABLE_ENTRYA, *LPSERVICE_TABLE_ENTRYA;
typedef struct _SERVICE_TABLE_ENTRYW {
    LPWSTR                      lpServiceName;
    LPVOID    lpServiceProc;
}SERVICE_TABLE_ENTRYW, *LPSERVICE_TABLE_ENTRYW;


BOOL

ChangeServiceConfigA(
    SC_HANDLE    hService,
    ServiceType        dwServiceType,
    ServiceStartType        dwStartType,
    ServiceErrorControlType        dwErrorControl,
    LPCSTR     lpBinaryPathName,
    LPCSTR     lpLoadOrderGroup,
    [out] LPDWORD      lpdwTagId,
    LPCSTR     lpDependencies,
    LPCSTR     lpServiceStartName,
    LPCSTR     lpPassword,
    LPCSTR     lpDisplayName
    );

BOOL

ChangeServiceConfigW(
    SC_HANDLE    hService,
    ServiceType        dwServiceType,
    ServiceStartType        dwStartType,
    ServiceErrorControlType        dwErrorControl,
    LPCWSTR     lpBinaryPathName,
    LPCWSTR     lpLoadOrderGroup,
    [out] LPDWORD      lpdwTagId,
    LPCWSTR     lpDependencies,
    LPCWSTR     lpServiceStartName,
    LPCWSTR     lpPassword,
    LPCWSTR     lpDisplayName
    );


BOOL

ChangeServiceConfig2A(
    SC_HANDLE    hService,
    ServiceConfig2Values        dwInfoLevel,
    LPVOID       lpInfo
    );

BOOL

ChangeServiceConfig2W(
    SC_HANDLE    hService,
    ServiceConfig2Values        dwInfoLevel,
    LPVOID       lpInfo
    );


BOOL

CloseServiceHandle(
    SC_HANDLE   hSCObject
    );


BOOL

ControlService(
    SC_HANDLE           hService,
    ServiceControl               dwControl,
    LPSERVICE_STATUS    lpServiceStatus
    );


SC_HANDLE

CreateServiceA(
    SC_HANDLE    hSCManager,
    LPCSTR     lpServiceName,
    LPCSTR     lpDisplayName,
    AccessMode        dwDesiredAccess,
    ServiceType        dwServiceType,
    ServiceStartType        dwStartType,
    ServiceErrorControlType        dwErrorControl,
    LPCSTR     lpBinaryPathName,
    LPCSTR     lpLoadOrderGroup,
    [out] LPDWORD      lpdwTagId,
    LPCSTR     lpDependencies,
    LPCSTR     lpServiceStartName,
    LPCSTR     lpPassword
    );

SC_HANDLE

CreateServiceW(
    SC_HANDLE    hSCManager,
    LPCWSTR     lpServiceName,
    LPCWSTR     lpDisplayName,
    AccessMode        dwDesiredAccess,
    ServiceType        dwServiceType,
    ServiceStartType        dwStartType,
    ServiceErrorControlType        dwErrorControl,
    LPCWSTR     lpBinaryPathName,
    LPCWSTR     lpLoadOrderGroup,
    [out] LPDWORD      lpdwTagId,
    LPCWSTR     lpDependencies,
    LPCWSTR     lpServiceStartName,
    LPCWSTR     lpPassword
    );


BOOL

DeleteService(
    SC_HANDLE   hService
    );


BOOL

EnumDependentServicesA(
    SC_HANDLE               hService,
    ServiceState            dwServiceState,
    [out] LPENUM_SERVICE_STATUSA  lpServices,
    DWORD                   cbBufSize,
    [out] LPDWORD                 pcbBytesNeeded,
    [out] LPDWORD                 lpServicesReturned
    );

BOOL

EnumDependentServicesW(
    SC_HANDLE               hService,
    ServiceState                   dwServiceState,
    [out] LPENUM_SERVICE_STATUSW  lpServices,
    DWORD                   cbBufSize,
    [out] LPDWORD                 pcbBytesNeeded,
    [out] LPDWORD                 lpServicesReturned
    );


BOOL

EnumServicesStatusA(
    SC_HANDLE               hSCManager,
    ServiceType                   dwServiceType,
    ServiceState                   dwServiceState,
    [out] LPENUM_SERVICE_STATUSA  lpServices,
    DWORD                   cbBufSize,
    [out] LPDWORD                 pcbBytesNeeded,
    [out] LPDWORD                 lpServicesReturned,
    [out] LPDWORD                 lpResumeHandle
    );

BOOL

EnumServicesStatusW(
    SC_HANDLE               hSCManager,
    ServiceType                   dwServiceType,
    ServiceState                   dwServiceState,
    [out] LPENUM_SERVICE_STATUSW  lpServices,
    DWORD                   cbBufSize,
    [out] LPDWORD                 pcbBytesNeeded,
    [out] LPDWORD                 lpServicesReturned,
    [out] LPDWORD                 lpResumeHandle
    );


BOOL

EnumServicesStatusExA(
    SC_HANDLE                  hSCManager,
    SC_ENUM_TYPE               InfoLevel,
    ServiceType                      dwServiceType,
    ServiceState                      dwServiceState,
    LPBYTE                     lpServices,
    DWORD                      cbBufSize,
    [out] LPDWORD                    pcbBytesNeeded,
    [out] LPDWORD                    lpServicesReturned,
    [out] LPDWORD                    lpResumeHandle,
    LPCSTR                   pszGroupName
    );

BOOL

EnumServicesStatusExW(
    SC_HANDLE                  hSCManager,
    SC_ENUM_TYPE               InfoLevel,
    ServiceType                      dwServiceType,
    ServiceState                      dwServiceState,
    LPBYTE                     lpServices,
    DWORD                      cbBufSize,
    [out] LPDWORD                    pcbBytesNeeded,
    [out] LPDWORD                    lpServicesReturned,
    [out] LPDWORD                    lpResumeHandle,
    LPCWSTR                   pszGroupName
    );


BOOL

GetServiceKeyNameA(
    SC_HANDLE               hSCManager,
    LPCSTR                lpDisplayName,
    [out] LPSTR                 lpServiceName,
    [out] LPDWORD                 lpcchBuffer
    );

BOOL

GetServiceKeyNameW(
    SC_HANDLE               hSCManager,
    LPCWSTR                lpDisplayName,
    [out] LPWSTR                 lpServiceName,
    [out] LPDWORD                 lpcchBuffer
    );


BOOL

GetServiceDisplayNameA(
    SC_HANDLE               hSCManager,
    LPCSTR                lpServiceName,
    [out] LPSTR                 lpDisplayName,
    [out] LPDWORD                 lpcchBuffer
    );

BOOL

GetServiceDisplayNameW(
    SC_HANDLE               hSCManager,
    LPCWSTR                lpServiceName,
    [out] LPWSTR                 lpDisplayName,
    [out] LPDWORD                 lpcchBuffer
    );


SC_LOCK

LockServiceDatabase(
    SC_HANDLE   hSCManager
    );


BOOL

NotifyBootConfigStatus(
    BOOL     BootAcceptable
    );


SC_HANDLE

OpenSCManagerA(
    LPCSTR lpMachineName,
    LPCSTR lpDatabaseName,
    AccessMode   dwDesiredAccess
    );

SC_HANDLE

OpenSCManagerW(
    LPCWSTR lpMachineName,
    LPCWSTR lpDatabaseName,
    AccessMode   dwDesiredAccess
    );


SC_HANDLE

OpenServiceA(
    SC_HANDLE   hSCManager,
    LPCSTR    lpServiceName,
    AccessMode       dwDesiredAccess
    );

SC_HANDLE

OpenServiceW(
    SC_HANDLE   hSCManager,
    LPCWSTR    lpServiceName,
    AccessMode       dwDesiredAccess
    );


BOOL

QueryServiceConfigA(
    SC_HANDLE               hService,
    [out] LPQUERY_SERVICE_CONFIGA lpServiceConfig,
    DWORD                   cbBufSize,
    [out] LPDWORD                 pcbBytesNeeded
    );

BOOL

QueryServiceConfigW(
    SC_HANDLE               hService,
    [out] LPQUERY_SERVICE_CONFIGW lpServiceConfig,
    DWORD                   cbBufSize,
    [out] LPDWORD                 pcbBytesNeeded
    );


BOOL

QueryServiceConfig2A(
    SC_HANDLE   hService,
    ServiceConfig2Values       dwInfoLevel,
    LPBYTE      lpBuffer,
    DWORD       cbBufSize,
    [out] LPDWORD     pcbBytesNeeded
    );

BOOL

QueryServiceConfig2W(
    SC_HANDLE   hService,
    ServiceConfig2Values       dwInfoLevel,
    LPBYTE      lpBuffer,
    DWORD       cbBufSize,
    LPDWORD     pcbBytesNeeded
    );


BOOL

QueryServiceLockStatusA(
    SC_HANDLE                       hSCManager,
    [OUT] LPQUERY_SERVICE_LOCK_STATUSA    lpLockStatus,
    DWORD                           cbBufSize,
    [OUT] LPDWORD                         pcbBytesNeeded
    );

BOOL

QueryServiceLockStatusW(
    SC_HANDLE                       hSCManager,
    [OUT] LPQUERY_SERVICE_LOCK_STATUSW    lpLockStatus,
    DWORD                           cbBufSize,
    [OUT] LPDWORD                         pcbBytesNeeded
    );


BOOL

QueryServiceObjectSecurity(
    SC_HANDLE               hService,
    SECURITY_INFORMATION    dwSecurityInformation,
    [OUT] PSECURITY_DESCRIPTOR    lpSecurityDescriptor,
    DWORD                   cbBufSize,
    [OUT] LPDWORD                 pcbBytesNeeded
    );


BOOL

QueryServiceStatus(
    SC_HANDLE           hService,
    [OUT] LPSERVICE_STATUS    lpServiceStatus
    );


BOOL

QueryServiceStatusEx(
    SC_HANDLE           hService,
    SC_STATUS_TYPE      InfoLevel,
    LPBYTE              lpBuffer,
    DWORD               cbBufSize,
    [OUT] LPDWORD             pcbBytesNeeded
    );


SERVICE_STATUS_HANDLE

RegisterServiceCtrlHandlerA(
    LPCSTR             lpServiceName,
    LPVOID   lpHandlerProc
    );

SERVICE_STATUS_HANDLE

RegisterServiceCtrlHandlerW(
    LPCWSTR             lpServiceName,
    LPVOID   lpHandlerProc
    );


SERVICE_STATUS_HANDLE

RegisterServiceCtrlHandlerExA(
    LPCSTR                lpServiceName,
    LPVOID   lpHandlerProc,
    LPVOID                  lpContext
    );

SERVICE_STATUS_HANDLE

RegisterServiceCtrlHandlerExW(
    LPCWSTR                lpServiceName,
    LPVOID   lpHandlerProc,
    LPVOID                  lpContext
    );


BOOL

SetServiceObjectSecurity(
    SC_HANDLE               hService,
    SECURITY_INFORMATION    dwSecurityInformation,
    [OUT] PSECURITY_DESCRIPTOR    lpSecurityDescriptor
    );


BOOL

SetServiceStatus(
    SERVICE_STATUS_HANDLE   hServiceStatus,
    LPSERVICE_STATUS        lpServiceStatus
    );


BOOL

StartServiceCtrlDispatcherA(
    SERVICE_TABLE_ENTRYA *lpServiceStartTable
    );

BOOL

StartServiceCtrlDispatcherW(
    SERVICE_TABLE_ENTRYW *lpServiceStartTable
    );



BOOL

StartServiceA(
    SC_HANDLE            hService,
    DWORD                dwNumServiceArgs,
    LPCSTR             *lpServiceArgVectors
    );

BOOL

StartServiceW(
    SC_HANDLE            hService,
    DWORD                dwNumServiceArgs,
    LPCWSTR             *lpServiceArgVectors
    );

BOOL
UnlockServiceDatabase(
    SC_LOCK     ScLock
    );
