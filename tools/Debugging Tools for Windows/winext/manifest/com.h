category ComponentObjectModel:

interface IUnknown
{
    HRESULT QueryInterface(
        [IID] REFIID iid,
        [out] COM_INTERFACE_PTR* ppvObject );

    ULONG AddRef();
    ULONG Release();
};

typedef IUnknown* LPUNKNOWN;

interface IClassFactory : IUnknown
{
    HRESULT CreateInstance(
        IUnknown * pUnkOuter,
        [IID] REFIID riid,
        [out] COM_INTERFACE_PTR* ppvObject );

    HRESULT LockServer( BOOL fLock );
};

interface IDispatch : IUnknown
{
    HRESULT GetTypeInfoCount( UINT pctinfo  );

    HRESULT GetTypeInfo(
        UINT iTInfo,
        LCID lcid,
        LPVOID ppTInfo );

    HRESULT GetIDsOfNames(
        REFIID riid,
        LPOLECHAR* rgszNames,
        UINT cNames,
        LCID lcid,
        [out] DISPID* rgDispId );

    HRESULT Invoke(
        DISPID  dispIdMember,
        REFIID  riid,
        LCID  lcid,
        WORD  wFlags,
        DISPPARAMS*  pDispParams,
        VARIANT*  pVarResult,
        EXCEPINFO*  pExcepInfo,
        UINT*  puArgErr );
};


interface IPersist : IUnknown
{
    HRESULT GetClassID(
        [out] CLSID *pClassID  //Pointer to CLSID of object
    );
};

interface IPersistFile : IPersist
{
    HRESULT IsDirty();

    HRESULT Load(
      LPCOLESTR pszFileName,
                    //Pointer to absolute path of the file to open
      DWORD dwMode  //Specifies the access mode from the STGM enumeration
    );

    HRESULT Save(
      LPCOLESTR pszFileName,   //Pointer to absolute path of the file
                               //where the object is saved
      BOOL fRemember           //Specifies whether the file is to be the
                               //current working file or not
    );

    HRESULT SaveCompleted(
      LPCOLESTR pszFileName  //Pointer to absolute path of the file
                             //where the object was saved
    );

    HRESULT GetCurFile(
      LPOLESTR *ppszFileName  //Pointer to the path for the current file
                              //or the default save prompt
    );
};

typedef VOID            DVTARGETDEVICE;
typedef LPVOID          CONTINUEPROC;
typedef LPVOID          IAdviseSink;

interface IViewObject : IUnknown
{


    HRESULT Draw
    (
        [in] DWORD dwDrawAspect,
        [in] LONG lindex,
        [in] void * pvAspect,
        [in] DVTARGETDEVICE *ptd,
        [in] HDC hdcTargetDev,
        [in] HDC hdcDraw,
        [in] LPCRECTL lprcBounds,
        [in] LPCRECTL lprcWBounds,
        [in] CONTINUEPROC ContinueProc,
        [in] ULONG_PTR dwContinue
    );

    HRESULT GetColorSet
    (
        [in] DWORD dwDrawAspect,
        [in] LONG lindex,
        [in] void *pvAspect,
        [in] DVTARGETDEVICE *ptd,
        [in] HDC hicTargetDev,
        [out] LOGPALETTE **ppColorSet
    );

    HRESULT Freeze
    (
        [in] DWORD dwDrawAspect,
        [in] LONG lindex,
        [in] void *pvAspect,
        [out] DWORD *pdwFreeze
    );

    HRESULT Unfreeze
    (
        [in] DWORD dwFreeze
    );

    HRESULT SetAdvise
    (
        [in] DWORD aspects,
        [in] DWORD advf,
        [in] IAdviseSink *pAdvSink
    );

    HRESULT GetAdvise
    (
        [out] DWORD *pAspects,
        [out] DWORD *pAdvf,
        [out] IAdviseSink **ppAdvSink
    );
};

interface IViewObject2 : IViewObject
{
    HRESULT GetExtent
    (
        [in]  DWORD dwDrawAspect,
        [in]  LONG lindex,
        [in]  DVTARGETDEVICE* ptd,
        [out] LPSIZEL lpsizel
    );
};

typedef struct tagExtentInfo {
    ULONG   cb;
    DWORD   dwExtentMode;
    SIZEL   sizelProposed;
} DVEXTENTINFO;

interface IViewObjectEx : IViewObject2
{

    HRESULT GetRect(
                [in] DWORD dwAspect,
                [out] LPRECTL pRect
            );

    HRESULT GetViewStatus(
                [out] DWORD * pdwStatus
            );

    HRESULT QueryHitPoint(
                [in] DWORD dwAspect,
                [in] LPCRECT pRectBounds,
                [in] POINT ptlLoc,
                [in] LONG lCloseHint,
                [out] DWORD * pHitResult
            );

    HRESULT QueryHitRect(
                [in] DWORD dwAspect,
                [in] LPCRECT pRectBounds,
                [in] LPCRECT pRectLoc,
                [in] LONG lCloseHint,
                [out] DWORD * pHitResult
            );

    HRESULT GetNaturalExtent (
                [in] DWORD dwAspect,
                [in] LONG lindex,
                [in] DVTARGETDEVICE * ptd,
                [in] HDC hicTargetDev,
                [in] DVEXTENTINFO * pExtentInfo,
                [out] LPSIZEL pSizel
            );
};

// We can't log IMalloc because it is a global object.
// It causes recursion problems in LogProcessHook because
// StringFromCLSID uses the global interface.
/*
interface IMalloc : IUnknown
{
    PVOID   Alloc( SIZE_T cb );

    PVOID   Realloc(
        PVOID pv,
        SIZE_T cb );

    VOID    Free( PVOID pv);

    SIZE_T  GetSize( PVOID pv );

    int     DidAlloc( PVOID pv );

    VOID    HeapMinimize();

};
*/
