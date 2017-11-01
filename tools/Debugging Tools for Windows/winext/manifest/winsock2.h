// ---------------------------------------------------------------------------
// ---------------------------------------------------------------------------
//
//                              WinSock 2 API Set
//
// ---------------------------------------------------------------------------
// ---------------------------------------------------------------------------

category WinSock2:
module WS2_32.DLL:


typedef UINT* SOCKET;

typedef struct sockaddr
{
        short sa_family;              /* address family */
        char    sa_data[14];            /* up to 14 bytes of direct address */
}sockaddr;
typedef struct  hostent {
        LPSTR   h_name;           /* official name of host */
        LPSTR   * h_aliases;  /* alias list */
        short     h_addrtype;             /* host address type */
        short     h_length;               /* length of address */
        LPSTR   * h_addr_list; /* list of addresses */
}hostent;
typedef struct  servent {
        LPSTR   s_name;           /* official service name */
        LPSTR   * s_aliases;  /* alias list */
        short     s_port;                 /* port # */
        LPSTR   s_proto;          /* protocol to use */
}servent;
typedef struct  protoent {
        LPSTR   p_name;           /* official protocol name */
        LPSTR   * p_aliases;  /* alias list */
        short     p_proto;                /* protocol # */
}protoent;
typedef struct WSAData {
        WORD                    wVersion;
        WORD                    wHighVersion;
        char                    szDescription;
        char                    szSystemStatus;
        USHORT                  iMaxSockets;
        USHORT                  iMaxUdpDg;
        LPSTR                   lpVendorInfo;
} WSADATA, * LPWSADATA;
SOCKET

accept(
    SOCKET s,
    [out] sockaddr * addr,
    [out] int * addrlen
    );


int

bind(
    SOCKET s,
    sockaddr * name,
    int namelen
    );



int

closesocket(
    SOCKET s
    );



int

connect(
    SOCKET s,
    sockaddr * name,
    int namelen
    );



int

ioctlsocket(
    SOCKET s,
    long cmd,
    [out] int * argp
    );


int

getpeername(
    SOCKET s,
    [out] sockaddr * name,
    [out] int * namelen
    );


int

getsockname(
    SOCKET s,
    [out] sockaddr * name,
    [out] int * namelen
    );



int

getsockopt(
    SOCKET s,
    int level,
    int optname,
    [out] LPSTR optval,
    [out] int * optlen
    );


int

htonl(
    int hostlong
    );


int

htons(
    int hostshort
    );

int

inet_addr(
    LPSTR cp
    );

int

listen(
    SOCKET s,
    int backlog
    );


int

ntohl(
    int netlong
    );

int

ntohs(
    int netshort
    );


int

recv(
    SOCKET s,
    [out] LPSTR buf,
    int len,
    int flags
    );


int

recvfrom(
    SOCKET s,
    [out] LPSTR buf,
    int len,
    int flags,
    [out] sockaddr * from,
    [out] int * fromlen
    );


int

send(
    SOCKET s,
    LPSTR buf,
    int len,
    int flags
    );


int

sendto(
    SOCKET s,
    LPSTR buf,
    int len,
    int flags,
    sockaddr * to,
    int tolen
    );


int

setsockopt(
    SOCKET s,
    int level,
    int optname,
    LPSTR optval,
    int optlen
    );




int

shutdown(
    SOCKET s,
    int how
    );




SOCKET

socket(
    int af,
    int type,
    int protocol
    );


/* Database function prototypes */

hostent *
gethostbyaddr(
    LPSTR addr,
    int len,
    int type
    );
hostent *
gethostbyname(
    LPSTR name
    );

int
gethostname(
    [out] LPSTR name,
    int namelen
    );


servent *
getservbyport(
    int port,
    LPSTR proto
    );



servent *
getservbyname(
    LPSTR name,
    LPSTR proto
    );

protoent *
getprotobynumber(
    int number
    );

protoent *
getprotobyname(
    LPSTR name
    );
