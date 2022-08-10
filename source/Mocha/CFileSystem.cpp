#include "CFileSystem.h"

#include <spdlog/spdlog.h>

CFileSystem::CFileSystem( std::string baseDirectory )
{
	m_baseDirectory = baseDirectory;
}

CFileSystem::~CFileSystem() {}

std::string CFileSystem::GetFullPath( std::string fileOrDirectoryPath )
{
	return ( m_baseDirectory / fileOrDirectoryPath ).string();
}

bool CFileSystem::DirectoryExists( std::string dirPath )
{
	std::string fullPath = GetFullPath( dirPath );
	return std::filesystem::exists( fullPath ) && IsDirectory( dirPath );
}

bool CFileSystem::FileExists( std::string filePath )
{
	std::string fullPath = GetFullPath( filePath );
	return std::filesystem::exists( fullPath ) && IsFile( filePath );
}

bool CFileSystem::IsDirectory( std::string dirPath )
{
	return std::filesystem::is_directory( GetFullPath( dirPath ) );
}

bool CFileSystem::IsFile( std::string filePath )
{
	return std::filesystem::is_regular_file( GetFullPath( filePath ) );
}

std::string* CFileSystem::SelectDirectoryEntries(
    std::string dirPath, int* outSize, std::function<bool( std::filesystem::directory_entry )> filter )
{
	std::vector<std::string> files;

	for ( auto& p : std::filesystem::directory_iterator( GetFullPath( dirPath ) ) )
	{
		if ( filter( p ) )
			files.push_back( p.path().filename().string() );
	}

	std::string* fileNames = new std::string[files.size()];

	for ( int i = 0; i < files.size(); i++ )
	{
		fileNames[i] = files[i].c_str();
	}

	*outSize = files.size();
	return fileNames;
}

std::string* CFileSystem::GetFiles( std::string dirPath, int* outSize )
{
	return SelectDirectoryEntries( dirPath, outSize, []( std::filesystem::directory_entry p ) { return p.is_regular_file(); } );
}

std::string* CFileSystem::GetDirectories( std::string dirPath, int* outSize )
{
	return SelectDirectoryEntries( dirPath, outSize, []( std::filesystem::directory_entry p ) { return p.is_directory(); } );
}

std::string CFileSystem::ReadAllText( std::string filePath )
{
	if ( !FileExists( filePath ) )
	{
		spdlog::error( "File not found" );
		throw new std::runtime_error( "File not found" );
	}

	std::ifstream file( GetFullPath( filePath ) );
	std::stringstream buffer;
	buffer << file.rdbuf();
	return buffer.str();
}

char* CFileSystem::ReadAllBytes( std::string filePath, int* outSize )
{
	if ( !FileExists( filePath ) )
	{
		spdlog::error( "File not found" );
		throw new std::runtime_error( "File not found" );
	}

	std::ifstream file( GetFullPath( filePath ), std::ios::binary );
	std::stringstream buffer;
	buffer << file.rdbuf();
	*outSize = buffer.str().size();
	return new char[*outSize];
}

bool CFileSystem::WriteAllText( std::string filePath, std::string text )
{
	spdlog::error( "Function not implemented" );
	throw new std::runtime_error( "Function not implemented" );
	return false;
}
