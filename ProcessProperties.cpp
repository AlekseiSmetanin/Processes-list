// Свойства процессов DLL.cpp : Определяет экспортированные функции для приложения DLL.
//

#include "stdafx.h"

#include <cstdio>
#include <iostream>
#include <windows.h>
#include <malloc.h>
#include <stdio.h>

#include <windows.h>
#include <tlhelp32.h>
#include <stdlib.h>

using namespace std;

extern "C" __declspec(dllexport) void GetProcessIntegrityLevel(DWORD pId, char* integrity_level, int length)
{
	HANDLE hToken = NULL;
	DWORD cbTokenIL = 0;
	PTOKEN_MANDATORY_LABEL pTokenIL = NULL;

	if (pId == NULL)
	{
		printf("Low Process1");
		//return ERROR_INVALID_PARAMETER; 
	}

	HANDLE proc = OpenProcess(PROCESS_ALL_ACCESS, false, pId);

	if (!OpenProcessToken(GetCurrentProcess(), TOKEN_QUERY, &hToken))
	{
		printf("Low Process2");
		//return GetLastError();
	}

	OpenProcessToken(proc, TOKEN_QUERY, &hToken);
	if (!GetTokenInformation(hToken, TokenIntegrityLevel, NULL, 0, &cbTokenIL))
	{
		if (ERROR_INSUFFICIENT_BUFFER != GetLastError())
		{
			length = GetLastError();
			printf("Low Process3");
			return;
		}
	}

	pTokenIL = (TOKEN_MANDATORY_LABEL*)LocalAlloc(LPTR, cbTokenIL);
	if (pTokenIL == NULL)
	{
		length = GetLastError();
		printf("Low Process4");
		return;
	}

	GetTokenInformation(hToken, TokenIntegrityLevel, NULL, 0, &cbTokenIL);
	if (!GetTokenInformation(hToken, TokenIntegrityLevel, pTokenIL, cbTokenIL, &cbTokenIL))
	{
		length = GetLastError();
		printf("Low Process6");
		return;
	}
	unsigned char int_lev;
	DWORD dwIntegrityLevel;
	GetTokenInformation(hToken, TokenIntegrityLevel, pTokenIL, cbTokenIL, &cbTokenIL);
	dwIntegrityLevel = *GetSidSubAuthority(pTokenIL->Label.Sid,
		(DWORD)(UCHAR)(*GetSidSubAuthorityCount(pTokenIL->Label.Sid) - 1));
	if (dwIntegrityLevel == SECURITY_MANDATORY_LOW_RID)
	{
		// Low Integrity
		integrity_level[0] = 'l';
		integrity_level[1] = 'o';
		integrity_level[2] = 'w';
		integrity_level[3] = '\0';
	}
	else if (dwIntegrityLevel >= SECURITY_MANDATORY_MEDIUM_RID &&
		dwIntegrityLevel < SECURITY_MANDATORY_HIGH_RID)
	{
		// Medium Integrity
		integrity_level[0] = 'm';
		integrity_level[1] = 'e';
		integrity_level[2] = 'd';
		integrity_level[3] = 'i';
		integrity_level[4] = 'u';
		integrity_level[5] = 'm';
		integrity_level[6] = '\0';
	}
	else if (dwIntegrityLevel >= SECURITY_MANDATORY_HIGH_RID)
	{
		// High Integrity
		integrity_level[0] = 'h';
		integrity_level[1] = 'i';
		integrity_level[2] = 'g';
		integrity_level[3] = 'h';
		integrity_level[6] = '\0';
	}
	else if (dwIntegrityLevel >= SECURITY_MANDATORY_SYSTEM_RID)
	{
		// System Integrity
		integrity_level[0] = 's';
		integrity_level[1] = 'y';
		integrity_level[2] = 's';
		integrity_level[3] = 't';
		integrity_level[4] = 'е';
		integrity_level[5] = 'm';
		integrity_level[6] = '\0';
	}
	CloseHandle(proc);
}

typedef BOOL(WINAPI* LPFN_ISWOW64PROCESS) (HANDLE, PBOOL);
LPFN_ISWOW64PROCESS fnIsWow64Process;
int IsWow64(HANDLE hProcess);

int __declspec(dllexport) GetPPIDbyDLL(int pid)
{
	HANDLE hProcessSnapshot;
	PROCESSENTRY32 pe32;
	int ppid = -1;

	hProcessSnapshot = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);
	if (hProcessSnapshot == INVALID_HANDLE_VALUE)
	{
		return -2;
	}

	pe32.dwSize = sizeof(PROCESSENTRY32);

	if (!Process32First(hProcessSnapshot, &pe32))
	{
		CloseHandle(hProcessSnapshot);
		return -3;
	}

	do
	{
		if (pe32.th32ProcessID == pid)
		{
			ppid = pe32.th32ParentProcessID;
			break;
		}
	} while (Process32Next(hProcessSnapshot, &pe32));

	CloseHandle(hProcessSnapshot);
	return ppid;
}

int __declspec(dllexport) Is32bitProcessbyDLL(int pid)
{
	HANDLE hProcessSnapshot;
	HANDLE hProcess;
	PROCESSENTRY32 pe32;
	DWORD dwPriorityClass;
	int is32bit = -1;

	hProcessSnapshot = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);
	if (hProcessSnapshot == INVALID_HANDLE_VALUE)
	{
		return -2;
	}

	pe32.dwSize = sizeof(PROCESSENTRY32);

	if (!Process32First(hProcessSnapshot, &pe32))
	{
		CloseHandle(hProcessSnapshot);//освободим память, выделенную под снимок процессов
		return -3;
	}

	do
	{
		if (pe32.th32ProcessID == pid)
		{
			dwPriorityClass = 0;
			hProcess = OpenProcess(PROCESS_ALL_ACCESS, FALSE, pe32.th32ProcessID);

			is32bit = IsWow64(hProcess);
			break;
		}
	} while (Process32Next(hProcessSnapshot, &pe32));

	CloseHandle(hProcessSnapshot);
	return is32bit;
}

int IsWow64(HANDLE hProcess)
{
	int bIsWow64 = -1;

	fnIsWow64Process = (LPFN_ISWOW64PROCESS)GetProcAddress(GetModuleHandle(TEXT("kernel32")), "IsWow64Process");

	if (NULL != fnIsWow64Process)
	{
		if (!fnIsWow64Process(hProcess, &bIsWow64))
		{
		}
	}
	return bIsWow64;
}

int __declspec(dllexport) IsASLR_Using(int pid)
{
	HANDLE hProc = OpenProcess(PROCESS_ALL_ACCESS, FALSE, pid);

	PROCESS_MITIGATION_ASLR_POLICY ASLR;

	if (GetProcessMitigationPolicy(hProc, ProcessASLRPolicy, &ASLR, sizeof(ASLR)))
	{
		CloseHandle(hProc);
		if (ASLR.EnableBottomUpRandomization || ASLR.EnableForceRelocateImages || ASLR.EnableHighEntropy)
			return 1;
		else
			return 0;
	}
	else
	{
		CloseHandle(hProc);

		return -1;
	}
}

int __declspec(dllexport) IsDEP_Using(int pid)
{
	HANDLE hProc = OpenProcess(PROCESS_ALL_ACCESS, FALSE, pid);

	PROCESS_MITIGATION_DEP_POLICY dep;

	int res = GetProcessMitigationPolicy(hProc, ProcessDEPPolicy, &dep, sizeof(dep));
	if (res)
	{
		if (dep.Enable)
			return 1;
		else
			return 0;
	}
	else
	{
		return res;
	}
}