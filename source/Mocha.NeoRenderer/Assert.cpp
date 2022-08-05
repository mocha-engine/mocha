#include <vector>

#include <Windows.h>
#include "AssertDialog.h"
#include "Assert.h"

#define MAX_ASSERT_STRING_LENGTH 256

struct dialogInitInfo_t
{
	const char* message;
	const char* expression;
	const char* file;
	UINT32 line;
};

struct assertDisable_t
{
	const char* file;
	UINT32 line;
};

static std::vector<assertDisable_t> s_assertDisables;

static bool s_bIgnoreAllAsserts;

//-------------------------------------------------------------------------------------------------

static INT_PTR CALLBACK AssertDialogProc(HWND hDlg, UINT uMsg, WPARAM wParam, LPARAM lParam)
{
	static thread_local dialogInitInfo_t* s_pInfo;

	wchar_t wideMessage[MAX_ASSERT_STRING_LENGTH];
	wchar_t wideExpression[MAX_ASSERT_STRING_LENGTH];
	wchar_t wideFile[MAX_PATH];

	switch (uMsg)
	{
	case WM_INITDIALOG:
	{
		s_pInfo = reinterpret_cast<dialogInitInfo_t*>(lParam);

		SetDlgItemTextW(hDlg, IDC_MESSAGE, wideMessage);
		SetDlgItemTextW(hDlg, IDC_EXPRESSION, wideExpression);
		SetDlgItemTextW(hDlg, IDC_FILE, wideFile);
		SetDlgItemInt(hDlg, IDC_LINE, s_pInfo->line, false);

		// Centre the dialog on screen
		RECT rcDlg, rcDesktop;
		GetWindowRect(hDlg, &rcDlg);
		GetWindowRect(GetDesktopWindow(), &rcDesktop);
		SetWindowPos(
			hDlg,
			HWND_TOP,
			((rcDesktop.right - rcDesktop.left) - (rcDlg.right - rcDlg.left)) / 2,
			((rcDesktop.bottom - rcDesktop.top) - (rcDlg.bottom - rcDlg.top)) / 2,
			0,
			0,
			SWP_NOSIZE);

		return TRUE;
	}

	case WM_COMMAND:
	{
		switch (LOWORD(wParam))
		{
		case IDC_IGNORE_ONCE:
		{
			EndDialog(hDlg, FALSE);
			return TRUE;
		}

		case IDC_IGNORE_ALWAYS:
		{
			s_assertDisables.push_back({ s_pInfo->file, s_pInfo->line });

			EndDialog(hDlg, FALSE);
			return TRUE;
		}

		case IDC_IGNORE_ALL:
		{
			s_bIgnoreAllAsserts = true;

			EndDialog(hDlg, FALSE);
			return TRUE;
		}

		case IDC_BREAK:
		{
			EndDialog(hDlg, TRUE);
			return TRUE;
		}
		}

	case WM_KEYDOWN:
	{
		if (wParam == 2) // This is escape for some reason
		{
			// Ignore once
			EndDialog(hDlg, FALSE);
			return TRUE;
		}
	}

	return FALSE;
	}

	case WM_CLOSE:
	{
		// Ignore once
		EndDialog(hDlg, FALSE);
		return TRUE;
	}
	}

	return FALSE;
}

void AssertionFailed(const char* message, const char* expression, const char* file, UINT32 line)
{
	// Trim the filename down to "code", this obviously only works if the code is in a folder called "code"
	const char* trimmedFile = strstr(file, "code");
	if (trimmedFile)
	{
		trimmedFile += 5; // length of "code\\"
	}
	else
	{
		trimmedFile = file;
	}

	// Are we ignoring all asserts?
	if (s_bIgnoreAllAsserts)
	{
		return;
	}

	// Are we ignoring *this* assert?
	for (const assertDisable_t& disable : s_assertDisables)
	{
		if (disable.file == trimmedFile && disable.line == line)
		{
			return;
		}
	}

	dialogInitInfo_t info;
	info.message = message ? message : "N/A";
	info.expression = expression;
	info.file = trimmedFile;
	info.line = line;

	HINSTANCE instance = GetModuleHandleW(nullptr);

	bool shouldBreak = static_cast<bool>(DialogBoxParamW(instance, MAKEINTRESOURCEW(IDD_ASSERT_DIALOG), nullptr, AssertDialogProc, reinterpret_cast<LPARAM>(&info)));

	if (shouldBreak && IsDebuggerPresent())
	{
		__debugbreak();
	}
}

//
// Override the CRT's assertion handler with our own
//
void _wassert(_In_z_ wchar_t const* message, _In_z_ wchar_t const* file, _In_ unsigned line)
{
	char narrowMessage[MAX_ASSERT_STRING_LENGTH];
	char narrowFile[MAX_ASSERT_STRING_LENGTH];

	sprintf_s(narrowMessage, MAX_ASSERT_STRING_LENGTH, "%ws", message);
	sprintf_s(narrowFile, MAX_ASSERT_STRING_LENGTH, "%ws", file);

	AssertionFailed(nullptr, narrowMessage, narrowFile, line);
}