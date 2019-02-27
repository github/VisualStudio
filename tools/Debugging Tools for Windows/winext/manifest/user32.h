// ---------------------------------------------------------------------------
// ---------------------------------------------------------------------------
//
//                              USER32 API Set
//
// ---------------------------------------------------------------------------
// ---------------------------------------------------------------------------
module USER32.DLL:
category User32:

value DWORD PeekMessageOptions
{
#define PM_NOREMOVE         0x0000
#define PM_REMOVE           0x0001
#define PM_NOYIELD          0x0002
};

value DWORD WindowsMessage
{
#define WM_NULL                         0x0000
#define WM_CREATE                       0x0001
#define WM_DESTROY                      0x0002
#define WM_MOVE                         0x0003
#define WM_SIZE                         0x0005
#define WM_ACTIVATE                     0x0006
#define WM_SETFOCUS                     0x0007
#define WM_KILLFOCUS                    0x0008
#define WM_ENABLE                       0x000A
#define WM_SETREDRAW                    0x000B
#define WM_SETTEXT                      0x000C
#define WM_GETTEXT                      0x000D
#define WM_GETTEXTLENGTH                0x000E
#define WM_PAINT                        0x000F
#define WM_CLOSE                        0x0010
#define WM_QUERYENDSESSION              0x0011
#define WM_QUIT                         0x0012
#define WM_QUERYOPEN                    0x0013
#define WM_ERASEBKGND                   0x0014
#define WM_SYSCOLORCHANGE               0x0015
#define WM_ENDSESSION                   0x0016
#define WM_SHOWWINDOW                   0x0018
#define WM_WININICHANGE                 0x001A
#define WM_DEVMODECHANGE                0x001B
#define WM_ACTIVATEAPP                  0x001C
#define WM_FONTCHANGE                   0x001D
#define WM_TIMECHANGE                   0x001E
#define WM_CANCELMODE                   0x001F
#define WM_SETCURSOR                    0x0020
#define WM_MOUSEACTIVATE                0x0021
#define WM_CHILDACTIVATE                0x0022
#define WM_QUEUESYNC                    0x0023
#define WM_GETMINMAXINFO                0x0024
#define WM_PAINTICON                    0x0026
#define WM_ICONERASEBKGND               0x0027
#define WM_NEXTDLGCTL                   0x0028
#define WM_SPOOLERSTATUS                0x002A
#define WM_DRAWITEM                     0x002B
#define WM_MEASUREITEM                  0x002C
#define WM_DELETEITEM                   0x002D
#define WM_VKEYTOITEM                   0x002E
#define WM_CHARTOITEM                   0x002F
#define WM_SETFONT                      0x0030
#define WM_GETFONT                      0x0031
#define WM_SETHOTKEY                    0x0032
#define WM_GETHOTKEY                    0x0033
#define WM_QUERYDRAGICON                0x0037
#define WM_COMPAREITEM                  0x0039
#define WM_GETOBJECT                    0x003D
#define WM_COMPACTING                   0x0041
#define WM_COMMNOTIFY                   0x0044
#define WM_WINDOWPOSCHANGING            0x0046
#define WM_WINDOWPOSCHANGED             0x0047
#define WM_POWER                        0x0048
#define WM_COPYDATA                     0x004A
#define WM_CANCELJOURNAL                0x004B
#define WM_NOTIFY                       0x004E
#define WM_INPUTLANGCHANGEREQUEST       0x0050
#define WM_INPUTLANGCHANGE              0x0051
#define WM_TCARD                        0x0052
#define WM_HELP                         0x0053
#define WM_USERCHANGED                  0x0054
#define WM_NOTIFYFORMAT                 0x0055
#define WM_CONTEXTMENU                  0x007B
#define WM_STYLECHANGING                0x007C
#define WM_STYLECHANGED                 0x007D
#define WM_DISPLAYCHANGE                0x007E
#define WM_GETICON                      0x007F
#define WM_SETICON                      0x0080
#define WM_NCCREATE                     0x0081
#define WM_NCDESTROY                    0x0082
#define WM_NCCALCSIZE                   0x0083
#define WM_NCHITTEST                    0x0084
#define WM_NCPAINT                      0x0085
#define WM_NCACTIVATE                   0x0086
#define WM_GETDLGCODE                   0x0087
#define WM_SYNCPAINT                    0x0088
#define WM_NCMOUSEMOVE                  0x00A0
#define WM_NCLBUTTONDOWN                0x00A1
#define WM_NCLBUTTONUP                  0x00A2
#define WM_NCLBUTTONDBLCLK              0x00A3
#define WM_NCRBUTTONDOWN                0x00A4
#define WM_NCRBUTTONUP                  0x00A5
#define WM_NCRBUTTONDBLCLK              0x00A6
#define WM_NCMBUTTONDOWN                0x00A7
#define WM_NCMBUTTONUP                  0x00A8
#define WM_NCMBUTTONDBLCLK              0x00A9

#define EM_GETSEL                       0x00B0
#define EM_SETSEL                       0x00B1
#define EM_GETRECT                      0x00B2
#define EM_SETRECT                      0x00B3
#define EM_SETRECTNP                    0x00B4
#define EM_SCROLL                       0x00B5
#define EM_LINESCROLL                   0x00B6
#define EM_SCROLLCARET                  0x00B7
#define EM_GETMODIFY                    0x00B8
#define EM_SETMODIFY                    0x00B9
#define EM_GETLINECOUNT                 0x00BA
#define EM_LINEINDEX                    0x00BB
#define EM_SETHANDLE                    0x00BC
#define EM_GETHANDLE                    0x00BD
#define EM_GETTHUMB                     0x00BE
#define EM_LINELENGTH                   0x00C1
#define EM_REPLACESEL                   0x00C2
#define EM_GETLINE                      0x00C4
#define EM_LIMITTEXT                    0x00C5
#define EM_CANUNDO                      0x00C6
#define EM_UNDO                         0x00C7
#define EM_FMTLINES                     0x00C8
#define EM_LINEFROMCHAR                 0x00C9
#define EM_SETTABSTOPS                  0x00CB
#define EM_SETPASSWORDCHAR              0x00CC
#define EM_EMPTYUNDOBUFFER              0x00CD
#define EM_GETFIRSTVISIBLELINE          0x00CE
#define EM_SETREADONLY                  0x00CF
#define EM_SETWORDBREAKPROC             0x00D0
#define EM_GETWORDBREAKPROC             0x00D1
#define EM_GETPASSWORDCHAR              0x00D2
#define EM_SETMARGINS                   0x00D3
#define EM_GETMARGINS                   0x00D4
#define EM_GETLIMITTEXT                 0x00D5
#define EM_POSFROMCHAR                  0x00D6
#define EM_CHARFROMPOS                  0x00D7
#define EM_SETIMESTATUS                 0x00D8
#define EM_GETIMESTATUS                 0x00D9

#define WM_KEYFIRST                     0x0100
#define WM_KEYDOWN                      0x0100
#define WM_KEYUP                        0x0101
#define WM_CHAR                         0x0102
#define WM_DEADCHAR                     0x0103
#define WM_SYSKEYDOWN                   0x0104
#define WM_SYSKEYUP                     0x0105
#define WM_SYSCHAR                      0x0106
#define WM_SYSDEADCHAR                  0x0107
#define WM_KEYLAST                      0x0108
#define WM_IME_STARTCOMPOSITION         0x010D
#define WM_IME_ENDCOMPOSITION           0x010E
#define WM_IME_COMPOSITION              0x010F
#define WM_IME_KEYLAST                  0x010F
#define WM_INITDIALOG                   0x0110
#define WM_COMMAND                      0x0111
#define WM_SYSCOMMAND                   0x0112
#define WM_TIMER                        0x0113
#define WM_HSCROLL                      0x0114
#define WM_VSCROLL                      0x0115
#define WM_INITMENU                     0x0116
#define WM_INITMENUPOPUP                0x0117
#define WM_SYSTIMER                     0x0118
#define WM_MENUSELECT                   0x011F
#define WM_MENUCHAR                     0x0120
#define WM_ENTERIDLE                    0x0121
#define WM_MENURBUTTONUP                0x0122
#define WM_MENUDRAG                     0x0123
#define WM_MENUGETOBJECT                0x0124
#define WM_UNINITMENUPOPUP              0x0125
#define WM_MENUCOMMAND                  0x0126
#define WM_CTLCOLORMSGBOX               0x0132
#define WM_CTLCOLOREDIT                 0x0133
#define WM_CTLCOLORLISTBOX              0x0134
#define WM_CTLCOLORBTN                  0x0135
#define WM_CTLCOLORDLG                  0x0136
#define WM_CTLCOLORSCROLLBAR            0x0137
#define WM_CTLCOLORSTATIC               0x0138
#define WM_MOUSEFIRST                   0x0200
#define WM_MOUSEMOVE                    0x0200
#define WM_LBUTTONDOWN                  0x0201
#define WM_LBUTTONUP                    0x0202
#define WM_LBUTTONDBLCLK                0x0203
#define WM_RBUTTONDOWN                  0x0204
#define WM_RBUTTONUP                    0x0205
#define WM_RBUTTONDBLCLK                0x0206
#define WM_MBUTTONDOWN                  0x0207
#define WM_MBUTTONUP                    0x0208
#define WM_MBUTTONDBLCLK                0x0209
#define WM_MOUSEWHEEL                   0x020A
#define WM_MOUSELAST                    0x0209
#define WM_PARENTNOTIFY                 0x0210
#define WM_ENTERMENULOOP                0x0211
#define WM_EXITMENULOOP                 0x0212
#define WM_NEXTMENU                     0x0213
#define WM_SIZING                       0x0214
#define WM_CAPTURECHANGED               0x0215
#define WM_MOVING                       0x0216
#define WM_POWERBROADCAST               0x0218
#define WM_DEVICECHANGE                 0x0219
#define WM_MDICREATE                    0x0220
#define WM_MDIDESTROY                   0x0221
#define WM_MDIACTIVATE                  0x0222
#define WM_MDIRESTORE                   0x0223
#define WM_MDINEXT                      0x0224
#define WM_MDIMAXIMIZE                  0x0225
#define WM_MDITILE                      0x0226
#define WM_MDICASCADE                   0x0227
#define WM_MDIICONARRANGE               0x0228
#define WM_MDIGETACTIVE                 0x0229
#define WM_MDISETMENU                   0x0230
#define WM_ENTERSIZEMOVE                0x0231
#define WM_EXITSIZEMOVE                 0x0232
#define WM_DROPFILES                    0x0233
#define WM_MDIREFRESHMENU               0x0234
#define WM_IME_SETCONTEXT               0x0281
#define WM_IME_NOTIFY                   0x0282
#define WM_IME_CONTROL                  0x0283
#define WM_IME_COMPOSITIONFULL          0x0284
#define WM_IME_SELECT                   0x0285
#define WM_IME_CHAR                     0x0286
#define WM_IME_REQUEST                  0x0288
#define WM_IME_KEYDOWN                  0x0290
#define WM_IME_KEYUP                    0x0291
#define WM_MOUSEHOVER                   0x02A1
#define WM_MOUSELEAVE                   0x02A3
#define WM_CUT                          0x0300
#define WM_COPY                         0x0301
#define WM_PASTE                        0x0302
#define WM_CLEAR                        0x0303
#define WM_UNDO                         0x0304
#define WM_RENDERFORMAT                 0x0305
#define WM_RENDERALLFORMATS             0x0306
#define WM_DESTROYCLIPBOARD             0x0307
#define WM_DRAWCLIPBOARD                0x0308
#define WM_PAINTCLIPBOARD               0x0309
#define WM_VSCROLLCLIPBOARD             0x030A
#define WM_SIZECLIPBOARD                0x030B
#define WM_ASKCBFORMATNAME              0x030C
#define WM_CHANGECBCHAIN                0x030D
#define WM_HSCROLLCLIPBOARD             0x030E
#define WM_QUERYNEWPALETTE              0x030F
#define WM_PALETTEISCHANGING            0x0310
#define WM_PALETTECHANGED               0x0311
#define WM_HOTKEY                       0x0312
#define WM_PRINT                        0x0317
#define WM_PRINTCLIENT                  0x0318
#define WM_HANDHELDFIRST                0x0358
#define WM_HANDHELDLAST                 0x035F
#define WM_AFXFIRST                     0x0360
#define WM_AFXLAST                      0x037F
};

value DWORD SystemObjectId
{
#define     OBJID_WINDOW            0x00000000
#define     OBJID_SYSMENU           0xFFFFFFFF
#define     OBJID_TITLEBAR          0xFFFFFFFE
#define     OBJID_MENU              0xFFFFFFFD
#define     OBJID_CLIENT            0xFFFFFFFC
#define     OBJID_VSCROLL           0xFFFFFFFB
#define     OBJID_HSCROLL           0xFFFFFFFA
#define     OBJID_SIZEGRIP          0xFFFFFFF9
#define     OBJID_CARET             0xFFFFFFF8
#define     OBJID_CURSOR            0xFFFFFFF7
#define     OBJID_ALERT             0xFFFFFFF6
#define     OBJID_SOUND             0xFFFFFFF5
#define     OBJID_QUERYCLASSNAMEIDX 0xFFFFFFF4
#define     OBJID_NATIVEOM          0xFFFFFFF0
};

typedef struct tagMSG {     // msg
    HWND            hwnd;
    WindowsMessage  message;
    WPARAM          wParam;
    LPARAM          lParam;
    DWORD           time;
    POINT           pt;
} MSG, *LPMSG;

mask DWORD WindowStyle
{
#define WS_OVERLAPPED       0x00000000
#define WS_POPUP            0x80000000
#define WS_CHILD            0x40000000
#define WS_MINIMIZE         0x20000000
#define WS_VISIBLE          0x10000000
#define WS_DISABLED         0x08000000
#define WS_CLIPSIBLINGS     0x04000000
#define WS_CLIPCHILDREN     0x02000000
#define WS_MAXIMIZE         0x01000000
#define WS_BORDER           0x00800000
#define WS_DLGFRAME         0x00400000
#define WS_VSCROLL          0x00200000
#define WS_HSCROLL          0x00100000
#define WS_SYSMENU          0x00080000
#define WS_THICKFRAME       0x00040000
#define WS_GROUP            0x00020000
#define WS_TABSTOP          0x00010000
#define WS_ACTIVECAPTION    0x00000001
};

mask DWORD WindowStyleEx
{
#define WS_EX_LEFT              0x00000000

#define WS_EX_DLGMODALFRAME     0x00000001
#define WS_EX_NOPARENTNOTIFY    0x00000004
#define WS_EX_TOPMOST           0x00000008
#define WS_EX_ACCEPTFILES       0x00000010
#define WS_EX_TRANSPARENT       0x00000020
#define WS_EX_MDICHILD          0x00000040
#define WS_EX_TOOLWINDOW        0x00000080
#define WS_EX_WINDOWEDGE        0x00000100
#define WS_EX_CLIENTEDGE        0x00000200
#define WS_EX_CONTEXTHELP       0x00000400

#define WS_EX_RIGHT             0x00001000
#define WS_EX_RTLREADING        0x00002000
#define WS_EX_LEFTSCROLLBAR     0x00004000

#define WS_EX_CONTROLPARENT     0x00010000
#define WS_EX_STATICEDGE        0x00020000
#define WS_EX_APPWINDOW         0x00040000
#define WS_EX_LAYERED           0x00080000

#define WS_EX_NOINHERITLAYOUT   0x00100000
#define WS_EX_LAYOUTRTL         0x00400000
#define WS_EX_COMPOSITED        0x02000000
#define WS_EX_NOACTIVATE        0x08000000
};


HWND [gle] CreateWindowExA(
    WindowStyleEx dwExStyle,
    LPCSTR        lpClassName,
    LPCSTR        lpWindowName,
    WindowStyle   dwStyle,
    int           X,
    int           Y,
    int           nWidth,
    int           nHeight,
    HWND          hWndParent,
    ULONG         hMenu,
    HINSTANCE     hInstance,
    LPVOID        lpParam);

HWND [gle] CreateWindowExW(
    WindowStyleEx dwExStyle,
    LPCWSTR       lpClassName,
    LPCWSTR       lpWindowName,
    WindowStyle   dwStyle,
    int           X,
    int           Y,
    int           nWidth,
    int           nHeight,
    HWND          hWndParent,
    ULONG         hMenu,
    HINSTANCE     hInstance,
    LPVOID        lpParam);

FailOnFalse [gle] DestroyWindow( [da] HWND hWnd);

FailOnFalse [gle] CloseWindow(HWND hWnd);

mask DWORD ActivateKeyboardLayoutFlags
{
#define KLF_ACTIVATE        0x00000001
#define KLF_SUBSTITUTE_OK   0x00000002
#define KLF_REORDER         0x00000008
#define KLF_REPLACELANG     0x00000010
#define KLF_NOTELLSHELL     0x00000080
#define KLF_SETFORPROCESS   0x00000100
#define KLF_SHIFTLOCK       0x00010000
#define KLF_RESET           0x40000000
};


HKL ActivateKeyboardLayout(HKL hkl, ActivateKeyboardLayoutFlags Flags);
HKL GetKeyboardLayout(ThreadId idThread);

// BUGBUG: lpList is an array but there is no easy way to show all the values in it.
LongFailIfZero [gle] GetKeyboardLayoutList(int nBuff, [out] HKL* lpList);

FailOnFalse [gle] GetKeyboardLayoutNameA([out] LPSTR pwszKLID);
FailOnFalse [gle] GetKeyboardLayoutNameW([out] LPWSTR pwszKLID);


UINT GetKBCodePage();

mask DWORD KeyboardLayoutFlags
{
#define KLF_ACTIVATE        0x00000001
#define KLF_SUBSTITUTE_OK   0x00000002
#define KLF_REORDER         0x00000008
#define KLF_REPLACELANG     0x00000010
#define KLF_NOTELLSHELL     0x00000080
#define KLF_SETFORPROCESS   0x00000100
#define KLF_SHIFTLOCK       0x00010000
#define KLF_RESET           0x40000000
};

HKL [gle] LoadKeyboardLayoutA(LPCSTR pwszKLID, KeyboardLayoutFlags Flags);
HKL [gle] LoadKeyboardLayoutW(LPCWSTR pwszKLID, KeyboardLayoutFlags Flags);

FailOnFalse [gle] UnloadKeyboardLayout([da] HKL hkl);

FailOnFalse [gle] SetProcessDefaultLayout(DWORD dwDefaultLayout);
FailOnFalse [gle] GetProcessDefaultLayout([out] LPDWORD lpdwDefaultLayout);



FailOnFalse AdjustWindowRect([out] LPRECT lpRect, WindowStyle dwStyle, BOOL bMenu);
FailOnFalse AdjustWindowRectEx([out] LPRECT lpRect, WindowStyle dwStyle, BOOL bMenu, WindowStyleEx dwExStyle);

FailOnFalse AllowSetForegroundWindow(ProcessId dwProcessId);

HWND GetForegroundWindow();
FailOnFalse SetForegroundWindow(HWND hWnd);

value DWORD LockForegroundFlags
{
#define LSFW_LOCK       1
#define LSFW_UNLOCK     2
};

FailOnFalse [gle] LockSetForegroundWindow(LockForegroundFlags uLockCode);

HWND GetActiveWindow();
HWND [gle] SetActiveWindow(HWND hWnd);


mask DWORD AnimateWindowFlags
{
#define AW_HOR_POSITIVE             0x00000001
#define AW_HOR_NEGATIVE             0x00000002
#define AW_VER_POSITIVE             0x00000004
#define AW_VER_NEGATIVE             0x00000008
#define AW_CENTER                   0x00000010
#define AW_HIDE                     0x00010000
#define AW_ACTIVATE                 0x00020000
#define AW_SLIDE                    0x00040000
#define AW_BLEND                    0x00080000
};

FailOnFalse AnimateWindow(HWND hWnd, DWORD dwTime, AnimateWindowFlags dwFlags);

FailOnFalse AnyPopup();

// Menu flags:

mask DWORD MenuFlags
{
#define MF_ENABLED          0x00000000
#define MF_GRAYED           0x00000001
#define MF_DISABLED         0x00000002
#define MF_BITMAP           0x00000004
#define MF_CHECKED          0x00000008
#define MF_POPUP            0x00000010
#define MF_MENUBARBREAK     0x00000020
#define MF_MENUBREAK        0x00000040
#define MF_CHANGE_HILITE    0x00000080
#define MF_OWNERDRAW        0x00000100
#define MF_DELETE           0x00000200
#define MF_BYPOSITION       0x00000400
#define MF_SEPARATOR        0x00000800
};



FailOnFalse [gle] AppendMenuA(HMENU hMenu, MenuFlags uFlags, DWORD uIDNewItem, LPCSTR lpNewItem);
FailOnFalse [gle] AppendMenuW(HMENU hMenu, MenuFlags uFlags, DWORD uIDNewItem, LPCWSTR lpNewItem);

HMENU [gle] LoadMenuA(HINSTANCE hInstance, LPCSTR lpMenuName);
HMENU [gle] LoadMenuW(HINSTANCE hInstance, LPCWSTR lpMenuName);

HMENU [gle] LoadMenuIndirectA(LPVOID lpMenuTemplate);
HMENU [gle] LoadMenuIndirectW(LPVOID lpMenuTemplate);


HMENU [gle] CreateMenu();

FailOnFalse [gle] DestroyMenu( [da] HMENU hMenu);

FailOnFalse [gle] DeleteMenu(HMENU hMenu, UINT uPosition, MenuFlags uFlags);

MenuFlags CheckMenuItem(HMENU hMenu, UINT uIDCheckItem, MenuFlags uCheck);

FailOnFalse [gle] CheckMenuRadioItem(HMENU     hMenu,
                                     UINT      idFirst,
                                     UINT      idLast,
                                     UINT      idCheck,
                                     MenuFlags uFlags);

HMENU [gle] CreatePopupMenu();

FailOnFalse [gle] DrawMenuBar(HWND hWnd);

MenuFlags EnableMenuItem(HMENU hMenu, UINT uIDEnableItem, MenuFlags uEnable);

FailOnFalse [gle] EndMenu();

HMENU GetMenu(HWND hWnd);

typedef struct tagMENUBARINFO
{
    DWORD cbSize;
    RECT  rcBar;
    HMENU hMenu;
    HWND  hwndMenu;
    DWORD fFocused;
} MENUBARINFO, *PMENUBARINFO, *LPMENUBARINFO;

FailOnFalse [gle] GetMenuBarInfo(HWND              hwnd,
                                 SystemObjectId    idObject,
                                 LONG              idItem,
                                 [out] MENUBARINFO pmbi);

LONG GetMenuCheckMarkDimensions();

DWORD GetMenuContextHelpId(HMENU hmenu);

mask DWORD GetMenuDefItemFlags
{
#define GMDI_USEDISABLED    0x0001
#define GMDI_GOINTOPOPUPS   0x0002
};

LongFailIfNeg1 [gle] GetMenuDefaultItem(HMENU hMenu, BOOL bByPos, GetMenuDefItemFlags gmdiFlags);

mask DWORD MenuInfoMask
{
#define MIM_MAXHEIGHT               0x00000001
#define MIM_BACKGROUND              0x00000002
#define MIM_HELPID                  0x00000004
#define MIM_MENUDATA                0x00000008
#define MIM_STYLE                   0x00000010
#define MIM_APPLYTOSUBMENUS         0x80000000
};

mask DWORD MenuInfoStyle
{
#define MNS_NOCHECK         0x80000000
#define MNS_MODELESS        0x40000000
#define MNS_DRAGDROP        0x20000000
#define MNS_AUTODISMISS     0x10000000
#define MNS_NOTIFYBYPOS     0x08000000
#define MNS_CHECKORBMP      0x04000000
};

typedef struct tagMENUINFO
{
    DWORD         cbSize;
    MenuInfoMask  fMask;
    MenuInfoStyle dwStyle;
    UINT          cyMax;
    HRSRC         hbrBack;
    DWORD         dwContextHelpID;
    DWORD         dwMenuData;
} MENUINFO, *LPMENUINFO;

FailOnFalse [gle] GetMenuInfo(HMENU hMenu, [out] LPMENUINFO lpmi);

LongFailIfNeg1 [gle] GetMenuItemCount(HMENU hMenu);

LongFailIfNeg1 GetMenuItemID(HMENU hMenu, int nPos);

mask DWORD MenuItemInfoMask
{
#define MIIM_STATE       0x00000001
#define MIIM_ID          0x00000002
#define MIIM_SUBMENU     0x00000004
#define MIIM_CHECKMARKS  0x00000008
#define MIIM_TYPE        0x00000010
#define MIIM_DATA        0x00000020

#define MIIM_STRING      0x00000040
#define MIIM_BITMAP      0x00000080
#define MIIM_FTYPE       0x00000100
};

mask DWORD MenuItemInfoType
{
#define MFT_STRING          0x00000000
#define MFT_BITMAP          0x00000004
#define MFT_MENUBARBREAK    0x00000020
#define MFT_MENUBREAK       0x00000040
#define MFT_OWNERDRAW       0x00000100
#define MFT_RADIOCHECK      0x00000200
#define MFT_SEPARATOR       0x00000800
#define MFT_RIGHTORDER      0x00002000
#define MFT_RIGHTJUSTIFY    0x00004000
};

mask DWORD MenuItemInfoState
{
#define MFS_CHECKED         0x00000008
#define MFS_DEFAULT         0x00001000
#define MFS_DISABLED1       0x00000001
#define MFS_DISABLED2       0x00000002
#define MFS_HILITE          0x00000080
};

typedef struct tagMENUITEMINFOA
{
    UINT              cbSize;
    MenuItemInfoMask  fMask;
    MenuItemInfoType  fType;
    MenuItemInfoState fState;
    UINT              wID;
    HMENU             hSubMenu;
    HRSRC             hbmpChecked;
    HRSRC             hbmpUnchecked;
    DWORD             dwItemData;
    LPSTR             dwTypeData;
    UINT              cch;
    HRSRC             hbmpItem;
}   MENUITEMINFOA, *LPMENUITEMINFOA;

typedef struct tagMENUITEMINFOW
{
    UINT              cbSize;
    MenuItemInfoMask  fMask;
    MenuItemInfoType  fType;
    MenuItemInfoState fState;
    UINT              wID;
    HMENU             hSubMenu;
    HRSRC             hbmpChecked;
    HRSRC             hbmpUnchecked;
    DWORD             dwItemData;
    LPWSTR            dwTypeData;
    UINT              cch;
    HRSRC             hbmpItem;
}   MENUITEMINFOW, *LPMENUITEMINFOW;


FailOnFalse [gle] GetMenuItemInfoA(HMENU hMenu,
                                   UINT  uItem,
                                   BOOL  fByPosition,
                                   [out] LPMENUITEMINFOA lpmii);

FailOnFalse [gle] GetMenuItemInfoW(HMENU hMenu,
                                   UINT  uItem,
                                   BOOL  fByPosition,
                                   [out] LPMENUITEMINFOW lpmii);

FailOnFalse [gle] GetMenuItemRect(HWND hWnd, HMENU hMenu, UINT uItem, [out] LPRECT lprcItem);

MenuFlags GetMenuState(HMENU hMenu, UINT uId, MenuFlags uFlags);

UintFailIfZero GetMenuStringA(HMENU       hMenu,
                              UINT        uIDItem,
                              [out] LPSTR lpString,
                              int         nMaxCount,
                              MenuFlags   uFlag);

UintFailIfZero GetMenuStringW(HMENU        hMenu,
                              UINT         uIDItem,
                              [out] LPWSTR lpString,
                              int          nMaxCount,
                              MenuFlags    uFlag);

HMENU GetSubMenu(HMENU hMenu, int nPos);

HMENU GetSystemMenu(HWND hWnd, BOOL bRevert);

FailOnFalse HiliteMenuItem(HWND hWnd, HMENU hMenu, UINT uIDHiliteItem, MenuFlags uHilite);

FailOnFalse [gle] InsertMenuA(HMENU     hMenu,
                              UINT      uPosition,
                              MenuFlags uFlags,
                              DWORD     uIDNewItem,
                              DWORD     lpNewItem);

FailOnFalse [gle] InsertMenuW(HMENU     hMenu,
                              UINT      uPosition,
                              MenuFlags uFlags,
                              DWORD     uIDNewItem,
                              DWORD     lpNewItem);

FailOnFalse InsertMenuItemA(HMENU           hMenu,
                            UINT            uItem,
                            BOOL            bByPosition,
                            LPMENUITEMINFOA lpmii);

FailOnFalse InsertMenuItemW(HMENU           hMenu,
                            UINT            uItem,
                            BOOL            bByPosition,
                            LPMENUITEMINFOW lpmii);


LongFailIfNeg1 MenuItemFromPoint(HWND hWnd, HMENU hMenu, int x, int y);

FailOnFalse [gle] ModifyMenuA(HMENU     hMenu,
                              UINT      uPosition,
                              MenuFlags uFlags,
                              DWORD     uIDNewItem,
                              DWORD     lpNewItem);

FailOnFalse [gle] ModifyMenuW(HMENU     hMenu,
                              UINT      uPosition,
                              MenuFlags uFlags,
                              DWORD     uIDNewItem,
                              DWORD     lpNewItem);


FailOnFalse [gle] RemoveMenu(HMENU     hMenu,
                             UINT      uPosition,
                             MenuFlags uFlags);

FailOnFalse [gle] SetMenu(HWND hWnd, HMENU hMenu);

FailOnFalse [gle] SetMenuContextHelpId(HMENU hmenu, DWORD dwContextHelpId);

FailOnFalse [gle] SetMenuDefaultItem(HMENU hMenu, UINT uItem, BOOL fByPos);

FailOnFalse [gle] SetMenuInfo(HMENU hMenu, LPMENUINFO lpmi);

FailOnFalse [gle] SetMenuItemBitmaps(HMENU     hMenu,
                                     UINT      uPosition,
                                     MenuFlags uFlags,
                                     HRSRC     hBitmapUnchecked,
                                     HRSRC     hBitmapChecked);

FailOnFalse [gle] SetMenuItemInfoA(HMENU           hMenu,
                                   UINT            uItem,
                                   BOOL            fByPosition,
                                   LPMENUITEMINFOA lpmii);

FailOnFalse [gle] SetMenuItemInfoW(HMENU           hMenu,
                                   UINT            uItem,
                                   BOOL            fByPosition,
                                   LPMENUITEMINFOW lpmii);

mask DWORD TrackPopupMenuFlags
{
#define TPM_LEFTTOPALIGN    0x0000
#define TPM_RECURSE         0x0001
#define TPM_RIGHTBUTTON     0x0002
#define TPM_CENTERALIGN     0x0004
#define TPM_RIGHTALIGN      0x0008
#define TPM_VCENTERALIGN    0x0010
#define TPM_BOTTOMALIGN     0x0020
#define TPM_VERTICAL        0x0040
#define TPM_NONOTIFY        0x0080
#define TPM_RETURNCMD       0x0100
#define TPM_HORPOSANIMATION 0x0400
#define TPM_HORNEGANIMATION 0x0800
#define TPM_VERPOSANIMATION 0x1000
#define TPM_VERNEGANIMATION 0x2000
#define TPM_NOANIMATION     0x4000
};


LongFailIfZero [gle] TrackPopupMenu(HMENU               hMenu,
                                    TrackPopupMenuFlags uFlags,
                                    int                 x,
                                    int                 y,
                                    int                 nReserved,
                                    HWND                hWnd,
                                    LPRECT              lprect);

typedef struct tagTPMPARAMS
{
    UINT    cbSize;
    RECT    rcExclude;
} TPMPARAMS, *LPTPMPARAMS;

LongFailIfZero [gle] TrackPopupMenuEx(HMENU hMenu,
                                      TrackPopupMenuFlags uFlags,
                                      int                 x,
                                      int                 y,
                                      HWND                hWnd,
                                      LPTPMPARAMS         lpParams);


FailOnFalse [gle] ArrangeIconicWindows(HWND hwnd);

FailOnFalse [gle] AttachThreadInput(ThreadId idAttach, ThreadId idAttachTo, BOOL fAttach);

value DWORD HDWP
{
#define NULL                    0  [fail]
};

alias HDWP;

mask DWORD SetWindowPosFlags
{
#define SWP_NOSIZE          0x0001
#define SWP_NOMOVE          0x0002
#define SWP_NOZORDER        0x0004
#define SWP_NOREDRAW        0x0008
#define SWP_NOACTIVATE      0x0010
#define SWP_FRAMECHANGED    0x0020
#define SWP_SHOWWINDOW      0x0040
#define SWP_HIDEWINDOW      0x0080
#define SWP_NOCOPYBITS      0x0100
#define SWP_NOOWNERZORDER   0x0200
#define SWP_NOSENDCHANGING  0x0400
#define SWP_DEFERERASE      0x2000
#define SWP_ASYNCWINDOWPOS  0x4000
};

FailOnFalse [gle] SetWindowPos(HWND              hWnd,
                               HWND              hWndInsertAfter,
                               int               x,
                               int               y,
                               int               cx,
                               int               cy,
                               SetWindowPosFlags uFlags);

BOOL ShowWindow(HWND hWnd, ShowWindowCommand nCmdShow);

BOOL ShowWindowAsync(HWND hWnd, ShowWindowCommand nCmdShow);

typedef struct tagWINDOWPLACEMENT {
    UINT  length;
    UINT  flags;
    UINT  showCmd;
    POINT ptMinPosition;
    POINT ptMaxPosition;
    RECT  rcNormalPosition;
} WINDOWPLACEMENT, *LPWINDOWPLACEMENT;


FailOnFalse [gle] SetWindowPlacement(HWND              hWnd,
                                     LPWINDOWPLACEMENT lpwndpl);

FailOnFalse [gle] GetWindowPlacement(HWND hWnd, [out] LPWINDOWPLACEMENT lpwndpl);


HDWP BeginDeferWindowPos(int nNumWindows);
HDWP DeferWindowPos(HDWP              hWinPosInfo,
                    HWND              hWnd,
                    HWND              hWndInsertAfter,
                    int               x,
                    int               y,
                    int               cx,
                    int               cy,
                    SetWindowPosFlags uFlags);

FailOnFalse [gle] EndDeferWindowPos( [da] HDWP hWinPosInfo);

FailOnFalse [gle] MoveWindow(HWND hWnd,
                             int  X,
                             int  Y,
                             int  nWidth,
                             int  nHeight,
                             BOOL bRepaint);

typedef struct tagPAINTSTRUCT {
    HDC         hdc;
    BOOL        fErase;
    RECT        rcPaint;
    BOOL        fRestore;
    BOOL        fIncUpdate;
    BYTE        rgbReserved[32];
} PAINTSTRUCT, *PPAINTSTRUCT, *LPPAINTSTRUCT;


HDC  BeginPaint(HWND hWnd, [out] LPPAINTSTRUCT lpPaint);
FailOnFalse EndPaint(HWND hWnd, LPPAINTSTRUCT lpPaint);

mask DWORD RedrawWindowFlags
{
#define RDW_INVALIDATE          0x0001
#define RDW_INTERNALPAINT       0x0002
#define RDW_ERASE               0x0004

#define RDW_VALIDATE            0x0008
#define RDW_NOINTERNALPAINT     0x0010
#define RDW_NOERASE             0x0020

#define RDW_NOCHILDREN          0x0040
#define RDW_ALLCHILDREN         0x0080

#define RDW_UPDATENOW           0x0100
#define RDW_ERASENOW            0x0200

#define RDW_FRAME               0x0400
#define RDW_NOFRAME             0x0800
};

FailOnFalse [gle] RedrawWindow(HWND              hWnd,
                               LPRECT            lprcUpdate,
                               HRGN              hrgnUpdate,
                               RedrawWindowFlags flags);

FailOnFalse [gle] PaintDesktop(HDC hdc);

HWND GetDesktopWindow();

HDC [gle] GetDC(HWND hWnd);

mask DWORD GetDCExFlags
{
#define DCX_WINDOW           0x00000001
#define DCX_CACHE            0x00000002
#define DCX_NORESETATTRS     0x00000004
#define DCX_CLIPCHILDREN     0x00000008
#define DCX_CLIPSIBLINGS     0x00000010
#define DCX_PARENTCLIP       0x00000020
#define DCX_EXCLUDERGN       0x00000040
#define DCX_INTERSECTRGN     0x00000080
#define DCX_EXCLUDEUPDATE    0x00000100
#define DCX_INTERSECTUPDATE  0x00000200
#define DCX_LOCKWINDOWUPDATE 0x00000400
#define DCX_VALIDATE         0x00200000
};

HDC [gle] GetDCEx(HWND hWnd, HRGN hrgnClip, GetDCExFlags flags);

HDC [gle] GetWindowDC(HWND hWnd);

FailOnFalse ReleaseDC(HWND hWnd, [da] HDC hDC);

FailOnFalse [gle] ScrollDC(HDC          hDC,
                           int          dx,
                           int          dy,
                           LPRECT       lprcScroll,
                           LPRECT       lprcClip,
                           HRGN         hrgnUpdate,
                           [out] LPRECT lprcUpdate);

FailOnFalse [gle] UpdateWindow(HWND hWnd);

FailOnFalse [gle] InvalidateRect(HWND hWnd, LPRECT lpRect, BOOL bErase);
FailOnFalse [gle] InvalidateRgn(HWND hWnd, HRGN hRgn, BOOL bErase);

FailOnFalse [gle] ValidateRect(HWND hWnd, LPRECT lpRect);
FailOnFalse [gle] ValidateRgn(HWND hWnd, HRGN hRgn);

HRGN ExcludeUpdateRgn(HDC hDC, HWND hWnd);

FailOnFalse [gle] GetUpdateRect(HWND         hWnd,
                                [out] LPRECT lpRect,
                                BOOL         bErase);

HRGN GetUpdateRgn(HWND hWnd, HRGN hRgn, BOOL bErase);


HWND WindowFromDC(HDC hDC);

HWND WindowFromPoint(long x, long y);

HWND ChildWindowFromPoint(HWND hWndParent, int x, int y);

mask DWORD ChildWindowFromPointExFlags
{
#define CWP_ALL             0x0000
#define CWP_SKIPINVISIBLE   0x0001
#define CWP_SKIPDISABLED    0x0002
#define CWP_SKIPTRANSPARENT 0x0004
};

HWND ChildWindowFromPointEx(HWND hwndParent, int x, int y, ChildWindowFromPointExFlags dwFlags);

HWND RealChildWindowFromPoint(HWND hwndParent, int x, int y);

// BUGBUG: need to use [in] [ou]
FailOnFalse [gle] ClientToScreen(HWND hWnd, [out] LPPOINT lpPoint);
FailOnFalse [gle] ScreenToClient(HWND hWnd, [out] LPPOINT lpPoint);


FailOnFalse [gle] ScrollWindow(HWND   hWnd,
                               int    XAmount,
                               int    YAmount,
                               LPRECT lpRect,
                               LPRECT lpClipRect);

mask DWORD ScrollWindowExFlags
{
#define SW_SCROLLCHILDREN   0x0001
#define SW_INVALIDATE       0x0002
#define SW_ERASE            0x0004
#define SW_SMOOTHSCROLL     0x0010
};

HRGN [gle] ScrollWindowEx(HWND                hWnd,
                          int                 dx,
                          int                 dy,
                          LPRECT              lprcScroll,
                          LPRECT              lprcClip,
                          HRGN                hrgnUpdate,
                          [out] LPRECT        lprcUpdate,
                          ScrollWindowExFlags flags);



LRESULT SendMessageA(HWND hWnd, WindowsMessage Msg, WPARAM wParam, LPARAM lParam);
LRESULT SendMessageW(HWND hWnd, WindowsMessage Msg, WPARAM wParam, LPARAM lParam);

LRESULT SendDlgItemMessageA(HWND hDlg, int nIDDlgItem, WindowsMessage Msg, WPARAM wParam, LPARAM lParam);
LRESULT SendDlgItemMessageW(HWND hDlg, int nIDDlgItem, WindowsMessage Msg, WPARAM wParam, LPARAM lParam);

FailOnFalse [gle] PostMessageA(HWND           hWnd,
                               WindowsMessage Msg,
                               WPARAM         wParam,
                               LPARAM         lParam);

FailOnFalse [gle] PostMessageW(HWND           hWnd,
                               WindowsMessage Msg,
                               WPARAM         wParam,
                               LPARAM         lParam);

VOID PostQuitMessage(int nExitCode);

FailOnFalse [gle] PostThreadMessageA(ThreadId       idThread,
                                     WindowsMessage Msg,
                                     WPARAM         wParam,
                                     LPARAM         lParam);

FailOnFalse [gle] PostThreadMessageW(ThreadId       idThread,
                                     WindowsMessage Msg,
                                     WPARAM         wParam,
                                     LPARAM         lParam);

BOOL TranslateMessage(MSG* lpMsg);


typedef DWORD SENDASYNCPROC;

alias SENDASYNCPROC;

FailOnFalse [gle] SendMessageCallbackA(HWND           hWnd,
                                       WindowsMessage Msg,
                                       WPARAM         wParam,
                                       LPARAM         lParam,
                                       SENDASYNCPROC  lpResultCallBack,
                                       ULONG_PTR      dwData);

FailOnFalse [gle] SendMessageCallbackW(HWND           hWnd,
                                       WindowsMessage Msg,
                                       WPARAM         wParam,
                                       LPARAM         lParam,
                                       SENDASYNCPROC  lpResultCallBack,
                                       ULONG_PTR      dwData);

mask DWORD SendMessageTimeoutFlags
{
#define SMTO_NORMAL             0x0000
#define SMTO_BLOCK              0x0001
#define SMTO_ABORTIFHUNG        0x0002
#define SMTO_NOTIMEOUTIFNOTHUNG 0x0008
};

LRESULT SendMessageTimeoutA(HWND                    hWnd,
                            WindowsMessage          Msg,
                            WPARAM                  wParam,
                            LPARAM                  lParam,
                            SendMessageTimeoutFlags fuFlags,
                            UINT                    uTimeout,
                            [out] LPDWORD           lpdwResult);

LRESULT SendMessageTimeoutW(HWND                    hWnd,
                            WindowsMessage          Msg,
                            WPARAM                  wParam,
                            LPARAM                  lParam,
                            SendMessageTimeoutFlags fuFlags,
                            UINT                    uTimeout,
                            [out] LPDWORD           lpdwResult);

FailOnFalse [gle] SendNotifyMessageA(HWND           hWnd,
                                     WindowsMessage Msg,
                                     WPARAM         wParam,
                                     LPARAM         lParam);

FailOnFalse [gle] SendNotifyMessageW(HWND           hWnd,
                                     WindowsMessage Msg,
                                     WPARAM         wParam,
                                     LPARAM         lParam);


BOOL InSendMessage();

mask DWORD InSendMessageExFlags
{
#define ISMEX_NOSEND      0x00000000
#define ISMEX_SEND        0x00000001
#define ISMEX_NOTIFY      0x00000002
#define ISMEX_CALLBACK    0x00000004
#define ISMEX_REPLIED     0x00000008
};

InSendMessageExFlags InSendMessageEx(LPVOID lpReserved);


mask DWORD BroadcastSystemMessageFlags
{
#define BSF_QUERY               0x00000001
#define BSF_IGNORECURRENTTASK   0x00000002
#define BSF_FLUSHDISK           0x00000004
#define BSF_NOHANG              0x00000008
#define BSF_POSTMESSAGE         0x00000010
#define BSF_FORCEIFHUNG         0x00000020
#define BSF_NOTIMEOUTIFNOTHUNG  0x00000040
#define BSF_ALLOWSFW            0x00000080
#define BSF_SENDNOTIFYMESSAGE   0x00000100
#define BSF_RETURNHDESK         0x00000200
#define BSF_LUID                0x00000400
};

mask DWORD BroadcastSystemMessageRecipients
{
#define BSM_ALLCOMPONENTS       0x00000000
#define BSM_VXDS                0x00000001
#define BSM_NETDRIVER           0x00000002
#define BSM_INSTALLABLEDRIVERS  0x00000004
#define BSM_APPLICATIONS        0x00000008
#define BSM_ALLDESKTOPS         0x00000010
};

LongFailIfNeg1 [gle] BroadcastSystemMessageA(BroadcastSystemMessageFlags             Flags,
                                             [out] BroadcastSystemMessageRecipients* lpRecipients,
                                             WindowsMessage                          Msg,
                                             WPARAM                                  wParam,
                                             LPARAM                                  lParam);

LongFailIfNeg1 [gle] BroadcastSystemMessageW(BroadcastSystemMessageFlags             Flags,
                                             [out] BroadcastSystemMessageRecipients* lpRecipients,
                                             WindowsMessage                          Msg,
                                             WPARAM                                  wParam,
                                             LPARAM                                  lParam);

LRESULT DispatchMessageA(MSG* lpMsg);
LRESULT DispatchMessageW(MSG* lpMsg);

LongFailIfNeg1 [gle] GetMessageA( [out] LPMSG   lpMsg,
                                 HWND           hWnd,
                                 WindowsMessage wMsgFilterMin,
                                 WindowsMessage wMsgFilterMax);

LongFailIfNeg1 [gle] GetMessageW( [out] LPMSG   lpMsg,
                                 HWND           hWnd,
                                 WindowsMessage wMsgFilterMin,
                                 WindowsMessage wMsgFilterMax);

FailOnFalse [gle] WaitMessage();

BOOL ReplyMessage(LRESULT lResult);

mask DWORD PeekMessageFlags
{
#define PM_NOREMOVE         0x0000
#define PM_REMOVE           0x0001
#define PM_NOYIELD          0x0002
};

BOOL PeekMessageA( [out] LPMSG      lpMsg,
                  HWND              hWnd,
                  WindowsMessage    wMsgFilterMin,
                  WindowsMessage    wMsgFilterMax,
                  PeekMessageFlags  wRemoveMsg);

BOOL PeekMessageW( [out] LPMSG      lpMsg,
                  HWND              hWnd,
                  WindowsMessage    wMsgFilterMin,
                  WindowsMessage    wMsgFilterMax,
                  PeekMessageFlags  wRemoveMsg);


LPARAM GetMessageExtraInfo();
DWORD  GetMessagePos();
LONG   GetMessageTime();

LRESULT DefWindowProcA(HWND           hWnd,
                       WindowsMessage Msg,
                       WPARAM         wParam,
                       LPARAM         lParam);

LRESULT DefWindowProcW(HWND           hWnd,
                       WindowsMessage Msg,
                       WPARAM         wParam,
                       LPARAM         lParam);

LRESULT DefDlgProcA(HWND           hDlg,
                    WindowsMessage Msg,
                    WPARAM         wParam,
                    LPARAM         lParam);

LRESULT DefDlgProcW(HWND           hDlg,
                    WindowsMessage Msg,
                    WPARAM         wParam,
                    LPARAM         lParam);

LRESULT DefFrameProcA(HWND           hWnd,
                      HWND           hWndMDIClient,
                      WindowsMessage uMsg,
                      WPARAM         wParam,
                      LPARAM         lParam);

LRESULT DefFrameProcW(HWND           hWnd,
                      HWND           hWndMDIClient,
                      WindowsMessage uMsg,
                      WPARAM         wParam,
                      LPARAM         lParam);


LRESULT CallWindowProcA(WNDPROC lpPrevWndFunc, HWND hWnd, WindowsMessage Msg, WPARAM wParam, LPARAM lParam);
LRESULT CallWindowProcW(WNDPROC lpPrevWndFunc, HWND hWnd, WindowsMessage Msg, WPARAM wParam, LPARAM lParam);



value DWORD WindowLongIndex
{
#define GWL_WNDPROC         -4
#define GWL_HINSTANCE       -6
#define GWL_HWNDPARENT      -8
#define GWL_STYLE           -16
#define GWL_EXSTYLE         -20
#define GWL_USERDATA        -21
#define GWL_ID              -12
#define DWL_MSGRESULT       0
#define DWL_DLGPROC         4
#define DWL_USER            8
};

DwordFailIfZero [gle] GetWindowLongA(HWND hWnd, WindowLongIndex nIndex);
DwordFailIfZero [gle] GetWindowLongW(HWND hWnd, WindowLongIndex nIndex);

DwordFailIfZero [gle] SetWindowLongA(HWND hWnd, WindowLongIndex nIndex, DWORD dwNewLong);
DwordFailIfZero [gle] SetWindowLongW(HWND hWnd, WindowLongIndex nIndex, DWORD dwNewLong);

DwordFailIfZero [gle] GetWindowWord(HWND hWnd, int nIndex);
DwordFailIfZero [gle] SetWindowWord(HWND hWnd, int nIndex, DWORD wNewWord);


LongFailIfZero [gle] SetWindowTextA(HWND hWnd, LPCSTR lpString);
LongFailIfZero [gle] SetWindowTextW(HWND hWnd, LPCWSTR lpString);

LongFailIfZero [gle] GetWindowTextA(HWND hWnd, [out] LPSTR lpString, int nMaxCount);
LongFailIfZero [gle] GetWindowTextW(HWND hWnd, [out] LPWSTR lpString, int nMaxCount);

LongFailIfZero [gle] GetWindowTextLengthA(HWND hWnd);
LongFailIfZero [gle] GetWindowTextLengthW(HWND hWnd);

LongFailIfZero [gle] GetDlgItemTextA(HWND hDlg, int nIDDlgItem, [out] LPSTR lpString, int nMaxCount);
LongFailIfZero [gle] GetDlgItemTextW(HWND hDlg, int nIDDlgItem, [out] LPWSTR lpString, int nMaxCount);

FailOnFalse [gle] SetDlgItemInt(HWND hDlg, int nIDDlgItem, UINT uValue, BOOL bSigned);

FailOnFalse [gle] SetDlgItemTextA(HWND hDlg, int nIDDlgItem, LPCSTR lpString);
FailOnFalse [gle] SetDlgItemTextW(HWND hDlg, int nIDDlgItem, LPCWSTR lpString);

HWND [gle] GetNextDlgGroupItem(HWND hDlg, HWND hCtl, BOOL bPrevious);
HWND [gle] GetNextDlgTabItem(HWND hDlg, HWND hCtl, BOOL bPrevious);

mask DWORD DlgDirListType
{
#define DDL_READWRITE       0x0000
#define DDL_READONLY        0x0001
#define DDL_HIDDEN          0x0002
#define DDL_SYSTEM          0x0004
#define DDL_DIRECTORY       0x0010
#define DDL_ARCHIVE         0x0020

#define DDL_POSTMSGS        0x2000
#define DDL_DRIVES          0x4000
#define DDL_EXCLUSIVE       0x8000
};

LongFailIfZero [gle] DlgDirListA(HWND           hDlg,
                                 [out] LPSTR    lpPathSpec,
                                 int            nIDListBox,
                                 int            nIDStaticPath,
                                 DlgDirListType uFileType);

LongFailIfZero [gle] DlgDirListW(HWND           hDlg,
                                 [out] LPWSTR   lpPathSpec,
                                 int            nIDListBox,
                                 int            nIDStaticPath,
                                 DlgDirListType uFileType);

BOOL DlgDirSelectExA(HWND        hDlg,
                     [out] LPSTR lpString,
                     int         nCount,
                     int         nIDListBox);

BOOL DlgDirSelectExW(HWND         hDlg,
                     [out] LPWSTR lpString,
                     int          nCount,
                     int          nIDListBox);

LongFailIfZero [gle] DlgDirListComboBoxA(HWND           hDlg,
                                         [out] LPSTR    lpPathSpec,
                                         int            nIDComboBox,
                                         int            nIDStaticPath,
                                         DlgDirListType uFiletype);

LongFailIfZero [gle] DlgDirListComboBoxW(HWND           hDlg,
                                         [out] LPWSTR   lpPathSpec,
                                         int            nIDComboBox,
                                         int            nIDStaticPath,
                                         DlgDirListType uFiletype);

BOOL DlgDirSelectComboBoxExA(HWND        hDlg,
                             [out] LPSTR lpString,
                             int         nCount,
                             int         nIDComboBox);

BOOL DlgDirSelectComboBoxExW(HWND         hDlg,
                             [out] LPWSTR lpString,
                             int          nCount,
                             int          nIDComboBox);


value DWORD ChangeDisplaySettingsRet
{
#define DISP_CHANGE_SUCCESSFUL       0
#define DISP_CHANGE_RESTART          1 [fail]
#define DISP_CHANGE_FAILED          -1 [fail]
#define DISP_CHANGE_BADMODE         -2 [fail]
#define DISP_CHANGE_NOTUPDATED      -3 [fail]
#define DISP_CHANGE_BADFLAGS        -4 [fail]
#define DISP_CHANGE_BADPARAM        -5 [fail]
};


mask DWORD ChangeDisplaySettingsFlags
{
#define CDS_UPDATEREGISTRY  0x00000001
#define CDS_TEST            0x00000002
#define CDS_FULLSCREEN      0x00000004
#define CDS_GLOBAL          0x00000008
#define CDS_SET_PRIMARY     0x00000010
#define CDS_VIDEOPARAMETERS 0x00000020
#define CDS_RESET           0x40000000
#define CDS_NORESET         0x10000000
};

ChangeDisplaySettingsRet ChangeDisplaySettingsA(LPDEVMODEA lpDevMode, ChangeDisplaySettingsFlags dwFlags);
ChangeDisplaySettingsRet ChangeDisplaySettingsW(LPDEVMODEW lpDevMode, ChangeDisplaySettingsFlags dwFlags);

ChangeDisplaySettingsRet ChangeDisplaySettingsExA(LPCSTR                     lpszDeviceName,
                                                  LPDEVMODEA                 lpDevMode,
                                                  HWND                       hwnd,
                                                  ChangeDisplaySettingsFlags dwflags,
                                                  LPVOID                     lParam);

ChangeDisplaySettingsRet ChangeDisplaySettingsExW(LPCWSTR                    lpszDeviceName,
                                                  LPDEVMODEW                 lpDevMode,
                                                  HWND                       hwnd,
                                                  ChangeDisplaySettingsFlags dwflags,
                                                  LPVOID                     lParam);

typedef struct _DISPLAY_DEVICEA {
    DWORD  cb;
    char   DeviceName[32];
    char   DeviceString[128];
    DWORD  StateFlags;
    char   DeviceID[128];
    char   DeviceKey[128];
} DISPLAY_DEVICEA, *PDISPLAY_DEVICEA, *LPDISPLAY_DEVICEA;

typedef struct _DISPLAY_DEVICEW {
    DWORD  cb;
    WCHAR  DeviceName[32];
    WCHAR  DeviceString[128];
    DWORD  StateFlags;
    WCHAR  DeviceID[128];
    WCHAR  DeviceKey[128];
} DISPLAY_DEVICEW, *PDISPLAY_DEVICEW, *LPDISPLAY_DEVICEW;


LongFailIfZero EnumDisplayDevicesA(LPCSTR                 lpDevice,
                                   DWORD                  iDevNum,
                                   [out] PDISPLAY_DEVICEA lpDisplayDevice,
                                   DWORD                  dwFlags);

LongFailIfZero EnumDisplayDevicesW(LPCWSTR                lpDevice,
                                   DWORD                  iDevNum,
                                   [out] PDISPLAY_DEVICEW lpDisplayDevice,
                                   DWORD                  dwFlags);

typedef LPVOID MONITORENUMPROC;

FailOnFalse [gle] EnumDisplayMonitors(HDC             hdc,
                                      LPRECT          lprcClip,
                                      MONITORENUMPROC lpfnEnum,
                                      LPARAM          dwData);

typedef struct tagMONITORINFO
{
    DWORD   cbSize;
    RECT    rcMonitor;
    RECT    rcWork;
    DWORD   dwFlags;
} MONITORINFO, *LPMONITORINFO;

FailOnFalse [gle] GetMonitorInfoA(HMONITOR hMonitor, [out] LPMONITORINFO lpmi);
FailOnFalse [gle] GetMonitorInfoW(HMONITOR hMonitor, [out] LPMONITORINFO lpmi);

mask DWORD MonitorFlags
{
#define MONITOR_DEFAULTTONULL       0x00000000
#define MONITOR_DEFAULTTOPRIMARY    0x00000001
#define MONITOR_DEFAULTTONEAREST    0x00000002
};

HMONITOR MonitorFromPoint(int x, int y, MonitorFlags dwFlags);
HMONITOR MonitorFromRect(LPRECT lprc, MonitorFlags dwFlags);
HMONITOR MonitorFromWindow(HWND hwnd, MonitorFlags dwFlags);


value DWORD EnumDisplaySettingsIndex
{
#define ENUM_CURRENT_SETTINGS       -1
#define ENUM_REGISTRY_SETTINGS      -2
};

FailOnFalse [gle] EnumDisplaySettingsA(LPCSTR                   lpszDeviceName,
                                       EnumDisplaySettingsIndex iModeNum,
                                       [out] LPDEVMODEA         lpDevMode);

FailOnFalse [gle] EnumDisplaySettingsW(LPCWSTR                  lpszDeviceName,
                                       EnumDisplaySettingsIndex iModeNum,
                                       [out] LPDEVMODEW         lpDevMode);

FailOnFalse [gle] EnumDisplaySettingsExA(LPCSTR                   lpszDeviceName,
                                         EnumDisplaySettingsIndex iModeNum,
                                         [out] LPDEVMODEA         lpDevMode,
                                         DWORD                    dwFlags);

FailOnFalse [gle] EnumDisplaySettingsExW(LPCWSTR                  lpszDeviceName,
                                         EnumDisplaySettingsIndex iModeNum,
                                         [out] LPDEVMODEW         lpDevMode,
                                         DWORD                    dwFlags);



HWND [gle] SetFocus(HWND hWnd);
HWND GetFocus();

FailOnFalse [gle] DrawFocusRect(HDC hDC, LPRECT* lprc);

mask DWORD GetGUIThreadInfoFlags
{
#define GUI_CARETBLINKING   0x00000001
#define GUI_INMOVESIZE      0x00000002
#define GUI_INMENUMODE      0x00000004
#define GUI_SYSTEMMENUMODE  0x00000008
#define GUI_POPUPMENUMODE   0x00000010
};

typedef struct tagGUITHREADINFO
{
    DWORD                 cbSize;
    GetGUIThreadInfoFlags flags;
    HWND                  hwndActive;
    HWND                  hwndFocus;
    HWND                  hwndCapture;
    HWND                  hwndMenuOwner;
    HWND                  hwndMoveSize;
    HWND                  hwndCaret;
    RECT                  rcCaret;
} GUITHREADINFO, *PGUITHREADINFO, *LPGUITHREADINFO;

FailOnFalse [gle] GetGUIThreadInfo(ThreadId idThread, [out] PGUITHREADINFO pgui);

HWND SetCapture(HWND hWnd);
HWND GetCapture();

FailOnFalse [gle] ReleaseCapture();


HRSRC SetCursor(HRSRC hCursor);

value LPSTR CursorValueA
{
#define IDC_ARROW           32512
#define IDC_IBEAM           32513
#define IDC_WAIT            32514
#define IDC_CROSS           32515
#define IDC_UPARROW         32516
#define IDC_SIZE            32640
#define IDC_ICON            32641
#define IDC_SIZENWSE        32642
#define IDC_SIZENESW        32643
#define IDC_SIZEWE          32644
#define IDC_SIZENS          32645
#define IDC_SIZEALL         32646
#define IDC_NO              32648
#define IDC_HAND            32649
#define IDC_APPSTARTING     32650
#define IDC_HELP            32651
};

value LPWSTR CursorValueW
{
#define IDC_ARROW           32512
#define IDC_IBEAM           32513
#define IDC_WAIT            32514
#define IDC_CROSS           32515
#define IDC_UPARROW         32516
#define IDC_SIZE            32640
#define IDC_ICON            32641
#define IDC_SIZENWSE        32642
#define IDC_SIZENESW        32643
#define IDC_SIZEWE          32644
#define IDC_SIZENS          32645
#define IDC_SIZEALL         32646
#define IDC_NO              32648
#define IDC_HAND            32649
#define IDC_APPSTARTING     32650
#define IDC_HELP            32651
};

HRSRC [gle] LoadCursorA(HINSTANCE hInstance, CursorValueA lpCursorName);
HRSRC [gle] LoadCursorW(HINSTANCE hInstance, CursorValueW lpCursorName);

value LPSTR IconValueA
{
#define IDI_APPLICATION     32512
#define IDI_HAND            32513
#define IDI_QUESTION        32514
#define IDI_EXCLAMATION     32515
#define IDI_ASTERISK        32516
#define IDI_WINLOGO         32517
};

value LPWSTR IconValueW
{
#define IDI_APPLICATION     32512
#define IDI_HAND            32513
#define IDI_QUESTION        32514
#define IDI_EXCLAMATION     32515
#define IDI_ASTERISK        32516
#define IDI_WINLOGO         32517
};

HRSRC [gle] LoadIconA(HINSTANCE hInstance, IconValueA lpIconName);
HRSRC [gle] LoadIconW(HINSTANCE hInstance, IconValueW lpIconName);

mask DWORD ImageType
{
#define IMAGE_BITMAP        0
#define IMAGE_ICON          1
#define IMAGE_CURSOR        2
#define IMAGE_ENHMETAFILE   3
};

mask DWORD LoadImageFlags
{
#define LR_DEFAULTCOLOR     0x0000
#define LR_MONOCHROME       0x0001
#define LR_COLOR            0x0002
#define LR_COPYRETURNORG    0x0004
#define LR_COPYDELETEORG    0x0008
#define LR_LOADFROMFILE     0x0010
#define LR_LOADTRANSPARENT  0x0020
#define LR_DEFAULTSIZE      0x0040
#define LR_VGACOLOR         0x0080
#define LR_LOADMAP3DCOLORS  0x1000
#define LR_CREATEDIBSECTION 0x2000
#define LR_COPYFROMRESOURCE 0x4000
#define LR_SHARED           0x8000
};

value LPSTR ImageValueA
{
#define OBM_CLOSE           32754
#define OBM_UPARROW         32753
#define OBM_DNARROW         32752
#define OBM_RGARROW         32751
#define OBM_LFARROW         32750
#define OBM_REDUCE          32749
#define OBM_ZOOM            32748
#define OBM_RESTORE         32747
#define OBM_REDUCED         32746
#define OBM_ZOOMD           32745
#define OBM_RESTORED        32744
#define OBM_UPARROWD        32743
#define OBM_DNARROWD        32742
#define OBM_RGARROWD        32741
#define OBM_LFARROWD        32740
#define OBM_MNARROW         32739
#define OBM_COMBO           32738
#define OBM_UPARROWI        32737
#define OBM_DNARROWI        32736
#define OBM_RGARROWI        32735
#define OBM_LFARROWI        32734

#define OBM_OLD_CLOSE       32767
#define OBM_SIZE            32766
#define OBM_OLD_UPARROW     32765
#define OBM_OLD_DNARROW     32764
#define OBM_OLD_RGARROW     32763
#define OBM_OLD_LFARROW     32762
#define OBM_BTSIZE          32761
#define OBM_CHECK           32760
#define OBM_CHECKBOXES      32759
#define OBM_BTNCORNERS      32758
#define OBM_OLD_REDUCE      32757
#define OBM_OLD_ZOOM        32756
#define OBM_OLD_RESTORE     32755

#define OCR_NORMAL          32512
#define OCR_IBEAM           32513
#define OCR_WAIT            32514
#define OCR_CROSS           32515
#define OCR_UP              32516
#define OCR_SIZE            32640
#define OCR_ICON            32641
#define OCR_SIZENWSE        32642
#define OCR_SIZENESW        32643
#define OCR_SIZEWE          32644
#define OCR_SIZENS          32645
#define OCR_SIZEALL         32646
#define OCR_ICOCUR          32647
#define OCR_NO              32648
#define OCR_HAND            32649
#define OCR_APPSTARTING     32650

#define OIC_WINLOGO         32517
};

value LPWSTR ImageValueW
{
#define OBM_CLOSE           32754
#define OBM_UPARROW         32753
#define OBM_DNARROW         32752
#define OBM_RGARROW         32751
#define OBM_LFARROW         32750
#define OBM_REDUCE          32749
#define OBM_ZOOM            32748
#define OBM_RESTORE         32747
#define OBM_REDUCED         32746
#define OBM_ZOOMD           32745
#define OBM_RESTORED        32744
#define OBM_UPARROWD        32743
#define OBM_DNARROWD        32742
#define OBM_RGARROWD        32741
#define OBM_LFARROWD        32740
#define OBM_MNARROW         32739
#define OBM_COMBO           32738
#define OBM_UPARROWI        32737
#define OBM_DNARROWI        32736
#define OBM_RGARROWI        32735
#define OBM_LFARROWI        32734

#define OBM_OLD_CLOSE       32767
#define OBM_SIZE            32766
#define OBM_OLD_UPARROW     32765
#define OBM_OLD_DNARROW     32764
#define OBM_OLD_RGARROW     32763
#define OBM_OLD_LFARROW     32762
#define OBM_BTSIZE          32761
#define OBM_CHECK           32760
#define OBM_CHECKBOXES      32759
#define OBM_BTNCORNERS      32758
#define OBM_OLD_REDUCE      32757
#define OBM_OLD_ZOOM        32756
#define OBM_OLD_RESTORE     32755

#define OCR_NORMAL          32512
#define OCR_IBEAM           32513
#define OCR_WAIT            32514
#define OCR_CROSS           32515
#define OCR_UP              32516
#define OCR_SIZE            32640
#define OCR_ICON            32641
#define OCR_SIZENWSE        32642
#define OCR_SIZENESW        32643
#define OCR_SIZEWE          32644
#define OCR_SIZENS          32645
#define OCR_SIZEALL         32646
#define OCR_ICOCUR          32647
#define OCR_NO              32648
#define OCR_HAND            32649
#define OCR_APPSTARTING     32650

#define OIC_WINLOGO         32517
};

HRSRC [gle] LoadImageA(HINSTANCE      hInstance,
                       ImageValueA    lpszName,
                       ImageType      uType,
                       int            cxDesired,
                       int            cyDesired,
                       LoadImageFlags fuLoad);

HRSRC [gle] LoadImageW(HINSTANCE      hInstance,
                       ImageValueW    lpszName,
                       ImageType      uType,
                       int            cxDesired,
                       int            cyDesired,
                       LoadImageFlags fuLoad);

HRSRC [gle] CopyImage(HRSRC          hImage,
                      ImageType      uType,
                      int            x,
                      int            y,
                      LoadImageFlags fuFlags);

HRSRC [gle] CopyIcon(HRSRC hIcon);

typedef struct _ICONINFO {
    BOOL    fIcon;
    DWORD   xHotspot;
    DWORD   yHotspot;
    HRSRC   hbmMask;
    HRSRC   hbmColor;
} ICONINFO;
typedef ICONINFO *PICONINFO;

FailOnFalse [gle] GetIconInfo(HRSRC hIcon, [out] PICONINFO piconinfo);

HRSRC [gle] CreateIcon(HINSTANCE  hInstance,
                       int        nWidth,
                       int        nHeight,
                       BYTE       cPlanes,
                       BYTE       cBitsPixel,
                       BYTE*      lpbANDbits,
                       BYTE*      lpbXORbits);

FailOnFalse [gle] OpenIcon(HWND hWnd);

HRSRC [gle] CreateIconFromResource(PBYTE presbits,
                                   DWORD dwResSize,
                                   BOOL  fIcon,
                                   DWORD dwVer);

HRSRC [gle] CreateIconFromResourceEx(PBYTE          presbits,
                                     DWORD          dwResSize,
                                     BOOL           fIcon,
                                     DWORD          dwVer,
                                     int            cxDesired,
                                     int            cyDesired,
                                     LoadImageFlags Flags);

HRSRC [gle]  CreateIconIndirect(PICONINFO piconinfo);

DwordFailIfZero [gle] LookupIconIdFromDirectory(PBYTE presbits, BOOL fIcon);

DwordFailIfZero [gle] LookupIconIdFromDirectoryEx(PBYTE          presbits,
                                                  BOOL           fIcon,
                                                  int            cxDesired,
                                                  int            cyDesired,
                                                  LoadImageFlags Flags);

FailOnFalse [gle] DestroyCursor( [da] HRSRC hCursor);
FailOnFalse [gle] DestroyIcon( [da] HRSRC hIcon);

value DWORD SystemMetric
{
#define SM_CXSCREEN             0
#define SM_CYSCREEN             1
#define SM_CXVSCROLL            2
#define SM_CYHSCROLL            3
#define SM_CYCAPTION            4
#define SM_CXBORDER             5
#define SM_CYBORDER             6
#define SM_CXDLGFRAME           7
#define SM_CYDLGFRAME           8
#define SM_CYVTHUMB             9
#define SM_CXHTHUMB             10
#define SM_CXICON               11
#define SM_CYICON               12
#define SM_CXCURSOR             13
#define SM_CYCURSOR             14
#define SM_CYMENU               15
#define SM_CXFULLSCREEN         16
#define SM_CYFULLSCREEN         17
#define SM_CYKANJIWINDOW        18
#define SM_MOUSEPRESENT         19
#define SM_CYVSCROLL            20
#define SM_CXHSCROLL            21
#define SM_DEBUG                22
#define SM_SWAPBUTTON           23
#define SM_RESERVED1            24
#define SM_RESERVED2            25
#define SM_RESERVED3            26
#define SM_RESERVED4            27
#define SM_CXMIN                28
#define SM_CYMIN                29
#define SM_CXSIZE               30
#define SM_CYSIZE               31
#define SM_CXFRAME              32
#define SM_CYFRAME              33
#define SM_CXMINTRACK           34
#define SM_CYMINTRACK           35
#define SM_CXDOUBLECLK          36
#define SM_CYDOUBLECLK          37
#define SM_CXICONSPACING        38
#define SM_CYICONSPACING        39
#define SM_MENUDROPALIGNMENT    40
#define SM_PENWINDOWS           41
#define SM_DBCSENABLED          42
#define SM_CMOUSEBUTTONS        43

#define SM_SECURE               44
#define SM_CXEDGE               45
#define SM_CYEDGE               46
#define SM_CXMINSPACING         47
#define SM_CYMINSPACING         48
#define SM_CXSMICON             49
#define SM_CYSMICON             50
#define SM_CYSMCAPTION          51
#define SM_CXSMSIZE             52
#define SM_CYSMSIZE             53
#define SM_CXMENUSIZE           54
#define SM_CYMENUSIZE           55
#define SM_ARRANGE              56
#define SM_CXMINIMIZED          57
#define SM_CYMINIMIZED          58
#define SM_CXMAXTRACK           59
#define SM_CYMAXTRACK           60
#define SM_CXMAXIMIZED          61
#define SM_CYMAXIMIZED          62
#define SM_NETWORK              63
#define SM_CLEANBOOT            67
#define SM_CXDRAG               68
#define SM_CYDRAG               69
#define SM_SHOWSOUNDS           70
#define SM_CXMENUCHECK          71
#define SM_CYMENUCHECK          72
#define SM_SLOWMACHINE          73
#define SM_MIDEASTENABLED       74

#define SM_MOUSEWHEELPRESENT    75
#define SM_XVIRTUALSCREEN       76
#define SM_YVIRTUALSCREEN       77
#define SM_CXVIRTUALSCREEN      78
#define SM_CYVIRTUALSCREEN      79
#define SM_CMONITORS            80
#define SM_SAMEDISPLAYFORMAT    81
#define SM_IMMENABLED           82
#define SM_CXFOCUSBORDER        83
#define SM_CYFOCUSBORDER        84

#define SM_REMOTESESSION        0x1000
};

LongFailIfZero GetSystemMetrics(SystemMetric nIndex);

HACCEL [gle] LoadAcceleratorsA(HINSTANCE hInstance, LPCSTR lpTableName);
HACCEL [gle] LoadAcceleratorsW(HINSTANCE hInstance, LPCWSTR lpTableName);

FailOnFalse [gle] DestroyAcceleratorTable( [da] HACCEL hAccel);

int TranslateAcceleratorA(HWND hWnd, HACCEL hAccTable, LPMSG lpMsg);
int TranslateAcceleratorW(HWND hWnd, HACCEL hAccTable, LPMSG lpMsg);

mask DWORD ClassStyles
{
#define CS_VREDRAW          0x0001
#define CS_HREDRAW          0x0002
#define CS_DBLCLKS          0x0008
#define CS_OWNDC            0x0020
#define CS_CLASSDC          0x0040
#define CS_PARENTDC         0x0080
#define CS_NOCLOSE          0x0200
#define CS_SAVEBITS         0x0800
#define CS_BYTEALIGNCLIENT  0x1000
#define CS_BYTEALIGNWINDOW  0x2000
#define CS_GLOBALCLASS      0x4000
#define CS_IME              0x00010000
#define CS_DROPSHADOW       0x00020000
};

typedef struct tagWNDCLASSA {
    ClassStyles style;
    FILLER64    align1[4];
    WNDPROC     lpfnWndProc;
    int         cbClsExtra;
    int         cbWndExtra;
    HINSTANCE   hInstance;
    HRSRC       hIcon;
    HRSRC       hCursor;
    HRSRC       hbrBackground;
    LPCSTR      lpszMenuName;
    LPCSTR      lpszClassName;
} WNDCLASSA, *PWNDCLASSA, *LPWNDCLASSA;

typedef struct tagWNDCLASSW {
    ClassStyles style;
    FILLER64    align1[4];
    WNDPROC     lpfnWndProc;
    int         cbClsExtra;
    int         cbWndExtra;
    HINSTANCE   hInstance;
    HRSRC       hIcon;
    HRSRC       hCursor;
    HRSRC       hbrBackground;
    LPCWSTR     lpszMenuName;
    LPCWSTR     lpszClassName;
} WNDCLASSW, *PWNDCLASSW, *LPWNDCLASSW;


DwordFailIfZero [gle] RegisterClassA(WNDCLASSA* lpWndClass);
DwordFailIfZero [gle] RegisterClassW(WNDCLASSW* lpWndClass);

FailOnFalse [gle] UnregisterClassA(LPCSTR lpClassName, HINSTANCE hInstance);
FailOnFalse [gle] UnregisterClassW(LPCWSTR lpClassName, HINSTANCE hInstance);

typedef struct tagWNDCLASSEXA {
    UINT        cbSize;
    ClassStyles style;
    WNDPROC     lpfnWndProc;
    int         cbClsExtra;
    int         cbWndExtra;
    HINSTANCE   hInstance;
    HRSRC       hIcon;
    HRSRC       hCursor;
    HRSRC       hbrBackground;
    LPCSTR      lpszMenuName;
    LPCSTR      lpszClassName;
    HRSRC       hIconSm;
} WNDCLASSEXA, *PWNDCLASSEXA, *LPWNDCLASSEXA;

typedef struct tagWNDCLASSEXW {
    UINT        cbSize;
    ClassStyles style;
    WNDPROC     lpfnWndProc;
    int         cbClsExtra;
    int         cbWndExtra;
    HINSTANCE   hInstance;
    HRSRC       hIcon;
    HRSRC       hCursor;
    HRSRC       hbrBackground;
    LPCWSTR     lpszMenuName;
    LPCWSTR     lpszClassName;
    HRSRC       hIconSm;
} WNDCLASSEXW, *PWNDCLASSEXW, *LPWNDCLASSEXW;

DwordFailIfZero [gle] RegisterClassExA(WNDCLASSEXA* lpWndClass);
DwordFailIfZero [gle] RegisterClassExW(WNDCLASSEXW* lpWndClass);

typedef struct tagDLGTEMPLATE {
    DWORD style;
    DWORD dwExtendedStyle;
    WORD  cdit;
    DWORD x_y;
    DWORD cx_cy;
} DLGTEMPLATE;

typedef DLGTEMPLATE *LPDLGTEMPLATEA;
typedef DLGTEMPLATE *LPDLGTEMPLATEW;

HWND [gle] CreateDialogIndirectParamA(HINSTANCE      hInstance,
                                      LPDLGTEMPLATEA lpTemplate,
                                      HWND           hWndParent,
                                      WNDPROC        lpDialogFunc,
                                      LPARAM         dwInitParam);

HWND [gle] CreateDialogIndirectParamW(HINSTANCE      hInstance,
                                      LPDLGTEMPLATEA lpTemplate,
                                      HWND           hWndParent,
                                      WNDPROC        lpDialogFunc,
                                      LPARAM         dwInitParam);

HWND [gle] CreateDialogParamA(HINSTANCE hInstance,
                              LPCSTR    lpTemplateName,
                              HWND      hWndParent,
                              WNDPROC   lpDialogFunc,
                              LPARAM    dwInitParam);

HWND [gle] CreateDialogParamW(HINSTANCE hInstance,
                              LPCWSTR   lpTemplateName,
                              HWND      hWndParent,
                              WNDPROC   lpDialogFunc,
                              LPARAM    dwInitParam);

LongFailIfNeg1 [gle] DialogBoxIndirectParamA(HINSTANCE      hInstance,
                                             LPDLGTEMPLATEA hDialogTemplate,
                                             HWND           hWndParent,
                                             WNDPROC        lpDialogFunc,
                                             LPARAM         dwInitParam);

LongFailIfNeg1 [gle] DialogBoxIndirectParamW(HINSTANCE      hInstance,
                                             LPDLGTEMPLATEW hDialogTemplate,
                                             HWND           hWndParent,
                                             WNDPROC        lpDialogFunc,
                                             LPARAM         dwInitParam);

LongFailIfNeg1 [gle] DialogBoxParamA(HINSTANCE hInstance,
                                     LPCSTR    lpTemplateName,
                                     HWND      hWndParent,
                                     WNDPROC   lpDialogFunc,
                                     LPARAM    dwInitParam);

LongFailIfNeg1 [gle] DialogBoxParamW(HINSTANCE hInstance,
                                     LPCWSTR   lpTemplateName,
                                     HWND      hWndParent,
                                     WNDPROC   lpDialogFunc,
                                     LPARAM    dwInitParam);

FailOnFalse [gle] EndDialog(HWND hDlg, int nResult);

DWORD GetDialogBaseUnits();

FailOnFalse [gle] MapDialogRect(HWND hDlg, [out] LPRECT lpRect);


HWND [gle] CreateMDIWindowA(LPCSTR      lpClassName,
                            LPCSTR      lpWindowName,
                            WindowStyle dwStyle,
                            int         X,
                            int         Y,
                            int         nWidth,
                            int         nHeight,
                            HWND        hWndParent,
                            HINSTANCE   hInstance,
                            LPARAM      lParam);

HWND [gle] CreateMDIWindowW(LPCWSTR     lpClassName,
                            LPCWSTR     lpWindowName,
                            WindowStyle dwStyle,
                            int         X,
                            int         Y,
                            int         nWidth,
                            int         nHeight,
                            HWND        hWndParent,
                            HINSTANCE   hInstance,
                            LPARAM      lParam);

LRESULT DefMDIChildProcA(HWND           hWnd,
                         WindowsMessage uMsg,
                         WPARAM         wParam,
                         LPARAM         lParam);

LRESULT DefMDIChildProcW(HWND           hWnd,
                         WindowsMessage uMsg,
                         WPARAM         wParam,
                         LPARAM         lParam);

BOOL TranslateMDISysAccel(HWND  hWndClient,
                          LPMSG lpMsg);

value DWORD MessageBoxReturn
{
#define ERROR               0 [fail]
#define IDOK                1
#define IDCANCEL            2
#define IDABORT             3
#define IDRETRY             4
#define IDIGNORE            5
#define IDYES               6
#define IDNO                7
#define IDCLOSE             8
#define IDHELP              9
#define IDTRYAGAIN          10
#define IDCONTINUE          11
};

mask DWORD MessageBoxType
{
#define MB_OK                       0x00000000
#define MB_OKCANCEL                 0x00000001
#define MB_ABORTRETRYIGNORE         0x00000002
#define MB_YESNOCANCEL              0x00000003
#define MB_YESNO                    0x00000004
#define MB_RETRYCANCEL              0x00000005
#define MB_CANCELTRYCONTINUE        0x00000006

#define MB_ICONHAND                 0x00000010
#define MB_ICONQUESTION             0x00000020
#define MB_ICONEXCLAMATION          0x00000030
#define MB_ICONASTERISK             0x00000040
#define MB_USERICON                 0x00000080

#define MB_DEFBUTTON1               0x00000000
#define MB_DEFBUTTON2               0x00000100
#define MB_DEFBUTTON3               0x00000200

#define MB_SYSTEMMODAL              0x00001000
#define MB_TASKMODAL                0x00002000
#define MB_HELP                     0x00004000

#define MB_NOFOCUS                  0x00008000
#define MB_SETFOREGROUND            0x00010000
#define MB_DEFAULT_DESKTOP_ONLY     0x00020000

#define MB_TOPMOST                  0x00040000
#define MB_RIGHT                    0x00080000
#define MB_RTLREADING               0x00100000
#define MB_SERVICE_NOTIFICATION     0x00200000
};

MessageBoxReturn [gle] MessageBoxA(HWND           hWnd,
                                   LPCSTR         lpText,
                                   LPCSTR         lpCaption,
                                   MessageBoxType uType);

MessageBoxReturn [gle] MessageBoxW(HWND           hWnd,
                                   LPCWSTR        lpText,
                                   LPCWSTR        lpCaption,
                                   MessageBoxType uType);

value DWORD LanguageId
{
#define LANG_NEUTRAL                     0x00
#define LANG_INVARIANT                   0x7f

#define LANG_AFRIKAANS                   0x36
#define LANG_ALBANIAN                    0x1c
#define LANG_ARABIC                      0x01
#define LANG_ARMENIAN                    0x2b
#define LANG_ASSAMESE                    0x4d
#define LANG_AZERI                       0x2c
#define LANG_BASQUE                      0x2d
#define LANG_BELARUSIAN                  0x23
#define LANG_BENGALI                     0x45
#define LANG_BULGARIAN                   0x02
#define LANG_CATALAN                     0x03
#define LANG_CHINESE                     0x04
#define LANG_CROATIAN                    0x1a
#define LANG_CZECH                       0x05
#define LANG_DANISH                      0x06
#define LANG_DUTCH                       0x13
#define LANG_ENGLISH                     0x09
#define LANG_ESTONIAN                    0x25
#define LANG_FAEROESE                    0x38
#define LANG_FARSI                       0x29
#define LANG_FINNISH                     0x0b
#define LANG_FRENCH                      0x0c
#define LANG_GEORGIAN                    0x37
#define LANG_GERMAN                      0x07
#define LANG_GREEK                       0x08
#define LANG_GUJARATI                    0x47
#define LANG_HEBREW                      0x0d
#define LANG_HINDI                       0x39
#define LANG_HUNGARIAN                   0x0e
#define LANG_ICELANDIC                   0x0f
#define LANG_INDONESIAN                  0x21
#define LANG_ITALIAN                     0x10
#define LANG_JAPANESE                    0x11
#define LANG_KANNADA                     0x4b
#define LANG_KASHMIRI                    0x60
#define LANG_KAZAK                       0x3f
#define LANG_KONKANI                     0x57
#define LANG_KOREAN                      0x12
#define LANG_LATVIAN                     0x26
#define LANG_LITHUANIAN                  0x27
#define LANG_MACEDONIAN                  0x2f
#define LANG_MALAY                       0x3e
#define LANG_MALAYALAM                   0x4c
#define LANG_MANIPURI                    0x58
#define LANG_MARATHI                     0x4e
#define LANG_NEPALI                      0x61
#define LANG_NORWEGIAN                   0x14
#define LANG_ORIYA                       0x48
#define LANG_POLISH                      0x15
#define LANG_PORTUGUESE                  0x16
#define LANG_PUNJABI                     0x46
#define LANG_ROMANIAN                    0x18
#define LANG_RUSSIAN                     0x19
#define LANG_SANSKRIT                    0x4f
#define LANG_SERBIAN                     0x1a
#define LANG_SINDHI                      0x59
#define LANG_SLOVAK                      0x1b
#define LANG_SLOVENIAN                   0x24
#define LANG_SPANISH                     0x0a
#define LANG_SWAHILI                     0x41
#define LANG_SWEDISH                     0x1d
#define LANG_TAMIL                       0x49
#define LANG_TATAR                       0x44
#define LANG_TELUGU                      0x4a
#define LANG_THAI                        0x1e
#define LANG_TURKISH                     0x1f
#define LANG_UKRAINIAN                   0x22
#define LANG_URDU                        0x20
#define LANG_UZBEK                       0x43
#define LANG_VIETNAMESE                  0x2a
};

MessageBoxReturn [gle] MessageBoxExA(HWND           hWnd,
                                     LPCSTR         lpText,
                                     LPCSTR         lpCaption,
                                     MessageBoxType uType,
                                     LanguageId     dwLanguageId);

MessageBoxReturn [gle] MessageBoxExW(HWND           hWnd,
                                     LPCWSTR        lpText,
                                     LPCWSTR        lpCaption,
                                     MessageBoxType uType,
                                     LanguageId     dwLanguageId);

typedef struct tagMSGBOXPARAMSA
{
    UINT            cbSize;
    HWND            hwndOwner;
    HINSTANCE       hInstance;
    LPCSTR          lpszText;
    LPCSTR          lpszCaption;
    MessageBoxType  dwStyle;
    LPCSTR          lpszIcon;
    DWORD           dwContextHelpId;
    WNDPROC         lpfnMsgBoxCallback;
    LanguageId      dwLanguageId;
} MSGBOXPARAMSA, *PMSGBOXPARAMSA, *LPMSGBOXPARAMSA;

typedef struct tagMSGBOXPARAMSW
{
    UINT            cbSize;
    HWND            hwndOwner;
    HINSTANCE       hInstance;
    LPCWSTR         lpszText;
    LPCWSTR         lpszCaption;
    MessageBoxType  dwStyle;
    LPCWSTR         lpszIcon;
    DWORD           dwContextHelpId;
    WNDPROC         lpfnMsgBoxCallback;
    LanguageId      dwLanguageId;
} MSGBOXPARAMSW, *PMSGBOXPARAMSW, *LPMSGBOXPARAMSW;


MessageBoxReturn MessageBoxIndirectA(MSGBOXPARAMSA* lpMsgBoxParam);
MessageBoxReturn MessageBoxIndirectW(MSGBOXPARAMSW* lpMsgBoxParam);

BOOL IsDialogMessageA(HWND hDlg, LPMSG lpMsg);
BOOL IsDialogMessageW(HWND hDlg, LPMSG lpMsg);

BOOL IsChild(HWND hWndParent, HWND hWnd);

BOOL IsIconic(HWND hWnd);

BOOL IsMenu(HMENU hMenu);

BOOL IsRectEmpty(LPRECT lprc);

BOOL IntersectRect([out] LPRECT lprcDst, LPRECT lprcSrc1, LPRECT lprcSrc2);

BOOL UnionRect([out] LPRECT lprcDst, LPRECT lprcSrc1, LPRECT lprcSrc2);

FailOnFalse [gle] CopyRect([out] LPRECT lprcDst, LPRECT lprcSrc);

BOOL SubtractRect([out] LPRECT lprcDst, LPRECT lprcSrc1, LPRECT lprcSrc2);


FailOnFalse [gle] InvertRect(HDC hDC, LPRECT lprc);


BOOL PtInRect(LPRECT lprc, int x, int y);

FailOnFalse IsWindow(HWND hWnd);

BOOL IsWindowEnabled(HWND hWnd);

BOOL IsWindowUnicode(HWND hWnd);

BOOL IsWindowVisible(HWND hWnd);

BOOL IsZoomed(HWND hWnd);

DwordFailIfZero [gle] RegisterWindowMessageA(LPCSTR lpString);
DwordFailIfZero [gle] RegisterWindowMessageW(LPCWSTR lpString);

FailOnFalse [gle] SetPropA(HWND hWnd, LPCSTR lpString, HANDLE hData);
FailOnFalse [gle] SetPropW(HWND hWnd, LPCWSTR lpString, HANDLE hData);

HANDLE GetPropA(HWND hWnd, LPCSTR lpString);
HANDLE GetPropW(HWND hWnd, LPCWSTR lpString);

HANDLE RemovePropA(HWND hWnd, LPCSTR lpString);
HANDLE RemovePropW(HWND hWnd, LPCWSTR lpString);

typedef LPVOID PROPENUMPROCA;
typedef LPVOID PROPENUMPROCW;
typedef LPVOID PROPENUMPROCEXA;
typedef LPVOID PROPENUMPROCEXW;

LongFailIfNeg1 EnumPropsA(HWND hWnd, PROPENUMPROCA lpEnumFunc);
LongFailIfNeg1 EnumPropsW(HWND hWnd, PROPENUMPROCW lpEnumFunc);

LongFailIfNeg1 EnumPropsExA(HWND hWnd, PROPENUMPROCEXA lpEnumFunc, LPARAM lParam);
LongFailIfNeg1 EnumPropsExW(HWND hWnd, PROPENUMPROCEXW lpEnumFunc, LPARAM lParam);

mask DWORD QueueStates
{
#define QS_KEY              0x0001
#define QS_MOUSEMOVE        0x0002
#define QS_MOUSEBUTTON      0x0004
#define QS_POSTMESSAGE      0x0008
#define QS_TIMER            0x0010
#define QS_PAINT            0x0020
#define QS_SENDMESSAGE      0x0040
#define QS_HOTKEY           0x0080
#define QS_ALLPOSTMESSAGE   0x0100
#define QS_RAWINPUT         0x0400
};

WaitReturnValues [gle] MsgWaitForMultipleObjects(DWORD       nCount,
                                                 HANDLE*     pHandles,
                                                 BOOL        fWaitAll,
                                                 DWORD       dwMilliseconds,
                                                 QueueStates dwWakeMask);

mask DWORD MWMOFlags
{
#define MWMO_WAITALL        0x0001
#define MWMO_ALERTABLE      0x0002
#define MWMO_INPUTAVAILABLE 0x0004
};

WaitReturnValues [gle] MsgWaitForMultipleObjectsEx(DWORD       nCount,
                                                   HANDLE*     pHandles,
                                                   DWORD       dwMilliseconds,
                                                   QueueStates dwWakeMask,
                                                   MWMOFlags   dwFlags);

WaitReturnValues [gle] WaitForInputIdle(HANDLE hProcess, DWORD dwMilliseconds);

value DWORD GetWindowCmd
{
#define GW_HWNDFIRST        0
#define GW_HWNDLAST         1
#define GW_HWNDNEXT         2
#define GW_HWNDPREV         3
#define GW_OWNER            4
#define GW_CHILD            5
#define GW_ENABLEDPOPUP     6
};

HWND [gle] GetWindow(HWND hWnd, GetWindowCmd uCmd);

DWORD GetWindowContextHelpId(HWND hwnd);

FailOnFalse [gle] SetWindowContextHelpId(HWND hwnd, DWORD dwHelpId);

typedef struct tagWINDOWINFO
{
    DWORD cbSize;
    RECT  rcWindow;
    RECT  rcClient;
    DWORD dwStyle;
    DWORD dwExStyle;
    DWORD dwWindowStatus;
    UINT  cxWindowBorders;
    UINT  cyWindowBorders;
    ATOM  atomWindowType;
    WORD  wCreatorVersion;
} WINDOWINFO, *PWINDOWINFO, *LPWINDOWINFO;


FailOnFalse [gle] GetWindowInfo(HWND hwnd, [out] PWINDOWINFO pwi);

UINT GetWindowModuleFileNameA(HWND hwnd, [out] LPSTR pszFileName, UINT cchFileNameMax);
UINT GetWindowModuleFileNameW(HWND hwnd, [out] LPWSTR pszFileName, UINT cchFileNameMax);


FailOnFalse [gle] GetWindowRect(HWND hWnd, [out] LPRECT lpRect);
FailOnFalse [gle] GetClientRect(HWND hwnd, [out] LPRECT lprect);

FailOnFalse [gle] SetWindowRgn(HWND hWnd, HRGN hRgn, BOOL bRedraw);

HRGN GetWindowRgn(HWND hWnd, HRGN hRgn);

ThreadId GetWindowThreadProcessId(HWND hWnd, [out] ProcessId* lpdwProcessId);

FailOnFalse [gle] EnableWindow(HWND hWnd, BOOL bEnable);

value DWORD SPIValues
{
#define SPI_GETBEEP                 1
#define SPI_SETBEEP                 2
#define SPI_GETMOUSE                3
#define SPI_SETMOUSE                4
#define SPI_GETBORDER               5
#define SPI_SETBORDER               6
#define SPI_GETKEYBOARDSPEED       10
#define SPI_SETKEYBOARDSPEED       11
#define SPI_LANGDRIVER             12
#define SPI_ICONHORIZONTALSPACING  13
#define SPI_GETSCREENSAVETIMEOUT   14
#define SPI_SETSCREENSAVETIMEOUT   15
#define SPI_GETSCREENSAVEACTIVE    16
#define SPI_SETSCREENSAVEACTIVE    17
#define SPI_GETGRIDGRANULARITY     18
#define SPI_SETGRIDGRANULARITY     19
#define SPI_SETDESKWALLPAPER       20
#define SPI_SETDESKPATTERN         21
#define SPI_GETKEYBOARDDELAY       22
#define SPI_SETKEYBOARDDELAY       23
#define SPI_ICONVERTICALSPACING    24
#define SPI_GETICONTITLEWRAP       25
#define SPI_SETICONTITLEWRAP       26
#define SPI_GETMENUDROPALIGNMENT   27
#define SPI_SETMENUDROPALIGNMENT   28
#define SPI_SETDOUBLECLKWIDTH      29
#define SPI_SETDOUBLECLKHEIGHT     30
#define SPI_GETICONTITLELOGFONT    31
#define SPI_SETDOUBLECLICKTIME     32
#define SPI_SETMOUSEBUTTONSWAP     33
#define SPI_SETICONTITLELOGFONT    34
#define SPI_GETFASTTASKSWITCH      35
#define SPI_SETFASTTASKSWITCH      36
#define SPI_SETDRAGFULLWINDOWS     37
#define SPI_GETDRAGFULLWINDOWS     38
#define SPI_GETNONCLIENTMETRICS    41
#define SPI_SETNONCLIENTMETRICS    42
#define SPI_GETMINIMIZEDMETRICS    43
#define SPI_SETMINIMIZEDMETRICS    44
#define SPI_GETICONMETRICS         45
#define SPI_SETICONMETRICS         46
#define SPI_SETWORKAREA            47
#define SPI_GETWORKAREA            48
#define SPI_SETPENWINDOWS          49

#define SPI_GETFILTERKEYS          50
#define SPI_SETFILTERKEYS          51
#define SPI_GETTOGGLEKEYS          52
#define SPI_SETTOGGLEKEYS          53
#define SPI_GETMOUSEKEYS           54
#define SPI_SETMOUSEKEYS           55
#define SPI_GETSHOWSOUNDS          56
#define SPI_SETSHOWSOUNDS          57
#define SPI_GETSTICKYKEYS          58
#define SPI_SETSTICKYKEYS          59
#define SPI_GETACCESSTIMEOUT       60
#define SPI_SETACCESSTIMEOUT       61
#define SPI_GETSERIALKEYS          62
#define SPI_SETSERIALKEYS          63
#define SPI_GETSOUNDSENTRY         64
#define SPI_SETSOUNDSENTRY         65

#define SPI_GETHIGHCONTRAST        66
#define SPI_SETHIGHCONTRAST        67
#define SPI_GETKEYBOARDPREF        68
#define SPI_SETKEYBOARDPREF        69
#define SPI_GETSCREENREADER        70
#define SPI_SETSCREENREADER        71
#define SPI_GETANIMATION           72
#define SPI_SETANIMATION           73
#define SPI_GETFONTSMOOTHING       74
#define SPI_SETFONTSMOOTHING       75
#define SPI_SETDRAGWIDTH           76
#define SPI_SETDRAGHEIGHT          77
#define SPI_SETHANDHELD            78
#define SPI_GETLOWPOWERTIMEOUT     79
#define SPI_GETPOWEROFFTIMEOUT     80
#define SPI_SETLOWPOWERTIMEOUT     81
#define SPI_SETPOWEROFFTIMEOUT     82
#define SPI_GETLOWPOWERACTIVE      83
#define SPI_GETPOWEROFFACTIVE      84
#define SPI_SETLOWPOWERACTIVE      85
#define SPI_SETPOWEROFFACTIVE      86
#define SPI_SETCURSORS             87
#define SPI_SETICONS               88
#define SPI_GETDEFAULTINPUTLANG    89
#define SPI_SETDEFAULTINPUTLANG    90
#define SPI_SETLANGTOGGLE          91
#define SPI_GETWINDOWSEXTENSION    92
#define SPI_SETMOUSETRAILS         93
#define SPI_GETMOUSETRAILS         94

#define SPI_GETSNAPTODEFBUTTON     95
#define SPI_SETSNAPTODEFBUTTON     96

#define SPI_SETSCREENSAVERRUNNING  97

#define SPI_GETMOUSEHOVERWIDTH     98
#define SPI_SETMOUSEHOVERWIDTH     99
#define SPI_GETMOUSEHOVERHEIGHT   100
#define SPI_SETMOUSEHOVERHEIGHT   101
#define SPI_GETMOUSEHOVERTIME     102
#define SPI_SETMOUSEHOVERTIME     103
#define SPI_GETWHEELSCROLLLINES   104
#define SPI_SETWHEELSCROLLLINES   105
#define SPI_GETMENUSHOWDELAY      106
#define SPI_SETMENUSHOWDELAY      107

#define SPI_GETSHOWIMEUI          110
#define SPI_SETSHOWIMEUI          111


#define SPI_GETMOUSESPEED         112
#define SPI_SETMOUSESPEED         113
#define SPI_GETSCREENSAVERRUNNING 114
#define SPI_GETDESKWALLPAPER      115

#define SPI_GETACTIVEWINDOWTRACKING         0x1000
#define SPI_SETACTIVEWINDOWTRACKING         0x1001
#define SPI_GETMENUANIMATION                0x1002
#define SPI_SETMENUANIMATION                0x1003
#define SPI_GETCOMBOBOXANIMATION            0x1004
#define SPI_SETCOMBOBOXANIMATION            0x1005
#define SPI_GETLISTBOXSMOOTHSCROLLING       0x1006
#define SPI_SETLISTBOXSMOOTHSCROLLING       0x1007
#define SPI_GETGRADIENTCAPTIONS             0x1008
#define SPI_SETGRADIENTCAPTIONS             0x1009
#define SPI_GETKEYBOARDCUES                 0x100A
#define SPI_SETKEYBOARDCUES                 0x100B
#define SPI_GETACTIVEWNDTRKZORDER           0x100C
#define SPI_SETACTIVEWNDTRKZORDER           0x100D
#define SPI_GETHOTTRACKING                  0x100E
#define SPI_SETHOTTRACKING                  0x100F
#define SPI_GETMENUFADE                     0x1012
#define SPI_SETMENUFADE                     0x1013
#define SPI_GETSELECTIONFADE                0x1014
#define SPI_SETSELECTIONFADE                0x1015
#define SPI_GETTOOLTIPANIMATION             0x1016
#define SPI_SETTOOLTIPANIMATION             0x1017
#define SPI_GETTOOLTIPFADE                  0x1018
#define SPI_SETTOOLTIPFADE                  0x1019
#define SPI_GETCURSORSHADOW                 0x101A
#define SPI_SETCURSORSHADOW                 0x101B
#define SPI_GETMOUSESONAR                   0x101C
#define SPI_SETMOUSESONAR                   0x101D
#define SPI_GETMOUSECLICKLOCK               0x101E
#define SPI_SETMOUSECLICKLOCK               0x101F
#define SPI_GETMOUSEVANISH                  0x1020
#define SPI_SETMOUSEVANISH                  0x1021
#define SPI_GETFLATMENU                     0x1022
#define SPI_SETFLATMENU                     0x1023
#define SPI_GETDROPSHADOW                   0x1024
#define SPI_SETDROPSHADOW                   0x1025

#define SPI_GETUIEFFECTS                    0x103E
#define SPI_SETUIEFFECTS                    0x103F

#define SPI_GETFOREGROUNDLOCKTIMEOUT        0x2000
#define SPI_SETFOREGROUNDLOCKTIMEOUT        0x2001
#define SPI_GETACTIVEWNDTRKTIMEOUT          0x2002
#define SPI_SETACTIVEWNDTRKTIMEOUT          0x2003
#define SPI_GETFOREGROUNDFLASHCOUNT         0x2004
#define SPI_SETFOREGROUNDFLASHCOUNT         0x2005
#define SPI_GETCARETWIDTH                   0x2006
#define SPI_SETCARETWIDTH                   0x2007

#define SPI_GETMOUSECLICKLOCKTIME           0x2008
#define SPI_SETMOUSECLICKLOCKTIME           0x2009
#define SPI_GETFONTSMOOTHINGTYPE            0x200A
#define SPI_SETFONTSMOOTHINGTYPE            0x200B

#define SPI_GETFONTSMOOTHINGGAMMA           0x200C
#define SPI_SETFONTSMOOTHINGGAMMA           0x200D

#define SPI_GETFOCUSBORDERWIDTH             0x200E
#define SPI_SETFOCUSBORDERWIDTH             0x200F
#define SPI_GETFOCUSBORDERHEIGHT            0x2010
#define SPI_SETFOCUSBORDERHEIGHT            0x2011
};

mask DWORD SPIWinini
{
#define SPIF_UPDATEINIFILE    0x0001
#define SPIF_SENDWININICHANGE 0x0002
};

FailOnFalse [gle] SystemParametersInfoA(SPIValues uiAction,
                                        UINT      uiParam,
                            /* [out] */ LPVOID    pvParam,
                                        SPIWinini fWinIni);

FailOnFalse [gle] SystemParametersInfoW(SPIValues uiAction,
                                        UINT      uiParam,
                            /* [out] */ LPVOID    pvParam,
                                        SPIWinini fWinIni);

value DWORD ColorIndex
{
#define CTLCOLOR_MSGBOX         0
#define CTLCOLOR_EDIT           1
#define CTLCOLOR_LISTBOX        2
#define CTLCOLOR_BTN            3
#define CTLCOLOR_DLG            4
#define CTLCOLOR_SCROLLBAR      5
#define CTLCOLOR_STATIC         6
#define CTLCOLOR_MAX            7

#define COLOR_SCROLLBAR         0
#define COLOR_BACKGROUND        1
#define COLOR_ACTIVECAPTION     2
#define COLOR_INACTIVECAPTION   3
#define COLOR_MENU              4
#define COLOR_WINDOW            5
#define COLOR_WINDOWFRAME       6
#define COLOR_MENUTEXT          7
#define COLOR_WINDOWTEXT        8
#define COLOR_CAPTIONTEXT       9
#define COLOR_ACTIVEBORDER      10
#define COLOR_INACTIVEBORDER    11
#define COLOR_APPWORKSPACE      12
#define COLOR_HIGHLIGHT         13
#define COLOR_HIGHLIGHTTEXT     14
#define COLOR_BTNFACE           15
#define COLOR_BTNSHADOW         16
#define COLOR_GRAYTEXT          17
#define COLOR_BTNTEXT           18
#define COLOR_INACTIVECAPTIONTEXT 19
#define COLOR_BTNHIGHLIGHT      20

#define COLOR_3DDKSHADOW        21
#define COLOR_3DLIGHT           22
#define COLOR_INFOTEXT          23
#define COLOR_INFOBK            24

#define COLOR_HOTLIGHT          26
#define COLOR_GRADIENTACTIVECAPTION 27
#define COLOR_GRADIENTINACTIVECAPTION 28
#define COLOR_MENUHILIGHT       29
#define COLOR_MENUBAR           30
};

DWORD GetSysColor(ColorIndex nIndex);

FailOnFalse [gle] SetSysColors(int         cElements,
                               ColorIndex* lpaElements,
                               DWORD*      lpaRgbValues);

HRSRC GetSysColorBrush(ColorIndex nIndex);

HWND [gle] SetParent(HWND hWndChild, HWND hWndNewParent);
HWND [gle] GetParent(HWND hWnd);

value DWORD GetAncestorType
{
#define     GA_PARENT       1
#define     GA_ROOT         2
#define     GA_ROOTOWNER    3
};

HWND GetAncestor(HWND hwnd, GetAncestorType gaFlags);

HWND [gle] GetTopWindow(HWND hWnd);

FailOnFalse [gle] ClipCursor(LPRECT lpRect);
FailOnFalse [gle] GetClipCursor([out] LPRECT lpRect);

HRSRC [gle] CreateCursor(HINSTANCE hInst,
                         int xHotSpot,
                         int yHotSpot,
                         int nWidth,
                         int nHeight,
                         LPVOID pvANDPlane,
                         LPVOID pvXORPlane);

HRSRC [gle] LoadCursorFromFileA(LPCSTR lpFileName);
HRSRC [gle] LoadCursorFromFileW(LPCWSTR lpFileName);

HRSRC GetCursor();

value DWORD CursorInfoFlags
{
#define CURSOR_HIDDEN      0x00000000
#define CURSOR_SHOWING     0x00000001
};

typedef struct tagCURSORINFO
{
    DWORD           cbSize;
    CursorInfoFlags flags;
    HRSRC           hCursor;
    POINT           ptScreenPos;
} CURSORINFO, *PCURSORINFO, *LPCURSORINFO;

FailOnFalse [gle] GetCursorInfo([out] PCURSORINFO pci);

FailOnFalse [gle] SetCursorPos(int X, int Y);
FailOnFalse [gle] GetCursorPos( [out] LPPOINT lpPoint);

FailOnFalse [gle] SetSystemCursor(HRSRC hcur, ImageValueA id);

int ShowCursor(BOOL bShow);


FailOnFalse [gle] GetClassInfoA(HINSTANCE         hInstance,
                                LPCSTR            lpClassName,
                                [out] LPWNDCLASSA lpWndClass);

FailOnFalse [gle] GetClassInfoW(HINSTANCE         hInstance,
                                LPCWSTR           lpClassName,
                                [out] LPWNDCLASSW lpWndClass);

FailOnFalse [gle] GetClassInfoExA(HINSTANCE           hInstance,
                                  LPCSTR              lpClassName,
                                  [out] LPWNDCLASSEXA lpWndClassEx);

FailOnFalse [gle] GetClassInfoExW(HINSTANCE           hInstance,
                                  LPCWSTR             lpClassName,
                                  [out] LPWNDCLASSEXW lpWndClassEx);

value DWORD ClassLongIndex
{
#define GWLP_WNDPROC        -4
#define GWLP_HINSTANCE      -6
#define GWLP_HWNDPARENT     -8
#define GWLP_USERDATA       -21
#define GWLP_ID             -12

#define GCL_MENUNAME        -8
#define GCL_HBRBACKGROUND   -10
#define GCL_HCURSOR         -12
#define GCL_HICON           -14
#define GCL_HMODULE         -16
#define GCL_CBWNDEXTRA      -18
#define GCL_CBCLSEXTRA      -20
#define GCL_WNDPROC         -24
#define GCL_STYLE           -26
#define GCW_ATOM            -32
};

DwordFailIfZero [gle] GetClassLongA(HWND hWnd, ClassLongIndex nIndex);
DwordFailIfZero [gle] GetClassLongW(HWND hWnd, ClassLongIndex nIndex);

DwordFailIfZero [gle] SetClassLongA(HWND hWnd, ClassLongIndex nIndex, DWORD dwNewLong);
DwordFailIfZero [gle] SetClassLongW(HWND hWnd, ClassLongIndex nIndex, DWORD dwNewLong);

LongFailIfZero [gle] GetClassNameA(HWND hWnd, [out] LPSTR lpClassName, int nMaxCount);
LongFailIfZero [gle] GetClassNameW(HWND hWnd, [out] LPWSTR lpClassName, int nMaxCount);

HWND GetLastActivePopup(HWND hWnd);

FailOnFalse [gle] ShowOwnedPopups(HWND hWnd, BOOL fShow);

mask DWORD ExitWindowsExFlags
{
#define EWX_LOGOFF          0x00000000
#define EWX_SHUTDOWN        0x00000001
#define EWX_REBOOT          0x00000002
#define EWX_FORCE           0x00000004
#define EWX_POWEROFF        0x00000008
#define EWX_FORCEIFHUNG     0x00000010
};

FailOnFalse [gle] ExitWindowsEx(ExitWindowsExFlags uFlags, DWORD dwReserved);

LongFailIfZero [gle] FillRect(HDC hDC, LPRECT lprc, HRSRC hbr);

HWND [gle] FindWindowA(LPCSTR lpClassName, LPCSTR lpWindowName);
HWND [gle] FindWindowW(LPCWSTR lpClassName, LPCWSTR lpWindowName);

HWND [gle] FindWindowExA(HWND hwndParent, HWND hwndChildAfter, LPCSTR lpszClass, LPCSTR lpszWindow);
HWND [gle] FindWindowExW(HWND hwndParent, HWND hwndChildAfter, LPCWSTR lpszClass, LPCWSTR lpszWindow);

BOOL FlashWindow(HWND hWnd, BOOL bInvert);

mask DWORD FlashWindowExFlags
{
#define FLASHW_STOP         0
#define FLASHW_CAPTION      0x00000001
#define FLASHW_TRAY         0x00000002
#define FLASHW_TIMER        0x00000004
#define FLASHW_TIMERNOFG    0x00000008
};

typedef struct _FLASHWINFO {
    UINT  cbSize;
    HWND  hwnd;
    FlashWindowExFlags dwFlags;
    UINT  uCount;
    DWORD dwTimeout;
} FLASHWINFO, *PFLASHWINFO;

BOOL FlashWindowEx(PFLASHWINFO pfwi);

LongFailIfZero [gle] FrameRect(HDC hDC, LPRECT lprc, HRSRC hbr);

FailOnFalse [gle] SetRect([out] LPRECT lprc, int xLeft, int yTop, int xRight, int yBottom);

FailOnFalse [gle] SetRectEmpty([out] LPRECT lprc);

FailOnFalse [gle] EqualRect(LPRECT lprc1, LPRECT lprc2);

FailOnFalse [gle] OffsetRect([out] LPRECT lprc, int dx, int dy);

FailOnFalse [gle] InflateRect([out] LPRECT lprc, int dx, int dy);

value DWORD VirtualKey
{
#define VK_LBUTTON        0x01
#define VK_RBUTTON        0x02
#define VK_CANCEL         0x03
#define VK_MBUTTON        0x04

#define VK_XBUTTON1       0x05
#define VK_XBUTTON2       0x06

#define VK_BACK           0x08
#define VK_TAB            0x09

#define VK_CLEAR          0x0C
#define VK_RETURN         0x0D

#define VK_SHIFT          0x10
#define VK_CONTROL        0x11
#define VK_MENU           0x12
#define VK_PAUSE          0x13
#define VK_CAPITAL        0x14

#define VK_KANA           0x15
#define VK_JUNJA          0x17
#define VK_FINAL          0x18
#define VK_KANJI          0x19

#define VK_ESCAPE         0x1B

#define VK_CONVERT        0x1C
#define VK_NONCONVERT     0x1D
#define VK_ACCEPT         0x1E
#define VK_MODECHANGE     0x1F

#define VK_SPACE          0x20
#define VK_PRIOR          0x21
#define VK_NEXT           0x22
#define VK_END            0x23
#define VK_HOME           0x24
#define VK_LEFT           0x25
#define VK_UP             0x26
#define VK_RIGHT          0x27
#define VK_DOWN           0x28
#define VK_SELECT         0x29
#define VK_PRINT          0x2A
#define VK_EXECUTE        0x2B
#define VK_SNAPSHOT       0x2C
#define VK_INSERT         0x2D
#define VK_DELETE         0x2E
#define VK_HELP           0x2F

#define VK_0              0x30
#define VK_1              0x31
#define VK_2              0x32
#define VK_3              0x33
#define VK_4              0x34
#define VK_5              0x35
#define VK_6              0x36
#define VK_7              0x37
#define VK_8              0x38
#define VK_9              0x39

#define VK_A              0x41
#define VK_B              0x42
#define VK_C              0x43
#define VK_D              0x44
#define VK_E              0x45
#define VK_F              0x46
#define VK_G              0x47
#define VK_H              0x48
#define VK_I              0x49
#define VK_J              0x4A
#define VK_K              0x4B
#define VK_L              0x4C
#define VK_M              0x4D
#define VK_N              0x4E
#define VK_O              0x4F
#define VK_P              0x50
#define VK_Q              0x51
#define VK_R              0x52
#define VK_S              0x53
#define VK_T              0x54
#define VK_U              0x55
#define VK_V              0x56
#define VK_W              0x57
#define VK_X              0x58
#define VK_Y              0x59
#define VK_Z              0x5A

#define VK_LWIN           0x5B
#define VK_RWIN           0x5C
#define VK_APPS           0x5D

#define VK_SLEEP          0x5F

#define VK_NUMPAD0        0x60
#define VK_NUMPAD1        0x61
#define VK_NUMPAD2        0x62
#define VK_NUMPAD3        0x63
#define VK_NUMPAD4        0x64
#define VK_NUMPAD5        0x65
#define VK_NUMPAD6        0x66
#define VK_NUMPAD7        0x67
#define VK_NUMPAD8        0x68
#define VK_NUMPAD9        0x69
#define VK_MULTIPLY       0x6A
#define VK_ADD            0x6B
#define VK_SEPARATOR      0x6C
#define VK_SUBTRACT       0x6D
#define VK_DECIMAL        0x6E
#define VK_DIVIDE         0x6F
#define VK_F1             0x70
#define VK_F2             0x71
#define VK_F3             0x72
#define VK_F4             0x73
#define VK_F5             0x74
#define VK_F6             0x75
#define VK_F7             0x76
#define VK_F8             0x77
#define VK_F9             0x78
#define VK_F10            0x79
#define VK_F11            0x7A
#define VK_F12            0x7B
#define VK_F13            0x7C
#define VK_F14            0x7D
#define VK_F15            0x7E
#define VK_F16            0x7F
#define VK_F17            0x80
#define VK_F18            0x81
#define VK_F19            0x82
#define VK_F20            0x83
#define VK_F21            0x84
#define VK_F22            0x85
#define VK_F23            0x86
#define VK_F24            0x87

#define VK_NUMLOCK        0x90
#define VK_SCROLL         0x91

#define VK_OEM_FJ_JISHO   0x92
#define VK_OEM_FJ_MASSHOU 0x93
#define VK_OEM_FJ_TOUROKU 0x94
#define VK_OEM_FJ_LOYA    0x95
#define VK_OEM_FJ_ROYA    0x96

#define VK_LSHIFT         0xA0
#define VK_RSHIFT         0xA1
#define VK_LCONTROL       0xA2
#define VK_RCONTROL       0xA3
#define VK_LMENU          0xA4
#define VK_RMENU          0xA5

#define VK_BROWSER_BACK        0xA6
#define VK_BROWSER_FORWARD     0xA7
#define VK_BROWSER_REFRESH     0xA8
#define VK_BROWSER_STOP        0xA9
#define VK_BROWSER_SEARCH      0xAA
#define VK_BROWSER_FAVORITES   0xAB
#define VK_BROWSER_HOME        0xAC

#define VK_VOLUME_MUTE         0xAD
#define VK_VOLUME_DOWN         0xAE
#define VK_VOLUME_UP           0xAF
#define VK_MEDIA_NEXT_TRACK    0xB0
#define VK_MEDIA_PREV_TRACK    0xB1
#define VK_MEDIA_STOP          0xB2
#define VK_MEDIA_PLAY_PAUSE    0xB3
#define VK_LAUNCH_MAIL         0xB4
#define VK_LAUNCH_MEDIA_SELECT 0xB5
#define VK_LAUNCH_APP1         0xB6
#define VK_LAUNCH_APP2         0xB7

#define VK_OEM_1          0xBA
#define VK_OEM_PLUS       0xBB
#define VK_OEM_COMMA      0xBC
#define VK_OEM_MINUS      0xBD
#define VK_OEM_PERIOD     0xBE
#define VK_OEM_2          0xBF
#define VK_OEM_3          0xC0

#define VK_OEM_4          0xDB
#define VK_OEM_5          0xDC
#define VK_OEM_6          0xDD
#define VK_OEM_7          0xDE
#define VK_OEM_8          0xDF

#define VK_OEM_AX         0xE1
#define VK_OEM_102        0xE2
#define VK_ICO_HELP       0xE3
#define VK_ICO_00         0xE4

#define VK_PROCESSKEY     0xE5

#define VK_ICO_CLEAR      0xE6

#define VK_PACKET         0xE7

#define VK_OEM_RESET      0xE9
#define VK_OEM_JUMP       0xEA
#define VK_OEM_PA1        0xEB
#define VK_OEM_PA2        0xEC
#define VK_OEM_PA3        0xED
#define VK_OEM_WSCTRL     0xEE
#define VK_OEM_CUSEL      0xEF
#define VK_OEM_ATTN       0xF0
#define VK_OEM_FINISH     0xF1
#define VK_OEM_COPY       0xF2
#define VK_OEM_AUTO       0xF3
#define VK_OEM_ENLW       0xF4
#define VK_OEM_BACKTAB    0xF5

#define VK_ATTN           0xF6
#define VK_CRSEL          0xF7
#define VK_EXSEL          0xF8
#define VK_EREOF          0xF9
#define VK_PLAY           0xFA
#define VK_ZOOM           0xFB
#define VK_NONAME         0xFC
#define VK_PA1            0xFD
#define VK_OEM_CLEAR      0xFE
};

DWORD GetKeyState(VirtualKey nVirtKey);
DWORD GetAsyncKeyState(VirtualKey vKey);

DwordFailIfNeg1 VkKeyScanA(char ch);
DwordFailIfNeg1 VkKeyScanW(WCHAR ch);

DwordFailIfNeg1 VkKeyScanExA(char ch, HKL dwhkl);
DwordFailIfNeg1 VkKeyScanExW(WCHAR ch, HKL dwhkl);


mask DWORD KeyModifier
{
#define MOD_ALT         0x0001
#define MOD_CONTROL     0x0002
#define MOD_SHIFT       0x0004
#define MOD_WIN         0x0008
};

FailOnFalse [gle] RegisterHotKey(HWND        hWnd,
                                 int         id,
                                 KeyModifier fsModifiers,
                                 VirtualKey  vk);

FailOnFalse [gle] UnregisterHotKey(HWND hWnd, int id);


LongFailIfZero [gle] GetKeyNameTextA(LONG lParam, [out] LPSTR lpString, int nSize);
LongFailIfZero [gle] GetKeyNameTextW(LONG lParam, [out] LPWSTR lpString, int nSize);

typedef struct tagKEYSTATE
{
    BYTE st[256];
} KEYSTATE, *LPKEYSTATE;

FailOnFalse [gle] GetKeyboardState([out] LPKEYSTATE lpKeyState);
FailOnFalse [gle] SetKeyboardState(LPKEYSTATE lpKeyState);

LongFailIfZero [gle] GetKeyboardType(int nTypeFlag);

typedef struct tagLASTINPUTINFO {
    UINT  cbSize;
    DWORD dwTime;
} LASTINPUTINFO, * PLASTINPUTINFO;

FailOnFalse GetLastInputInfo([out] PLASTINPUTINFO plii);

typedef LPVOID WNDENUMPROC;

FailOnFalse [gle] EnumThreadWindows(ThreadId dwThreadId, WNDENUMPROC lpfn, LPARAM lParam);

FailOnFalse [gle] EnumWindows(WNDENUMPROC lpEnumFunc, LPARAM lParam);

FailOnFalse [gle] EnumChildWindows(HWND hWndParent, WNDENUMPROC lpEnumFunc, LPARAM lParam);

FailOnFalse [gle] EnumDesktopWindows(HDESK hDesktop, WNDENUMPROC lpfn, LPARAM lParam);

HDESK [gle] GetThreadDesktop(ThreadId dwThreadId);

FailOnFalse [gle] CloseDesktop(HDESK hDesktop);

mask DWORD DesktopAccessMask
{
#define DESKTOP_READOBJECTS         0x0001
#define DESKTOP_CREATEWINDOW        0x0002
#define DESKTOP_CREATEMENU          0x0004
#define DESKTOP_HOOKCONTROL         0x0008
#define DESKTOP_JOURNALRECORD       0x0010
#define DESKTOP_JOURNALPLAYBACK     0x0020
#define DESKTOP_ENUMERATE           0x0040
#define DESKTOP_WRITEOBJECTS        0x0080
#define DESKTOP_SWITCHDESKTOP       0x0100
};

HDESK [gle] CreateDesktopA(LPCSTR                lpszDesktop,
                           LPCSTR                lpszDevice,
                           LPDEVMODEA            pDevmode,
                           DWORD                 dwFlags,
                           DesktopAccessMask     dwDesiredAccess,
                           LPSECURITY_ATTRIBUTES lpsa);

HDESK [gle] CreateDesktopW(LPCWSTR               lpszDesktop,
                           LPCWSTR               lpszDevice,
                           LPDEVMODEW            pDevmode,
                           DWORD                 dwFlags,
                           DesktopAccessMask     dwDesiredAccess,
                           LPSECURITY_ATTRIBUTES lpsa);

typedef LPVOID DESKTOPENUMPROCA;
typedef LPVOID DESKTOPENUMPROCW;

FailOnFalse [gle] EnumDesktopsA(HWINSTA hwinsta, DESKTOPENUMPROCA lpEnumFunc, LPARAM lParam);
FailOnFalse [gle] EnumDesktopsW(HWINSTA hwinsta, DESKTOPENUMPROCW lpEnumFunc, LPARAM lParam);

HDESK [gle] OpenInputDesktop(DWORD dwFlags, BOOL fInherit, DesktopAccessMask dwDesiredAccess);

FailOnFalse [gle] SetThreadDesktop(HDESK hDesktop);

HDESK [gle] OpenDesktopA(LPCSTR            lpszDesktop,
                         DWORD             dwFlags,
                         BOOL              fInherit,
                         DesktopAccessMask dwDesiredAccess);

HDESK [gle] OpenDesktopW(LPCWSTR           lpszDesktop,
                         DWORD             dwFlags,
                         BOOL              fInherit,
                         DesktopAccessMask dwDesiredAccess);

FailOnFalse [gle] SwitchDesktop(HDESK hDesktop);

LongFailIfZero [gle] TileWindows(HWND   hwndParent,
                                 UINT   wHow,
                                 LPRECT lpRect,
                                 UINT   cKids,
                                 HWND*  lpKids);

LongFailIfZero [gle] CascadeWindows(HWND   hwndParent,
                                    UINT   wHow,
                                    LPRECT lpRect,
                                    UINT   cKids,
                                    HWND*  lpKids);

FailOnFalse [gle] BringWindowToTop(HWND hWnd);

mask DWORD ButtonState
{
#define BST_UNCHECKED      0x0000
#define BST_CHECKED        0x0001
#define BST_INDETERMINATE  0x0002
#define BST_PUSHED         0x0004
#define BST_FOCUS          0x0008
};

FailOnFalse [gle] CheckDlgButton(HWND hDlg, int nIDButton, ButtonState uCheck);

FailOnFalse [gle] CheckRadioButton(HWND hDlg,
                                   int  nIDFirstButton,
                                   int  nIDLastButton,
                                   int  nIDCheckButton);

ButtonState IsDlgButtonChecked(HWND hDlg, int nIDButton);

LongFailIfZero [gle] GetDlgCtrlID(HWND hWnd);

HWND [gle] GetDlgItem(HWND hDlg, int nIDDlgItem);

LongFailIfZero [gle] GetDlgItemInt(HWND        hDlg,
                                   int         nIDDlgItem,
                                   [out] BOOL* lpTranslated,
                                   BOOL        bSigned);


mask DWORD DrawTextFlags
{
#define DT_TOP                      0x00000000
#define DT_LEFT                     0x00000000
#define DT_CENTER                   0x00000001
#define DT_RIGHT                    0x00000002
#define DT_VCENTER                  0x00000004
#define DT_BOTTOM                   0x00000008
#define DT_WORDBREAK                0x00000010
#define DT_SINGLELINE               0x00000020
#define DT_EXPANDTABS               0x00000040
#define DT_TABSTOP                  0x00000080
#define DT_NOCLIP                   0x00000100
#define DT_EXTERNALLEADING          0x00000200
#define DT_CALCRECT                 0x00000400
#define DT_NOPREFIX                 0x00000800
#define DT_INTERNAL                 0x00001000

#define DT_EDITCONTROL              0x00002000
#define DT_PATH_ELLIPSIS            0x00004000
#define DT_END_ELLIPSIS             0x00008000
#define DT_MODIFYSTRING             0x00010000
#define DT_RTLREADING               0x00020000
#define DT_WORD_ELLIPSIS            0x00040000
#define DT_NOFULLWIDTHCHARBREAK     0x00080000
#define DT_HIDEPREFIX               0x00100000
#define DT_PREFIXONLY               0x00200000
};

LongFailIfZero [gle] DrawTextA(HDC           hDC,
                               LPCSTR        lpString,
                               int           nCount,
                               [out] LPRECT  lpRect,
                               DrawTextFlags uFormat);

LongFailIfZero [gle] DrawTextW(HDC           hDC,
                               LPCWSTR       lpString,
                               int           nCount,
                               [out] LPRECT  lpRect,
                               DrawTextFlags uFormat);

typedef struct tagDRAWTEXTPARAMS
{
    UINT    cbSize;
    int     iTabLength;
    int     iLeftMargin;
    int     iRightMargin;
    UINT    uiLengthDrawn;
} DRAWTEXTPARAMS, *LPDRAWTEXTPARAMS;

LongFailIfZero [gle] DrawTextExA(HDC              hDC,
                                 [out] LPSTR      lpString,
                                 int              nCount,
                                 [out] LPRECT     lpRect,
                                 DrawTextFlags    uFormat,
                                 LPDRAWTEXTPARAMS lpDrawTextParams);

LongFailIfZero [gle] DrawTextExW(HDC              hDC,
                                 [out] LPWSTR     lpString,
                                 int              nCount,
                                 [out] LPRECT     lpRect,
                                 DrawTextFlags    uFormat,
                                 LPDRAWTEXTPARAMS lpDrawTextParams);

DwordFailIfZero [gle] GetTabbedTextExtentA(HDC    hDC,
                                           LPCSTR lpString,
                                           int    nCount,
                                           int    nTabPositions,
                                           int*   lpnTabStopPositions);

DwordFailIfZero [gle] GetTabbedTextExtentW(HDC     hDC,
                                           LPCWSTR lpString,
                                           int     nCount,
                                           int     nTabPositions,
                                           int*    lpnTabStopPositions);

DwordFailIfZero [gle] TabbedTextOutA(HDC     hDC,
                                     int     X,
                                     int     Y,
                                     LPCSTR  lpString,
                                     int     nCount,
                                     int     nTabPositions,
                                     int*    lpnTabStopPositions,
                                     int     nTabOrigin);

DwordFailIfZero [gle] TabbedTextOutW(HDC     hDC,
                                     int     X,
                                     int     Y,
                                     LPCWSTR lpString,
                                     int     nCount,
                                     int     nTabPositions,
                                     int*    lpnTabStopPositions,
                                     int     nTabOrigin);



typedef struct tagALTTABINFO
{
    DWORD cbSize;
    int   cItems;
    int   cColumns;
    int   cRows;
    int   iColFocus;
    int   iRowFocus;
    int   cxItem;
    int   cyItem;
    POINT ptStart;
} ALTTABINFO, *PALTTABINFO, *LPALTTABINFO;

FailOnFalse [gle] GetAltTabInfoA(HWND              hwnd,
                                 int               iItem,
                                 [out] PALTTABINFO pati,
                                 [out] LPSTR       pszItemText,
                                 UINT              cchItemText);

FailOnFalse [gle] GetAltTabInfoW(HWND              hwnd,
                                 int               iItem,
                                 [out] PALTTABINFO pati,
                                 [out] LPWSTR      pszItemText,
                                 UINT              cchItemText);

LongFailIfZero [gle] RealGetWindowClassA(HWND  hwnd, [out] LPSTR pszType, UINT cchType);
LongFailIfZero [gle] RealGetWindowClassW(HWND  hwnd, [out] LPWSTR pszType, UINT cchType);

DWORD GetQueueStatus(QueueStates flags);

FailOnFalse [gle] BlockInput(BOOL fBlockIt);

// not detailed
// export SendInput;

mask DWORD KeybdEventFlags
{
#define KEYEVENTF_EXTENDEDKEY 0x0001
#define KEYEVENTF_KEYUP       0x0002
#define KEYEVENTF_UNICODE     0x0004
#define KEYEVENTF_SCANCODE    0x0008
};

VOID keybd_event(VirtualKey bVk, BYTE bScan, KeybdEventFlags dwFlags, DWORD dwExtraInfo);

mask DWORD MouseEventFlags
{
#define MOUSEEVENTF_MOVE        0x0001
#define MOUSEEVENTF_LEFTDOWN    0x0002
#define MOUSEEVENTF_LEFTUP      0x0004
#define MOUSEEVENTF_RIGHTDOWN   0x0008
#define MOUSEEVENTF_RIGHTUP     0x0010
#define MOUSEEVENTF_MIDDLEDOWN  0x0020
#define MOUSEEVENTF_MIDDLEUP    0x0040
#define MOUSEEVENTF_XDOWN       0x0080
#define MOUSEEVENTF_XUP         0x0100
#define MOUSEEVENTF_WHEEL       0x0800
#define MOUSEEVENTF_VIRTUALDESK 0x4000
#define MOUSEEVENTF_ABSOLUTE    0x8000
};

VOID mouse_event(MouseEventFlags dwFlags, DWORD dx, DWORD dy, DWORD dwData, DWORD dwExtraInfo);


BOOL GetInputState();

mask DWORD DrawCaptionFlags
{
#define DC_ACTIVE           0x0001
#define DC_SMALLCAP         0x0002
#define DC_ICON             0x0004
#define DC_TEXT             0x0008
#define DC_INBUTTON         0x0010
#define DC_GRADIENT         0x0020
};

FailOnFalse [gle] DrawCaption(HWND hwnd, HDC hdc, LPRECT lprc, DrawCaptionFlags dwFlags);

FailOnFalse [gle] DrawAnimatedRects(HWND   hwnd,
                                    int    idAni,
                                    LPRECT lprcFrom,
                                    LPRECT lprcTo);

mask DWORD BorderStyle
{
#define BDR_RAISEDOUTER 0x0001
#define BDR_SUNKENOUTER 0x0002
#define BDR_RAISEDINNER 0x0004
#define BDR_SUNKENINNER 0x0008
};

mask DWORD BorderType
{
#define BF_LEFT         0x0001
#define BF_TOP          0x0002
#define BF_RIGHT        0x0004
#define BF_BOTTOM       0x0008

#define BF_DIAGONAL     0x0010

#define BF_MIDDLE       0x0800
#define BF_SOFT         0x1000
#define BF_ADJUST       0x2000
#define BF_FLAT         0x4000
#define BF_MONO         0x8000
};

FailOnFalse [gle] DrawEdge(HDC          hdc,
                           [out] LPRECT lprc,
                           BorderStyle  edge,
                           BorderType   grfFlags);

value DWORD DrawFrameControlType
{
#define DFC_CAPTION             1
#define DFC_MENU                2
#define DFC_SCROLL              3
#define DFC_BUTTON              4
#define DFC_POPUPMENU           5
};

mask DWORD DrawFrameControlState
{
#define DFCS_INACTIVE           0x0100
#define DFCS_PUSHED             0x0200
#define DFCS_CHECKED            0x0400

#define DFCS_TRANSPARENT        0x0800
#define DFCS_HOT                0x1000

#define DFCS_ADJUSTRECT         0x2000
#define DFCS_FLAT               0x4000
#define DFCS_MONO               0x8000
};

FailOnFalse [gle] DrawFrameControl(HDC                   hDC,
                                   [out] LPRECT          lprc,
                                   DrawFrameControlType  type,
                                   DrawFrameControlState state);

FailOnFalse [gle] DrawIcon(HDC hDC, int X, int Y, HRSRC hIcon);

value DWORD DrawIconExFlags
{
#define DI_MASK         0x0001
#define DI_IMAGE        0x0002
#define DI_NORMAL       0x0003
#define DI_COMPAT       0x0004
#define DI_DEFAULTSIZE  0x0008
};


FailOnFalse [gle] DrawIconEx(HDC             hdc,
                             int             xLeft,
                             int             yTop,
                             HRSRC           hIcon,
                             int             cxWidth,
                             int             cyWidth,
                             UINT            istepIfAniCur,
                             HRSRC           hbrFlickerFreeDraw,
                             DrawIconExFlags diFlags);

typedef LPVOID DRAWSTATEPROC;

mask DWORD DrawStateFlags
{
#define DSS_NORMAL      0x0000
#define DSS_UNION       0x0010
#define DSS_DISABLED    0x0020
#define DSS_MONO        0x0080
#define DSS_HIDEPREFIX  0x0200
#define DSS_PREFIXONLY  0x0400
#define DSS_RIGHT       0x8000
};

FailOnFalse [gle] DrawStateA(HDC            hDC,
                             HRSRC          hBrush,
                             DRAWSTATEPROC  pfnOutput,
                             LPARAM         lParam,
                             WPARAM         wParam,
                             int            x,
                             int            y,
                             int            cx,
                             int            cy,
                             DrawStateFlags flags);

FailOnFalse [gle] DrawStateW(HDC            hDC,
                             HRSRC          hBrush,
                             DRAWSTATEPROC  pfnOutput,
                             LPARAM         lParam,
                             WPARAM         wParam,
                             int            x,
                             int            y,
                             int            cx,
                             int            cy,
                             DrawStateFlags flags);

typedef LPVOID GRAYSTRINGPROC;

FailOnFalse [gle] GrayStringA(HDC            hDC,
                              HRSRC          hBrush,
                              GRAYSTRINGPROC lpOutputFunc,
                              LPARAM         lpData,
                              int            nCount,
                              int            X,
                              int            Y,
                              int            nWidth,
                              int            nHeight);

FailOnFalse [gle] GrayStringW(HDC            hDC,
                              HRSRC          hBrush,
                              GRAYSTRINGPROC lpOutputFunc,
                              LPARAM         lpData,
                              int            nCount,
                              int            X,
                              int            Y,
                              int            nWidth,
                              int            nHeight);

FailOnFalse [gle] CreateCaret(HWND  hWnd,
                              HRSRC hBitmap,
                              int   nWidth,
                              int   nHeight);

FailOnFalse [gle] DestroyCaret();

FailOnFalse [gle] SetCaretBlinkTime(UINT uMSeconds);

LongFailIfZero [gle] GetCaretBlinkTime();

FailOnFalse [gle] SetCaretPos(int X, int Y);

FailOnFalse [gle] GetCaretPos([out] LPPOINT lpPoint);

FailOnFalse [gle] ShowCaret(HWND hWnd);

FailOnFalse [gle] HideCaret(HWND hWnd);

// WindowStation APIs

mask DWORD WinstaAccessMask
{
#define WINSTA_ENUMDESKTOPS         0x0001
#define WINSTA_READATTRIBUTES       0x0002
#define WINSTA_ACCESSCLIPBOARD      0x0004
#define WINSTA_CREATEDESKTOP        0x0008
#define WINSTA_WRITEATTRIBUTES      0x0010
#define WINSTA_ACCESSGLOBALATOMS    0x0020
#define WINSTA_EXITWINDOWS          0x0040
#define WINSTA_ENUMERATE            0x0100
#define WINSTA_READSCREEN           0x0200
};

HWINSTA [gle] CreateWindowStationA(LPCSTR                lpwinsta,
                                   DWORD                 dwReserved,
                                   WinstaAccessMask      dwDesiredAccess,
                                   LPSECURITY_ATTRIBUTES lpsa);

HWINSTA [gle] CreateWindowStationW(LPCWSTR               lpwinsta,
                                   DWORD                 dwReserved,
                                   WinstaAccessMask      dwDesiredAccess,
                                   LPSECURITY_ATTRIBUTES lpsa);

FailOnFalse [gle] CloseWindowStation([da] HWINSTA hWinSta);

typedef LPVOID WINSTAENUMPROCA;
typedef LPVOID WINSTAENUMPROCW;

FailOnFalse [gle] EnumWindowStationsA(WINSTAENUMPROCA lpEnumFunc, LPARAM lParam);
FailOnFalse [gle] EnumWindowStationsW(WINSTAENUMPROCW lpEnumFunc, LPARAM lParam);

HWINSTA [gle] GetProcessWindowStation();

FailOnFalse [gle] SetProcessWindowStation(HWINSTA hWinSta);

HWINSTA [gle] OpenWindowStationA(LPCSTR lpszWinSta, BOOL fInherit, WinstaAccessMask dwDesiredAccess);
HWINSTA [gle] OpenWindowStationW(LPCWSTR lpszWinSta, BOOL fInherit, WinstaAccessMask dwDesiredAccess);

// scrollbar

value DWORD ScrollWhich
{
#define SB_HORZ             0
#define SB_VERT             1
#define SB_CTL              2
#define SB_BOTH             3
};

mask DWORD ScrollEnableFlags
{
#define ESB_ENABLE_BOTH     0x0000
#define ESB_DISABLE_LEFT    0x0001
#define ESB_DISABLE_RIGHT   0x0002
};

FailOnFalse [gle] EnableScrollBar(HWND hWnd, ScrollWhich wSBflags, ScrollEnableFlags wArrows);

mask DWORD ScrollInfoFlags
{
#define SIF_RANGE           0x0001
#define SIF_PAGE            0x0002
#define SIF_POS             0x0004
#define SIF_DISABLENOSCROLL 0x0008
#define SIF_TRACKPOS        0x0010
};

typedef struct tagSCROLLINFO
{
    UINT            cbSize;
    ScrollInfoFlags fMask;
    int             nMin;
    int             nMax;
    UINT            nPage;
    int             nPos;
    int             nTrackPos;
} SCROLLINFO, *LPSCROLLINFO;

int SetScrollInfo(HWND hWnd, ScrollWhich nWhich, LPSCROLLINFO lpScrollInfo, BOOL fRedraw);

LongFailIfZero [gle] SetScrollPos(HWND hWnd, ScrollWhich nBar, int nPos, BOOL bRedraw);

FailOnFalse [gle] SetScrollRange(HWND        hWnd,
                                 ScrollWhich nBar,
                                 int         nMinPos,
                                 int         nMaxPos,
                                 BOOL        bRedraw);

typedef struct tagSCROLLBARINFO
{
    DWORD cbSize;
    RECT  rcScrollBar;
    int   dxyLineButton;
    int   xyThumbTop;
    int   xyThumbBottom;
    int   reserved;
    DWORD rgstate[6];
} SCROLLBARINFO, *PSCROLLBARINFO, *LPSCROLLBARINFO;

FailOnFalse [gle] GetScrollBarInfo(HWND hwnd, SystemObjectId idObject, [out] PSCROLLBARINFO psbi);

FailOnFalse [gle] GetScrollInfo(HWND hwnd, ScrollWhich nBar, [out] LPSCROLLINFO lpScrollInfo);

LongFailIfZero [gle] GetScrollPos(HWND hWnd, ScrollWhich nBar);

FailOnFalse [gle] GetScrollRange(HWND        hWnd,
                                 ScrollWhich nBar,
                                 [out] int*  lpMinPos,
                                 [out] int*  lpMaxPos);

FailOnFalse [gle] ShowScrollBar(HWND hWnd, ScrollWhich nBar, BOOL bShow);

typedef struct tagCOMBOBOXINFO
{
    DWORD cbSize;
    RECT  rcItem;
    RECT  rcButton;
    DWORD stateButton;
    HWND  hwndCombo;
    HWND  hwndItem;
    HWND  hwndList;
} COMBOBOXINFO, *PCOMBOBOXINFO, *LPCOMBOBOXINFO;

FailOnFalse [gle] GetComboBoxInfo(HWND hwndCombo, [out] PCOMBOBOXINFO pcbi);

int GetListBoxInfo(HWND hwnd);

typedef struct tagTITLEBARINFO
{
    DWORD cbSize;
    RECT  rcTitleBar;
    DWORD rgstate[6];
} TITLEBARINFO, *PTITLEBARINFO, *LPTITLEBARINFO;

BOOL GetTitleBarInfo(HWND hwnd, [out] PTITLEBARINFO pti);

typedef LPVOID TIMERPROC;

DwordFailIfZero [gle] SetTimer(HWND hWnd, DWORD nIDEvent, int uElapse, TIMERPROC lpTimerFunc);

FailOnFalse [gle] KillTimer(HWND hWnd, DWORD uIDEvent);

FailOnFalse [gle] SetDoubleClickTime(int nTimeout);

int GetDoubleClickTime();

LPARAM SetMessageExtraInfo(LPARAM lParam);

mask DWORD TrackMouseEventFlags
{
#define TME_HOVER       0x00000001
#define TME_LEAVE       0x00000002
#define TME_NONCLIENT   0x00000010
#define TME_QUERY       0x40000000
#define TME_CANCEL      0x80000000
};

typedef struct tagTRACKMOUSEEVENT {
    DWORD                cbSize;
    TrackMouseEventFlags dwFlags;
    HWND                 hwndTrack;
    DWORD                dwHoverTime;
} TRACKMOUSEEVENT, *LPTRACKMOUSEEVENT;


// BUGBUG: this is [in] and [out]
FailOnFalse [gle] TrackMouseEvent([out] LPTRACKMOUSEEVENT lpEventTrack);

BOOL SwapMouseButton(BOOL fSwap);

LongFailIfZero [gle] MapWindowPoints(HWND          hWndFrom,
                                     HWND          hWndTo,
                                     [out] LPPOINT lpPoints,
                                     int           cPoints);


category User32StringExports:

LongFailIfZero [gle] LoadStringA(HINSTANCE hInstance, UINT uID, [out] LPSTR lpBuffer, int nBufferMax);
LongFailIfZero [gle] LoadStringW(HINSTANCE hInstance, UINT uID, [out] LPWSTR lpBuffer, int nBufferMax);

#include "clipboard.h"
#include "hook.h"
