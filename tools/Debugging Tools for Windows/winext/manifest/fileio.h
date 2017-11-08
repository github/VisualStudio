// +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
//
//                         File I/O Functions
//
// +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
category IOFunctions:
module KERNEL32.DLL:

value long FileErrorReturnValue
{
#define HFILE_ERROR -1
};

value int _lcreatFileAttributes
{
#define Normal          0
#define Read_Only       1
#define Hidden          2
#define System          3
};

mask DWORD CopyFlags
{
#define COPY_FILE_FAIL_IF_EXISTS        0x00000001
#define COPY_FILE_RESTARTABLE           0x00000002
#define COPY_FILE_OPEN_SOURCE_FOR_WRITE 0x00000004
};

value DWORD CallBackReasons
{
#define CALLBACK_CHUNK_FINISHED         0x00000000
#define CALLBACK_STREAM_SWITCH          0x00000001
};

value DWORD CopyRoutineReturns
{
#define PROGRESS_CONTINUE   0
#define PROGRESS_CANCEL     1
#define PROGRESS_STOP       2
#define PROGRESS_QUIET      3
};

mask DWORD DefineDosDeviceFlags
{
#define DDD_RAW_TARGET_PATH         0x00000001
#define DDD_REMOVE_DEFINITION       0x00000002
#define DDD_EXACT_MATCH_ON_REMOVE   0x00000004
#define DDD_NO_BROADCAST_SYSTEM     0x00000008
};

mask DWORD ChangeNotifications
{
#define FILE_NOTIFY_CHANGE_FILE_NAME    0x00000001
#define FILE_NOTIFY_CHANGE_DIR_NAME     0x00000002
#define FILE_NOTIFY_CHANGE_ATTRIBUTES   0x00000004
#define FILE_NOTIFY_CHANGE_SIZE         0x00000008
#define FILE_NOTIFY_CHANGE_LAST_WRITE   0x00000010
#define FILE_NOTIFY_CHANGE_LAST_ACCESS  0x00000020
#define FILE_NOTIFY_CHANGE_CREATION     0x00000040
#define FILE_NOTIFY_CHANGE_SECURITY     0x00000100
};

value LONG _FILEEX_INFO_LEVELS
{
#define FindExInfoStandard          0
#define FindExInfoMaxInfoLevel      1
};

value LONG OpenFileStyle
{
#define OF_READ         0
#define OF_WRITE        1
#define OF_READ_WRITE   2
};


value LONG FINDEX_INFO_LEVELS
{
#define   FindExSearchNameMatch             0
#define   FindExSearchLimitToDirectories   1
#define   FindExSearchLimitToDevices       2
};

value LONG FINDEX_SEARCH_OPS
{
#define FindExSearchNameMatch               0
#define FindExSearchLimitToDirectories      1
#define FindExSearchLimitToDevices          2
};

value LPDWORD GetBinaryTypeValues
{
#define SCS_32BIT_BINARY    0
#define SCS_DOS_BINARY      1
#define SCS_WOW_BINARY      2
#define SCS_PIF_BINARY      3
#define SCS_POSIX_BINARY    4
#define SCS_OS216_BINARY    5
};

value UINT DriveTypes
{
#define DRIVE_UNKNOWN     0
#define DRIVE_NO_ROOT_DIR 1
#define DRIVE_REMOVABLE   2
#define DRIVE_FIXED       3
#define DRIVE_REMOTE      4
#define DRIVE_CDROM       5
#define DRIVE_RAMDISK     6
};

value DWORD SPDataFlags
{
#define SPINT_ACTIVE  0x00000001
#define SPINT_DEFAULT 0x00000002
#define SPINT_REMOVED 0x00000004
};

typedef struct _SP_DEVICE_INTERFACE_DATA {
    DWORD cbSize;
    GUID  InterfaceClassGuid;
    SPDataFlags  Flags;
    ULONG_PTR Reserved;
} SP_DEVICE_INTERFACE_DATA, *PSP_DEVICE_INTERFACE_DATA;

typedef struct _SP_DEVINFO_LIST_DETAIL_DATA_A {
    DWORD  cbSize;
    GUID   ClassGuid;
    HANDLE RemoteMachineHandle;
    CHAR   RemoteMachineName[263];
} SP_DEVINFO_LIST_DETAIL_DATA_A, *PSP_DEVINFO_LIST_DETAIL_DATA_A;

typedef struct _SP_DEVINFO_LIST_DETAIL_DATA_W {
    DWORD  cbSize;
    GUID   ClassGuid;
    HANDLE RemoteMachineHandle;
    WCHAR  RemoteMachineName[263];
} SP_DEVINFO_LIST_DETAIL_DATA_W, *PSP_DEVINFO_LIST_DETAIL_DATA_W;

typedef struct _SP_DEVICE_INTERFACE_DETAIL_DATA_A {
    DWORD  cbSize;
    CHAR   DevicePath[1];
} SP_DEVICE_INTERFACE_DETAIL_DATA_A, *PSP_DEVICE_INTERFACE_DETAIL_DATA_A;

typedef struct _SP_DEVICE_INTERFACE_DETAIL_DATA_W {
    DWORD  cbSize;
    WCHAR  DevicePath[1];
} SP_DEVICE_INTERFACE_DETAIL_DATA_W, *PSP_DEVICE_INTERFACE_DETAIL_DATA_W;

value LONG GET_FILEEX_INFO_LEVELS
{
#define   GetFileExInfoStandard 0
};

typedef struct _BY_HANDLE_FILE_INFORMATION {
    FileFlagsAndAttributes    dwFileAttributes;
    FILETIME ftCreationTime;
    FILETIME ftLastAccessTime;
    FILETIME ftLastWriteTime;
    DWORD    dwVolumeSerialNumber;
    DWORD    nFileSizeHigh;
    DWORD    nFileSizeLow;
    DWORD    nNumberOfLinks;
    DWORD    nFileIndexHigh;
    DWORD    nFileIndexLow;
} BY_HANDLE_FILE_INFORMATION, *LPBY_HANDLE_FILE_INFORMATION;


typedef struct _OFSTRUCT {
    BYTE cBytes;
    BYTE fFixedDisk;
    WORD nErrCode;
    WORD Reserved1;
    WORD Reserved2;
    CHAR szPathName[128];
} OFSTRUCT, *LPOFSTRUCT, *POFSTRUCT;

value DWORD GetFileTypeReturnValue
{
#define FILE_TYPE_UNKNOWN   0x0000
#define FILE_TYPE_DISK      0x0001
#define FILE_TYPE_CHAR      0x0002
#define FILE_TYPE_PIPE      0x0003
#define FILE_TYPE_REMOTE    0x8000
};

value DWORD LockOptions
{
#define LOCKFILE_FAIL_IMMEDIATELY   0x00000001
#define LOCKFILE_EXCLUSIVE_LOCK     0x00000002
};

mask DWORD MoveFilePossibilities
{
#define MOVEFILE_REPLACE_EXISTING       0x00000001
#define MOVEFILE_COPY_ALLOWED           0x00000002
#define MOVEFILE_DELAY_UNTIL_REBOOT     0x00000004
#define MOVEFILE_WRITE_THROUGH          0x00000008
#define MOVEFILE_CREATE_HARDLINK        0x00000010
#define MOVEFILE_FAIL_IF_NOT_TRACKABLE  0x00000020
};

value UINT OpenFileActions
{
#define OF_READ             0x00000000
#define OF_WRITE            0x00000001
#define OF_READWRITE        0x00000002
#define OF_SHARE_COMPAT     0x00000000
#define OF_SHARE_EXCLUSIVE  0x00000010
#define OF_SHARE_DENY_WRITE 0x00000020
#define OF_SHARE_DENY_READ  0x00000030
#define OF_SHARE_DENY_NONE  0x00000040
#define OF_PARSE            0x00000100
#define OF_DELETE           0x00000200
#define OF_VERIFY           0x00000400
#define OF_CANCEL           0x00000800
#define OF_CREATE           0x00001000
#define OF_PROMPT           0x00002000
#define OF_EXIST            0x00004000
#define OF_REOPEN           0x00008000
};


value DWORD FileChangeType
{
#define FILE_ACTION_ADDED                   0x00000001
#define FILE_ACTION_REMOVED                 0x00000002
#define FILE_ACTION_MODIFIED                0x00000003
#define FILE_ACTION_RENAMED_OLD_NAME        0x00000004
#define FILE_ACTION_RENAMED_NEW_NAME        0x00000005
};

typedef struct _FILE_NOTIFY_INFORMATION {
    DWORD NextEntryOffset;
    FileChangeType Action;
    DWORD FileNameLength;
    WCHAR FileName[1];
} FILE_NOTIFY_INFORMATION, *LPFILE_NOTIFY_INFORMATION;

typedef DWORD NumberOfClusters;
alias NumberOfClusters;

typedef DWORD SectorsPerCluster;
alias SectorsPerCluster;


value DWORD SetFilePointerReturn
{
#define INVALID_SET_POINTER         -1 [fail]
};

FileErrorReturnValue [gle] _hread(
                                  HFILE hFile,
                                  [out] LPVOID lpBuffer,
                                  long lBytes
                                  );

FileErrorReturnValue [gle] _hwrite(
                                   HFILE hFile,
                                   LPCSTR lpBuffer,
                                   long lBytes
                                   );

FileErrorReturnValue [gle] _lclose(
                                   [out] HFILE hFile
                                   );

FileErrorReturnValue [gle] _lcreat(
                                   LPCSTR lpPathName,
                                   _lcreatFileAttributes iAttribute
                                   );

FileErrorReturnValue [gle] _llseek(
                                   HFILE hFile,
                                   LONG lOffset,
                                   FilePointerStartingPosition iOrigin
                                   );

FileErrorReturnValue [gle] _lopen(
                                  LPCSTR lpPathName,
                                  OpenFileStyle iReadWrite
                                  );

FileErrorReturnValue [gle] _lread(
                                  HFILE hFile,
                                  [out] LPVOID lpBuffer,
                                  UINT uBytes
                                  );

FileErrorReturnValue [gle] _lwrite(
                                   HFILE hFile,
                                   LPCSTR lpBuffer,
                                   UINT uBytes
                                   );

FailOnFalse AreFileApisANSI();

FailOnFalse [gle] CancelIo(
                           HANDLE hFile
                           );

FailOnFalse [gle] CopyFileA(
                            LPCSTR lpExistingFileName,
                            LPCSTR lpNewFileName,
                            BOOL bFailIfExists
                            );

FailOnFalse [gle] CopyFileW(
                            LPCWSTR lpExistingFileName,
                            LPCWSTR lpNewFileName,
                            BOOL bFailIfExists
                            );

FailOnFalse [gle] CopyFileExA(
                              LPCSTR lpExistingFileName,
                              LPCSTR lpNewFileName,
                              LPVOID lpProgressRoutine,
                              LPVOID lpData,
                              LPBOOL pbCancel,
                              CopyFlags dwCopyFlags
                              );

FailOnFalse [gle] CopyFileExW(
                              LPCWSTR lpExistingFileName,
                              LPCWSTR lpNewFileName,
                              LPVOID lpProgressRoutine,
                              LPVOID lpData,
                              LPBOOL pbCancel,
                              CopyFlags dwCopyFlags
                              );

FailOnFalse [gle] CreateDirectoryA(
                                   LPCSTR lpPathName,
                                   LPSECURITY_ATTRIBUTES lpSecurityAttributes
                                   );

FailOnFalse [gle] CreateDirectoryW(
                                   LPCWSTR lpPathName,
                                   LPSECURITY_ATTRIBUTES lpSecurityAttributes
                                   );

FailOnFalse [gle] CreateDirectoryExA(
                                     LPCSTR lpTemplateDirectory,
                                     LPCSTR lpNewDirectory,
                                     LPSECURITY_ATTRIBUTES lpSecurityAttributes
                                     );

FailOnFalse [gle] CreateDirectoryExW(
                                     LPCWSTR lpTemplateDirectory,
                                     LPCWSTR lpNewDirectory,
                                     LPSECURITY_ATTRIBUTES lpSecurityAttributes
                                     );

HANDLE [gle] CreateFileA(
                              LPCSTR lpFileName,
                              DWORD dwDesiredAccess,
                              GenericAccessRights dwShareMode,
                              LPSECURITY_ATTRIBUTES lpSecurityAttributes,
                              CreationActions dwCreationDisposition,
                              FileFlagsAndAttributes dwFlagsAndAttributes,
                              HANDLE hTemplateFile
                              );

HANDLE [gle] CreateFileW(
                              LPCWSTR lpFileName,
                              GenericAccessRights dwDesiredAccess,
                              ShareRights dwShareMode,
                              LPSECURITY_ATTRIBUTES lpSecurityAttributes,
                              CreationActions dwCreationDisposition,
                              FileFlagsAndAttributes dwFlagsAndAttributes,
                              HANDLE hTemplateFile
                              );

HANDLE [gle] CreateIoCompletionPort (
                                               HANDLE FileHandle,
                                               HANDLE ExistingCompletionPort,
                                               ULONG_PTR CompletionKey,
                                               DWORD NumberOfConcurrentThreads
                                               );

FailOnFalse [gle] DefineDosDeviceA(
                                   DefineDosDeviceFlags dwFlags,
                                   LPCSTR lpDeviceName,
                                   LPCSTR lpTargetPath
                                   );

FailOnFalse [gle] DefineDosDeviceW(
                                   DefineDosDeviceFlags dwFlags,
                                   LPCWSTR lpDeviceName,
                                   LPCWSTR lpTargetPath
                                   );

FailOnFalse [gle] DeleteFileA(
                              LPCSTR lpFileName
                              );

FailOnFalse [gle] DeleteFileW(
                              LPCWSTR lpFileName
                              );

FailOnFalse [gle] FindClose(
                            [out] HANDLE hFindFile
                            );

FailOnFalse [gle] FindCloseChangeNotification(
                                              HANDLE hChangeHandle
                                              );

HANDLE [gle] FindFirstChangeNotificationA(
                                                       LPCSTR lpPathName,
                                                       BOOL bWatchSubtree,
                                                       DWORD dwNotifyFilter
                                                       );

HANDLE [gle] FindFirstChangeNotificationW(
                                                       LPCWSTR lpPathName,
                                                       BOOL bWatchSubtree,
                                                       DWORD ChangeNotifications
                                                       );

HANDLE [gle] FindFirstFileA(
                                         LPCSTR lpFileName,
                                         [out] LPWIN32_FIND_DATAA lpFindFileData
                                         );

HANDLE [gle] FindFirstFileW(
                                         LPCWSTR lpFileName,
                                         [out] LPWIN32_FIND_DATAW lpFindFileData
                                         );

HANDLE [gle] FindFirstFileExA(
                                           LPCSTR lpFileName,
                                           FINDEX_INFO_LEVELS fInfoLevelId,
                                           [out] LPWIN32_FIND_DATAA lpFindFileData,
                                           FINDEX_SEARCH_OPS fSearchOp,
                                           LPVOID lpSearchFilter,
                                           DWORD dwAdditionalFlags
                                           );

HANDLE [gle] FindFirstFileExW(
                                           LPCWSTR lpFileName,
                                           FINDEX_INFO_LEVELS fInfoLevelId,
                                           [out] LPWIN32_FIND_DATAW lpFindFileData,
                                           FINDEX_SEARCH_OPS fSearchOp,
                                           LPVOID lpSearchFilter,
                                           DWORD dwAdditionalFlags
                                           );

FailOnFalse [gle] FindNextChangeNotification(
                                             HANDLE hChangeHandle
                                             );

FailOnFalse [gle] FindNextFileA(
                                HANDLE hFindFile,
                                [out] LPWIN32_FIND_DATAA lpFindFileData
                                );

FailOnFalse [gle] FindNextFileW(
                                HANDLE hFindFile,
                                [out] LPWIN32_FIND_DATAW lpFindFileData
                                );

FailOnFalse [gle] FlushFileBuffers(
                                   HANDLE hFile
                                   );

FailOnFalse [gle] GetBinaryTypeA(
                                 LPCSTR lpApplicationName,
                                 [out] GetBinaryTypeValues lpBinaryType
                                 );

FailOnFalse [gle] GetBinaryTypeW(
                                 LPCWSTR lpApplicationName,
                                 [out] GetBinaryTypeValues lpBinaryType
                                 );

DwordFailIfZero [gle] GetCurrentDirectoryA(
                                           DWORD nBufferLength,
                                           [out] LPSTR lpBuffer
                                           );

DwordFailIfZero [gle] GetCurrentDirectoryW(
                                           DWORD nBufferLength,
                                           [out] LPWSTR lpBuffer
                                           );

DwordFailIfZero [gle] GetWindowsDirectoryA(
                                   [out] LPSTR lpBuffer,
                                   UINT uSize
                                   );

DwordFailIfZero [gle] GetWindowsDirectoryW(
                                   [out] LPWSTR lpBuffer,
                                   UINT uSize
                                   );

DwordFailIfZero [gle] GetSystemDirectoryA(
                                   [out] LPSTR lpBuffer,
                                   UINT uSize
                                   );

DwordFailIfZero [gle] GetSystemDirectoryW(
                                   [out] LPWSTR lpBuffer,
                                   UINT uSize
                                   );

DwordFailIfZero [gle] GetSystemWindowsDirectoryA(
                                   [out] LPSTR lpBuffer,
                                   UINT uSize
                                   );

DwordFailIfZero [gle] GetSystemWindowsDirectoryW(
                                   [out] LPWSTR lpBuffer,
                                   UINT uSize
                                   );

FailOnFalse [gle] GetDiskFreeSpaceA(
                                    LPCSTR lpRootPathName,
                                    [out] SectorsPerCluster* lpSectorsPerCluster,
                                    [out] LPDWORD lpBytesPerSector,
                                    [out] NumberOfClusters* lpNumberOfFreeClusters,
                                    [out] NumberOfClusters* lpTotalNumberOfClusters
                                    );

FailOnFalse [gle] GetDiskFreeSpaceW(
                                    LPCWSTR lpRootPathName,
                                    [out] SectorsPerCluster* lpSectorsPerCluster,
                                    [out] LPDWORD lpBytesPerSector,
                                    [out] NumberOfClusters* lpNumberOfFreeClusters,
                                    [out] NumberOfClusters* lpTotalNumberOfClusters
                                    );

typedef DWORD DiskBytesDWORD;
alias DiskBytesDWORD;

typedef struct _DiskBytes
{
    DiskBytesDWORD   Low;
    DiskBytesDWORD   High;
} DiskBytes,*PDiskBytes;


FailOnFalse [gle] GetDiskFreeSpaceExA(
                                      LPCSTR lpDirectoryName,
                                      [out] PDiskBytes lpFreeBytesAvailable,
                                      [out] PDiskBytes lpTotalNumberOfBytes,
                                      [out] PDiskBytes lpTotalNumberOfFreeBytes
                                      );

FailOnFalse [gle] GetDiskFreeSpaceExW(
                                      LPCWSTR lpDirectoryName,
                                      [out] PDiskBytes lpFreeBytesAvailableToCaller,
                                      [out] PDiskBytes lpTotalNumberOfBytes,
                                      [out] PDiskBytes lpTotalNumberOfFreeBytes
                                      );

DriveTypes GetDriveTypeA(
                         LPCSTR lpRootPathName
                         );

DriveTypes GetDriveTypeW(
                         LPCWSTR lpRootPathName
                         );

FileFlagsAndAttributes [gle] GetFileAttributesA(
                                                LPCSTR lpFileName
                                                );

FileFlagsAndAttributes [gle] GetFileAttributesW(
                                                LPCWSTR lpFileName
                                                );

FailOnFalse [gle] GetFileAttributesExA(
                                       LPCSTR lpFileName,
                                       GET_FILEEX_INFO_LEVELS fInfoLevelId,
                                       [out] LPVOID lpFileInformation
                                       );

FailOnFalse [gle] GetFileAttributesExW(
                                       LPCWSTR lpFileName,
                                       GET_FILEEX_INFO_LEVELS fInfoLevelId,
                                       [out] LPVOID lpFileInformation
                                       );

FailOnFalse [gle] GetFileInformationByHandle(
                                             HANDLE hFile,
                                             [out] LPBY_HANDLE_FILE_INFORMATION lpFileInformation
                                             );

DwordFailIfNeg1 [gle] GetFileSize(
                                  HANDLE hFile,
                                  [out] LPDWORD lpFileSizeHigh
                                  );

FailOnFalse [gle] GetFileSizeEx(
                                HANDLE hFile,
                                [out] PLARGE_INTEGER lpFileSize
                                );

GetFileTypeReturnValue GetFileType(
                                   HANDLE hFile
                                   );

DwordFailIfZero [gle] GetFullPathNameA(
                                       LPCSTR lpFileName,
                                       DWORD nBufferLength,
                                       [out] LPSTR lpBuffer,
                                       [out] LPSTR *lpFilePart
                                       );

DwordFailIfZero [gle] GetFullPathNameW(
                                       LPCWSTR lpFileName,
                                       DWORD nBufferLength,
                                       [out] LPWSTR lpBuffer,
                                       [out] LPWSTR *lpFilePart
                                       );

DwordFailIfZero [gle] GetLogicalDrives();

DwordFailIfZero [gle] GetLogicalDriveStringsA(
                                              DWORD nBufferLength,
                                              [out] LPSTR lpBuffer
                                              );

DwordFailIfZero [gle] GetLogicalDriveStringsW(
                                              DWORD nBufferLength,
                                              [out] LPWSTR lpBuffer
                                              );

DwordFailIfZero [gle] GetLongPathNameA(
                                       LPCSTR lpszShortPath,
                                       [out] LPSTR  lpszLongPath,
                                       DWORD    cchBuffer
                                       );

DwordFailIfZero [gle] GetLongPathNameW(
                                       LPCWSTR lpszShortPath,
                                       [out] LPWSTR  lpszLongPath,
                                       DWORD    cchBuffer
                                       );

FailOnFalse [gle] GetQueuedCompletionStatus(
                                            HANDLE CompletionPort,
                                            [out] LPDWORD lpNumberOfBytes,
                                            [out] PULONG_PTR lpCompletionKey,
                                            [out] LPOVERLAPPED *lpOverlapped,
                                            DWORD dwMilliseconds
                                            );

DwordFailIfZero [gle] GetShortPathNameA(
                                        LPCSTR lpszLongPath,
                                        [out] LPSTR  lpszShortPath,
                                        DWORD    cchBuffer
                                        );

DwordFailIfZero [gle] GetShortPathNameW(
                                        LPCWSTR lpszLongPath,
                                        [out] LPWSTR  lpszShortPath,
                                        DWORD    cchBuffer
                                        );

UintFailIfZero [gle] GetTempFileNameA(
                                      LPCSTR lpPathName,
                                      LPCSTR lpPrefixString,
                                      UINT uUnique,
                                      [out] LPSTR lpTempFileName
                                      );

UintFailIfZero [gle] GetTempFileNameW(
                                      LPCWSTR lpPathName,
                                      LPCWSTR lpPrefixString,
                                      UINT uUnique,
                                      [out] LPWSTR lpTempFileName
                                      );

DwordFailIfZero [gle] GetTempPathA(
                                   DWORD nBufferLength,
                                   [out] LPSTR lpBuffer
                                   );

DwordFailIfZero [gle] GetTempPathW(
                                   DWORD nBufferLength,
                                   [out] LPWSTR lpBuffer
                                   );

FailOnFalse [gle] LockFile(
                           HANDLE hFile,
                           DWORD dwFileOffsetLow,
                           DWORD dwFileOffsetHigh,
                           DWORD nNumberOfBytesToLockLow,
                           DWORD nNumberOfBytesToLockHigh
                           );

FailOnFalse [gle] LockFileEx(
                             HANDLE hFile,
                             LockOptions dwFlags,
                             DWORD dwReserved,
                             DWORD nNumberOfBytesToLockLow,
                             DWORD nNumberOfBytesToLockHigh,
                             LPOVERLAPPED lpOverlapped
                             );

FailOnFalse [gle] MoveFileA(
                            LPCSTR lpExistingFileName,
                            LPCSTR lpNewFileName
                            );

FailOnFalse [gle] MoveFileW(
                            LPCWSTR lpExistingFileName,
                            LPCWSTR lpNewFileName
                            );

FailOnFalse [gle] MoveFileExA(
                              LPCSTR lpExistingFileName,
                              LPCSTR lpNewFileName,
                              MoveFilePossibilities dwFlags
                              );

FailOnFalse [gle] MoveFileExW(
                              LPCWSTR lpExistingFileName,
                              LPCWSTR lpNewFileName,
                              MoveFilePossibilities dwFlags
                              );

FailOnFalse [gle] MoveFileWithProgressA(
                                        LPCSTR lpExistingFileName,
                                        LPCSTR lpNewFileName,
                                        LPVOID lpProgressRoutine ,
                                        LPVOID lpData ,
                                        DWORD dwFlags
                                        );

FailOnFalse [gle] MoveFileWithProgressW(
                                        LPCWSTR lpExistingFileName,
                                        LPCWSTR lpNewFileName,
                                        LPVOID  lpProgressRoutine ,
                                        LPVOID lpData ,
                                        MoveFilePossibilities dwFlags
                                        );

IntFailIfNeg1 MulDiv(
                     int nNumber,
                     int nNumerator,
                     int nDenominator
                     );

HFILE OpenFile(
               LPCSTR lpFileName,
               [out] LPOFSTRUCT lpReOpenBuff,
               OpenFileActions uStyle
               );

FailOnFalse [gle] PostQueuedCompletionStatus(
                                             HANDLE CompletionPort,
                                             DWORD dwNumberOfBytesTransferred,
                                             ULONG_PTR dwCompletionKey,
                                             LPOVERLAPPED lpOverlapped
                                             );

FailOnFalse [gle] PrivCopyFileExW(
                                  LPCWSTR lpExistingFileName,
                                  LPCWSTR lpNewFileName,
                                  LPVOID lpProgressRoutine,
                                  LPVOID lpData,
                                  LPBOOL pbCancel,
                                  CopyFlags dwCopyFlags
                                  );

DwordFailIfZero [gle] QueryDosDeviceA(
                                      LPCSTR lpDeviceName,
                                      [out] LPSTR lpTargetPath,
                                      DWORD ucchMax
                                      );

DwordFailIfZero [gle] QueryDosDeviceW(
                                      LPCWSTR lpDeviceName,
                                      [out] LPWSTR lpTargetPath,
                                      DWORD ucchMax
                                      );

FailOnFalse [gle] ReadDirectoryChangesW(
                                        HANDLE hDirectory,
                                        [out] LPVOID lpBuffer,
                                        DWORD nBufferLength,
                                        BOOL bWatchSubtree,
                                        DWORD dwNotifyFilter,
                                        LPDWORD lpBytesReturned,
                                        LPOVERLAPPED lpOverlapped,
                                        LPVOID lpCompletionRoutine
                                        );

FailOnFalse [gle] ReadFile(
                           HANDLE hFile,
                           [out] LPVOID lpBuffer,
                           DWORD nNumberOfBytesToRead,
                           LPDWORD lpNumberOfBytesRead,
                           LPOVERLAPPED lpOverlapped
                           );

FailOnFalse [gle] ReadFileEx(
                             HANDLE hFile,
                             [out] LPVOID lpBuffer,
                             DWORD nNumberOfBytesToRead,
                             LPOVERLAPPED lpOverlapped,
                             LPVOID lpCompletionRoutine
                             );

// cjc ULONGLONGFILE_SEGMENT_ELEMENT aSegmentArray[ ],
FailOnFalse [gle] ReadFileScatter(
                                  HANDLE hFile,
                                  ULONGLONG aSegmentArray,
                                  DWORD nNumberOfBytesToRead,
                                  LPDWORD lpReserved,
                                  LPOVERLAPPED lpOverlapped
                                  );

FailOnFalse [gle] RemoveDirectoryA(
                                   LPCSTR lpPathName
                                   );

FailOnFalse [gle] RemoveDirectoryW(
                                   LPCWSTR lpPathName
                                   );

FailOnFalse [gle] ReplaceFile(
                              LPCSTR lpReplacedFileName,
                              LPCSTR lpReplacementFileName,
                              LPCSTR lpBackupFileName,
                              DWORD dwReplaceFlags,
                              LPVOID lpExclude,
                              LPVOID lpReserved
                              );

FailOnFalse [gle] ReplaceFileW(
                              LPCWSTR lpReplacedFileName,
                              LPCWSTR lpReplacementFileName,
                              LPCWSTR lpBackupFileName,
                              DWORD dwReplaceFlags,
                              LPVOID lpExclude,
                              LPVOID lpReserved
                              );

DwordFailIfZero [gle] SearchPathA(
                                  LPCSTR lpPath,
                                  LPCSTR lpFileName,
                                  LPCSTR lpExtension,
                                  DWORD nBufferLength,
                                  [out] LPSTR lpBuffer,
                                  [out] LPSTR *lpFilePart
                                  );

DwordFailIfZero [gle] SearchPathW(
                                  LPCWSTR lpPath,
                                  LPCWSTR lpFileName,
                                  LPCWSTR lpExtension,
                                  DWORD nBufferLength,
                                  [out] LPWSTR lpBuffer,
                                  [out] LPWSTR *lpFilePart
                                  );

FailOnFalse [gle] SetCurrentDirectoryA(
                                       LPCSTR lpPathName
                                       );

FailOnFalse [gle] SetCurrentDirectoryW(
                                       LPCWSTR lpPathName
                                       );

FailOnFalse [gle] SetEndOfFile(
                               HANDLE hFile
                               );

VOID SetFileApisToANSI();

VOID SetFileApisToOEM();

FailOnFalse [gle] SetFileAttributesA(
                                     LPCSTR lpFileName,
                                     FileFlagsAndAttributes dwFileAttributes
                                     );

FailOnFalse [gle] SetFileAttributesW(
                                     LPCWSTR lpFileName,
                                     FileFlagsAndAttributes dwFileAttributes
                                     );

SetFilePointerReturn [gle] SetFilePointer(
                                          HANDLE hFile,
                                          LONG lDistanceToMove,
                                          PLONG lpDistanceToMoveHigh,
                                          FilePointerStartingPosition dwMoveMethod
                                          );

FailOnFalse [gle] SetFilePointerEx(
                                   HANDLE hFile,
                                   LARGE_INTEGER liDistanceToMove,
                                   PLARGE_INTEGER lpNewFilePointer,
                                   FilePointerStartingPosition dwMoveMethod
                                   );

UINT SetHandleCount(
                    UINT uNumber
                    );

FailOnFalse [gle] SetVolumeLabelA(
                                  LPCSTR lpRootPathName,
                                  LPCSTR lpVolumeName
                                  );

FailOnFalse [gle] SetVolumeLabelW(
                                  LPCWSTR lpRootPathName,
                                  LPCWSTR lpVolumeName
                                  );

FailOnFalse [gle] UnlockFile(
                             HANDLE hFile,
                             DWORD dwFileOffsetLow,
                             DWORD dwFileOffsetHigh,
                             DWORD nNumberOfBytesToUnlockLow,
                             DWORD nNumberOfBytesToUnlockHigh
                             );

FailOnFalse [gle] UnlockFileEx(
                               HANDLE hFile,
                               DWORD dwReserved,
                               DWORD nNumberOfBytesToUnlockLow,
                               DWORD nNumberOfBytesToUnlockHigh,
                               LPOVERLAPPED lpOverlapped
                               );

FailOnFalse [gle] WriteFile(
                            HANDLE hFile,
                            LPCVOID lpBuffer,
                            DWORD nNumberOfBytesToWrite,
                            LPDWORD lpNumberOfBytesWritten,
                            LPOVERLAPPED lpOverlapped
                            );

FailOnFalse [gle] WriteFileEx(
                              HANDLE hFile,
                              LPCVOID lpBuffer,
                              DWORD nNumberOfBytesToWrite,
                              LPOVERLAPPED lpOverlapped,
                              LPVOID lpCompletionRoutine
                              );

// cjc[out] FILE_SEGMENT_ELEMENT aSegmentArray[ ],
FailOnFalse [gle] WriteFileGather(
                                  HANDLE hFile,
                                  [out] ULONGLONG aSegmentArray,
                                  DWORD nNumberOfBytesToWrite,
                                  LPDWORD lpReserved,
                                  LPOVERLAPPED lpOverlapped
                                  );


// +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
//
//                         File System Functions
//
// +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
mask LPDWORD FileSystemFlags
{
#define FS_CASE_SENSITIVE               0x00000001
#define FS_CASE_IS_PRESERVED            0x00000002
#define FS_UNICODE_STORED_ON_DISK       0x00000004
#define FS_PERSISTENT_ACLS              0x00000008
#define FS_VOL_IS_COMPRESSED            0x00008000
#define FS_FILE_COMPRESSION             0x00000010
#define FILE_SUPPORTS_OBJECT_IDS        0x00010000
#define FILE_SUPPORTS_ENCRYPTION        0x00020000
#define FILE_SUPPORTS_REPARSE_POINTS    0x00000080
#define FILE_SUPPORTS_REMOTE_STORAGE    0x00000100
};

typedef struct _EFS_HASH_BLOB {
    DWORD cbData;
    PBYTE pbData;
} EFS_HASH_BLOB, *LPEFS_HASH_BLOB, *PEFS_HASH_BLOB;

typedef struct _CERTIFICATE_BLOB {
    DWORD dwCertEncodingType;
    DWORD cbData;
    PBYTE pbData;
} EFS_CERTIFICATE_BLOB, *PEFS_CERTIFICATE_BLOB;

//cjc SID *pUserSid;
typedef struct _ENCRYPTION_CERTIFICATE_HASH {
    DWORD cbTotalLength;
    VOID *pUserSid;
    PEFS_HASH_BLOB pHash;
    LPWSTR lpDisplayInformation;
} ENCRYPTION_CERTIFICATE_HASH, *LPENCRYPTION_CERTIFICATE_HASH, *PENCRYPTION_CERTIFICATE_HASH;

typedef struct _ENCRYPTION_CERTIFICATE_HASH_LIST {
    DWORD nCert_Hash;
    PENCRYPTION_CERTIFICATE_HASH *pUsers;
} ENCRYPTION_CERTIFICATE_HASH_LIST, *LPENCRYPTION_CERTIFICATE_HASH_LIST, *PENCRYPTION_CERTIFICATE_HASH_LIST;


//cjc SID *pUserSid;
typedef struct _ENCRYPTION_CERTIFICATE {
    DWORD cbTotalLength;
    VOID *pUserSid;
    PEFS_CERTIFICATE_BLOB pCertBlob;
} ENCRYPTION_CERTIFICATE, *LPENCRYPTION_CERTIFICATE, *PENCRYPTION_CERTIFICATE;

typedef struct _ENCRYPTION_CERTIFICATE_LIST {
    DWORD nUsers;
    PENCRYPTION_CERTIFICATE * pUsers;
} ENCRYPTION_CERTIFICATE_LIST, *LPENCRYPTION_CERTIFICATE_LIST, *PENCRYPTION_CERTIFICATE_LIST;

FailOnFalse [gle] CreateHardLinkA(
                                  LPCSTR lpFileName,
                                  LPCSTR lpExistingFileName,
                                  LPSECURITY_ATTRIBUTES lpSecurityAttributes
                                  );

FailOnFalse [gle] CreateHardLinkW(
                                  LPCWSTR lpFileName,
                                  LPCWSTR lpExistingFileName,
                                  LPSECURITY_ATTRIBUTES lpSecurityAttributes
                                  );

module ADVAPI32.DLL:
WinError AddUsersToEncryptedFile(
                                 LPCWSTR lpFileName,
                                 PENCRYPTION_CERTIFICATE_LIST pUsers
                                 );

FailOnFalse [gle] DecryptFileA(
                               LPCSTR lpFileName,
                               DWORD    dwReserved
                               );

FailOnFalse [gle] DecryptFileW(
                               LPCWSTR lpFileName,
                               DWORD    dwReserved
                               );

module KERNEL32.DLL:
FailOnFalse [gle] DeleteVolumeMountPointA(
                                         LPCSTR lpszVolumeMountPoint
                                         );

FailOnFalse [gle] DeleteVolumeMountPointW(
                                         LPCWSTR lpszVolumeMountPoint
                                         );


module ADVAPI32.DLL:
FailOnFalse [gle] EncryptFileA(
                               LPCSTR lpFileName
                               );

FailOnFalse [gle] EncryptFileW(
                               LPCWSTR lpFileName
                               );

FailOnFalse [gle] EncryptionDisable(
                                    LPCWSTR DirPath,
                                    BOOL Disable
                                    );

FailOnFalse [gle] FileEncryptionStatusA(
                                       LPCSTR lpFileName,
                                       LPDWORD lpStatus
                                       );

FailOnFalse [gle] FileEncryptionStatusW(
                                       LPCWSTR lpFileName,
                                       LPDWORD lpStatus
                                       );

module KERNEL32.DLL:

HANDLE FindFirstVolumeA(
                                    [out] LPCSTR lpszVolumeName,
                                    DWORD cchBufferLength
                                    );

HANDLE FindFirstVolumeW(
                                    [out] LPCWSTR lpszVolumeName,
                                    DWORD cchBufferLength
                                    );

HANDLE FindFirstVolumeMountPointA(
                                              LPSTR lpszRootPathName,
                                              [out] LPSTR lpszVolumeMountPoint,
                                              DWORD cchBufferLength
                                              );


HANDLE FindFirstVolumeMountPointW(
                                              LPWSTR lpszRootPathName,
                                              [out] LPWSTR lpszVolumeMountPoint,
                                              DWORD cchBufferLength
                                              );

FailOnFalse [gle] FindNextVolumeA(
                                 HANDLE hFindVolume,
                                 [out] LPSTR lpszVolumeName,
                                 DWORD cchBufferLength
                                 );

FailOnFalse [gle] FindNextVolumeW(
                                 HANDLE hFindVolume,
                                 [out] LPWSTR lpszVolumeName,
                                 DWORD cchBufferLength
                                 );


FailOnFalse [gle] FindNextVolumeMountPointA(
                                           HANDLE hFindVolumeMountPoint,
                                           [out] LPSTR lpszVolumeMountPoint,
                                           DWORD cchBufferLength
                                           );

FailOnFalse [gle] FindNextVolumeMountPointW(
                                           HANDLE hFindVolumeMountPoint,
                                           [out] LPWSTR lpszVolumeMountPoint,
                                           DWORD cchBufferLength
                                           );


FailOnFalse [gle] FindVolumeClose(
                                  HANDLE hFindVolume
                                  );

FailOnFalse [gle] FindVolumeMountPointClose(
                                            HANDLE hFindVolumeMountPoint
                                            );

module ADVAPI32.DLL:
VOID FreeEncryptionCertificateHashList(
                                       PENCRYPTION_CERTIFICATE_HASH_LIST pHashes
                                       );

module KERNEL32.DLL:

DwordFailIfNeg1 [gle] GetCompressedFileSizeA(
                                             LPCSTR lpFileName,
                                             [out] LPDWORD lpFileSizeHigh
                                             );

DwordFailIfNeg1 [gle] GetCompressedFileSizeW(
                                             LPCWSTR lpFileName,
                                             [out] LPDWORD lpFileSizeHigh
                                             );

FailOnFalse [gle] GetVolumeInformationA(
                                        LPCSTR lpRootPathName,
                                        LPSTR lpVolumeNameBuffer,
                                        DWORD nVolumeNameSize,
                                        LPDWORD lpVolumeSerialNumber,
                                        LPDWORD lpMaximumComponentLength,
                                        LPDWORD lpFileSystemFlags,
                                        LPSTR lpFileSystemNameBuffer,
                                        DWORD nFileSystemNameSize
                                        );

FailOnFalse [gle] GetVolumeInformationW(
                                        LPCWSTR lpRootPathName,
                                        [out] LPWSTR lpVolumeNameBuffer,
                                        DWORD nVolumeNameSize,
                                        [out] LPDWORD lpVolumeSerialNumber,
                                        [out] LPDWORD lpMaximumComponentLength,
                                        [out] FileSystemFlags lpFileSystemFlags,
                                        [out] LPWSTR lpFileSystemNameBuffer,
                                        DWORD nFileSystemNameSize
                                        );

FailOnFalse [gle] GetVolumeNameForVolumeMountPointA(
                                                   LPCSTR lpszVolumeMountPoint,
                                                   [out] LPSTR lpszVolumeName,
                                                   DWORD cchBufferLength
                                                   );

FailOnFalse [gle] GetVolumeNameForVolumeMountPointW(
                                                   LPCWSTR lpszVolumeMountPoint,
                                                   [out] LPWSTR lpszVolumeName,
                                                   DWORD cchBufferLength
                                                   );


FailOnFalse [gle] GetVolumePathNameA(
                                    LPCSTR lpszFileName,
                                    [out] LPSTR lpszVolumePathName,
                                    DWORD cchBufferLength
                                    );

FailOnFalse [gle] GetVolumePathNameW(
                                    LPCWSTR lpszFileName,
                                    [out] LPWSTR lpszVolumePathName,
                                    DWORD cchBufferLength
                                    );


module ADVAPI32.DLL:
WinError QueryRecoveryAgentsOnEncryptedFile(
                                            LPCWSTR lpFileName,
                                            [out] LPENCRYPTION_CERTIFICATE_HASH_LIST *pRecoveryAgents
                                            );

WinError QueryUsersOnEncryptedFile(
                                   LPCWSTR lpFileName,
                                   [out] LPENCRYPTION_CERTIFICATE_HASH_LIST *pUsers
                                   );

WinError RemoveUsersFromEncryptedFile(
                                      LPCWSTR lpFileName,
                                      LPENCRYPTION_CERTIFICATE_HASH_LIST pHashes
                                      );

DWORD SetUserFileEncryptionKey(
                               LPENCRYPTION_CERTIFICATE pEncryptionCertificate
                               );

module KERNEL32.DLL:
FailOnFalse [gle] SetVolumeMountPointA(
                                      LPCSTR lpszVolumeMountPoint,
                                      LPCSTR lpszVolumeName
                                      );


FailOnFalse [gle] SetVolumeMountPointW(
                                      LPCWSTR lpszVolumeMountPoint,
                                      LPCWSTR lpszVolumeName
                                      );


// +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
//
//                         Tape Backup Functions
//
// +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
category DeviceFunctions:

value DWORD StreamIds
{
#define BACKUP_DATA             0x00000001
#define BACKUP_EA_DATA          0x00000002
#define BACKUP_SECURITY_DATA    0x00000003
#define BACKUP_ALTERNATE_DATA   0x00000004
#define BACKUP_LINK             0x00000005
#define BACKUP_PROPERTY_DATA    0x00000006
#define BACKUP_OBJECT_ID        0x00000007
#define BACKUP_REPARSE_DATA     0x00000008
#define BACKUP_SPARSE_BLOCK     0x00000009
};

value DWORD StreamAttributes
{
#define STREAM_MODIFIED_WHEN_READ       0x00000001
#define STREAM_CONTAINS_SECURITY        0x00000002
};

typedef struct _WIN32_STREAM_ID {
    StreamIds         dwStreamId;
    StreamAttributes         dwStreamAttributes;
    LARGE_INTEGER Size;
    DWORD         dwStreamNameSize;
    WCHAR         cStreamName[1];
} WIN32_STREAM_ID, *LPWIN32_STREAM_ID;

value DWORD TapeCreateDefinitions
{
#define TAPE_FIXED_PARTITIONS       0L
#define TAPE_SELECT_PARTITIONS      1L
#define TAPE_INITIATOR_PARTITIONS   2L
};

value DWORD EraseTypeToPerform
{
#define TAPE_ERASE_SHORT            0L
#define TAPE_ERASE_LONG             1L
};

value DWORD TypeOfInformation
{
#define GET_TAPE_MEDIA_INFORMATION 0
#define GET_TAPE_DRIVE_INFORMATION 1
};

typedef struct _TAPE_GET_MEDIA_PARAMETERS {
    LARGE_INTEGER   Capacity;
    LARGE_INTEGER   Remaining;
    DWORD   BlockSize;
    DWORD   PartitionCount;
    BOOLEAN WriteProtected;
} TAPE_GET_MEDIA_PARAMETERS, *LPTAPE_GET_MEDIA_PARAMETERS;

typedef struct _TAPE_GET_DRIVE_PARAMETERS {
    BOOLEAN ECC;
    BOOLEAN Compression;
    BOOLEAN DataPadding;
    BOOLEAN ReportSetmarks;
    DWORD   DefaultBlockSize;
    DWORD   MaximumBlockSize;
    DWORD   MinimumBlockSize;
    DWORD   MaximumPartitionCount;
    DWORD   FeaturesLow;
    DWORD   FeaturesHigh;
    DWORD   EOTWarningZoneSize;
} TAPE_GET_DRIVE_PARAMETERS, *LPTAPE_GET_DRIVE_PARAMETERS;

value DWORD TapePosition
{
#define TAPE_ABSOLUTE_POSITION       0L
#define TAPE_LOGICAL_POSITION        1L
#define TAPE_PSEUDO_LOGICAL_POSITION 2L
};

value DWORD TapeDevicePreparation
{
#define TAPE_LOAD                   0L
#define TAPE_UNLOAD                 1L
#define TAPE_TENSION                2L
#define TAPE_LOCK                   3L
#define TAPE_UNLOCK                 4L
#define TAPE_FORMAT                 5L
};

value DWORD InformationToBeSet
{
#define SET_TAPE_MEDIA_INFORMATION 0
#define SET_TAPE_DRIVE_INFORMATION 1
};

typedef struct _TAPE_SET_MEDIA_PARAMETERS {
    DWORD BlockSize;
} TAPE_SET_MEDIA_PARAMETERS, *LPTAPE_SET_MEDIA_PARAMETERS;

typedef struct _TAPE_SET_DRIVE_PARAMETERS {
    BOOLEAN ECC;
    BOOLEAN Compression;
    BOOLEAN DataPadding;
    BOOLEAN ReportSetmarks;
    DWORD   EOTWarningZoneSize;
} TAPE_SET_DRIVE_PARAMETERS, *LPTAPE_SET_DRIVE_PARAMETERS;

value DWORD TypeOfPositioning
{
#define TAPE_REWIND                 0L
#define TAPE_ABSOLUTE_BLOCK         1L
#define TAPE_LOGICAL_BLOCK          2L
#define TAPE_PSEUDO_LOGICAL_BLOCK   3L
#define TAPE_SPACE_END_OF_DATA      4L
#define TAPE_SPACE_RELATIVE_BLOCKS  5L
#define TAPE_SPACE_FILEMARKS        6L
#define TAPE_SPACE_SEQUENTIAL_FMKS  7L
#define TAPE_SPACE_SETMARKS         8L
#define TAPE_SPACE_SEQUENTIAL_SMKS  9L
};

value DWORD TapeMarksToWrite
{
#define TAPE_SETMARKS               0L
#define TAPE_FILEMARKS              1L
#define TAPE_SHORT_FILEMARKS        2L
#define TAPE_LONG_FILEMARKS         3L
};

FailOnFalse [gle] BackupRead(
                             HANDLE hFile,
                             [out] LPBYTE lpBuffer,
                             DWORD nNumberOfBytesToRead,
                             LPDWORD lpNumberOfBytesRead,
                             BOOL bAbort,
                             BOOL bProcessSecurity,
                             [out] LPVOID *lpContext
                             );

FailOnFalse [gle] BackupSeek(
                             HANDLE hFile,
                             DWORD dwLowBytesToSeek,
                             DWORD dwHighBytesToSeek,
                             [out] LPDWORD lpdwLowByteSeeked,
                             [out] LPDWORD lpdwHighByteSeeked,
                             LPVOID *lpContext
                             );

FailOnFalse [gle] BackupWrite(
                              HANDLE hFile,
                              LPBYTE lpBuffer,
                              DWORD nNumberOfBytesToWrite,
                              [out] LPDWORD lpNumberOfBytesWritten,
                              BOOL bAbort,
                              BOOL bProcessSecurity,
                              [out] LPVOID *lpContext
                              );

WinError CreateTapePartition(
                             HANDLE hDevice,
                             TapeCreateDefinitions dwPartitionMethod,
                             DWORD dwCount,
                             DWORD dwSize
                             );

WinError EraseTape(
                   HANDLE hDevice,
                   EraseTypeToPerform dwEraseType,
                   BOOL bImmediate
                   );

WinError GetTapeParameters(
                           HANDLE hDevice,
                           TypeOfInformation dwOperation,
                           [out] LPDWORD lpdwSize,
                           [out] LPVOID lpTapeInformation
                           );

WinError GetTapePosition(
                         HANDLE hDevice,
                         TapePosition dwPositionType,
                         [out] LPDWORD lpdwPartition,
                         [out] LPDWORD lpdwOffsetLow,
                         [out] LPDWORD lpdwOffsetHigh
                         );

WinError GetTapeStatus(
                       HANDLE hDevice
                       );

WinError PrepareTape(
                     HANDLE hDevice,
                     TapeDevicePreparation dwOperation,
                     BOOL bImmediate
                     );

WinError SetTapeParameters(
                           HANDLE hDevice,
                           InformationToBeSet dwOperation,
                           LPVOID lpTapeInformation
                           );

WinError SetTapePosition(
                         HANDLE hDevice,
                         TypeOfPositioning dwPositionMethod,
                         DWORD dwPartition,
                         DWORD dwOffsetLow,
                         DWORD dwOffsetHigh,
                         BOOL bImmediate
                         );

WinError WriteTapemark(
                       HANDLE hDevice,
                       TapeMarksToWrite dwTapemarkType,
                       DWORD dwTapemarkCount,
                       BOOL bImmediate
                       );





// +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
//
//                         Device Input and Output Functions
//
// +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

FailOnFalse [gle] DeviceIoControl(
                                  HANDLE hDevice,
                                  DWORD dwIoControlCode,
                                  LPVOID lpInBuffer,
                                  DWORD nInBufferSize,
                                  LPVOID lpOutBuffer,
                                  DWORD nOutBufferSize,
                                  [out] LPDWORD lpBytesReturned,
                                  LPOVERLAPPED lpOverlapped
                                  );

// +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
//
//                         Device Management Functions
//
// +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

typedef struct _DEV_BROADCAST_HDR {
    DWORD dbch_size;
    DWORD dbch_devicetype;
    DWORD dbch_reserved;
} DEV_BROADCAST_HDR;
typedef DEV_BROADCAST_HDR *LPDEV_BROADCAST_HDR;

value DWORD HandleType
{
#define DEVICE_NOTIFY_WINDOW_HANDLE     0x00000000
};

value HANDLE HdevnotifyFailNull
{
#define NULL                0 [fail]
};

value HANDLE HdevinfoFailInvalid
{
#define INVALID_HANDLE_VALUE            -1 [fail]
};

value HKEY HkeyFailInvalid
{
#define INVALID_HANDLE_VALUE            -1 [fail]
};

mask DWORD DeviceInterfaceFlags
{
#define SPINT_ACTIVE  0x00000001
#define SPINT_DEFAULT 0x00000002
#define SPINT_REMOVED 0x00000004
};

typedef struct _SP_DEVINFO_DATA {
    DWORD cbSize;
    GUID  ClassGuid;
    DWORD DevInst;
    ULONG_PTR Reserved;
} SP_DEVINFO_DATA, *PSP_DEVINFO_DATA;

value DWORD ControlOptionFlags
{
#define DIGCF_DEFAULT           0x00000001  // only valid with DIGCF_DEVICEINTERFACE
#define DIGCF_PRESENT           0x00000002
#define DIGCF_ALLCLASSES        0x00000004
#define DIGCF_PROFILE           0x00000008
#define DIGCF_DEVICEINTERFACE   0x00000010
};

value DWORD SetupDiopenClassRegKeyExFlags
{
#define DIOCR_INSTALLER   0x00000001    // class installer registry branch
#define DIOCR_INTERFACE   0x00000002    // interface class registry branch
};

module USER32.DLL:
HdevnotifyFailNull [gle] RegisterDeviceNotificationA(
                                                     HANDLE hRecipient,
                                                     LPVOID NotificationFilter,
                                                     HandleType Flags
                                                     );

HdevnotifyFailNull [gle] RegisterDeviceNotificationW(
                                                     HANDLE hRecipient,
                                                     LPVOID NotificationFilter,
                                                     HandleType Flags
                                                     );

FailOnFalse [gle] UnregisterDeviceNotification(
                                               HDEVNOTIFY Handle
                                               );

module SETUPAPI.DLL:
HdevinfoFailInvalid [gle] SetupDiCreateDeviceInfoList(
                                                      LPGUID ClassGuid,
                                                      HWND hwndParent
                                                      );

HdevinfoFailInvalid [gle] SetupDiCreateDeviceInfoListExA(
                                                         LPGUID ClassGuid,
                                                         HWND hwndParent,
                                                         LPCSTR MachineName,
                                                         PVOID Reserved
                                                         );

HdevinfoFailInvalid [gle] SetupDiCreateDeviceInfoListExW(
                                                         LPGUID ClassGuid,
                                                         HWND   hwndParent,
                                                         LPCWSTR MachineName,
                                                         PVOID  Reserved
                                                         );

HkeyFailInvalid [gle] SetupDiCreateDeviceInterfaceRegKeyA(
                                                          HDEVINFO DeviceInfoSet,
                                                          PSP_DEVICE_INTERFACE_DATA DeviceInterfaceData,
                                                          DWORD Reserved,
                                                          DesiredSecurityAccess samDesired,
                                                          HINF InfHandle,
                                                          LPCSTR InfSectionName
                                                          );

HkeyFailInvalid [gle] SetupDiCreateDeviceInterfaceRegKeyW(
                                                          HDEVINFO                  DeviceInfoSet,
                                                          PSP_DEVICE_INTERFACE_DATA DeviceInterfaceData,
                                                          DWORD                     Reserved,
                                                          DesiredSecurityAccess                    samDesired,
                                                          HINF                      InfHandle,
                                                          LPCWSTR                   InfSectionName
                                                          );

FailOnFalse [gle] SetupDiDeleteDeviceInterfaceData(
                                                   HDEVINFO DeviceInfoSet,
                                                   PSP_DEVICE_INTERFACE_DATA DeviceInterfaceData
                                                   );

FailOnFalse [gle] SetupDiDeleteDeviceInterfaceRegKey(
                                                     HDEVINFO DeviceInfoSet,
                                                     PSP_DEVICE_INTERFACE_DATA DeviceInterfaceData,
                                                     DWORD Reserved
                                                     );

FailOnFalse [gle] SetupDiDestroyDeviceInfoList(
                                               HDEVINFO DeviceInfoSet
                                               );

FailOnFalse [gle] SetupDiEnumDeviceInterfaces(
                                              HDEVINFO DeviceInfoSet,
                                              PSP_DEVINFO_DATA DeviceInfoData,
                                              LPGUID InterfaceClassGuid,
                                              DWORD MemberIndex,
                                              [out] PSP_DEVICE_INTERFACE_DATA DeviceInterfaceData
                                              );

HdevinfoFailInvalid [gle] SetupDiGetClassDevsA(
                                               LPGUID ClassGuid,
                                               LPCSTR Enumerator,
                                               HWND hwndParent,
                                               ControlOptionFlags Flags
                                               );

HdevinfoFailInvalid [gle] SetupDiGetClassDevsW(
                                               LPGUID ClassGuid,
                                               LPCWSTR Enumerator,
                                               HWND   hwndParent,
                                               ControlOptionFlags  Flags
                                               );

HdevinfoFailInvalid [gle] SetupDiGetClassDevsExA(
                                                 LPGUID ClassGuid,
                                                 LPCSTR Enumerator,
                                                 HWND hwndParent,
                                                 ControlOptionFlags Flags,
                                                 HDEVINFO DeviceInfoSet,
                                                 LPCSTR MachineName,
                                                 PVOID Reserved
                                                 );

HdevinfoFailInvalid [gle] SetupDiGetClassDevsExW(
                                                 LPGUID ClassGuid,
                                                 LPCWSTR Enumerator,
                                                 HWND   hwndParent,
                                                 ControlOptionFlags Flags,
                                                 HDEVINFO DeviceInfoSet,
                                                 LPCWSTR MachineName,
                                                 PVOID  Reserved
                                                 );

FailOnFalse [gle] SetupDiGetDeviceInterfaceAlias(
                                                 HDEVINFO DeviceInfoSet,
                                                 PSP_DEVICE_INTERFACE_DATA DeviceInterfaceData,
                                                 LPGUID AliasInterfaceClassGuid,
                                                 [out] PSP_DEVICE_INTERFACE_DATA AliasDeviceInterfaceData
                                                 );

FailOnFalse [gle] SetupDiGetDeviceInterfaceDetailA(
                                                   HDEVINFO DeviceInfoSet,
                                                   PSP_DEVICE_INTERFACE_DATA DeviceInterfaceData,
                                                   [out] PSP_DEVICE_INTERFACE_DETAIL_DATA_A DeviceInterfaceDetailData,
                                                   DWORD DeviceInterfaceDetailDataSize,
                                                   [out] PDWORD RequiredSize,
                                                   [out] PSP_DEVINFO_DATA DeviceInfoData
                                                   );

FailOnFalse [gle] SetupDiGetDeviceInterfaceDetailW(
                                                   HDEVINFO                           DeviceInfoSet,
                                                   PSP_DEVICE_INTERFACE_DATA          DeviceInterfaceData,
                                                   [out] PSP_DEVICE_INTERFACE_DETAIL_DATA_W DeviceInterfaceDetailData,
                                                   DWORD                              DeviceInterfaceDetailDataSize,
                                                   [out] PDWORD                       RequiredSize,
                                                   [out] PSP_DEVINFO_DATA             DeviceInfoData
                                                   );

HkeyFailInvalid [gle] SetupDiOpenClassRegKeyExA(
                                                LPGUID ClassGuid,
                                                DesiredSecurityAccess samDesired,
                                                SetupDiopenClassRegKeyExFlags Flags,
                                                LPCSTR MachineName,
                                                PVOID Reserved
                                                );

HkeyFailInvalid [gle] SetupDiOpenClassRegKeyExW(
                                                LPGUID ClassGuid,
                                                DesiredSecurityAccess samDesired,
                                                SetupDiopenClassRegKeyExFlags Flags,
                                                LPCWSTR MachineName,
                                                PVOID  Reserved
                                                );

FailOnFalse [gle] SetupDiOpenDeviceInterfaceA(
                                              HDEVINFO DeviceInfoSet,
                                              LPCSTR DevicePath,
                                              DWORD OpenFlags,
                                              [out] PSP_DEVICE_INTERFACE_DATA DeviceInterfaceData
                                              );

FailOnFalse [gle] SetupDiOpenDeviceInterfaceW(
                                              HDEVINFO                  DeviceInfoSet,
                                              LPCWSTR                   DevicePath,
                                              DWORD                     OpenFlags,
                                              [out] PSP_DEVICE_INTERFACE_DATA DeviceInterfaceData
                                              );

HkeyFailInvalid [gle] SetupDiOpenDeviceInterfaceRegKey(
                                                       HDEVINFO DeviceInfoSet,
                                                       PSP_DEVICE_INTERFACE_DATA DeviceInterfaceData,
                                                       DWORD Reserved,
                                                       DesiredSecurityAccess samDesired
                                                       );
module KERNEL32.DLL:

// +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
//
//                         Power Management Functions
//
// +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

value BYTE ACLineStatusValues
{
#define Offline         0
#define Online          1
#define Unknown         255
};

value BYTE BatteryFlags
{
#define High                    1
#define Low                     2
#define Critical                4
#define Charging                8
#define No_system_battery       128
#define Unknown_status          255
};

typedef struct _SYSTEM_POWER_STATUS {
    ACLineStatusValues ACLineStatus;
    BatteryFlags  BatteryFlag;
    BYTE  BatteryLifePercent;
    BYTE  Reserved1;
    DWORD  BatteryLifeTime;
    DWORD  BatteryFullLifeTime;
} SYSTEM_POWER_STATUS, *LPSYSTEM_POWER_STATUS;

mask DWORD ExecutionRequirements
{
#define ES_SYSTEM_REQUIRED  0x00000001
#define ES_DISPLAY_REQUIRED 0x00000002
#define ES_USER_PRESENT     0x00000004
#define ES_CONTINUOUS       0x80000000
};


value DWORD LATENCY_TIME
{
#define LT_DONT_CARE      0
#define LT_LOWEST_LATENCY 1
};

FailOnFalse GetDevicePowerState(
                                HANDLE hDevice,
                                [out] BOOL *pfOn
                                );

FailOnFalse [gle] GetSystemPowerStatus(
                                       LPSYSTEM_POWER_STATUS lpSystemPowerStatus
                                       );

FailOnFalse IsSystemResumeAutomatic();

FailOnFalse RequestWakeupLatency(
                                 LATENCY_TIME latency
                                 );

FailOnFalse [gle] SetSystemPowerState(
                                      BOOL fSuspend,
                                      BOOL fForce
                                      );

ULONG SetThreadExecutionState(
                                        ExecutionRequirements esFlags
                                        );

// +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
//
//                         Atom Functions
//
// +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
category AtomFunctions:

value ATOM AtomFailIfZero
{
#define ZERO            0 [fail]
};

AtomFailIfZero [gle] AddAtomA(
                              LPCSTR lpString
                              );

AtomFailIfZero [gle] AddAtomW(
                              LPCWSTR lpString
                              );

ATOM [gle] DeleteAtom(
                      ATOM nAtom
                      );

AtomFailIfZero [gle] FindAtomA(
                              LPCSTR lpString
                              );

AtomFailIfZero [gle] FindAtomW(
                              LPCWSTR lpString
                              );

UintFailIfZero [gle] GetAtomNameA(
                                 ATOM nAtom,
                                 [out] LPSTR lpBuffer,
                                 int nSize
                                 );

UintFailIfZero [gle] GetAtomNameW(
                                 ATOM nAtom,
                                 [out] LPWSTR lpBuffer,
                                 int nSize
                                 );



AtomFailIfZero [gle] GlobalAddAtomA(
                                    LPCSTR lpString
                                    );

AtomFailIfZero [gle] GlobalAddAtomW(
                                    LPCWSTR lpString
                                    );

ATOM [gle] GlobalDeleteAtom(
                            ATOM nAtom
                            );

AtomFailIfZero [gle] GlobalFindAtomA(
                                     LPCSTR lpString
                                     );

AtomFailIfZero [gle] GlobalFindAtomW(
                                     LPCWSTR lpString
                                     );

UintFailIfZero [gle] GlobalGetAtomNameA(
                                        ATOM nAtom,
                                        [out] LPSTR lpBuffer,
                                        int nSize
                                        );

UintFailIfZero [gle] GlobalGetAtomNameW(
                                        ATOM nAtom,
                                        [out] LPWSTR lpBuffer,
                                        int nSize
                                        );

FailOnFalse InitAtomTable(
                          DWORD nSize
                          );

// +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
//
//                        Handle and Object Functions

//
// +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
category HandleAndObjectFunctions:

mask DWORD OptionalActions
{
#define DUPLICATE_CLOSE_SOURCE      0x00000001
#define DUPLICATE_SAME_ACCESS       0x00000002
};

mask LPDWORD HandleProperties
{
#define HANDLE_FLAG_INHERIT             0x00000001
#define HANDLE_FLAG_PROTECT_FROM_CLOSE  0x00000002
};

FailOnFalse [gle] CloseHandle(
                              [out] HANDLE hObject
                              );

FailOnFalse [gle] DuplicateHandle(
                                  HANDLE hSourceProcessHandle,
                                  HANDLE hSourceHandle,
                                  HANDLE hTargetProcessHandle,
                                  [out] LPHANDLE lpTargetHandle,
                                  DWORD dwDesiredAccess,
                                  BOOL bInheritHandle,
                                  OptionalActions dwOptions
                                  );

FailOnFalse [gle] GetHandleInformation(
                                       HANDLE hObject,
                                       [out] HandleProperties lpdwFlags
                                       );

FailOnFalse [gle] SetHandleInformation(
                                       HANDLE hObject,
                                       DWORD dwMask,
                                       HandleProperties dwFlags
                                       );

// +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
//
//                Mailslot Functions
//
// +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
category IOFunctions:

value DWORD MessageWaitLength
{
#define NoMessage                       0
#define MAILSLOT_WAIT_FOREVER           -1
};

value LPDWORD NextSizeValue
{
#define MAILSLOT_NO_MESSAGE             -1
};

HANDLE [gle] CreateMailslotA(
                                          LPCSTR lpName,
                                          DWORD nMaxMessageSize,
                                          MessageWaitLength lReadTimeout,
                                          LPSECURITY_ATTRIBUTES lpSecurityAttributes
                                          );

HANDLE [gle] CreateMailslotW(
                                          LPCWSTR lpName,
                                          DWORD nMaxMessageSize,
                                          MessageWaitLength lReadTimeout,
                                          LPSECURITY_ATTRIBUTES lpSecurityAttributes
                                          );

FailOnFalse [gle] GetMailslotInfo(
                                  HANDLE hMailslot,
                                  LPDWORD lpMaxMessageSize,
                                  NextSizeValue lpNextSize,
                                  LPDWORD lpMessageCount,
                                  LPDWORD lpReadTimeout
                                  );

FailOnFalse [gle] SetMailslotInfo(
                                  HANDLE hMailslot,
                                  MessageWaitLength lReadTimeout
                                  );

// +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
//
//                              Pipe Functions
//
// +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
category IOFunctions:

value DWORD TimeOutValue
{
#define NMPWAIT_WAIT_FOREVER            0xffffffff
#define NMPWAIT_NOWAIT                  0x00000001
#define NMPWAIT_USE_DEFAULT_WAIT        0x00000000
};


mask DWORD PipeSpecificModes
{
#define PIPE_TYPE_BYTE              0x00000000
#define PIPE_TYPE_MESSAGE           0x00000004
#define PIPE_READMODE_MESSAGE       0x00000002
    // #define PIPE_READMODE_BYTE          0x00000000
    // #define PIPE_WAIT                   0x00000000
#define PIPE_NOWAIT                 0x00000001
};

value LPDWORD PipeType
{
#define PIPE_CLIENT_END             0x00000000
#define PIPE_SERVER_END             0x00000001
    // #define PIPE_TYPE_BYTE              0x00000000
#define PIPE_TYPE_MESSAGE           0x00000004
};




FailOnFalse [gle] CallNamedPipeA(
                                 LPCSTR lpNamedPipeName,
                                 LPVOID lpInBuffer,
                                 DWORD nInBufferSize,
                                 [out] LPVOID lpOutBuffer,
                                 DWORD nOutBufferSize,
                                 [out] LPDWORD lpBytesRead,
                                 TimeOutValue nTimeOut
                                 );

FailOnFalse [gle] CallNamedPipeW(
                                 LPCWSTR lpNamedPipeName,
                                 LPVOID lpInBuffer,
                                 DWORD nInBufferSize,
                                 [out] LPVOID lpOutBuffer,
                                 DWORD nOutBufferSize,
                                 [out] LPDWORD lpBytesRead,
                                 TimeOutValue nTimeOut
                                 );

FailOnFalse [gle] ConnectNamedPipe(
                                   HANDLE hNamedPipe,
                                   LPOVERLAPPED lpOverlapped
                                   );

HANDLE [gle] CreateNamedPipeA(
                                           LPCSTR lpName,
                                           AccessMode dwOpenMode,
                                           PipeSpecificModes dwPipeMode,
                                           DWORD nMaxInstances,
                                           DWORD nOutBufferSize,
                                           DWORD nInBufferSize,
                                           DWORD nDefaultTimeOut,
                                           LPSECURITY_ATTRIBUTES lpSecurityAttributes
                                           );

HANDLE [gle] CreateNamedPipeW(
                                           LPCWSTR lpName,
                                           AccessMode dwOpenMode,
                                           PipeSpecificModes dwPipeMode,
                                           DWORD nMaxInstances,
                                           DWORD nOutBufferSize,
                                           DWORD nInBufferSize,
                                           DWORD nDefaultTimeOut,
                                           LPSECURITY_ATTRIBUTES lpSecurityAttributes
                                           );

FailOnFalse [gle] CreatePipe(
                             [out] PHANDLE hReadPipe,
                             [out] PHANDLE hWritePipe,
                             LPSECURITY_ATTRIBUTES lpPipeAttributes,
                             DWORD nSize
                             );

FailOnFalse [gle] DisconnectNamedPipe(
                                      HANDLE hNamedPipe
                                      );

FailOnFalse [gle] GetNamedPipeHandleStateA(
                                           HANDLE hNamedPipe,
                                           [out] PipeSpecificModes lpState,
                                           [out] LPDWORD lpCurInstances,
                                           [out] LPDWORD lpMaxCollectionCount,
                                           [out] LPDWORD lpCollectDataTimeout,
                                           [out] LPSTR lpUserName,
                                           DWORD nMaxUserNameSize
                                           );

FailOnFalse [gle] GetNamedPipeHandleStateW(
                                           HANDLE hNamedPipe,
                                           [out] PipeSpecificModes lpState,
                                           [out] LPDWORD lpCurInstances,
                                           [out] LPDWORD lpMaxCollectionCount,
                                           [out] LPDWORD lpCollectDataTimeout,
                                           [out] LPWSTR lpUserName,
                                           DWORD nMaxUserNameSize
                                           );

FailOnFalse [gle] GetNamedPipeInfo(
                                   HANDLE hNamedPipe,
                                   PipeType lpFlags,
                                   [out] LPDWORD lpOutBufferSize,
                                   [out] LPDWORD lpInBufferSize,
                                   [out] LPDWORD lpMaxInstances
                                   );

FailOnFalse [gle] PeekNamedPipe(
                                HANDLE hNamedPipe,
                                [out] LPVOID lpBuffer,
                                DWORD nBufferSize,
                                [out] LPDWORD lpBytesRead,
                                [out] LPDWORD lpTotalBytesAvail,
                                [out] LPDWORD lpBytesLeftThisMessage
                                );

FailOnFalse [gle] SetNamedPipeHandleState(
                                          HANDLE hNamedPipe,
                                          PipeSpecificModes lpMode,
                                          LPDWORD lpMaxCollectionCount,
                                          LPDWORD lpCollectDataTimeout
                                          );

FailOnFalse [gle] TransactNamedPipe(
                                    HANDLE hNamedPipe,
                                    LPVOID lpInBuffer,
                                    DWORD nInBufferSize,
                                    LPVOID lpOutBuffer,
                                    DWORD nOutBufferSize,
                                    [out] LPDWORD lpBytesRead,
                                    LPOVERLAPPED lpOverlapped
                                    );

FailOnFalse [gle] WaitNamedPipeA(
                                 LPCSTR lpNamedPipeName,
                                 TimeOutValue nTimeOut
                                 );

FailOnFalse [gle] WaitNamedPipeW(
                                 LPCWSTR lpNamedPipeName,
                                 TimeOutValue nTimeOut
                                 );
