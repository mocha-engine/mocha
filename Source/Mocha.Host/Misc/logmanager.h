#pragma once
#include <Misc/defs.h>
#include <Misc/globalvars.h>
#include <Misc/subsystem.h>
#include <Root/clientroot.h>
#include <atomic>
#include <iostream>
#include <mutex>
#include <spdlog/details/null_mutex.h>
#include <spdlog/sinks/base_sink.h>
#include <spdlog/sinks/stdout_color_sinks.h>
#include <spdlog/spdlog.h>
#include <string>

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

inline std::atomic<bool> IsInitialized = false;

template <typename Mutex>
class MochaSink;

using MochaSinkMT = MochaSink<std::mutex>;
using MochaSinkST = MochaSink<spdlog::details::null_mutex>;

class LogManager : ISubSystem
{
private:
	std::shared_ptr<MochaSinkMT> m_sink;

	inline std::shared_ptr<spdlog::logger> GetLogger( std::string loggerName )
	{
		std::shared_ptr<spdlog::logger> existingLogger = spdlog::get( loggerName );

		if ( !existingLogger )
		{
			auto logger = std::make_shared<spdlog::logger>( loggerName, m_sink );
			spdlog::register_logger( logger );

			return logger;
		}

		return existingLogger;
	}

public:
	void Startup();
	void Shutdown(){};

	std::vector<LogEntryInterop> m_logHistory;

	GENERATE_BINDINGS void ManagedInfo( std::string loggerName, std::string str );
	GENERATE_BINDINGS void ManagedWarning( std::string loggerName, std::string str );
	GENERATE_BINDINGS void ManagedError( std::string loggerName, std::string str );
	GENERATE_BINDINGS void ManagedTrace( std::string loggerName, std::string str );

	GENERATE_BINDINGS inline LogHistory GetLogHistory()
	{
		LogHistory logHistory = {};
		logHistory.count = static_cast<int>( Globals::m_logManager->m_logHistory.size() );
		logHistory.items = Globals::m_logManager->m_logHistory.data();

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

		if ( Globals::m_isDedicatedServer )
		{
			// Servers use the console
			std::cout << fmt::to_string( formatted );
		}
		else
		{
			// In client, use visual studio's output window
			OutputDebugStringA( fmt::to_string( formatted ).c_str() );
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

		Globals::m_logManager->m_logHistory.emplace_back( logEntry );

		// If we have more than 128 messages in the log history, start getting rid
		if ( Globals::m_logManager->m_logHistory.size() > MAX_LOG_MESSAGES )
		{
			Globals::m_logManager->m_logHistory.erase( Globals::m_logManager->m_logHistory.begin() );
		}
	}

	void flush_() override { std::cout << std::flush; }
};
