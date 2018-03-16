
mask DWORD PlaySoundFlags
{
#define SND_SYNC            0x00000000L  /* play synchronously (default) */
#define SND_ASYNC           0x00000001L  /* play asynchronously */
#define SND_NODEFAULT       0x00000002L  /* silence (!default) if sound not found */
#define SND_MEMORY          0x00000004L  /* pszSound points to a memory file */
#define SND_LOOP            0x00000008L  /* loop the sound until next sndPlaySound */
#define SND_NOSTOP          0x00000010L  /* don't stop any currently playing sound */

#define SND_PURGE           0x00000040L  /* purge non-static events for task */
#define SND_APPLICATION     0x00000080L  /* look for application specific association */

#define SND_NOWAIT	        0x00002000L /* don't wait if the driver is busy */
#define SND_ALIAS           0x00010000L /* name is a registry alias */
#define SND_FILENAME        0x00020000L /* name is file name */
#define SND_RESOURCE        0x00040000L /* name is resource name or atom */

#define SND_ALIAS_ID	    0x00100000L /* alias is a predefined ID */
};

mask DWORD WaveOutCapsFlags
{
#define WAVECAPS_PITCH          0x0001   /* supports pitch control */
#define WAVECAPS_PLAYBACKRATE   0x0002   /* supports playback rate control */
#define WAVECAPS_VOLUME         0x0004   /* supports volume control */
#define WAVECAPS_LRVOLUME       0x0008   /* separate left-right volume control */
#define WAVECAPS_SYNC           0x0010
#define WAVECAPS_SAMPLEACCURATE 0x0020
#define WAVECAPS_DIRECTSOUND    0x0040
};

mask DWORD WaveCapsFormatFlags
{
#define WAVE_INVALIDFORMAT     0x00000000       /* invalid format */
#define WAVE_FORMAT_1M08       0x00000001       /* 11.025 kHz, Mono,   8-bit  */
#define WAVE_FORMAT_1S08       0x00000002       /* 11.025 kHz, Stereo, 8-bit  */
#define WAVE_FORMAT_1M16       0x00000004       /* 11.025 kHz, Mono,   16-bit */
#define WAVE_FORMAT_1S16       0x00000008       /* 11.025 kHz, Stereo, 16-bit */
#define WAVE_FORMAT_2M08       0x00000010       /* 22.05  kHz, Mono,   8-bit  */
#define WAVE_FORMAT_2S08       0x00000020       /* 22.05  kHz, Stereo, 8-bit  */
#define WAVE_FORMAT_2M16       0x00000040       /* 22.05  kHz, Mono,   16-bit */
#define WAVE_FORMAT_2S16       0x00000080       /* 22.05  kHz, Stereo, 16-bit */
#define WAVE_FORMAT_4M08       0x00000100       /* 44.1   kHz, Mono,   8-bit  */
#define WAVE_FORMAT_4S08       0x00000200       /* 44.1   kHz, Stereo, 8-bit  */
#define WAVE_FORMAT_4M16       0x00000400       /* 44.1   kHz, Mono,   16-bit */
#define WAVE_FORMAT_4S16       0x00000800       /* 44.1   kHz, Stereo, 16-bit */
};

mask DWORD WAVE_OUT_OPEN_FLAGS
{
/* flags for dwFlags parameter in waveOutOpen() and waveInOpen() */
#define  WAVE_FORMAT_QUERY         0x0001
#define  WAVE_ALLOWSYNC            0x0002
#define  WAVE_MAPPED               0x0004
#define  WAVE_FORMAT_DIRECT        0x0008
#define CALLBACK_WINDOW     0x000100000    /* dwCallback is a HWND */
#define CALLBACK_TASK       0x000200000    /* dwCallback is a HTASK */
#define CALLBACK_FUNCTION   0x000300000    /* dwCallback is a FARPROC */
};

value DWORD MMRESULT_VALUES
{
#define MMSYSERR_NOERROR      0
#define MMSYSERR_ERROR        1     [fail]
#define MMSYSERR_BADDEVICEID  2     [fail]
#define MMSYSERR_NOTENABLED   3     [fail]
#define MMSYSERR_ALLOCATED    4     [fail]
#define MMSYSERR_INVALHANDLE  5     [fail]
#define MMSYSERR_NODRIVER     6     [fail]
#define MMSYSERR_NOMEM        7     [fail]
#define MMSYSERR_NOTSUPPORTED 8     [fail]
#define MMSYSERR_BADERRNUM    9     [fail]
#define MMSYSERR_INVALFLAG    10     [fail]
#define MMSYSERR_INVALPARAM   11     [fail]
#define MMSYSERR_HANDLEBUSY   12     [fail]
#define MMSYSERR_INVALIDALIAS 13     [fail]
#define MMSYSERR_BADDB        14     [fail]
#define MMSYSERR_KEYNOTFOUND  15     [fail]
#define MMSYSERR_READERROR    16     [fail]
#define MMSYSERR_WRITEERROR   17     [fail]
#define MMSYSERR_DELETEERROR  18     [fail]
#define MMSYSERR_VALNOTFOUND  19     [fail]
#define MMSYSERR_NODRIVERCB   20     [fail]
#define MMSYSERR_LASTERROR    20     [fail]
};

value UINT PRODUCT_ID_VALUE
{
/* MM_MICROSOFT product IDs */
#define  MM_MIDI_MAPPER                     1       /*  Midi Mapper  */
#define  MM_WAVE_MAPPER                     2       /*  Wave Mapper  */
#define  MM_SNDBLST_MIDIOUT                 3       /*  Sound Blaster MIDI output port  */
#define  MM_SNDBLST_MIDIIN                  4       /*  Sound Blaster MIDI input port  */
#define  MM_SNDBLST_SYNTH                   5       /*  Sound Blaster internal synth  */
#define  MM_SNDBLST_WAVEOUT                 6       /*  Sound Blaster waveform output  */
#define  MM_SNDBLST_WAVEIN                  7       /*  Sound Blaster waveform input  */
#define  MM_ADLIB                           9       /*  Ad Lib Compatible synth  */
#define  MM_MPU401_MIDIOUT                  10      /*  MPU 401 compatible MIDI output port  */
#define  MM_MPU401_MIDIIN                   11      /*  MPU 401 compatible MIDI input port  */
#define  MM_PC_JOYSTICK                     12      /*  Joystick adapter  */
#define  MM_PCSPEAKER_WAVEOUT               13      /*  PC speaker waveform output  */
#define  MM_MSFT_WSS_WAVEIN                 14      /*  MS Audio Board waveform input  */
#define  MM_MSFT_WSS_WAVEOUT                15      /*  MS Audio Board waveform output  */
#define  MM_MSFT_WSS_FMSYNTH_STEREO         16      /*  MS Audio Board  Stereo FM synth  */
#define  MM_MSFT_WSS_MIXER                  17      /*  MS Audio Board Mixer Driver  */
#define  MM_MSFT_WSS_OEM_WAVEIN             18      /*  MS OEM Audio Board waveform input  */
#define  MM_MSFT_WSS_OEM_WAVEOUT            19      /*  MS OEM Audio Board waveform output  */
#define  MM_MSFT_WSS_OEM_FMSYNTH_STEREO     20      /*  MS OEM Audio Board Stereo FM Synth  */
#define  MM_MSFT_WSS_AUX                    21      /*  MS Audio Board Aux. Port  */
#define  MM_MSFT_WSS_OEM_AUX                22      /*  MS OEM Audio Aux Port  */
#define  MM_MSFT_GENERIC_WAVEIN             23      /*  MS Vanilla driver waveform input  */
#define  MM_MSFT_GENERIC_WAVEOUT            24      /*  MS Vanilla driver wavefrom output  */
#define  MM_MSFT_GENERIC_MIDIIN             25      /*  MS Vanilla driver MIDI in  */
#define  MM_MSFT_GENERIC_MIDIOUT            26      /*  MS Vanilla driver MIDI  external out  */
#define  MM_MSFT_GENERIC_MIDISYNTH          27      /*  MS Vanilla driver MIDI synthesizer  */
#define  MM_MSFT_GENERIC_AUX_LINE           28      /*  MS Vanilla driver aux (line in)  */
#define  MM_MSFT_GENERIC_AUX_MIC            29      /*  MS Vanilla driver aux (mic)  */
#define  MM_MSFT_GENERIC_AUX_CD             30      /*  MS Vanilla driver aux (CD)  */
#define  MM_MSFT_WSS_OEM_MIXER              31      /*  MS OEM Audio Board Mixer Driver  */
#define  MM_MSFT_MSACM                      32      /*  MS Audio Compression Manager  */
#define  MM_MSFT_ACM_MSADPCM                33      /*  MS ADPCM Codec  */
#define  MM_MSFT_ACM_IMAADPCM               34      /*  IMA ADPCM Codec  */
#define  MM_MSFT_ACM_MSFILTER               35      /*  MS Filter  */
#define  MM_MSFT_ACM_GSM610                 36      /*  GSM 610 codec  */
#define  MM_MSFT_ACM_G711                   37      /*  G.711 codec  */
#define  MM_MSFT_ACM_PCM                    38      /*  PCM converter  */

   // Microsoft Windows Sound System drivers

#define  MM_WSS_SB16_WAVEIN                 39      /*  Sound Blaster 16 waveform input  */
#define  MM_WSS_SB16_WAVEOUT                40      /*  Sound Blaster 16  waveform output  */
#define  MM_WSS_SB16_MIDIIN                 41      /*  Sound Blaster 16 midi-in  */
#define  MM_WSS_SB16_MIDIOUT                42      /*  Sound Blaster 16 midi out  */
#define  MM_WSS_SB16_SYNTH                  43      /*  Sound Blaster 16 FM Synthesis  */
#define  MM_WSS_SB16_AUX_LINE               44      /*  Sound Blaster 16 aux (line in)  */
#define  MM_WSS_SB16_AUX_CD                 45      /*  Sound Blaster 16 aux (CD)  */
#define  MM_WSS_SB16_MIXER                  46      /*  Sound Blaster 16 mixer device  */
#define  MM_WSS_SBPRO_WAVEIN                47      /*  Sound Blaster Pro waveform input  */
#define  MM_WSS_SBPRO_WAVEOUT               48      /*  Sound Blaster Pro waveform output  */
#define  MM_WSS_SBPRO_MIDIIN                49      /*  Sound Blaster Pro midi in  */
#define  MM_WSS_SBPRO_MIDIOUT               50      /*  Sound Blaster Pro midi out  */
#define  MM_WSS_SBPRO_SYNTH                 51      /*  Sound Blaster Pro FM synthesis  */
#define  MM_WSS_SBPRO_AUX_LINE              52      /*  Sound Blaster Pro aux (line in )  */
#define  MM_WSS_SBPRO_AUX_CD                53      /*  Sound Blaster Pro aux (CD)  */
#define  MM_WSS_SBPRO_MIXER                 54      /*  Sound Blaster Pro mixer  */

#define  MM_MSFT_WSS_NT_WAVEIN              55      /*  WSS NT wave in  */
#define  MM_MSFT_WSS_NT_WAVEOUT             56      /*  WSS NT wave out  */
#define  MM_MSFT_WSS_NT_FMSYNTH_STEREO      57      /*  WSS NT FM synth  */
#define  MM_MSFT_WSS_NT_MIXER               58      /*  WSS NT mixer  */
#define  MM_MSFT_WSS_NT_AUX                 59      /*  WSS NT aux  */

#define  MM_MSFT_SB16_WAVEIN                60      /*  Sound Blaster 16 waveform input  */
#define  MM_MSFT_SB16_WAVEOUT               61      /*  Sound Blaster 16  waveform output  */
#define  MM_MSFT_SB16_MIDIIN                62      /*  Sound Blaster 16 midi-in  */
#define  MM_MSFT_SB16_MIDIOUT               63      /*  Sound Blaster 16 midi out  */
#define  MM_MSFT_SB16_SYNTH                 64      /*  Sound Blaster 16 FM Synthesis  */
#define  MM_MSFT_SB16_AUX_LINE              65      /*  Sound Blaster 16 aux (line in)  */
#define  MM_MSFT_SB16_AUX_CD                66      /*  Sound Blaster 16 aux (CD)  */
#define  MM_MSFT_SB16_MIXER                 67      /*  Sound Blaster 16 mixer device  */
#define  MM_MSFT_SBPRO_WAVEIN               68      /*  Sound Blaster Pro waveform input  */
#define  MM_MSFT_SBPRO_WAVEOUT              69      /*  Sound Blaster Pro waveform output  */
#define  MM_MSFT_SBPRO_MIDIIN               70      /*  Sound Blaster Pro midi in  */
#define  MM_MSFT_SBPRO_MIDIOUT              71      /*  Sound Blaster Pro midi out  */
#define  MM_MSFT_SBPRO_SYNTH                72      /*  Sound Blaster Pro FM synthesis  */
#define  MM_MSFT_SBPRO_AUX_LINE             73      /*  Sound Blaster Pro aux (line in )  */
#define  MM_MSFT_SBPRO_AUX_CD               74      /*  Sound Blaster Pro aux (CD)  */
#define  MM_MSFT_SBPRO_MIXER                75      /*  Sound Blaster Pro mixer  */

#define  MM_MSFT_MSOPL_SYNTH                76      /* Yamaha OPL2/OPL3 compatible FM synthesis */

#define  MM_MSFT_VMDMS_LINE_WAVEIN          80     /* Voice Modem Serial Line Wave Input */
#define  MM_MSFT_VMDMS_LINE_WAVEOUT         81     /* Voice Modem Serial Line Wave Output */
#define  MM_MSFT_VMDMS_HANDSET_WAVEIN       82     /* Voice Modem Serial Handset Wave Input */
#define  MM_MSFT_VMDMS_HANDSET_WAVEOUT      83     /* Voice Modem Serial Handset Wave Output */
#define  MM_MSFT_VMDMW_LINE_WAVEIN          84     /* Voice Modem Wrapper Line Wave Input */
#define  MM_MSFT_VMDMW_LINE_WAVEOUT         85     /* Voice Modem Wrapper Line Wave Output */
#define  MM_MSFT_VMDMW_HANDSET_WAVEIN       86     /* Voice Modem Wrapper Handset Wave Input */
#define  MM_MSFT_VMDMW_HANDSET_WAVEOUT      87     /* Voice Modem Wrapper Handset Wave Output */
#define  MM_MSFT_VMDMW_MIXER                88     /* Voice Modem Wrapper Mixer */
#define  MM_MSFT_VMDM_GAME_WAVEOUT          89     /* Voice Modem Game Compatible Wave Device */
#define  MM_MSFT_VMDM_GAME_WAVEIN           90     /* Voice Modem Game Compatible Wave Device */

#define  MM_MSFT_ACM_MSNAUDIO               91     /* */
#define  MM_MSFT_ACM_MSG723                 92     /* */

#define  MM_MSFT_WDMAUDIO_WAVEOUT           100    /* Generic id for WDM Audio drivers */
#define  MM_MSFT_WDMAUDIO_WAVEIN            101    /* Generic id for WDM Audio drivers */
#define  MM_MSFT_WDMAUDIO_MIDIOUT           102    /* Generic id for WDM Audio drivers */
#define  MM_MSFT_WDMAUDIO_MIDIIN            103    /* Generic id for WDM Audio drivers */
#define  MM_MSFT_WDMAUDIO_MIXER             104    /* Generic id for WDM Audio drivers */
};

typedef UINT        MMVERSION;  /* major (high byte), minor (low byte) */
typedef UINT       *LPUINT;
typedef HANDLE      HWAVEOUT;
typedef HANDLE      HWAVEIN;
typedef UINT        MMRESULT;   /* error return code, 0 means no error */

typedef struct mmtime_tag
{
    UINT            wType;      /* indicates the contents of the union */
	DWORD       ms;         /* milliseconds */
	DWORD       sample;     /* samples */
	DWORD       cb;         /* byte count */
	DWORD       ticks;      /* ticks in MIDI stream */

} MMTIME;
typedef MMTIME *PMMTIME;
typedef MMTIME *NPMMTIME;
typedef MMTIME *LPMMTIME;

typedef struct wavehdr_tag {
    LPSTR       lpData;                 /* pointer to locked data buffer */
    DWORD       dwBufferLength;         /* length of data buffer */
    DWORD       dwBytesRecorded;        /* used for input only */
    DWORD       dwUser;                 /* for client's use */
    DWORD       dwFlags;                /* assorted flags (see defines) */
    DWORD       dwLoops;                /* loop control counter */
    DWORD       lpNext;     /* reserved for driver */
    DWORD       reserved;               /* reserved for driver */
} WAVEHDR;
typedef WAVEHDR *PWAVEHDR;
typedef WAVEHDR *NPWAVEHDR;
typedef WAVEHDR *LPWAVEHDR;

typedef struct tagWAVEOUTCAPSA {
    WORD    wMid;                  /* manufacturer ID */
    WORD    wPid;                  /* product ID */
    MMVERSION vDriverVersion;      /* version of the driver */
    CHAR    szPname[32];  /* product name (NULL terminated string) */
    DWORD   dwFormats;             /* formats supported */
    WORD    wChannels;             /* number of sources supported */
    WORD    wReserved1;            /* packing */
    DWORD   dwSupport;             /* functionality supported by driver */
} WAVEOUTCAPSA;
typedef WAVEOUTCAPSA *PWAVEOUTCAPSA;
typedef WAVEOUTCAPSA *NPWAVEOUTCAPSA;
typedef WAVEOUTCAPSA *LPWAVEOUTCAPSA;


typedef struct tagWAVEOUTCAPSW {
    WORD    wMid;                  /* manufacturer ID */
    WORD    wPid;                  /* product ID */
    MMVERSION vDriverVersion;      /* version of the driver */
    WCHAR   szPname[32];  /* product name (NULL terminated string) */
    DWORD   dwFormats;             /* formats supported */
    WORD    wChannels;             /* number of sources supported */
    WORD    wReserved1;            /* packing */
    DWORD   dwSupport;             /* functionality supported by driver */
} WAVEOUTCAPSW;
typedef WAVEOUTCAPSW *PWAVEOUTCAPSW;
typedef WAVEOUTCAPSW  *NPWAVEOUTCAPSW;
typedef WAVEOUTCAPSW  *LPWAVEOUTCAPSW;


//typedef void (CALLBACK DRVCALLBACK)(HDRVR hdrvr, UINT uMsg, DWORD dwUser, DWORD dw1, DWORD dw2);
typedef DWORD DRVCALLBACK;


typedef HWAVEIN *LPHWAVEIN;
typedef HWAVEOUT *LPHWAVEOUT;
typedef DRVCALLBACK WAVECALLBACK;
typedef WAVECALLBACK *LPWAVECALLBACK;

value WORD FORMAT_TAG_VALUE
{
#define  WAVE_FORMAT_UNKNOWN    0x0000  /*  Microsoft Corporation  */
#define  WAVE_FORMAT_ADPCM      0x0002  /*  Microsoft Corporation  */
#define  WAVE_FORMAT_IEEE_FLOAT 0x0003  /*  Microsoft Corporation  */
                                        /*  IEEE754: range (+1, -1]  */
                                        /*  32-bit/64-bit format as defined by */
                                        /*  MSVC++ float/double type */
#define  WAVE_FORMAT_IBM_CVSD   0x0005  /*  IBM Corporation  */
#define  WAVE_FORMAT_ALAW       0x0006  /*  Microsoft Corporation  */
#define  WAVE_FORMAT_MULAW      0x0007  /*  Microsoft Corporation  */
#define  WAVE_FORMAT_OKI_ADPCM  0x0010  /*  OKI  */
#define  WAVE_FORMAT_DVI_ADPCM  0x0011  /*  Intel Corporation  */
#define  WAVE_FORMAT_IMA_ADPCM  0x0011 /*  Intel Corporation  */
#define  WAVE_FORMAT_MEDIASPACE_ADPCM   0x0012  /*  Videologic  */
#define  WAVE_FORMAT_SIERRA_ADPCM       0x0013  /*  Sierra Semiconductor Corp  */
#define  WAVE_FORMAT_G723_ADPCM 0x0014  /*  Antex Electronics Corporation  */
#define  WAVE_FORMAT_DIGISTD    0x0015  /*  DSP Solutions, Inc.  */
#define  WAVE_FORMAT_DIGIFIX    0x0016  /*  DSP Solutions, Inc.  */
#define  WAVE_FORMAT_DIALOGIC_OKI_ADPCM 0x0017  /*  Dialogic Corporation  */
#define  WAVE_FORMAT_MEDIAVISION_ADPCM  0x0018  /*  Media Vision, Inc. */
#define  WAVE_FORMAT_YAMAHA_ADPCM       0x0020  /*  Yamaha Corporation of America  */
#define  WAVE_FORMAT_SONARC     0x0021  /*  Speech Compression  */
#define  WAVE_FORMAT_DSPGROUP_TRUESPEECH        0x0022  /*  DSP Group, Inc  */
#define  WAVE_FORMAT_ECHOSC1    0x0023  /*  Echo Speech Corporation  */
#define  WAVE_FORMAT_AUDIOFILE_AF36     0x0024  /*    */
#define  WAVE_FORMAT_APTX       0x0025  /*  Audio Processing Technology  */
#define  WAVE_FORMAT_AUDIOFILE_AF10     0x0026  /*    */
#define  WAVE_FORMAT_DOLBY_AC2  0x0030  /*  Dolby Laboratories  */
#define  WAVE_FORMAT_GSM610     0x0031  /*  Microsoft Corporation  */
#define  WAVE_FORMAT_MSNAUDIO   0x0032  /*  Microsoft Corporation  */
#define  WAVE_FORMAT_ANTEX_ADPCME       0x0033  /*  Antex Electronics Corporation  */
#define  WAVE_FORMAT_CONTROL_RES_VQLPC  0x0034  /*  Control Resources Limited  */
#define  WAVE_FORMAT_DIGIREAL   0x0035  /*  DSP Solutions, Inc.  */
#define  WAVE_FORMAT_DIGIADPCM  0x0036  /*  DSP Solutions, Inc.  */
#define  WAVE_FORMAT_CONTROL_RES_CR10   0x0037  /*  Control Resources Limited  */
#define  WAVE_FORMAT_NMS_VBXADPCM       0x0038  /*  Natural MicroSystems  */
#define  WAVE_FORMAT_CS_IMAADPCM 0x0039 /* Crystal Semiconductor IMA ADPCM */
#define  WAVE_FORMAT_ECHOSC3     0x003A /* Echo Speech Corporation */
#define  WAVE_FORMAT_ROCKWELL_ADPCM     0x003B  /* Rockwell International */
#define  WAVE_FORMAT_ROCKWELL_DIGITALK  0x003C  /* Rockwell International */
#define  WAVE_FORMAT_XEBEC      0x003D  /* Xebec Multimedia Solutions Limited */
#define  WAVE_FORMAT_G721_ADPCM 0x0040  /*  Antex Electronics Corporation  */
#define  WAVE_FORMAT_G728_CELP  0x0041  /*  Antex Electronics Corporation  */
#define  WAVE_FORMAT_MPEG       0x0050  /*  Microsoft Corporation  */
#define  WAVE_FORMAT_MPEGLAYER3 0x0055  /*  ISO/MPEG Layer3 Format Tag */
#define  WAVE_FORMAT_CIRRUS     0x0060  /*  Cirrus Logic  */
#define  WAVE_FORMAT_ESPCM      0x0061  /*  ESS Technology  */
#define  WAVE_FORMAT_VOXWARE    0x0062  /*  Voxware Inc  */
#define  WAVEFORMAT_CANOPUS_ATRAC       0x0063  /*  Canopus, co., Ltd.  */
#define  WAVE_FORMAT_G726_ADPCM 0x0064  /*  APICOM  */
#define  WAVE_FORMAT_G722_ADPCM 0x0065  /*  APICOM      */
#define  WAVE_FORMAT_DSAT       0x0066  /*  Microsoft Corporation  */
#define  WAVE_FORMAT_DSAT_DISPLAY       0x0067  /*  Microsoft Corporation  */
#define  WAVE_FORMAT_SOFTSOUND  0x0080  /*  Softsound, Ltd.      */
#define  WAVE_FORMAT_RHETOREX_ADPCM     0x0100  /*  Rhetorex Inc  */
#define  WAVE_FORMAT_CREATIVE_ADPCM     0x0200  /*  Creative Labs, Inc  */
#define  WAVE_FORMAT_CREATIVE_FASTSPEECH8       0x0202  /*  Creative Labs, Inc  */
#define  WAVE_FORMAT_CREATIVE_FASTSPEECH10      0x0203  /*  Creative Labs, Inc  */
#define  WAVE_FORMAT_QUARTERDECK 0x0220 /*  Quarterdeck Corporation  */
#define  WAVE_FORMAT_FM_TOWNS_SND       0x0300  /*  Fujitsu Corp.  */
#define  WAVE_FORMAT_BTV_DIGITAL        0x0400  /*  Brooktree Corporation  */
#define  WAVE_FORMAT_OLIGSM     0x1000  /*  Ing C. Olivetti & C., S.p.A.  */
#define  WAVE_FORMAT_OLIADPCM   0x1001  /*  Ing C. Olivetti & C., S.p.A.  */
#define  WAVE_FORMAT_OLICELP    0x1002  /*  Ing C. Olivetti & C., S.p.A.  */
#define  WAVE_FORMAT_OLISBC     0x1003  /*  Ing C. Olivetti & C., S.p.A.  */
#define  WAVE_FORMAT_OLIOPR     0x1004  /*  Ing C. Olivetti & C., S.p.A.  */
#define  WAVE_FORMAT_LH_CODEC   0x1100  /*  Lernout & Hauspie  */
#define  WAVE_FORMAT_NORRIS     0x1400  /*  Norris Communications, Inc.  */

};

typedef struct waveformat_tag {
    FORMAT_TAG_VALUE    wFormatTag;        /* format type */
    WORD    nChannels;         /* number of channels (i.e. mono, stereo, etc.) */
    DWORD   nSamplesPerSec;    /* sample rate */
    DWORD   nAvgBytesPerSec;   /* for buffer estimation */
    WORD    nBlockAlign;       /* block size of data */
} WAVEFORMAT;
typedef WAVEFORMAT *PWAVEFORMAT;
typedef WAVEFORMAT *NPWAVEFORMAT;
typedef WAVEFORMAT *LPWAVEFORMAT;

typedef struct pcmwaveformat_tag {
    WAVEFORMAT  wf;
    WORD        wBitsPerSample;
} PCMWAVEFORMAT;
typedef PCMWAVEFORMAT *PPCMWAVEFORMAT;
typedef PCMWAVEFORMAT *NPPCMWAVEFORMAT;
typedef PCMWAVEFORMAT *LPPCMWAVEFORMAT;

typedef struct tWAVEFORMATEX
{
    WORD        wFormatTag;         /* format type */
    WORD        nChannels;          /* number of channels (i.e. mono, stereo...) */
    DWORD       nSamplesPerSec;     /* sample rate */
    DWORD       nAvgBytesPerSec;    /* for buffer estimation */
    WORD        nBlockAlign;        /* block size of data */
    WORD        wBitsPerSample;     /* number of bits per sample of mono data */
    WORD        cbSize;             /* the count in bytes of the size of */
				    /* extra information (after cbSize) */
} WAVEFORMATEX;
typedef WAVEFORMATEX *PWAVEFORMATEX;
typedef WAVEFORMATEX *NPWAVEFORMATEX;
typedef WAVEFORMATEX *LPWAVEFORMATEX;
typedef WAVEFORMATEX *LPCWAVEFORMATEX;

typedef struct tagWAVEINCAPSA {
    WORD    wMid;                    /* manufacturer ID */
    WORD    wPid;                    /* product ID */
    MMVERSION vDriverVersion;        /* version of the driver */
    CHAR    szPname[32];    /* product name (NULL terminated string) */
    DWORD   dwFormats;               /* formats supported */
    WORD    wChannels;               /* number of channels supported */
    WORD    wReserved1;              /* structure packing */
} WAVEINCAPSA, *PWAVEINCAPSA, *NPWAVEINCAPSA, *LPWAVEINCAPSA;
typedef struct tagWAVEINCAPSW {
    WORD    wMid;                    /* manufacturer ID */
    WORD    wPid;                    /* product ID */
    MMVERSION vDriverVersion;        /* version of the driver */
    WCHAR   szPname[32];    /* product name (NULL terminated string) */
    DWORD   dwFormats;               /* formats supported */
    WORD    wChannels;               /* number of channels supported */
    WORD    wReserved1;              /* structure packing */
} WAVEINCAPSW;

typedef WAVEINCAPSW *PWAVEINCAPSW;
typedef WAVEINCAPSW *NPWAVEINCAPSW;
typedef WAVEINCAPSW *LPWAVEINCAPSW;

category Multimedia:
module WINMM.DLL:

BOOL                    sndPlaySoundA(LPCSTR pszSound, PlaySoundFlags fuSound);
BOOL                    sndPlaySoundW(LPCWSTR pszSound, PlaySoundFlags fuSound);
BOOL                    PlaySound(LPCSTR pszSound, HMODULE hmod, PlaySoundFlags fdwSound);
BOOL                    PlaySoundA(LPCSTR pszSound, HMODULE hmod, PlaySoundFlags fdwSound);
BOOL                    PlaySoundW(LPCWSTR pszSound, HMODULE hmod, PlaySoundFlags fdwSound);

MMRESULT                waveOutGetDevCapsA(PRODUCT_ID_VALUE uDeviceID, [out] LPWAVEOUTCAPSA pwoc, UINT cbwoc);
MMRESULT_VALUES         waveOutGetDevCapsW(PRODUCT_ID_VALUE uDeviceID, [out] LPWAVEOUTCAPSW pwoc, UINT cbwoc);
MMRESULT_VALUES         waveOutGetVolume(HWAVEOUT hwo, [out] LPDWORD pdwVolume);
MMRESULT_VALUES         waveOutSetVolume(HWAVEOUT hwo, DWORD dwVolume);
MMRESULT_VALUES         waveOutGetErrorTextA(MMRESULT_VALUES mmrError, [out] LPSTR pszText, UINT cchText);
MMRESULT_VALUES         waveOutGetErrorTextW(MMRESULT_VALUES mmrError, [out] LPWSTR pszText, UINT cchText);
MMRESULT_VALUES         waveOutOpen(LPHWAVEOUT phwo, PRODUCT_ID_VALUE uDeviceID, LPCWAVEFORMATEX pwfx, DWORD dwCallback, DWORD dwInstance, WAVE_OUT_OPEN_FLAGS fdwOpen);
UINT                    waveOutGetNumDevs();
MMRESULT_VALUES         waveOutClose(HWAVEOUT hwo);
MMRESULT_VALUES         waveOutPrepareHeader(HWAVEOUT hwo, LPWAVEHDR pwh, UINT cbwh);
MMRESULT_VALUES         waveOutUnprepareHeader(HWAVEOUT hwo, LPWAVEHDR pwh, UINT cbwh);
MMRESULT_VALUES         waveOutWrite(HWAVEOUT hwo, LPWAVEHDR pwh, UINT cbwh);
MMRESULT_VALUES         waveOutPause(HWAVEOUT hwo);
MMRESULT_VALUES         waveOutRestart(HWAVEOUT hwo);
MMRESULT_VALUES         waveOutReset(HWAVEOUT hwo);
MMRESULT_VALUES         waveOutBreakLoop(HWAVEOUT hwo);
MMRESULT_VALUES         waveOutGetPosition(HWAVEOUT hwo, [out] LPMMTIME pmmt, UINT cbmmt);
MMRESULT_VALUES         waveOutGetPitch(HWAVEOUT hwo, [out] LPDWORD pdwPitch);
MMRESULT_VALUES         waveOutSetPitch(HWAVEOUT hwo, DWORD dwPitch);
MMRESULT_VALUES         waveOutGetPlaybackRate(HWAVEOUT hwo, [out] LPDWORD pdwRate);
MMRESULT_VALUES         waveOutSetPlaybackRate(HWAVEOUT hwo, DWORD dwRate);
MMRESULT_VALUES         waveOutGetID(HWAVEOUT hwo, [out] LPUINT puDeviceID);
MMRESULT_VALUES         waveOutMessage(HWAVEOUT hwo, UINT uMsg, DWORD dw1, DWORD dw2);


MMRESULT_VALUES         waveInGetDevCapsA(PRODUCT_ID_VALUE uDeviceID, [out] LPWAVEINCAPSA pwic, UINT cbwic);
MMRESULT_VALUES         waveInGetDevCapsW(PRODUCT_ID_VALUE uDeviceID, [out] LPWAVEINCAPSW pwic, UINT cbwic);
MMRESULT_VALUES         waveInGetErrorTextA(MMRESULT_VALUES mmrError, [out] LPSTR pszText, UINT cchText);
MMRESULT_VALUES         waveInGetErrorTextW(MMRESULT_VALUES mmrError, [out] LPWSTR pszText, UINT cchText);
MMRESULT_VALUES         waveInOpen(LPHWAVEIN phwi, PRODUCT_ID_VALUE uDeviceID, LPCWAVEFORMATEX pwfx, DWORD dwCallback, DWORD dwInstance, WAVE_OUT_OPEN_FLAGS fdwOpen);
MMRESULT_VALUES         waveInClose(HWAVEIN hwi);
MMRESULT_VALUES         waveInPrepareHeader(HWAVEIN hwi, LPWAVEHDR pwh, UINT cbwh);
MMRESULT_VALUES         waveInUnprepareHeader(HWAVEIN hwi, LPWAVEHDR pwh, UINT cbwh);
MMRESULT_VALUES         waveInAddBuffer(HWAVEIN hwi, LPWAVEHDR pwh, UINT cbwh);
MMRESULT_VALUES         waveInStart(HWAVEIN hwi);
MMRESULT_VALUES         waveInStop(HWAVEIN hwi);
MMRESULT_VALUES         waveInReset(HWAVEIN hwi);
MMRESULT_VALUES         waveInGetPosition(HWAVEIN hwi, [out] LPMMTIME pmmt, UINT cbmmt);
MMRESULT_VALUES         waveInGetID(HWAVEIN hwi, [out] LPUINT puDeviceID);

value DWORD MCIERROR
{
#define MCIERR_NO_ERROR                  0
#define MCIERR_UNRECOGNIZED_KEYWORD      259
#define MCIERR_UNRECOGNIZED_COMMAND      261
#define MCIERR_HARDWARE                  262
#define MCIERR_INVALID_DEVICE_NAME       263
#define MCIERR_OUT_OF_MEMORY             264
#define MCIERR_DEVICE_OPEN               265
#define MCIERR_CANNOT_LOAD_DRIVER        266
#define MCIERR_MISSING_COMMAND_STRING    267
#define MCIERR_PARAM_OVERFLOW            268
#define MCIERR_MISSING_STRING_ARGUMENT   269
#define MCIERR_BAD_INTEGER               270
#define MCIERR_PARSER_INTERNAL           271
#define MCIERR_DRIVER_INTERNAL           272
#define MCIERR_MISSING_PARAMETER         273
#define MCIERR_UNSUPPORTED_FUNCTION      274
#define MCIERR_FILE_NOT_FOUND            275
#define MCIERR_DEVICE_NOT_READY          276
#define MCIERR_INTERNAL                  277
#define MCIERR_DRIVER                    278
#define MCIERR_CANNOT_USE_ALL            279
#define MCIERR_MULTIPLE                  280
#define MCIERR_EXTENSION_NOT_FOUND       281
#define MCIERR_OUTOFRANGE                282
#define MCIERR_FLAGS_NOT_COMPATIBLE      284
#define MCIERR_FILE_NOT_SAVED            286
#define MCIERR_DEVICE_TYPE_REQUIRED      287
#define MCIERR_DEVICE_LOCKED             288
#define MCIERR_DUPLICATE_ALIAS           289
#define MCIERR_BAD_CONSTANT              290
#define MCIERR_MUST_USE_SHAREABLE        291
#define MCIERR_MISSING_DEVICE_NAME       292
#define MCIERR_BAD_TIME_FORMAT           293
#define MCIERR_NO_CLOSING_QUOTE          294
#define MCIERR_DUPLICATE_FLAGS           295
#define MCIERR_INVALID_FILE              296
#define MCIERR_NULL_PARAMETER_BLOCK      297
#define MCIERR_UNNAMED_RESOURCE          298
#define MCIERR_NEW_REQUIRES_ALIAS        299
#define MCIERR_NOTIFY_ON_AUTO_OPEN       300
#define MCIERR_NO_ELEMENT_ALLOWED        301
#define MCIERR_NONAPPLICABLE_FUNCTION    302
#define MCIERR_ILLEGAL_FOR_AUTO_OPEN     303
#define MCIERR_FILENAME_REQUIRED         304
#define MCIERR_EXTRA_CHARACTERS          305
#define MCIERR_DEVICE_NOT_INSTALLED      306
#define MCIERR_GET_CD                    307
#define MCIERR_SET_CD                    308
#define MCIERR_SET_DRIVE                 309
#define MCIERR_DEVICE_LENGTH             310
#define MCIERR_DEVICE_ORD_LENGTH         311
#define MCIERR_NO_INTEGER                312
#define MCIERR_WAVE_OUTPUTSINUSE         320
#define MCIERR_WAVE_SETOUTPUTINUSE       321
#define MCIERR_WAVE_INPUTSINUSE          322
#define MCIERR_WAVE_SETINPUTINUSE        323
#define MCIERR_WAVE_OUTPUTUNSPECIFIED    324
#define MCIERR_WAVE_INPUTUNSPECIFIED     325
#define MCIERR_WAVE_OUTPUTSUNSUITABLE    326
#define MCIERR_WAVE_SETOUTPUTUNSUITABLE  327
#define MCIERR_WAVE_INPUTSUNSUITABLE     328
#define MCIERR_WAVE_SETINPUTUNSUITABLE   329
#define MCIERR_SEQ_DIV_INCOMPATIBLE      336
#define MCIERR_SEQ_PORT_INUSE            337
#define MCIERR_SEQ_PORT_NONEXISTENT      338
#define MCIERR_SEQ_PORT_MAPNODEVICE      339
#define MCIERR_SEQ_PORT_MISCERROR        340
#define MCIERR_SEQ_TIMER                 341
#define MCIERR_SEQ_PORTUNSPECIFIED       342
#define MCIERR_SEQ_NOMIDIPRESENT         343
#define MCIERR_NO_WINDOW                 346
#define MCIERR_CREATEWINDOW              347
#define MCIERR_FILE_READ                 348
#define MCIERR_FILE_WRITE                349
#define MCIERR_NO_IDENTITY               350
#define MCIERR_CUSTOM_DRIVER_BASE        512
};

typedef UINT    MCIDEVICEID;    /* MCI device ID type */
typedef HANDLE      HTASK;

value UINT MCI_COMMAND_MESSAGE_VALUE
{
#define MCI_OPEN                        0x0803
#define MCI_CLOSE                       0x0804
#define MCI_ESCAPE                      0x0805
#define MCI_PLAY                        0x0806
#define MCI_SEEK                        0x0807
#define MCI_STOP                        0x0808
#define MCI_PAUSE                       0x0809
#define MCI_INFO                        0x080A
#define MCI_GETDEVCAPS                  0x080B
#define MCI_SPIN                        0x080C
#define MCI_SET                         0x080D
#define MCI_STEP                        0x080E
#define MCI_RECORD                      0x080F
#define MCI_SYSINFO                     0x0810
#define MCI_BREAK                       0x0811
#define MCI_SAVE                        0x0813
#define MCI_STATUS                      0x0814
#define MCI_CUE                         0x0830
#define MCI_REALIZE                     0x0840
#define MCI_WINDOW                      0x0841
#define MCI_PUT                         0x0842
#define MCI_WHERE                       0x0843
#define MCI_FREEZE                      0x0844
#define MCI_UNFREEZE                    0x0845
#define MCI_LOAD                        0x0850
#define MCI_CUT                         0x0851
#define MCI_COPY                        0x0852
#define MCI_PASTE                       0x0853
#define MCI_UPDATE                      0x0854
#define MCI_RESUME                      0x0855
#define MCI_DELETE                      0x0856
};

mask DWORD MCI_SEND_COMMAND_MASK
{
#define MCI_NOTIFY                      0x00000001L
#define MCI_WAIT                        0x00000002L
#define MCI_FROM                        0x00000004L
#define MCI_TO                          0x00000008L
#define MCI_TRACK                       0x00000010L
#define MCI_COMMAND1                    0x00000020L
#define MCI_COMMAND2                    0x00000040L
#define MCI_COMMAND3                    0x00000080L
#define MCI_COMMAND4                    0x00000100L
#define MCI_COMMAND5                    0x00000200L
#define MCI_COMMAND6                    0x00000400L
#define MCI_COMMAND7                    0x00000800L
#define MCI_COMMAND8                    0x00001000L
#define MCI_COMMAND9                    0x00002000L
#define MCI_COMMAND10                   0x00004000L
#define MCI_COMMAND11                   0x00008000L
};


MCIERROR                     mciSendCommandA(MCIDEVICEID mciId, MCI_COMMAND_MESSAGE_VALUE uMsg, MCI_SEND_COMMAND_MASK dwParam1, DWORD dwParam2);
MCIERROR                     mciSendCommandW(MCIDEVICEID mciId, MCI_COMMAND_MESSAGE_VALUE uMsg, MCI_SEND_COMMAND_MASK dwParam1, DWORD dwParam2);
MCIERROR                     mciSendStringA(LPCSTR lpstrCommand, [out] LPSTR lpstrReturnString, UINT uReturnLength, HWND hwndCallback);
MCIERROR                     mciSendStringW(LPCWSTR lpstrCommand, [out] LPWSTR lpstrReturnString, UINT uReturnLength, HWND hwndCallback);
MCIDEVICEID                  mciGetDeviceIDA(LPCSTR pszDevice);
MCIDEVICEID                  mciGetDeviceIDW(LPCWSTR pszDevice);
MCIDEVICEID                  mciGetDeviceIDFromElementIDA(DWORD dwElementID, LPCSTR lpstrType );
MCIDEVICEID                  mciGetDeviceIDFromElementIDW(DWORD dwElementID, LPCWSTR lpstrType );
BOOL                         mciGetErrorStringA(MCIERROR mcierr, LPSTR pszText, UINT cchText);
BOOL                         mciGetErrorStringW(MCIERROR mcierr, LPWSTR pszText, UINT cchText);

BOOL                         mciSetYieldProc(MCIDEVICEID mciId, DWORD fpYieldProc, DWORD dwYieldData);
HTASK                        mciGetCreatorTask(MCIDEVICEID mciId);
DWORD                        mciGetYieldProc(MCIDEVICEID mciId, LPDWORD pdwYieldData);
BOOL                         mciExecute(LPCSTR pszCommand);


/****************************************************************************

			Multimedia File I/O support

****************************************************************************/
typedef DWORD           FOURCC;         /* a four character code */
typedef char            HPSTR;          /* a huge version of LPSTR */
typedef DWORD           LPMMIOPROC;
typedef HANDLE          HMMIO;

typedef struct _MMIOINFO
{
	/* general fields */
	DWORD           dwFlags;        /* general status flags */
	FOURCC          fccIOProc;      /* pointer to I/O procedure */
	LPMMIOPROC      pIOProc;        /* pointer to I/O procedure */
	UINT            wErrorRet;      /* place for error to be returned */
	HTASK           htask;          /* alternate local task */

	/* fields maintained by MMIO functions during buffered I/O */
	LONG            cchBuffer;      /* size of I/O buffer (or 0L) */
	HPSTR           pchBuffer;      /* start of I/O buffer (or NULL) */
	HPSTR           pchNext;        /* pointer to next byte to read/write */
	HPSTR           pchEndRead;     /* pointer to last valid byte to read */
	HPSTR           pchEndWrite;    /* pointer to last byte to write */
	LONG            lBufOffset;     /* disk offset of start of buffer */

	/* fields maintained by I/O procedure */
	LONG            lDiskOffset;    /* disk offset of next read or write */
	DWORD           adwInfo[3];     /* data specific to type of MMIOPROC */

	/* other fields maintained by MMIO */
	DWORD           dwReserved1;    /* reserved for MMIO use */
	DWORD           dwReserved2;    /* reserved for MMIO use */
	HMMIO           hmmio;          /* handle to open file */
} MMIOINFO;

typedef MMIOINFO *PMMIOINFO;
typedef MMIOINFO *NPMMIOINFO;
typedef MMIOINFO *LPMMIOINFO;
typedef MMIOINFO *LPCMMIOINFO;

/* RIFF chunk information data structure */
typedef struct _MMCKINFO
{
	FOURCC          ckid;           /* chunk ID */
	DWORD           cksize;         /* chunk size */
	FOURCC          fccType;        /* form type or list type */
	DWORD           dwDataOffset;   /* offset of data portion of chunk */
	DWORD           dwFlags;        /* flags used by MMIO functions */
} MMCKINFO;
typedef MMCKINFO *PMMCKINFO;
typedef MMCKINFO *NPMMCKINFO;
typedef MMCKINFO *LPMMCKINFO;
typedef MMCKINFO *LPCMMCKINFO;

mask DWORD MMIOINFO_MASK
{
#define MMIO_EXCLUSIVE  0x00000010      /* exclusive-access mode */
#define MMIO_DENYWRITE  0x00000020      /* deny writing to other processes */
#define MMIO_DENYREAD   0x00000030      /* deny reading to other processes */
#define MMIO_DENYNONE   0x00000040      /* deny nothing to other processes */
#define MMIO_CREATE     0x00001000      /* create new file (or truncate file) */
#define MMIO_PARSE      0x00000100      /* parse new file returning path */
#define MMIO_DELETE     0x00000200      /* create new file (or truncate file) */
#define MMIO_EXIST      0x00004000      /* checks for existence of file */
#define MMIO_ALLOCBUF   0x00010000      /* mmioOpen() should allocate a buffer */
#define MMIO_GETTEMP    0x00020000      /* mmioOpen() should retrieve temp name */
};

value DWORD MMIO_SEEK_VALUE
{
#define SEEK_SET        0               /* seek to an absolute position */
#define SEEK_CUR        1               /* seek relative to current position */
#define SEEK_END        2               /* seek relative to end of file */
};

value DWORD MMIO_RW_VALUE
{
#define MMIO_READ       0x00000000      /* open file for reading only */
#define MMIO_WRITE      0x00000001      /* open file for writing only */
#define MMIO_READWRITE  0x00000002      /* open file for reading and writing */
};

FOURCC          mmioStringToFOURCCA(LPCSTR sz, UINT uFlags);
FOURCC          mmioStringToFOURCCW(LPCWSTR sz, UINT uFlags);
LPMMIOPROC      mmioInstallIOProcA(FOURCC fccIOProc, LPMMIOPROC pIOProc, DWORD dwFlags);
LPMMIOPROC      mmioInstallIOProcW(FOURCC fccIOProc, LPMMIOPROC pIOProc, DWORD dwFlags);
HMMIO           mmioOpenA(LPSTR pszFileName, LPMMIOINFO pmmioinfo, MMIOINFO_MASK fdwOpen);
HMMIO           mmioOpenW(LPWSTR pszFileName, LPMMIOINFO pmmioinfo, MMIOINFO_MASK fdwOpen);
MMRESULT        mmioRenameA(LPCSTR pszFileName, LPCSTR pszNewFileName, LPCMMIOINFO pmmioinfo, DWORD fdwRename);
MMRESULT        mmioRenameW(LPCWSTR pszFileName, LPCWSTR pszNewFileName, LPCMMIOINFO pmmioinfo, DWORD fdwRename);
MMRESULT        mmioClose(HMMIO hmmio, UINT fuClose);
LONG            mmioRead(HMMIO hmmio, HPSTR pch, LONG cch);
LONG            mmioWrite(HMMIO hmmio, char * pch, LONG cch);
LONG            mmioSeek(HMMIO hmmio, LONG lOffset, MMIO_SEEK_VALUE iOrigin);
MMRESULT        mmioGetInfo(HMMIO hmmio, [out] LPMMIOINFO pmmioinfo, UINT fuInfo);
MMRESULT        mmioSetInfo(HMMIO hmmio, LPCMMIOINFO pmmioinfo, UINT fuInfo);
MMRESULT        mmioSetBuffer(HMMIO hmmio, LPSTR pchBuffer, LONG cchBuffer, UINT fuBuffer);
MMRESULT        mmioFlush(HMMIO hmmio, UINT fuFlush);
MMRESULT        mmioAdvance(HMMIO hmmio, LPMMIOINFO pmmioinfo, MMIO_RW_VALUE fuAdvance);
LRESULT         mmioSendMessage(HMMIO hmmio, UINT uMsg, LPARAM lParam1, LPARAM lParam2);
MMRESULT        mmioDescend(HMMIO hmmio, LPMMCKINFO pmmcki, MMCKINFO * pmmckiParent, UINT fuDescend);
MMRESULT        mmioAscend(HMMIO hmmio, LPMMCKINFO pmmcki, UINT fuAscend);
MMRESULT        mmioCreateChunk(HMMIO hmmio, LPMMCKINFO pmmcki, UINT fuCreate);
