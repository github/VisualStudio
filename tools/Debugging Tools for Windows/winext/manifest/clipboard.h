// +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
//
//                              Clipboard Functions
//
// +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

value UINT ClipboardFormats
{
#define CF_TEXT             1
#define CF_BITMAP           2
#define CF_METAFILEPICT     3
#define CF_SYLK             4
#define CF_DIF              5
#define CF_TIFF             6
#define CF_OEMTEXT          7
#define CF_DIB              8
#define CF_PALETTE          9
#define CF_PENDATA          10
#define CF_RIFF             11
#define CF_WAVE             12
#define CF_UNICODETEXT      13
#define CF_ENHMETAFILE      14
#define CF_HDROP            15
#define CF_LOCALE           16
#define CF_MAX              17
};

value INT ClipboardStatus
{
#define ClipboardEmpty                  0
#define ClipboardContainsUnknownFormat  -1
};

category Clipboard:

BOOL ChangeClipboardChain(
                          HWND hWndRemove,
                          HWND hWndNewNext);

FailOnFalse [gle] CloseClipboard();

LongFailIfZero [gle] CountClipboardFormats();

FailOnFalse [gle] EmptyClipboard();

LongFailIfZero [gle] EnumClipboardFormats(ClipboardFormats format);

HANDLE [gle] GetClipboardData(ClipboardFormats uFormat);

LongFailIfZero [gle] GetClipboardFormatNameA(UINT        format,
                                             [out] LPSTR lpszFormatName,
                                             int         cchMaxCount);

LongFailIfZero [gle] GetClipboardFormatNameW(UINT         format,
                                             [out] LPWSTR lpszFormatName,
                                             int          cchMaxCount);

DwordFailIfZero [gle] GetClipboardOwner();

DWORD GetClipboardSequenceNumber();

DwordFailIfZero [gle] GetClipboardViewer();

DwordFailIfZero [gle] GetOpenClipboardWindow();

ClipboardStatus [gle] GetPriorityClipboardFormat(ClipboardFormats* paFormatPriorityList,
                                                 int               cFormats);

BOOL [gle] IsClipboardFormatAvailable(ClipboardFormats format);

FailOnFalse [gle] OpenClipboard(HWND hWndNewOwner);

LongFailIfZero [gle] RegisterClipboardFormatA(LPCSTR lpszFormat);

LongFailIfZero [gle] RegisterClipboardFormatW(LPCWSTR lpszFormat);

HANDLE [gle] SetClipboardData(ClipboardFormats uFormat,
                              HANDLE           hMem);

HWND [gle] SetClipboardViewer(HWND hWndNewViewer);
