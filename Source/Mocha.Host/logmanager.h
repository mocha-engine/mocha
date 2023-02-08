#pragma once
#include <defs.h>
#include <globalvars.h>
#include <iostream>
#include <mutex>
#include <spdlog/details/null_mutex.h>
#include <spdlog/sinks/base_sink.h>
#include <spdlog/sinks/stdout_color_sinks.h>
#include <spdlog/spdlog.h>
#include <string>
#include <subsystem.h>
#include <clientroot.h>

#define MAX_LOG_MESSAGES 50

struct LogEntryInterop
{
	char* time;
	char* logger;
	char* level;
	char* message;
};

struct LogHistory
{
	int count;
	LogEntryInterop* items;
};

class LogManager : ISubSystem
{
public:
	LogManager( Root* parent )
	    : ISubSystem( parent )
	{
	}
	
	void Startup();
	void Shutdown(){};

	std::vector<LogEntryInterop> m_logHistory;

	GENERATE_BINDINGS static void ManagedInfo( std::string str );
	GENERATE_BINDINGS static void ManagedWarning( std::string str );
	GENERATE_BINDINGS static void ManagedError( std::string str );
	GENERATE_BINDINGS static void ManagedTrace( std::string str );

	GENERATE_BINDINGS inline static LogHistory GetLogHistory()
	{
		auto root = ClientRoot::GetInstance();

		LogHistory logHistory = {};
		logHistory.count = static_cast<int>( root.m_logManager->m_logHistory.size() );
		logHistory.items = root.m_logManager->m_logHistory.data();

		return logHistory;
	}
};

template <typename Mutex>
class MochaSink : public spdlog::sinks::base_sink<Mutex>
{
protected:
	static std::string TimePointToString( const std::chrono::system_clock::time_point& tp )
	{
		const std::time_t t = std::chrono::system_clock::to_time_t( tp );

		std::tm tm;
		localtime_s( &tm, &t );

		const int MAX_SIZE = 32;
		char* s = ( char* )malloc( MAX_SIZE );
		strftime( s, MAX_SIZE, "[%H:%M:%S]", &tm );

		std::string ts = std::string( s );
		free( s );

		return ts;
	}

	inline static void CopyString( char** dest, std::string source )
	{
		size_t destSize = source.size() + 1;
		*dest = new char[destSize];
		strcpy_s( *dest, destSize, source.c_str() );
	}

	void sink_it_( const spdlog::details::log_msg& msg ) override
	{
		spdlog::memory_buf_t formatted;
		spdlog::sinks::base_sink<Mutex>::formatter_->format( msg, formatted );
		auto root = ClientRoot::GetInstance();

		if ( IS_CLIENT )
		{
			// In client, use visual studio's output window
			OutputDebugStringA( fmt::to_string( formatted ).c_str() );
		}
		else
		{
			// Servers use the console
			std::cout << fmt::to_string( formatted );
		}

		// Format everything to std::string
		std::string time = TimePointToString( msg.time );
		std::string logger = fmt::format( "{}", msg.logger_name.begin() );
		std::string level = fmt::format( "{}", msg.level );
		std::string message = fmt::format( "{}", msg.payload.begin() );

		LogEntryInterop logEntry = {};
		CopyString( &logEntry.time, time );
		CopyString( &logEntry.logger, logger );
		CopyString( &logEntry.level, level );
		CopyString( &logEntry.message, message );

		root.m_logManager->m_logHistory.emplace_back( logEntry );

		// If we have more than 128 messages in the log history, start getting rid
		if ( root.m_logManager->m_logHistory.size() > MAX_LOG_MESSAGES )
		{
			root.m_logManager->m_logHistory.erase( root.m_logManager->m_logHistory.begin() );
		}
	}

	void flush_() override { std::cout << std::flush; }
};

using MochaSinkMT = MochaSink<std::mutex>;
using MochaSinkST = MochaSink<spdlog::details::null_mutex>;
