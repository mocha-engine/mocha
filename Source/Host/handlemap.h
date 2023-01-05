#pragma once
#define THREADED
#include <defs.h>
#include <functional>
#include <memory>
#include <unordered_map>

#ifdef THREADED
#include <algorithm>
#include <execution>
#endif

// A class that manages a collection of objects of type T, indexed by a handle.
template <typename T>
class HandleMap
{
private:
	// A map of objects, indexed by their handle index.
	std::unordered_map<Handle, std::shared_ptr<T>> m_objects;

	// The current index to use when inserting a new object into the map.
	Handle m_nextIndex;

public:
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
	// The function should take a std::unique_ptr<T> as its argument.
	void ForEach( std::function<void( std::shared_ptr<T> object )> func );

	// Calls the specified function for each object managed by this HandleMap.
	// The function should take a Handle and a std::unique_ptr<T> as its arguments.
	void For( std::function<void( Handle handle, std::shared_ptr<T> object )> func );
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
#ifndef THREADED
	for ( const auto& [handle, object] : m_objects )
	{
		func( object );
	}
#else
	std::for_each(
	    std::execution::par_unseq, m_objects.cbegin(), m_objects.cend(), [func]( const auto& pair ) { func( pair.second ); } );
#endif
}

// Calls the specified function for each object managed by this HandleMap.
// The function should take a Handle as its argument.
template <typename T>
inline void HandleMap<T>::For( std::function<void( Handle handle, std::shared_ptr<T> object )> func )
{
#ifndef THREADED
	for ( const auto& [handle, object] : m_objects )
	{
		func( handle, object );
	}
#else
	std::for_each( std::execution::par_unseq, m_objects.cbegin(), m_objects.cend(),
	    [func]( const auto& pair ) { func( pair.first, pair.second ); } );
#endif
}