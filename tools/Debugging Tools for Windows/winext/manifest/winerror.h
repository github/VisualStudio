value LONG WinError
{
#define ERROR_SUCCESS                    0L
#define ERROR_INVALID_FUNCTION           1L           [fail]
#define ERROR_FILE_NOT_FOUND             2L           [fail]
#define ERROR_PATH_NOT_FOUND             3L           [fail]
#define ERROR_TOO_MANY_OPEN_FILES        4L           [fail]
#define ERROR_ACCESS_DENIED              5L           [fail]
#define ERROR_INVALID_HANDLE             6L           [fail]
#define ERROR_ARENA_TRASHED              7L           [fail]
#define ERROR_NOT_ENOUGH_MEMORY          8L           [fail]
#define ERROR_INVALID_BLOCK              9L           [fail]
#define ERROR_BAD_ENVIRONMENT            10L          [fail]
#define ERROR_BAD_FORMAT                 11L          [fail]
#define ERROR_INVALID_ACCESS             12L          [fail]
#define ERROR_INVALID_DATA               13L          [fail]
#define ERROR_OUTOFMEMORY                14L          [fail]
#define ERROR_INVALID_DRIVE              15L          [fail]
#define ERROR_CURRENT_DIRECTORY          16L          [fail]
#define ERROR_NOT_SAME_DEVICE            17L          [fail]
#define ERROR_NO_MORE_FILES              18L          [fail]
#define ERROR_WRITE_PROTECT              19L          [fail]
#define ERROR_BAD_UNIT                   20L          [fail]
#define ERROR_NOT_READY                  21L          [fail]
#define ERROR_BAD_COMMAND                22L          [fail]
#define ERROR_CRC                        23L          [fail]
#define ERROR_BAD_LENGTH                 24L          [fail]
#define ERROR_SEEK                       25L          [fail]
#define ERROR_NOT_DOS_DISK               26L          [fail]
#define ERROR_SECTOR_NOT_FOUND           27L          [fail]
#define ERROR_OUT_OF_PAPER               28L          [fail]
#define ERROR_WRITE_FAULT                29L          [fail]
#define ERROR_READ_FAULT                 30L          [fail]
#define ERROR_GEN_FAILURE                31L          [fail]
#define ERROR_SHARING_VIOLATION          32L          [fail]
#define ERROR_LOCK_VIOLATION             33L          [fail]
#define ERROR_WRONG_DISK                 34L          [fail]
#define ERROR_SHARING_BUFFER_EXCEEDED    36L          [fail]
#define ERROR_HANDLE_EOF                 38L          [fail]
#define ERROR_HANDLE_DISK_FULL           39L          [fail]
#define ERROR_NOT_SUPPORTED              50L          [fail]
#define ERROR_REM_NOT_LIST               51L          [fail]
#define ERROR_DUP_NAME                   52L          [fail]
#define ERROR_BAD_NETPATH                53L          [fail]
#define ERROR_NETWORK_BUSY               54L          [fail]
#define ERROR_DEV_NOT_EXIST              55L          [fail]
#define ERROR_TOO_MANY_CMDS              56L          [fail]
#define ERROR_ADAP_HDW_ERR               57L          [fail]
#define ERROR_BAD_NET_RESP               58L          [fail]
#define ERROR_UNEXP_NET_ERR              59L          [fail]
#define ERROR_BAD_REM_ADAP               60L          [fail]
#define ERROR_PRINTQ_FULL                61L          [fail]
#define ERROR_NO_SPOOL_SPACE             62L          [fail]
#define ERROR_PRINT_CANCELLED            63L          [fail]
#define ERROR_NETNAME_DELETED            64L          [fail]
#define ERROR_NETWORK_ACCESS_DENIED      65L          [fail]
#define ERROR_BAD_DEV_TYPE               66L          [fail]
#define ERROR_BAD_NET_NAME               67L          [fail]
#define ERROR_TOO_MANY_NAMES             68L          [fail]
#define ERROR_TOO_MANY_SESS              69L          [fail]
#define ERROR_SHARING_PAUSED             70L          [fail]
#define ERROR_REQ_NOT_ACCEP              71L          [fail]
#define ERROR_REDIR_PAUSED               72L          [fail]
#define ERROR_FILE_EXISTS                80L          [fail]
#define ERROR_CANNOT_MAKE                82L          [fail]
#define ERROR_FAIL_I24                   83L          [fail]
#define ERROR_OUT_OF_STRUCTURES          84L          [fail]
#define ERROR_ALREADY_ASSIGNED           85L          [fail]
#define ERROR_INVALID_PASSWORD           86L          [fail]
#define ERROR_INVALID_PARAMETER          87L          [fail]
#define ERROR_NET_WRITE_FAULT            88L          [fail]
#define ERROR_NO_PROC_SLOTS              89L          [fail]
#define ERROR_TOO_MANY_SEMAPHORES        100L         [fail]
#define ERROR_EXCL_SEM_ALREADY_OWNED     101L         [fail]
#define ERROR_SEM_IS_SET                 102L         [fail]
#define ERROR_TOO_MANY_SEM_REQUESTS      103L         [fail]
#define ERROR_INVALID_AT_INTERRUPT_TIME  104L         [fail]
#define ERROR_SEM_OWNER_DIED             105L         [fail]
#define ERROR_SEM_USER_LIMIT             106L         [fail]
#define ERROR_DISK_CHANGE                107L         [fail]
#define ERROR_DRIVE_LOCKED               108L         [fail]
#define ERROR_BROKEN_PIPE                109L         [fail]
#define ERROR_OPEN_FAILED                110L         [fail]
#define ERROR_BUFFER_OVERFLOW            111L         [fail]
#define ERROR_DISK_FULL                  112L         [fail]
#define ERROR_NO_MORE_SEARCH_HANDLES     113L         [fail]
#define ERROR_INVALID_TARGET_HANDLE      114L         [fail]
#define ERROR_INVALID_CATEGORY           117L         [fail]
#define ERROR_INVALID_VERIFY_SWITCH      118L         [fail]
#define ERROR_BAD_DRIVER_LEVEL           119L         [fail]
#define ERROR_CALL_NOT_IMPLEMENTED       120L         [fail]
#define ERROR_SEM_TIMEOUT                121L         [fail]
#define ERROR_INSUFFICIENT_BUFFER        122L         [fail]
#define ERROR_INVALID_NAME               123L         [fail]
#define ERROR_INVALID_LEVEL              124L         [fail]
#define ERROR_NO_VOLUME_LABEL            125L         [fail]
#define ERROR_MOD_NOT_FOUND              126L         [fail]
#define ERROR_PROC_NOT_FOUND             127L         [fail]
#define ERROR_WAIT_NO_CHILDREN           128L         [fail]
#define ERROR_CHILD_NOT_COMPLETE         129L         [fail]
#define ERROR_DIRECT_ACCESS_HANDLE       130L         [fail]
#define ERROR_NEGATIVE_SEEK              131L         [fail]
#define ERROR_SEEK_ON_DEVICE             132L         [fail]
#define ERROR_IS_JOIN_TARGET             133L         [fail]
#define ERROR_IS_JOINED                  134L         [fail]
#define ERROR_IS_SUBSTED                 135L         [fail]
#define ERROR_NOT_JOINED                 136L         [fail]
#define ERROR_NOT_SUBSTED                137L         [fail]
#define ERROR_JOIN_TO_JOIN               138L         [fail]
#define ERROR_SUBST_TO_SUBST             139L         [fail]
#define ERROR_JOIN_TO_SUBST              140L         [fail]
#define ERROR_SUBST_TO_JOIN              141L         [fail]
#define ERROR_BUSY_DRIVE                 142L         [fail]
#define ERROR_SAME_DRIVE                 143L         [fail]
#define ERROR_DIR_NOT_ROOT               144L         [fail]
#define ERROR_DIR_NOT_EMPTY              145L         [fail]
#define ERROR_IS_SUBST_PATH              146L         [fail]
#define ERROR_IS_JOIN_PATH               147L         [fail]
#define ERROR_PATH_BUSY                  148L         [fail]
#define ERROR_IS_SUBST_TARGET            149L         [fail]
#define ERROR_SYSTEM_TRACE               150L         [fail]
#define ERROR_INVALID_EVENT_COUNT        151L         [fail]
#define ERROR_TOO_MANY_MUXWAITERS        152L         [fail]
#define ERROR_INVALID_LIST_FORMAT        153L         [fail]
#define ERROR_LABEL_TOO_LONG             154L         [fail]
#define ERROR_TOO_MANY_TCBS              155L         [fail]
#define ERROR_SIGNAL_REFUSED             156L         [fail]
#define ERROR_DISCARDED                  157L         [fail]
#define ERROR_NOT_LOCKED                 158L         [fail]
#define ERROR_BAD_THREADID_ADDR          159L         [fail]
#define ERROR_BAD_ARGUMENTS              160L         [fail]
#define ERROR_BAD_PATHNAME               161L         [fail]
#define ERROR_SIGNAL_PENDING             162L         [fail]
#define ERROR_MAX_THRDS_REACHED          164L         [fail]
#define ERROR_LOCK_FAILED                167L         [fail]
#define ERROR_BUSY                       170L         [fail]
#define ERROR_CANCEL_VIOLATION           173L         [fail]
#define ERROR_ATOMIC_LOCKS_NOT_SUPPORTED 174L         [fail]
#define ERROR_INVALID_SEGMENT_NUMBER     180L         [fail]
#define ERROR_INVALID_ORDINAL            182L         [fail]
#define ERROR_ALREADY_EXISTS             183L         [fail]
#define ERROR_INVALID_FLAG_NUMBER        186L         [fail]
#define ERROR_SEM_NOT_FOUND              187L         [fail]
#define ERROR_INVALID_STARTING_CODESEG   188L         [fail]
#define ERROR_INVALID_STACKSEG           189L         [fail]
#define ERROR_INVALID_MODULETYPE         190L         [fail]
#define ERROR_INVALID_EXE_SIGNATURE      191L         [fail]
#define ERROR_EXE_MARKED_INVALID         192L         [fail]
#define ERROR_BAD_EXE_FORMAT             193L         [fail]
#define ERROR_ITERATED_DATA_EXCEEDS_64k  194L         [fail]
#define ERROR_INVALID_MINALLOCSIZE       195L         [fail]
#define ERROR_DYNLINK_FROM_INVALID_RING  196L         [fail]
#define ERROR_IOPL_NOT_ENABLED           197L         [fail]
#define ERROR_INVALID_SEGDPL             198L         [fail]
#define ERROR_AUTODATASEG_EXCEEDS_64k    199L         [fail]
#define ERROR_RING2SEG_MUST_BE_MOVABLE   200L         [fail]
#define ERROR_RELOC_CHAIN_XEEDS_SEGLIM   201L         [fail]
#define ERROR_INFLOOP_IN_RELOC_CHAIN     202L         [fail]
#define ERROR_ENVVAR_NOT_FOUND           203L         [fail]
#define ERROR_NO_SIGNAL_SENT             205L         [fail]
#define ERROR_FILENAME_EXCED_RANGE       206L         [fail]
#define ERROR_RING2_STACK_IN_USE         207L         [fail]
#define ERROR_META_EXPANSION_TOO_LONG    208L         [fail]
#define ERROR_INVALID_SIGNAL_NUMBER      209L         [fail]
#define ERROR_THREAD_1_INACTIVE          210L         [fail]
#define ERROR_LOCKED                     212L         [fail]
#define ERROR_TOO_MANY_MODULES           214L         [fail]
#define ERROR_NESTING_NOT_ALLOWED        215L         [fail]
#define ERROR_EXE_MACHINE_TYPE_MISMATCH  216L         [fail]
#define ERROR_BAD_PIPE                   230L         [fail]
#define ERROR_PIPE_BUSY                  231L         [fail]
#define ERROR_NO_DATA                    232L         [fail]
#define ERROR_PIPE_NOT_CONNECTED         233L         [fail]
#define ERROR_MORE_DATA                  234L         [fail]
#define ERROR_VC_DISCONNECTED            240L         [fail]
#define ERROR_INVALID_EA_NAME            254L         [fail]
#define ERROR_EA_LIST_INCONSISTENT       255L         [fail]
#define ERROR_NO_MORE_ITEMS              259L         [fail]
#define ERROR_CANNOT_COPY                266L         [fail]
#define ERROR_DIRECTORY                  267L         [fail]
#define ERROR_EAS_DIDNT_FIT              275L         [fail]
#define ERROR_EA_FILE_CORRUPT            276L         [fail]
#define ERROR_EA_TABLE_FULL              277L         [fail]
#define ERROR_INVALID_EA_HANDLE          278L         [fail]
#define ERROR_EAS_NOT_SUPPORTED          282L         [fail]
#define ERROR_NOT_OWNER                  288L         [fail]
#define ERROR_TOO_MANY_POSTS             298L         [fail]
#define ERROR_PARTIAL_COPY               299L         [fail]
#define ERROR_OPLOCK_NOT_GRANTED         300L         [fail]
#define ERROR_INVALID_OPLOCK_PROTOCOL    301L         [fail]
#define ERROR_MR_MID_NOT_FOUND           317L         [fail]
#define ERROR_INVALID_ADDRESS            487L         [fail]
#define ERROR_ARITHMETIC_OVERFLOW        534L         [fail]
#define ERROR_PIPE_CONNECTED             535L         [fail]
#define ERROR_PIPE_LISTENING             536L         [fail]
#define ERROR_EA_ACCESS_DENIED           994L         [fail]
#define ERROR_OPERATION_ABORTED          995L         [fail]
#define ERROR_IO_INCOMPLETE              996L         [fail]
#define ERROR_IO_PENDING                 997L         [fail]
#define ERROR_NOACCESS                   998L         [fail]
#define ERROR_SWAPERROR                  999L         [fail]
#define ERROR_STACK_OVERFLOW             1001L        [fail]
#define ERROR_INVALID_MESSAGE            1002L        [fail]
#define ERROR_CAN_NOT_COMPLETE           1003L        [fail]
#define ERROR_INVALID_FLAGS              1004L        [fail]
#define ERROR_UNRECOGNIZED_VOLUME        1005L        [fail]
#define ERROR_FILE_INVALID               1006L        [fail]
#define ERROR_FULLSCREEN_MODE            1007L        [fail]
#define ERROR_NO_TOKEN                   1008L        [fail]
#define ERROR_BADDB                      1009L        [fail]
#define ERROR_BADKEY                     1010L        [fail]
#define ERROR_CANTOPEN                   1011L        [fail]
#define ERROR_CANTREAD                   1012L        [fail]
#define ERROR_CANTWRITE                  1013L        [fail]
#define ERROR_REGISTRY_RECOVERED         1014L        [fail]
#define ERROR_REGISTRY_CORRUPT           1015L        [fail]
#define ERROR_REGISTRY_IO_FAILED         1016L        [fail]
#define ERROR_NOT_REGISTRY_FILE          1017L        [fail]
#define ERROR_KEY_DELETED                1018L        [fail]
#define ERROR_NO_LOG_SPACE               1019L        [fail]
#define ERROR_KEY_HAS_CHILDREN           1020L        [fail]
#define ERROR_CHILD_MUST_BE_VOLATILE     1021L        [fail]
#define ERROR_NOTIFY_ENUM_DIR            1022L        [fail]
#define ERROR_DEPENDENT_SERVICES_RUNNING 1051L        [fail]
#define ERROR_INVALID_SERVICE_CONTROL    1052L        [fail]
#define ERROR_SERVICE_REQUEST_TIMEOUT    1053L        [fail]
#define ERROR_SERVICE_NO_THREAD          1054L        [fail]
#define ERROR_SERVICE_DATABASE_LOCKED    1055L        [fail]
#define ERROR_SERVICE_ALREADY_RUNNING    1056L        [fail]
#define ERROR_INVALID_SERVICE_ACCOUNT    1057L        [fail]
#define ERROR_SERVICE_DISABLED           1058L        [fail]
#define ERROR_CIRCULAR_DEPENDENCY        1059L        [fail]
#define ERROR_SERVICE_DOES_NOT_EXIST     1060L        [fail]
#define ERROR_SERVICE_CANNOT_ACCEPT_CTRL 1061L        [fail]
#define ERROR_SERVICE_NOT_ACTIVE         1062L        [fail]
#define ERROR_FAILED_SERVICE_CONTROLLER_CONNECT 1063L [fail]
#define ERROR_EXCEPTION_IN_SERVICE       1064L        [fail]
#define ERROR_DATABASE_DOES_NOT_EXIST    1065L        [fail]
#define ERROR_SERVICE_SPECIFIC_ERROR     1066L        [fail]
#define ERROR_PROCESS_ABORTED            1067L        [fail]
#define ERROR_SERVICE_DEPENDENCY_FAIL    1068L        [fail]
#define ERROR_SERVICE_LOGON_FAILED       1069L        [fail]
#define ERROR_SERVICE_START_HANG         1070L        [fail]
#define ERROR_INVALID_SERVICE_LOCK       1071L        [fail]
#define ERROR_SERVICE_MARKED_FOR_DELETE  1072L        [fail]
#define ERROR_SERVICE_EXISTS             1073L        [fail]
#define ERROR_ALREADY_RUNNING_LKG        1074L        [fail]
#define ERROR_SERVICE_DEPENDENCY_DELETED 1075L        [fail]
#define ERROR_BOOT_ALREADY_ACCEPTED      1076L        [fail]
#define ERROR_SERVICE_NEVER_STARTED      1077L        [fail]
#define ERROR_DUPLICATE_SERVICE_NAME     1078L        [fail]
#define ERROR_DIFFERENT_SERVICE_ACCOUNT  1079L        [fail]
#define ERROR_CANNOT_DETECT_DRIVER_FAILURE 1080L      [fail]
#define ERROR_CANNOT_DETECT_PROCESS_ABORT 1081L       [fail]
#define ERROR_NO_RECOVERY_PROGRAM        1082L        [fail]
#define ERROR_END_OF_MEDIA               1100L        [fail]
#define ERROR_FILEMARK_DETECTED          1101L        [fail]
#define ERROR_BEGINNING_OF_MEDIA         1102L        [fail]
#define ERROR_SETMARK_DETECTED           1103L        [fail]
#define ERROR_NO_DATA_DETECTED           1104L        [fail]
#define ERROR_PARTITION_FAILURE          1105L        [fail]
#define ERROR_INVALID_BLOCK_LENGTH       1106L        [fail]
#define ERROR_DEVICE_NOT_PARTITIONED     1107L        [fail]
#define ERROR_UNABLE_TO_LOCK_MEDIA       1108L        [fail]
#define ERROR_UNABLE_TO_UNLOAD_MEDIA     1109L        [fail]
#define ERROR_MEDIA_CHANGED              1110L        [fail]
#define ERROR_BUS_RESET                  1111L        [fail]
#define ERROR_NO_MEDIA_IN_DRIVE          1112L        [fail]
#define ERROR_NO_UNICODE_TRANSLATION     1113L        [fail]
#define ERROR_DLL_INIT_FAILED            1114L        [fail]
#define ERROR_SHUTDOWN_IN_PROGRESS       1115L        [fail]
#define ERROR_NO_SHUTDOWN_IN_PROGRESS    1116L        [fail]
#define ERROR_IO_DEVICE                  1117L        [fail]
#define ERROR_SERIAL_NO_DEVICE           1118L        [fail]
#define ERROR_IRQ_BUSY                   1119L        [fail]
#define ERROR_MORE_WRITES                1120L        [fail]
#define ERROR_COUNTER_TIMEOUT            1121L        [fail]
#define ERROR_FLOPPY_ID_MARK_NOT_FOUND   1122L        [fail]
#define ERROR_FLOPPY_WRONG_CYLINDER      1123L        [fail]
#define ERROR_FLOPPY_UNKNOWN_ERROR       1124L        [fail]
#define ERROR_FLOPPY_BAD_REGISTERS       1125L        [fail]
#define ERROR_DISK_RECALIBRATE_FAILED    1126L        [fail]
#define ERROR_DISK_OPERATION_FAILED      1127L        [fail]
#define ERROR_DISK_RESET_FAILED          1128L        [fail]
#define ERROR_EOM_OVERFLOW               1129L        [fail]
#define ERROR_NOT_ENOUGH_SERVER_MEMORY   1130L        [fail]
#define ERROR_POSSIBLE_DEADLOCK          1131L        [fail]
#define ERROR_MAPPED_ALIGNMENT           1132L        [fail]
#define ERROR_SET_POWER_STATE_VETOED     1140L        [fail]
#define ERROR_SET_POWER_STATE_FAILED     1141L        [fail]
#define ERROR_TOO_MANY_LINKS             1142L        [fail]
#define ERROR_OLD_WIN_VERSION            1150L        [fail]
#define ERROR_APP_WRONG_OS               1151L        [fail]
#define ERROR_SINGLE_INSTANCE_APP        1152L        [fail]
#define ERROR_RMODE_APP                  1153L        [fail]
#define ERROR_INVALID_DLL                1154L        [fail]
#define ERROR_NO_ASSOCIATION             1155L        [fail]
#define ERROR_DDE_FAIL                   1156L        [fail]
#define ERROR_DLL_NOT_FOUND              1157L        [fail]
#define ERROR_NO_MORE_USER_HANDLES       1158L        [fail]
#define ERROR_MESSAGE_SYNC_ONLY          1159L        [fail]
#define ERROR_SOURCE_ELEMENT_EMPTY       1160L        [fail]
#define ERROR_DESTINATION_ELEMENT_FULL   1161L        [fail]
#define ERROR_ILLEGAL_ELEMENT_ADDRESS    1162L        [fail]
#define ERROR_MAGAZINE_NOT_PRESENT       1163L        [fail]
#define ERROR_DEVICE_REINITIALIZATION_NEEDED 1164L    [fail]
#define ERROR_DEVICE_REQUIRES_CLEANING   1165L        [fail]
#define ERROR_DEVICE_DOOR_OPEN           1166L        [fail]
#define ERROR_DEVICE_NOT_CONNECTED       1167L        [fail]
#define ERROR_NOT_FOUND                  1168L        [fail]
#define ERROR_NO_MATCH                   1169L        [fail]
#define ERROR_SET_NOT_FOUND              1170L        [fail]
#define ERROR_POINT_NOT_FOUND            1171L        [fail]
#define ERROR_NO_TRACKING_SERVICE        1172L        [fail]
#define ERROR_NO_VOLUME_ID               1173L        [fail]
#define ERROR_CONNECTED_OTHER_PASSWORD   2108L        [fail]
#define ERROR_BAD_USERNAME               2202L        [fail]
#define ERROR_NOT_CONNECTED              2250L        [fail]
#define ERROR_OPEN_FILES                 2401L        [fail]
#define ERROR_ACTIVE_CONNECTIONS         2402L        [fail]
#define ERROR_DEVICE_IN_USE              2404L        [fail]
#define ERROR_BAD_DEVICE                 1200L        [fail]
#define ERROR_CONNECTION_UNAVAIL         1201L        [fail]
#define ERROR_DEVICE_ALREADY_REMEMBERED  1202L        [fail]
#define ERROR_NO_NET_OR_BAD_PATH         1203L        [fail]
#define ERROR_BAD_PROVIDER               1204L        [fail]
#define ERROR_CANNOT_OPEN_PROFILE        1205L        [fail]
#define ERROR_BAD_PROFILE                1206L        [fail]
#define ERROR_NOT_CONTAINER              1207L        [fail]
#define ERROR_EXTENDED_ERROR             1208L        [fail]
#define ERROR_INVALID_GROUPNAME          1209L        [fail]
#define ERROR_INVALID_COMPUTERNAME       1210L        [fail]
#define ERROR_INVALID_EVENTNAME          1211L        [fail]
#define ERROR_INVALID_DOMAINNAME         1212L        [fail]
#define ERROR_INVALID_SERVICENAME        1213L        [fail]
#define ERROR_INVALID_NETNAME            1214L        [fail]
#define ERROR_INVALID_SHARENAME          1215L        [fail]
#define ERROR_INVALID_PASSWORDNAME       1216L        [fail]
#define ERROR_INVALID_MESSAGENAME        1217L        [fail]
#define ERROR_INVALID_MESSAGEDEST        1218L        [fail]
#define ERROR_SESSION_CREDENTIAL_CONFLICT 1219L       [fail]
#define ERROR_REMOTE_SESSION_LIMIT_EXCEEDED 1220L     [fail]
#define ERROR_DUP_DOMAINNAME             1221L        [fail]
#define ERROR_NO_NETWORK                 1222L        [fail]
#define ERROR_CANCELLED                  1223L        [fail]
#define ERROR_USER_MAPPED_FILE           1224L        [fail]
#define ERROR_CONNECTION_REFUSED         1225L        [fail]
#define ERROR_GRACEFUL_DISCONNECT        1226L        [fail]
#define ERROR_ADDRESS_ALREADY_ASSOCIATED 1227L        [fail]
#define ERROR_ADDRESS_NOT_ASSOCIATED     1228L        [fail]
#define ERROR_CONNECTION_INVALID         1229L        [fail]
#define ERROR_CONNECTION_ACTIVE          1230L        [fail]
#define ERROR_NETWORK_UNREACHABLE        1231L        [fail]
#define ERROR_HOST_UNREACHABLE           1232L        [fail]
#define ERROR_PROTOCOL_UNREACHABLE       1233L        [fail]
#define ERROR_PORT_UNREACHABLE           1234L        [fail]
#define ERROR_REQUEST_ABORTED            1235L        [fail]
#define ERROR_CONNECTION_ABORTED         1236L        [fail]
#define ERROR_RETRY                      1237L        [fail]
#define ERROR_CONNECTION_COUNT_LIMIT     1238L        [fail]
#define ERROR_LOGIN_TIME_RESTRICTION     1239L        [fail]
#define ERROR_LOGIN_WKSTA_RESTRICTION    1240L        [fail]
#define ERROR_INCORRECT_ADDRESS          1241L        [fail]
#define ERROR_ALREADY_REGISTERED         1242L        [fail]
#define ERROR_SERVICE_NOT_FOUND          1243L        [fail]
#define ERROR_NOT_AUTHENTICATED          1244L        [fail]
#define ERROR_NOT_LOGGED_ON              1245L        [fail]
#define ERROR_CONTINUE                   1246L        [fail]
#define ERROR_ALREADY_INITIALIZED        1247L        [fail]
#define ERROR_NO_MORE_DEVICES            1248L        [fail]
#define ERROR_NO_SUCH_SITE               1249L        [fail]
#define ERROR_DOMAIN_CONTROLLER_EXISTS   1250L        [fail]
#define ERROR_DS_NOT_INSTALLED           1251L        [fail]
#define ERROR_NOT_ALL_ASSIGNED           1300L        [fail]
#define ERROR_SOME_NOT_MAPPED            1301L        [fail]
#define ERROR_NO_QUOTAS_FOR_ACCOUNT      1302L        [fail]
#define ERROR_LOCAL_USER_SESSION_KEY     1303L        [fail]
#define ERROR_NULL_LM_PASSWORD           1304L        [fail]
#define ERROR_UNKNOWN_REVISION           1305L        [fail]
#define ERROR_REVISION_MISMATCH          1306L        [fail]
#define ERROR_INVALID_OWNER              1307L        [fail]
#define ERROR_INVALID_PRIMARY_GROUP      1308L        [fail]
#define ERROR_NO_IMPERSONATION_TOKEN     1309L        [fail]
#define ERROR_CANT_DISABLE_MANDATORY     1310L        [fail]
#define ERROR_NO_LOGON_SERVERS           1311L        [fail]
#define ERROR_NO_SUCH_LOGON_SESSION      1312L        [fail]
#define ERROR_NO_SUCH_PRIVILEGE          1313L        [fail]
#define ERROR_PRIVILEGE_NOT_HELD         1314L        [fail]
#define ERROR_INVALID_ACCOUNT_NAME       1315L        [fail]
#define ERROR_USER_EXISTS                1316L        [fail]
#define ERROR_NO_SUCH_USER               1317L        [fail]
#define ERROR_GROUP_EXISTS               1318L        [fail]
#define ERROR_NO_SUCH_GROUP              1319L        [fail]
#define ERROR_MEMBER_IN_GROUP            1320L        [fail]
#define ERROR_MEMBER_NOT_IN_GROUP        1321L        [fail]
#define ERROR_LAST_ADMIN                 1322L        [fail]
#define ERROR_WRONG_PASSWORD             1323L        [fail]
#define ERROR_ILL_FORMED_PASSWORD        1324L        [fail]
#define ERROR_PASSWORD_RESTRICTION       1325L        [fail]
#define ERROR_LOGON_FAILURE              1326L        [fail]
#define ERROR_ACCOUNT_RESTRICTION        1327L        [fail]
#define ERROR_INVALID_LOGON_HOURS        1328L        [fail]
#define ERROR_INVALID_WORKSTATION        1329L        [fail]
#define ERROR_PASSWORD_EXPIRED           1330L        [fail]
#define ERROR_ACCOUNT_DISABLED           1331L        [fail]
#define ERROR_NONE_MAPPED                1332L        [fail]
#define ERROR_TOO_MANY_LUIDS_REQUESTED   1333L        [fail]
#define ERROR_LUIDS_EXHAUSTED            1334L        [fail]
#define ERROR_INVALID_SUB_AUTHORITY      1335L        [fail]
#define ERROR_INVALID_ACL                1336L        [fail]
#define ERROR_INVALID_SID                1337L        [fail]
#define ERROR_INVALID_SECURITY_DESCR     1338L        [fail]
#define ERROR_BAD_INHERITANCE_ACL        1340L        [fail]
#define ERROR_SERVER_DISABLED            1341L        [fail]
#define ERROR_SERVER_NOT_DISABLED        1342L        [fail]
#define ERROR_INVALID_ID_AUTHORITY       1343L        [fail]
#define ERROR_ALLOTTED_SPACE_EXCEEDED    1344L        [fail]
#define ERROR_INVALID_GROUP_ATTRIBUTES   1345L        [fail]
#define ERROR_BAD_IMPERSONATION_LEVEL    1346L        [fail]
#define ERROR_CANT_OPEN_ANONYMOUS        1347L        [fail]
#define ERROR_BAD_VALIDATION_CLASS       1348L        [fail]
#define ERROR_BAD_TOKEN_TYPE             1349L        [fail]
#define ERROR_NO_SECURITY_ON_OBJECT      1350L        [fail]
#define ERROR_CANT_ACCESS_DOMAIN_INFO    1351L        [fail]
#define ERROR_INVALID_SERVER_STATE       1352L        [fail]
#define ERROR_INVALID_DOMAIN_STATE       1353L        [fail]
#define ERROR_INVALID_DOMAIN_ROLE        1354L        [fail]
#define ERROR_NO_SUCH_DOMAIN             1355L        [fail]
#define ERROR_DOMAIN_EXISTS              1356L        [fail]
#define ERROR_DOMAIN_LIMIT_EXCEEDED      1357L        [fail]
#define ERROR_INTERNAL_DB_CORRUPTION     1358L        [fail]
#define ERROR_INTERNAL_ERROR             1359L        [fail]
#define ERROR_GENERIC_NOT_MAPPED         1360L        [fail]
#define ERROR_BAD_DESCRIPTOR_FORMAT      1361L        [fail]
#define ERROR_NOT_LOGON_PROCESS          1362L        [fail]
#define ERROR_LOGON_SESSION_EXISTS       1363L        [fail]
#define ERROR_NO_SUCH_PACKAGE            1364L        [fail]
#define ERROR_BAD_LOGON_SESSION_STATE    1365L        [fail]
#define ERROR_LOGON_SESSION_COLLISION    1366L        [fail]
#define ERROR_INVALID_LOGON_TYPE         1367L        [fail]
#define ERROR_CANNOT_IMPERSONATE         1368L        [fail]
#define ERROR_RXACT_INVALID_STATE        1369L        [fail]
#define ERROR_RXACT_COMMIT_FAILURE       1370L        [fail]
#define ERROR_SPECIAL_ACCOUNT            1371L        [fail]
#define ERROR_SPECIAL_GROUP              1372L        [fail]
#define ERROR_SPECIAL_USER               1373L        [fail]
#define ERROR_MEMBERS_PRIMARY_GROUP      1374L        [fail]
#define ERROR_TOKEN_ALREADY_IN_USE       1375L        [fail]
#define ERROR_NO_SUCH_ALIAS              1376L        [fail]
#define ERROR_MEMBER_NOT_IN_ALIAS        1377L        [fail]
#define ERROR_MEMBER_IN_ALIAS            1378L        [fail]
#define ERROR_ALIAS_EXISTS               1379L        [fail]
#define ERROR_LOGON_NOT_GRANTED          1380L        [fail]
#define ERROR_TOO_MANY_SECRETS           1381L        [fail]
#define ERROR_SECRET_TOO_LONG            1382L        [fail]
#define ERROR_INTERNAL_DB_ERROR          1383L        [fail]
#define ERROR_TOO_MANY_CONTEXT_IDS       1384L        [fail]
#define ERROR_LOGON_TYPE_NOT_GRANTED     1385L        [fail]
#define ERROR_NT_CROSS_ENCRYPTION_REQUIRED 1386L      [fail]
#define ERROR_NO_SUCH_MEMBER             1387L        [fail]
#define ERROR_INVALID_MEMBER             1388L        [fail]
#define ERROR_TOO_MANY_SIDS              1389L        [fail]
#define ERROR_LM_CROSS_ENCRYPTION_REQUIRED 1390L      [fail]
#define ERROR_NO_INHERITANCE             1391L        [fail]
#define ERROR_FILE_CORRUPT               1392L        [fail]
#define ERROR_DISK_CORRUPT               1393L        [fail]
#define ERROR_NO_USER_SESSION_KEY        1394L        [fail]
#define ERROR_LICENSE_QUOTA_EXCEEDED     1395L        [fail]
#define ERROR_INVALID_WINDOW_HANDLE      1400L        [fail]
#define ERROR_INVALID_MENU_HANDLE        1401L        [fail]
#define ERROR_INVALID_CURSOR_HANDLE      1402L        [fail]
#define ERROR_INVALID_ACCEL_HANDLE       1403L        [fail]
#define ERROR_INVALID_HOOK_HANDLE        1404L        [fail]
#define ERROR_INVALID_DWP_HANDLE         1405L        [fail]
#define ERROR_TLW_WITH_WSCHILD           1406L        [fail]
#define ERROR_CANNOT_FIND_WND_CLASS      1407L        [fail]
#define ERROR_WINDOW_OF_OTHER_THREAD     1408L        [fail]
#define ERROR_HOTKEY_ALREADY_REGISTERED  1409L        [fail]
#define ERROR_CLASS_ALREADY_EXISTS       1410L        [fail]
#define ERROR_CLASS_DOES_NOT_EXIST       1411L        [fail]
#define ERROR_CLASS_HAS_WINDOWS          1412L        [fail]
#define ERROR_INVALID_INDEX              1413L        [fail]
#define ERROR_INVALID_ICON_HANDLE        1414L        [fail]
#define ERROR_PRIVATE_DIALOG_INDEX       1415L        [fail]
#define ERROR_LISTBOX_ID_NOT_FOUND       1416L        [fail]
#define ERROR_NO_WILDCARD_CHARACTERS     1417L        [fail]
#define ERROR_CLIPBOARD_NOT_OPEN         1418L        [fail]
#define ERROR_HOTKEY_NOT_REGISTERED      1419L        [fail]
#define ERROR_WINDOW_NOT_DIALOG          1420L        [fail]
#define ERROR_CONTROL_ID_NOT_FOUND       1421L        [fail]
#define ERROR_INVALID_COMBOBOX_MESSAGE   1422L        [fail]
#define ERROR_WINDOW_NOT_COMBOBOX        1423L        [fail]
#define ERROR_INVALID_EDIT_HEIGHT        1424L        [fail]
#define ERROR_DC_NOT_FOUND               1425L        [fail]
#define ERROR_INVALID_HOOK_FILTER        1426L        [fail]
#define ERROR_INVALID_FILTER_PROC        1427L        [fail]
#define ERROR_HOOK_NEEDS_HMOD            1428L        [fail]
#define ERROR_GLOBAL_ONLY_HOOK           1429L        [fail]
#define ERROR_JOURNAL_HOOK_SET           1430L        [fail]
#define ERROR_HOOK_NOT_INSTALLED         1431L        [fail]
#define ERROR_INVALID_LB_MESSAGE         1432L        [fail]
#define ERROR_SETCOUNT_ON_BAD_LB         1433L        [fail]
#define ERROR_LB_WITHOUT_TABSTOPS        1434L        [fail]
#define ERROR_DESTROY_OBJECT_OF_OTHER_THREAD 1435L    [fail]
#define ERROR_CHILD_WINDOW_MENU          1436L        [fail]
#define ERROR_NO_SYSTEM_MENU             1437L        [fail]
#define ERROR_INVALID_MSGBOX_STYLE       1438L        [fail]
#define ERROR_INVALID_SPI_VALUE          1439L        [fail]
#define ERROR_SCREEN_ALREADY_LOCKED      1440L        [fail]
#define ERROR_HWNDS_HAVE_DIFF_PARENT     1441L        [fail]
#define ERROR_NOT_CHILD_WINDOW           1442L        [fail]
#define ERROR_INVALID_GW_COMMAND         1443L        [fail]
#define ERROR_INVALID_THREAD_ID          1444L        [fail]
#define ERROR_NON_MDICHILD_WINDOW        1445L        [fail]
#define ERROR_POPUP_ALREADY_ACTIVE       1446L        [fail]
#define ERROR_NO_SCROLLBARS              1447L        [fail]
#define ERROR_INVALID_SCROLLBAR_RANGE    1448L        [fail]
#define ERROR_INVALID_SHOWWIN_COMMAND    1449L        [fail]
#define ERROR_NO_SYSTEM_RESOURCES        1450L        [fail]
#define ERROR_NONPAGED_SYSTEM_RESOURCES  1451L        [fail]
#define ERROR_PAGED_SYSTEM_RESOURCES     1452L        [fail]
#define ERROR_WORKING_SET_QUOTA          1453L        [fail]
#define ERROR_PAGEFILE_QUOTA             1454L        [fail]
#define ERROR_COMMITMENT_LIMIT           1455L        [fail]
#define ERROR_MENU_ITEM_NOT_FOUND        1456L        [fail]
#define ERROR_INVALID_KEYBOARD_HANDLE    1457L        [fail]
#define ERROR_HOOK_TYPE_NOT_ALLOWED      1458L        [fail]
#define ERROR_REQUIRES_INTERACTIVE_WINDOWSTATION 1459L[fail]
#define ERROR_TIMEOUT                    1460L        [fail]
#define ERROR_INVALID_MONITOR_HANDLE     1461L        [fail]
#define ERROR_EVENTLOG_FILE_CORRUPT      1500L        [fail]
#define ERROR_EVENTLOG_CANT_START        1501L        [fail]
#define ERROR_LOG_FILE_FULL              1502L        [fail]
#define ERROR_EVENTLOG_FILE_CHANGED      1503L        [fail]
#define ERROR_INSTALL_SERVICE            1601L        [fail]
#define ERROR_INSTALL_USEREXIT           1602L        [fail]
#define ERROR_INSTALL_FAILURE            1603L        [fail]
#define ERROR_INSTALL_SUSPEND            1604L        [fail]
#define ERROR_UNKNOWN_PRODUCT            1605L        [fail]
#define ERROR_UNKNOWN_FEATURE            1606L        [fail]
#define ERROR_UNKNOWN_COMPONENT          1607L        [fail]
#define ERROR_UNKNOWN_PROPERTY           1608L        [fail]
#define ERROR_INVALID_HANDLE_STATE       1609L        [fail]
#define ERROR_BAD_CONFIGURATION          1610L        [fail]
#define ERROR_INDEX_ABSENT               1611L        [fail]
#define ERROR_INSTALL_SOURCE_ABSENT      1612L        [fail]
#define ERROR_BAD_DATABASE_VERSION       1613L        [fail]
#define ERROR_PRODUCT_UNINSTALLED        1614L        [fail]
#define ERROR_BAD_QUERY_SYNTAX           1615L        [fail]
#define ERROR_INVALID_FIELD              1616L        [fail]
#define RPC_S_INVALID_STRING_BINDING     1700L        [fail]
#define RPC_S_WRONG_KIND_OF_BINDING      1701L        [fail]
#define RPC_S_INVALID_BINDING            1702L        [fail]
#define RPC_S_PROTSEQ_NOT_SUPPORTED      1703L        [fail]
#define RPC_S_INVALID_RPC_PROTSEQ        1704L        [fail]
#define RPC_S_INVALID_STRING_UUID        1705L        [fail]
#define RPC_S_INVALID_ENDPOINT_FORMAT    1706L        [fail]
#define RPC_S_INVALID_NET_ADDR           1707L        [fail]
#define RPC_S_NO_ENDPOINT_FOUND          1708L        [fail]
#define RPC_S_INVALID_TIMEOUT            1709L        [fail]
#define RPC_S_OBJECT_NOT_FOUND           1710L        [fail]
#define RPC_S_ALREADY_REGISTERED         1711L        [fail]
#define RPC_S_TYPE_ALREADY_REGISTERED    1712L        [fail]
#define RPC_S_ALREADY_LISTENING          1713L        [fail]
#define RPC_S_NO_PROTSEQS_REGISTERED     1714L        [fail]
#define RPC_S_NOT_LISTENING              1715L        [fail]
#define RPC_S_UNKNOWN_MGR_TYPE           1716L        [fail]
#define RPC_S_UNKNOWN_IF                 1717L        [fail]
#define RPC_S_NO_BINDINGS                1718L        [fail]
#define RPC_S_NO_PROTSEQS                1719L        [fail]
#define RPC_S_CANT_CREATE_ENDPOINT       1720L        [fail]
#define RPC_S_OUT_OF_RESOURCES           1721L        [fail]
#define RPC_S_SERVER_UNAVAILABLE         1722L        [fail]
#define RPC_S_SERVER_TOO_BUSY            1723L        [fail]
#define RPC_S_INVALID_NETWORK_OPTIONS    1724L        [fail]
#define RPC_S_NO_CALL_ACTIVE             1725L        [fail]
#define RPC_S_CALL_FAILED                1726L        [fail]
#define RPC_S_CALL_FAILED_DNE            1727L        [fail]
#define RPC_S_PROTOCOL_ERROR             1728L        [fail]
#define RPC_S_UNSUPPORTED_TRANS_SYN      1730L        [fail]
#define RPC_S_UNSUPPORTED_TYPE           1732L        [fail]
#define RPC_S_INVALID_TAG                1733L        [fail]
#define RPC_S_INVALID_BOUND              1734L        [fail]
#define RPC_S_NO_ENTRY_NAME              1735L        [fail]
#define RPC_S_INVALID_NAME_SYNTAX        1736L        [fail]
#define RPC_S_UNSUPPORTED_NAME_SYNTAX    1737L        [fail]
#define RPC_S_UUID_NO_ADDRESS            1739L        [fail]
#define RPC_S_DUPLICATE_ENDPOINT         1740L        [fail]
#define RPC_S_UNKNOWN_AUTHN_TYPE         1741L        [fail]
#define RPC_S_MAX_CALLS_TOO_SMALL        1742L        [fail]
#define RPC_S_STRING_TOO_LONG            1743L        [fail]
#define RPC_S_PROTSEQ_NOT_FOUND          1744L        [fail]
#define RPC_S_PROCNUM_OUT_OF_RANGE       1745L        [fail]
#define RPC_S_BINDING_HAS_NO_AUTH        1746L        [fail]
#define RPC_S_UNKNOWN_AUTHN_SERVICE      1747L        [fail]
#define RPC_S_UNKNOWN_AUTHN_LEVEL        1748L        [fail]
#define RPC_S_INVALID_AUTH_IDENTITY      1749L        [fail]
#define RPC_S_UNKNOWN_AUTHZ_SERVICE      1750L        [fail]
#define EPT_S_INVALID_ENTRY              1751L        [fail]
#define EPT_S_CANT_PERFORM_OP            1752L        [fail]
#define EPT_S_NOT_REGISTERED             1753L        [fail]
#define RPC_S_NOTHING_TO_EXPORT          1754L        [fail]
#define RPC_S_INCOMPLETE_NAME            1755L        [fail]
#define RPC_S_INVALID_VERS_OPTION        1756L        [fail]
#define RPC_S_NO_MORE_MEMBERS            1757L        [fail]
#define RPC_S_NOT_ALL_OBJS_UNEXPORTED    1758L        [fail]
#define RPC_S_INTERFACE_NOT_FOUND        1759L        [fail]
#define RPC_S_ENTRY_ALREADY_EXISTS       1760L        [fail]
#define RPC_S_ENTRY_NOT_FOUND            1761L        [fail]
#define RPC_S_NAME_SERVICE_UNAVAILABLE   1762L        [fail]
#define RPC_S_INVALID_NAF_ID             1763L        [fail]
#define RPC_S_CANNOT_SUPPORT             1764L        [fail]
#define RPC_S_NO_CONTEXT_AVAILABLE       1765L        [fail]
#define RPC_S_INTERNAL_ERROR             1766L        [fail]
#define RPC_S_ZERO_DIVIDE                1767L        [fail]
#define RPC_S_ADDRESS_ERROR              1768L        [fail]
#define RPC_S_FP_DIV_ZERO                1769L        [fail]
#define RPC_S_FP_UNDERFLOW               1770L        [fail]
#define RPC_S_FP_OVERFLOW                1771L        [fail]
#define RPC_X_NO_MORE_ENTRIES            1772L        [fail]
#define RPC_X_SS_CHAR_TRANS_OPEN_FAIL    1773L        [fail]
#define RPC_X_SS_CHAR_TRANS_SHORT_FILE   1774L        [fail]
#define RPC_X_SS_IN_NULL_CONTEXT         1775L        [fail]
#define RPC_X_SS_CONTEXT_DAMAGED         1777L        [fail]
#define RPC_X_SS_HANDLES_MISMATCH        1778L        [fail]
#define RPC_X_SS_CANNOT_GET_CALL_HANDLE  1779L        [fail]
#define RPC_X_NULL_REF_POINTER           1780L        [fail]
#define RPC_X_ENUM_VALUE_OUT_OF_RANGE    1781L        [fail]
#define RPC_X_BYTE_COUNT_TOO_SMALL       1782L        [fail]
#define RPC_X_BAD_STUB_DATA              1783L        [fail]
#define ERROR_INVALID_USER_BUFFER        1784L        [fail]
#define ERROR_UNRECOGNIZED_MEDIA         1785L        [fail]
#define ERROR_NO_TRUST_LSA_SECRET        1786L        [fail]
#define ERROR_NO_TRUST_SAM_ACCOUNT       1787L        [fail]
#define ERROR_TRUSTED_DOMAIN_FAILURE     1788L        [fail]
#define ERROR_TRUSTED_RELATIONSHIP_FAILURE 1789L      [fail]
#define ERROR_TRUST_FAILURE              1790L        [fail]
#define RPC_S_CALL_IN_PROGRESS           1791L        [fail]
#define ERROR_NETLOGON_NOT_STARTED       1792L        [fail]
#define ERROR_ACCOUNT_EXPIRED            1793L        [fail]
#define ERROR_REDIRECTOR_HAS_OPEN_HANDLES 1794L       [fail]
#define ERROR_PRINTER_DRIVER_ALREADY_INSTALLED 1795L  [fail]
#define ERROR_UNKNOWN_PORT               1796L        [fail]
#define ERROR_UNKNOWN_PRINTER_DRIVER     1797L        [fail]
#define ERROR_UNKNOWN_PRINTPROCESSOR     1798L        [fail]
#define ERROR_INVALID_SEPARATOR_FILE     1799L        [fail]
#define ERROR_INVALID_PRIORITY           1800L        [fail]
#define ERROR_INVALID_PRINTER_NAME       1801L        [fail]
#define ERROR_PRINTER_ALREADY_EXISTS     1802L        [fail]
#define ERROR_INVALID_PRINTER_COMMAND    1803L        [fail]
#define ERROR_INVALID_DATATYPE           1804L        [fail]
#define ERROR_INVALID_ENVIRONMENT        1805L        [fail]
#define RPC_S_NO_MORE_BINDINGS           1806L        [fail]
#define ERROR_NOLOGON_INTERDOMAIN_TRUST_ACCOUNT 1807L [fail]
#define ERROR_NOLOGON_WORKSTATION_TRUST_ACCOUNT 1808L [fail]
#define ERROR_NOLOGON_SERVER_TRUST_ACCOUNT 1809L      [fail]
#define ERROR_DOMAIN_TRUST_INCONSISTENT  1810L        [fail]
#define ERROR_SERVER_HAS_OPEN_HANDLES    1811L        [fail]
#define ERROR_RESOURCE_DATA_NOT_FOUND    1812L        [fail]
#define ERROR_RESOURCE_TYPE_NOT_FOUND    1813L        [fail]
#define ERROR_RESOURCE_NAME_NOT_FOUND    1814L        [fail]
#define ERROR_RESOURCE_LANG_NOT_FOUND    1815L        [fail]
#define ERROR_NOT_ENOUGH_QUOTA           1816L        [fail]
#define RPC_S_NO_INTERFACES              1817L        [fail]
#define RPC_S_CALL_CANCELLED             1818L        [fail]
#define RPC_S_BINDING_INCOMPLETE         1819L        [fail]
#define RPC_S_COMM_FAILURE               1820L        [fail]
#define RPC_S_UNSUPPORTED_AUTHN_LEVEL    1821L        [fail]
#define RPC_S_NO_PRINC_NAME              1822L        [fail]
#define RPC_S_NOT_RPC_ERROR              1823L        [fail]
#define RPC_S_UUID_LOCAL_ONLY            1824L        [fail]
#define RPC_S_SEC_PKG_ERROR              1825L        [fail]
#define RPC_S_NOT_CANCELLED              1826L        [fail]
#define RPC_X_INVALID_ES_ACTION          1827L        [fail]
#define RPC_X_WRONG_ES_VERSION           1828L        [fail]
#define RPC_X_WRONG_STUB_VERSION         1829L        [fail]
#define RPC_X_INVALID_PIPE_OBJECT        1830L        [fail]
#define RPC_X_WRONG_PIPE_ORDER           1831L        [fail]
#define RPC_X_WRONG_PIPE_VERSION         1832L        [fail]
#define RPC_S_GROUP_MEMBER_NOT_FOUND     1898L        [fail]
#define EPT_S_CANT_CREATE                1899L        [fail]
#define RPC_S_INVALID_OBJECT             1900L        [fail]
#define ERROR_INVALID_TIME               1901L        [fail]
#define ERROR_INVALID_FORM_NAME          1902L        [fail]
#define ERROR_INVALID_FORM_SIZE          1903L        [fail]
#define ERROR_ALREADY_WAITING            1904L        [fail]
#define ERROR_PRINTER_DELETED            1905L        [fail]
#define ERROR_INVALID_PRINTER_STATE      1906L        [fail]
#define ERROR_PASSWORD_MUST_CHANGE       1907L        [fail]
#define ERROR_DOMAIN_CONTROLLER_NOT_FOUND 1908L       [fail]
#define ERROR_ACCOUNT_LOCKED_OUT         1909L        [fail]
#define OR_INVALID_OXID                  1910L        [fail]
#define OR_INVALID_OID                   1911L        [fail]
#define OR_INVALID_SET                   1912L        [fail]
#define RPC_S_SEND_INCOMPLETE            1913L        [fail]
#define RPC_S_INVALID_ASYNC_HANDLE       1914L        [fail]
#define RPC_S_INVALID_ASYNC_CALL         1915L        [fail]
#define RPC_X_PIPE_CLOSED                1916L        [fail]
#define RPC_X_PIPE_DISCIPLINE_ERROR      1917L        [fail]
#define RPC_X_PIPE_EMPTY                 1918L        [fail]
#define ERROR_NO_SITENAME                1919L        [fail]
#define ERROR_CANT_ACCESS_FILE           1920L        [fail]
#define ERROR_CANT_RESOLVE_FILENAME      1921L        [fail]
#define ERROR_DS_MEMBERSHIP_EVALUATED_LOCALLY 1922L   [fail]
#define ERROR_DS_NO_ATTRIBUTE_OR_VALUE   1923L        [fail]
#define ERROR_DS_INVALID_ATTRIBUTE_SYNTAX 1924L       [fail]
#define ERROR_DS_ATTRIBUTE_TYPE_UNDEFINED 1925L       [fail]
#define ERROR_DS_ATTRIBUTE_OR_VALUE_EXISTS 1926L      [fail]
#define ERROR_DS_BUSY                    1927L        [fail]
#define ERROR_DS_UNAVAILABLE             1928L        [fail]
#define ERROR_DS_NO_RIDS_ALLOCATED       1929L        [fail]
#define ERROR_DS_NO_MORE_RIDS            1930L        [fail]
#define ERROR_DS_INCORRECT_ROLE_OWNER    1931L        [fail]
#define ERROR_DS_RIDMGR_INIT_ERROR       1932L        [fail]
#define ERROR_DS_OBJ_CLASS_VIOLATION     1933L        [fail]
#define ERROR_DS_CANT_ON_NON_LEAF        1934L        [fail]
#define ERROR_DS_CANT_ON_RDN             1935L        [fail]
#define ERROR_DS_CANT_MOD_OBJ_CLASS      1936L        [fail]
#define ERROR_DS_CROSS_DOM_MOVE_ERROR    1937L        [fail]
#define ERROR_DS_GC_NOT_AVAILABLE        1938L        [fail]
#define ERROR_NO_BROWSER_SERVERS_FOUND   6118L        [fail]
#define ERROR_INVALID_PIXEL_FORMAT       2000L        [fail]
#define ERROR_BAD_DRIVER                 2001L        [fail]
#define ERROR_INVALID_WINDOW_STYLE       2002L        [fail]
#define ERROR_METAFILE_NOT_SUPPORTED     2003L        [fail]
#define ERROR_TRANSFORM_NOT_SUPPORTED    2004L        [fail]
#define ERROR_CLIPPING_NOT_SUPPORTED     2005L        [fail]

// End of OpenGL error codes


///////////////////////////////////////////
//                                       //
//   Image Color Management Error Code   //
//                                       //
///////////////////////////////////////////


#define ERROR_INVALID_CMM                2300L               [fail]
#define ERROR_INVALID_PROFILE            2301L               [fail]
#define ERROR_TAG_NOT_FOUND              2302L               [fail]
#define ERROR_TAG_NOT_PRESENT            2303L               [fail]
#define ERROR_DUPLICATE_TAG              2304L               [fail]
#define ERROR_PROFILE_NOT_ASSOCIATED_WITH_DEVICE 2305L       [fail]
#define ERROR_PROFILE_NOT_FOUND          2306L               [fail]
#define ERROR_INVALID_COLORSPACE         2307L               [fail]
#define ERROR_ICM_NOT_ENABLED            2308L               [fail]
#define ERROR_DELETING_ICM_XFORM         2309L               [fail]
#define ERROR_INVALID_TRANSFORM          2310L               [fail]


////////////////////////////////////
//                                //
//     Win32 Spooler Error Codes  //
//                                //
////////////////////////////////////


#define ERROR_UNKNOWN_PRINT_MONITOR      3000L               [fail]
#define ERROR_PRINTER_DRIVER_IN_USE      3001L               [fail]
#define ERROR_SPOOL_FILE_NOT_FOUND       3002L               [fail]
#define ERROR_SPL_NO_STARTDOC            3003L               [fail]
#define ERROR_SPL_NO_ADDJOB              3004L               [fail]
#define ERROR_PRINT_PROCESSOR_ALREADY_INSTALLED 3005L        [fail]
#define ERROR_PRINT_MONITOR_ALREADY_INSTALLED 3006L          [fail]
#define ERROR_INVALID_PRINT_MONITOR      3007L               [fail]
#define ERROR_PRINT_MONITOR_IN_USE       3008L               [fail]
#define ERROR_PRINTER_HAS_JOBS_QUEUED    3009L               [fail]
#define ERROR_SUCCESS_REBOOT_REQUIRED    3010L               [fail]
#define ERROR_SUCCESS_RESTART_REQUIRED   3011L               [fail]

////////////////////////////////////
//                                //
//     Wins Error Codes           //
//                                //
////////////////////////////////////

#define ERROR_WINS_INTERNAL              4000L               [fail]
#define ERROR_CAN_NOT_DEL_LOCAL_WINS     4001L               [fail]
#define ERROR_STATIC_INIT                4002L               [fail]
#define ERROR_INC_BACKUP                 4003L               [fail]
#define ERROR_FULL_BACKUP                4004L               [fail]
#define ERROR_REC_NON_EXISTENT           4005L               [fail]
#define ERROR_RPL_NOT_ALLOWED            4006L               [fail]


////////////////////////////////////
//                                //
//     DHCP Error Codes           //
//                                //
////////////////////////////////////


#define ERROR_DHCP_ADDRESS_CONFLICT      4100L               [fail]


////////////////////////////////////
//                                //
//     WMI Error Codes            //
//                                //
////////////////////////////////////

#define ERROR_WMI_GUID_NOT_FOUND         4200L               [fail]
#define ERROR_WMI_INSTANCE_NOT_FOUND     4201L               [fail]
#define ERROR_WMI_ITEMID_NOT_FOUND       4202L               [fail]
#define ERROR_WMI_TRY_AGAIN              4203L               [fail]
#define ERROR_WMI_DP_NOT_FOUND           4204L               [fail]
#define ERROR_WMI_UNRESOLVED_INSTANCE_REF 4205L              [fail]
#define ERROR_WMI_ALREADY_ENABLED        4206L               [fail]
#define ERROR_WMI_GUID_DISCONNECTED      4207L               [fail]
#define ERROR_WMI_SERVER_UNAVAILABLE     4208L               [fail]
#define ERROR_WMI_DP_FAILED              4209L               [fail]
#define ERROR_WMI_INVALID_MOF            4210L               [fail]
#define ERROR_WMI_INVALID_REGINFO        4211L               [fail]

////////////////////////////////////
//                                //
// NT Media Services Error Codes  //
//                                //
////////////////////////////////////


#define ERROR_INVALID_MEDIA              4300L               [fail]
#define ERROR_INVALID_LIBRARY            4301L               [fail]
#define ERROR_INVALID_MEDIA_POOL         4302L               [fail]
#define ERROR_DRIVE_MEDIA_MISMATCH       4303L               [fail]
#define ERROR_MEDIA_OFFLINE              4304L               [fail]
#define ERROR_LIBRARY_OFFLINE            4305L               [fail]
#define ERROR_EMPTY                      4306L               [fail]
#define ERROR_NOT_EMPTY                  4307L               [fail]
#define ERROR_MEDIA_UNAVAILABLE          4308L               [fail]
#define ERROR_RESOURCE_DISABLED          4309L               [fail]
#define ERROR_INVALID_CLEANER            4310L               [fail]
#define ERROR_UNABLE_TO_CLEAN            4311L               [fail]
#define ERROR_OBJECT_NOT_FOUND           4312L               [fail]
#define ERROR_DATABASE_FAILURE           4313L               [fail]
#define ERROR_DATABASE_FULL              4314L               [fail]
#define ERROR_MEDIA_INCOMPATIBLE         4315L               [fail]
#define ERROR_RESOURCE_NOT_PRESENT       4316L               [fail]
#define ERROR_INVALID_OPERATION          4317L               [fail]
#define ERROR_MEDIA_NOT_AVAILABLE        4318L               [fail]
#define ERROR_DEVICE_NOT_AVAILABLE       4319L               [fail]
#define ERROR_REQUEST_REFUSED            4320L               [fail]

////////////////////////////////////////////
//                                        //
// NT Remote Storage Service Error Codes  //
//                                        //
////////////////////////////////////////////


#define ERROR_FILE_OFFLINE               4350L               [fail]
#define ERROR_REMOTE_STORAGE_NOT_ACTIVE  4351L               [fail]
#define ERROR_REMOTE_STORAGE_MEDIA_ERROR 4352L               [fail]

////////////////////////////////////////////
//                                        //
// NT Reparse Points Error Codes          //
//                                        //
////////////////////////////////////////////


#define ERROR_NOT_A_REPARSE_POINT        4390L               [fail]
#define ERROR_REPARSE_ATTRIBUTE_CONFLICT 4391L               [fail]

////////////////////////////////////
//                                //
//     Cluster Error Codes        //
//                                //
////////////////////////////////////


#define ERROR_DEPENDENT_RESOURCE_EXISTS  5001L               [fail]
#define ERROR_DEPENDENCY_NOT_FOUND       5002L               [fail]
#define ERROR_DEPENDENCY_ALREADY_EXISTS  5003L               [fail]
#define ERROR_RESOURCE_NOT_ONLINE        5004L               [fail]
#define ERROR_HOST_NODE_NOT_AVAILABLE    5005L               [fail]
#define ERROR_RESOURCE_NOT_AVAILABLE     5006L               [fail]
#define ERROR_RESOURCE_NOT_FOUND         5007L               [fail]
#define ERROR_SHUTDOWN_CLUSTER           5008L               [fail]
#define ERROR_CANT_EVICT_ACTIVE_NODE     5009L               [fail]
#define ERROR_OBJECT_ALREADY_EXISTS      5010L               [fail]
#define ERROR_OBJECT_IN_LIST             5011L               [fail]
#define ERROR_GROUP_NOT_AVAILABLE        5012L               [fail]
#define ERROR_GROUP_NOT_FOUND            5013L               [fail]
#define ERROR_GROUP_NOT_ONLINE           5014L               [fail]
#define ERROR_HOST_NODE_NOT_RESOURCE_OWNER 5015L             [fail]
#define ERROR_HOST_NODE_NOT_GROUP_OWNER  5016L               [fail]
#define ERROR_RESMON_CREATE_FAILED       5017L               [fail]
#define ERROR_RESMON_ONLINE_FAILED       5018L               [fail]
#define ERROR_RESOURCE_ONLINE            5019L               [fail]
#define ERROR_QUORUM_RESOURCE            5020L               [fail]
#define ERROR_NOT_QUORUM_CAPABLE         5021L               [fail]
#define ERROR_CLUSTER_SHUTTING_DOWN      5022L               [fail]
#define ERROR_INVALID_STATE              5023L               [fail]
#define ERROR_RESOURCE_PROPERTIES_STORED 5024L               [fail]
#define ERROR_NOT_QUORUM_CLASS           5025L               [fail]
#define ERROR_CORE_RESOURCE              5026L               [fail]
#define ERROR_QUORUM_RESOURCE_ONLINE_FAILED 5027L            [fail]
#define ERROR_QUORUMLOG_OPEN_FAILED      5028L               [fail]
#define ERROR_CLUSTERLOG_CORRUPT         5029L               [fail]
#define ERROR_CLUSTERLOG_RECORD_EXCEEDS_MAXSIZE 5030L        [fail]
#define ERROR_CLUSTERLOG_EXCEEDS_MAXSIZE 5031L               [fail]
#define ERROR_CLUSTERLOG_CHKPOINT_NOT_FOUND 5032L            [fail]
#define ERROR_CLUSTERLOG_NOT_ENOUGH_SPACE 5033L              [fail]

////////////////////////////////////
//                                //
//     EFS Error Codes            //
//                                //
////////////////////////////////////


#define ERROR_ENCRYPTION_FAILED          6000L               [fail]
#define ERROR_DECRYPTION_FAILED          6001L               [fail]
#define ERROR_FILE_ENCRYPTED             6002L               [fail]
#define ERROR_NO_RECOVERY_POLICY         6003L               [fail]
#define ERROR_NO_EFS                     6004L               [fail]
#define ERROR_WRONG_EFS                  6005L               [fail]
#define ERROR_NO_USER_KEYS               6006L               [fail]
#define ERROR_FILE_NOT_ENCRYPTED         6007L               [fail]
#define ERROR_NOT_EXPORT_FORMAT          6008L               [fail]
};


value DWORD HRESULT
{
//
// Error definitions follow
//

//
// Codes 0x4000-0x40ff are reserved for OLE
//
//
// Error codes
//
//
// MessageId: E_UNEXPECTED
//
// MessageText:
//
//  Catastrophic failure
//
#define E_UNEXPECTED                     0x8000FFFFL           [fail]

//
// MessageId: E_NOTIMPL
//
// MessageText:
//
//  Not implemented
//
#define E_NOTIMPL                        0x80004001L           [fail]

//
// MessageId: E_OUTOFMEMORY
//
// MessageText:
//
//  Ran out of memory
//
#define E_OUTOFMEMORY                    0x8007000EL           [fail]

//
// MessageId: E_INVALIDARG
//
// MessageText:
//
//  One or more arguments are invalid
//
#define E_INVALIDARG                     0x80070057L           [fail]

//
// MessageId: E_NOINTERFACE
//
// MessageText:
//
//  No such interface supported
//
#define E_NOINTERFACE                    0x80004002L           [fail]

//
// MessageId: E_POINTER
//
// MessageText:
//
//  Invalid pointer
//
#define E_POINTER                        0x80004003L           [fail]

//
// MessageId: E_HANDLE
//
// MessageText:
//
//  Invalid handle
//
#define E_HANDLE                         0x80070006L           [fail]

//
// MessageId: E_ABORT
//
// MessageText:
//
//  Operation aborted
//
#define E_ABORT                          0x80004004L           [fail]

//
// MessageId: E_FAIL
//
// MessageText:
//
//  Unspecified error
//
#define E_FAIL                           0x80004005L           [fail]

//
// MessageId: E_ACCESSDENIED
//
// MessageText:
//
//  General access denied error
//
#define E_ACCESSDENIED                   0x80070005L           [fail]

//
// MessageId: E_NOTIMPL
//
// MessageText:
//
//  Not implemented
//
#define E_NOTIMPL                        0x80000001L           [fail]

//
// MessageId: E_OUTOFMEMORY
//
// MessageText:
//
//  Ran out of memory
//
#define E_OUTOFMEMORY                    0x80000002L           [fail]

//
// MessageId: E_INVALIDARG
//
// MessageText:
//
//  One or more arguments are invalid
//
#define E_INVALIDARG                     0x80000003L           [fail]

//
// MessageId: E_NOINTERFACE
//
// MessageText:
//
//  No such interface supported
//
#define E_NOINTERFACE                    0x80000004L           [fail]

//
// MessageId: E_POINTER
//
// MessageText:
//
//  Invalid pointer
//
#define E_POINTER                        0x80000005L           [fail]

//
// MessageId: E_HANDLE
//
// MessageText:
//
//  Invalid handle
//
#define E_HANDLE                         0x80000006L           [fail]

//
// MessageId: E_ABORT
//
// MessageText:
//
//  Operation aborted
//
#define E_ABORT                          0x80000007L           [fail]

//
// MessageId: E_FAIL
//
// MessageText:
//
//  Unspecified error
//
#define E_FAIL                           0x80000008L           [fail]

//
// MessageId: E_ACCESSDENIED
//
// MessageText:
//
//  General access denied error
//
#define E_ACCESSDENIED                   0x80000009L           [fail]

//
// MessageId: E_PENDING
//
// MessageText:
//
//  The data necessary to complete this operation is not yet available.
//
#define E_PENDING                        0x8000000AL           [fail]

//
// MessageId: CO_E_INIT_TLS
//
// MessageText:
//
//  Thread local storage failure
//
#define CO_E_INIT_TLS                    0x80004006L           [fail]

//
// MessageId: CO_E_INIT_SHARED_ALLOCATOR
//
// MessageText:
//
//  Get shared memory allocator failure
//
#define CO_E_INIT_SHARED_ALLOCATOR       0x80004007L           [fail]

//
// MessageId: CO_E_INIT_MEMORY_ALLOCATOR
//
// MessageText:
//
//  Get memory allocator failure
//
#define CO_E_INIT_MEMORY_ALLOCATOR       0x80004008L           [fail]

//
// MessageId: CO_E_INIT_CLASS_CACHE
//
// MessageText:
//
//  Unable to initialize class cache
//
#define CO_E_INIT_CLASS_CACHE            0x80004009L           [fail]

//
// MessageId: CO_E_INIT_RPC_CHANNEL
//
// MessageText:
//
//  Unable to initialize RPC services
//
#define CO_E_INIT_RPC_CHANNEL            0x8000400AL           [fail]

//
// MessageId: CO_E_INIT_TLS_SET_CHANNEL_CONTROL
//
// MessageText:
//
//  Cannot set thread local storage channel control
//
#define CO_E_INIT_TLS_SET_CHANNEL_CONTROL 0x8000400BL           [fail]

//
// MessageId: CO_E_INIT_TLS_CHANNEL_CONTROL
//
// MessageText:
//
//  Could not allocate thread local storage channel control
//
#define CO_E_INIT_TLS_CHANNEL_CONTROL    0x8000400CL           [fail]

//
// MessageId: CO_E_INIT_UNACCEPTED_USER_ALLOCATOR
//
// MessageText:
//
//  The user supplied memory allocator is unacceptable
//
#define CO_E_INIT_UNACCEPTED_USER_ALLOCATOR 0x8000400DL           [fail]

//
// MessageId: CO_E_INIT_SCM_MUTEX_EXISTS
//
// MessageText:
//
//  The OLE service mutex already exists
//
#define CO_E_INIT_SCM_MUTEX_EXISTS       0x8000400EL           [fail]

//
// MessageId: CO_E_INIT_SCM_FILE_MAPPING_EXISTS
//
// MessageText:
//
//  The OLE service file mapping already exists
//
#define CO_E_INIT_SCM_FILE_MAPPING_EXISTS 0x8000400FL           [fail]

//
// MessageId: CO_E_INIT_SCM_MAP_VIEW_OF_FILE
//
// MessageText:
//
//  Unable to map view of file for OLE service
//
#define CO_E_INIT_SCM_MAP_VIEW_OF_FILE   0x80004010L           [fail]

//
// MessageId: CO_E_INIT_SCM_EXEC_FAILURE
//
// MessageText:
//
//  Failure attempting to launch OLE service
//
#define CO_E_INIT_SCM_EXEC_FAILURE       0x80004011L           [fail]

//
// MessageId: CO_E_INIT_ONLY_SINGLE_THREADED
//
// MessageText:
//
//  There was an attempt to call CoInitialize a second time while single threaded
//
#define CO_E_INIT_ONLY_SINGLE_THREADED   0x80004012L           [fail]

//
// MessageId: CO_E_CANT_REMOTE
//
// MessageText:
//
//  A Remote activation was necessary but was not allowed
//
#define CO_E_CANT_REMOTE                 0x80004013L           [fail]

//
// MessageId: CO_E_BAD_SERVER_NAME
//
// MessageText:
//
//  A Remote activation was necessary but the server name provided was invalid
//
#define CO_E_BAD_SERVER_NAME             0x80004014L           [fail]

//
// MessageId: CO_E_WRONG_SERVER_IDENTITY
//
// MessageText:
//
//  The class is configured to run as a security id different from the caller
//
#define CO_E_WRONG_SERVER_IDENTITY       0x80004015L           [fail]

//
// MessageId: CO_E_OLE1DDE_DISABLED
//
// MessageText:
//
//  Use of Ole1 services requiring DDE windows is disabled
//
#define CO_E_OLE1DDE_DISABLED            0x80004016L           [fail]

//
// MessageId: CO_E_RUNAS_SYNTAX
//
// MessageText:
//
//  A RunAs specification must be <domain name>\<user name> or simply <user name>
//
#define CO_E_RUNAS_SYNTAX                0x80004017L           [fail]

//
// MessageId: CO_E_CREATEPROCESS_FAILURE
//
// MessageText:
//
//  The server process could not be started.  The pathname may be incorrect.
//
#define CO_E_CREATEPROCESS_FAILURE       0x80004018L           [fail]

//
// MessageId: CO_E_RUNAS_CREATEPROCESS_FAILURE
//
// MessageText:
//
//  The server process could not be started as the configured identity.  The pathname may be incorrect or unavailable.
//
#define CO_E_RUNAS_CREATEPROCESS_FAILURE 0x80004019L           [fail]

//
// MessageId: CO_E_RUNAS_LOGON_FAILURE
//
// MessageText:
//
//  The server process could not be started because the configured identity is incorrect.  Check the username and password.
//
#define CO_E_RUNAS_LOGON_FAILURE         0x8000401AL           [fail]

//
// MessageId: CO_E_LAUNCH_PERMSSION_DENIED
//
// MessageText:
//
//  The client is not allowed to launch this server.
//
#define CO_E_LAUNCH_PERMSSION_DENIED     0x8000401BL           [fail]

//
// MessageId: CO_E_START_SERVICE_FAILURE
//
// MessageText:
//
//  The service providing this server could not be started.
//
#define CO_E_START_SERVICE_FAILURE       0x8000401CL           [fail]

//
// MessageId: CO_E_REMOTE_COMMUNICATION_FAILURE
//
// MessageText:
//
//  This computer was unable to communicate with the computer providing the server.
//
#define CO_E_REMOTE_COMMUNICATION_FAILURE 0x8000401DL           [fail]

//
// MessageId: CO_E_SERVER_START_TIMEOUT
//
// MessageText:
//
//  The server did not respond after being launched.
//
#define CO_E_SERVER_START_TIMEOUT        0x8000401EL           [fail]

//
// MessageId: CO_E_CLSREG_INCONSISTENT
//
// MessageText:
//
//  The registration information for this server is inconsistent or incomplete.
//
#define CO_E_CLSREG_INCONSISTENT         0x8000401FL           [fail]

//
// MessageId: CO_E_IIDREG_INCONSISTENT
//
// MessageText:
//
//  The registration information for this interface is inconsistent or incomplete.
//
#define CO_E_IIDREG_INCONSISTENT         0x80004020L           [fail]

//
// MessageId: CO_E_NOT_SUPPORTED
//
// MessageText:
//
//  The operation attempted is not supported.
//
#define CO_E_NOT_SUPPORTED               0x80004021L           [fail]

//
// MessageId: CO_E_RELOAD_DLL
//
// MessageText:
//
//  A dll must be loaded.
//
#define CO_E_RELOAD_DLL                  0x80004022L           [fail]

//
// MessageId: CO_E_MSI_ERROR
//
// MessageText:
//
//  A Microsoft Software Installer error was encountered.
//
#define CO_E_MSI_ERROR                   0x80004023L           [fail]


//
// Success codes
//
#define S_OK                                   0x00000000L
#define S_FALSE                                0x00000001L

// ******************
// FACILITY_ITF
// ******************

//
// Codes 0x0-0x01ff are reserved for the OLE group of
// interfaces.
//


//
// Generic OLE errors that may be returned by many inerfaces
//

#define OLE_E_FIRST 0x80040000L
#define OLE_E_LAST  0x800400FFL
#define OLE_S_FIRST 0x00040000L
#define OLE_S_LAST  0x000400FFL

//
// Old OLE errors
//
//
// MessageId: OLE_E_OLEVERB
//
// MessageText:
//
//  Invalid OLEVERB structure
//
#define OLE_E_OLEVERB                    0x80040000L           [fail]

//
// MessageId: OLE_E_ADVF
//
// MessageText:
//
//  Invalid advise flags
//
#define OLE_E_ADVF                       0x80040001L           [fail]

//
// MessageId: OLE_E_ENUM_NOMORE
//
// MessageText:
//
//  Can't enumerate any more, because the associated data is missing
//
#define OLE_E_ENUM_NOMORE                0x80040002L           [fail]

//
// MessageId: OLE_E_ADVISENOTSUPPORTED
//
// MessageText:
//
//  This implementation doesn't take advises
//
#define OLE_E_ADVISENOTSUPPORTED         0x80040003L           [fail]

//
// MessageId: OLE_E_NOCONNECTION
//
// MessageText:
//
//  There is no connection for this connection ID
//
#define OLE_E_NOCONNECTION               0x80040004L           [fail]

//
// MessageId: OLE_E_NOTRUNNING
//
// MessageText:
//
//  Need to run the object to perform this operation
//
#define OLE_E_NOTRUNNING                 0x80040005L           [fail]

//
// MessageId: OLE_E_NOCACHE
//
// MessageText:
//
//  There is no cache to operate on
//
#define OLE_E_NOCACHE                    0x80040006L           [fail]

//
// MessageId: OLE_E_BLANK
//
// MessageText:
//
//  Uninitialized object
//
#define OLE_E_BLANK                      0x80040007L           [fail]

//
// MessageId: OLE_E_CLASSDIFF
//
// MessageText:
//
//  Linked object's source class has changed
//
#define OLE_E_CLASSDIFF                  0x80040008L           [fail]

//
// MessageId: OLE_E_CANT_GETMONIKER
//
// MessageText:
//
//  Not able to get the moniker of the object
//
#define OLE_E_CANT_GETMONIKER            0x80040009L           [fail]

//
// MessageId: OLE_E_CANT_BINDTOSOURCE
//
// MessageText:
//
//  Not able to bind to the source
//
#define OLE_E_CANT_BINDTOSOURCE          0x8004000AL           [fail]

//
// MessageId: OLE_E_STATIC
//
// MessageText:
//
//  Object is static; operation not allowed
//
#define OLE_E_STATIC                     0x8004000BL           [fail]

//
// MessageId: OLE_E_PROMPTSAVECANCELLED
//
// MessageText:
//
//  User canceled out of save dialog
//
#define OLE_E_PROMPTSAVECANCELLED        0x8004000CL           [fail]

//
// MessageId: OLE_E_INVALIDRECT
//
// MessageText:
//
//  Invalid rectangle
//
#define OLE_E_INVALIDRECT                0x8004000DL           [fail]

//
// MessageId: OLE_E_WRONGCOMPOBJ
//
// MessageText:
//
//  compobj.dll is too old for the ole2.dll initialized
//
#define OLE_E_WRONGCOMPOBJ               0x8004000EL           [fail]

//
// MessageId: OLE_E_INVALIDHWND
//
// MessageText:
//
//  Invalid window handle
//
#define OLE_E_INVALIDHWND                0x8004000FL           [fail]

//
// MessageId: OLE_E_NOT_INPLACEACTIVE
//
// MessageText:
//
//  Object is not in any of the inplace active states
//
#define OLE_E_NOT_INPLACEACTIVE          0x80040010L           [fail]

//
// MessageId: OLE_E_CANTCONVERT
//
// MessageText:
//
//  Not able to convert object
//
#define OLE_E_CANTCONVERT                0x80040011L           [fail]

//
// MessageId: OLE_E_NOSTORAGE
//
// MessageText:
//
//  Not able to perform the operation because object is not given storage yet
//
//
#define OLE_E_NOSTORAGE                  0x80040012L           [fail]

//
// MessageId: DV_E_FORMATETC
//
// MessageText:
//
//  Invalid FORMATETC structure
//
#define DV_E_FORMATETC                   0x80040064L           [fail]

//
// MessageId: DV_E_DVTARGETDEVICE
//
// MessageText:
//
//  Invalid DVTARGETDEVICE structure
//
#define DV_E_DVTARGETDEVICE              0x80040065L           [fail]

//
// MessageId: DV_E_STGMEDIUM
//
// MessageText:
//
//  Invalid STDGMEDIUM structure
//
#define DV_E_STGMEDIUM                   0x80040066L           [fail]

//
// MessageId: DV_E_STATDATA
//
// MessageText:
//
//  Invalid STATDATA structure
//
#define DV_E_STATDATA                    0x80040067L           [fail]

//
// MessageId: DV_E_LINDEX
//
// MessageText:
//
//  Invalid lindex
//
#define DV_E_LINDEX                      0x80040068L           [fail]

//
// MessageId: DV_E_TYMED
//
// MessageText:
//
//  Invalid tymed
//
#define DV_E_TYMED                       0x80040069L           [fail]

//
// MessageId: DV_E_CLIPFORMAT
//
// MessageText:
//
//  Invalid clipboard format
//
#define DV_E_CLIPFORMAT                  0x8004006AL           [fail]

//
// MessageId: DV_E_DVASPECT
//
// MessageText:
//
//  Invalid aspect(s)
//
#define DV_E_DVASPECT                    0x8004006BL           [fail]

//
// MessageId: DV_E_DVTARGETDEVICE_SIZE
//
// MessageText:
//
//  tdSize parameter of the DVTARGETDEVICE structure is invalid
//
#define DV_E_DVTARGETDEVICE_SIZE         0x8004006CL           [fail]

//
// MessageId: DV_E_NOIVIEWOBJECT
//
// MessageText:
//
//  Object doesn't support IViewObject interface
//
#define DV_E_NOIVIEWOBJECT               0x8004006DL           [fail]

#define DRAGDROP_E_FIRST 0x80040100L
#define DRAGDROP_E_LAST  0x8004010FL
#define DRAGDROP_S_FIRST 0x00040100L
#define DRAGDROP_S_LAST  0x0004010FL
//
// MessageId: DRAGDROP_E_NOTREGISTERED
//
// MessageText:
//
//  Trying to revoke a drop target that has not been registered
//
#define DRAGDROP_E_NOTREGISTERED         0x80040100L           [fail]

//
// MessageId: DRAGDROP_E_ALREADYREGISTERED
//
// MessageText:
//
//  This window has already been registered as a drop target
//
#define DRAGDROP_E_ALREADYREGISTERED     0x80040101L           [fail]

//
// MessageId: DRAGDROP_E_INVALIDHWND
//
// MessageText:
//
//  Invalid window handle
//
#define DRAGDROP_E_INVALIDHWND           0x80040102L           [fail]

#define CLASSFACTORY_E_FIRST  0x80040110L
#define CLASSFACTORY_E_LAST   0x8004011FL
#define CLASSFACTORY_S_FIRST  0x00040110L
#define CLASSFACTORY_S_LAST   0x0004011FL
//
// MessageId: CLASS_E_NOAGGREGATION
//
// MessageText:
//
//  Class does not support aggregation (or class object is remote)
//
#define CLASS_E_NOAGGREGATION            0x80040110L           [fail]

//
// MessageId: CLASS_E_CLASSNOTAVAILABLE
//
// MessageText:
//
//  ClassFactory cannot supply requested class
//
#define CLASS_E_CLASSNOTAVAILABLE        0x80040111L           [fail]

//
// MessageId: CLASS_E_NOTLICENSED
//
// MessageText:
//
//  Class is not licensed for use
//
#define CLASS_E_NOTLICENSED              0x80040112L           [fail]

#define MARSHAL_E_FIRST  0x80040120L
#define MARSHAL_E_LAST   0x8004012FL
#define MARSHAL_S_FIRST  0x00040120L
#define MARSHAL_S_LAST   0x0004012FL
#define DATA_E_FIRST     0x80040130L
#define DATA_E_LAST      0x8004013FL
#define DATA_S_FIRST     0x00040130L
#define DATA_S_LAST      0x0004013FL
#define VIEW_E_FIRST     0x80040140L
#define VIEW_E_LAST      0x8004014FL
#define VIEW_S_FIRST     0x00040140L
#define VIEW_S_LAST      0x0004014FL
//
// MessageId: VIEW_E_DRAW
//
// MessageText:
//
//  Error drawing view
//
#define VIEW_E_DRAW                      0x80040140L           [fail]

#define REGDB_E_FIRST     0x80040150L
#define REGDB_E_LAST      0x8004015FL
#define REGDB_S_FIRST     0x00040150L
#define REGDB_S_LAST      0x0004015FL
//
// MessageId: REGDB_E_READREGDB
//
// MessageText:
//
//  Could not read key from registry
//
#define REGDB_E_READREGDB                0x80040150L           [fail]

//
// MessageId: REGDB_E_WRITEREGDB
//
// MessageText:
//
//  Could not write key to registry
//
#define REGDB_E_WRITEREGDB               0x80040151L           [fail]

//
// MessageId: REGDB_E_KEYMISSING
//
// MessageText:
//
//  Could not find the key in the registry
//
#define REGDB_E_KEYMISSING               0x80040152L           [fail]

//
// MessageId: REGDB_E_INVALIDVALUE
//
// MessageText:
//
//  Invalid value for registry
//
#define REGDB_E_INVALIDVALUE             0x80040153L           [fail]

//
// MessageId: REGDB_E_CLASSNOTREG
//
// MessageText:
//
//  Class not registered
//
#define REGDB_E_CLASSNOTREG              0x80040154L           [fail]

//
// MessageId: REGDB_E_IIDNOTREG
//
// MessageText:
//
//  Interface not registered
//
#define REGDB_E_IIDNOTREG                0x80040155L           [fail]

#define CAT_E_FIRST     0x80040160L
#define CAT_E_LAST      0x80040161L
//
// MessageId: CAT_E_CATIDNOEXIST
//
// MessageText:
//
//  CATID does not exist
//
#define CAT_E_CATIDNOEXIST               0x80040160L           [fail]

//
// MessageId: CAT_E_NODESCRIPTION
//
// MessageText:
//
//  Description not found
//
#define CAT_E_NODESCRIPTION              0x80040161L           [fail]

////////////////////////////////////
//                                //
//     Class Store Error Codes    //
//                                //
////////////////////////////////////
#define CS_E_FIRST     0x80040164L
#define CS_E_LAST      0x80040168L
//
// MessageId: CS_E_PACKAGE_NOTFOUND
//
// MessageText:
//
//  No package in Class Store meets this criteria
//
#define CS_E_PACKAGE_NOTFOUND            0x80040164L           [fail]

//
// MessageId: CS_E_NOT_DELETABLE
//
// MessageText:
//
//  Deleting this will break referential integrity
//
#define CS_E_NOT_DELETABLE               0x80040165L           [fail]

//
// MessageId: CS_E_CLASS_NOTFOUND
//
// MessageText:
//
//  No such CLSID in Class Store
//
#define CS_E_CLASS_NOTFOUND              0x80040166L           [fail]

//
// MessageId: CS_E_INVALID_VERSION
//
// MessageText:
//
//  The Class Store is corrupted or has a version that is no more supported
//
#define CS_E_INVALID_VERSION             0x80040167L           [fail]

//
// MessageId: CS_E_NO_CLASSSTORE
//
// MessageText:
//
//  No such Class Store
//
#define CS_E_NO_CLASSSTORE               0x80040168L           [fail]

#define CACHE_E_FIRST     0x80040170L
#define CACHE_E_LAST      0x8004017FL
#define CACHE_S_FIRST     0x00040170L
#define CACHE_S_LAST      0x0004017FL
//
// MessageId: CACHE_E_NOCACHE_UPDATED
//
// MessageText:
//
//  Cache not updated
//
#define CACHE_E_NOCACHE_UPDATED          0x80040170L           [fail]

#define OLEOBJ_E_FIRST     0x80040180L
#define OLEOBJ_E_LAST      0x8004018FL
#define OLEOBJ_S_FIRST     0x00040180L
#define OLEOBJ_S_LAST      0x0004018FL
//
// MessageId: OLEOBJ_E_NOVERBS
//
// MessageText:
//
//  No verbs for OLE object
//
#define OLEOBJ_E_NOVERBS                 0x80040180L           [fail]

//
// MessageId: OLEOBJ_E_INVALIDVERB
//
// MessageText:
//
//  Invalid verb for OLE object
//
#define OLEOBJ_E_INVALIDVERB             0x80040181L           [fail]

#define CLIENTSITE_E_FIRST     0x80040190L
#define CLIENTSITE_E_LAST      0x8004019FL
#define CLIENTSITE_S_FIRST     0x00040190L
#define CLIENTSITE_S_LAST      0x0004019FL
//
// MessageId: INPLACE_E_NOTUNDOABLE
//
// MessageText:
//
//  Undo is not available
//
#define INPLACE_E_NOTUNDOABLE            0x800401A0L           [fail]

//
// MessageId: INPLACE_E_NOTOOLSPACE
//
// MessageText:
//
//  Space for tools is not available
//
#define INPLACE_E_NOTOOLSPACE            0x800401A1L           [fail]

#define INPLACE_E_FIRST     0x800401A0L
#define INPLACE_E_LAST      0x800401AFL
#define INPLACE_S_FIRST     0x000401A0L
#define INPLACE_S_LAST      0x000401AFL
#define ENUM_E_FIRST        0x800401B0L
#define ENUM_E_LAST         0x800401BFL
#define ENUM_S_FIRST        0x000401B0L
#define ENUM_S_LAST         0x000401BFL
#define CONVERT10_E_FIRST        0x800401C0L
#define CONVERT10_E_LAST         0x800401CFL
#define CONVERT10_S_FIRST        0x000401C0L
#define CONVERT10_S_LAST         0x000401CFL
//
// MessageId: CONVERT10_E_OLESTREAM_GET
//
// MessageText:
//
//  OLESTREAM Get method failed
//
#define CONVERT10_E_OLESTREAM_GET        0x800401C0L           [fail]

//
// MessageId: CONVERT10_E_OLESTREAM_PUT
//
// MessageText:
//
//  OLESTREAM Put method failed
//
#define CONVERT10_E_OLESTREAM_PUT        0x800401C1L           [fail]

//
// MessageId: CONVERT10_E_OLESTREAM_FMT
//
// MessageText:
//
//  Contents of the OLESTREAM not in correct format
//
#define CONVERT10_E_OLESTREAM_FMT        0x800401C2L           [fail]

//
// MessageId: CONVERT10_E_OLESTREAM_BITMAP_TO_DIB
//
// MessageText:
//
//  There was an error in a Windows GDI call while converting the bitmap to a DIB
//
#define CONVERT10_E_OLESTREAM_BITMAP_TO_DIB 0x800401C3L           [fail]

//
// MessageId: CONVERT10_E_STG_FMT
//
// MessageText:
//
//  Contents of the IStorage not in correct format
//
#define CONVERT10_E_STG_FMT              0x800401C4L           [fail]

//
// MessageId: CONVERT10_E_STG_NO_STD_STREAM
//
// MessageText:
//
//  Contents of IStorage is missing one of the standard streams
//
#define CONVERT10_E_STG_NO_STD_STREAM    0x800401C5L           [fail]

//
// MessageId: CONVERT10_E_STG_DIB_TO_BITMAP
//
// MessageText:
//
//  There was an error in a Windows GDI call while converting the DIB to a bitmap.
//
//
#define CONVERT10_E_STG_DIB_TO_BITMAP    0x800401C6L           [fail]

#define CLIPBRD_E_FIRST        0x800401D0L
#define CLIPBRD_E_LAST         0x800401DFL
#define CLIPBRD_S_FIRST        0x000401D0L
#define CLIPBRD_S_LAST         0x000401DFL
//
// MessageId: CLIPBRD_E_CANT_OPEN
//
// MessageText:
//
//  OpenClipboard Failed
//
#define CLIPBRD_E_CANT_OPEN              0x800401D0L           [fail]

//
// MessageId: CLIPBRD_E_CANT_EMPTY
//
// MessageText:
//
//  EmptyClipboard Failed
//
#define CLIPBRD_E_CANT_EMPTY             0x800401D1L           [fail]

//
// MessageId: CLIPBRD_E_CANT_SET
//
// MessageText:
//
//  SetClipboard Failed
//
#define CLIPBRD_E_CANT_SET               0x800401D2L           [fail]

//
// MessageId: CLIPBRD_E_BAD_DATA
//
// MessageText:
//
//  Data on clipboard is invalid
//
#define CLIPBRD_E_BAD_DATA               0x800401D3L           [fail]

//
// MessageId: CLIPBRD_E_CANT_CLOSE
//
// MessageText:
//
//  CloseClipboard Failed
//
#define CLIPBRD_E_CANT_CLOSE             0x800401D4L           [fail]

#define MK_E_FIRST        0x800401E0L
#define MK_E_LAST         0x800401EFL
#define MK_S_FIRST        0x000401E0L
#define MK_S_LAST         0x000401EFL
//
// MessageId: MK_E_CONNECTMANUALLY
//
// MessageText:
//
//  Moniker needs to be connected manually
//
#define MK_E_CONNECTMANUALLY             0x800401E0L           [fail]

//
// MessageId: MK_E_EXCEEDEDDEADLINE
//
// MessageText:
//
//  Operation exceeded deadline
//
#define MK_E_EXCEEDEDDEADLINE            0x800401E1L           [fail]

//
// MessageId: MK_E_NEEDGENERIC
//
// MessageText:
//
//  Moniker needs to be generic
//
#define MK_E_NEEDGENERIC                 0x800401E2L           [fail]

//
// MessageId: MK_E_UNAVAILABLE
//
// MessageText:
//
//  Operation unavailable
//
#define MK_E_UNAVAILABLE                 0x800401E3L           [fail]

//
// MessageId: MK_E_SYNTAX
//
// MessageText:
//
//  Invalid syntax
//
#define MK_E_SYNTAX                      0x800401E4L           [fail]

//
// MessageId: MK_E_NOOBJECT
//
// MessageText:
//
//  No object for moniker
//
#define MK_E_NOOBJECT                    0x800401E5L           [fail]

//
// MessageId: MK_E_INVALIDEXTENSION
//
// MessageText:
//
//  Bad extension for file
//
#define MK_E_INVALIDEXTENSION            0x800401E6L           [fail]

//
// MessageId: MK_E_INTERMEDIATEINTERFACENOTSUPPORTED
//
// MessageText:
//
//  Intermediate operation failed
//
#define MK_E_INTERMEDIATEINTERFACENOTSUPPORTED 0x800401E7L           [fail]

//
// MessageId: MK_E_NOTBINDABLE
//
// MessageText:
//
//  Moniker is not bindable
//
#define MK_E_NOTBINDABLE                 0x800401E8L           [fail]

//
// MessageId: MK_E_NOTBOUND
//
// MessageText:
//
//  Moniker is not bound
//
#define MK_E_NOTBOUND                    0x800401E9L           [fail]

//
// MessageId: MK_E_CANTOPENFILE
//
// MessageText:
//
//  Moniker cannot open file
//
#define MK_E_CANTOPENFILE                0x800401EAL           [fail]

//
// MessageId: MK_E_MUSTBOTHERUSER
//
// MessageText:
//
//  User input required for operation to succeed
//
#define MK_E_MUSTBOTHERUSER              0x800401EBL           [fail]

//
// MessageId: MK_E_NOINVERSE
//
// MessageText:
//
//  Moniker class has no inverse
//
#define MK_E_NOINVERSE                   0x800401ECL           [fail]

//
// MessageId: MK_E_NOSTORAGE
//
// MessageText:
//
//  Moniker does not refer to storage
//
#define MK_E_NOSTORAGE                   0x800401EDL           [fail]

//
// MessageId: MK_E_NOPREFIX
//
// MessageText:
//
//  No common prefix
//
#define MK_E_NOPREFIX                    0x800401EEL           [fail]

//
// MessageId: MK_E_ENUMERATION_FAILED
//
// MessageText:
//
//  Moniker could not be enumerated
//
#define MK_E_ENUMERATION_FAILED          0x800401EFL           [fail]

#define CO_E_FIRST        0x800401F0L
#define CO_E_LAST         0x800401FFL
#define CO_S_FIRST        0x000401F0L
#define CO_S_LAST         0x000401FFL
//
// MessageId: CO_E_NOTINITIALIZED
//
// MessageText:
//
//  CoInitialize has not been called.
//
#define CO_E_NOTINITIALIZED              0x800401F0L           [fail]

//
// MessageId: CO_E_ALREADYINITIALIZED
//
// MessageText:
//
//  CoInitialize has already been called.
//
#define CO_E_ALREADYINITIALIZED          0x800401F1L           [fail]

//
// MessageId: CO_E_CANTDETERMINECLASS
//
// MessageText:
//
//  Class of object cannot be determined
//
#define CO_E_CANTDETERMINECLASS          0x800401F2L           [fail]

//
// MessageId: CO_E_CLASSSTRING
//
// MessageText:
//
//  Invalid class string
//
#define CO_E_CLASSSTRING                 0x800401F3L           [fail]

//
// MessageId: CO_E_IIDSTRING
//
// MessageText:
//
//  Invalid interface string
//
#define CO_E_IIDSTRING                   0x800401F4L           [fail]

//
// MessageId: CO_E_APPNOTFOUND
//
// MessageText:
//
//  Application not found
//
#define CO_E_APPNOTFOUND                 0x800401F5L           [fail]

//
// MessageId: CO_E_APPSINGLEUSE
//
// MessageText:
//
//  Application cannot be run more than once
//
#define CO_E_APPSINGLEUSE                0x800401F6L           [fail]

//
// MessageId: CO_E_ERRORINAPP
//
// MessageText:
//
//  Some error in application program
//
#define CO_E_ERRORINAPP                  0x800401F7L           [fail]

//
// MessageId: CO_E_DLLNOTFOUND
//
// MessageText:
//
//  DLL for class not found
//
#define CO_E_DLLNOTFOUND                 0x800401F8L           [fail]

//
// MessageId: CO_E_ERRORINDLL
//
// MessageText:
//
//  Error in the DLL
//
#define CO_E_ERRORINDLL                  0x800401F9L           [fail]

//
// MessageId: CO_E_WRONGOSFORAPP
//
// MessageText:
//
//  Wrong OS or OS version for application
//
#define CO_E_WRONGOSFORAPP               0x800401FAL           [fail]

//
// MessageId: CO_E_OBJNOTREG
//
// MessageText:
//
//  Object is not registered
//
#define CO_E_OBJNOTREG                   0x800401FBL           [fail]

//
// MessageId: CO_E_OBJISREG
//
// MessageText:
//
//  Object is already registered
//
#define CO_E_OBJISREG                    0x800401FCL           [fail]

//
// MessageId: CO_E_OBJNOTCONNECTED
//
// MessageText:
//
//  Object is not connected to server
//
#define CO_E_OBJNOTCONNECTED             0x800401FDL           [fail]

//
// MessageId: CO_E_APPDIDNTREG
//
// MessageText:
//
//  Application was launched but it didn't register a class factory
//
#define CO_E_APPDIDNTREG                 0x800401FEL           [fail]

//
// MessageId: CO_E_RELEASED
//
// MessageText:
//
//  Object has been released
//
#define CO_E_RELEASED                    0x800401FFL           [fail]

//
// MessageId: CO_E_FAILEDTOIMPERSONATE
//
// MessageText:
//
//  Unable to impersonate DCOM client
//
#define CO_E_FAILEDTOIMPERSONATE         0x80040200L           [fail]

//
// MessageId: CO_E_FAILEDTOGETSECCTX
//
// MessageText:
//
//  Unable to obtain server's security context
//
#define CO_E_FAILEDTOGETSECCTX           0x80040201L           [fail]

//
// MessageId: CO_E_FAILEDTOOPENTHREADTOKEN
//
// MessageText:
//
//  Unable to open the access token of the current thread
//
#define CO_E_FAILEDTOOPENTHREADTOKEN     0x80040202L           [fail]

//
// MessageId: CO_E_FAILEDTOGETTOKENINFO
//
// MessageText:
//
//  Unable to obtain user info from an access token
//
#define CO_E_FAILEDTOGETTOKENINFO        0x80040203L           [fail]

//
// MessageId: CO_E_TRUSTEEDOESNTMATCHCLIENT
//
// MessageText:
//
//  The client who called IAccessControl::IsAccessPermitted was the trustee provided tot he method
//
#define CO_E_TRUSTEEDOESNTMATCHCLIENT    0x80040204L           [fail]

//
// MessageId: CO_E_FAILEDTOQUERYCLIENTBLANKET
//
// MessageText:
//
//  Unable to obtain the client's security blanket
//
#define CO_E_FAILEDTOQUERYCLIENTBLANKET  0x80040205L           [fail]

//
// MessageId: CO_E_FAILEDTOSETDACL
//
// MessageText:
//
//  Unable to set a discretionary ACL into a security descriptor
//
#define CO_E_FAILEDTOSETDACL             0x80040206L           [fail]

//
// MessageId: CO_E_ACCESSCHECKFAILED
//
// MessageText:
//
//  The system function, AccessCheck, returned false
//
#define CO_E_ACCESSCHECKFAILED           0x80040207L           [fail]

//
// MessageId: CO_E_NETACCESSAPIFAILED
//
// MessageText:
//
//  Either NetAccessDel or NetAccessAdd returned an error code.
//
#define CO_E_NETACCESSAPIFAILED          0x80040208L           [fail]

//
// MessageId: CO_E_WRONGTRUSTEENAMESYNTAX
//
// MessageText:
//
//  One of the trustee strings provided by the user did not conform to the <Domain>\<Name> syntax and it was not the "*" string
//
#define CO_E_WRONGTRUSTEENAMESYNTAX      0x80040209L           [fail]

//
// MessageId: CO_E_INVALIDSID
//
// MessageText:
//
//  One of the security identifiers provided by the user was invalid
//
#define CO_E_INVALIDSID                  0x8004020AL           [fail]

//
// MessageId: CO_E_CONVERSIONFAILED
//
// MessageText:
//
//  Unable to convert a wide character trustee string to a multibyte trustee string
//
#define CO_E_CONVERSIONFAILED            0x8004020BL           [fail]

//
// MessageId: CO_E_NOMATCHINGSIDFOUND
//
// MessageText:
//
//  Unable to find a security identifier that corresponds to a trustee string provided by the user
//
#define CO_E_NOMATCHINGSIDFOUND          0x8004020CL           [fail]

//
// MessageId: CO_E_LOOKUPACCSIDFAILED
//
// MessageText:
//
//  The system function, LookupAccountSID, failed
//
#define CO_E_LOOKUPACCSIDFAILED          0x8004020DL           [fail]

//
// MessageId: CO_E_NOMATCHINGNAMEFOUND
//
// MessageText:
//
//  Unable to find a trustee name that corresponds to a security identifier provided by the user
//
#define CO_E_NOMATCHINGNAMEFOUND         0x8004020EL           [fail]

//
// MessageId: CO_E_LOOKUPACCNAMEFAILED
//
// MessageText:
//
//  The system function, LookupAccountName, failed
//
#define CO_E_LOOKUPACCNAMEFAILED         0x8004020FL           [fail]

//
// MessageId: CO_E_SETSERLHNDLFAILED
//
// MessageText:
//
//  Unable to set or reset a serialization handle
//
#define CO_E_SETSERLHNDLFAILED           0x80040210L           [fail]

//
// MessageId: CO_E_FAILEDTOGETWINDIR
//
// MessageText:
//
//  Unable to obtain the Windows directory
//
#define CO_E_FAILEDTOGETWINDIR           0x80040211L           [fail]

//
// MessageId: CO_E_PATHTOOLONG
//
// MessageText:
//
//  Path too long
//
#define CO_E_PATHTOOLONG                 0x80040212L           [fail]

//
// MessageId: CO_E_FAILEDTOGENUUID
//
// MessageText:
//
//  Unable to generate a uuid.
//
#define CO_E_FAILEDTOGENUUID             0x80040213L           [fail]

//
// MessageId: CO_E_FAILEDTOCREATEFILE
//
// MessageText:
//
//  Unable to create file
//
#define CO_E_FAILEDTOCREATEFILE          0x80040214L           [fail]

//
// MessageId: CO_E_FAILEDTOCLOSEHANDLE
//
// MessageText:
//
//  Unable to close a serialization handle or a file handle.
//
#define CO_E_FAILEDTOCLOSEHANDLE         0x80040215L           [fail]

//
// MessageId: CO_E_EXCEEDSYSACLLIMIT
//
// MessageText:
//
//  The number of ACEs in an ACL exceeds the system limit
//
#define CO_E_EXCEEDSYSACLLIMIT           0x80040216L           [fail]

//
// MessageId: CO_E_ACESINWRONGORDER
//
// MessageText:
//
//  Not all the DENY_ACCESS ACEs are arranged in front of the GRANT_ACCESS ACEs in the stream
//
#define CO_E_ACESINWRONGORDER            0x80040217L           [fail]

//
// MessageId: CO_E_INCOMPATIBLESTREAMVERSION
//
// MessageText:
//
//  The version of ACL format in the stream is not supported by this implementation of IAccessControl
//
#define CO_E_INCOMPATIBLESTREAMVERSION   0x80040218L           [fail]

//
// MessageId: CO_E_FAILEDTOOPENPROCESSTOKEN
//
// MessageText:
//
//  Unable to open the access token of the server process
//
#define CO_E_FAILEDTOOPENPROCESSTOKEN    0x80040219L           [fail]

//
// MessageId: CO_E_DECODEFAILED
//
// MessageText:
//
//  Unable to decode the ACL in the stream provided by the user
//
#define CO_E_DECODEFAILED                0x8004021AL           [fail]

//
// MessageId: CO_E_ACNOTINITIALIZED
//
// MessageText:
//
//  The COM IAccessControl object is not initialized
//
#define CO_E_ACNOTINITIALIZED            0x8004021BL           [fail]

//
// Old OLE Success Codes
//
//
// MessageId: OLE_S_USEREG
//
// MessageText:
//
//  Use the registry database to provide the requested information
//
#define OLE_S_USEREG                     0x00040000L           [fail]

//
// MessageId: OLE_S_STATIC
//
// MessageText:
//
//  Success, but static
//
#define OLE_S_STATIC                     0x00040001L           [fail]

//
// MessageId: OLE_S_MAC_CLIPFORMAT
//
// MessageText:
//
//  Macintosh clipboard format
//
#define OLE_S_MAC_CLIPFORMAT             0x00040002L           [fail]

//
// MessageId: DRAGDROP_S_DROP
//
// MessageText:
//
//  Successful drop took place
//
#define DRAGDROP_S_DROP                  0x00040100L           [fail]

//
// MessageId: DRAGDROP_S_CANCEL
//
// MessageText:
//
//  Drag-drop operation canceled
//
#define DRAGDROP_S_CANCEL                0x00040101L           [fail]

//
// MessageId: DRAGDROP_S_USEDEFAULTCURSORS
//
// MessageText:
//
//  Use the default cursor
//
#define DRAGDROP_S_USEDEFAULTCURSORS     0x00040102L           [fail]

//
// MessageId: DATA_S_SAMEFORMATETC
//
// MessageText:
//
//  Data has same FORMATETC
//
#define DATA_S_SAMEFORMATETC             0x00040130L           [fail]

//
// MessageId: VIEW_S_ALREADY_FROZEN
//
// MessageText:
//
//  View is already frozen
//
#define VIEW_S_ALREADY_FROZEN            0x00040140L           [fail]

//
// MessageId: CACHE_S_FORMATETC_NOTSUPPORTED
//
// MessageText:
//
//  FORMATETC not supported
//
#define CACHE_S_FORMATETC_NOTSUPPORTED   0x00040170L           [fail]

//
// MessageId: CACHE_S_SAMECACHE
//
// MessageText:
//
//  Same cache
//
#define CACHE_S_SAMECACHE                0x00040171L           [fail]

//
// MessageId: CACHE_S_SOMECACHES_NOTUPDATED
//
// MessageText:
//
//  Some cache(s) not updated
//
#define CACHE_S_SOMECACHES_NOTUPDATED    0x00040172L           [fail]

//
// MessageId: OLEOBJ_S_INVALIDVERB
//
// MessageText:
//
//  Invalid verb for OLE object
//
#define OLEOBJ_S_INVALIDVERB             0x00040180L           [fail]

//
// MessageId: OLEOBJ_S_CANNOT_DOVERB_NOW
//
// MessageText:
//
//  Verb number is valid but verb cannot be done now
//
#define OLEOBJ_S_CANNOT_DOVERB_NOW       0x00040181L           [fail]

//
// MessageId: OLEOBJ_S_INVALIDHWND
//
// MessageText:
//
//  Invalid window handle passed
//
#define OLEOBJ_S_INVALIDHWND             0x00040182L           [fail]

//
// MessageId: INPLACE_S_TRUNCATED
//
// MessageText:
//
//  Message is too long; some of it had to be truncated before displaying
//
#define INPLACE_S_TRUNCATED              0x000401A0L           [fail]

//
// MessageId: CONVERT10_S_NO_PRESENTATION
//
// MessageText:
//
//  Unable to convert OLESTREAM to IStorage
//
#define CONVERT10_S_NO_PRESENTATION      0x000401C0L           [fail]

//
// MessageId: MK_S_REDUCED_TO_SELF
//
// MessageText:
//
//  Moniker reduced to itself
//
#define MK_S_REDUCED_TO_SELF             0x000401E2L           [fail]

//
// MessageId: MK_S_ME
//
// MessageText:
//
//  Common prefix is this moniker
//
#define MK_S_ME                          0x000401E4L           [fail]

//
// MessageId: MK_S_HIM
//
// MessageText:
//
//  Common prefix is input moniker
//
#define MK_S_HIM                         0x000401E5L           [fail]

//
// MessageId: MK_S_US
//
// MessageText:
//
//  Common prefix is both monikers
//
#define MK_S_US                          0x000401E6L           [fail]

//
// MessageId: MK_S_MONIKERALREADYREGISTERED
//
// MessageText:
//
//  Moniker is already registered in running object table
//
#define MK_S_MONIKERALREADYREGISTERED    0x000401E7L           [fail]

// ******************
// FACILITY_WINDOWS
// ******************
//
// Codes 0x0-0x01ff are reserved for the OLE group of
// interfaces.
//
//
// MessageId: CO_E_CLASS_CREATE_FAILED
//
// MessageText:
//
//  Attempt to create a class object failed
//
#define CO_E_CLASS_CREATE_FAILED         0x80080001L           [fail]

//
// MessageId: CO_E_SCM_ERROR
//
// MessageText:
//
//  OLE service could not bind object
//
#define CO_E_SCM_ERROR                   0x80080002L           [fail]

//
// MessageId: CO_E_SCM_RPC_FAILURE
//
// MessageText:
//
//  RPC communication failed with OLE service
//
#define CO_E_SCM_RPC_FAILURE             0x80080003L           [fail]

//
// MessageId: CO_E_BAD_PATH
//
// MessageText:
//
//  Bad path to object
//
#define CO_E_BAD_PATH                    0x80080004L           [fail]

//
// MessageId: CO_E_SERVER_EXEC_FAILURE
//
// MessageText:
//
//  Server execution failed
//
#define CO_E_SERVER_EXEC_FAILURE         0x80080005L           [fail]

//
// MessageId: CO_E_OBJSRV_RPC_FAILURE
//
// MessageText:
//
//  OLE service could not communicate with the object server
//
#define CO_E_OBJSRV_RPC_FAILURE          0x80080006L           [fail]

//
// MessageId: MK_E_NO_NORMALIZED
//
// MessageText:
//
//  Moniker path could not be normalized
//
#define MK_E_NO_NORMALIZED               0x80080007L           [fail]

//
// MessageId: CO_E_SERVER_STOPPING
//
// MessageText:
//
//  Object server is stopping when OLE service contacts it
//
#define CO_E_SERVER_STOPPING             0x80080008L           [fail]

//
// MessageId: MEM_E_INVALID_ROOT
//
// MessageText:
//
//  An invalid root block pointer was specified
//
#define MEM_E_INVALID_ROOT               0x80080009L           [fail]

//
// MessageId: MEM_E_INVALID_LINK
//
// MessageText:
//
//  An allocation chain contained an invalid link pointer
//
#define MEM_E_INVALID_LINK               0x80080010L           [fail]

//
// MessageId: MEM_E_INVALID_SIZE
//
// MessageText:
//
//  The requested allocation size was too large
//
#define MEM_E_INVALID_SIZE               0x80080011L           [fail]

//
// MessageId: CO_S_NOTALLINTERFACES
//
// MessageText:
//
//  Not all the requested interfaces were available
//
#define CO_S_NOTALLINTERFACES            0x00080012L           [fail]

// ******************
// FACILITY_DISPATCH
// ******************
//
// MessageId: DISP_E_UNKNOWNINTERFACE
//
// MessageText:
//
//  Unknown interface.
//
#define DISP_E_UNKNOWNINTERFACE          0x80020001L           [fail]

//
// MessageId: DISP_E_MEMBERNOTFOUND
//
// MessageText:
//
//  Member not found.
//
#define DISP_E_MEMBERNOTFOUND            0x80020003L           [fail]

//
// MessageId: DISP_E_PARAMNOTFOUND
//
// MessageText:
//
//  Parameter not found.
//
#define DISP_E_PARAMNOTFOUND             0x80020004L           [fail]

//
// MessageId: DISP_E_TYPEMISMATCH
//
// MessageText:
//
//  Type mismatch.
//
#define DISP_E_TYPEMISMATCH              0x80020005L           [fail]

//
// MessageId: DISP_E_UNKNOWNNAME
//
// MessageText:
//
//  Unknown name.
//
#define DISP_E_UNKNOWNNAME               0x80020006L           [fail]

//
// MessageId: DISP_E_NONAMEDARGS
//
// MessageText:
//
//  No named arguments.
//
#define DISP_E_NONAMEDARGS               0x80020007L           [fail]

//
// MessageId: DISP_E_BADVARTYPE
//
// MessageText:
//
//  Bad variable type.
//
#define DISP_E_BADVARTYPE                0x80020008L           [fail]

//
// MessageId: DISP_E_EXCEPTION
//
// MessageText:
//
//  Exception occurred.
//
#define DISP_E_EXCEPTION                 0x80020009L           [fail]

//
// MessageId: DISP_E_OVERFLOW
//
// MessageText:
//
//  Out of present range.
//
#define DISP_E_OVERFLOW                  0x8002000AL           [fail]

//
// MessageId: DISP_E_BADINDEX
//
// MessageText:
//
//  Invalid index.
//
#define DISP_E_BADINDEX                  0x8002000BL           [fail]

//
// MessageId: DISP_E_UNKNOWNLCID
//
// MessageText:
//
//  Unknown language.
//
#define DISP_E_UNKNOWNLCID               0x8002000CL           [fail]

//
// MessageId: DISP_E_ARRAYISLOCKED
//
// MessageText:
//
//  Memory is locked.
//
#define DISP_E_ARRAYISLOCKED             0x8002000DL           [fail]

//
// MessageId: DISP_E_BADPARAMCOUNT
//
// MessageText:
//
//  Invalid number of parameters.
//
#define DISP_E_BADPARAMCOUNT             0x8002000EL           [fail]

//
// MessageId: DISP_E_PARAMNOTOPTIONAL
//
// MessageText:
//
//  Parameter not optional.
//
#define DISP_E_PARAMNOTOPTIONAL          0x8002000FL           [fail]

//
// MessageId: DISP_E_BADCALLEE
//
// MessageText:
//
//  Invalid callee.
//
#define DISP_E_BADCALLEE                 0x80020010L           [fail]

//
// MessageId: DISP_E_NOTACOLLECTION
//
// MessageText:
//
//  Does not support a collection.
//
#define DISP_E_NOTACOLLECTION            0x80020011L           [fail]

//
// MessageId: DISP_E_DIVBYZERO
//
// MessageText:
//
//  Division by zero.
//
#define DISP_E_DIVBYZERO                 0x80020012L           [fail]

//
// MessageId: TYPE_E_BUFFERTOOSMALL
//
// MessageText:
//
//  Buffer too small.
//
#define TYPE_E_BUFFERTOOSMALL            0x80028016L           [fail]

//
// MessageId: TYPE_E_FIELDNOTFOUND
//
// MessageText:
//
//  Field name not defined in the record.
//
#define TYPE_E_FIELDNOTFOUND             0x80028017L           [fail]

//
// MessageId: TYPE_E_INVDATAREAD
//
// MessageText:
//
//  Old format or invalid type library.
//
#define TYPE_E_INVDATAREAD               0x80028018L           [fail]

//
// MessageId: TYPE_E_UNSUPFORMAT
//
// MessageText:
//
//  Old format or invalid type library.
//
#define TYPE_E_UNSUPFORMAT               0x80028019L           [fail]

//
// MessageId: TYPE_E_REGISTRYACCESS
//
// MessageText:
//
//  Error accessing the OLE registry.
//
#define TYPE_E_REGISTRYACCESS            0x8002801CL           [fail]

//
// MessageId: TYPE_E_LIBNOTREGISTERED
//
// MessageText:
//
//  Library not registered.
//
#define TYPE_E_LIBNOTREGISTERED          0x8002801DL           [fail]

//
// MessageId: TYPE_E_UNDEFINEDTYPE
//
// MessageText:
//
//  Bound to unknown type.
//
#define TYPE_E_UNDEFINEDTYPE             0x80028027L           [fail]

//
// MessageId: TYPE_E_QUALIFIEDNAMEDISALLOWED
//
// MessageText:
//
//  Qualified name disallowed.
//
#define TYPE_E_QUALIFIEDNAMEDISALLOWED   0x80028028L           [fail]

//
// MessageId: TYPE_E_INVALIDSTATE
//
// MessageText:
//
//  Invalid forward reference, or reference to uncompiled type.
//
#define TYPE_E_INVALIDSTATE              0x80028029L           [fail]

//
// MessageId: TYPE_E_WRONGTYPEKIND
//
// MessageText:
//
//  Type mismatch.
//
#define TYPE_E_WRONGTYPEKIND             0x8002802AL           [fail]

//
// MessageId: TYPE_E_ELEMENTNOTFOUND
//
// MessageText:
//
//  Element not found.
//
#define TYPE_E_ELEMENTNOTFOUND           0x8002802BL           [fail]

//
// MessageId: TYPE_E_AMBIGUOUSNAME
//
// MessageText:
//
//  Ambiguous name.
//
#define TYPE_E_AMBIGUOUSNAME             0x8002802CL           [fail]

//
// MessageId: TYPE_E_NAMECONFLICT
//
// MessageText:
//
//  Name already exists in the library.
//
#define TYPE_E_NAMECONFLICT              0x8002802DL           [fail]

//
// MessageId: TYPE_E_UNKNOWNLCID
//
// MessageText:
//
//  Unknown LCID.
//
#define TYPE_E_UNKNOWNLCID               0x8002802EL           [fail]

//
// MessageId: TYPE_E_DLLFUNCTIONNOTFOUND
//
// MessageText:
//
//  Function not defined in specified DLL.
//
#define TYPE_E_DLLFUNCTIONNOTFOUND       0x8002802FL           [fail]

//
// MessageId: TYPE_E_BADMODULEKIND
//
// MessageText:
//
//  Wrong module kind for the operation.
//
#define TYPE_E_BADMODULEKIND             0x800288BDL           [fail]

//
// MessageId: TYPE_E_SIZETOOBIG
//
// MessageText:
//
//  Size may not exceed 64K.
//
#define TYPE_E_SIZETOOBIG                0x800288C5L           [fail]

//
// MessageId: TYPE_E_DUPLICATEID
//
// MessageText:
//
//  Duplicate ID in inheritance hierarchy.
//
#define TYPE_E_DUPLICATEID               0x800288C6L           [fail]

//
// MessageId: TYPE_E_INVALIDID
//
// MessageText:
//
//  Incorrect inheritance depth in standard OLE hmember.
//
#define TYPE_E_INVALIDID                 0x800288CFL           [fail]

//
// MessageId: TYPE_E_TYPEMISMATCH
//
// MessageText:
//
//  Type mismatch.
//
#define TYPE_E_TYPEMISMATCH              0x80028CA0L           [fail]

//
// MessageId: TYPE_E_OUTOFBOUNDS
//
// MessageText:
//
//  Invalid number of arguments.
//
#define TYPE_E_OUTOFBOUNDS               0x80028CA1L           [fail]

//
// MessageId: TYPE_E_IOERROR
//
// MessageText:
//
//  I/O Error.
//
#define TYPE_E_IOERROR                   0x80028CA2L           [fail]

//
// MessageId: TYPE_E_CANTCREATETMPFILE
//
// MessageText:
//
//  Error creating unique tmp file.
//
#define TYPE_E_CANTCREATETMPFILE         0x80028CA3L           [fail]

//
// MessageId: TYPE_E_CANTLOADLIBRARY
//
// MessageText:
//
//  Error loading type library/DLL.
//
#define TYPE_E_CANTLOADLIBRARY           0x80029C4AL           [fail]

//
// MessageId: TYPE_E_INCONSISTENTPROPFUNCS
//
// MessageText:
//
//  Inconsistent property functions.
//
#define TYPE_E_INCONSISTENTPROPFUNCS     0x80029C83L           [fail]

//
// MessageId: TYPE_E_CIRCULARTYPE
//
// MessageText:
//
//  Circular dependency between types/modules.
//
#define TYPE_E_CIRCULARTYPE              0x80029C84L           [fail]

// ******************
// FACILITY_STORAGE
// ******************
//
// MessageId: STG_E_INVALIDFUNCTION
//
// MessageText:
//
//  Unable to perform requested operation.
//
#define STG_E_INVALIDFUNCTION            0x80030001L           [fail]

//
// MessageId: STG_E_FILENOTFOUND
//
// MessageText:
//
//  %1 could not be found.
//
#define STG_E_FILENOTFOUND               0x80030002L           [fail]

//
// MessageId: STG_E_PATHNOTFOUND
//
// MessageText:
//
//  The path %1 could not be found.
//
#define STG_E_PATHNOTFOUND               0x80030003L           [fail]

//
// MessageId: STG_E_TOOMANYOPENFILES
//
// MessageText:
//
//  There are insufficient resources to open another file.
//
#define STG_E_TOOMANYOPENFILES           0x80030004L           [fail]

//
// MessageId: STG_E_ACCESSDENIED
//
// MessageText:
//
//  Access Denied.
//
#define STG_E_ACCESSDENIED               0x80030005L           [fail]

//
// MessageId: STG_E_INVALIDHANDLE
//
// MessageText:
//
//  Attempted an operation on an invalid object.
//
#define STG_E_INVALIDHANDLE              0x80030006L           [fail]

//
// MessageId: STG_E_INSUFFICIENTMEMORY
//
// MessageText:
//
//  There is insufficient memory available to complete operation.
//
#define STG_E_INSUFFICIENTMEMORY         0x80030008L           [fail]

//
// MessageId: STG_E_INVALIDPOINTER
//
// MessageText:
//
//  Invalid pointer error.
//
#define STG_E_INVALIDPOINTER             0x80030009L           [fail]

//
// MessageId: STG_E_NOMOREFILES
//
// MessageText:
//
//  There are no more entries to return.
//
#define STG_E_NOMOREFILES                0x80030012L           [fail]

//
// MessageId: STG_E_DISKISWRITEPROTECTED
//
// MessageText:
//
//  Disk is write-protected.
//
#define STG_E_DISKISWRITEPROTECTED       0x80030013L           [fail]

//
// MessageId: STG_E_SEEKERROR
//
// MessageText:
//
//  An error occurred during a seek operation.
//
#define STG_E_SEEKERROR                  0x80030019L           [fail]

//
// MessageId: STG_E_WRITEFAULT
//
// MessageText:
//
//  A disk error occurred during a write operation.
//
#define STG_E_WRITEFAULT                 0x8003001DL           [fail]

//
// MessageId: STG_E_READFAULT
//
// MessageText:
//
//  A disk error occurred during a read operation.
//
#define STG_E_READFAULT                  0x8003001EL           [fail]

//
// MessageId: STG_E_SHAREVIOLATION
//
// MessageText:
//
//  A share violation has occurred.
//
#define STG_E_SHAREVIOLATION             0x80030020L           [fail]

//
// MessageId: STG_E_LOCKVIOLATION
//
// MessageText:
//
//  A lock violation has occurred.
//
#define STG_E_LOCKVIOLATION              0x80030021L           [fail]

//
// MessageId: STG_E_FILEALREADYEXISTS
//
// MessageText:
//
//  %1 already exists.
//
#define STG_E_FILEALREADYEXISTS          0x80030050L           [fail]

//
// MessageId: STG_E_INVALIDPARAMETER
//
// MessageText:
//
//  Invalid parameter error.
//
#define STG_E_INVALIDPARAMETER           0x80030057L           [fail]

//
// MessageId: STG_E_MEDIUMFULL
//
// MessageText:
//
//  There is insufficient disk space to complete operation.
//
#define STG_E_MEDIUMFULL                 0x80030070L           [fail]

//
// MessageId: STG_E_PROPSETMISMATCHED
//
// MessageText:
//
//  Illegal write of non-simple property to simple property set.
//
#define STG_E_PROPSETMISMATCHED          0x800300F0L           [fail]

//
// MessageId: STG_E_ABNORMALAPIEXIT
//
// MessageText:
//
//  An API call exited abnormally.
//
#define STG_E_ABNORMALAPIEXIT            0x800300FAL           [fail]

//
// MessageId: STG_E_INVALIDHEADER
//
// MessageText:
//
//  The file %1 is not a valid compound file.
//
#define STG_E_INVALIDHEADER              0x800300FBL           [fail]

//
// MessageId: STG_E_INVALIDNAME
//
// MessageText:
//
//  The name %1 is not valid.
//
#define STG_E_INVALIDNAME                0x800300FCL           [fail]

//
// MessageId: STG_E_UNKNOWN
//
// MessageText:
//
//  An unexpected error occurred.
//
#define STG_E_UNKNOWN                    0x800300FDL           [fail]

//
// MessageId: STG_E_UNIMPLEMENTEDFUNCTION
//
// MessageText:
//
//  That function is not implemented.
//
#define STG_E_UNIMPLEMENTEDFUNCTION      0x800300FEL           [fail]

//
// MessageId: STG_E_INVALIDFLAG
//
// MessageText:
//
//  Invalid flag error.
//
#define STG_E_INVALIDFLAG                0x800300FFL           [fail]

//
// MessageId: STG_E_INUSE
//
// MessageText:
//
//  Attempted to use an object that is busy.
//
#define STG_E_INUSE                      0x80030100L           [fail]

//
// MessageId: STG_E_NOTCURRENT
//
// MessageText:
//
//  The storage has been changed since the last commit.
//
#define STG_E_NOTCURRENT                 0x80030101L           [fail]

//
// MessageId: STG_E_REVERTED
//
// MessageText:
//
//  Attempted to use an object that has ceased to exist.
//
#define STG_E_REVERTED                   0x80030102L           [fail]

//
// MessageId: STG_E_CANTSAVE
//
// MessageText:
//
//  Can't save.
//
#define STG_E_CANTSAVE                   0x80030103L           [fail]

//
// MessageId: STG_E_OLDFORMAT
//
// MessageText:
//
//  The compound file %1 was produced with an incompatible version of storage.
//
#define STG_E_OLDFORMAT                  0x80030104L           [fail]

//
// MessageId: STG_E_OLDDLL
//
// MessageText:
//
//  The compound file %1 was produced with a newer version of storage.
//
#define STG_E_OLDDLL                     0x80030105L           [fail]

//
// MessageId: STG_E_SHAREREQUIRED
//
// MessageText:
//
//  Share.exe or equivalent is required for operation.
//
#define STG_E_SHAREREQUIRED              0x80030106L           [fail]

//
// MessageId: STG_E_NOTFILEBASEDSTORAGE
//
// MessageText:
//
//  Illegal operation called on non-file based storage.
//
#define STG_E_NOTFILEBASEDSTORAGE        0x80030107L           [fail]

//
// MessageId: STG_E_EXTANTMARSHALLINGS
//
// MessageText:
//
//  Illegal operation called on object with extant marshallings.
//
#define STG_E_EXTANTMARSHALLINGS         0x80030108L           [fail]

//
// MessageId: STG_E_DOCFILECORRUPT
//
// MessageText:
//
//  The docfile has been corrupted.
//
#define STG_E_DOCFILECORRUPT             0x80030109L           [fail]

//
// MessageId: STG_E_BADBASEADDRESS
//
// MessageText:
//
//  OLE32.DLL has been loaded at the wrong address.
//
#define STG_E_BADBASEADDRESS             0x80030110L           [fail]

//
// MessageId: STG_E_INCOMPLETE
//
// MessageText:
//
//  The file download was aborted abnormally.  The file is incomplete.
//
#define STG_E_INCOMPLETE                 0x80030201L           [fail]

//
// MessageId: STG_E_TERMINATED
//
// MessageText:
//
//  The file download has been terminated.
//
#define STG_E_TERMINATED                 0x80030202L           [fail]

//
// MessageId: STG_S_CONVERTED
//
// MessageText:
//
//  The underlying file was converted to compound file format.
//
#define STG_S_CONVERTED                  0x00030200L           [fail]

//
// MessageId: STG_S_BLOCK
//
// MessageText:
//
//  The storage operation should block until more data is available.
//
#define STG_S_BLOCK                      0x00030201L           [fail]

//
// MessageId: STG_S_RETRYNOW
//
// MessageText:
//
//  The storage operation should retry immediately.
//
#define STG_S_RETRYNOW                   0x00030202L           [fail]

//
// MessageId: STG_S_MONITORING
//
// MessageText:
//
//  The notified event sink will not influence the storage operation.
//
#define STG_S_MONITORING                 0x00030203L           [fail]

//
// MessageId: STG_S_MULTIPLEOPENS
//
// MessageText:
//
//  Multiple opens prevent consolidated. (commit succeeded).
//
#define STG_S_MULTIPLEOPENS              0x00030204L           [fail]

//
// MessageId: STG_S_CONSOLIDATIONFAILED
//
// MessageText:
//
//  Consolidation of the storage file failed. (commit succeeded).
//
#define STG_S_CONSOLIDATIONFAILED        0x00030205L           [fail]

//
// MessageId: STG_S_CANNOTCONSOLIDATE
//
// MessageText:
//
//  Consolidation of the storage file is inappropriate. (commit succeeded).
//
#define STG_S_CANNOTCONSOLIDATE          0x00030206L           [fail]

// ******************
// FACILITY_RPC
// ******************
//
// Codes 0x0-0x11 are propagated from 16 bit OLE.
//
//
// MessageId: RPC_E_CALL_REJECTED
//
// MessageText:
//
//  Call was rejected by callee.
//
#define RPC_E_CALL_REJECTED              0x80010001L           [fail]

//
// MessageId: RPC_E_CALL_CANCELED
//
// MessageText:
//
//  Call was canceled by the message filter.
//
#define RPC_E_CALL_CANCELED              0x80010002L           [fail]

//
// MessageId: RPC_E_CANTPOST_INSENDCALL
//
// MessageText:
//
//  The caller is dispatching an intertask SendMessage call and
//  cannot call out via PostMessage.
//
#define RPC_E_CANTPOST_INSENDCALL        0x80010003L           [fail]

//
// MessageId: RPC_E_CANTCALLOUT_INASYNCCALL
//
// MessageText:
//
//  The caller is dispatching an asynchronous call and cannot
//  make an outgoing call on behalf of this call.
//
#define RPC_E_CANTCALLOUT_INASYNCCALL    0x80010004L           [fail]

//
// MessageId: RPC_E_CANTCALLOUT_INEXTERNALCALL
//
// MessageText:
//
//  It is illegal to call out while inside message filter.
//
#define RPC_E_CANTCALLOUT_INEXTERNALCALL 0x80010005L           [fail]

//
// MessageId: RPC_E_CONNECTION_TERMINATED
//
// MessageText:
//
//  The connection terminated or is in a bogus state
//  and cannot be used any more. Other connections
//  are still valid.
//
#define RPC_E_CONNECTION_TERMINATED      0x80010006L           [fail]

//
// MessageId: RPC_E_SERVER_DIED
//
// MessageText:
//
//  The callee (server [not server application]) is not available
//  and disappeared; all connections are invalid.  The call may
//  have executed.
//
#define RPC_E_SERVER_DIED                0x80010007L           [fail]

//
// MessageId: RPC_E_CLIENT_DIED
//
// MessageText:
//
//  The caller (client) disappeared while the callee (server) was
//  processing a call.
//
#define RPC_E_CLIENT_DIED                0x80010008L           [fail]

//
// MessageId: RPC_E_INVALID_DATAPACKET
//
// MessageText:
//
//  The data packet with the marshalled parameter data is incorrect.
//
#define RPC_E_INVALID_DATAPACKET         0x80010009L           [fail]

//
// MessageId: RPC_E_CANTTRANSMIT_CALL
//
// MessageText:
//
//  The call was not transmitted properly; the message queue
//  was full and was not emptied after yielding.
//
#define RPC_E_CANTTRANSMIT_CALL          0x8001000AL           [fail]

//
// MessageId: RPC_E_CLIENT_CANTMARSHAL_DATA
//
// MessageText:
//
//  The client (caller) cannot marshall the parameter data - low memory, etc.
//
#define RPC_E_CLIENT_CANTMARSHAL_DATA    0x8001000BL           [fail]

//
// MessageId: RPC_E_CLIENT_CANTUNMARSHAL_DATA
//
// MessageText:
//
//  The client (caller) cannot unmarshall the return data - low memory, etc.
//
#define RPC_E_CLIENT_CANTUNMARSHAL_DATA  0x8001000CL           [fail]

//
// MessageId: RPC_E_SERVER_CANTMARSHAL_DATA
//
// MessageText:
//
//  The server (callee) cannot marshall the return data - low memory, etc.
//
#define RPC_E_SERVER_CANTMARSHAL_DATA    0x8001000DL           [fail]

//
// MessageId: RPC_E_SERVER_CANTUNMARSHAL_DATA
//
// MessageText:
//
//  The server (callee) cannot unmarshall the parameter data - low memory, etc.
//
#define RPC_E_SERVER_CANTUNMARSHAL_DATA  0x8001000EL           [fail]

//
// MessageId: RPC_E_INVALID_DATA
//
// MessageText:
//
//  Received data is invalid; could be server or client data.
//
#define RPC_E_INVALID_DATA               0x8001000FL           [fail]

//
// MessageId: RPC_E_INVALID_PARAMETER
//
// MessageText:
//
//  A particular parameter is invalid and cannot be (un)marshalled.
//
#define RPC_E_INVALID_PARAMETER          0x80010010L           [fail]

//
// MessageId: RPC_E_CANTCALLOUT_AGAIN
//
// MessageText:
//
//  There is no second outgoing call on same channel in DDE conversation.
//
#define RPC_E_CANTCALLOUT_AGAIN          0x80010011L           [fail]

//
// MessageId: RPC_E_SERVER_DIED_DNE
//
// MessageText:
//
//  The callee (server [not server application]) is not available
//  and disappeared; all connections are invalid.  The call did not execute.
//
#define RPC_E_SERVER_DIED_DNE            0x80010012L           [fail]

//
// MessageId: RPC_E_SYS_CALL_FAILED
//
// MessageText:
//
//  System call failed.
//
#define RPC_E_SYS_CALL_FAILED            0x80010100L           [fail]

//
// MessageId: RPC_E_OUT_OF_RESOURCES
//
// MessageText:
//
//  Could not allocate some required resource (memory, events, ...)
//
#define RPC_E_OUT_OF_RESOURCES           0x80010101L           [fail]

//
// MessageId: RPC_E_ATTEMPTED_MULTITHREAD
//
// MessageText:
//
//  Attempted to make calls on more than one thread in single threaded mode.
//
#define RPC_E_ATTEMPTED_MULTITHREAD      0x80010102L           [fail]

//
// MessageId: RPC_E_NOT_REGISTERED
//
// MessageText:
//
//  The requested interface is not registered on the server object.
//
#define RPC_E_NOT_REGISTERED             0x80010103L           [fail]

//
// MessageId: RPC_E_FAULT
//
// MessageText:
//
//  RPC could not call the server or could not return the results of calling the server.
//
#define RPC_E_FAULT                      0x80010104L           [fail]

//
// MessageId: RPC_E_SERVERFAULT
//
// MessageText:
//
//  The server threw an exception.
//
#define RPC_E_SERVERFAULT                0x80010105L           [fail]

//
// MessageId: RPC_E_CHANGED_MODE
//
// MessageText:
//
//  Cannot change thread mode after it is set.
//
#define RPC_E_CHANGED_MODE               0x80010106L           [fail]

//
// MessageId: RPC_E_INVALIDMETHOD
//
// MessageText:
//
//  The method called does not exist on the server.
//
#define RPC_E_INVALIDMETHOD              0x80010107L           [fail]

//
// MessageId: RPC_E_DISCONNECTED
//
// MessageText:
//
//  The object invoked has disconnected from its clients.
//
#define RPC_E_DISCONNECTED               0x80010108L           [fail]

//
// MessageId: RPC_E_RETRY
//
// MessageText:
//
//  The object invoked chose not to process the call now.  Try again later.
//
#define RPC_E_RETRY                      0x80010109L           [fail]

//
// MessageId: RPC_E_SERVERCALL_RETRYLATER
//
// MessageText:
//
//  The message filter indicated that the application is busy.
//
#define RPC_E_SERVERCALL_RETRYLATER      0x8001010AL           [fail]

//
// MessageId: RPC_E_SERVERCALL_REJECTED
//
// MessageText:
//
//  The message filter rejected the call.
//
#define RPC_E_SERVERCALL_REJECTED        0x8001010BL           [fail]

//
// MessageId: RPC_E_INVALID_CALLDATA
//
// MessageText:
//
//  A call control interfaces was called with invalid data.
//
#define RPC_E_INVALID_CALLDATA           0x8001010CL           [fail]

//
// MessageId: RPC_E_CANTCALLOUT_ININPUTSYNCCALL
//
// MessageText:
//
//  An outgoing call cannot be made since the application is dispatching an input-synchronous call.
//
#define RPC_E_CANTCALLOUT_ININPUTSYNCCALL 0x8001010DL           [fail]

//
// MessageId: RPC_E_WRONG_THREAD
//
// MessageText:
//
//  The application called an interface that was marshalled for a different thread.
//
#define RPC_E_WRONG_THREAD               0x8001010EL           [fail]

//
// MessageId: RPC_E_THREAD_NOT_INIT
//
// MessageText:
//
//  CoInitialize has not been called on the current thread.
//
#define RPC_E_THREAD_NOT_INIT            0x8001010FL           [fail]

//
// MessageId: RPC_E_VERSION_MISMATCH
//
// MessageText:
//
//  The version of OLE on the client and server machines does not match.
//
#define RPC_E_VERSION_MISMATCH           0x80010110L           [fail]

//
// MessageId: RPC_E_INVALID_HEADER
//
// MessageText:
//
//  OLE received a packet with an invalid header.
//
#define RPC_E_INVALID_HEADER             0x80010111L           [fail]

//
// MessageId: RPC_E_INVALID_EXTENSION
//
// MessageText:
//
//  OLE received a packet with an invalid extension.
//
#define RPC_E_INVALID_EXTENSION          0x80010112L           [fail]

//
// MessageId: RPC_E_INVALID_IPID
//
// MessageText:
//
//  The requested object or interface does not exist.
//
#define RPC_E_INVALID_IPID               0x80010113L           [fail]

//
// MessageId: RPC_E_INVALID_OBJECT
//
// MessageText:
//
//  The requested object does not exist.
//
#define RPC_E_INVALID_OBJECT             0x80010114L           [fail]

//
// MessageId: RPC_S_CALLPENDING
//
// MessageText:
//
//  OLE has sent a request and is waiting for a reply.
//
#define RPC_S_CALLPENDING                0x80010115L           [fail]

//
// MessageId: RPC_S_WAITONTIMER
//
// MessageText:
//
//  OLE is waiting before retrying a request.
//
#define RPC_S_WAITONTIMER                0x80010116L           [fail]

//
// MessageId: RPC_E_CALL_COMPLETE
//
// MessageText:
//
//  Call context cannot be accessed after call completed.
//
#define RPC_E_CALL_COMPLETE              0x80010117L           [fail]

//
// MessageId: RPC_E_UNSECURE_CALL
//
// MessageText:
//
//  Impersonate on unsecure calls is not supported.
//
#define RPC_E_UNSECURE_CALL              0x80010118L           [fail]

//
// MessageId: RPC_E_TOO_LATE
//
// MessageText:
//
//  Security must be initialized before any interfaces are marshalled or
//  unmarshalled.  It cannot be changed once initialized.
//
#define RPC_E_TOO_LATE                   0x80010119L           [fail]

//
// MessageId: RPC_E_NO_GOOD_SECURITY_PACKAGES
//
// MessageText:
//
//  No security packages are installed on this machine or the user is not logged
//  on or there are no compatible security packages between the client and server.
//
#define RPC_E_NO_GOOD_SECURITY_PACKAGES  0x8001011AL           [fail]

//
// MessageId: RPC_E_ACCESS_DENIED
//
// MessageText:
//
//  Access is denied.
//
#define RPC_E_ACCESS_DENIED              0x8001011BL           [fail]

//
// MessageId: RPC_E_REMOTE_DISABLED
//
// MessageText:
//
//  Remote calls are not allowed for this process.
//
#define RPC_E_REMOTE_DISABLED            0x8001011CL           [fail]

//
// MessageId: RPC_E_INVALID_OBJREF
//
// MessageText:
//
//  The marshaled interface data packet (OBJREF) has an invalid or unknown format.
//
#define RPC_E_INVALID_OBJREF             0x8001011DL           [fail]

//
// MessageId: RPC_E_NO_CONTEXT
//
// MessageText:
//
//  No context is associated with this call.  This happens for some custom
//  marshalled calls and on the client side of the call.
//
#define RPC_E_NO_CONTEXT                 0x8001011EL           [fail]

//
// MessageId: RPC_E_TIMEOUT
//
// MessageText:
//
//  This operation returned because the timeout period expired.
//
#define RPC_E_TIMEOUT                    0x8001011FL           [fail]

//
// MessageId: RPC_E_NO_SYNC
//
// MessageText:
//
//  There are no synchronize objects to wait on.
//
#define RPC_E_NO_SYNC                    0x80010120L           [fail]

//
// MessageId: RPC_E_UNEXPECTED
//
// MessageText:
//
//  An internal error occurred.
//
#define RPC_E_UNEXPECTED                 0x8001FFFFL           [fail]


 /////////////////
 //
 //  FACILITY_SSPI
 //
 /////////////////

//
// MessageId: NTE_BAD_UID
//
// MessageText:
//
//  Bad UID.
//
#define NTE_BAD_UID                      0x80090001L           [fail]

//
// MessageId: NTE_BAD_HASH
//
// MessageText:
//
//  Bad Hash.
//
#define NTE_BAD_HASH                     0x80090002L           [fail]

//
// MessageId: NTE_BAD_KEY
//
// MessageText:
//
//  Bad Key.
//
#define NTE_BAD_KEY                      0x80090003L           [fail]

//
// MessageId: NTE_BAD_LEN
//
// MessageText:
//
//  Bad Length.
//
#define NTE_BAD_LEN                      0x80090004L           [fail]

//
// MessageId: NTE_BAD_DATA
//
// MessageText:
//
//  Bad Data.
//
#define NTE_BAD_DATA                     0x80090005L           [fail]

//
// MessageId: NTE_BAD_SIGNATURE
//
// MessageText:
//
//  Invalid Signature.
//
#define NTE_BAD_SIGNATURE                0x80090006L           [fail]

//
// MessageId: NTE_BAD_VER
//
// MessageText:
//
//  Bad Version of provider.
//
#define NTE_BAD_VER                      0x80090007L           [fail]

//
// MessageId: NTE_BAD_ALGID
//
// MessageText:
//
//  Invalid algorithm specified.
//
#define NTE_BAD_ALGID                    0x80090008L           [fail]

//
// MessageId: NTE_BAD_FLAGS
//
// MessageText:
//
//  Invalid flags specified.
//
#define NTE_BAD_FLAGS                    0x80090009L           [fail]

//
// MessageId: NTE_BAD_TYPE
//
// MessageText:
//
//  Invalid type specified.
//
#define NTE_BAD_TYPE                     0x8009000AL           [fail]

//
// MessageId: NTE_BAD_KEY_STATE
//
// MessageText:
//
//  Key not valid for use in specified state.
//
#define NTE_BAD_KEY_STATE                0x8009000BL           [fail]

//
// MessageId: NTE_BAD_HASH_STATE
//
// MessageText:
//
//  Hash not valid for use in specified state.
//
#define NTE_BAD_HASH_STATE               0x8009000CL           [fail]

//
// MessageId: NTE_NO_KEY
//
// MessageText:
//
//  Key does not exist.
//
#define NTE_NO_KEY                       0x8009000DL           [fail]

//
// MessageId: NTE_NO_MEMORY
//
// MessageText:
//
//  Insufficient memory available for the operation.
//
#define NTE_NO_MEMORY                    0x8009000EL           [fail]

//
// MessageId: NTE_EXISTS
//
// MessageText:
//
//  Object already exists.
//
#define NTE_EXISTS                       0x8009000FL           [fail]

//
// MessageId: NTE_PERM
//
// MessageText:
//
//  Access denied.
//
#define NTE_PERM                         0x80090010L           [fail]

//
// MessageId: NTE_NOT_FOUND
//
// MessageText:
//
//  Object was not found.
//
#define NTE_NOT_FOUND                    0x80090011L           [fail]

//
// MessageId: NTE_DOUBLE_ENCRYPT
//
// MessageText:
//
//  Data already encrypted.
//
#define NTE_DOUBLE_ENCRYPT               0x80090012L           [fail]

//
// MessageId: NTE_BAD_PROVIDER
//
// MessageText:
//
//  Invalid provider specified.
//
#define NTE_BAD_PROVIDER                 0x80090013L           [fail]

//
// MessageId: NTE_BAD_PROV_TYPE
//
// MessageText:
//
//  Invalid provider type specified.
//
#define NTE_BAD_PROV_TYPE                0x80090014L           [fail]

//
// MessageId: NTE_BAD_PUBLIC_KEY
//
// MessageText:
//
//  Provider's public key is invalid.
//
#define NTE_BAD_PUBLIC_KEY               0x80090015L           [fail]

//
// MessageId: NTE_BAD_KEYSET
//
// MessageText:
//
//  Keyset does not exist
//
#define NTE_BAD_KEYSET                   0x80090016L           [fail]

//
// MessageId: NTE_PROV_TYPE_NOT_DEF
//
// MessageText:
//
//  Provider type not defined.
//
#define NTE_PROV_TYPE_NOT_DEF            0x80090017L           [fail]

//
// MessageId: NTE_PROV_TYPE_ENTRY_BAD
//
// MessageText:
//
//  Provider type as registered is invalid.
//
#define NTE_PROV_TYPE_ENTRY_BAD          0x80090018L           [fail]

//
// MessageId: NTE_KEYSET_NOT_DEF
//
// MessageText:
//
//  The keyset is not defined.
//
#define NTE_KEYSET_NOT_DEF               0x80090019L           [fail]

//
// MessageId: NTE_KEYSET_ENTRY_BAD
//
// MessageText:
//
//  Keyset as registered is invalid.
//
#define NTE_KEYSET_ENTRY_BAD             0x8009001AL           [fail]

//
// MessageId: NTE_PROV_TYPE_NO_MATCH
//
// MessageText:
//
//  Provider type does not match registered value.
//
#define NTE_PROV_TYPE_NO_MATCH           0x8009001BL           [fail]

//
// MessageId: NTE_SIGNATURE_FILE_BAD
//
// MessageText:
//
//  The digital signature file is corrupt.
//
#define NTE_SIGNATURE_FILE_BAD           0x8009001CL           [fail]

//
// MessageId: NTE_PROVIDER_DLL_FAIL
//
// MessageText:
//
//  Provider DLL failed to initialize correctly.
//
#define NTE_PROVIDER_DLL_FAIL            0x8009001DL           [fail]

//
// MessageId: NTE_PROV_DLL_NOT_FOUND
//
// MessageText:
//
//  Provider DLL could not be found.
//
#define NTE_PROV_DLL_NOT_FOUND           0x8009001EL           [fail]

//
// MessageId: NTE_BAD_KEYSET_PARAM
//
// MessageText:
//
//  The Keyset parameter is invalid.
//
#define NTE_BAD_KEYSET_PARAM             0x8009001FL           [fail]

//
// MessageId: NTE_FAIL
//
// MessageText:
//
//  An internal error occurred.
//
#define NTE_FAIL                         0x80090020L           [fail]

//
// MessageId: NTE_SYS_ERR
//
// MessageText:
//
//  A base error occurred.
//
#define NTE_SYS_ERR                      0x80090021L           [fail]

//
// MessageId: CRYPT_E_MSG_ERROR
//
// MessageText:
//
//  An error was encountered doing a cryptographic message operation.
//
#define CRYPT_E_MSG_ERROR                0x80091001L           [fail]

//
// MessageId: CRYPT_E_UNKNOWN_ALGO
//
// MessageText:
//
//  The cryptographic algorithm is unknown.
//
#define CRYPT_E_UNKNOWN_ALGO             0x80091002L           [fail]

//
// MessageId: CRYPT_E_OID_FORMAT
//
// MessageText:
//
//  The object identifier is badly formatted.
//
#define CRYPT_E_OID_FORMAT               0x80091003L           [fail]

//
// MessageId: CRYPT_E_INVALID_MSG_TYPE
//
// MessageText:
//
//  The message type is invalid.
//
#define CRYPT_E_INVALID_MSG_TYPE         0x80091004L           [fail]

//
// MessageId: CRYPT_E_UNEXPECTED_ENCODING
//
// MessageText:
//
//  The message is not encoded as expected.
//
#define CRYPT_E_UNEXPECTED_ENCODING      0x80091005L           [fail]

//
// MessageId: CRYPT_E_AUTH_ATTR_MISSING
//
// MessageText:
//
//  The message does not contain an expected authenticated attribute.
//
#define CRYPT_E_AUTH_ATTR_MISSING        0x80091006L           [fail]

//
// MessageId: CRYPT_E_HASH_VALUE
//
// MessageText:
//
//  The hash value is not correct.
//
#define CRYPT_E_HASH_VALUE               0x80091007L           [fail]

//
// MessageId: CRYPT_E_INVALID_INDEX
//
// MessageText:
//
//  The index value is not valid.
//
#define CRYPT_E_INVALID_INDEX            0x80091008L           [fail]

//
// MessageId: CRYPT_E_ALREADY_DECRYPTED
//
// MessageText:
//
//  The message content has already been decrypted.
//
#define CRYPT_E_ALREADY_DECRYPTED        0x80091009L           [fail]

//
// MessageId: CRYPT_E_NOT_DECRYPTED
//
// MessageText:
//
//  The message content has not been decrypted yet.
//
#define CRYPT_E_NOT_DECRYPTED            0x8009100AL           [fail]

//
// MessageId: CRYPT_E_RECIPIENT_NOT_FOUND
//
// MessageText:
//
//  The enveloped-data message does not contain the specified recipient.
//
#define CRYPT_E_RECIPIENT_NOT_FOUND      0x8009100BL           [fail]

//
// MessageId: CRYPT_E_CONTROL_TYPE
//
// MessageText:
//
//  The control type is not valid.
//
#define CRYPT_E_CONTROL_TYPE             0x8009100CL           [fail]

//
// MessageId: CRYPT_E_ISSUER_SERIALNUMBER
//
// MessageText:
//
//  The issuer and/or serial number are/is not valid.
//
#define CRYPT_E_ISSUER_SERIALNUMBER      0x8009100DL           [fail]

//
// MessageId: CRYPT_E_SIGNER_NOT_FOUND
//
// MessageText:
//
//  The original signer is not found.
//
#define CRYPT_E_SIGNER_NOT_FOUND         0x8009100EL           [fail]

//
// MessageId: CRYPT_E_ATTRIBUTES_MISSING
//
// MessageText:
//
//  The message does not contain the requested attributes.
//
#define CRYPT_E_ATTRIBUTES_MISSING       0x8009100FL           [fail]

//
// MessageId: CRYPT_E_STREAM_MSG_NOT_READY
//
// MessageText:
//
//  The steamed message is note yet able to return the requested data.
//
#define CRYPT_E_STREAM_MSG_NOT_READY     0x80091010L           [fail]

//
// MessageId: CRYPT_E_STREAM_INSUFFICIENT_DATA
//
// MessageText:
//
//  The streamed message needs more data before the decode can complete.
//
#define CRYPT_E_STREAM_INSUFFICIENT_DATA 0x80091011L           [fail]

//
// MessageId: CRYPT_E_BAD_LEN
//
// MessageText:
//
//  The length specified for the output data was insufficient.
//
#define CRYPT_E_BAD_LEN                  0x80092001L           [fail]

//
// MessageId: CRYPT_E_BAD_ENCODE
//
// MessageText:
//
//  An error was encountered while encoding or decoding.
//
#define CRYPT_E_BAD_ENCODE               0x80092002L           [fail]

//
// MessageId: CRYPT_E_FILE_ERROR
//
// MessageText:
//
//  An error occurred while reading or writing to the file
//
#define CRYPT_E_FILE_ERROR               0x80092003L           [fail]

//
// MessageId: CRYPT_E_NOT_FOUND
//
// MessageText:
//
//  The object or property wasn't found
//
#define CRYPT_E_NOT_FOUND                0x80092004L           [fail]

//
// MessageId: CRYPT_E_EXISTS
//
// MessageText:
//
//  The object or property already exists
//
#define CRYPT_E_EXISTS                   0x80092005L           [fail]

//
// MessageId: CRYPT_E_NO_PROVIDER
//
// MessageText:
//
//  No provider was specified for the store or object
//
#define CRYPT_E_NO_PROVIDER              0x80092006L           [fail]

//
// MessageId: CRYPT_E_SELF_SIGNED
//
// MessageText:
//
//  The specified certificate is self signed.
//
#define CRYPT_E_SELF_SIGNED              0x80092007L           [fail]

//
// MessageId: CRYPT_E_DELETED_PREV
//
// MessageText:
//
//  The previous certificate or CRL context was deleted.
//
#define CRYPT_E_DELETED_PREV             0x80092008L           [fail]

//
// MessageId: CRYPT_E_NO_MATCH
//
// MessageText:
//
//  No match when trying to find the object.
//
#define CRYPT_E_NO_MATCH                 0x80092009L           [fail]

//
// MessageId: CRYPT_E_UNEXPECTED_MSG_TYPE
//
// MessageText:
//
//  The type of the cryptographic message being decoded is different than what was expected.
//
#define CRYPT_E_UNEXPECTED_MSG_TYPE      0x8009200AL           [fail]

//
// MessageId: CRYPT_E_NO_KEY_PROPERTY
//
// MessageText:
//
//  The certificate doesn't have a private key property
//
#define CRYPT_E_NO_KEY_PROPERTY          0x8009200BL           [fail]

//
// MessageId: CRYPT_E_NO_DECRYPT_CERT
//
// MessageText:
//
//  No certificate was found having a private key property to use for decrypting.
//
#define CRYPT_E_NO_DECRYPT_CERT          0x8009200CL           [fail]

//
// MessageId: CRYPT_E_BAD_MSG
//
// MessageText:
//
//  Either, not a cryptographic message or incorrectly formatted.
//
#define CRYPT_E_BAD_MSG                  0x8009200DL           [fail]

//
// MessageId: CRYPT_E_NO_SIGNER
//
// MessageText:
//
//  The signed message doesn't have a signer for the specified signer index
//
#define CRYPT_E_NO_SIGNER                0x8009200EL           [fail]

//
// MessageId: CRYPT_E_PENDING_CLOSE
//
// MessageText:
//
//  Final closure is pending until additional frees or closes.
//
#define CRYPT_E_PENDING_CLOSE            0x8009200FL           [fail]

//
// MessageId: CRYPT_E_REVOKED
//
// MessageText:
//
//  The certificate or signature has been revoked
//
#define CRYPT_E_REVOKED                  0x80092010L           [fail]

//
// MessageId: CRYPT_E_NO_REVOCATION_DLL
//
// MessageText:
//
//  No Dll or exported function was found to verify revocation.
//
#define CRYPT_E_NO_REVOCATION_DLL        0x80092011L           [fail]

//
// MessageId: CRYPT_E_NO_REVOCATION_CHECK
//
// MessageText:
//
//  The called function wasn't able to do a revocation check on the certificate or signature.
//
#define CRYPT_E_NO_REVOCATION_CHECK      0x80092012L           [fail]

//
// MessageId: CRYPT_E_REVOCATION_OFFLINE
//
// MessageText:
//
//  Since the revocation server was offline, the called function wasn't able to complete the revocation check.
//
#define CRYPT_E_REVOCATION_OFFLINE       0x80092013L           [fail]

//
// MessageId: CRYPT_E_NOT_IN_REVOCATION_DATABASE
//
// MessageText:
//
//  The certificate or signature to be checked was not found in the revocation servers database.
//
#define CRYPT_E_NOT_IN_REVOCATION_DATABASE 0x80092014L           [fail]

//
// MessageId: CRYPT_E_INVALID_NUMERIC_STRING
//
// MessageText:
//
//  The string contains a non-numeric character.
//
#define CRYPT_E_INVALID_NUMERIC_STRING   0x80092020L           [fail]

//
// MessageId: CRYPT_E_INVALID_PRINTABLE_STRING
//
// MessageText:
//
//  The string contains a non-printable character.
//
#define CRYPT_E_INVALID_PRINTABLE_STRING 0x80092021L           [fail]

//
// MessageId: CRYPT_E_INVALID_IA5_STRING
//
// MessageText:
//
//  The string contains a character not in the 7 bit ASCII character set.
//
#define CRYPT_E_INVALID_IA5_STRING       0x80092022L           [fail]

//
// MessageId: CRYPT_E_INVALID_X500_STRING
//
// MessageText:
//
//  The string contains an invalid X500 name attribute key, oid, value or delimiter.
//
#define CRYPT_E_INVALID_X500_STRING      0x80092023L           [fail]

//
// MessageId: CRYPT_E_NOT_CHAR_STRING
//
// MessageText:
//
//  The dwValueType for the CERT_NAME_VALUE is not one of the character strings.  Most likely it is either a CERT_RDN_ENCODED_BLOB or CERT_TDN_OCTED_STRING.
//
#define CRYPT_E_NOT_CHAR_STRING          0x80092024L           [fail]

//
// MessageId: CRYPT_E_FILERESIZED
//
// MessageText:
//
//  The Put operation can not continue.  The file needs to be resized.  However, there is already a signature present.  A complete signing operation must be done.
//
#define CRYPT_E_FILERESIZED              0x80092025L           [fail]

//
// MessageId: CRYPT_E_SECURITY_SETTINGS
//
// MessageText:
//
//  The cryptography operation has failed due to a local security option setting.
//
#define CRYPT_E_SECURITY_SETTINGS        0x80092026L           [fail]

//
// MessageId: CRYPT_E_NO_VERIFY_USAGE_DLL
//
// MessageText:
//
//  No DLL or exported function was found to verify subject usage.
//
#define CRYPT_E_NO_VERIFY_USAGE_DLL      0x80092027L           [fail]

//
// MessageId: CRYPT_E_NO_VERIFY_USAGE_CHECK
//
// MessageText:
//
//  The called function wasn't able to do a usage check on the subject.
//
#define CRYPT_E_NO_VERIFY_USAGE_CHECK    0x80092028L           [fail]

//
// MessageId: CRYPT_E_VERIFY_USAGE_OFFLINE
//
// MessageText:
//
//  Since the server was offline, the called function wasn't able to complete the usage check.
//
#define CRYPT_E_VERIFY_USAGE_OFFLINE     0x80092029L           [fail]

//
// MessageId: CRYPT_E_NOT_IN_CTL
//
// MessageText:
//
//  The subject was not found in a Certificate Trust List (CTL           [fail].
//
#define CRYPT_E_NOT_IN_CTL               0x8009202AL           [fail]

//
// MessageId: CRYPT_E_NO_TRUSTED_SIGNER
//
// MessageText:
//
//  No trusted signer was found to verify the signature of the message or trust list.
//
#define CRYPT_E_NO_TRUSTED_SIGNER        0x8009202BL           [fail]

//
// MessageId: CRYPT_E_OSS_ERROR
//
// MessageText:
//
//  OSS Certificate encode/decode error code base
//
//  See asn1code.h for a definition of the OSS runtime errors. The OSS
//  error values are offset by CRYPT_E_OSS_ERROR.
//
#define CRYPT_E_OSS_ERROR                0x80093000L           [fail]

//
// MessageId: CERTSRV_E_BAD_REQUESTSUBJECT
//
// MessageText:
//
//  The request subject name is invalid or too long.
//
#define CERTSRV_E_BAD_REQUESTSUBJECT     0x80094001L           [fail]

//
// MessageId: CERTSRV_E_NO_REQUEST
//
// MessageText:
//
//  The request does not exist.
//
#define CERTSRV_E_NO_REQUEST             0x80094002L           [fail]

//
// MessageId: CERTSRV_E_BAD_REQUESTSTATUS
//
// MessageText:
//
//  The request's current status does not allow this operation.
//
#define CERTSRV_E_BAD_REQUESTSTATUS      0x80094003L           [fail]

//
// MessageId: CERTSRV_E_PROPERTY_EMPTY
//
// MessageText:
//
//  The requested property value is empty.
//
#define CERTSRV_E_PROPERTY_EMPTY         0x80094004L           [fail]

//
// MessageId: CERTDB_E_JET_ERROR
//
// MessageText:
//
//  Jet error code base
//
//  See jet.h for a definition of the Jet runtime errors.
//  Negative Jet error values are masked to three digits and offset by CERTDB_E_JET_ERROR.
//
#define CERTDB_E_JET_ERROR               0x80095000L           [fail]

//
// MessageId: TRUST_E_SYSTEM_ERROR
//
// MessageText:
//
//  A system-level error occured while verifying trust.
//
#define TRUST_E_SYSTEM_ERROR             0x80096001L           [fail]

//
// MessageId: TRUST_E_NO_SIGNER_CERT
//
// MessageText:
//
//  The certificate for the signer of the message is invalid or not found.
//
#define TRUST_E_NO_SIGNER_CERT           0x80096002L           [fail]

//
// MessageId: TRUST_E_COUNTER_SIGNER
//
// MessageText:
//
//  One of the counter signers was invalid.
//
#define TRUST_E_COUNTER_SIGNER           0x80096003L           [fail]

//
// MessageId: TRUST_E_CERT_SIGNATURE
//
// MessageText:
//
//  The signature of the certificate can not be verified.
//
#define TRUST_E_CERT_SIGNATURE           0x80096004L           [fail]

//
// MessageId: TRUST_E_TIME_STAMP
//
// MessageText:
//
//  The time stamp signer and or certificate could not be verified or is malformed.
//
#define TRUST_E_TIME_STAMP               0x80096005L           [fail]

//
// MessageId: TRUST_E_BAD_DIGEST
//
// MessageText:
//
//  The objects digest did not verify.
//
#define TRUST_E_BAD_DIGEST               0x80096010L           [fail]

//
// MessageId: TRUST_E_BASIC_CONSTRAINTS
//
// MessageText:
//
//  The cerficates basic constraints are invalid or missing.
//
#define TRUST_E_BASIC_CONSTRAINTS        0x80096019L           [fail]

//
// MessageId: TRUST_E_FINANCIAL_CRITERIA
//
// MessageText:
//
//  The certificate does not meet or contain the Authenticode financial extensions.
//
#define TRUST_E_FINANCIAL_CRITERIA       0x8009601EL           [fail]

#define NTE_OP_OK 0

//
// Note that additional FACILITY_SSPI errors are in issperr.h
//
// ******************
// FACILITY_CERT
// ******************
//
// MessageId: TRUST_E_PROVIDER_UNKNOWN
//
// MessageText:
//
//  The specified trust provider is not known on this system.
//
#define TRUST_E_PROVIDER_UNKNOWN         0x800B0001L           [fail]

//
// MessageId: TRUST_E_ACTION_UNKNOWN
//
// MessageText:
//
//  The trust verification action specified is not supported by the specified trust provider.
//
#define TRUST_E_ACTION_UNKNOWN           0x800B0002L           [fail]

//
// MessageId: TRUST_E_SUBJECT_FORM_UNKNOWN
//
// MessageText:
//
//  The form specified for the subject is not one supported or known by the specified trust provider.
//
#define TRUST_E_SUBJECT_FORM_UNKNOWN     0x800B0003L           [fail]

//
// MessageId: TRUST_E_SUBJECT_NOT_TRUSTED
//
// MessageText:
//
//  The subject is not trusted for the specified action.
//
#define TRUST_E_SUBJECT_NOT_TRUSTED      0x800B0004L           [fail]

//
// MessageId: DIGSIG_E_ENCODE
//
// MessageText:
//
//  Error due to problem in ASN.1 encoding process.
//
#define DIGSIG_E_ENCODE                  0x800B0005L           [fail]

//
// MessageId: DIGSIG_E_DECODE
//
// MessageText:
//
//  Error due to problem in ASN.1 decoding process.
//
#define DIGSIG_E_DECODE                  0x800B0006L           [fail]

//
// MessageId: DIGSIG_E_EXTENSIBILITY
//
// MessageText:
//
//  Reading / writing Extensions where Attributes are appropriate, and visa versa.
//
#define DIGSIG_E_EXTENSIBILITY           0x800B0007L           [fail]

//
// MessageId: DIGSIG_E_CRYPTO
//
// MessageText:
//
//  Unspecified cryptographic failure.
//
#define DIGSIG_E_CRYPTO                  0x800B0008L           [fail]

//
// MessageId: PERSIST_E_SIZEDEFINITE
//
// MessageText:
//
//  The size of the data could not be determined.
//
#define PERSIST_E_SIZEDEFINITE           0x800B0009L           [fail]

//
// MessageId: PERSIST_E_SIZEINDEFINITE
//
// MessageText:
//
//  The size of the indefinite-sized data could not be determined.
//
#define PERSIST_E_SIZEINDEFINITE         0x800B000AL           [fail]

//
// MessageId: PERSIST_E_NOTSELFSIZING
//
// MessageText:
//
//  This object does not read and write self-sizing data.
//
#define PERSIST_E_NOTSELFSIZING          0x800B000BL           [fail]

//
// MessageId: TRUST_E_NOSIGNATURE
//
// MessageText:
//
//  No signature was present in the subject.
//
#define TRUST_E_NOSIGNATURE              0x800B0100L           [fail]

//
// MessageId: CERT_E_EXPIRED
//
// MessageText:
//
//  A required certificate is not within its validity period.
//
#define CERT_E_EXPIRED                   0x800B0101L           [fail]

//
// MessageId: CERT_E_VALIDITYPERIODNESTING
//
// MessageText:
//
//  The validity periods of the certification chain do not nest correctly.
//
#define CERT_E_VALIDITYPERIODNESTING     0x800B0102L           [fail]

//
// MessageId: CERT_E_ROLE
//
// MessageText:
//
//  A certificate that can only be used as an end-entity is being used as a CA or visa versa.
//
#define CERT_E_ROLE                      0x800B0103L           [fail]

//
// MessageId: CERT_E_PATHLENCONST
//
// MessageText:
//
//  A path length constraint in the certification chain has been violated.
//
#define CERT_E_PATHLENCONST              0x800B0104L           [fail]

//
// MessageId: CERT_E_CRITICAL
//
// MessageText:
//
//  An extension of unknown type that is labeled 'critical' is present in a certificate.
//
#define CERT_E_CRITICAL                  0x800B0105L           [fail]

//
// MessageId: CERT_E_PURPOSE
//
// MessageText:
//
//  A certificate is being used for a purpose other than that for which it is permitted.
//
#define CERT_E_PURPOSE                   0x800B0106L           [fail]

//
// MessageId: CERT_E_ISSUERCHAINING
//
// MessageText:
//
//  A parent of a given certificate in fact did not issue that child certificate.
//
#define CERT_E_ISSUERCHAINING            0x800B0107L           [fail]

//
// MessageId: CERT_E_MALFORMED
//
// MessageText:
//
//  A certificate is missing or has an empty value for an important field, such as a subject or issuer name.
//
#define CERT_E_MALFORMED                 0x800B0108L           [fail]

//
// MessageId: CERT_E_UNTRUSTEDROOT
//
// MessageText:
//
//  A certification chain processed correctly, but terminated in a root certificate which isn't trusted by the trust provider.
//
#define CERT_E_UNTRUSTEDROOT             0x800B0109L           [fail]

//
// MessageId: CERT_E_CHAINING
//
// MessageText:
//
//  A chain of certs didn't chain as they should in a certain application of chaining.
//
#define CERT_E_CHAINING                  0x800B010AL           [fail]

//
// MessageId: TRUST_E_FAIL
//
// MessageText:
//
//  Generic Trust Failure.
//
#define TRUST_E_FAIL                     0x800B010BL           [fail]

//
// MessageId: CERT_E_REVOKED
//
// MessageText:
//
//  A certificate was explicitly revoked by its issuer.
//
#define CERT_E_REVOKED                   0x800B010CL           [fail]

//
// MessageId: CERT_E_UNTRUSTEDTESTROOT
//
// MessageText:
//
//  The root certificate is a testing certificate and the policy settings disallow test certificates.
//
#define CERT_E_UNTRUSTEDTESTROOT         0x800B010DL           [fail]

//
// MessageId: CERT_E_REVOCATION_FAILURE
//
// MessageText:
//
//  The revocation process could not continue - the certificate(s) could not be checked.
//
#define CERT_E_REVOCATION_FAILURE        0x800B010EL           [fail]

//
// MessageId: CERT_E_CN_NO_MATCH
//
// MessageText:
//
//  The certificate's CN name does not match the passed value.
//
#define CERT_E_CN_NO_MATCH               0x800B010FL           [fail]

//
// MessageId: CERT_E_WRONG_USAGE
//
// MessageText:
//
//  The certificate is not valid for the requested usage.
//
#define CERT_E_WRONG_USAGE               0x800B0110L           [fail]

// *****************
// FACILITY_SETUPAPI
// *****************
//
//
// MessageId: SPAPI_E_EXPECTED_SECTION_NAME
//
// MessageText:
//
//  A non-empty line was encountered in the INF before the start of a section.
//
#define SPAPI_E_EXPECTED_SECTION_NAME    0x800F0000L           [fail]

//
// MessageId: SPAPI_E_BAD_SECTION_NAME_LINE
//
// MessageText:
//
//  A section name marker in the INF is not complete, or does not exist on a line by itself.
//
#define SPAPI_E_BAD_SECTION_NAME_LINE    0x800F0001L           [fail]

//
// MessageId: SPAPI_E_SECTION_NAME_TOO_LONG
//
// MessageText:
//
//  An INF section was encountered whose name exceeds the maximum section name length.
//
#define SPAPI_E_SECTION_NAME_TOO_LONG    0x800F0002L           [fail]

//
// MessageId: SPAPI_E_GENERAL_SYNTAX
//
// MessageText:
//
//  The syntax of the INF is invalid.
//
#define SPAPI_E_GENERAL_SYNTAX           0x800F0003L           [fail]

//
// MessageId: SPAPI_E_WRONG_INF_STYLE
//
// MessageText:
//
//  The style of the INF is different than what was requested.
//
#define SPAPI_E_WRONG_INF_STYLE          0x800F0100L           [fail]

//
// MessageId: SPAPI_E_SECTION_NOT_FOUND
//
// MessageText:
//
//  The required section was not found in the INF.
//
#define SPAPI_E_SECTION_NOT_FOUND        0x800F0101L           [fail]

//
// MessageId: SPAPI_E_LINE_NOT_FOUND
//
// MessageText:
//
//  The required line was not found in the INF.
//
#define SPAPI_E_LINE_NOT_FOUND           0x800F0102L           [fail]

//
// MessageId: SPAPI_E_NO_ASSOCIATED_CLASS
//
// MessageText:
//
//  The INF or the device information set or element does not have an associated install class.
//
#define SPAPI_E_NO_ASSOCIATED_CLASS      0x800F0200L           [fail]

//
// MessageId: SPAPI_E_CLASS_MISMATCH
//
// MessageText:
//
//  The INF or the device information set or element does not match the specified install class.
//
#define SPAPI_E_CLASS_MISMATCH           0x800F0201L           [fail]

//
// MessageId: SPAPI_E_DUPLICATE_FOUND
//
// MessageText:
//
//  An existing device was found that is a duplicate of the device being manually installed.
//
#define SPAPI_E_DUPLICATE_FOUND          0x800F0202L           [fail]

//
// MessageId: SPAPI_E_NO_DRIVER_SELECTED
//
// MessageText:
//
//  There is no driver selected for the device information set or element.
//
#define SPAPI_E_NO_DRIVER_SELECTED       0x800F0203L           [fail]

//
// MessageId: SPAPI_E_KEY_DOES_NOT_EXIST
//
// MessageText:
//
//  The requested device registry key does not exist.
//
#define SPAPI_E_KEY_DOES_NOT_EXIST       0x800F0204L           [fail]

//
// MessageId: SPAPI_E_INVALID_DEVINST_NAME
//
// MessageText:
//
//  The device instance name is invalid.
//
#define SPAPI_E_INVALID_DEVINST_NAME     0x800F0205L           [fail]

//
// MessageId: SPAPI_E_INVALID_CLASS
//
// MessageText:
//
//  The install class is not present or is invalid.
//
#define SPAPI_E_INVALID_CLASS            0x800F0206L           [fail]

//
// MessageId: SPAPI_E_DEVINST_ALREADY_EXISTS
//
// MessageText:
//
//  The device instance cannot be created because it already exists.
//
#define SPAPI_E_DEVINST_ALREADY_EXISTS   0x800F0207L           [fail]

//
// MessageId: SPAPI_E_DEVINFO_NOT_REGISTERED
//
// MessageText:
//
//  The operation cannot be performed on a device information element that has not been registered.
//
#define SPAPI_E_DEVINFO_NOT_REGISTERED   0x800F0208L           [fail]

//
// MessageId: SPAPI_E_INVALID_REG_PROPERTY
//
// MessageText:
//
//  The device property code is invalid.
//
#define SPAPI_E_INVALID_REG_PROPERTY     0x800F0209L           [fail]

//
// MessageId: SPAPI_E_NO_INF
//
// MessageText:
//
//  The INF from which a driver list is to be built does not exist.
//
#define SPAPI_E_NO_INF                   0x800F020AL           [fail]

//
// MessageId: SPAPI_E_NO_SUCH_DEVINST
//
// MessageText:
//
//  The device instance does not exist in the hardware tree.
//
#define SPAPI_E_NO_SUCH_DEVINST          0x800F020BL           [fail]

//
// MessageId: SPAPI_E_CANT_LOAD_CLASS_ICON
//
// MessageText:
//
//  The icon representing this install class cannot be loaded.
//
#define SPAPI_E_CANT_LOAD_CLASS_ICON     0x800F020CL           [fail]

//
// MessageId: SPAPI_E_INVALID_CLASS_INSTALLER
//
// MessageText:
//
//  The class installer registry entry is invalid.
//
#define SPAPI_E_INVALID_CLASS_INSTALLER  0x800F020DL           [fail]

//
// MessageId: SPAPI_E_DI_DO_DEFAULT
//
// MessageText:
//
//  The class installer has indicated that the default action should be performed for this installation request.
//
#define SPAPI_E_DI_DO_DEFAULT            0x800F020EL           [fail]

//
// MessageId: SPAPI_E_DI_NOFILECOPY
//
// MessageText:
//
//  The operation does not require any files to be copied.
//
#define SPAPI_E_DI_NOFILECOPY            0x800F020FL           [fail]

//
// MessageId: SPAPI_E_INVALID_HWPROFILE
//
// MessageText:
//
//  The specified hardware profile does not exist.
//
#define SPAPI_E_INVALID_HWPROFILE        0x800F0210L           [fail]

//
// MessageId: SPAPI_E_NO_DEVICE_SELECTED
//
// MessageText:
//
//  There is no device information element currently selected for this device information set.
//
#define SPAPI_E_NO_DEVICE_SELECTED       0x800F0211L           [fail]

//
// MessageId: SPAPI_E_DEVINFO_LIST_LOCKED
//
// MessageText:
//
//  The operation cannot be performed because the device information set is locked.
//
#define SPAPI_E_DEVINFO_LIST_LOCKED      0x800F0212L           [fail]

//
// MessageId: SPAPI_E_DEVINFO_DATA_LOCKED
//
// MessageText:
//
//  The operation cannot be performed because the device information element is locked.
//
#define SPAPI_E_DEVINFO_DATA_LOCKED      0x800F0213L           [fail]

//
// MessageId: SPAPI_E_DI_BAD_PATH
//
// MessageText:
//
//  The specified path does not contain any applicable device INFs.
//
#define SPAPI_E_DI_BAD_PATH              0x800F0214L           [fail]

//
// MessageId: SPAPI_E_NO_CLASSINSTALL_PARAMS
//
// MessageText:
//
//  No class installer parameters have been set for the device information set or element.
//
#define SPAPI_E_NO_CLASSINSTALL_PARAMS   0x800F0215L           [fail]

//
// MessageId: SPAPI_E_FILEQUEUE_LOCKED
//
// MessageText:
//
//  The operation cannot be performed because the file queue is locked.
//
#define SPAPI_E_FILEQUEUE_LOCKED         0x800F0216L           [fail]

//
// MessageId: SPAPI_E_BAD_SERVICE_INSTALLSECT
//
// MessageText:
//
//  A service installation section in this INF is invalid.
//
#define SPAPI_E_BAD_SERVICE_INSTALLSECT  0x800F0217L           [fail]

//
// MessageId: SPAPI_E_NO_CLASS_DRIVER_LIST
//
// MessageText:
//
//  There is no class driver list for the device information element.
//
#define SPAPI_E_NO_CLASS_DRIVER_LIST     0x800F0218L           [fail]

//
// MessageId: SPAPI_E_NO_ASSOCIATED_SERVICE
//
// MessageText:
//
//  The installation failed because a function driver was not specified for this device instance.
//
#define SPAPI_E_NO_ASSOCIATED_SERVICE    0x800F0219L           [fail]

//
// MessageId: SPAPI_E_NO_DEFAULT_DEVICE_INTERFACE
//
// MessageText:
//
//  There is presently no default device interface designated for this interface class.
//
#define SPAPI_E_NO_DEFAULT_DEVICE_INTERFACE 0x800F021AL           [fail]

//
// MessageId: SPAPI_E_DEVICE_INTERFACE_ACTIVE
//
// MessageText:
//
//  The operation cannot be performed because the device interface is currently active.
//
#define SPAPI_E_DEVICE_INTERFACE_ACTIVE  0x800F021BL           [fail]

//
// MessageId: SPAPI_E_DEVICE_INTERFACE_REMOVED
//
// MessageText:
//
//  The operation cannot be performed because the device interface has been removed from the system.
//
#define SPAPI_E_DEVICE_INTERFACE_REMOVED 0x800F021CL           [fail]

//
// MessageId: SPAPI_E_BAD_INTERFACE_INSTALLSECT
//
// MessageText:
//
//  An interface installation section in this INF is invalid.
//
#define SPAPI_E_BAD_INTERFACE_INSTALLSECT 0x800F021DL           [fail]

//
// MessageId: SPAPI_E_NO_SUCH_INTERFACE_CLASS
//
// MessageText:
//
//  This interface class does not exist in the system.
//
#define SPAPI_E_NO_SUCH_INTERFACE_CLASS  0x800F021EL           [fail]

//
// MessageId: SPAPI_E_INVALID_REFERENCE_STRING
//
// MessageText:
//
//  The reference string supplied for this interface device is invalid.
//
#define SPAPI_E_INVALID_REFERENCE_STRING 0x800F021FL           [fail]

//
// MessageId: SPAPI_E_INVALID_MACHINENAME
//
// MessageText:
//
//  The specified machine name does not conform to UNC naming conventions.
//
#define SPAPI_E_INVALID_MACHINENAME      0x800F0220L           [fail]

//
// MessageId: SPAPI_E_REMOTE_COMM_FAILURE
//
// MessageText:
//
//  A general remote communication error occurred.
//
#define SPAPI_E_REMOTE_COMM_FAILURE      0x800F0221L           [fail]

//
// MessageId: SPAPI_E_MACHINE_UNAVAILABLE
//
// MessageText:
//
//  The machine selected for remote communication is not available at this time.
//
#define SPAPI_E_MACHINE_UNAVAILABLE      0x800F0222L           [fail]

//
// MessageId: SPAPI_E_NO_CONFIGMGR_SERVICES
//
// MessageText:
//
//  The Plug and Play service is not available on the remote machine.
//
#define SPAPI_E_NO_CONFIGMGR_SERVICES    0x800F0223L           [fail]

//
// MessageId: SPAPI_E_INVALID_PROPPAGE_PROVIDER
//
// MessageText:
//
//  The property page provider registry entry is invalid.
//
#define SPAPI_E_INVALID_PROPPAGE_PROVIDER 0x800F0224L           [fail]

//
// MessageId: SPAPI_E_NO_SUCH_DEVICE_INTERFACE
//
// MessageText:
//
//  The requested device interface is not present in the system.
//
#define SPAPI_E_NO_SUCH_DEVICE_INTERFACE 0x800F0225L           [fail]

//
// MessageId: SPAPI_E_DI_POSTPROCESSING_REQUIRED
//
// MessageText:
//
//  The device's co-installer has additional work to perform after installation is complete.
//
#define SPAPI_E_DI_POSTPROCESSING_REQUIRED 0x800F0226L           [fail]

//
// MessageId: SPAPI_E_INVALID_COINSTALLER
//
// MessageText:
//
//  The device's co-installer is invalid.
//
#define SPAPI_E_INVALID_COINSTALLER      0x800F0227L           [fail]

//
// MessageId: SPAPI_E_NO_COMPAT_DRIVERS
//
// MessageText:
//
//  There are no compatible drivers for this device.
//
#define SPAPI_E_NO_COMPAT_DRIVERS        0x800F0228L           [fail]

//
// MessageId: SPAPI_E_NO_DEVICE_ICON
//
// MessageText:
//
//  There is no icon that represents this device or device type.
//
#define SPAPI_E_NO_DEVICE_ICON           0x800F0229L           [fail]

//
// MessageId: SPAPI_E_INVALID_INF_LOGCONFIG
//
// MessageText:
//
//  A logical configuration specified in this INF is invalid.
//
#define SPAPI_E_INVALID_INF_LOGCONFIG    0x800F022AL           [fail]

//
// MessageId: SPAPI_E_DI_DONT_INSTALL
//
// MessageText:
//
//  The class installer has denied the request to install or upgrade this device.
//
#define SPAPI_E_DI_DONT_INSTALL          0x800F022BL           [fail]

//
// MessageId: SPAPI_E_INVALID_FILTER_DRIVER
//
// MessageText:
//
//  One of the filter drivers installed for this device is invalid.
//
#define SPAPI_E_INVALID_FILTER_DRIVER    0x800F022CL           [fail]

//
// MessageId: SPAPI_E_ERROR_NOT_INSTALLED
//
// MessageText:
//
//  No installed components were detected.
//

#define SPAPI_E_ERROR_NOT_INSTALLED      0x800F1000L           [fail]


};
