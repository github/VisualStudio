module DSOUND.DLL:
category DirectSound:

 class __declspec(uuid("47d4d946-62e8-11cf-93bc-444553540000"))        DirectSound;
 class __declspec(uuid("3901cc3f-84b5-4fa4-ba35-aa8172b8a09b"))        DirectSound8;
 class __declspec(uuid("b0210780-89cd-11d0-af08-00a0c925cd16"))        DirectSoundCapture;
 class __declspec(uuid("e4bcac13-7f99-4908-9a8e-74e3bf24b6e1"))        DirectSoundCapture8;
 class __declspec(uuid("fea4300c-7959-4147-b26a-2377b9e7a91d"))        DirectSoundFullDuplex;
 class __declspec(uuid("b2f586d4-5558-49d1-a07b-3249dbbb33c2"))        DirectSoundBufferConfig;

struct __declspec(uuid("279AFA83-4981-11CE-A521-0020AF0BE560")) IDirectSound;
struct __declspec(uuid("279AFA85-4981-11CE-A521-0020AF0BE560")) IDirectSoundBuffer;
struct __declspec(uuid("279AFA84-4981-11CE-A521-0020AF0BE560")) IDirectSound3DListener;
struct __declspec(uuid("279AFA86-4981-11CE-A521-0020AF0BE560")) IDirectSound3DBuffer;
struct __declspec(uuid("b0210781-89cd-11d0-af08-00a0c925cd16")) IDirectSoundCapture;
struct __declspec(uuid("b0210782-89cd-11d0-af08-00a0c925cd16")) IDirectSoundCaptureBuffer;
struct __declspec(uuid("b30f3564-1698-45ba-9f75-fc3c6c3b2810")) IDirectSoundFXSend;
struct __declspec(uuid("C50A7E93-F395-4834-9EF6-7FA99DE50966")) IDirectSound8;
struct __declspec(uuid("6825a449-7524-4d82-920f-50e36ab3ab1e")) IDirectSoundBuffer8;
struct __declspec(uuid("00990df4-0dbb-4872-833e-6d303e80aeb6")) IDirectSoundCaptureBuffer8;

struct __declspec(uuid("b0210783-89cd-11d0-af08-00a0c925cd16")) IDirectSoundNotify;
struct __declspec(uuid("31efac30-515c-11d0-a9aa-00aa0061be93")) IKsPropertySet;

struct __declspec(uuid("def00000-9c6d-47ed-aaf1-4dda8f2b5c03"))        IDefaultPlayback;
struct __declspec(uuid("def00001-9c6d-47ed-aaf1-4dda8f2b5c03"))        IDefaultCapture;
struct __declspec(uuid("def00002-9c6d-47ed-aaf1-4dda8f2b5c03"))        IDefaultVoicePlayback;
struct __declspec(uuid("def00003-9c6d-47ed-aaf1-4dda8f2b5c03"))        IDefaultVoiceCapture;
struct __declspec(uuid("d616f352-d622-11ce-aac5-0020af0b99a3"))        IDirectSoundFXGargle;
struct __declspec(uuid("880842e3-145f-43e6-a934-a71806e50547"))  IDirectSoundFXChorus;
struct __declspec(uuid("903e9878-2c92-4072-9b2c-ea68f5396783"))  IDirectSoundFXFlanger;
struct __declspec(uuid("8bd28edf-50db-4e92-a2bd-445488d1ed42"))  IDirectSoundFXEcho;
struct __declspec(uuid("8ecf4326-455f-4d8b-bda9-8d5d3e9e3e0b"))  IDirectSoundFXDistortion;
struct __declspec(uuid("4bbd1154-62f6-4e2c-a15c-d3b6c417f7a0"))  IDirectSoundFXCompressor;
struct __declspec(uuid("c03ca9fe-fe90-4204-8078-82334cd177da"))  IDirectSoundFXParamEq;
struct __declspec(uuid("4b166a6a-0d66-43f3-80e3-ee6280dee1a4"))  IDirectSoundFXI3DL2Reverb;
struct __declspec(uuid("46858c3a-0dc6-45e3-b760-d4eef16cb325"))  IDirectSoundFXWavesReverb;
struct __declspec(uuid("174d3eb9-6696-4fac-a46c-a0ac7bc9e20f"))  IDirectSoundCaptureFXAec;
struct __declspec(uuid("ed311e41-fbae-4175-9625-cd0854f693ca"))  IDirectSoundCaptureFXNoiseSuppress;
struct __declspec(uuid("edcb4c7a-daab-4216-a42e-6c50596ddc1d"))  IDirectSoundFullDuplex;




typedef IDirectSound *LPDIRECTSOUND;
typedef IDirectSoundBuffer *LPDIRECTSOUNDBUFFER;
typedef IDirectSound3DListener *LPDIRECTSOUND3DLISTENER;
typedef IDirectSound3DBuffer *LPDIRECTSOUND3DBUFFER;
typedef IDirectSoundCapture *LPDIRECTSOUNDCAPTURE;
typedef IDirectSoundCaptureBuffer *LPDIRECTSOUNDCAPTUREBUFFER;
typedef IDirectSoundNotify *LPDIRECTSOUNDNOTIFY;

typedef IDirectSoundFXSend           *LPDIRECTSOUNDFXSEND;
typedef IDirectSoundFXGargle         *LPDIRECTSOUNDFXGARGLE;
typedef IDirectSoundFXChorus         *LPDIRECTSOUNDFXCHORUS;
typedef IDirectSoundFXFlanger        *LPDIRECTSOUNDFXFLANGER;
typedef IDirectSoundFXEcho           *LPDIRECTSOUNDFXECHO;
typedef IDirectSoundFXDistortion     *LPDIRECTSOUNDFXDISTORTION;
typedef IDirectSoundFXCompressor     *LPDIRECTSOUNDFXCOMPRESSOR;
typedef IDirectSoundFXParamEq        *LPDIRECTSOUNDFXPARAMEQ;
typedef IDirectSoundFXWavesReverb    *LPDIRECTSOUNDFXWAVESREVERB;
typedef IDirectSoundFXI3DL2Reverb    *LPDIRECTSOUNDFXI3DL2REVERB;
typedef IDirectSoundCaptureFXAec     *LPDIRECTSOUNDCAPTUREFXAEC;
typedef IDirectSoundCaptureFXNoiseSuppress *LPDIRECTSOUNDCAPTUREFXNOISESUPPRESS;
typedef IDirectSoundFullDuplex       *LPDIRECTSOUNDFULLDUPLEX;

typedef IDirectSound8                *LPDIRECTSOUND8;
typedef IDirectSoundBuffer8          *LPDIRECTSOUNDBUFFER8;
typedef IDirectSoundCaptureBuffer8   *LPDIRECTSOUNDCAPTUREBUFFER8;

typedef LPDIRECTSOUND8 *LPLPDIRECTSOUND8;
typedef LPDIRECTSOUNDBUFFER8 *LPLPDIRECTSOUNDBUFFER8;
typedef LPDIRECTSOUNDCAPTUREBUFFER8 *LPLPDIRECTSOUNDCAPTUREBUFFER8;

//
// Flags
//

mask DWORD DSCAPS_MASK
{
#define DSCAPS_PRIMARYMONO          0x00000001
#define DSCAPS_PRIMARYSTEREO        0x00000002
#define DSCAPS_PRIMARY8BIT          0x00000004
#define DSCAPS_PRIMARY16BIT         0x00000008
#define DSCAPS_CONTINUOUSRATE       0x00000010
#define DSCAPS_EMULDRIVER           0x00000020
#define DSCAPS_CERTIFIED            0x00000040
#define DSCAPS_SECONDARYMONO        0x00000100
#define DSCAPS_SECONDARYSTEREO      0x00000200
#define DSCAPS_SECONDARY8BIT        0x00000400
#define DSCAPS_SECONDARY16BIT       0x00000800
};

mask DWORD DSBPLAY_MASK
{
#define DSBPLAY_LOOPING             0x00000001
#define DSBPLAY_LOCHARDWARE         0x00000002
#define DSBPLAY_LOCSOFTWARE         0x00000004
#define DSBPLAY_TERMINATEBY_TIME    0x00000008
#define DSBPLAY_TERMINATEBY_DISTANCE    0x000000010
#define DSBPLAY_TERMINATEBY_PRIORITY    0x000000020
};

mask DWORD DSBSTATUS_MASK
{
#define DSBSTATUS_PLAYING           0x00000001
#define DSBSTATUS_BUFFERLOST        0x00000002
#define DSBSTATUS_LOOPING           0x00000004
#define DSBSTATUS_LOCHARDWARE       0x00000008
#define DSBSTATUS_LOCSOFTWARE       0x00000010
#define DSBSTATUS_TERMINATED        0x00000020
};

value DWORD DSBLOCK_VALUE
{
#define DSBLOCK_FROMWRITECURSOR     0x00000001
#define DSBLOCK_ENTIREBUFFER        0x00000002
};

value DWORD DSSCL_VALUE
{
#define DSSCL_NORMAL                0x00000001
#define DSSCL_PRIORITY              0x00000002
#define DSSCL_EXCLUSIVE             0x00000003
#define DSSCL_WRITEPRIMARY          0x00000004
};

value DWORD DS3DMODE_VALUE
{
#define DS3DMODE_NORMAL             0x00000000
#define DS3DMODE_HEADRELATIVE       0x00000001
#define DS3DMODE_DISABLE            0x00000002
};

mask DWORD DSBCAPS_MASK
{
#define DSBCAPS_PRIMARYBUFFER       0x00000001
#define DSBCAPS_STATIC              0x00000002
#define DSBCAPS_LOCHARDWARE         0x00000004
#define DSBCAPS_LOCSOFTWARE         0x00000008
#define DSBCAPS_CTRL3D              0x00000010
#define DSBCAPS_CTRLFREQUENCY       0x00000020
#define DSBCAPS_CTRLPAN             0x00000040
#define DSBCAPS_CTRLVOLUME          0x00000080
#define DSBCAPS_CTRLPOSITIONNOTIFY  0x00000100
#define DSCBCAPS_CTRLFX             0x00000200
//#define DSBCAPS_CTRLDEFAULT         0x000000E0
//#define DSBCAPS_CTRLALL             0x000001F0
#define DSBCAPS_STICKYFOCUS         0x00004000
#define DSBCAPS_GLOBALFOCUS         0x00008000
#define DSBCAPS_GETCURRENTPOSITION2 0x00010000
#define DSBCAPS_MUTE3DATMAXDISTANCE 0x00020000
#define DSBCAPS_LOCDEFER            0x00040000

#define DSCBCAPS_WAVEMAPPED         0x80000000
};

value DWORD DSSPEAKER_VALUE
{
#define DSSPEAKER_HEADPHONE         0x00000001
#define DSSPEAKER_MONO              0x00000002
#define DSSPEAKER_QUAD              0x00000003
#define DSSPEAKER_STEREO            0x00000004
#define DSSPEAKER_SURROUND          0x00000005

#define DSSPEAKER_GEOMETRY_MIN      0x00000005  //   5 degrees
#define DSSPEAKER_GEOMETRY_NARROW   0x0000000A  //  10 degrees
#define DSSPEAKER_GEOMETRY_WIDE     0x00000014  //  20 degrees
#define DSSPEAKER_GEOMETRY_MAX      0x000000B4  // 180 degrees

//#define DSSPEAKER_COMBINED(c, g)    ((DWORD)(((BYTE)(c)) | ((DWORD)((BYTE)(g))) << 16))
//#define DSSPEAKER_CONFIG(a)         ((BYTE)(a))
//#define DSSPEAKER_GEOMETRY(a)       ((BYTE)(((DWORD)(a) >> 16) & 0x00FF))
};

value DWORD DSBFREQUENCY_VALUE
{
#define DSBFREQUENCY_MIN            100
#define DSBFREQUENCY_MAX            100000
#define DSBFREQUENCY_ORIGINAL       0
};

value DWORD DSBPAN_VALUE
{
#define DSBPAN_LEFT                 -10000
#define DSBPAN_CENTER               0
#define DSBPAN_RIGHT                10000
};

value DWORD DSBVOLUME_VALUE
{
#define DSBVOLUME_MIN               -10000
#define DSBVOLUME_MAX               0
};

value DWORD DSBSIZE_VALUE
{
#define DSBSIZE_MIN                 4
#define DSBSIZE_MAX                 0x0FFFFFFF
};

value DWORD DS3D_VALUE
{
#define DS3D_IMMEDIATE              0x00000000
#define DS3D_DEFERRED               0x00000001
};


//#define DSCCAPS_EMULDRIVER          0x00000020

mask DWORD DSCBLOCK_MASK
{
#define DSCBLOCK_ENTIREBUFFER       0x00000001
};

mask DWORD DSCBSTATUS_MASK
{
#define DSCBSTATUS_CAPTURING        0x00000001
#define DSCBSTATUS_LOOPING          0x00000002
};
mask DWORD DSCBSTART_MASK
{
#define DSCBSTART_LOOPING           0x00000001
};
//
//#define DSBPN_OFFSETSTOP            0xFFFFFFFF
//
mask DWORD DSFX_MASK
{
    #define DSFX_LOCHARDWARE    0x00000001
    #define DSFX_LOCSOFTWARE    0x00000002
};

mask DWORD DSFXR_MASK
{
    #define DSCFXR_LOCHARDWARE  0x00000010
    #define DSCFXR_LOCSOFTWARE  0x00000020
    #define DSCFXR_UNALLOCATED  0x00000040
    #define DSCFXR_FAILED       0x00000080
    #define DSCFXR_UNKNOWN      0x00000100
};

typedef struct _DSEFFECTDESC
{
    DWORD               dwSize;
    DWORD               dwFlags;
    GUID                guidDSFXClass;
    LPDIRECTSOUNDBUFFER lpSendBuffer;
    DWORD               dwReserved;
} DSEFFECTDESC;
typedef DSEFFECTDESC *LPDSEFFECTDESC;
typedef DSEFFECTDESC *LPCDSEFFECTDESC;

value DWORD DSFXR_ENUM
{
#define  DSFXR_PRESENT           0
#define  DSFXR_LOCHARDWARE       1
#define  DSFXR_LOCSOFTWARE       2
#define  DSFXR_UNALLOCATED       3
#define  DSFXR_FAILED            4
#define  DSFXR_UNKNOWN           5
#define  DSFXR_SENDLOOP          6
};

typedef struct _DSCEFFECTDESC
{
    DWORD       dwSize;
    DWORD       dwFlags;
    GUID        guidDSCFXClass;
    GUID        guidDSCFXInstance;
    DWORD       dwReserved1;
    DWORD       dwReserved2;
} DSCEFFECTDESC;
typedef  DSCEFFECTDESC *LPDSCEFFECTDESC;
typedef DSCEFFECTDESC *LPCDSCEFFECTDESC;

typedef struct _DSCAPS
{
    DWORD           dwSize;
    DSCAPS_MASK     dwFlags;
    DWORD           dwMinSecondarySampleRate;
    DWORD           dwMaxSecondarySampleRate;
    DWORD           dwPrimaryBuffers;
    DWORD           dwMaxHwMixingAllBuffers;
    DWORD           dwMaxHwMixingStaticBuffers;
    DWORD           dwMaxHwMixingStreamingBuffers;
    DWORD           dwFreeHwMixingAllBuffers;
    DWORD           dwFreeHwMixingStaticBuffers;
    DWORD           dwFreeHwMixingStreamingBuffers;
    DWORD           dwMaxHw3DAllBuffers;
    DWORD           dwMaxHw3DStaticBuffers;
    DWORD           dwMaxHw3DStreamingBuffers;
    DWORD           dwFreeHw3DAllBuffers;
    DWORD           dwFreeHw3DStaticBuffers;
    DWORD           dwFreeHw3DStreamingBuffers;
    DWORD           dwTotalHwMemBytes;
    DWORD           dwFreeHwMemBytes;
    DWORD           dwMaxContigFreeHwMemBytes;
    DWORD           dwUnlockTransferRateHwBuffers;
    DWORD           dwPlayCpuOverheadSwBuffers;
    DWORD           dwReserved1;
    DWORD           dwReserved2;
} DSCAPS, *LPDSCAPS;

typedef DSCAPS *LPCDSCAPS;

typedef struct _DSBCAPS
{
    DWORD           dwSize;
    DWORD           dwFlags;
    DWORD           dwBufferBytes;
    DWORD           dwUnlockTransferRate;
    DWORD           dwPlayCpuOverhead;
} DSBCAPS, *LPDSBCAPS;

typedef DSBCAPS *LPCDSBCAPS;

typedef struct _DSBUFFERDESC
{
    DWORD           dwSize;
    DSBCAPS_MASK    dwFlags;
    DWORD           dwBufferBytes;
    DWORD           dwReserved;
    LPWAVEFORMATEX  lpwfxFormat;
} DSBUFFERDESC, *LPDSBUFFERDESC;

typedef DSBUFFERDESC *LPCDSBUFFERDESC;

typedef struct _DS3DBUFFER
{
    DWORD           dwSize;
    D3DVECTOR       vPosition;
    D3DVECTOR       vVelocity;
    DWORD           dwInsideConeAngle;
    DWORD           dwOutsideConeAngle;
    D3DVECTOR       vConeOrientation;
    LONG            lConeOutsideVolume;
    D3DVALUE        flMinDistance;
    D3DVALUE        flMaxDistance;
    DWORD           dwMode;
} DS3DBUFFER, *LPDS3DBUFFER;

typedef DS3DBUFFER *LPCDS3DBUFFER;

typedef struct _DS3DLISTENER
{
    DWORD           dwSize;
    D3DVECTOR       vPosition;
    D3DVECTOR       vVelocity;
    D3DVECTOR       vOrientFront;
    D3DVECTOR       vOrientTop;
    D3DVALUE        flDistanceFactor;
    D3DVALUE        flRolloffFactor;
    D3DVALUE        flDopplerFactor;
} DS3DLISTENER, *LPDS3DLISTENER;

typedef DS3DLISTENER *LPCDS3DLISTENER;

typedef struct _DSCCAPS
{
    DWORD           dwSize;
    DWORD           dwFlags;
    DWORD           dwFormats;
    DWORD           dwChannels;
} DSCCAPS, *LPDSCCAPS;

typedef DSCCAPS *LPCDSCCAPS;

typedef struct _DSCBUFFERDESC
{
    DWORD           dwSize;
    DWORD           dwFlags;
    DWORD           dwBufferBytes;
    DWORD           dwReserved;
    LPWAVEFORMATEX  lpwfxFormat;
    DWORD           dwFXCount;
    LPDSCEFFECTDESC lpDSCFXDesc;
} DSCBUFFERDESC, *LPDSCBUFFERDESC;

typedef DSCBUFFERDESC *LPCDSCBUFFERDESC;

typedef struct _DSCBCAPS
{
    DWORD           dwSize;
    DWORD           dwFlags;
    DWORD           dwBufferBytes;
    DWORD           dwReserved;
} DSCBCAPS, *LPDSCBCAPS;

typedef DSCBCAPS *LPCDSCBCAPS;

typedef struct _DSBPOSITIONNOTIFY
{
    DWORD           dwOffset;
    HANDLE          hEventNotify;
} DSBPOSITIONNOTIFY, *LPDSBPOSITIONNOTIFY;

typedef DSBPOSITIONNOTIFY *LPCDSBPOSITIONNOTIFY;

//
// Compatibility typedefs
//

typedef LPDIRECTSOUND *LPLPDIRECTSOUND;
typedef LPDIRECTSOUNDBUFFER *LPLPDIRECTSOUNDBUFFER;
typedef LPDIRECTSOUND3DLISTENER *LPLPDIRECTSOUND3DLISTENER;
typedef LPDIRECTSOUND3DBUFFER *LPLPDIRECTSOUND3DBUFFER;
typedef LPDIRECTSOUNDCAPTURE *LPLPDIRECTSOUNDCAPTURE;
typedef LPDIRECTSOUNDCAPTUREBUFFER *LPLPDIRECTSOUNDCAPTUREBUFFER;
typedef LPDIRECTSOUNDNOTIFY *LPLPDIRECTSOUNDNOTIFY;
typedef LPVOID *LPLPVOID;
//typedef WAVEFORMATEX *LPCWAVEFORMATEX;

value DWORD DSRESULT
{
//
// Return Codes
//

#define DS_OK                           0

// The call failed because resources (such as a priority level)
// were already being used by another caller.
#define DSERR_ALLOCATED                 0x8878000A  [fail]

// The control (vol,pan,etc.) requested by the caller is not available.
#define DSERR_CONTROLUNAVAIL            0x8878001E  [fail]

// An invalid parameter was passed to the returning function
#define DSERR_INVALIDPARAM              0x80070057  [fail]

// This call is not valid for the current state of this object
#define DSERR_INVALIDCALL               0x88780032  [fail]

// An undetermined error occured inside the DirectSound subsystem
#define DSERR_GENERIC                   0x80004005  [fail]

// The caller does not have the priority level required for the function to
// succeed.
#define DSERR_PRIOLEVELNEEDED           0x88780046  [fail]

// Not enough free memory is available to complete the operation
#define DSERR_OUTOFMEMORY               0x8007000E  [fail]

// The specified WAVE format is not supported
#define DSERR_BADFORMAT                 0x88780064  [fail]

// The function called is not supported at this time
#define DSERR_UNSUPPORTED               0x80004001  [fail]

// No sound driver is available for use
#define DSERR_NODRIVER                  0x88780078  [fail]

// This object is already initialized
#define DSERR_ALREADYINITIALIZED        0x88780082  [fail]

// This object does not support aggregation
#define DSERR_NOAGGREGATION             0x80040110  [fail]

// The buffer memory has been lost, and must be restored.
#define DSERR_BUFFERLOST                0x88780096  [fail]

// Another app has a higher priority level, preventing this call from
// succeeding.
#define DSERR_OTHERAPPHASPRIO           0x887800A0  [fail]

// This object has not been initialized
#define DSERR_UNINITIALIZED             0x887800AA  [fail]

// The requested COM interface is not available
#define DSERR_NOINTERFACE               0x80000004  [fail]

// Access is denied
#define DSERR_ACCESSDENIED              0x80070005  [fail]

// Tried to create a DSBCAPS_CTRLFX buffer shorter than DSBSIZE_FX_MIN milliseconds
#define DSERR_BUFFERTOOSMALL            0x887800B4  [fail]

// Attempt to use DirectSound 8 functionality on an older DirectSound object
#define DSERR_DS8_REQUIRED              0x887800BE  [fail]

// A circular loop of send effects was detected
#define DSERR_SENDLOOP                  0x887800C8  [fail]

// The GUID specified in an audiopath file does not match a valid MIXIN buffer
#define DSERR_BADSENDBUFFERGUID         0x887800D2  [fail]

// The object requested was not found (numerically equal to DMUS_E_NOT_FOUND)
#define DSERR_OBJECTNOTFOUND            0x88781193  [fail]

};


//
// IDirectSound
//


interface IDirectSound: IUnknown
{
    // IDirectSound methods
    DSRESULT  CreateSoundBuffer    ([in] LPCDSBUFFERDESC lpcDSBufferDesc, [out] LPLPDIRECTSOUNDBUFFER lplpDirectSoundBuffer, [in] IUnknown * pUnkOuter);
    DSRESULT  GetCaps              ([out] LPDSCAPS lpDSCaps) ;
    DSRESULT  DuplicateSoundBuffer ([in] LPDIRECTSOUNDBUFFER lpDsbOriginal, [out] LPLPDIRECTSOUNDBUFFER lplpDsbDuplicate) ;
    DSRESULT  SetCooperativeLevel  (HWND hwnd, DSSCL_VALUE dwLevel) ;
    DSRESULT  Compact              () ;
    DSRESULT  GetSpeakerConfig     ([out] DSSPEAKER_VALUE * lpdwSpeakerConfig) ;
    DSRESULT  SetSpeakerConfig     (DSSPEAKER_VALUE dwSpeakerConfig) ;
    DSRESULT  Initialize           ([in] LPCGUID lpcGuid) ;
};
interface IDirectSound8 : IDirectSound
{
    // IDirectSound8 methods
     DSRESULT VerifyCertification  (LPDWORD pdwCertified) ;
};

//
// IDirectSoundBuffer
//


interface IDirectSoundBuffer: IUnknown
{
    // IDirectSoundBuffer methods
    DSRESULT  GetCaps               ([out] LPDSCAPS lpDSCaps ) ;
    DSRESULT  GetCurrentPosition    ([out] LPDWORD lpdwCurrentPlayCursor, [out] LPDWORD lpdwCurrentWriteCursor );
    DSRESULT  GetFormat             ([out] LPWAVEFORMATEX lpwfxFormat, DWORD dwSizeAllocated,  [out] LPDWORD lpdwSizeWritten );
    DSRESULT  GetVolume             ([out] LPLONG lplVolume );
    DSRESULT  GetPan                ([out] LPLONG lplPan);
    DSRESULT  GetFrequency          ([out] LPDWORD lpdwFrequency );
    DSRESULT  GetStatus             ([out] LPDWORD lpdwStatus );
    DSRESULT  Initialize            ([in] LPDIRECTSOUND lpDirectSound, [in] LPCDSBUFFERDESC lpcDSBufferDesc );
    DSRESULT  Lock                  (DWORD dwWriteCursor,   DWORD dwWriteBytes,   [out]  LPVOID lplpvAudioPtr1, [out] LPDWORD lpdwAudioBytes1, [out] LPVOID lplpvAudioPtr2, [out] LPDWORD lpdwAudioBytes2, DSBLOCK_VALUE dwFlags);
    DSRESULT  Play                  (DWORD dwReserved1, DWORD dwPriority,  DSBPLAY_MASK dwFlags );
    DSRESULT  SetCurrentPosition    (DWORD dwNewPosition);
    DSRESULT  SetFormat             ([in] LPCWAVEFORMATEX lpcfxFormat );
    DSRESULT  SetVolume             (LONG lVolume );
    DSRESULT  SetPan                (LONG lPan );
    DSRESULT  SetFrequency          (DWORD dwFrequency );
    DSRESULT  Stop                  ( );
    DSRESULT  Unlock                ([in] LPVOID lpvAudioPtr1, DWORD dwAudioBytes1, [in] LPVOID lpvAudioPtr2, DWORD dwAudioBytes2 );
    DSRESULT  Restore               ( );
};

interface IDirectSoundBuffer8: IDirectSoundBuffer
{
    // IDirectSoundBuffer8 methods
    DSRESULT  SetFX                 (DWORD dwEffectsCount, [in] LPDSEFFECTDESC pDSFXDesc, [out] LPDWORD pdwResultCodes) ;
    DSRESULT  AcquireResources      (DWORD dwFlags, DWORD dwEffectsCount, [out] LPDWORD pdwResultCodes) ;
    DSRESULT  GetObjectInPath       (REFGUID rguidObject, DWORD dwIndex, REFGUID rguidInterface, [out] LPVOID *ppObject) ;
};


//
// IDirectSound3DListener
//


interface IDirectSound3DListener: IUnknown
{
    // IDirectSound3D methods
    DSRESULT  GetAllParameters          ([out] LPDS3DLISTENER lpListener );
    DSRESULT  GetDistanceFactor         ([out] LPD3DVALUE lpflDistanceFactor );
    DSRESULT  GetDopplerFactor          ([out] LPD3DVALUE lpflDopplerFactor );
    DSRESULT  GetOrientation            ([out] LPD3DVECTOR lpvOrientFront, [out] LPD3DVECTOR lpvOrientTop );
    DSRESULT  GetPosition               ([out] LPD3DVECTOR lpvPosition);
    DSRESULT  GetRolloffFactor          ([out] LPD3DVALUE lpflRolloffFactor );
    DSRESULT  GetVelocity               ([out] LPD3DVECTOR lpvVelocity);
    DSRESULT  SetAllParameters          ([in] LPCDS3DLISTENER lpcListener, DS3D_VALUE dwApply );
    DSRESULT  SetDistanceFactor         (D3DVALUE flDistanceFactor, DS3D_VALUE dwApply );
    DSRESULT  SetDopplerFactor          (D3DVALUE flDopplerFactor, DS3D_VALUE dwApply );
    DSRESULT  SetOrientation            (D3DVALUE xFront, D3DVALUE yFront, D3DVALUE zFront, D3DVALUE xTop,   D3DVALUE yTop,   D3DVALUE zTop,   DS3D_VALUE dwApply);
    DSRESULT  SetPosition               (D3DVALUE x,   D3DVALUE y,   D3DVALUE z,   DS3D_VALUE dwApply );
    DSRESULT  SetRolloffFactor          (D3DVALUE flRolloffFactor, DS3D_VALUE dwApply);
    DSRESULT  SetVelocity               (D3DVALUE x,   D3DVALUE y,   D3DVALUE z,   DS3D_VALUE dwApply );
    DSRESULT  CommitDeferredSettings    ( );
};

//
// IDirectSound3DBuffer
//


interface IDirectSound3DBuffer: IUnknown
{
    // IDirectSoundBuffer3D methods
    DSRESULT  GetAllParameters      ([out] LPDS3DBUFFER lpDs3dBuffer );
    DSRESULT  GetConeAngles         ([out] LPDWORD lpdwInsideConeAngle, [out] LPDWORD lpdwOutsideConeAngle );
    DSRESULT  GetConeOrientation    ([out] LPD3DVECTOR lpvOrientation );
    DSRESULT  GetConeOutsideVolume  ([out] LPLONG lplConeOutsideVolume );
    DSRESULT  GetMaxDistance        ([out] LPD3DVALUE lpflMaxDistance );
    DSRESULT  GetMinDistance        ([out] LPD3DVALUE lpflMinDistance );
    DSRESULT  GetMode               ([out] LPDWORD lpdwMode );
    DSRESULT  GetPosition           ([out] LPD3DVECTOR lpvPosition );
    DSRESULT  GetVelocity           ([out] LPD3DVECTOR lpvVelocity );
    DSRESULT  SetAllParameters      ([in] LPCDS3DBUFFER lpcDs3dBuffer, DS3D_VALUE dwApply );
    DSRESULT  SetConeAngles         (DWORD dwInsideConeAngle, DWORD dwOutsideConeAngle, DS3D_VALUE dwApply );
    DSRESULT  SetConeOrientation    (D3DVALUE x,   D3DVALUE y,   D3DVALUE z,   DS3D_VALUE dwApply );
    DSRESULT  SetConeOutsideVolume  (LONG lConeOutsideVolume, DS3D_VALUE dwApply );
    DSRESULT  SetMaxDistance        (D3DVALUE flMaxDistance, DS3D_VALUE dwApply );
    DSRESULT  SetMinDistance        (D3DVALUE flMinDistance, DS3D_VALUE dwApply );
    DSRESULT  SetMode               (DS3DMODE_VALUE dwMode, DS3D_VALUE dwApply );
    DSRESULT  SetPosition           (D3DVALUE x,   D3DVALUE y,   D3DVALUE z,   DS3D_VALUE dwApply );
    DSRESULT  SetVelocity           (D3DVALUE x,   D3DVALUE y,   D3DVALUE z,   DS3D_VALUE dwApply );
};


//
// IDirectSoundCapture
//


interface IDirectSoundCapture: IUnknown
{
    // IDirectSoundCapture methods
    DSRESULT  CreateCaptureBuffer   ([in] LPDSCBUFFERDESC lpDSCBufferDesc, [out] LPLPDIRECTSOUNDCAPTUREBUFFER lplpDirectSoundCaptureBuffer, [in] LPUNKNOWN pUnkOuter );
    DSRESULT  GetCaps               ([out] LPDSCAPS lpDSCaps ) ;
    DSRESULT  Initialize            ([in] LPCGUID lpcGuid );
};

interface IDirectSoundCaptureBuffer8 : IDirectSoundCapture
{
    // IDirectSoundCaptureBuffer8 methods
    DSRESULT GetObjectInPath       (REFGUID rguidObject, DWORD dwIndex, REFGUID rguidInterface, [out] LPVOID *ppObject) ;
    DSRESULT GetFXStatus           (DWORD dwFXCount, [out] LPDWORD pdwFXStatus) ;
};

//
// IDirectSoundCaptureBuffer
//


interface IDirectSoundCaptureBuffer: IUnknown
{
    // IDirectSoundCaptureBuffer methods
    DSRESULT  GetCaps               ([out] LPDSCAPS lpDSCaps ) ;
    DSRESULT  GetCurrentPosition    ([out] LPDWORD lpdwCapturePosition, [out] LPDWORD lpdwReadPosition );
    DSRESULT  GetFormat             ([out] LPWAVEFORMATEX lpwfxFormat, DWORD dwSizeAllocated,  [out] LPDWORD lpdwSizeWritten );
    DSRESULT  GetStatus             ([out] DWORD *lpdwStatus );
    DSRESULT  Initialize            (LPDIRECTSOUNDCAPTURE lpDirectSoundCapture, [in] LPCDSCBUFFERDESC lpcDSCBufferDesc );
    DSRESULT  Lock                  (DWORD dwReadCursor,     DWORD dwReadBytes,  [out] LPVOID *lplpvAudioPtr1, [out] LPDWORD lpdwAudioBytes1, [out] LPVOID *lplpvAudioPtr2, [out] LPDWORD lpdwAudioBytes2, DSCBLOCK_MASK dwFlags);
    DSRESULT  Start                 (DSCBSTART_MASK dwFlags );
    DSRESULT  Stop                  ( );
    DSRESULT  Unlock                ([in] LPVOID lpvAudioPtr1,DWORD dwAudioBytes1, [in] LPVOID lpvAudioPtr2,DWORD dwAudioBytes2 );
};


//
// IDirectSoundNotify
//


interface IDirectSoundNotify: IUnknown
{
    // IDirectSoundNotify methods
    DSRESULT  SetNotificationPositions  ( );
};

typedef struct _DSFXSend
{
    LONG lSendLevel;
} DSFXSend;
typedef DSFXSend *LPDSFXSend;
typedef DSFXSend *LPCDSFXSend;


interface IDirectSoundFXSend: IUnknown
{
    // IDirectSoundFXSend methods
    DSRESULT SetAllParameters      ([in] LPCDSFXSend pcDsFxSend) ;
    DSRESULT GetAllParameters      ([out] LPDSFXSend pDsFxSend) ;
};


typedef struct _DSFXGargle
{
    DWORD       dwRateHz;               // Rate of modulation in hz
    DWORD       dwWaveShape;            // DSFXGARGLE_WAVE_xxx
} DSFXGargle;
typedef DSFXGargle *LPDSFXGargle;
typedef DSFXGargle *LPCDSFXGargle;

value DWORD DSFXGARGLE_VALUE
{
#define DSFXGARGLE_WAVE_TRIANGLE        0
#define DSFXGARGLE_WAVE_SQUARE          1
};



interface IDirectSoundFXGargle: IUnknown
{
    // IDirectSoundFXGargle methods
    DSRESULT SetAllParameters      ([in] LPCDSFXGargle pcDsFxGargle) ;
    DSRESULT GetAllParameters      ([out] LPDSFXGargle pDsFxGargle) ;
};

typedef struct _DSFXChorus
{
    FLOAT       fWetDryMix;
    FLOAT       fDepth;
    FLOAT       fFeedback;
    FLOAT       fFrequency;
    LONG        lWaveform;          // LFO shape; DSFXCHORUS_WAVE_xxx
    FLOAT       fDelay;
    LONG        lPhase;
} DSFXChorus;
typedef DSFXChorus *LPDSFXChorus;
typedef DSFXChorus *LPCDSFXChorus;

interface IDirectSoundFXChorus: IUnknown
{
    // IDirectSoundFXChorus methods
    DSRESULT SetAllParameters      ([in] LPCDSFXChorus pcDsFxChorus) ;
    DSRESULT GetAllParameters      ([out] LPDSFXChorus pDsFxChorus) ;
};


typedef struct _DSFXFlanger
{
    FLOAT       fWetDryMix;
    FLOAT       fDepth;
    FLOAT       fFeedback;
    FLOAT       fFrequency;
    LONG        lWaveform;
    FLOAT       fDelay;
    LONG        lPhase;
} DSFXFlanger;
typedef DSFXFlanger *LPDSFXFlanger;
typedef DSFXFlanger *LPCDSFXFlanger;

interface IDirectSoundFXFlanger: IUnknown
{
    // IDirectSoundFXFlanger methods
    DSRESULT  SetAllParameters      ([in] LPCDSFXFlanger pcDsFxFlanger) ;
    DSRESULT  GetAllParameters      ([out] LPDSFXFlanger pDsFxFlanger) ;
};

typedef struct _DSFXEcho
{
    FLOAT   fWetDryMix;
    FLOAT   fFeedback;
    FLOAT   fLeftDelay;
    FLOAT   fRightDelay;
    LONG    lPanDelay;
} DSFXEcho;
typedef DSFXEcho *LPDSFXEcho;
typedef DSFXEcho *LPCDSFXEcho;

interface IDirectSoundFXEcho: IUnknown
{
    // IDirectSoundFXEcho methods
    DSRESULT SetAllParameters      ([in] LPCDSFXEcho pcDsFxEcho) ;
    DSRESULT GetAllParameters      ([out] LPDSFXEcho pDsFxEcho) ;
};
typedef struct _DSFXDistortion
{
    FLOAT   fGain;
    FLOAT   fEdge;
    FLOAT   fPostEQCenterFrequency;
    FLOAT   fPostEQBandwidth;
    FLOAT   fPreLowpassCutoff;
} DSFXDistortion;
typedef DSFXDistortion *LPDSFXDistortion;
typedef DSFXDistortion *LPCDSFXDistortion;

interface IDirectSoundFXDistortion: IUnknown
{
    // IDirectSoundFXDistortion methods
    DSRESULT SetAllParameters      ([in] LPCDSFXDistortion pcDsFxDistortion) ;
    DSRESULT GetAllParameters      ([out] LPDSFXDistortion pDsFxDistortion) ;
};

typedef struct _DSFXCompressor
{
    FLOAT   fGain;
    FLOAT   fAttack;
    FLOAT   fRelease;
    FLOAT   fThreshold;
    FLOAT   fRatio;
    FLOAT   fPredelay;
} DSFXCompressor;
typedef DSFXCompressor  *LPDSFXCompressor;
typedef DSFXCompressor *LPCDSFXCompressor;

interface IDirectSoundFXCompressor: IUnknown
{
    // IDirectSoundFXCompressor methods
    DSRESULT SetAllParameters      ([in] LPCDSFXCompressor pcDsFxCompressor) ;
    DSRESULT GetAllParameters      ([out] LPDSFXCompressor pDsFxCompressor) ;
};


typedef struct _DSFXParamEq
{
    FLOAT   fCenter;
    FLOAT   fBandwidth;
    FLOAT   fGain;
} DSFXParamEq;
typedef DSFXParamEq *LPDSFXParamEq;
typedef DSFXParamEq *LPCDSFXParamEq;

interface IDirectSoundFXParamEq: IUnknown
{
    // IDirectSoundFXParamEq methods
    DSRESULT SetAllParameters      ([in] LPCDSFXParamEq pcDsFxParamEq) ;
    DSRESULT GetAllParameters      ([out] LPDSFXParamEq pDsFxParamEq) ;
};


typedef struct _DSFXI3DL2Reverb
{
    LONG    lRoom;                  // [-10000, 0]      default: -1000 mB
    LONG    lRoomHF;                // [-10000, 0]      default: 0 mB
    FLOAT   flRoomRolloffFactor;    // [0.0, 10.0]      default: 0.0
    FLOAT   flDecayTime;            // [0.1, 20.0]      default: 1.49s
    FLOAT   flDecayHFRatio;         // [0.1, 2.0]       default: 0.83
    LONG    lReflections;           // [-10000, 1000]   default: -2602 mB
    FLOAT   flReflectionsDelay;     // [0.0, 0.3]       default: 0.007 s
    LONG    lReverb;                // [-10000, 2000]   default: 200 mB
    FLOAT   flReverbDelay;          // [0.0, 0.1]       default: 0.011 s
    FLOAT   flDiffusion;            // [0.0, 100.0]     default: 100.0 %
    FLOAT   flDensity;              // [0.0, 100.0]     default: 100.0 %
    FLOAT   flHFReference;          // [20.0, 20000.0]  default: 5000.0 Hz
} DSFXI3DL2Reverb;
typedef DSFXI3DL2Reverb *LPDSFXI3DL2Reverb;
typedef DSFXI3DL2Reverb *LPCDSFXI3DL2Reverb;


interface IDirectSoundFXI3DL2Reverb: IUnknown
{
    // IDirectSoundFXI3DL2Reverb methods
    DSRESULT SetAllParameters      ([in] LPCDSFXI3DL2Reverb pcDsFxI3DL2Reverb) ;
    DSRESULT GetAllParameters      ([out] LPDSFXI3DL2Reverb pDsFxI3DL2Reverb) ;
    DSRESULT SetPreset             (DWORD dwPreset) ;
    DSRESULT GetPreset             ([out] LPDWORD pdwPreset) ;
    DSRESULT SetQuality            (LONG lQuality) ;
    DSRESULT GetQuality            ([out] LONG *plQuality) ;
};

typedef struct _DSFXWavesReverb
{
    FLOAT   fInGain;                // [-96.0,0.0]            default: 0.0 dB
    FLOAT   fReverbMix;             // [-96.0,0.0]            default: 0.0 db
    FLOAT   fReverbTime;            // [0.001,3000.0]         default: 1000.0 ms
    FLOAT   fHighFreqRTRatio;       // [0.001,0.999]          default: 0.001
} DSFXWavesReverb;
typedef DSFXWavesReverb *LPDSFXWavesReverb;
typedef DSFXWavesReverb *LPCDSFXWavesReverb;

interface IDirectSoundFXWavesReverb: IUnknown
{
    // IDirectSoundFXWavesReverb methods
    DSRESULT SetAllParameters      ([in] LPCDSFXWavesReverb pcDsFxWavesReverb) ;
    DSRESULT GetAllParameters      ([out] LPDSFXWavesReverb pDsFxWavesReverb) ;
};

typedef struct _DSCFXAec
{
    BOOL    fEnable;
    BOOL    fReset;
} DSCFXAec;
typedef DSCFXAec *LPDSCFXAec;
typedef DSCFXAec *LPCDSCFXAec;

interface IDirectSoundCaptureFXAec: IUnknown
{
    // IDirectSoundCaptureFXAec methods
    DSRESULT SetAllParameters      ([in] LPCDSCFXAec pDscFxAec) ;
    DSRESULT GetAllParameters      ([out] LPDSCFXAec pDscFxAec) ;
};

typedef struct _DSCFXNoiseSuppress
{
    BOOL    fEnable;
    BOOL    fReset;
} DSCFXNoiseSuppress;
typedef DSCFXNoiseSuppress *LPDSCFXNoiseSuppress;
typedef DSCFXNoiseSuppress *LPCDSCFXNoiseSuppress;

interface IDirectSoundCaptureFXNoiseSuppress: IUnknown
{
    // IDirectSoundCaptureFXNoiseSuppress methods
    DSRESULT SetAllParameters      ([in] LPCDSCFXNoiseSuppress pcDscFxNoiseSuppress) ;
    DSRESULT GetAllParameters      ([out] LPDSCFXNoiseSuppress pDscFxNoiseSuppress) ;
};


interface IDirectSoundFullDuplex: IUnknown
{
    // IDirectSoundFullDuplex methods
    DSRESULT Initialize      ([in] LPCGUID pCaptureGuid, [in] LPCGUID pRenderGuid, [in] LPCDSCBUFFERDESC lpDscBufferDesc, [in] LPCDSBUFFERDESC lpDsBufferDesc, HWND hWnd, DWORD dwLevel, [out] LPLPDIRECTSOUNDCAPTUREBUFFER8 lplpDirectSoundCaptureBuffer8, [out] LPLPDIRECTSOUNDBUFFER8 lplpDirectSoundBuffer8) ;
};





//
// IKsPropertySet
//


mask DWORD KSPROPERTY_SUPPORT_MASK
{
#define KSPROPERTY_SUPPORT_GET  0x00000001
#define KSPROPERTY_SUPPORT_SET  0x00000002
};

interface IKsPropertySet: IUnknown
{
    // IKsPropertySet methods
    DSRESULT  Get               (REFGUID rguidPropSet, ULONG ulId, LPVOID pInstanceData, ULONG ulInstanceLength, [out] LPVOID pPropertyData, ULONG ulDataLength, [out] ULONG * pulBytesReturned );
    DSRESULT  Set               (REFGUID rguidPropSet, ULONG ulId, LPVOID pInstanceData, ULONG ulInstanceLength, LPVOID pPropertyData, ULONG ulDataLength );
    DSRESULT  QuerySupport      (REFGUID rguidPropSet, ULONG ulId, ULONG* pulTypeSupport );
};

typedef IKsPropertySet *LPKSPROPERTYSET;

//
// DirectSound API
//

typedef LPVOID LPDSENUMCALLBACKW;
typedef LPVOID LPDSENUMCALLBACKA;

DSRESULT DirectSoundCreate( [in] LPCGUID lpcGuid, [out] LPDIRECTSOUND * ppDS, [in] LPUNKNOWN pUnkOuter );
DSRESULT DirectSoundEnumerateA( LPDSENUMCALLBACKA lpDSEnumCallback, LPVOID lpContext );
DSRESULT DirectSoundEnumerateW( LPDSENUMCALLBACKW lpDSEnumCallback, LPVOID lpContext );
DSRESULT DirectSoundCaptureCreate( [in] LPCGUID lpcGUID, [out] LPDIRECTSOUNDCAPTURE *lplpDSC, [in] LPUNKNOWN pUnkOuter );
DSRESULT DirectSoundCaptureEnumerateA( [in] LPDSENUMCALLBACKA lpDSEnumCallback, [in] LPVOID lpContext );
DSRESULT DirectSoundCaptureEnumerateW( [in] LPDSENUMCALLBACKW lpDSEnumCallback, [in] LPVOID lpContext );

DSRESULT DirectSoundCreate8([in] LPCGUID pcGuidDevice, [out] LPDIRECTSOUND8 *ppDS8, [in] LPUNKNOWN pUnkOuter);
DSRESULT DirectSoundCaptureCreate8([in] LPCGUID pcGuidDevice, [out] LPDIRECTSOUNDCAPTURE *ppDSC8, [in] LPUNKNOWN pUnkOuter);
DSRESULT DirectSoundFullDuplexCreate([in] LPCGUID pcGuidCaptureDevice, [in] LPCGUID pcGuidRenderDevice, [in] LPCDSCBUFFERDESC pcDSCBufferDesc, [in] LPCDSBUFFERDESC pcDSBufferDesc, HWND hWnd, DWORD dwLevel, [out] LPDIRECTSOUNDFULLDUPLEX* ppDSFD, [out] LPDIRECTSOUNDCAPTUREBUFFER8 *ppDSCBuffer8, [out] LPDIRECTSOUNDBUFFER8 *ppDSBuffer8, [in] LPUNKNOWN pUnkOuter);
DSRESULT GetDeviceID([in] LPCGUID pGuidSrc, [out] LPGUID pGuidDest);
