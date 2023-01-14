#include <atomic>
#include <chrono>
#include <defs.h>
#include <filesystem>
#include <functional>
#include <mutex>
#include <set>
#include <spdlog/spdlog.h>
#include <thread>

#ifndef _WIN32
#pragma error "FileSystemWatcher is only supported on Windows"
#else

class FileSystemWatcher
{
public:
	FileSystemWatcher() {}

	~FileSystemWatcher()
	{
		m_isThreadExiting.store( true );
		m_watcherThread.join();
	}

	void WatchDirectory( std::string path )
	{
		m_path = std::filesystem::absolute( path ).string();

		m_isThreadExiting.store( false );
		m_watcherThread = std::thread{ &FileSystemWatcher::DirectoryWatcherThread, this };
	}

	void WatchFile( std::string path )
	{
		m_path = std::filesystem::absolute( path ).string();

		m_isThreadExiting.store( false );
		m_watcherThread = std::thread{ &FileSystemWatcher::FileWatcherThread, this };
	}

	void RegisterModifiedCallback( std::function<void( std::string path )> callback ) { m_fileModifiedCallback = callback; }
	void RegisterCreatedCallback( std::function<void( std::string path )> callback ) { m_fileCreatedCallback = callback; }
	void RegisterDeletedCallback( std::function<void( std::string path )> callback ) { m_fileDeletedCallback = callback; }

private:
	void FileWatcherThread();
	void DirectoryWatcherThread();

	std::atomic<bool> m_isThreadExiting;
	std::thread m_watcherThread;
	std::string m_path;

	std::function<void( std::string path )> m_fileModifiedCallback;
	std::function<void( std::string path )> m_fileCreatedCallback;
	std::function<void( std::string path )> m_fileDeletedCallback;
};

#endif