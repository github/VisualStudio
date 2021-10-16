
typedef HANDLE HDROP;

typedef LPVOID LPSHELLFLAGSTATE;
typedef LPVOID LPBC;
typedef LPVOID IEnumIDList;
typedef LPVOID LPSOFTDISTINFO;

typedef struct _SHITEMID        // mkid
{
    USHORT      cb;             // Size of the ID (including cb itself)
    BYTE        abID[1];        // The item ID (variable length)
} SHITEMID;
typedef SHITEMID *LPSHITEMID;
typedef SHITEMID *LPCSHITEMID;

//
// ITEMIDLIST -- List if item IDs (combined with 0-terminator)
//
typedef struct _ITEMIDLIST      // idl
{
    SHITEMID    mkid;
} ITEMIDLIST;
typedef ITEMIDLIST * LPITEMIDLIST;
typedef ITEMIDLIST * LPCITEMIDLIST;

value UINT AppBarEdge
{
#define ABE_LEFT        0
#define ABE_TOP         1
#define ABE_RIGHT       2
#define ABE_BOTTOM      3
};

typedef struct _AppBarData {
    DWORD  cbSize;
    HWND   hWnd;
    UINT   uCallbackMessage;
    AppBarEdge   uEdge;
    RECT   rc;
    LPARAM lParam;
} APPBARDATA, *PAPPBARDATA;

value DWORD AppBarMessage
{
#define ABM_NEW           0x00000000
#define ABM_REMOVE        0x00000001
#define ABM_QUERYPOS      0x00000002
#define ABM_SETPOS        0x00000003
#define ABM_GETSTATE      0x00000004
#define ABM_GETTASKBARPOS 0x00000005
#define ABM_ACTIVATE      0x00000006  // lParam == TRUE/FALSE means activate/deactivate
#define ABM_GETAUTOHIDEBAR 0x00000007
#define ABM_SETAUTOHIDEBAR 0x00000008  // this can fail at any time.  MUST check the result
                                        // lParam = TRUE/FALSE  Set/Unset
                                        // uEdge = what edge
#define ABM_WINDOWPOSCHANGED 0x0000009
};

value DWORD FindExecutableError
{
#define OutOfMemory                      0L           [fail]
#define ERROR_FILE_NOT_FOUND             2L           [fail]
#define ERROR_PATH_NOT_FOUND             3L           [fail]
#define ERROR_BAD_FORMAT                 11L          [fail]
#define NoAssociation                    31L          [fail]
};

value UINT SHAddToRecentDocsFlags
{
#define SHARD_PIDL      0x00000001L
#define SHARD_PATHA     0x00000002L
#define SHARD_PATHW     0x00000003L
};

mask UINT BrowseInfoFlags
{
#define BIF_RETURNONLYFSDIRS   0x0001  // For finding a folder to start document searching
#define BIF_DONTGOBELOWDOMAIN  0x0002  // For starting the Find Computer
#define BIF_STATUSTEXT         0x0004
#define BIF_RETURNFSANCESTORS  0x0008
#define BIF_EDITBOX            0x0010
#define BIF_VALIDATE           0x0020   // insist on valid result (or CANCEL)

#define BIF_BROWSEFORCOMPUTER  0x1000  // Browsing for Computers.
#define BIF_BROWSEFORPRINTER   0x2000  // Browsing for Printers
#define BIF_BROWSEINCLUDEFILES 0x4000  // Browsing for Everything
};

typedef struct _browseinfoA {
    HWND        hwndOwner;
    LPCITEMIDLIST pidlRoot;
    LPSTR        pszDisplayName;// Return display name of item selected.
    LPCSTR       lpszTitle;      // text to go in the banner over the tree.
    BrowseInfoFlags ulFlags;       // Flags that control the return stuff
    LPVOID      lpfn;
    LPARAM      lParam;         // extra info that's passed back in callbacks

    int          iImage;      // output var: where to return the Image index.
} BROWSEINFOA, *PBROWSEINFOA, *LPBROWSEINFOA;

typedef struct _browseinfoW {
    HWND        hwndOwner;
    LPCITEMIDLIST pidlRoot;
    LPWSTR       pszDisplayName;// Return display name of item selected.
    LPCWSTR      lpszTitle;      // text to go in the banner over the tree.
    BrowseInfoFlags ulFlags;       // Flags that control the return stuff
    LPVOID      lpfn;
    LPARAM      lParam;         // extra info that's passed back in callbacks

    int          iImage;      // output var: where to return the Image index.
} BROWSEINFOW, *PBROWSEINFOW, *LPBROWSEINFOW;


value LONG ChangeNotifyEventId
{
#define SHCNE_RENAMEITEM          0x00000001L
#define SHCNE_CREATE              0x00000002L
#define SHCNE_DELETE              0x00000004L
#define SHCNE_MKDIR               0x00000008L
#define SHCNE_RMDIR               0x00000010L
#define SHCNE_MEDIAINSERTED       0x00000020L
#define SHCNE_MEDIAREMOVED        0x00000040L
#define SHCNE_DRIVEREMOVED        0x00000080L
#define SHCNE_DRIVEADD            0x00000100L
#define SHCNE_NETSHARE            0x00000200L
#define SHCNE_NETUNSHARE          0x00000400L
#define SHCNE_ATTRIBUTES          0x00000800L
#define SHCNE_UPDATEDIR           0x00001000L
#define SHCNE_UPDATEITEM          0x00002000L
#define SHCNE_SERVERDISCONNECT    0x00004000L
#define SHCNE_UPDATEIMAGE         0x00008000L
#define SHCNE_DRIVEADDGUI         0x00010000L
#define SHCNE_RENAMEFOLDER        0x00020000L
#define SHCNE_FREESPACE           0x00040000L

#define SHCNE_EXTENDED_EVENT      0x04000000L
#define SHCNE_ASSOCCHANGED        0x08000000L

#define SHCNE_DISKEVENTS          0x0002381FL
#define SHCNE_GLOBALEVENTS        0x0C0581E0L
#define SHCNE_ALLEVENTS           0x7FFFFFFFL
#define SHCNE_INTERRUPT           0x80000000L // The presence of this flag indicates
                                            // that the event was generated by an
                                            // interrupt.  It is stripped out before
                                            // the clients of SHCNNotify_ see it.
};


value LONG ChangeNotifyFlags
{
#define SHCNF_IDLIST      0x0000        // LPITEMIDLIST
#define SHCNF_PATHA       0x0001        // path name
#define SHCNF_PRINTERA    0x0002        // printer friendly name
#define SHCNF_DWORD       0x0003        // DWORD
#define SHCNF_PATHW       0x0005        // path name
#define SHCNF_PRINTERW    0x0006        // printer friendly name
};

mask ULONG SHCreateProcessInfoMask
{
#define SEE_MASK_CLASSNAME        0x00000001
#define SEE_MASK_CLASSKEY         0x00000003
#define SEE_MASK_IDLIST           0x00000004
#define SEE_MASK_INVOKEIDLIST     0x0000000c
#define SEE_MASK_ICON             0x00000010
#define SEE_MASK_HOTKEY           0x00000020
#define SEE_MASK_NOCLOSEPROCESS   0x00000040
#define SEE_MASK_CONNECTNETDRV    0x00000080
#define SEE_MASK_FLAG_DDEWAIT     0x00000100
#define SEE_MASK_DOENVSUBST       0x00000200
#define SEE_MASK_FLAG_NO_UI       0x00000400
#define SEE_MASK_UNICODE          0x00004000
#define SEE_MASK_NO_CONSOLE       0x00008000
#define SEE_MASK_ASYNCOK          0x00100000
#define SEE_MASK_HMONITOR         0x00200000
#define SEE_MASK_NOQUERYCLASSSTORE 0x01000000
#define SEE_MASK_WAITFORINPUTIDLE  0x02000000
};

typedef struct _SHCREATEPROCESSINFOW
{
        DWORD cbSize;
        SHCreateProcessInfoMask fMask;
        HWND hwnd;
        LPCWSTR  pwszFile;
        LPCWSTR  pwszParameters;
        LPCWSTR  pwszCurrentDirectory;
        HANDLE hUserToken;
        LPSECURITY_ATTRIBUTES lpProcessAttributes;
        LPSECURITY_ATTRIBUTES lpThreadAttributes;
        BOOL bInheritHandles;
        DWORD dwCreationFlags;
        LPSTARTUPINFOW lpStartupInfo;
        LPPROCESS_INFORMATION lpProcessInformation;
} SHCREATEPROCESSINFOW, *PSHCREATEPROCESSINFOW;

value DWORD NotifyIconMessage
{
#define NIM_ADD         0x00000000
#define NIM_MODIFY      0x00000001
#define NIM_DELETE      0x00000002
#define NIM_SETFOCUS    0x00000003
#define NIM_SETVERSION  0x00000004
};

typedef LPVOID PNOTIFYICONDATA;
/*   --- Can't handle variable length structures
typedef struct _NOTIFYICONDATA {
    DWORD cbSize;
    HWND hWnd;
    UINT uID;
    UINT uFlags;
    UINT uCallbackMessage;
    HICON hIcon;
    TCHAR szTip[64];
    DWORD dwState; //Version 5.0
    DWORD dwStateMask; //Version 5.0
    TCHAR szInfo[256]; //Version 5.0
    union {
        UINT  uTimeout; //Version 5.0
        UINT  uVersion; //Version 5.0
    } DUMMYUNIONNAME;
    TCHAR szInfoTitle[64]; //Version 5.0
    DWORD dwInfoFlags; //Version 5.0
} NOTIFYICONDATA, *PNOTIFYICONDATA;
*/

value DWORD ShellExecuteError
{
#define OutOfMemory                      0L           [fail]
#define ERROR_FILE_NOT_FOUND             2L           [fail]
#define ERROR_PATH_NOT_FOUND             3L           [fail]
#define ERROR_BAD_FORMAT                 11L          [fail]
#define SE_ERR_ACCESSDENIED              5             [fail]
#define SE_ERR_OOM                      8             [fail]
#define SE_ERR_DLLNOTFOUND              32             [fail]
#define SE_ERR_SHARE                    26             [fail]
#define SE_ERR_ASSOCINCOMPLETE          27             [fail]
#define SE_ERR_DDETIMEOUT               28             [fail]
#define SE_ERR_DDEFAIL                  29             [fail]
#define SE_ERR_DDEBUSY                  30             [fail]
#define SE_ERR_NOASSOC                  31             [fail]
};

typedef struct _SHELLEXECUTEINFOA
{
        DWORD cbSize;
        ULONG fMask;
        HWND hwnd;
        LPCSTR   lpVerb;
        LPCSTR   lpFile;
        LPCSTR   lpParameters;
        LPCSTR   lpDirectory;
        INT nShow;
        HINSTANCE hInstApp;
        // Optional fields
        LPVOID lpIDList;
        LPCSTR   lpClass;
        HKEY hkeyClass;
        DWORD dwHotKey;
        HRSRC  hIcon;
        HANDLE hProcess;
} SHELLEXECUTEINFOA, *LPSHELLEXECUTEINFOA;

typedef struct _SHELLEXECUTEINFOW
{
        DWORD cbSize;
        ULONG fMask;
        HWND hwnd;
        LPCWSTR  lpVerb;
        LPCWSTR  lpFile;
        LPCWSTR  lpParameters;
        LPCWSTR  lpDirectory;
        INT nShow;
        HINSTANCE hInstApp;
        // Optional fields
        LPVOID lpIDList;
        LPCWSTR  lpClass;
        HKEY hkeyClass;
        DWORD dwHotKey;
        HRSRC hIcon;
        HANDLE hProcess;
} SHELLEXECUTEINFOW, *LPSHELLEXECUTEINFOW;

mask DWORD SHEmptyRecycleBinFlags
{
#define SHERB_NOCONFIRMATION    0x00000001
#define SHERB_NOPROGRESSUI      0x00000002
#define SHERB_NOSOUND           0x00000004
};

mask DWORD FILEOP_FLAGS
{
#define FOF_MULTIDESTFILES         0x0001
#define FOF_CONFIRMMOUSE           0x0002
#define FOF_SILENT                 0x0004  // don't create progress/report
#define FOF_RENAMEONCOLLISION      0x0008
#define FOF_NOCONFIRMATION         0x0010  // Don't prompt the user.
#define FOF_WANTMAPPINGHANDLE      0x0020  // Fill in SHFILEOPSTRUCT.hNameMappings
                                      // Must be freed using SHFreeNameMappings
#define FOF_ALLOWUNDO              0x0040
#define FOF_FILESONLY              0x0080  // on *.*, do only files
#define FOF_SIMPLEPROGRESS         0x0100  // means don't show names of files
#define FOF_NOCONFIRMMKDIR         0x0200  // don't confirm making any needed dirs
#define FOF_NOERRORUI              0x0400  // don't put up error UI
#define FOF_NOCOPYSECURITYATTRIBS  0x0800  // dont copy NT file Security Attributes
#define FOF_NORECURSION            0x1000  // don't recurse into directories.
#define FOF_NO_CONNECTED_ELEMENTS  0x2000  // don't operate on connected elements.
#define FOF_WANTNUKEWARNING        0x4000  // during delete operation, warn if nuking instead of recycling (partially overrides FOF_NOCONFIRMATION)
};

typedef struct _SHFILEOPSTRUCTA
{
        HWND            hwnd;
        UINT            wFunc;
        LPCSTR          pFrom;
        LPCSTR          pTo;
        FILEOP_FLAGS    fFlags;
        BOOL            fAnyOperationsAborted;
        LPVOID          hNameMappings;
        LPCSTR           lpszProgressTitle; // only used if FOF_SIMPLEPROGRESS
} SHFILEOPSTRUCTA, *LPSHFILEOPSTRUCTA;

typedef struct _SHFILEOPSTRUCTW
{
        HWND            hwnd;
        UINT            wFunc;
        LPCWSTR         pFrom;
        LPCWSTR         pTo;
        FILEOP_FLAGS    fFlags;
        BOOL            fAnyOperationsAborted;
        LPVOID          hNameMappings;
        LPCWSTR          lpszProgressTitle; // only used if FOF_SIMPLEPROGRESS
} SHFILEOPSTRUCTW, *LPSHFILEOPSTRUCTW;

value INT SHGetDataFromIDListFormat
{
#define SHGDFIL_FINDDATA        1
#define SHGDFIL_NETRESOURCE     2
#define SHGDFIL_DESCRIPTIONID   3
};

mask UINT SHGetFileInfoFlags
{
#define SHGFI_ICON              0x000000100     // get icon
#define SHGFI_DISPLAYNAME       0x000000200     // get display name
#define SHGFI_TYPENAME          0x000000400     // get type name
#define SHGFI_ATTRIBUTES        0x000000800     // get attributes
#define SHGFI_ICONLOCATION      0x000001000     // get icon location
#define SHGFI_EXETYPE           0x000002000     // return exe type
#define SHGFI_SYSICONINDEX      0x000004000     // get system icon index
#define SHGFI_LINKOVERLAY       0x000008000     // put a link overlay on icon
#define SHGFI_SELECTED          0x000010000     // show icon in selected state
#define SHGFI_ATTR_SPECIFIED    0x000020000     // get only specified attributes
#define SHGFI_LARGEICON         0x000000000     // get large icon
#define SHGFI_SMALLICON         0x000000001     // get small icon
#define SHGFI_OPENICON          0x000000002     // get open icon
#define SHGFI_SHELLICONSIZE     0x000000004     // get shell size icon
#define SHGFI_PIDL              0x000000008     // pszPath is a pidl
#define SHGFI_USEFILEATTRIBUTES 0x000000010     // use passed dwFileAttribute
#define SHGFI_ADDOVERLAYS       0x000000020     // apply the appropriate overlays
#define SHGFI_OVERLAYINDEX      0x000000040     // Get the index of the overlay
                                                // in the upper 8 bits of the iIcon
};

mask DWORD ShellFolderGetAttributesOf
{
#define SFGAO_CANCOPY           0x1 // Objects can be copied    (0x1)
#define SFGAO_CANMOVE           0x2 // Objects can be moved     (0x2)
#define SFGAO_CANLINK           0x4 // Objects can be linked    (0x4)
#define SFGAO_CANRENAME         0x00000010L     // Objects can be renamed
#define SFGAO_CANDELETE         0x00000020L     // Objects can be deleted
#define SFGAO_HASPROPSHEET      0x00000040L     // Objects have property sheets
#define SFGAO_DROPTARGET        0x00000100L     // Objects are drop target
#define SFGAO_CAPABILITYMASK    0x00000177L
#define SFGAO_LINK              0x00010000L     // Shortcut (link)
#define SFGAO_SHARE             0x00020000L     // shared
#define SFGAO_READONLY          0x00040000L     // read-only
#define SFGAO_GHOSTED           0x00080000L     // ghosted icon
#define SFGAO_HIDDEN            0x00080000L     // hidden object
#define SFGAO_DISPLAYATTRMASK   0x000F0000L
#define SFGAO_FILESYSANCESTOR   0x10000000L     // It contains file system folder
#define SFGAO_FOLDER            0x20000000L     // It's a folder.
#define SFGAO_FILESYSTEM        0x40000000L     // is a file system thing (file/folder/root)
#define SFGAO_HASSUBFOLDER      0x80000000L     // Expandable in the map pane
#define SFGAO_CONTENTSMASK      0x80000000L
#define SFGAO_VALIDATE          0x01000000L     // invalidate cached information
#define SFGAO_REMOVABLE         0x02000000L     // is this removeable media?
#define SFGAO_COMPRESSED        0x04000000L     // Object is compressed (use alt color)
#define SFGAO_BROWSABLE         0x08000000L     // is in-place browsable
#define SFGAO_NONENUMERATED     0x00100000L     // is a non-enumerated object
#define SFGAO_NEWCONTENT        0x00200000L     // should show bold in explorer tree
#define SFGAO_CANMONIKER        0x00400000L     // can create monikers for its objects
};

typedef struct _SHFILEINFOA
{
        HRSRC       hIcon;                      // out: icon
        int         iIcon;                      // out: icon index
        ShellFolderGetAttributesOf dwAttributes;               // out: SFGAO_ flags
        CHAR        szDisplayName[260];    // out: display name (or path)
        CHAR        szTypeName[80];             // out: type name
} SHFILEINFOA;
typedef struct _SHFILEINFOW
{
        HRSRC       hIcon;                      // out: icon
        int         iIcon;                      // out: icon index
        ShellFolderGetAttributesOf dwAttributes;               // out: SFGAO_ flags
        WCHAR       szDisplayName[260];    // out: display name (or path)
        WCHAR       szTypeName[80];             // out: type name
} SHFILEINFOW;

value DWORD SHGetFolderPathFlags
{
#define SHGFP_TYPE_CURRENT   0   // current value for user, verify it exists
#define SHGFP_TYPE_DEFAULT   1   // default value, may not exist
};

value int SHGetIconOverlayIndexValue
{
#define IDO_SHGIOI_SHARE  0x0FFFFFFF
#define IDO_SHGIOI_LINK   0x0FFFFFFE
#define IDO_SHGIOI_SLOWFILE 0x0FFFFFFFD
};

mask DWORD SHGetSettingsMask
{
#define SSF_SHOWALLOBJECTS          0x00000001
#define SSF_SHOWEXTENSIONS          0x00000002
#define SSF_SHOWCOMPCOLOR           0x00000008
#define SSF_SHOWSYSFILES            0x00000020
#define SSF_DOUBLECLICKINWEBVIEW    0x00000080
#define SSF_SHOWATTRIBCOL           0x00000100
#define SSF_DESKTOPHTML             0x00000200
#define SSF_WIN95CLASSIC            0x00000400
#define SSF_DONTPRETTYPATH          0x00000800
#define SSF_SHOWINFOTIP             0x00002000
#define SSF_MAPNETDRVBUTTON         0x00001000
#define SSF_NOCONFIRMRECYCLE        0x00008000
#define SSF_HIDEICONS               0x00004000
};


value int CSIDL
{
#define CSIDL_DESKTOP                   0x0000        // <desktop>
#define CSIDL_INTERNET                  0x0001        // Internet Explorer (icon on desktop)
#define CSIDL_PROGRAMS                  0x0002        // Start Menu\Programs
#define CSIDL_CONTROLS                  0x0003        // My Computer\Control Panel
#define CSIDL_PRINTERS                  0x0004        // My Computer\Printers
#define CSIDL_PERSONAL                  0x0005        // My Documents
#define CSIDL_FAVORITES                 0x0006        // <user name>\Favorites
#define CSIDL_STARTUP                   0x0007        // Start Menu\Programs\Startup
#define CSIDL_RECENT                    0x0008        // <user name>\Recent
#define CSIDL_SENDTO                    0x0009        // <user name>\SendTo
#define CSIDL_BITBUCKET                 0x000a        // <desktop>\Recycle Bin
#define CSIDL_STARTMENU                 0x000b        // <user name>\Start Menu
#define CSIDL_DESKTOPDIRECTORY          0x0010        // <user name>\Desktop
#define CSIDL_DRIVES                    0x0011        // My Computer
#define CSIDL_NETWORK                   0x0012        // Network Neighborhood
#define CSIDL_NETHOOD                   0x0013        // <user name>\nethood
#define CSIDL_FONTS                     0x0014        // windows\fonts
#define CSIDL_TEMPLATES                 0x0015
#define CSIDL_COMMON_STARTMENU          0x0016        // All Users\Start Menu
#define CSIDL_COMMON_PROGRAMS           0X0017        // All Users\Programs
#define CSIDL_COMMON_STARTUP            0x0018        // All Users\Startup
#define CSIDL_COMMON_DESKTOPDIRECTORY   0x0019        // All Users\Desktop
#define CSIDL_APPDATA                   0x001a        // <user name>\Application Data
#define CSIDL_PRINTHOOD                 0x001b        // <user name>\PrintHood
#define CSIDL_LOCAL_APPDATA             0x001c        // <user name>\Local Settings\Applicaiton Data (non roaming)
#define CSIDL_ALTSTARTUP                0x001d        // non localized startup
#define CSIDL_COMMON_ALTSTARTUP         0x001e        // non localized common startup
#define CSIDL_COMMON_FAVORITES          0x001f
#define CSIDL_INTERNET_CACHE            0x0020
#define CSIDL_COOKIES                   0x0021
#define CSIDL_HISTORY                   0x0022
#define CSIDL_COMMON_APPDATA            0x0023        // All Users\Application Data
#define CSIDL_WINDOWS                   0x0024        // GetWindowsDirectory()
#define CSIDL_SYSTEM                    0x0025        // GetSystemDirectory()
#define CSIDL_PROGRAM_FILES             0x0026        // C:\Program Files
#define CSIDL_MYPICTURES                0x0027        // C:\Program Files\My Pictures
#define CSIDL_PROFILE                   0x0028        // USERPROFILE
#define CSIDL_SYSTEMX86                 0x0029        // x86 system directory on RISC
#define CSIDL_PROGRAM_FILESX86          0x002a        // x86 C:\Program Files on RISC
#define CSIDL_PROGRAM_FILES_COMMON      0x002b        // C:\Program Files\Common
#define CSIDL_PROGRAM_FILES_COMMONX86   0x002c        // x86 Program Files\Common on RISC
#define CSIDL_COMMON_TEMPLATES          0x002d        // All Users\Templates
#define CSIDL_COMMON_DOCUMENTS          0x002e        // All Users\Documents
#define CSIDL_COMMON_ADMINTOOLS         0x002f        // All Users\Start Menu\Programs\Administrative Tools
#define CSIDL_ADMINTOOLS                0x0030        // <user name>\Start Menu\Programs\Administrative Tools
#define CSIDL_CONNECTIONS               0x0031        // Network and Dial-up Connections

#define CSIDL_FLAG_CREATE               0x8000        // combine with CSIDL_ value to force folder creation in SHGetFolderPath()
#define CSIDL_FLAG_DONT_VERIFY          0x4000        // combine with CSIDL_ value to return an unverified folder path
#define CSIDL_FLAG_MASK                 0xFF00        // mask for all possible flag values
};

value UINT SHInvokePrinterCommandAction
{
#define PRINTACTION_OPEN           0
#define PRINTACTION_PROPERTIES     1
#define PRINTACTION_NETINSTALL     2
#define PRINTACTION_NETINSTALLLINK 3
#define PRINTACTION_TESTPAGE       4
#define PRINTACTION_OPENNETPRN     5
#define PRINTACTION_DOCUMENTDEFAULTS 6
#define PRINTACTION_SERVERPROPERTIES 7
};

typedef struct _SHQUERYRBINFO {
    DWORD cbSize;
    __int64 i64Size;
    __int64 i64NumItems;
} SHQUERYRBINFO, *LPSHQUERYRBINFO;

value DWORD TranslateUrlFlags
{
#define TRANSLATEURL_FL_GUESS_PROTOCOL         0x0001     // Guess protocol if missing
#define TRANSLATEURL_FL_USE_DEFAULT_PROTOCOL   0x0002     // Use default protocol if missing
};

value DWORD URLAssociationDialogFlags
{
#define URLASSOCDLG_FL_USE_DEFAULT_NAME        0x0001
#define URLASSOCDLG_FL_REGISTER_ASSOC          0x0002
};

value UINT WinHelpCommands
{
#define HELP_CONTEXT      0x0001L  /* Display topic in ulTopic */
#define HELP_QUIT         0x0002L  /* Terminate help */
#define HELP_INDEX        0x0003L  /* Display index */
#define HELP_CONTENTS     0x0003L
#define HELP_HELPONHELP   0x0004L  /* Display help on using help */
#define HELP_SETINDEX     0x0005L  /* Set current Index for multi index help */
#define HELP_CONTEXTPOPUP 0x0008L
#define HELP_FORCEFILE    0x0009L
#define HELP_KEY          0x0101L  /* Display topic for keyword in offabData */
#define HELP_COMMAND      0x0102L
#define HELP_PARTIALKEY   0x0105L
#define HELP_MULTIKEY     0x0201L
#define HELP_SETWINPOS    0x0203L
#define HELP_CONTEXTMENU  0x000a
#define HELP_FINDER       0x000b
#define HELP_WM_HELP      0x000c
#define HELP_SETPOPUP_POS 0x000d

#define HELP_TCARD              0x8000
#define HELP_TCARD_DATA         0x0010
#define HELP_TCARD_OTHER_CALLER 0x0011
};

typedef struct _STRRET {
   UINT uType;
   CHAR cStr[260];
} STRRET, *LPSTRRET;


module SHELL32.DLL:
category Shell:

HRESULT DllGetClassObject(
                         REFCLSID rclsid,
                         [iid] REFIID riid,
                         [out] COM_INTERFACE_PTR * ppv
                         );

interface IShellFolder : IUnknown
{
    HRESULT ParseDisplayName(HWND hwnd, LPBC pbc, LPOLESTR pszDisplayName,
                            [out] ULONG *pchEaten, [out] LPITEMIDLIST *ppidl, [out] ULONG *pdwAttributes);

    HRESULT EnumObjects(HWND hwnd, DWORD grfFlags, IEnumIDList **ppenumIDList);

    HRESULT BindToObject(LPCITEMIDLIST pidl, LPBC pbc, [iid] REFIID riid, [out] COM_INTERFACE_PTR *ppv);
    HRESULT BindToStorage(LPCITEMIDLIST pidl, LPBC pbc, [iid] REFIID riid, [out] COM_INTERFACE_PTR *ppv);
    HRESULT CompareIDs(LPARAM lParam, LPCITEMIDLIST pidl1, LPCITEMIDLIST pidl2);
    HRESULT CreateViewObject(HWND hwndOwner, [iid] REFIID riid, [out] COM_INTERFACE_PTR *ppv);
    HRESULT GetAttributesOf(UINT cidl, [out] LPCITEMIDLIST * apidl, [out] ULONG * rgfInOut);
    HRESULT GetUIObjectOf(HWND hwndOwner, UINT cidl, LPCITEMIDLIST * apidl,
                         [iid] REFIID riid, UINT * prgfInOut, [out] COM_INTERFACE_PTR *ppv);
    HRESULT GetDisplayNameOf(LPCITEMIDLIST pidl, DWORD uFlags, [out] LPSTRRET lpName);
    HRESULT SetNameOf(HWND hwnd, LPCITEMIDLIST pidl, LPCOLESTR pszName,
                     DWORD uFlags, LPITEMIDLIST *ppidlOut);
};

interface IShellLinkA : IUnknown
{
    HRESULT GetPath( [out] LPSTR pszFile, INT cchMaxPath, [out] LPWIN32_FIND_DATAA pfd, DWORD fFlags );

    HRESULT GetIDList([out] LPITEMIDLIST * ppidl);
    HRESULT SetIDList(LPCITEMIDLIST pidl);

    HRESULT GetDescription([out] LPSTR pszName, INT cchMaxName);
    HRESULT SetDescription(LPCSTR pszName);

    HRESULT GetWorkingDirectory([out] LPSTR pszDir, INT cchMaxPath);
    HRESULT SetWorkingDirectory(LPCSTR pszDir);

    HRESULT GetArguments([out] LPSTR pszArgs, INT cchMaxPath);
    HRESULT SetArguments(LPCSTR pszArgs);

    HRESULT GetHotkey([out] WORD *pwHotkey);
    HRESULT SetHotkey(WORD wHotkey);

    HRESULT GetShowCmd([out] INT *piShowCmd);
    HRESULT SetShowCmd(INT iShowCmd);

    HRESULT GetIconLocation([out] LPSTR pszIconPath, INT cchIconPath, [out] INT *piIcon);
    HRESULT SetIconLocation(LPCSTR pszIconPath, INT iIcon);

    HRESULT SetRelativePath(LPCSTR pszPathRel, DWORD dwReserved);

    HRESULT Resolve(HWND hwnd, DWORD fFlags);

    HRESULT SetPath(LPCSTR pszFile);
};

interface IShellLinkW : IUnknown
{
    HRESULT GetPath( [out] LPWSTR pszFile, INT cchMaxPath, [out] LPWIN32_FIND_DATAW pfd, DWORD fFlags );

    HRESULT GetIDList([out] LPITEMIDLIST * ppidl);
    HRESULT SetIDList(LPCITEMIDLIST pidl);

    HRESULT GetDescription([out] LPWSTR pszName, INT cchMaxName);
    HRESULT SetDescription(LPCWSTR pszName);

    HRESULT GetWorkingDirectory([out] LPWSTR pszDir, INT cchMaxPath);
    HRESULT SetWorkingDirectory(LPCWSTR pszDir);

    HRESULT GetArguments([out] LPWSTR pszArgs, INT cchMaxPath);
    HRESULT SetArguments(LPCWSTR pszArgs);

    HRESULT GetHotkey([out] WORD *pwHotkey);
    HRESULT SetHotkey(WORD wHotkey);

    HRESULT GetShowCmd([out] INT *piShowCmd);
    HRESULT SetShowCmd(INT iShowCmd);

    HRESULT GetIconLocation([out] LPWSTR pszIconPath, INT cchIconPath, [out] INT *piIcon);
    HRESULT SetIconLocation(LPCWSTR pszIconPath, INT iIcon);

    HRESULT SetRelativePath(LPCWSTR pszPathRel, DWORD dwReserved);

    HRESULT Resolve(HWND hwnd, DWORD fFlags);

    HRESULT SetPath(LPCWSTR pszFile);
};

DWORD DoEnvironmentSubstA(
    LPCSTR pszString,
    UINT cbSize
);

DWORD DoEnvironmentSubstW(
    LPCWSTR pszString,
    UINT cbSize
);

VOID DragAcceptFiles(
    HWND hWnd,
    BOOL fAccept
);

VOID DragFinish(
    HDROP hDrop
);

UINT DragQueryFileA(
    HDROP hDrop,
    UINT iFile,
    [out] LPSTR lpszFile,
    UINT cch
);

UINT DragQueryFileW(
    HDROP hDrop,
    UINT iFile,
    [out] LPWSTR lpszFile,
    UINT cch
);

BOOL DragQueryPoint(
    HDROP hDrop,
    LPPOINT lppt
);

FindExecutableError FindExecutableA(
    LPCSTR lpFile,
    LPCSTR lpDirectory,
    [out] LPSTR lpResult
);

FindExecutableError FindExecutableW(
    LPCWSTR lpFile,
    LPCWSTR lpDirectory,
    [out] LPWSTR lpResult
);

VOID SHAddToRecentDocs(
    SHAddToRecentDocsFlags uFlags,
    LPCVOID pv
);

UINT SHAppBarMessage(
    DWORD dwMessage,
    PAPPBARDATA pData
);

HRESULT SHBindToParent(
  LPCITEMIDLIST pidl,
  [iid] REFIID riid,
  [out] COM_INTERFACE_PTR *ppv,
  [out] LPCITEMIDLIST *ppidlLast
);

LPITEMIDLIST SHBrowseForFolderA(
    LPBROWSEINFOA lpbi
);

LPITEMIDLIST SHBrowseForFolderW(
    LPBROWSEINFOW lpbi
);

VOID SHChangeNotify(
    ChangeNotifyEventId wEventId,
    ChangeNotifyFlags uFlags,
    LPCVOID dwItem1,
    LPCVOID dwItem2
);

int SHCreateDirectoryExA(
    HWND hwnd,
    LPCSTR pszPath,
    SECURITY_ATTRIBUTES *psa
);

int SHCreateDirectoryExW(
    HWND hwnd,
    LPCWSTR pszPath,
    SECURITY_ATTRIBUTES *psa
);

BOOL Shell_NotifyIcon(
    NotifyIconMessage dwMessage,
    PNOTIFYICONDATA pnid
);

int ShellAboutA(
   HWND hWnd,
   LPCSTR szApp,
   LPCSTR szOtherStuff,
   HRSRC hIcon
);

int ShellAboutW(
   HWND hWnd,
   LPCWSTR szApp,
   LPCWSTR szOtherStuff,
   HRSRC hIcon
);

ShellExecuteError ShellExecuteA(
    HWND hwnd,
    LPCSTR lpVerb,
    LPCSTR lpFile,
    LPCSTR lpParameters,
    LPCSTR lpDirectory,
    ShowWindowCommand nShowCmd
);

ShellExecuteError ShellExecuteW(
    HWND hwnd,
    LPCSTR lpVerb,
    LPCSTR lpFile,
    LPCSTR lpParameters,
    LPCSTR lpDirectory,
    ShowWindowCommand nShowCmd
);

FailOnFalse [gle] ShellExecuteExA(
    LPSHELLEXECUTEINFOA lpExecInfo
);

FailOnFalse [gle] ShellExecuteExW(
    LPSHELLEXECUTEINFOW lpExecInfo
);

HRESULT SHEmptyRecycleBinA(
    HWND hwnd,
    LPCSTR pszRootPath,
    DWORD dwFlags
);

HRESULT SHEmptyRecycleBinW(
    HWND hwnd,
    LPCWSTR pszRootPath,
    SHEmptyRecycleBinFlags dwFlags
);

INT SHFileOperationA(
    LPSHFILEOPSTRUCTA lpFileOp
);

INT SHFileOperationW(
    LPSHFILEOPSTRUCTW lpFileOp
);

VOID SHFreeNameMappings(
    HANDLE hNameMappings
);

HRESULT SHGetDataFromIDListA(
    IShellFolder* psf,
    LPCITEMIDLIST pidl,
    SHGetDataFromIDListFormat nFormat,
    PVOID pv,
    int cb
);

HRESULT SHGetDataFromIDListW(
    IShellFolder* psf,
    LPCITEMIDLIST pidl,
    SHGetDataFromIDListFormat nFormat,
    PVOID pv,
    int cb
);

HRESULT SHGetDesktopFolder(
    [out] IShellFolder **ppshf
);

FailOnFalse SHGetDiskFreeSpaceA(
    LPCSTR pszVolume,
    [out] ULARGE_INTEGER *pqwFreeCaller,
    [out] ULARGE_INTEGER *pqwTot,
    [out] ULARGE_INTEGER *pqwFree
);

DWORD_PTR SHGetFileInfoA(
    LPCSTR pszPath,
    ShellFolderGetAttributesOf dwFileAttributes,
    [out] SHFILEINFOA *psfi,
    UINT cbFileInfo,
    SHGetFileInfoFlags uFlags
);

DWORD_PTR SHGetFileInfoW(
    LPCWSTR pszPath,
    ShellFolderGetAttributesOf dwFileAttributes,
    [out] SHFILEINFOW *psfi,
    UINT cbFileInfo,
    SHGetFileInfoFlags uFlags
);

HRESULT SHGetFolderLocation(
    HWND hwndOwner,
    int nFolder,
    HANDLE hToken,
    DWORD dwReserved,
    LPITEMIDLIST *ppidl
);

HRESULT SHGetFolderPathA(
    HWND hwndOwner,
    CSIDL nFolder,
    HANDLE hToken,
    SHGetFolderPathFlags dwFlags,
    [out] LPSTR pszPath
);

HRESULT SHGetFolderPathW(
    HWND hwndOwner,
    CSIDL nFolder,
    HANDLE hToken,
    SHGetFolderPathFlags dwFlags,
    [out] LPWSTR pszPath
);

int SHGetIconOverlayIndexA(
    LPCSTR pszIconPath,
    SHGetIconOverlayIndexValue iIconIndex
);

int SHGetIconOverlayIndexW(
    LPCWSTR pszIconPath,
    SHGetIconOverlayIndexValue iIconIndex
);

HRESULT SHGetInstanceExplorer(
    [out] IUnknown **ppunk
);

//HRESULT SHGetMalloc(
//    [out] IMalloc **ppMalloc
//);

HRESULT SHGetMalloc(
    LPVOID ppMalloc
);

FailOnFalse SHGetNewLinkInfoA(
    LPCSTR pszLinkTo,
    LPCSTR pszDir,
    [out] LPSTR pszName,
    [out] BOOL *pfMustCopy,
    UINT uFlags
);

FailOnFalse SHGetNewLinkInfoW(
    LPCWSTR pszLinkTo,
    LPCWSTR pszDir,
    [out] LPWSTR pszName,
    [out] BOOL *pfMustCopy,
    UINT uFlags
);

FailOnFalse SHGetPathFromIDListA(
    LPCITEMIDLIST pidl,
    [out] LPSTR pszPath
);

FailOnFalse SHGetPathFromIDListW(
    LPCITEMIDLIST pidl,
    [out] LPWSTR pszPath
);

VOID SHGetSettings(
    LPSHELLFLAGSTATE lpsfs,
    SHGetSettingsMask dwMask
);

HRESULT SHGetSpecialFolderLocation(
    HWND hwndOwner,
    CSIDL nFolder,
    [out] LPITEMIDLIST *ppidl
);

FailOnFalse SHGetSpecialFolderPathA(
    HWND hwndOwner,
    [out] LPSTR lpszPath,
    CSIDL nFolder,
    BOOL fCreate
);

FailOnFalse SHGetSpecialFolderPathW(
    HWND hwndOwner,
    [out] LPWSTR lpszPath,
    CSIDL nFolder,
    BOOL fCreate
);

FailOnFalse SHInvokePrinterCommandW(
    HWND hwnd,
    SHInvokePrinterCommandAction uAction,
    LPCWSTR lpBuf1,
    LPCWSTR lpBuf2,
    BOOL fModal
);

FailOnFalse SHInvokePrinterCommandA(
    HWND hwnd,
    SHInvokePrinterCommandAction uAction,
    LPCSTR lpBuf1,
    LPCSTR lpBuf2,
    BOOL fModal
);

HRESULT SHLoadInProc(
    REFCLSID rclsid
);

HRESULT SHQueryRecycleBinA(
    LPCSTR pszRootPath,
    LPSHQUERYRBINFO pSHQueryRBInfo
);

HRESULT SHQueryRecycleBinW(
    LPCWSTR pszRootPath,
    LPSHQUERYRBINFO pSHQueryRBInfo
);

module USER32.DLL:
FailOnFalse [gle] WinHelpA(
    HWND hWndMain,
    LPCSTR lpszHelp,
    WinHelpCommands uCommand,
    DWORD dwData
);

FailOnFalse [gle] WinHelpW(
    HWND hWndMain,
    LPCWSTR lpszHelp,
    WinHelpCommands uCommand,
    DWORD dwData
);


module URL.DLL:

BOOL InetIsOffline(
  DWORD dwFlags
);

HRESULT MIMEAssociationDialogA(
  HWND     hwndParent,
  DWORD    dwInFlags,
  LPCSTR   pcszFile,
  LPCSTR   pcszMIMEContentType,
  LPSTR    pszAppBuf,
  UINT     ucAppBufLen
);

HRESULT MIMEAssociationDialogW(
  HWND     hwndParent,
  DWORD    dwInFlags,
  LPCWSTR   pcszFile,
  LPCWSTR   pcszMIMEContentType,
  LPWSTR    pszAppBuf,
  UINT     ucAppBufLen
);

HRESULT TranslateURL(
  LPCSTR pcszURL,
  TranslateUrlFlags dwInFlags,
  [out] LPSTR *ppszTranslatedURL
);

HRESULT URLAssociationDialog(
  HWND hwndParent,
  URLAssociationDialogFlags dwInFlags,
  LPCSTR pcszFile,
  LPCSTR pcszURL,
  LPSTR pszAppBuf,
  UINT ucAppBufLen
);
