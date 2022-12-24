#pragma once
#include <globalvars.h>
#include <iostream>
#include <mutex>
#include <spdlog/details/null_mutex.h>
#include <spdlog/sinks/base_sink.h>
#include <spdlog/sinks/stdout_color_sinks.h>
#include <spdlog/spdlog.h>
#include <string>
#include <subsystem.h>

#define MAX_LOG_MESSAGES 128

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

//@InteropGen generate class
class LogManager : ISubSystem
{
public:
	LogManager();

	void Startup();
	void Shutdown();

	static void ManagedInfo( std::string str );
	static void ManagedWarning( std::string str );
	static void ManagedError( std::string str );
	static void ManagedTrace( std::string str );

	std::vector<LogEntryInterop> m_logHistory;

	inline static LogHistory GetLogHistory()
	{
		LogHistory logHistory = {};
		logHistory.count = g_logManager->m_logHistory.size();
		logHistory.items = g_logManager->m_logHistory.data();

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
		strftime( s, MAX_SIZE, "[%H:%M]", &tm );

		std::string ts = std::string( s );
		free( s );

		return ts;
	}

	//@InteropGen ignore
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
		std::cout << fmt::to_string( formatted );

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

		g_logManager->m_logHistory.emplace_back( logEntry );

		// If we have more than 128 messages in the log history, start getting rid
		if ( g_logManager->m_logHistory.size() > MAX_LOG_MESSAGES )
		{
			g_logManager->m_logHistory.erase( g_logManager->m_logHistory.begin() );
		}
	}

	void flush_() override { std::cout << std::flush; }
};

using MochaSinkMT = MochaSink<std::mutex>;
using MochaSinkST = MochaSink<spdlog::details::null_mutex>;
