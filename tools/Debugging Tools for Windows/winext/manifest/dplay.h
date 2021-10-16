module DPLAYX.DLL:
category DirectPlay:

/*
 * GUIDS used by DirectPlay objects
 */
 class __declspec(uuid("D1EB6D20-8923-11d0-9D97-00A0C90A43CB"))  DirectPlay;
 class __declspec(uuid("2b74f7c0-9154-11cf-a9cd-00aa006886e3"))  IDirectPlay2;
 class __declspec(uuid("9d460580-a822-11cf-960c-0080c7534e82"))  IDirectPlay2A;
 class __declspec(uuid("133efe40-32dc-11d0-9cfb-00a0c90a43cb"))  IDirectPlay3;
 class __declspec(uuid("133efe41-32dc-11d0-9cfb-00a0c90a43cb"))  IDirectPlay3A;
 class __declspec(uuid("0ab1c530-4745-11d1-a7a1-0000f803abfc"))  IDirectPlay4;
 class __declspec(uuid("0ab1c531-4745-11d1-a7a1-0000f803abfc"))  IDirectPlay4A;

struct __declspec(uuid("685BC400-9D2C-11cf-A9CD-00AA006886E3")) DPSPGUID_IPX;
struct __declspec(uuid("36E95EE0-8577-11cf-960C-0080C7534E82")) DPSPGUID_TCPIP;
struct __declspec(uuid("0F1D6860-88D9-11cf-9C4E-00A0C905425E")) DPSPGUID_SERIAL;
struct __declspec(uuid("44EAA760-CB68-11cf-9C4E-00A0C905425E")) DPSPGUID_MODEM;

typedef IUnknown           *LPDIRECTPLAY;
typedef IDirectPlay2       *LPDIRECTPLAY2;
typedef IDirectPlay2       *LPDIRECTPLAY2A;
typedef IDirectPlay2       IDirectPlay2A;

typedef IDirectPlay3       *LPDIRECTPLAY3;
typedef IDirectPlay3       *LPDIRECTPLAY3A;
typedef IDirectPlay3       IDirectPlay3A;

typedef IDirectPlay4       *LPDIRECTPLAY4;
typedef IDirectPlay4       *LPDIRECTPLAY4A;
typedef IDirectPlay4       IDirectPlay4A;

/*
 * DPID
 * DirectPlay player and group ID
 */
typedef DWORD DPID;
typedef DWORD *LPDPID;

/*
#define DPID_SYSMSG         0
#define DPID_ALLPLAYERS     0
#define DPID_SERVERPLAYER	1
#define DPID_UNKNOWN		0xFFFFFFFF
*/

mask DWORD DPlayObjectFlags
{
/*
 * This DirectPlay object is the session host.  If the host exits the
 * session, another application will become the host and receive a
 * DPSYS_HOST system message.
 */
#define DPCAPS_ISHOST               0x00000002

/*
 * The service provider bound to this DirectPlay object can optimize
 * group messaging.
 */
#define DPCAPS_GROUPOPTIMIZED       0x00000008

/*
 * The service provider bound to this DirectPlay object can optimize
 * keep alives (see DPSESSION_KEEPALIVE)
 */
#define DPCAPS_KEEPALIVEOPTIMIZED   0x00000010

/*
 * The service provider bound to this DirectPlay object can optimize
 * guaranteed message delivery.
 */
#define DPCAPS_GUARANTEEDOPTIMIZED  0x00000020

/*
 * This DirectPlay object supports guaranteed message delivery.
 */
#define DPCAPS_GUARANTEEDSUPPORTED  0x00000040

/*
 * This DirectPlay object supports digital signing of messages.
 */
#define DPCAPS_SIGNINGSUPPORTED     0x00000080

/*
 * This DirectPlay object supports encryption of messages.
 */
#define DPCAPS_ENCRYPTIONSUPPORTED  0x00000100

};


/*
 * DPCAPS
 * Used to obtain the capabilities of a DirectPlay object
 */
typedef struct _DPCAPS
{
    DWORD dwSize;               // Size of structure, in bytes
    DPlayObjectFlags dwFlags;              // DPCAPS_xxx flags
    DWORD dwMaxBufferSize;      // Maximum message size, in bytes,  for this service provider
    DWORD dwMaxQueueSize;       // Obsolete.
    DWORD dwMaxPlayers;         // Maximum players/groups (local + remote)
    DWORD dwHundredBaud;        // Bandwidth in 100 bits per second units;
                                // i.e. 24 is 2400, 96 is 9600, etc.
    DWORD dwLatency;            // Estimated latency; 0 = unknown
    DWORD dwMaxLocalPlayers;    // Maximum # of locally created players allowed
    DWORD dwHeaderLength;       // Maximum header length, in bytes, on messages
                                // added by the service provider
    DWORD dwTimeout;            // Service provider's suggested timeout value
                                // This is how long DirectPlay will wait for
                                // responses to system messages
} DPCAPS;
typedef DPCAPS *LPDPCAPS;

/*
 * LPCDPSESSIONDESC2
 * A constant pointer to DPSESSIONDESC2
 */
typedef DPSESSIONDESC2 *LPCDPSESSIONDESC2;

mask DWORD DPSESSION_Flags
{
/*
 * Applications cannot create new players in this session.
 */
#define DPSESSION_NEWPLAYERSDISABLED    0x00000001

/*
 * If the DirectPlay object that created the session, the host,
 * quits, then the host will attempt to migrate to another
 * DirectPlay object so that new players can continue to be created
 * and new applications can join the session.
 */
#define DPSESSION_MIGRATEHOST           0x00000004

/*
 * This flag tells DirectPlay not to set the idPlayerTo and idPlayerFrom
 * fields in player messages.  This cuts two DWORD's off the message
 * overhead.
 */
#define DPSESSION_NOMESSAGEID           0x00000008


/*
 * This flag tells DirectPlay to not allow any new applications to
 * join the session.  Applications already in the session can still
 * create new players.
 */
#define DPSESSION_JOINDISABLED          0x00000020

/*
 * This flag tells DirectPlay to detect when remote players
 * exit abnormally (e.g. their computer or modem gets unplugged)
 */
#define DPSESSION_KEEPALIVE             0x00000040

/*
 * This flag tells DirectPlay not to send a message to all players
 * when a players remote data changes
 */
#define DPSESSION_NODATAMESSAGES        0x00000080

/*
 * This flag indicates that the session belongs to a secure server
 * and needs user authentication
 */
#define DPSESSION_SECURESERVER          0x00000100

/*
 * This flag indicates that the session is private and requirs a password
 * for EnumSessions as well as Open.
 */
#define DPSESSION_PRIVATE               0x00000200

/*
 * This flag indicates that the session requires a password for joining.
 */
#define DPSESSION_PASSWORDREQUIRED      0x00000400

/*
 * This flag tells DirectPlay to route all messages through the server
 */
#define DPSESSION_MULTICASTSERVER		0x00000800

/*
 * This flag tells DirectPlay to only download information about the
 * DPPLAYER_SERVERPLAYER.
 */
#define DPSESSION_CLIENTSERVER			0x00001000

/*
 * This flag tells DirectPlay to use the protocol built into dplay
 * for reliability and statistics all the time.  When this bit is
 * set, only other sessions with this bit set can join or be joined.
 */
#define DPSESSION_DIRECTPLAYPROTOCOL	0x00002000

/*
 * This flag tells DirectPlay that preserving order of received
 * packets is not important, when using reliable delivery.  This
 * will allow messages to be indicated out of order if preceding
 * messages have not yet arrived.  Otherwise DPLAY will wait for
 * earlier messages before delivering later reliable messages.
 */
#define DPSESSION_NOPRESERVEORDER		0x00004000

};


/*
 * DPSESSIONDESC2
 * Used to describe the properties of a DirectPlay
 * session instance
 */
typedef struct _DPSESSIONDESC2
{
    DWORD   dwSize;             // Size of structure
    DWORD   dwFlags;            // DPSESSION_xxx flags
    GUID    guidInstance;       // ID for the session instance
    GUID    guidApplication;    // GUID of the DirectPlay application.
                                // GUID_NULL for all applications.
    DWORD   dwMaxPlayers;       // Maximum # players allowed in session
    DWORD   dwCurrentPlayers;   // Current # players in session (read only)
//    union
//    {                           // Name of the session
//        LPWSTR  lpszSessionName;    // Unicode
        LPSTR   lpszSessionNameA;   // ANSI
//    };
//    union
//    {                           // Password of the session (optional)
//        LPWSTR  lpszPassword;       // Unicode
        LPSTR   lpszPasswordA;      // ANSI
//    };
    DWORD   dwReserved1;        // Reserved for future MS use.
    DWORD   dwReserved2;
    DWORD   dwUser1;            // For use by the application
    DWORD   dwUser2;
    DWORD   dwUser3;
    DWORD   dwUser4;
} DPSESSIONDESC2;
typedef DPSESSIONDESC2 *LPDPSESSIONDESC2;

/*
 * DPNAME
 * Used to hold the name of a DirectPlay entity
 * like a player or a group
 */
typedef struct _DPNAME
{
    DWORD   dwSize;             // Size of structure
    DWORD   dwFlags;            // Not used. Must be zero.
//    union
//    {                           // The short or friendly name
//        LPWSTR  lpszShortName;  // Unicode
        LPSTR   lpszShortNameA; // ANSI
//    };
//    union
//    {                           // The long or formal name
//        LPWSTR  lpszLongName;   // Unicode
        LPSTR   lpszLongNameA;  // ANSI
//    };

} DPNAME;
typedef DPNAME *LPDPNAME;

/*
 * LPCDPNAME
 * A constant pointer to DPNAME
 */
typedef DPNAME *LPCDPNAME;

/*
 * DPCREDENTIALS
 * Used to hold the user name and password of a DirectPlay user
 */
typedef struct _DPCREDENTIALS
{
    DWORD dwSize;               // Size of structure
    DWORD dwFlags;              // Not used. Must be zero.
//    union
//    {                           // User name of the account
//        LPWSTR  lpszUsername;   // Unicode
        LPSTR   lpszUsernameA;  // ANSI
//    };
//    union
//    {                           // Password of the account
//        LPWSTR  lpszPassword;   // Unicode
        LPSTR   lpszPasswordA;  // ANSI
//    };
//    union
//    {                           // Domain name of the account
//        LPWSTR  lpszDomain;     // Unicode
        LPSTR   lpszDomainA;    // ANSI
//    };
} DPCREDENTIALS;
typedef DPCREDENTIALS *LPDPCREDENTIALS;

typedef DPCREDENTIALS *LPCDPCREDENTIALS;

/*
 * DPSECURITYDESC
 * Used to describe the security properties of a DirectPlay
 * session instance
 */
typedef struct _DPSECURITYDESC
{
    DWORD dwSize;                   // Size of structure
    DWORD dwFlags;                  // Not used. Must be zero.
//    union
//    {                               // SSPI provider name
//        LPWSTR  lpszSSPIProvider;   // Unicode
        LPSTR   lpszSSPIProviderA;  // ANSI
//    };
//    union
//    {                               // CAPI provider name
//        LPWSTR lpszCAPIProvider;    // Unicode
        LPSTR  lpszCAPIProviderA;   // ANSI
//    };
    DWORD dwCAPIProviderType;       // Crypto Service Provider type
    DWORD dwEncryptionAlgorithm;    // Encryption Algorithm type
} DPSECURITYDESC;
typedef DPSECURITYDESC *LPDPSECURITYDESC;

typedef DPSECURITYDESC *LPCDPSECURITYDESC;

/*
 * DPACCOUNTDESC
 * Used to describe a user membership account
 */
typedef struct _DPACCOUNTDESC
{
    DWORD dwSize;                   // Size of structure
    DWORD dwFlags;                  // Not used. Must be zero.
//    union
//    {                               // Account identifier
//        LPWSTR  lpszAccountID;      // Unicode
        LPSTR   lpszAccountIDA;     // ANSI
//    };
} DPACCOUNTDESC;
typedef DPACCOUNTDESC *LPDPACCOUNTDESC;

typedef DPACCOUNTDESC *LPCDPACCOUNTDESC;

/*
 * LPCGUID
 * A constant pointer to a guid
 */
typedef GUID *LPCGUID;

/****************************************************************************
 *
 * DPLCONNECTION flags
 *
 ****************************************************************************/
mask DWORD DPLCONNECTIONFlags
{

/*
 * This application should create a new session as
 * described by the DPSESIONDESC structure
 */
#define DPLCONNECTION_CREATESESSION				 0x00000002

/*
 * This application should join the session described by
 * the DPSESIONDESC structure with the lpAddress data
 */
#define DPLCONNECTION_JOINSESSION				0x00000001

};

/*
 * DPLCONNECTION
 * Used to hold all in the informaion needed to connect
 * an application to a session or create a session
 */
typedef struct _DPLCONNECTION
{
    DWORD               dwSize;             // Size of this structure
    DPLCONNECTIONFlags  dwFlags;            // Flags specific to this structure
    LPDPSESSIONDESC2    lpSessionDesc;      // Pointer to session desc to use on connect
    LPDPNAME            lpPlayerName;       // Pointer to Player name structure
    GUID                guidSP;             // GUID of the DPlay SP to use
    LPVOID              lpAddress;          // Address for service provider
    DWORD               dwAddressSize;      // Size of address data
} DPLCONNECTION;
typedef DPLCONNECTION *LPDPLCONNECTION;

/*
 * LPCDPLCONNECTION
 * A constant pointer to DPLCONNECTION
 */
typedef DPLCONNECTION *LPCDPLCONNECTION;

/*
 * DPCHAT
 * Used to hold the a DirectPlay chat message
 */
typedef struct _DPCHAT
{
    DWORD               dwSize;
    DWORD               dwFlags;
//    union
//    {                          // Message string
//        LPWSTR  lpszMessage;   // Unicode
        LPSTR   lpszMessageA;  // ANSI
//    };
} DPCHAT;
typedef DPCHAT * LPDPCHAT;

/*
 * SGBUFFER
 * Scatter Gather Buffer used for SendEx
 */
typedef struct _SGBUFFER
{
	UINT         len;       // length of buffer data
	//PUCHAR	     pData;		// pointer to buffer data
	CHAR *	     pData;		// pointer to buffer data
} SGBUFFER;
typedef SGBUFFER *PSGBUFFER;
typedef SGBUFFER *LPSGBUFFER;


value DWORD DPRESULT
{

/****************************************************************************
 *
 * DIRECTPLAY ERRORS
 *
 * Errors are represented by negative values and cannot be combined.
 *
 ****************************************************************************/
#define DP_OK                           0
#define DPERR_ALREADYINITIALIZED        0x88770005L		[fail]
#define DPERR_ACCESSDENIED              0x8877000AL		[fail]
#define DPERR_ACTIVEPLAYERS             0x88770014L		[fail]
#define DPERR_BUFFERTOOSMALL            0x8877001EL		[fail]
#define DPERR_CANTADDPLAYER             0x88770028L		[fail]
#define DPERR_CANTCREATEGROUP           0x88770032L		[fail]
#define DPERR_CANTCREATEPLAYER          0x8877003CL		[fail]
#define DPERR_CANTCREATESESSION         0x88770046L		[fail]
#define DPERR_CAPSNOTAVAILABLEYET       0x88770050L		[fail]
#define DPERR_EXCEPTION                 0x8877005AL		[fail]
#define DPERR_GENERIC                   0x80004005L		[fail]
#define DPERR_INVALIDFLAGS              0x88770078L		[fail]
#define DPERR_INVALIDOBJECT             0x88770082L		[fail]
#define DPERR_INVALIDPARAM              0x80070057L		[fail]
#define DPERR_INVALIDPARAMS             0x80070057L		[fail]
#define DPERR_INVALIDPLAYER             0x88770096L		[fail]
#define DPERR_INVALIDGROUP             	0x8877009BL		[fail]
#define DPERR_NOCAPS                    0x887700A0L		[fail]
#define DPERR_NOCONNECTION              0x887700AAL		[fail]
#define DPERR_NOMEMORY                  0x8007000EL		[fail]
#define DPERR_OUTOFMEMORY               0x8007000EL		[fail]
#define DPERR_NOMESSAGES                0x887700BEL		[fail]
#define DPERR_NONAMESERVERFOUND         0x887700C8L		[fail]
#define DPERR_NOPLAYERS                 0x887700D2L		[fail]
#define DPERR_NOSESSIONS                0x887700DCL		[fail]
#define DPERR_PENDING			0x8000000AL		[fail]
#define DPERR_SENDTOOBIG		0x887700E6L		[fail]
#define DPERR_TIMEOUT                   0x887700F0L		[fail]
#define DPERR_UNAVAILABLE               0x887700FAL		[fail]
#define DPERR_UNSUPPORTED               0x80004001L		[fail]
#define DPERR_BUSY                      0x8877010EL		[fail]
#define DPERR_USERCANCEL                0x88770118L		[fail]
#define DPERR_NOINTERFACE               0x80004002L		[fail]
#define DPERR_CANNOTCREATESERVER        0x88770122L		[fail]
#define DPERR_PLAYERLOST                0x8877012CL		[fail]
#define DPERR_SESSIONLOST               0x88770136L		[fail]
#define DPERR_UNINITIALIZED             0x88770140L		[fail]
#define DPERR_NONEWPLAYERS              0x8877013AL		[fail]
#define DPERR_INVALIDPASSWORD           0x88770154L		[fail]
#define DPERR_CONNECTING                0x8877015EL		[fail]
#define DPERR_CONNECTIONLOST            0x88770168L		[fail]
#define DPERR_UNKNOWNMESSAGE            0x88770172L		[fail]
#define DPERR_CANCELFAILED              0x8877017CL		[fail]
#define DPERR_INVALIDPRIORITY           0x88770186L		[fail]
#define DPERR_NOTHANDLED                0x88770190L		[fail]
#define DPERR_CANCELLED                 0x8877019AL		[fail]
#define DPERR_ABORTED                   0x887701A4L		[fail]


#define DPERR_BUFFERTOOLARGE            0x887703E8L		[fail]
#define DPERR_CANTCREATEPROCESS         0x887703F2L		[fail]
#define DPERR_APPNOTSTARTED             0x887703FCL		[fail]
#define DPERR_INVALIDINTERFACE          0x88770406L		[fail]
#define DPERR_NOSERVICEPROVIDER         0x88770410L		[fail]
#define DPERR_UNKNOWNAPPLICATION        0x8877041AL		[fail]
#define DPERR_NOTLOBBIED                0x8877042EL		[fail]
#define DPERR_SERVICEPROVIDERLOADED	0x88770438L		[fail]
#define DPERR_ALREADYREGISTERED		0x88770442L		[fail]
#define DPERR_NOTREGISTERED		0x8877044CL		[fail]

//
// Security related errors
//
#define DPERR_AUTHENTICATIONFAILED      0x887707D0L		[fail]
#define DPERR_CANTLOADSSPI              0x887707DAL		[fail]
#define DPERR_ENCRYPTIONFAILED          0x887707E4L		[fail]
#define DPERR_SIGNFAILED                0x887707EEL		[fail]
#define DPERR_CANTLOADSECURITYPACKAGE   0x887707F8L		[fail]
#define DPERR_ENCRYPTIONNOTSUPPORTED    0x88770802L		[fail]
#define DPERR_CANTLOADCAPI              0x8877080CL		[fail]
#define DPERR_NOTLOGGEDIN               0x88770816L		[fail]
#define DPERR_LOGONDENIED               0x88770820L		[fail]

};

/****************************************************************************
 *
 * Prototypes for DirectPlay callback functions
 *
 ****************************************************************************/

typedef LPVOID LPDPENUMSESSIONSCALLBACK2;
typedef LPVOID LPDPENUMPLAYERSCALLBACK2;
typedef LPVOID LPDPENUMDPCALLBACK;
typedef LPVOID LPDPENUMDPCALLBACKA;
typedef LPVOID LPDPENUMCONNECTIONSCALLBACK;


/****************************************************************************
 *
 * EnumConnections API flags
 *
 ****************************************************************************/
mask DWORD EnumConnectionsFlags
{
/*
 * Enumerate Service Providers
 */
#define DPCONNECTION_DIRECTPLAY      0x00000001

/*
 * Enumerate Lobby Providers
 */
#define DPCONNECTION_DIRECTPLAYLOBBY 0x00000002

};

/****************************************************************************
 *
 * EnumPlayers API flags
 *
 ****************************************************************************/
mask DWORD EnumPlayersMask
{
/*
 * Enumerate all players in the current session
 */
#define DPENUMPLAYERS_ALL           0x00000000
//#define DPENUMGROUPS_ALL            DPENUMPLAYERS_ALL


/*
 * Enumerate only local (created by this application) players
 * or groups
 */
#define DPENUMPLAYERS_LOCAL         0x00000008
//#define DPENUMGROUPS_LOCAL			DPENUMPLAYERS_LOCAL

/*
 * Enumerate only remote (non-local) players
 * or groups
 */
#define DPENUMPLAYERS_REMOTE        0x00000010
//#define DPENUMGROUPS_REMOTE			DPENUMPLAYERS_REMOTE

/*
 * Enumerate groups along with the players
 */
#define DPENUMPLAYERS_GROUP         0x00000020

/*
 * Enumerate players or groups in another session
 * (must supply lpguidInstance)
 */
#define DPENUMPLAYERS_SESSION       0x00000080
//#define DPENUMGROUPS_SESSION		DPENUMPLAYERS_SESSION

/*
 * Enumerate server players
 */
#define DPENUMPLAYERS_SERVERPLAYER  0x00000100

/*
 * Enumerate spectator players
 */
#define DPENUMPLAYERS_SPECTATOR     0x00000200

/*
 * Enumerate shortcut groups
 */
#define DPENUMGROUPS_SHORTCUT       0x00000400

/*
 * Enumerate staging area groups
 */
#define DPENUMGROUPS_STAGINGAREA    0x00000800
/*
 * Enumerate hidden groups
 */
#define DPENUMGROUPS_HIDDEN         0x00001000

/*
 * Enumerate the group's owner
 */
#define DPENUMPLAYERS_OWNER			0x00002000

};

/****************************************************************************
 *
 * CreatePlayer API flags
 *
 ****************************************************************************/

mask DWORD CreatePlayerFlags
{
/*
 * This flag indicates that this player should be designated
 * the server player. The app should specify this at CreatePlayer.
 */
#define DPPLAYER_SERVERPLAYER           0x00000100

/*
 * This flag indicates that this player should be designated
 * a spectator. The app should specify this at CreatePlayer.
 */
#define DPPLAYER_SPECTATOR              0x00000200

/*
 * This flag indicates that this player was created locally.
 * (returned from GetPlayerFlags)
 */
#define DPPLAYER_LOCAL                  0x00000008

/*
 * This flag indicates that this player is the group's owner
 * (Only returned in EnumGroupPlayers)
 */
#define DPPLAYER_OWNER                   0x00002000

};

/****************************************************************************
 *
 * CreateGroup API flags
 *
 ****************************************************************************/


mask DWORD CreateGroupFlags
{
/*
 * This flag indicates that the StartSession can be called on the group.
 * The app should specify this at CreateGroup, or CreateGroupInGroup.
 */
#define DPGROUP_STAGINGAREA             0x00000800

/*
 * This flag indicates that this group was created locally.
 * (returned from GetGroupFlags)
 */
#define DPGROUP_LOCAL                   0x00000008

/*
 * This flag indicates that this group was created hidden.
 */
#define DPGROUP_HIDDEN                   0x00001000
};

/****************************************************************************
 *
 * EnumSessions API flags
 *
 ****************************************************************************/

mask DWORD EnumSessionsFlags
{
/*
 * Enumerate sessions which can be joined
 */
#define DPENUMSESSIONS_AVAILABLE    0x00000001

/*
 * Enumerate all sessions even if they can't be joined.
 */
#define DPENUMSESSIONS_ALL          0x00000002


/*
 * Start an asynchronous enum sessions
 */
 #define DPENUMSESSIONS_ASYNC		0x00000010

/*
 * Stop an asynchronous enum sessions
 */
 #define DPENUMSESSIONS_STOPASYNC	0x00000020

/*
 * Enumerate sessions even if they require a password
 */
 #define DPENUMSESSIONS_PASSWORDREQUIRED	0x00000040

/*
 * Return status about progress of enumeration instead of
 * showing any status dialogs.
 */
 #define DPENUMSESSIONS_RETURNSTATUS 0x00000080
};

/****************************************************************************
 *
 * GetCaps and GetPlayerCaps API flags
 *
 ****************************************************************************/
mask DWORD GetCapsFlags
{
/*
 * The latency returned should be for guaranteed message sending.
 * Default is non-guaranteed messaging.
 */
#define DPGETCAPS_GUARANTEED        0x00000001

};

/****************************************************************************
 *
 * GetGroupData, GetPlayerData API flags
 * Remote and local Group/Player data is maintained separately.
 * Default is DPGET_REMOTE.
 *
 ****************************************************************************/

mask DWORD GetDataFlags
{
/*
 * Get the remote data (set by any DirectPlay object in
 * the session using DPSET_REMOTE)
 */
#define DPGET_REMOTE                0x00000000

/*
 * Get the local data (set by this DirectPlay object
 * using DPSET_LOCAL)
 */
#define DPGET_LOCAL                 0x00000001

};

/****************************************************************************
 *
 * Open API flags
 *
 ****************************************************************************/

mask DWORD OpenFlags
{
/*
 * Join the session that is described by the DPSESSIONDESC2 structure
 */
#define DPOPEN_JOIN                 0x00000001

/*
 * Create a new session as described by the DPSESSIONDESC2 structure
 */
#define DPOPEN_CREATE               0x00000002

/*
 * Return status about progress of open instead of showing
 * any status dialogs.
 */
#define DPOPEN_RETURNSTATUS		0x00000080L

};

/****************************************************************************
 *
 * Receive API flags
 * Default is DPRECEIVE_ALL
 *
 ****************************************************************************/
mask DWORD ReceiveFlags
{
/*
 * Get the first message in the queue
 */
#define DPRECEIVE_ALL               0x00000001

/*
 * Get the first message in the queue directed to a specific player
 */
#define DPRECEIVE_TOPLAYER          0x00000002

/*
 * Get the first message in the queue from a specific player
 */
#define DPRECEIVE_FROMPLAYER        0x00000004

/*
 * Get the message but don't remove it from the queue
 */
#define DPRECEIVE_PEEK              0x00000008

};

/****************************************************************************
 *
 * Send API flags
 *
 ****************************************************************************/
mask DWORD SendFlags
{

/*
 * Send the message using a guaranteed send method.
 * Default is non-guaranteed.
 */
#define DPSEND_GUARANTEED           0x00000001


/*
 * This flag is obsolete. It is ignored by DirectPlay
 */
#define DPSEND_HIGHPRIORITY         0x00000002

/*
 * This flag is obsolete. It is ignored by DirectPlay
 */
#define DPSEND_OPENSTREAM           0x00000008

/*
 * This flag is obsolete. It is ignored by DirectPlay
 */
#define DPSEND_CLOSESTREAM          0x00000010

/*
 * Send the message digitally signed to ensure authenticity.
 */
#define DPSEND_SIGNED               0x00000020

/*
 * Send the message with encryption to ensure privacy.
 */
#define DPSEND_ENCRYPTED            0x00000040

/*
 * The message is a lobby system message
 */
#define DPSEND_LOBBYSYSTEMMESSAGE	0x00000080


/*
 * Send message asynchronously, must check caps
 * before using this flag.  It is always provided
 * if the protocol flag is set.
 */
#define DPSEND_ASYNC				0x00000200

/*
 * When an message is completed, don't tell me.
 * by default the application is notified with a system message.
 */
#define DPSEND_NOSENDCOMPLETEMSG    0x00000400


/*
 * Maximum priority for sends available to applications
 */
#define DPSEND_MAX_PRIORITY         0x0000FFFF

};

/****************************************************************************
 *
 * SetGroupData, SetGroupName, SetPlayerData, SetPlayerName,
 * SetSessionDesc API flags.
 * Default is DPSET_REMOTE.
 *
 ****************************************************************************/

mask DWORD SetDataFlags
{
/*
 * Propagate the data to all players in the session
 */
#define DPSET_REMOTE                0x00000000

/*
 * Do not propagate the data to other players
 */
#define DPSET_LOCAL                 0x00000001

/*
 * Used with DPSET_REMOTE, use guaranteed message send to
 * propagate the data
 */
#define DPSET_GUARANTEED            0x00000002

};

/****************************************************************************
 *
 * GetMessageQueue API flags.
 * Default is DPMESSAGEQUEUE_SEND
 *
 ****************************************************************************/
mask DWORD GetMessageQueueFlags
{
/*
 * Get Send Queue - requires Service Provider Support
 */
#define DPMESSAGEQUEUE_SEND        	0x00000001

/*
 * Get Receive Queue
 */
#define DPMESSAGEQUEUE_RECEIVE      0x00000002

};

/****************************************************************************
 *
 * Connect API flags
 *
 ****************************************************************************/


/*
 * Start an asynchronous connect which returns status codes
 */
//#define DPCONNECT_RETURNSTATUS      0x00000080


/****************************************************************************
 *
 * DirectPlay system messages and message data structures
 *
 * All system message come 'From' player DPID_SYSMSG.  To determine what type
 * of message it is, cast the lpData from Receive to DPMSG_GENERIC and check
 * the dwType member against one of the following DPSYS_xxx constants. Once
 * a match is found, cast the lpData to the corresponding of the DPMSG_xxx
 * structures to access the data of the message.
 *
 ****************************************************************************/

value DWORD DirectPlayMessages
{
/*
 * A new player or group has been created in the session
 * Use DPMSG_CREATEPLAYERORGROUP.  Check dwPlayerType to see if it
 * is a player or a group.
 */
#define DPSYS_CREATEPLAYERORGROUP   0x0003

/*
 * A player has been deleted from the session
 * Use DPMSG_DESTROYPLAYERORGROUP
 */
#define DPSYS_DESTROYPLAYERORGROUP  0x0005

/*
 * A player has been added to a group
 * Use DPMSG_ADDPLAYERTOGROUP
 */
#define DPSYS_ADDPLAYERTOGROUP      0x0007

/*
 * A player has been removed from a group
 * Use DPMSG_DELETEPLAYERFROMGROUP
 */
#define DPSYS_DELETEPLAYERFROMGROUP 0x0021

/*
 * This DirectPlay object lost its connection with all the
 * other players in the session.
 * Use DPMSG_SESSIONLOST.
 */
#define DPSYS_SESSIONLOST           0x0031

/*
 * The current host has left the session.
 * This DirectPlay object is now the host.
 * Use DPMSG_HOST.
 */
#define DPSYS_HOST                  0x0101

/*
 * The remote data associated with a player or
 * group has changed. Check dwPlayerType to see
 * if it is a player or a group
 * Use DPMSG_SETPLAYERORGROUPDATA
 */
#define DPSYS_SETPLAYERORGROUPDATA  0x0102

/*
 * The name of a player or group has changed.
 * Check dwPlayerType to see if it is a player
 * or a group.
 * Use DPMSG_SETPLAYERORGROUPNAME
 */
#define DPSYS_SETPLAYERORGROUPNAME  0x0103

/*
 * The session description has changed.
 * Use DPMSG_SETSESSIONDESC
 */
#define DPSYS_SETSESSIONDESC        0x0104

/*
 * A group has been added to a group
 * Use DPMSG_ADDGROUPTOGROUP
 */
#define DPSYS_ADDGROUPTOGROUP      	0x0105

/*
 * A group has been removed from a group
 * Use DPMSG_DELETEGROUPFROMGROUP
 */
#define DPSYS_DELETEGROUPFROMGROUP 	0x0106

/*
 * A secure player-player message has arrived.
 * Use DPMSG_SECUREMESSAGE
 */
#define DPSYS_SECUREMESSAGE         0x0107

/*
 * Start a new session.
 * Use DPMSG_STARTSESSION
 */
#define DPSYS_STARTSESSION          0x0108

/*
 * A chat message has arrived
 * Use DPMSG_CHAT
 */
#define DPSYS_CHAT                  0x0109

/*
 * The owner of a group has changed
 * Use DPMSG_SETGROUPOWNER
 */
#define DPSYS_SETGROUPOWNER         0x010A

/*
 * An async send has finished, failed or been cancelled
 * Use DPMSG_SENDCOMPLETE
 */
#define DPSYS_SENDCOMPLETE          0x010d

};


value DWORD PlayerTypeValue
{
/*
 * Used in the dwPlayerType field to indicate if it applies to a group
 * or a player
 */
#define DPPLAYERTYPE_GROUP          0x00000000
#define DPPLAYERTYPE_PLAYER         0x00000001

};


/*
 * DPMSG_GENERIC
 * Generic message structure used to identify the message type.
 */
typedef struct _DPMSG_GENERIC
{
    DirectPlayMessages       dwType;         // Message type
} DPMSG_GENERIC;
typedef DPMSG_GENERIC *LPDPMSG_GENERIC;

/*
 * DPMSG_CREATEPLAYERORGROUP
 * System message generated when a new player or group
 * created in the session with information about it.
 */
typedef struct _DPMSG_CREATEPLAYERORGROUP
{
    DWORD       dwType;         // Message type
    PlayerTypeValue    dwPlayerType;   // Is it a player or group
    DPID        dpId;           // ID of the player or group
    DWORD       dwCurrentPlayers;   // current # players & groups in session
    LPVOID      lpData;         // pointer to remote data
    DWORD       dwDataSize;     // size of remote data
    DPNAME      dpnName;        // structure with name info
	// the following fields are only available when using
	// the IDirectPlay3 interface or greater
    DPID	    dpIdParent;     // id of parent group
	DWORD		dwFlags;		// player or group flags
} DPMSG_CREATEPLAYERORGROUP;
typedef DPMSG_CREATEPLAYERORGROUP *LPDPMSG_CREATEPLAYERORGROUP;

/*
 * DPMSG_DESTROYPLAYERORGROUP
 * System message generated when a player or group is being
 * destroyed in the session with information about it.
 */
typedef struct _DPMSG_DESTROYPLAYERORGROUP
{
    DWORD       dwType;         // Message type
    DWORD       dwPlayerType;   // Is it a player or group
    DPID        dpId;           // player ID being deleted
    LPVOID      lpLocalData;    // copy of players local data
    DWORD       dwLocalDataSize; // sizeof local data
    LPVOID      lpRemoteData;   // copy of players remote data
    DWORD       dwRemoteDataSize; // sizeof remote data
	// the following fields are only available when using
	// the IDirectPlay3 interface or greater
    DPNAME      dpnName;        // structure with name info
    DPID	    dpIdParent;     // id of parent group
	DWORD		dwFlags;		// player or group flags
} DPMSG_DESTROYPLAYERORGROUP;
typedef DPMSG_DESTROYPLAYERORGROUP *LPDPMSG_DESTROYPLAYERORGROUP;

/*
 * DPMSG_ADDPLAYERTOGROUP
 * System message generated when a player is being added
 * to a group.
 */
typedef struct _DPMSG_ADDPLAYERTOGROUP
{
    DWORD       dwType;         // Message type
    DPID        dpIdGroup;      // group ID being added to
    DPID        dpIdPlayer;     // player ID being added
} DPMSG_ADDPLAYERTOGROUP;
typedef DPMSG_ADDPLAYERTOGROUP *LPDPMSG_ADDPLAYERTOGROUP;

/*
 * DPMSG_DELETEPLAYERFROMGROUP
 * System message generated when a player is being
 * removed from a group
 */
typedef DPMSG_ADDPLAYERTOGROUP          DPMSG_DELETEPLAYERFROMGROUP;
typedef DPMSG_DELETEPLAYERFROMGROUP     *LPDPMSG_DELETEPLAYERFROMGROUP;

/*
 * DPMSG_ADDGROUPTOGROUP
 * System message generated when a group is being added
 * to a group.
 */
typedef struct _DPMSG_ADDGROUPTOGROUP
{
    DWORD       dwType;         // Message type
    DPID        dpIdParentGroup; // group ID being added to
    DPID        dpIdGroup;     // group ID being added
} DPMSG_ADDGROUPTOGROUP;
typedef  DPMSG_ADDGROUPTOGROUP *LPDPMSG_ADDGROUPTOGROUP;

/*
 * DPMSG_DELETEGROUPFROMGROUP
 * System message generated when a GROUP is being
 * removed from a group
 */
typedef DPMSG_ADDGROUPTOGROUP          DPMSG_DELETEGROUPFROMGROUP;
typedef DPMSG_DELETEGROUPFROMGROUP     *LPDPMSG_DELETEGROUPFROMGROUP;

/*
 * DPMSG_SETPLAYERORGROUPDATA
 * System message generated when remote data for a player or
 * group has changed.
 */
typedef struct _DPMSG_SETPLAYERORGROUPDATA
{
    DWORD       dwType;         // Message type
    DWORD       dwPlayerType;   // Is it a player or group
    DPID        dpId;           // ID of player or group
    LPVOID      lpData;         // pointer to remote data
    DWORD       dwDataSize;     // size of remote data
} DPMSG_SETPLAYERORGROUPDATA;
typedef DPMSG_SETPLAYERORGROUPDATA *LPDPMSG_SETPLAYERORGROUPDATA;

/*
 * DPMSG_SETPLAYERORGROUPNAME
 * System message generated when the name of a player or
 * group has changed.
 */
typedef struct _DPMSG_SETPLAYERORGROUPNAME
{
    DWORD       dwType;         // Message type
    DWORD       dwPlayerType;   // Is it a player or group
    DPID        dpId;           // ID of player or group
    DPNAME      dpnName;        // structure with new name info
} DPMSG_SETPLAYERORGROUPNAME;
typedef DPMSG_SETPLAYERORGROUPNAME *LPDPMSG_SETPLAYERORGROUPNAME;

/*
 * DPMSG_SETSESSIONDESC
 * System message generated when session desc has changed
 */
typedef struct _DPMSG_SETSESSIONDESC
{
    DWORD           dwType;     // Message type
    DPSESSIONDESC2  dpDesc;     // Session desc
} DPMSG_SETSESSIONDESC;
typedef DPMSG_SETSESSIONDESC *LPDPMSG_SETSESSIONDESC;

/*
 * DPMSG_HOST
 * System message generated when the host has migrated to this
 * DirectPlay object.
 *
 */
typedef DPMSG_GENERIC       DPMSG_HOST;
typedef DPMSG_HOST          *LPDPMSG_HOST;

/*
 * DPMSG_SESSIONLOST
 * System message generated when the connection to the session is lost.
 *
 */
typedef DPMSG_GENERIC       DPMSG_SESSIONLOST;
typedef DPMSG_SESSIONLOST   *LPDPMSG_SESSIONLOST;

/*
 * DPMSG_SECUREMESSAGE
 * System message generated when a player requests a secure send
 */
typedef struct _DPMSG_SECUREMESSAGE
{
    DWORD		dwType;         // Message Type
    DWORD		dwFlags;        // Signed/Encrypted
    DPID        dpIdFrom;       // ID of Sending Player
    LPVOID		lpData;         // Player message
    DWORD		dwDataSize;     // Size of player message
} DPMSG_SECUREMESSAGE;
typedef  DPMSG_SECUREMESSAGE *LPDPMSG_SECUREMESSAGE;

/*
 * DPMSG_STARTSESSION
 * System message containing all information required to
 * start a new session
 */
typedef struct _DPMSG_STARTSESSION
{
    DWORD              dwType;     // Message type
    LPDPLCONNECTION    lpConn;     // DPLCONNECTION structure
} DPMSG_STARTSESSION;
typedef DPMSG_STARTSESSION *LPDPMSG_STARTSESSION;

/*
 * DPMSG_CHAT
 * System message containing a chat message
 */
typedef struct _DPMSG_CHAT
{
    DWORD              	dwType;       	// Message type
    DWORD              	dwFlags;      	// Message flags
    DPID               	idFromPlayer; 	// ID of the Sending Player
    DPID               	idToPlayer;   	// ID of the To Player
    DPID               	idToGroup;    	// ID of the To Group
	LPDPCHAT 			lpChat;			// Pointer to a structure containing the chat message
} DPMSG_CHAT;
typedef DPMSG_CHAT *LPDPMSG_CHAT;

/*
 * DPMSG_SETGROUPOWNER
 * System message generated when the owner of a group has changed
 */
typedef struct _DPMSG_SETGROUPOWNER
{
    DWORD       dwType;         // Message type
    DPID        idGroup;        // ID of the group
    DPID        idNewOwner;     // ID of the player that is the new owner
    DPID        idOldOwner;     // ID of the player that used to be the owner
} DPMSG_SETGROUPOWNER;
typedef DPMSG_SETGROUPOWNER *LPDPMSG_SETGROUPOWNER;

/*
 * DPMSG_SENDCOMPLETE
 * System message generated when finished with an Async Send message
 *
 * NOTE SENDPARMS has an overlay for DPMSG_SENDCOMPLETE, don't
 *                change this message w/o changing SENDPARMS.
 */
typedef struct _DPMSG_SENDCOMPLETE
{
	DWORD 		dwType;
	DPID		idFrom;
	DPID		idTo;
	DWORD		dwFlags;
	DWORD		dwPriority;
	DWORD		dwTimeout;
	LPVOID		lpvContext;
	DWORD		dwMsgID;
	DPRESULT     hr;
	DWORD       dwSendTime;
} DPMSG_SENDCOMPLETE;
typedef DPMSG_SENDCOMPLETE *LPDPMSG_SENDCOMPLETE;



/****************************************************************************
 *
 * IDirectPlay2 (and IDirectPlay2A) Interface
 *
 ****************************************************************************/


interface IDirectPlay2 : IUnknown
{
    /*** IDirectPlay2 methods ***/
    DPRESULT AddPlayerToGroup(DPID idGroup, DPID idPlayer );
    DPRESULT Close();
    DPRESULT CreateGroup( LPDPID lpidGroup, LPDPNAME lpGroupName, LPVOID lpData, DWORD dwDataSize, CreateGroupFlags dwFlags);
    DPRESULT CreatePlayer( LPDPID lpidPlayer, LPDPNAME lpPlayerName, HANDLE hEvent, LPVOID lpData, DWORD dwDataSize, CreatePlayerFlags dwFlags);
    DPRESULT DeletePlayerFromGroup(DPID idGroup, DPID idPlayer );
    DPRESULT DestroyGroup( DPID idGroup);
    DPRESULT DestroyPlayer( DPID idPlayer);
    DPRESULT EnumGroupPlayers( DPID idGroup, LPGUID lpguidInstance, LPDPENUMPLAYERSCALLBACK2 lpEnumPlayersCallback2, LPVOID lpContext, DWORD dwFlags);
    DPRESULT EnumGroups( LPGUID lpguidInstance, LPDPENUMPLAYERSCALLBACK2 lpEnumPlayersCallback2, LPVOID lpContext, DWORD dwFlags );
    DPRESULT EnumPlayers(LPGUID lpguidInstance, LPDPENUMPLAYERSCALLBACK2 lpEnumPlayersCallback2, LPVOID lpContext, EnumPlayersMask dwFlags);
    DPRESULT EnumSessions(LPDPSESSIONDESC2 lpsd, DWORD dwTimeout, LPDPENUMSESSIONSCALLBACK2 lpEnumSessionsCallback2, LPVOID lpContext, EnumSessionsFlags dwFlags );
    DPRESULT GetCaps( [out] LPDPCAPS lpDPCaps, GetCapsFlags dwFlags);
    DPRESULT GetGroupData( DPID idGroup, [out] LPVOID lpData, [out] LPDWORD lpdwDataSize, GetDataFlags dwFlags);
    DPRESULT GetGroupName( DPID idGroup, [out] LPVOID lpData, [out] LPDWORD lpdwDataSize);
    DPRESULT GetMessageCount( DPID idPlayer, [out] LPDWORD lpdwCount );
    DPRESULT GetPlayerAddress( DPID idPlayer, [out] LPVOID lpData, [out] LPDWORD lpdwDataSize );
    DPRESULT GetPlayerCaps( DPID idPlayer, [out] LPDPCAPS lpPlayerCaps , GetCapsFlags dwFlags);
    DPRESULT GetPlayerData( DPID idPlayerD, [out] LPVOID lpData, [out] LPDWORD lpdwDataSize, GetDataFlags dwFlags);
    DPRESULT GetPlayerName( DPID idPlayerD, [out] LPVOID lpData, [out] LPDWORD lpdwDataSize );
    DPRESULT GetSessionDesc( [out] LPVOID lpData, [out] LPDWORD lpdwDataSize );
    DPRESULT Initialize( LPGUID lpGUID );
    DPRESULT Open( LPDPSESSIONDESC2 lpsd , OpenFlags dwFlags);
    DPRESULT Receive( LPDPID lpidFrom, LPDPID lpidTo, ReceiveFlags dwFlags, [out] LPVOID lpData, [out] LPDWORD lpdwDataSize );
    DPRESULT Send( DPID idFrom, DPID idTo, SendFlags dwFlags, LPVOID lpData, DWORD dwDataSize);
    DPRESULT SetGroupData( DPID idGroup, LPVOID lpData, DWORD dwDataSize, SetDataFlags dwFlags);
    DPRESULT SetGroupName( DPID idGroup, LPDPNAME lpGroupName, SetDataFlags dwFlags);
    DPRESULT SetPlayerData( DPID idPlayer, LPVOID lpData, DWORD dwDataSize, SetDataFlags dwFlags);
    DPRESULT SetPlayerName( DPID idPlayer, LPDPNAME lpPlayerName, SetDataFlags dwFlags);
    DPRESULT SetSessionDesc( LPDPSESSIONDESC2 lpSessDesc , SetDataFlags dwFlags);
};

/****************************************************************************
 *
 * IDirectPlay3 (and IDirectPlay3A) Interface
 *
 ****************************************************************************/

interface IDirectPlay3 : IDirectPlay2
{
     /*** IDirectPlay3 methods ***/
    DPRESULT AddGroupToGroup(DPID idParentGroup, DPID idGroup);
    DPRESULT CreateGroupInGroup(DPID idParentGroup , LPDPID lpidGroup , LPDPNAME lpGroupName , LPVOID lpData, DWORD dwDataSize, CreateGroupFlags dwFlags);
    DPRESULT DeleteGroupFromGroup(DPID idParentGroup, DPID idGroup);
    DPRESULT EnumConnections( LPCGUID lpguidApplication, LPDPENUMCONNECTIONSCALLBACK lpEnumCallback, LPVOID lpContext, EnumConnectionsFlags dwFlags);
    DPRESULT EnumGroupsInGroup( DPID idGroup, LPGUID lpguidInstance, LPDPENUMPLAYERSCALLBACK2 lpEnumPlayersCallback2, LPVOID lpContext, EnumPlayersMask dwFlags);
	DPRESULT GetGroupConnectionSettings(DWORD dwFlags, DPID idGroup, [out] LPVOID lpData, [out] LPDWORD dwDataSize);
	DPRESULT InitializeConnection(LPVOID lpData, DWORD dwDataSize);
    DPRESULT SecureOpen(LPCDPSESSIONDESC2 lpsd, DWORD dwFlags, LPCDPSECURITYDESC lpSecurity, LPCDPCREDENTIALS lpCredentials);
    DPRESULT SendChatMessage(DPID idFrom, DPID idTo, SendFlags dwFlags, LPDPCHAT lpChatMessage );
    DPRESULT SetGroupConnectionSettings(DWORD dwFlags, DPID idGroup, LPDPLCONNECTION lpConnection );
    DPRESULT StartSession(DWORD dwFlags, DPID idGroup );
    DPRESULT GetGroupFlags(DPID idGroup, [out] CreateGroupFlags * lpdwFlags );
    DPRESULT GetGroupParent(DPID idGroup, [out] LPDPID lpidParentGroup);
    DPRESULT GetPlayerAccount(DPID idPlayer, DWORD dwFlags, [out] LPVOID lpData , [out] LPDWORD lpdwDataSize );
    DPRESULT GetPlayerFlags(DPID idPlayer,  [out] LPDWORD lpdwDataSize );
};


interface IDirectPlay4 : IDirectPlay3
{
    /*** IDirectPlay4 methods ***/
    DPRESULT GetGroupOwner( DPID idGroup, [out]  LPDPID lpidOwner );
    DPRESULT SetGroupOwner(DPID idGroup, DPID idOwner);
    DPRESULT SendEx( DPID idFrom, DPID idTo, SendFlags dwFlags, LPVOID lpData, DWORD dwDataSize, DWORD dwPriority, DWORD dwTimeout, LPVOID lpContext, LPDWORD lpdwMsgID );
    DPRESULT GetMessageQueue(DPID idFrom, DPID idTo, GetMessageQueueFlags dwFlags, [out] LPDWORD lpdwNumMsgs, [out] LPDWORD lpdwNumBytes );
    DPRESULT CancelMessage( DWORD dwMsgID, DWORD dwFlags );
    DPRESULT CancelPriority( DWORD dwMinPriority, DWORD dwMaxPriority, DWORD dwFlags );
};



/*
 * API's
 */

DPRESULT DirectPlayEnumerateA( LPDPENUMDPCALLBACKA lpCallback, LPVOID lpContext );
DPRESULT DirectPlayEnumerateW( LPDPENUMDPCALLBACK lpCallback, LPVOID  lpContext);
DPRESULT DirectPlayCreate( LPGUID lpGUID, [out] LPDIRECTPLAY *lplpDP, IUnknown *pUnk);

HRESULT DllGetClassObject(
  REFCLSID rclsid,  //CLSID for the class object
  [iid] REFIID riid,      //Reference to the identifier of the interface
                    // that communicates with the class object
  [out] COM_INTERFACE_PTR * ppv      //Address of output variable that receives the
                    // interface pointer requested in riid
);
