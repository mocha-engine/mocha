#pragma once
#include <handlemap.h>
#include <memory>
#include <string>
#include <texture.h>
#include <unordered_map>

//
// This is slightly different from a normal HandleMap in that we also
// store, along with the texture, the path of the texture so that we
// can refer to it again later.
//
// This is done by holding a separate map of ( string, handle ) so that
// we can just go straight from string -> handle -> texture without
// worrying too much about performance.
//
class TextureCache : HandleMap<Texture>
{
private:
	std::unordered_map<std::string, Handle> m_internalPathMap;

public:
	Handle AddTexture( std::string path, Texture texture );
	Handle GetHandle( std::string path );
	std::shared_ptr<Texture> GetTexture( std::string path );
};