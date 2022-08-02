#pragma once

//@InteropGen generate struct
struct LoggerArgs
{
	const char* str;
};

//@InteropGen generate class
class CLogger
{
public:
	CLogger();
	~CLogger();

	//
	// Logs a thing
	//
	void Log(const char* str);

	//
	// Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed sit amet venenatis quam. 
	// Curabitur erat dui, laoreet sit amet nulla ut, semper pulvinar erat. Fusce justo 
	// neque, tincidunt id nisi vitae, congue tincidunt lorem. Sed in cursus justo. 
	//
	int* InteropTest(const char* a, const char* b, int* c);

	//@InteropGen ignore
	static CLogger* GetActiveLogger();
};

extern "C" inline void __CLogger_Log(CLogger * logger, const char* str) { logger->Log(args); }
extern "C" inline static CLogger * __CLogger_GetActiveLogger(CLogger * instance) { return instance->GetActiveLogger(); }
extern "C" inline int* __CLogger_InteropTest(CLogger * logger, const char* a, const char* b, int* c) { return logger->InteropTest(a, b, c); }
extern "C" inline CLogger * __CLogger_Create() { return new CLogger(); };
extern "C" inline void __CLogger_Delete(CLogger * instance) { instance->~CLogger(); };

//@InteropGen generate class
class CInteropTest
{
public:
	CInteropTest(string_t string);
	~CInteropTest();

	//
	// Is this string 'hello'?
	//
	bool IsStringHello(string_t helloString);

	//
	// Get 'hello'
	//
	string_t GetHelloString();

	//
	// Get random number between 0 and 100
	//
	static int GetRandomNumber();

	//
	// Get current CInteropTest (singleton)
	//
	//@InteropGen ignore
	static CInteropTest* GetActiveInteropTest();
};