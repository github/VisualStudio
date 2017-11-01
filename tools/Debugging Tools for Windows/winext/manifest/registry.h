// +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
//
//                              Registry Functions
//
// +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
category RegistryFunctions:


value DWORD SpecialOptions
{
#define REG_OPTION_RESERVED        0x00000000L
#define REG_OPTION_NON_VOLATILE    0x00000000L
#define REG_OPTION_VOLATILE        0x00000001L
#define REG_OPTION_CREATE_LINK     0x00000002L
#define REG_OPTION_BACKUP_RESTORE  0x00000004L
#define REG_OPTION_OPEN_LINK       0x00000008L
};


value LPDWORD DispositionValue
{
#define REG_CREATED_NEW_KEY         0x00000001L
#define REG_OPENED_EXISTING_KEY     0x00000002L
};

value HKEY HandleToOpenKey
{
#define HKEY_CLASSES_ROOT           0x80000000
#define HKEY_CURRENT_USER           0x80000001
#define HKEY_LOCAL_MACHINE          0x80000002
#define HKEY_USERS                  0x80000003
#define HKEY_PERFORMANCE_DATA       0x80000004
#define HKEY_CURRENT_CONFIG         0x80000005
#define HKEY_DYN_DATA               0x80000006
};

alias HandleToOpenKey;

mask DWORD ChangesToReport
{
#define REG_NOTIFY_CHANGE_NAME          0x00000001L
#define REG_NOTIFY_CHANGE_ATTRIBUTES    0x00000002L
#define REG_NOTIFY_CHANGE_LAST_SET      0x00000004L
#define REG_NOTIFY_CHANGE_SECURITY      0x00000008L
};

/*
typedef struct _FILETIME {
    FTime dwLowDateTime;
    FTime dwHighDateTime;
}FILETIME, *PFILETIME;
*/

module KERNEL32.DLL:

UINT        GetPrivateProfileIntA(LPCSTR lpAppName,LPCSTR lpKeyName,INT nDefault,LPCSTR lpFileName);
UINT        GetPrivateProfileIntW(LPCWSTR lpAppName,LPCWSTR lpKeyName,INT nDefault,LPCWSTR lpFileName);
DWORD       GetPrivateProfileSectionA(LPCSTR lpAppName,[out] LPSTR lpReturnedString,DWORD nSize,LPCSTR lpFileName);
DWORD       GetPrivateProfileSectionW(LPCWSTR lpAppName,[out] LPWSTR lpReturnedString,DWORD nSize,LPCWSTR lpFileName);
DWORD       GetPrivateProfileSectionNamesA([out] LPSTR lpszReturnBuffer,DWORD nSize,LPCSTR lpFileName);
DWORD       GetPrivateProfileSectionNamesW([out] LPWSTR lpszReturnBuffer,DWORD nSize,LPCWSTR lpFileName);
DWORD       GetPrivateProfileStringA(LPCSTR lpAppName,LPCSTR lpKeyName,LPCSTR lpDefault,[out] LPSTR lpReturnedString,DWORD nSize,LPCSTR lpFileName);
DWORD       GetPrivateProfileStringW(LPCWSTR lpAppName,LPCWSTR lpKeyName,LPCWSTR lpDefault,[out] LPWSTR lpReturnedString,DWORD nSize,LPCWSTR lpFileName);
FailOnFalse GetPrivateProfileStructA(LPCSTR lpszSection,LPCSTR lpszKey,[out] LPVOID lpStruct,UINT uSizeStruct,LPCSTR szFile);
FailOnFalse GetPrivateProfileStructW(LPCWSTR lpszSection,LPCWSTR lpszKey,[out] LPVOID   lpStruct,UINT uSizeStruct,LPCWSTR szFile);
UINT        GetProfileIntA(LPCSTR lpAppName,LPCSTR lpKeyName,INT nDefault);
UINT        GetProfileIntW(LPCWSTR lpAppName,LPCWSTR lpKeyName,INT nDefault);
DWORD       GetProfileSectionA(LPCSTR lpAppName,[out] LPSTR lpReturnedString,DWORD nSize);
DWORD       GetProfileSectionW(LPCWSTR lpAppName,[out] LPWSTR lpReturnedString,DWORD nSize);
DWORD       GetProfileStringA(LPCSTR lpAppName,LPCSTR lpKeyName,LPCSTR lpDefault,[out] LPSTR lpReturnedString,DWORD nSize);
DWORD       GetProfileStringW(LPCWSTR lpAppName,LPCWSTR lpKeyName,LPCWSTR lpDefault,[out] LPWSTR lpReturnedString,DWORD nSize);
FailOnFalse  [gle]  WritePrivateProfileSectionA(LPCSTR lpAppName,LPCSTR lpString,LPCSTR lpFileName);
FailOnFalse  [gle]  WritePrivateProfileSectionW(LPCWSTR lpAppName,LPCWSTR lpString,LPCWSTR lpFileName);
FailOnFalse  [gle]  WritePrivateProfileStringA(LPCSTR lpAppName,LPCSTR lpKeyName,LPCSTR lpString,LPCSTR lpFileName);
FailOnFalse  [gle]  WritePrivateProfileStringW(LPCWSTR lpAppName,LPCWSTR lpKeyName,LPCWSTR lpString,LPCWSTR lpFileName);
FailOnFalse  [gle]  WritePrivateProfileStructA(LPCSTR lpszSection,LPCSTR lpszKey,LPVOID lpStruct,UINT uSizeStruct,LPCSTR szFile);
FailOnFalse  [gle]  WritePrivateProfileStructW(LPCWSTR lpszSection,LPCWSTR lpszKey,LPVOID lpStruct,UINT uSizeStruct,LPCWSTR szFile);
FailOnFalse  [gle]  WriteProfileSectionA(LPCSTR lpAppName,LPCSTR lpString);
FailOnFalse  [gle]  WriteProfileSectionW(LPCWSTR lpAppName,LPCWSTR lpString);
FailOnFalse  [gle]  WriteProfileStringA(LPCSTR lpAppName,LPCSTR lpKeyName,LPCSTR lpString);
FailOnFalse  [gle]  WriteProfileStringW(LPCWSTR lpAppName,LPCWSTR lpKeyName,LPCWSTR lpString);

module  ADVAPI32.DLL:

WinError    RegCloseKey( [da] HandleToOpenKey hKey);
WinError    RegConnectRegistryA(LPCSTR lpMachineName,HandleToOpenKey hKey,[out] HandleToOpenKey* phkResult);
WinError    RegConnectRegistryW(LPCWSTR lpMachineName,HandleToOpenKey hKey,[out] HandleToOpenKey* phkResult);
WinError    RegCreateKeyA(HandleToOpenKey hKey,LPCSTR lpSubKey,[out] HandleToOpenKey* phkResult);
WinError    RegCreateKeyW(HandleToOpenKey hKey,LPCWSTR lpSubKey,[out] HandleToOpenKey* phkResult);
WinError    RegCreateKeyExA(HandleToOpenKey hKey,LPCSTR lpSubKey,DWORD Reserved,LPSTR lpClass,SpecialOptions dwOptions,DesiredSecurityAccess samDesired,LPSECURITY_ATTRIBUTES lpSecurityAttributes,[out] HandleToOpenKey* phkResult,[out] DispositionValue lpdwDisposition);
WinError    RegCreateKeyExW(HandleToOpenKey hKey,LPCWSTR lpSubKey,DWORD Reserved,LPWSTR lpClass,SpecialOptions dwOptions,DesiredSecurityAccess samDesired,LPSECURITY_ATTRIBUTES lpSecurityAttributes,[out] HandleToOpenKey* phkResult,[out] DispositionValue lpdwDisposition);
WinError    RegDeleteKeyA(HandleToOpenKey hKey,LPCSTR lpSubKey);
WinError    RegDeleteKeyW(HandleToOpenKey hKey,LPCWSTR lpSubKey);
WinError    RegDeleteValueA(HandleToOpenKey hKey,LPCSTR lpValueName);
WinError    RegDeleteValueW(HandleToOpenKey hKey,LPCWSTR lpValueName);
WinError    RegEnumKeyA(HandleToOpenKey hKey,DWORD dwIndex,[out] LPSTR lpName,DWORD cbName);
WinError    RegEnumKeyW(HandleToOpenKey hKey,DWORD dwIndex,[out] LPWSTR lpName,DWORD cbName);
WinError    RegEnumKeyExA(HandleToOpenKey hKey,DWORD dwIndex,[out] LPSTR lpName,[out] LPDWORD lpcName,LPDWORD lpReserved,[out] LPSTR lpClass,[out] LPDWORD lpcClass,[out] PFILETIME lpftLastWriteTime);
WinError    RegEnumKeyExW(HandleToOpenKey hKey,DWORD dwIndex,[out] LPWSTR lpName,[out] LPDWORD lpcbName,LPDWORD lpReserved,[out] LPWSTR lpClass,[out] LPDWORD lpcbClass,[out] PFILETIME lpftLastWriteTime);
WinError    RegEnumValueA(HandleToOpenKey hKey,DWORD dwIndex,[out] LPSTR lpValueName,[out] LPDWORD lpcValueName,LPDWORD lpReserved,[out] LPDWORD lpType,[out] LPBYTE lpData,LPDWORD lpcbData);
WinError    RegEnumValueW(HandleToOpenKey hKey,DWORD dwIndex,[out] LPWSTR lpValueName,[out] LPDWORD lpcbValueName,LPDWORD lpReserved,[out] LPDWORD lpType,[out] LPBYTE lpData,LPDWORD lpcbData);
WinError    RegFlushKey(HandleToOpenKey hKey);
WinError    RegLoadKeyA(HandleToOpenKey hKey,LPCSTR lpSubKey,LPCSTR lpFile);
WinError    RegLoadKeyW(HandleToOpenKey    hKey,LPCWSTR  lpSubKey,LPCWSTR  lpFile);
WinError    RegNotifyChangeKeyValue(HandleToOpenKey hKey,BOOL bWatchSubtree,ChangesToReport dwNotifyFilter,HANDLE hEvent,BOOL fAsynchronous);
WinError    RegOpenCurrentUser(DesiredSecurityAccess samDesired,[out] HandleToOpenKey* phkResult);
WinError    RegOpenKeyA(HandleToOpenKey hKey,LPCSTR lpSubKey,[out] HandleToOpenKey* phkResult);
WinError    RegOpenKeyW(HandleToOpenKey hKey,LPCWSTR lpSubKey,[out] HandleToOpenKey* phkResult);
WinError    RegOpenKeyExA(HandleToOpenKey hKey,LPCSTR lpSubKey,DWORD ulOptions,DesiredSecurityAccess samDesired,[out] HandleToOpenKey* phkResult);
WinError    RegOpenKeyExW(HandleToOpenKey hKey,LPCWSTR lpSubKey,DWORD ulOptions,DesiredSecurityAccess samDesired,[out] HandleToOpenKey* phkResult);
WinError    RegOpenUserClassesRoot(HANDLE hToken,DWORD  dwOptions,DesiredSecurityAccess samDesired,[out] HandleToOpenKey*  phkResult);
WinError    RegOverridePredefKey(HandleToOpenKey hKey,HandleToOpenKey hNewHKey);
WinError    RegQueryInfoKeyA(HandleToOpenKey hKey,[out] LPSTR lpClass,[out] LPDWORD lpcClass,LPDWORD lpReserved,[out] LPDWORD lpcSubKeys,[out] LPDWORD lpcMaxSubKeyLen,[out] LPDWORD lpcMaxClassLen,[out] LPDWORD lpcValues,[out] LPDWORD lpcMaxValueNameLen,[out] LPDWORD lpcMaxValueLen,[out] LPDWORD lpcbSecurityDescriptor,[out] PFILETIME lpftLastWriteTime);
WinError    RegQueryInfoKeyW(HandleToOpenKey hKey,[out] LPWSTR lpClass,[out] LPDWORD lpcbClass,LPDWORD lpReserved,[out] LPDWORD lpcSubKeys,[out] LPDWORD lpcbMaxSubKeyLen,[out] LPDWORD lpcbMaxClassLen,[out] LPDWORD lpcValues,    [out] LPDWORD lpcbMaxValueNameLen,[out] LPDWORD lpcbMaxValueLen,[out] LPDWORD lpcbSecurityDescriptor,[out] PFILETIME lpftLastWriteTime);
WinError    RegQueryMultipleValuesA(HandleToOpenKey hKey,[out] LPVOID val_list,DWORD num_vals,[out] LPSTR lpValueBuf,[out] LPDWORD ldwTotsize);
WinError    RegQueryMultipleValuesW(HandleToOpenKey hKey,[out] LPVOID val_list,DWORD num_vals,[out] LPWSTR lpValueBuf,[out] LPDWORD ldwTotsize);
WinError    RegQueryValueA(HandleToOpenKey hKey,LPCSTR lpSubKey,[out] LPSTR lpValue,[out] PLONG lpcbValue);
WinError    RegQueryValueW(HandleToOpenKey hKey,LPCWSTR lpSubKey,[out] LPWSTR lpValue,[out] PLONG   lpcbValue);
WinError    RegQueryValueExA(HandleToOpenKey hKey,LPCSTR lpValueName,LPDWORD lpReserved,[out] LPDWORD lpType,[out] LPBYTE lpData,[out] LPDWORD lpcbData);
WinError    RegQueryValueExW(HandleToOpenKey hKey,LPCWSTR lpValueName,LPDWORD lpReserved,[out] LPDWORD lpType,[out] LPBYTE lpData,[out] LPDWORD lpcbData);
WinError    RegReplaceKeyA(HandleToOpenKey hKey,LPCSTR lpSubKey,LPCSTR lpNewFile,LPCSTR lpOldFile);
WinError    RegReplaceKeyW(HandleToOpenKey  hKey,LPCWSTR  lpSubKey,LPCWSTR  lpNewFile,LPCWSTR  lpOldFile);
WinError    RegRestoreKeyA(HandleToOpenKey hKey,LPCSTR lpFile,DWORD dwFlags);
WinError    RegRestoreKeyW(HandleToOpenKey hKey,LPCWSTR lpFile,DWORD   dwFlags);
WinError    RegSaveKeyA(HandleToOpenKey hKey,LPCSTR lpFile,LPSECURITY_ATTRIBUTES lpSecurityAttributes);
WinError    RegSaveKeyW(HandleToOpenKey hKey,LPCWSTR lpFile,LPSECURITY_ATTRIBUTES lpSecurityAttributes);
WinError    RegSetValueA(HandleToOpenKey hKey,LPCSTR lpSubKey,DWORD dwType,LPCSTR lpData,DWORD cbData);
WinError    RegSetValueW(HandleToOpenKey hKey,LPCWSTR lpSubKey,DWORD dwType,LPCWSTR lpData,DWORD cbData);
WinError    RegSetValueExA(HandleToOpenKey hKey,LPCSTR lpValueName,DWORD Reserved,DWORD dwType,BYTE *lpData,DWORD cbData);
WinError    RegSetValueExW(HandleToOpenKey hKey,LPCWSTR lpValueName,DWORD Reserved,DWORD dwType,BYTE* lpData,DWORD cbData);
WinError    RegUnLoadKeyA(HandleToOpenKey hKey,LPCSTR lpSubKey);
WinError    RegUnLoadKeyW(HandleToOpenKey hKey,LPCWSTR lpSubKey);
