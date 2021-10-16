module OLE32.DLL:
category ComponentObjectModel:

value DWORD CLSCTX
{
#define CLSCTX_INPROC_SERVER   1
#define CLSCTX_INPROC_HANDLER  2
#define CLSCTX_LOCAL_SERVER    4
#define CLSCTX_REMOTE_SERVER   16
};

typedef struct  _COAUTHIDENTITY
    {
    USHORT *User;
    ULONG UserLength;
    USHORT *Domain;
    ULONG DomainLength;
    USHORT *Password;
    ULONG PasswordLength;
    ULONG Flags;
    }	COAUTHIDENTITY;

typedef struct  _COAUTHINFO
    {
    DWORD dwAuthnSvc;
    DWORD dwAuthzSvc;
    LPWSTR pwszServerPrincName;
    DWORD dwAuthnLevel;
    DWORD dwImpersonationLevel;
    COAUTHIDENTITY *pAuthIdentityData;
    DWORD dwCapabilities;
    }	COAUTHINFO;

typedef struct  _COSERVERINFO
    {
    DWORD dwReserved1;
    LPWSTR pwszName;
    COAUTHINFO *pAuthInfo;
    DWORD dwReserved2;
    }	COSERVERINFO;

typedef struct  _COSERVERINFO2
    {
    DWORD dwFlags;
    LPWSTR pwszName;
    COAUTHINFO *pAuthInfo;
    IUnknown** ppCall;
    LPWSTR pwszCodeURL;
    DWORD dwFileVersionMS;
    DWORD dwFileVersionLS;
    LPWSTR pwszContentType;
    }	COSERVERINFO2;


STDAPI CoCreateInstance(
  REFCLSID rclsid,     //Class identifier (CLSID) of the object
  LPUNKNOWN pUnkOuter, //Pointer to controlling IUnknown
  CLSCTX dwClsContext,  //Context for running executable code
  [iid] REFIID riid,         //Reference to the identifier of the interface
  [out] COM_INTERFACE_PTR * ppv         //Address of output variable that receives
                       // the interface pointer requested in riid
);

HRESULT CoCreateInstanceEx(
  REFCLSID rclsid,             //CLSID of the object to be created
  IUnknown *punkOuter,         //If part of an aggregate, the
                               // controlling IUnknown
  CLSCTX dwClsCtx,              //CLSCTX values
  COSERVERINFO *pServerInfo,   //Machine on which the object is to
                               // be instantiated
  ULONG cmq,                   //Number of MULTI_QI structures in
                               // pResults
  PVOID *pResults           //Array of MULTI_QI structures
);

STDAPI CoGetClassObject(
  REFCLSID rclsid,  //CLSID associated with the class object
  CLSCTX dwClsContext,
                    //Context for running executable code
  COSERVERINFO * pServerInfo,
                    //Pointer to machine on which the object is to
                    // be instantiated
  [iid] REFIID riid,      //Reference to the identifier of the interface
  [out] COM_INTERFACE_PTR * ppv      //Address of output variable that receives the
                    // interface pointer requested in riid
);
