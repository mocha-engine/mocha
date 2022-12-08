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

struct LogEntry
{
	std::string time;
	std::string logger;
	std::string level;
	std::string message;
};

//@InteropGen generate class
class LogManager : ISubSystem
{
public:
	LogManager();

	void Startup();
	void Shutdown();

	void Info( std::string str );
	void Warning( std::string str );
	void Error( std::string str );
	void Trace( std::string str );

	std::vector<LogEntry> m_logHistory;
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
		return ts;
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

		// clang-format off
		LogEntry logEntry = {
			time,
			logger,
			level,
			message
		};
		// clang-format on

		g_logManager->m_logHistory.push_back( logEntry );
	}

	void flush_() override { std::cout << std::flush; }
};

using MochaSinkMT = MochaSink<std::mutex>;
using MochaSinkST = MochaSink<spdlog::details::null_mutex>;
