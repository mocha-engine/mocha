#pragma once
#include <handlemap.h>

//
// A generic version of HandleMap that can be used anywhere.
//
template <typename T>
class UtilHandleMap : public HandleMap
{
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