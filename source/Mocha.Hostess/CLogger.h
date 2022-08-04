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
	void Info(const char* str);
	void Warning(const char* str);
	void Error(const char* str);
	void Trace(const char* str);
};