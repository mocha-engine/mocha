#include "FileSystemWatcher.h"

void FileSystemWatcher::DirectoryWatcherThread()
{
	HANDLE hDirectory =
	    CreateFileA( m_path.c_str(), FILE_LIST_DIRECTORY, FILE_SHARE_READ | FILE_SHARE_WRITE | FILE_SHARE_DELETE, NULL,
	        OPEN_EXISTING, FILE_FLAG_BACKUP_SEMANTICS | FILE_FLAG_OVERLAPPED, NULL );

	if ( hDirectory == INVALID_HANDLE_VALUE )
	{
		spdlog::error( "Failed to open directory {}", m_path );
		ErrorMessage( "Failed to open FileSystemWatcher directory" );
		return;
	}

	std::vector<BYTE> buffer( 4096 );
	OVERLAPPED overlapped = { 0 };
	HANDLE hEvent = CreateEvent( NULL, TRUE, FALSE, NULL );
	overlapped.hEvent = hEvent;

	std::set<std::string> processedFiles;
	while ( !m_isThreadExiting.load( std::memory_order_relaxed ) )
	{
		std::this_thread::sleep_for( std::chrono::milliseconds( 500 ) );

		DWORD bytesReturned = 0;
		BOOL success = ReadDirectoryChangesW( hDirectory, buffer.data(), static_cast<DWORD>( buffer.size() ), TRUE,
		    FILE_NOTIFY_CHANGE_FILE_NAME | FILE_NOTIFY_CHANGE_LAST_WRITE, &bytesReturned, &overlapped, NULL );

		if ( !success )
		{
			DWORD errorCode = GetLastError();
			spdlog::error( "Failed to read directory changes {}", errorCode );
			break;
		}

		DWORD waitStatus = WaitForSingleObject( hEvent, INFINITE );

		if ( waitStatus == WAIT_OBJECT_0 )
		{
			DWORD bytesRetrieved = 0;
			DWORD offset = 0;

			if ( !GetOverlappedResult( hDirectory, &overlapped, &bytesRetrieved, TRUE ) )
			{
				DWORD errorCode = GetLastError();
				if ( errorCode != ERROR_IO_PENDING )
				{
					spdlog::error( "Failed to read directory changes {}", errorCode );
					break;
				}
			}

			processedFiles.clear();

			while ( offset < bytesRetrieved )
			{
				FILE_NOTIFY_INFORMATION* notifyInfo = reinterpret_cast<FILE_NOTIFY_INFORMATION*>( buffer.data() + offset );
				std::wstring fileName( notifyInfo->FileName, notifyInfo->FileNameLength / sizeof( WCHAR ) );
				std::string fileName_str( fileName.begin(), fileName.end() );

				if ( notifyInfo->Action == FILE_ACTION_ADDED )
				{
					if ( m_fileCreatedCallback != nullptr )
					{
						m_fileCreatedCallback( fileName_str );
					}
				}
				else if ( notifyInfo->Action == FILE_ACTION_REMOVED )
				{
					if ( m_fileDeletedCallback != nullptr )
					{
						m_fileDeletedCallback( fileName_str );
					}
				}
				else if ( notifyInfo->Action == FILE_ACTION_MODIFIED )
				{
					if ( processedFiles.count( fileName_str ) == 0 )
					{
						processedFiles.insert( fileName_str );

						if ( m_fileModifiedCallback != nullptr )
						{
							m_fileModifiedCallback( fileName_str );
						}
					}
				}

				offset += notifyInfo->NextEntryOffset;

				if ( notifyInfo->NextEntryOffset == 0 )
				{
					break; // Reached the end
				}
			}
		}
		else if ( waitStatus == WAIT_ABANDONED )
		{
			spdlog::error( "Wait abandoned" );
			break;
		}
		else
		{
			spdlog::error( "Wait failed" );
			break;
		}
	}

	CloseHandle( hDirectory );
	CloseHandle( hEvent );
}

void FileSystemWatcher::FileWatcherThread()
{
	auto lastWriteTime = std::filesystem::last_write_time( m_path );

	while ( !m_isThreadExiting.load( std::memory_order_relaxed ) )
	{
		std::this_thread::sleep_for( std::chrono::milliseconds( 500 ) );

		const auto currentWriteTime = std::filesystem::last_write_time( m_path );
		if ( currentWriteTime != lastWriteTime )
		{
			lastWriteTime = currentWriteTime;

			if ( m_fileModifiedCallback != nullptr )
			{
				m_fileModifiedCallback( m_path );
			}
		}
	}
}