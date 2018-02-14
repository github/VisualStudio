// +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
//
//                              Hook Functions
//
// +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

typedef struct tagCWPSTRUCT {
    LPARAM  lParam;
    WPARAM  wParam;
    UINT    message;
    HWND    hwnd;
} CWPSTRUCT, *PCWPSTRUCT;

typedef struct tagCWPRETSTRUCT {
    LRESULT lResult;
    LPARAM  lParam;
    WPARAM  wParam;
    UINT    message;
    HWND    hwnd;
} CWPRETSTRUCT, *PCWPRETSTRUCT;

value INT CBTHookCodes
{
#define HCBT_MOVESIZE       0
#define HCBT_MINMAX         1
#define HCBT_QS             2
#define HCBT_CREATEWND      3
#define HCBT_DESTROYWND     4
#define HCBT_ACTIVATE       5
#define HCBT_CLICKSKIPPED   6
#define HCBT_KEYSKIPPED     7
#define HCBT_SYSCOMMAND     8
#define HCBT_SETFOCUS       9
};

value WPARAM HookProcType
{
#define WH_CALLWNDPROC      4
#define WH_CALLWNDPROCRET  12
#define WH_CBT              5
#define WH_DEBUG            9
#define WH_FOREGROUNDIDLE  11
#define WH_GETMESSAGE       3
#define WH_JOURNALPLAYBACK  1
#define WH_JOURNALRECORD    0
#define WH_KEYBOARD         2
#define WH_KEYBOARD_LL     13
#define WH_MOUSE            7
#define WH_MOUSE_LL        14
#define WH_MSGFILTER       -1
#define WH_SHELL           10
#define WH_SYSMSGFILTER     6
};

typedef struct tagDEBUGHOOKINFO {
    DWORD  idThread;
    DWORD  idThreadInstaller;
    LPARAM lParam;
    WPARAM wParam;
    INT    code;
} DEBUGHOOKINFO, *PDEBUGHOOKINFO;

typedef struct tagEVENTMSG {
    UINT  message;
    UINT  paramL;
    UINT  paramH;
    DWORD time;
    HWND  hwnd;
} EVENTMSG, *PEVENTMSG;

value INT HookCode
{
#define HC_GETNEXT          1
#define HC_SKIP             2
#define HC_NOREMOVE         3
#define HC_SYSMODALON       4
#define HC_SYSMODALOFF      5
};

typedef struct tagKBDLLHOOKSTRUCT {
    DWORD     vkCode;
    DWORD     scanCode;
    DWORD     flags;
    DWORD     time;
    ULONG_PTR dwExtraInfo;
} KBDLLHOOKSTRUCT, *PKBDLLHOOKSTRUCT;

typedef struct tagMSLLHOOKSTRUCT {
    POINT     pt;
    DWORD     mouseData;
    DWORD     flags;
    DWORD     time;
    ULONG_PTR dwExtraInfo;
} MSLLHOOKSTRUCT, *PMSLLHOOKSTRUCT;

value INT InputWhichGeneratedMessage
{
#define MSGF_DDEMGR                 0x8001
#define MSGF_DIALOGBOX              0
#define MSGF_MENU                   2
#define MSGF_SCROLLBAR              5
};

typedef struct tagMOUSEHOOKSTRUCT {
    POINT     pt;
    HWND      hwnd;
    UINT      wHitTestCode;
    ULONG_PTR dwExtraInfo;
} MOUSEHOOKSTRUCT, *PMOUSEHOOKSTRUCT;

value INT ShellProcHookCode
{
#define HSHELL_ACCESSIBILITYSTATE   11
#define HSHELL_ACTIVATESHELLWINDOW  3
#define HSHELL_GETMINRECT           5
#define HSHELL_LANGUAGE             8
#define HSHELL_REDRAW               6
#define HSHELL_TASKMAN              7
#define HSHELL_WINDOWACTIVATED      4
#define HSHELL_WINDOWCREATED        1
#define HSHELL_WINDOWDESTROYED      2
};

category HookingFunctions:
module USER32.DLL:

BOOL CallMsgFilter(LPMSG lpMsg, int nCode);

LRESULT CallNextHookEx(HHOOK  hhk,
                       int    nCode,
                       WPARAM wParam,
                       LPARAM lParam);

HHOOK [gle] SetWindowsHookExA(HookProcType idHook,
                              HOOKPROC     lpfn,
                              HINSTANCE    hMod,
                              DWORD        dwThreadId);

HHOOK [gle] SetWindowsHookExW(HookProcType idHook,
                              HOOKPROC     lpfn,
                              HINSTANCE    hmod,
                              DWORD        dwThreadId);

FailOnFalse [gle] UnhookWindowsHookEx( [da] HHOOK hhk);
