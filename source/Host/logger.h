#pragma once
#include <string>

//@InteropGen generate class
class CLogger
{
public:
	CLogger();

	void Info( std::string str );
	void Warning( std::string str );
	void Error( std::string str );
	void Trace( std::string str );
};