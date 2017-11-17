
category Printing:
module WINSPOOL.DRV:

mask DWORD PrinterEnum
{
#define PRINTER_ENUM_DEFAULT     0x00000001
#define PRINTER_ENUM_LOCAL       0x00000002
#define PRINTER_ENUM_CONNECTIONS 0x00000004
#define PRINTER_ENUM_FAVORITE    0x00000004
#define PRINTER_ENUM_NAME        0x00000008
#define PRINTER_ENUM_REMOTE      0x00000010
#define PRINTER_ENUM_SHARED      0x00000020
#define PRINTER_ENUM_NETWORK     0x00000040

#define PRINTER_ENUM_EXPAND      0x00004000
#define PRINTER_ENUM_CONTAINER   0x00008000

#define PRINTER_ENUM_ICON1       0x00010000
#define PRINTER_ENUM_ICON2       0x00020000
#define PRINTER_ENUM_ICON3       0x00040000
#define PRINTER_ENUM_ICON4       0x00080000
#define PRINTER_ENUM_ICON5       0x00100000
#define PRINTER_ENUM_ICON6       0x00200000
#define PRINTER_ENUM_ICON7       0x00400000
#define PRINTER_ENUM_ICON8       0x00800000
#define PRINTER_ENUM_HIDE        0x01000000
};

FailOnFalse [gle] EnumPrintersA(PrinterEnum   Flags,        // printer object types
                                LPSTR         Name,         // name of printer object
                                int           Level,        // information level
                                LPBYTE        pPrinterEnum, // printer information buffer
                                int           cbBuf,        // size of printer information buffer
                                [out] int*    pcbNeeded,    // bytes received or required
                                [out] int*    pcReturned);  // number of printers enumerated

FailOnFalse [gle] EnumPrintersW(PrinterEnum   Flags,        // printer object types
                                LPWSTR        Name,         // name of printer object
                                int           Level,        // information level
                                LPBYTE        pPrinterEnum, // printer information buffer
                                int           cbBuf,        // size of printer information buffer
                                [out] int*    pcbNeeded,    // bytes received or required
                                [out] int*    pcReturned);  // number of printers enumerated

FailOnFalse [gle] GetPrinterA(HPRINTER      hPrinter,
                              int           Level,
                              [out] LPBYTE  pPrinter,
                              int           cbBuf,
                              [out] int*    pcbNeeded);

FailOnFalse [gle] GetPrinterW(HPRINTER      hPrinter,
                              int           Level,
                              [out] LPBYTE  pPrinter,
                              int           cbBuf,
                              [out] int*    pcbNeeded);

mask DWORD PrinterAccessMode
{
#define PRINTER_ACCESS_ADMINISTER   0x00000004
#define PRINTER_ACCESS_USE          0x00000008
#define STANDARD_RIGHTS_REQUIRED    0x000F000

#define DELETE                      0x00010000
#define READ_CONTROL                0x00020000
#define WRITE_DAC                   0x00040000
#define WRITE_OWNER                 0x00080000
#define SYNCHRONIZE                 0x00100000
};

value DWORD SetPrinterCommand
{
#define PRINTER_CONTROL_PAUSE            1
#define PRINTER_CONTROL_RESUME           2
#define PRINTER_CONTROL_PURGE            3
#define PRINTER_CONTROL_SET_STATUS       4
};

FailOnFalse [gle] SetPrinterA(HPRINTER          hPrinter,
                              int               Level,
                              LPBYTE            pPrinter,
                              SetPrinterCommand Command);

FailOnFalse [gle] SetPrinterW(HPRINTER          hPrinter,
                              int               Level,
                              LPBYTE            pPrinter,
                              SetPrinterCommand Command);

HPRINTER [gle] AddPrinterA(LPSTR   pName,
                           int     Level,
                           LPBYTE  pPrinter);

HPRINTER [gle] AddPrinterW(LPWSTR  pName,
                           int     Level,
                           LPBYTE  pPrinter);

FailOnFalse [gle] AbortPrinter(HPRINTER hPrinter);

FailOnFalse [gle] AddFormA(HPRINTER hPrinter,
                           int      Level,
                           LPBYTE   pForm);

FailOnFalse [gle] AddFormW(HPRINTER hPrinter,
                           int      Level,
                           LPBYTE   pForm);

typedef struct _ADDJOB_INFO_1A {
    LPSTR   Path;
    DWORD   JobId;
} ADDJOB_INFO_1A, *LPADDJOB_INFO_1A;

typedef struct _ADDJOB_INFO_1W {
    LPWSTR  Path;
    DWORD   JobId;
} ADDJOB_INFO_1W, *LPADDJOB_INFO_1W;

FailOnFalse [gle] AddJobA(HPRINTER              hPrinter,
                          int                   Level,
                          [out] ADDJOB_INFO_1A* pData,
                          int                   cbBuf,
                          [out] LPDWORD         pcbNeeded);

FailOnFalse [gle] AddJobW(HPRINTER              hPrinter,
                          int                   Level,
                          [out] ADDJOB_INFO_1W* pData,
                          int                   cbBuf,
                          [out] LPDWORD         pcbNeeded);

typedef struct _MONITOR_INFO_2A{
    LPSTR     pName;
    LPSTR     pEnvironment;
    LPSTR     pDLLName;
} MONITOR_INFO_2A, *LPMONITOR_INFO_2A;

typedef struct _MONITOR_INFO_2W{
    LPWSTR    pName;
    LPWSTR    pEnvironment;
    LPWSTR    pDLLName;
} MONITOR_INFO_2W, *LPMONITOR_INFO_2W;

FailOnFalse [gle] AddMonitorA(LPSTR             pName,
                              int               Level,
                              MONITOR_INFO_2A*  pMonitors);

FailOnFalse [gle] AddMonitorW(LPSTR             pName,
                              int               Level,
                              MONITOR_INFO_2A*  pMonitors);

FailOnFalse [gle] AddPortA(LPSTR   pName,
                           HWND    hWnd,
                           LPSTR   pMonitorName);

FailOnFalse [gle] AddPortW(LPWSTR  pName,
                           HWND    hWnd,
                           LPWSTR  pMonitorName);

FailOnFalse [gle] AddPrinterConnectionA(LPSTR pName);
FailOnFalse [gle] AddPrinterConnectionW(LPWSTR pName);

FailOnFalse [gle] AddPrinterDriverA(LPSTR        pName,
                                    int          Level,
                                    [out] LPBYTE pDriverInfo);

FailOnFalse [gle] AddPrinterDriverW(LPWSTR       pName,
                                    int          Level,
                                    [out] LPBYTE pDriverInfo);

mask DWORD AddPrinterDriverExFlags
{
#define APD_STRICT_UPGRADE               0x00000001
#define APD_STRICT_DOWNGRADE             0x00000002
#define APD_COPY_ALL_FILES               0x00000004
#define APD_COPY_NEW_FILES               0x00000008
#define APD_COPY_FROM_DIRECTORY          0x00000010
};

FailOnFalse [gle] AddPrinterDriverExA(LPSTR                     pName,
                                      int                       Level,
                                      [out] LPBYTE              pDriverInfo,
                                      AddPrinterDriverExFlags   dwFileCopyFlags);

FailOnFalse [gle] AddPrinterDriverExW(LPWSTR                    pName,
                                      int                       Level,
                                      [out] LPBYTE              pDriverInfo,
                                      AddPrinterDriverExFlags   dwFileCopyFlags);

FailOnFalse [gle] AddPrintProcessorA(LPSTR  pName,
                                     LPSTR  pEnvironment,
                                     LPSTR  pPathName,
                                     LPSTR  pPrintProcessorName);

FailOnFalse [gle] AddPrintProcessorW(LPWSTR pName,
                                     LPWSTR pEnvironment,
                                     LPWSTR pPathName,
                                     LPWSTR pPrintProcessorName);

FailOnFalse [gle] AddPrintProvidorA(LPSTR  pName,
                                    int    level,
                                    LPBYTE pProvidorInfo);

FailOnFalse [gle] AddPrintProvidorW(LPWSTR pName,
                                    int    level,
                                    LPBYTE pProvidorInfo);

FailOnFalse [gle] AdvancedDocumentPropertiesA(HWND              hWnd,
                                              HPRINTER          hPrinter,
                                              LPSTR             pDeviceName,
                                              [out] PDEVMODEA   pDevModeOutput,
                                              PDEVMODEA         pDevModeInput);

FailOnFalse [gle] AdvancedDocumentPropertiesW(HWND              hWnd,
                                              HPRINTER          hPrinter,
                                              LPWSTR            pDeviceName,
                                              [out] PDEVMODEW   pDevModeOutput,
                                              PDEVMODEW         pDevModeInput);

FailOnFalse [gle] ClosePrinter([da] HPRINTER hPrinter);

FailOnFalse [gle] ConfigurePortA(LPSTR   pName,
                                 HWND    hWnd,
                                 LPSTR   pPortName);

FailOnFalse [gle] ConfigurePortW(LPWSTR  pName,
                                 HWND    hWnd,
                                 LPWSTR  pPortName);

HPRINTER ConnectToPrinterDlg(HWND    hwnd,
                             DWORD   Flags);

FailOnFalse [gle] DeleteFormA(HPRINTER hPrinter,
                              LPSTR    pFormName);

FailOnFalse [gle] DeleteFormW(HPRINTER hPrinter,
                              LPWSTR   pFormName);

FailOnFalse [gle] DeleteMonitorA(LPSTR   pName,
                                 LPSTR   pEnvironment,
                                 LPSTR   pMonitorName);

FailOnFalse [gle] DeleteMonitorW(LPWSTR  pName,
                                 LPWSTR  pEnvironment,
                                 LPWSTR  pMonitorName);

FailOnFalse [gle] DeletePortA(LPSTR  pName,
                              HWND   hWnd,
                              LPSTR  pPortName);

FailOnFalse [gle] DeletePortW(LPWSTR pName,
                              HWND   hWnd,
                              LPWSTR pPortName);

FailOnFalse [gle] DeletePrinter(HPRINTER hPrinter);

FailOnFalse [gle] DeletePrinterConnectionA(LPSTR  pName);
FailOnFalse [gle] DeletePrinterConnectionW(LPWSTR pName);

FailOnFalse [gle] DeletePrinterDataA(HPRINTER hPrinter,
                                     LPSTR    pValueName);

FailOnFalse [gle] DeletePrinterDataW(HPRINTER hPrinter,
                                     LPWSTR   pValueName);

FailOnFalse [gle] DeletePrinterDataExA(HPRINTER hPrinter,
                                       LPSTR    pKeyName,
                                       LPSTR    pValueName);

FailOnFalse [gle] DeletePrinterDataExW(HPRINTER hPrinter,
                                       LPWSTR   pKeyName,
                                       LPWSTR   pValueName);

FailOnFalse [gle] DeletePrinterDriverA(LPSTR  pName,
                                       LPSTR  pEnvironment,
                                       LPSTR  pDriverName);

FailOnFalse [gle] DeletePrinterDriverW(LPWSTR pName,
                                       LPWSTR pEnvironment,
                                       LPWSTR pDriverName);
mask DWORD DPDFlags
{
// FLAGS for DeletePrinterDriverEx.
#define DPD_DELETE_UNUSED_FILES          0x00000001
#define DPD_DELETE_SPECIFIC_VERSION      0x00000002
#define DPD_DELETE_ALL_FILES             0x00000004
};

FailOnFalse [gle] DeletePrinterDriverExA(LPSTR    pName,
                                         LPSTR    pEnvironment,
                                         LPSTR    pDriverName,
                                         DPDFlags dwDeleteFlag,
                                         DWORD    dwVersionFlag);

FailOnFalse [gle] DeletePrinterDriverExW(LPWSTR   pName,
                                         LPWSTR   pEnvironment,
                                         LPWSTR   pDriverName,
                                         DPDFlags dwDeleteFlag,
                                         DWORD    dwVersionFlag);

WinError DeletePrinterKeyA(HPRINTER hPrinter, LPSTR  pKeyName);
WinError DeletePrinterKeyW(HPRINTER hPrinter, LPWSTR pKeyName);

FailOnFalse [gle] DeletePrintProcessorA(LPSTR  pName,
                                        LPSTR  pEnvironment,
                                        LPSTR  pPrintProcessorName);

FailOnFalse [gle] DeletePrintProcessorW(LPWSTR pName,
                                        LPWSTR pEnvironment,
                                        LPWSTR pPrintProcessorName);

FailOnFalse [gle] DeletePrintProvidorA(LPSTR  pName,
                                       LPSTR  pEnvironment,
                                       LPSTR  pPrintProvidorName);

FailOnFalse [gle] DeletePrintProvidorW(LPWSTR pName,
                                       LPWSTR pEnvironment,
                                       LPWSTR pPrintProvidorName);

mask DWORD DPFlags
{
#define DM_IN_BUFFER        8
#define DM_IN_PROMPT        4
#define DM_OUT_BUFFER       2
#define DM_OUT_DEFAULT      1
};

LongFailIfNeg1 [gle] DocumentPropertiesA(HWND            hWnd,
                                         HPRINTER        hPrinter,
                                         LPSTR           pDeviceName,
                                         [out] PDEVMODEA pDevModeOutput,
                                         PDEVMODEA       pDevModeInput,
                                         DPFlags         fMode);

LongFailIfNeg1 [gle] DocumentPropertiesW(HWND            hWnd,
                                         HPRINTER        hPrinter,
                                         LPWSTR          pDeviceName,
                                         [out] PDEVMODEW pDevModeOutput,
                                         PDEVMODEW       pDevModeInput,
                                         DPFlags         fMode);

FailOnFalse [gle] EndDocPrinter(HPRINTER hPrinter);

FailOnFalse [gle] EndPagePrinter(HPRINTER hPrinter);


mask DWORD FormInfoFlags
{
#define FORM_USER       0x00000000
#define FORM_BUILTIN    0x00000001
#define FORM_PRINTER    0x00000002
};

typedef struct _FORM_INFO_1A {
    FormInfoFlags Flags;
    LPSTR         pName;
    SIZE          Size;
    RECT          ImageableArea;
} FORM_INFO_1A, *LPFORM_INFO_1A, *PFORM_INFO_1A;

typedef struct _FORM_INFO_1W {
    FormInfoFlags Flags;
    LPWSTR        pName;
    SIZE          Size;
    RECT          ImageableArea;
} FORM_INFO_1W, *LPFORM_INFO_1W, *PFORM_INFO_1W;

FailOnFalse [gle] EnumFormsA(HPRINTER               hPrinter,
                             DWORD                  Level,
                             [out] LPFORM_INFO_1A   pForm,
                             int                    cbBuf,
                             [out] int*             pcbNeeded,
                             [out] int*             pcReturned);

FailOnFalse [gle] EnumFormsW(HPRINTER               hPrinter,
                             DWORD                  Level,
                             [out] LPFORM_INFO_1W   pForm,
                             int                    cbBuf,
                             [out] int*             pcbNeeded,
                             [out] int*             pcReturned);

FailOnFalse [gle] EnumJobsA(HPRINTER     hPrinter,
                            int          FirstJob,
                            int          NoJobs,
                            int          Level,
                            [out] LPBYTE pJob,
                            int          cbBuf,
                            [out] int*   pcbNeeded,
                            [out] int*   pcReturned);

FailOnFalse [gle] EnumJobsW(HPRINTER     hPrinter,
                            int          FirstJob,
                            int          NoJobs,
                            int          Level,
                            [out] LPBYTE pJob,
                            int          cbBuf,
                            [out] int*   pcbNeeded,
                            [out] int*   pcReturned);

FailOnFalse [gle] EnumMonitorsA(LPSTR        pName,
                                int          Level,
                                [out] LPBYTE pMonitors,
                                int          cbBuf,
                                [out] int*   pcbNeeded,
                                [out] int*   pcReturned);

FailOnFalse [gle] EnumMonitorsW(LPWSTR       pName,
                                int          Level,
                                [out] LPBYTE pMonitors,
                                int          cbBuf,
                                [out] int*   pcbNeeded,
                                [out] int*   pcReturned);

FailOnFalse [gle] EnumPortsA(LPSTR        pName,
                             int          Level,
                             [out] LPBYTE pPorts,
                             int          cbBuf,
                             [out] int*   pcbNeeded,
                             [out] int*   pcReturned);

FailOnFalse [gle] EnumPortsW(LPWSTR       pName,
                             int          Level,
                             [out] LPBYTE pPorts,
                             int          cbBuf,
                             [out] int*   pcbNeeded,
                             [out] int*   pcReturned);

FailOnFalse [gle] EnumPrinterDataA(HPRINTER             hPrinter,
                                   int                  dwIndex,
                                   [out] LPSTR          pValueName,
                                   int                  cbValueName,
                                   [out] int*           pcbValueName,
                                   [out] RegistryType*  pType,
                                   [out] LPBYTE         pData,
                                   int                  cbData,
                                   [out] int*           pcbData);

FailOnFalse [gle] EnumPrinterDataW(HPRINTER             hPrinter,
                                   int                  dwIndex,
                                   [out] LPWSTR         pValueName,
                                   int                  cbValueName,
                                   [out] int*           pcbValueName,
                                   [out] RegistryType*  pType,
                                   [out] LPBYTE         pData,
                                   int                  cbData,
                                   [out] int*           pcbData);

FailOnFalse [gle] EnumPrinterDataExA(HPRINTER       hPrinter,
                                     LPSTR          pKeyName,
                                     [out] LPBYTE   pEnumValues,
                                     int            cbEnumValues,
                                     [out] int*     pcbEnumValues,
                                     [out] int*     pnEnumValues);

FailOnFalse [gle] EnumPrinterDataExW(HPRINTER       hPrinter,
                                     LPWSTR         pKeyName,
                                     [out] LPBYTE   pEnumValues,
                                     int            cbEnumValues,
                                     [out] int*     pcbEnumValues,
                                     [out] int*     pnEnumValues);

FailOnFalse [gle] EnumPrinterDriversA(LPSTR         pName,
                                      LPSTR         pEnvironment,
                                      int           Level,
                                      [out] LPBYTE  pDriverInfo,
                                      int           cbBuf,
                                      [out] int*    pcbNeeded,
                                      [out] int*    pcReturned);

FailOnFalse [gle] EnumPrinterDriversW(LPWSTR        pName,
                                      LPWSTR        pEnvironment,
                                      int           Level,
                                      [out] LPBYTE  pDriverInfo,
                                      int           cbBuf,
                                      [out] int*    pcbNeeded,
                                      [out] int*    pcReturned);

WinError EnumPrinterKeyA(HPRINTER     hPrinter,
                         LPSTR        pKeyName,
                         [out] LPSTR  pSubkey,
                         int          cbSubkey,
                         [out] int*   pcbSubkey);

WinError EnumPrinterKeyW(HPRINTER     hPrinter,
                         LPWSTR       pKeyName,
                         [out] LPWSTR pSubkey,
                         int          cbSubkey,
                         [out] int*   pcbSubkey);

WinError GetPrinterDataA(HPRINTER            hPrinter,
                         LPSTR               pValueName,
                         [out] RegistryType* pType,
                         [out] LPBYTE        pData,
                         int                 nSize,
                         [out] int*          pcbNeeded);

WinError GetPrinterDataW(HPRINTER            hPrinter,
                         LPWSTR              pValueName,
                         [out] RegistryType* pType,
                         [out] LPBYTE        pData,
                         int                 nSize,
                         [out] int*          pcbNeeded);

WinError SetPrinterDataA(HPRINTER       hPrinter,
                         LPSTR          pValueName,
                         RegistryType   Type,
                         LPBYTE         pData,
                         int            cbData);

WinError SetPrinterDataW(HPRINTER       hPrinter,
                         LPWSTR         pValueName,
                         RegistryType   Type,
                         LPBYTE         pData,
                         int            cbData);

WinError SetPrinterDataExA(HPRINTER     hPrinter,
                           LPSTR        pKeyName,
                           LPSTR        pValueName,
                           RegistryType Type,
                           LPBYTE       pData,
                           int          cbData);

WinError SetPrinterDataExW(HPRINTER     hPrinter,
                           LPWSTR       pKeyName,
                           LPWSTR       pValueName,
                           RegistryType Type,
                           LPBYTE       pData,
                           int          cbData);

FailOnFalse [gle] GetDefaultPrinterA([out] LPSTR  pszBuffer,
                                     [out] int*   pcchBuffer);

FailOnFalse [gle] GetDefaultPrinterW([out] LPWSTR pszBuffer,
                                     [out] int*   pcchBuffer);

value DWORD DILevel
{
#define DRIVER_INFO_1       1
#define DRIVER_INFO_2       2
#define DRIVER_INFO_3       3
#define DRIVER_INFO_4       4
#define DRIVER_INFO_5       5
#define DRIVER_INFO_6       6
};

FailOnFalse [gle] GetPrinterDriverA( HPRINTER       hPrinter,
                                     LPSTR          pEnvironment,
                                     DILevel        Level,
                                     [out] LPBYTE   pDriverInfo,
                                     int            cbBuf,
                                     [out] LPDWORD  pcbNeeded);

FailOnFalse [gle] GetPrinterDriverW( HPRINTER       hPrinter,
                                     LPWSTR         pEnvironment,
                                     DILevel        Level,
                                     [out] LPBYTE   pDriverInfo,
                                     int            cbBuf,
                                     [out] LPDWORD  pcbNeeded);

FailOnFalse [gle] EnumPrintProcessorDatatypesA( LPSTR           pName,
                                                LPSTR           pPrintProcessorName,
                                                int             Level,
                                                [out] LPSTR     pDatatypes,
                                                int             cbBuf,
                                                [out] LPDWORD   pcbNeeded,
                                                [out] LPDWORD   pcReturned);

FailOnFalse [gle] EnumPrintProcessorDatatypesW( LPWSTR          pName,
                                                LPWSTR          pPrintProcessorName,
                                                int             Level,
                                                [out] LPWSTR    pDatatypes,
                                                int             cbBuf,
                                                [out] LPDWORD   pcbNeeded,
                                                [out] LPDWORD   pcReturned);

FailOnFalse [gle] EnumPrintProcessorsA( LPSTR          pName,
                                        LPSTR          pEnvironment,
                                        int            Level,
                                        [out] LPSTR    pPrintProcessorInfo,
                                        int            cbBuf,
                                        [out] LPDWORD  pcbNeeded,
                                        [out] LPDWORD  pcReturned);

FailOnFalse [gle] EnumPrintProcessorsW( LPWSTR         pName,
                                        LPWSTR         pEnvironment,
                                        int            Level,
                                        [out] LPWSTR   pPrintProcessorInfo,
                                        int            cbBuf,
                                        [out] LPDWORD  pcbNeeded,
                                        [out] LPDWORD  pcReturned);

FailOnFalse [gle] FindClosePrinterChangeNotification( HPRINTER hChange );

typedef struct _PRINTER_NOTIFY_OPTIONS_TYPE {
  WORD      Type;
  WORD      Reserved0;
  DWORD     Reserved1;
  DWORD     Reserved2;
  DWORD     Count;
  WORD      *pFields;
} PRINTER_NOTIFY_OPTIONS_TYPE, *PPRINTER_NOTIFY_OPTIONS_TYPE;

typedef struct _PRINTER_NOTIFY_OPTIONS {
  DWORD  Version;
  DWORD  Flags;
  DWORD  Count;
  PPRINTER_NOTIFY_OPTIONS_TYPE  pTypes;
} PRINTER_NOTIFY_OPTIONS, *PPRINTER_NOTIFY_OPTIONS;

mask DWORD PCFlags
{
#define PRINTER_CHANGE_ADD_PRINTER                  0x00000001
#define PRINTER_CHANGE_SET_PRINTER                  0x00000002
#define PRINTER_CHANGE_DELETE_PRINTER               0x00000004
#define PRINTER_CHANGE_FAILED_CONNECTION_PRINTER    0x00000008
#define PRINTER_CHANGE_PRINTER                      0x000000FF
#define PRINTER_CHANGE_ADD_JOB                      0x00000100
#define PRINTER_CHANGE_SET_JOB                      0x00000200
#define PRINTER_CHANGE_DELETE_JOB                   0x00000400
#define PRINTER_CHANGE_WRITE_JOB                    0x00000800
#define PRINTER_CHANGE_JOB                          0x0000FF00
#define PRINTER_CHANGE_ADD_FORM                     0x00010000
#define PRINTER_CHANGE_SET_FORM                     0x00020000
#define PRINTER_CHANGE_DELETE_FORM                  0x00040000
#define PRINTER_CHANGE_FORM                         0x00070000
#define PRINTER_CHANGE_ADD_PORT                     0x00100000
#define PRINTER_CHANGE_CONFIGURE_PORT               0x00200000
#define PRINTER_CHANGE_DELETE_PORT                  0x00400000
#define PRINTER_CHANGE_PORT                         0x00700000
#define PRINTER_CHANGE_ADD_PRINT_PROCESSOR          0x01000000
#define PRINTER_CHANGE_DELETE_PRINT_PROCESSOR       0x04000000
#define PRINTER_CHANGE_PRINT_PROCESSOR              0x07000000
#define PRINTER_CHANGE_ADD_PRINTER_DRIVER           0x10000000
#define PRINTER_CHANGE_SET_PRINTER_DRIVER           0x20000000
#define PRINTER_CHANGE_DELETE_PRINTER_DRIVER        0x40000000
#define PRINTER_CHANGE_PRINTER_DRIVER               0x70000000
#define PRINTER_CHANGE_TIMEOUT                      0x80000000
#define PRINTER_CHANGE_ALL                          0x7777FFFF
};

HPRINTER FindFirstPrinterChangeNotification( HPRINTER                hPrinter,
                                             PCFlags                 fdwFlags,
                                             int                     fdwOptions,
                                             PPRINTER_NOTIFY_OPTIONS pPrinterNotifyOptions);

FailOnFalse [gle] FindNextPrinterChangeNotification( HPRINTER       hChange,
                                                     PDWORD         pdwChange,
                                                     LPVOID         pPrinterNotifyOptions,
                                                     [out] LPVOID   *ppPrinterNotifyInfo);

FailOnFalse [gle] FlushPrinter( HPRINTER       hPrinter,
                                LPVOID         pBuf,
                                int            cbBuf,
                                [out] LPDWORD  pcWritten,
                                DWORD          cSleep);

typedef struct _PRINTER_NOTIFY_INFO_DATA {
  WORD      Type;
  WORD      Field;
  DWORD     Reserved;
  DWORD     Id;
  int       cbBuf;
  LPVOID    pBuf;
} PRINTER_NOTIFY_INFO_DATA, *PPRINTER_NOTIFY_INFO_DATA;

typedef struct _PRINTER_NOTIFY_INFO {
  DWORD     Version;
  DWORD     Flags;
  DWORD     Count;
  PRINTER_NOTIFY_INFO_DATA  aData[1];
} PRINTER_NOTIFY_INFO, *PPRINTER_NOTIFY_INFO;

FailOnFalse [gle] FreePrinterNotifyInfo( PPRINTER_NOTIFY_INFO pPrinterNotifyInfo );

FailOnFalse [gle] GetFormA( HPRINTER            hPrinter,
                            LPSTR               pFormName,
                            int                 Level,
                            [out] PFORM_INFO_1A pForm,
                            int                 cbBuf,
                            [out] LPDWORD       pcbNeeded);

FailOnFalse [gle] GetFormW( HPRINTER            hPrinter,
                            LPWSTR              pFormName,
                            int                 Level,
                            [out] PFORM_INFO_1W pForm,
                            int                 cbBuf,
                            [out] LPDWORD       pcbNeeded);

FailOnFalse [gle] GetJobA( HPRINTER        hPrinter,
                          DWORD           JobId,
                          int             Level,
                          [out] LPBYTE    pJob,
                          int             cbBuf,
                          [out] LPDWORD   pcbNeeded);

FailOnFalse [gle] GetJobW( HPRINTER        hPrinter,
                          DWORD           JobId,
                          int             Level,
                          [out] LPBYTE    pJob,
                          int             cbBuf,
                          [out] LPDWORD   pcbNeeded);

FailOnFalse [gle] GetPrinterDriverDirectoryA( LPSTR         pName,
                                              LPSTR         pEnvironment,
                                              int           Level,
                                              [out] LPBYTE  pDriverDirectory,
                                              int           cbBuf,
                                              [out] LPDWORD pcbNeeded);

FailOnFalse [gle] GetPrinterDriverDirectoryW( LPWSTR        pName,
                                              LPWSTR        pEnvironment,
                                              int           Level,
                                              [out] LPBYTE  pDriverDirectory,
                                              int           cbBuf,
                                              [out] LPDWORD pcbNeeded);

FailOnFalse [gle] GetPrintProcessorDirectoryA( LPSTR         pName,
                                               LPSTR         pEnvironment,
                                               int           Level,
                                               [out] LPBYTE  pPrintProcessorInfo,
                                               int           cbBuf,
                                               [out] LPDWORD pcbNeeded);

FailOnFalse [gle] GetPrintProcessorDirectoryW( LPWSTR        pName,
                                               LPWSTR        pEnvironment,
                                               int           Level,
                                               [out] LPBYTE  pPrintProcessorInfo,
                                               int           cbBuf,
                                               [out] LPDWORD pcbNeeded);

typedef struct _PRINTER_DEFAULTSA{
    LPSTR             pDatatype;
    LPDEVMODEA        pDevMode;
    PrinterAccessMode DesiredAccess;
} PRINTER_DEFAULTSA, *PPRINTER_DEFAULTSA, *LPPRINTER_DEFAULTSA;

typedef struct _PRINTER_DEFAULTSW{
    LPWSTR            pDatatype;
    LPDEVMODEW        pDevMode;
    PrinterAccessMode DesiredAccess;
} PRINTER_DEFAULTSW, *PPRINTER_DEFAULTSW, *LPPRINTER_DEFAULTSW;

FailOnFalse [gle] OpenPrinterA(LPSTR               pPrinterName,
                               [out] HPRINTER*     phPrinter,
                               LPPRINTER_DEFAULTSA pDefault);

FailOnFalse [gle] OpenPrinterW(LPWSTR              pPrinterName,
                               [out] HPRINTER*     phPrinter,
                               LPPRINTER_DEFAULTSW pDefault);

FailOnFalse [gle] PrinterProperties( HWND       hWnd,
                                     HPRINTER   hPrinter);

FailOnFalse [gle] ReadPrinter( HPRINTER       hPrinter,
                               [out] LPVOID   pBuf,
                               int            cbBuf,
                               [out] LPDWORD  pNoBytesRead);

FailOnFalse [gle] ResetPrinterA( HPRINTER               hPrinter,
                                 PPRINTER_DEFAULTSA     pDefault);

FailOnFalse [gle] ResetPrinterW( HPRINTER               hPrinter,
                                 PPRINTER_DEFAULTSW     pDefault);

FailOnFalse [gle] ScheduleJob( HPRINTER     hPrinter,
                               DWORD        dwJobID);

FailOnFalse [gle] SetDefaultPrinterA( LPCSTR pszPrinter );

FailOnFalse [gle] SetDefaultPrinterW( LPCWSTR pszPrinter );

FailOnFalse [gle] SetFormA( HPRINTER        hPrinter,
                            LPSTR           pFormName,
                            int             Level,
                            PFORM_INFO_1A   pForm);

FailOnFalse [gle] SetFormW( HPRINTER        hPrinter,
                            LPWSTR          pFormName,
                            int             Level,
                            PFORM_INFO_1W   pForm);

FailOnFalse [gle] SetJobA( HPRINTER      hPrinter,
                           DWORD         JobId,
                           int           Level,
                           LPBYTE        pJob,
                           DWORD         Command);

FailOnFalse [gle] SetJobW( HPRINTER      hPrinter,
                           DWORD         JobId,
                           int           Level,
                           LPBYTE        pJob,
                           DWORD         Command);

typedef struct _PORT_INFO_3A {
  DWORD dwStatus;
  LPSTR pszStatus;
  DWORD dwSeverity;
} PORT_INFO_3A, *PPORT_INFO_3A;

typedef struct _PORT_INFO_3W {
  DWORD dwStatus;
  LPWSTR pszStatus;
  DWORD dwSeverity;
} PORT_INFO_3W, *PPORT_INFO_3W;

FailOnFalse [gle] SetPortA( LPSTR           pName,
                            LPSTR           pPortName,
                            DWORD           dwLevel,
                            PPORT_INFO_3A   pPortInfo);

FailOnFalse [gle] SetPortW( LPWSTR          pName,
                            LPWSTR          pPortName,
                            DWORD           dwLevel,
                            PPORT_INFO_3W   pPortInfo);

typedef struct _DOC_INFO_1A {
  LPSTR pDocName;
  LPSTR pOutputFile;
  LPSTR pDatatype;
} DOC_INFO_1A, *PDOC_INFO_1A;

typedef struct _DOC_INFO_1W {
  LPWSTR pDocName;
  LPWSTR pOutputFile;
  LPWSTR pDatatype;
} DOC_INFO_1W, *PDOC_INFO_1W;

FailOnFalse [gle] StartDocPrinterA( HPRINTER        hPrinter,
                                    int             Level,
                                    PDOC_INFO_1A    pDocInfo);

FailOnFalse [gle] StartDocPrinterW( HPRINTER        hPrinter,
                                    int             Level,
                                    PDOC_INFO_1W    pDocInfo);

FailOnFalse [gle] StartPagePrinter( HPRINTER hPrinter );

FailOnFalse [gle] WritePrinter( HPRINTER        hPrinter,
                                LPVOID          pBuf,
                                int             cbBuf,
                                [out] LPDWORD   pcWritten);


//
// The following functions are used to print
//


value DWORD DeviceCapabilitiesEnum
{
#define DC_FIELDS               1
#define DC_PAPERS               2
#define DC_PAPERSIZE            3
#define DC_MINEXTENT            4
#define DC_MAXEXTENT            5
#define DC_BINS                 6
#define DC_DUPLEX               7
#define DC_SIZE                 8
#define DC_EXTRA                9
#define DC_VERSION              10
#define DC_DRIVER               11
#define DC_BINNAMES             12
#define DC_ENUMRESOLUTIONS      13
#define DC_FILEDEPENDENCIES     14
#define DC_TRUETYPE             15
#define DC_PAPERNAMES           16
#define DC_ORIENTATION          17
#define DC_COPIES               18
#define DC_BINADJUST            19
#define DC_EMF_COMPLIANT        20
#define DC_DATATYPE_PRODUCED    21
#define DC_COLLATE              22
#define DC_MANUFACTURER         23
#define DC_MODEL                24
#define DC_PERSONALITY          25
#define DC_PRINTRATE            26
#define DC_PRINTRATEUNIT        27
#define DC_PRINTERMEM           28
#define DC_MEDIAREADY           29
#define DC_STAPLE               30
#define DC_PRINTRATEPPM         31
#define DC_COLORDEVICE          32
#define DC_NUP                  33
};

SpoolerError [gle] DeviceCapabilitiesA(LPSTR                  pszPrinterName,
                                       LPSTR                  pszPortName,
                                       DeviceCapabilitiesEnum capabilities,
                                       [out] LPSTR            pszOutput,
                                       DEVMODEA*              pDevMode);

SpoolerError [gle] DeviceCapabilitiesW(LPWSTR                 pszPrinterName,
                                       LPWSTR                 pszPortName,
                                       DeviceCapabilitiesEnum capabilities,
                                       [out] LPWSTR           pszOutput,
                                       DEVMODEW*              pDevMode);

module GDI32.DLL:

SpoolerError [gle] AbortDoc(HDC hdc);
SpoolerError [gle] EndDoc(HDC hdc);
SpoolerError [gle] EndPage(HDC hdc);


SpoolerError [gle] Escape(HDC           hdc,
                          GdiEscapeCode escapeCode,
                          int           cbSize,
                          LPSTR         pszInData,
                          [out] LPVOID  pOutData);

SpoolerError [gle] ExtEscape(HDC            hdc,
                             GdiEscapeCode  escapeCode,
                             int            cbInput,
                             LPSTR          pszInData,
                             int            cbOutput,
                             [out] LPSTR    lpszOutData);

typedef LPVOID ABORTPROC;

SpoolerError [gle] SetAbortProc(HDC       hdc,
                                ABORTPROC pfnAbort);

value DWORD DocInfoType
{
#define DI_NONE                     0x00000000
#define DI_APPBANDING               0x00000001
#define DI_ROPS_READ_DESTINATION    0x00000002
};

typedef struct _DOCINFOA {
    int      cbSize;
    LPCSTR   lpszDocName;
    LPCSTR   lpszOutput;
    LPCSTR   lpszDatatype;
    DocInfoType    fwType;
} DOCINFOA, *LPDOCINFOA;

typedef struct _DOCINFOW {
    int      cbSize;
    LPCWSTR  lpszDocName;
    LPCWSTR  lpszOutput;
    LPCWSTR  lpszDatatype;
    DocInfoType    fwType;
} DOCINFOW, *LPDOCINFOW;

SpoolerError [gle] StartDocA(HDC       hdc,
                             DOCINFOA* pDocInfo);

SpoolerError [gle] StartDocW(HDC       hdc,
                             DOCINFOW* pDocInfo);

SpoolerError [gle] StartPage(HDC hdc);
