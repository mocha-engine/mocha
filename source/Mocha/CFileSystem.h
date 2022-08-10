#pragma once
#include <filesystem>
#include <fstream>
#include <functional>
#include <iostream>
#include <memory>
#include <sstream>

//@InteropGen generate class
class CFileSystem
{
private:
	std::filesystem::path m_baseDirectory;

	// TODO(InteropGen): Auto-ignore private / protected methods

	//@InteropGen ignore
	std::string* SelectDirectoryEntries(
	    std::string dirPath, int* outSize, std::function<bool( std::filesystem::directory_entry )> filter );
	//@InteropGen ignore
	std::string GetFullPath( std::string fileOrDirectoryPath );

public:
	CFileSystem( std::string baseDirectory );
	~CFileSystem();

	bool DirectoryExists( std::string dirPath );
	bool FileExists( std::string filePath );

	bool IsDirectory( std::string dirPath );
	bool IsFile( std::string filePath );

	std::string* GetFiles( std::string dirPath, int* outSize );
	std::string* GetDirectories( std::string dirPath, int* outSize );

	std::string ReadAllText( std::string filePath );
	char* ReadAllBytes( std::string filePath, int* outSize );

	bool WriteAllText( std::string filePath, std::string text );
};
