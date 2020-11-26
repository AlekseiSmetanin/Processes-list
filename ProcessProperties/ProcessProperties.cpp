// Свойства процессов DLL.cpp : Определяет экспортированные функции для приложения DLL.
//

#include "pch.h"

//#include "stdafx.h"

#include <cstdio>
#include <iostream>
#include <windows.h>
#include <malloc.h>
#include <stdio.h>

#include <windows.h>
#include <tlhelp32.h>
#include <stdlib.h>

using namespace std;

extern "C" __declspec(dllexport) int GetProcessIntegrityLevel(DWORD pId)
{
	HANDLE hToken = NULL;
	DWORD cbTokenIL = 0;
	PTOKEN_MANDATORY_LABEL pTokenIL = NULL;

	if (pId == NULL)
	{
		return -1;
	}

	HANDLE proc = OpenProcess(PROCESS_ALL_ACCESS, false, pId);

	if (!OpenProcessToken(GetCurrentProcess(), TOKEN_QUERY, &hToken))
	{
		return -2;
	}

	OpenProcessToken(proc, TOKEN_QUERY, &hToken);
	if (!GetTokenInformation(hToken, TokenIntegrityLevel, NULL, 0, &cbTokenIL))
	{
		if (ERROR_INSUFFICIENT_BUFFER != GetLastError())
		{
			return -3;
		}
	}

	pTokenIL = (TOKEN_MANDATORY_LABEL*)LocalAlloc(LPTR, cbTokenIL);
	if (pTokenIL == NULL)
	{
		return -4;
	}

	GetTokenInformation(hToken, TokenIntegrityLevel, NULL, 0, &cbTokenIL);
	if (!GetTokenInformation(hToken, TokenIntegrityLevel, pTokenIL, cbTokenIL, &cbTokenIL))
	{
		return -5;
	}

	DWORD dwIntegrityLevel;
	GetTokenInformation(hToken, TokenIntegrityLevel, pTokenIL, cbTokenIL, &cbTokenIL);
	dwIntegrityLevel = *GetSidSubAuthority(pTokenIL->Label.Sid,
		(DWORD)(UCHAR)(*GetSidSubAuthorityCount(pTokenIL->Label.Sid) - 1));
	if (dwIntegrityLevel == SECURITY_MANDATORY_LOW_RID)
	{
		// Low Integrity
		CloseHandle(proc);

		return 1;
	}
	else if (dwIntegrityLevel >= SECURITY_MANDATORY_MEDIUM_RID &&
		dwIntegrityLevel < SECURITY_MANDATORY_HIGH_RID)
	{
		// Medium Integrity
		CloseHandle(proc);

		return 2;
	}
	else if (dwIntegrityLevel >= SECURITY_MANDATORY_HIGH_RID)
	{
		// High Integrity
		CloseHandle(proc);

		return 3;
	}
	else if (dwIntegrityLevel >= SECURITY_MANDATORY_SYSTEM_RID)
	{
		// System Integrity
		CloseHandle(proc);

		return 4;
	}
	else
	{
		CloseHandle(proc);

		return -6;
	}

	CloseHandle(proc);
}

extern "C"  __declspec(dllexport) int GetParentPID(int pid)
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

extern "C"  __declspec(dllexport) int IsDEP_Using(int pid)
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

extern "C"  __declspec(dllexport) int IsASLR_Using(int pid)
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
