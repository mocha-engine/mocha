#pragma once
#include <string>
#include <spdlog/spdlog.h>

class ClassTest
{
public:
	ClassTest( int poopCount );

	void Info( std::string str );
	void Warning( std::string str );
	void Error( std::string str );
	void Trace( std::string str );

	int m_FieldTest;
	uint64_t m_LongTest;

	inline void PrintPoo() { spdlog::info( "Poo" ); }
};

struct StructTest
{
	int m_StructureField;

	void StructureMethod( int hairyBalls );
};