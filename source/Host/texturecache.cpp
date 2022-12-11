#include "texturecache.h"

Handle TextureCache::AddTexture( std::string path, Texture texture )
{
	Handle handle = Add( texture );
	m_internalPathMap[path] = handle;

	return handle;
}

Handle TextureCache::GetHandle( std::string path )
{
	return m_internalPathMap[path];
}

std::shared_ptr<Texture> TextureCache::GetTexture( std::string path )
{
	Handle handle = GetHandle( path );
	return Get( handle );
}
