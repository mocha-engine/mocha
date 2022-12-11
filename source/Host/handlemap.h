#pragma once
#include <functional>
#include <memory>
#include <unordered_map>

typedef uint32_t Handle;

// A class that manages a collection of objects of type T, indexed by a handle.
template <typename T>
class HandleMap
{
private:
	// A map of objects, indexed by their handle index.
	std::unordered_map<Handle, std::shared_ptr<T>> m_objects;

	// The current index to use when inserting a new object into the map.
	Handle m_nextIndex;

protected:
	// Adds the specified object to the map and returns a handle to it.
	Handle Add( T object );

	// Returns a pointer to the object associated with the specified handle.
	std::shared_ptr<T> Get( Handle handle );

	// Use this if you want to get a derived type.
	template <typename T1>
	std::shared_ptr<T1> GetSpecific( Handle handle );

	// Use this if you want to add a derived type.
	template <typename T1>
	Handle AddSpecific( T1 object );

	// Calls the specified function for each object managed by this HandleMap.
	// The function should take a std::unique_ptr <T> as its argument.
	void ForEach( std::function<void( std::shared_ptr<T> object )> func );
};

template <typename T>
inline Handle HandleMap<T>::Add( T object )
{
	// Create a shared pointer to the object.
	auto objectPtr = std::make_shared<T>( object );

	// Add the object to the map.
	m_objects[m_nextIndex] = objectPtr;

	return m_nextIndex++;
}

// Returns a pointer to the object associated with the specified handle.
template <typename T>
inline std::shared_ptr<T> HandleMap<T>::Get( Handle handle )
{
	return m_objects[handle];
}

// Use this if you want to get a derived type.
template <typename T>
template <typename T1>
inline std::shared_ptr<T1> HandleMap<T>::GetSpecific( Handle handle )
{
	static_assert( std::is_base_of<T, T1>::value, "T1 must be derived from T" );

	return std::dynamic_pointer_cast<T1>( m_objects[handle] );
}

// Use this if you want to add a derived type.
template <typename T>
template <typename T1>
inline Handle HandleMap<T>::AddSpecific( T1 object )
{
	static_assert( std::is_base_of<T, T1>::value, "T1 must be derived from T" );

	// Create a shared pointer to the object.
	auto objectPtr = std::make_shared<T1>( object );

	// Add the object to the map.
	m_objects[m_nextIndex] = objectPtr;

	return m_nextIndex++;
}

// Calls the specified function for each object managed by this HandleMap.
// The function should take a std::shared_ptr<T> as its argument.
template <typename T>
inline void HandleMap<T>::ForEach( std::function<void( std::shared_ptr<T> object )> func )
{
	for ( const auto& [handle, object] : m_objects )
	{
		func( object );
	}
}
