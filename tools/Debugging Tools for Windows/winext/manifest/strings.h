// +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
//
//                String manipulation Functions
//
// +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
module KERNEL32.DLL:
category StringManipulation:

int CompareStringA(
  LCID Locale,       // locale identifier
  DWORD dwCmpFlags,  // comparison-style options
  LPCSTR lpString1, // first string
  int cchCount1,     // size of first string
  LPCSTR lpString2, // second string
  int cchCount2      // size of second string
);

int CompareStringW(
  LCID Locale,       // locale identifier
  DWORD dwCmpFlags,  // comparison-style options
  LPCWSTR lpString1, // first string
  int cchCount1,     // size of first string
  LPCWSTR lpString2, // second string
  int cchCount2      // size of second string
);

LPSTR lstrcat(
  LPSTR lpString1,  // first string
  LPCSTR lpString2  // second string
);

LPSTR lstrcatA(
  LPSTR lpString1,  // first string
  LPCSTR lpString2  // second string
);

LPWSTR lstrcatW(
  LPWSTR lpString1,  // first string
  LPCWSTR lpString2  // second string
);

int lstrcmp(
  LPCSTR lpString1,  // first string
  LPCSTR lpString2   // second string
);

int lstrcmpA(
  LPCSTR lpString1,  // first string
  LPCSTR lpString2   // second string
);

int lstrcmpW(
  LPCWSTR lpString1,  // first string
  LPCWSTR lpString2   // second string
);

int lstrcmpi(
  LPCSTR lpString1,  // first string
  LPCSTR lpString2   // second string
);

int lstrcmpiA(
  LPCSTR lpString1,  // first string
  LPCSTR lpString2   // second string
);

int lstrcmpiW(
  LPCWSTR lpString1,  // first string
  LPCWSTR lpString2   // second string
);

LPSTR lstrcpy(
  LPSTR lpString1,  // destination buffer
  LPCSTR lpString2  // string
);

LPSTR lstrcpyA(
  LPSTR lpString1,  // destination buffer
  LPCSTR lpString2  // string
);

LPWSTR lstrcpyW(
  LPWSTR lpString1,  // destination buffer
  LPCWSTR lpString2  // string
);

LPSTR lstrcpyn(
  [out] LPSTR lpString1,  // destination buffer
  LPCSTR lpString2, // string
  int iMaxLength     // number of characters to copy
);

LPSTR lstrcpynA(
  LPSTR lpString1,  // destination buffer
  LPCSTR lpString2, // string
  int iMaxLength     // number of characters to copy
);

LPWSTR lstrcpynW(
  LPWSTR lpString1,  // destination buffer
  LPCWSTR lpString2, // string
  int iMaxLength     // number of characters to copy
);

int lstrlen(
    LPCSTR lpszString
    );

int lstrlenA(
    LPCSTR lpszString
    );

int lstrlenW(
    LPCWSTR lpszString
    );
