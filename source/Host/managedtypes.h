#pragma once
#include <vector>

//@InteropGen generate struct
struct InteropStruct
{
	int count;
	int size;
	void* data;
};

template <typename T>
std::vector<T> GetData( InteropStruct list )
{
	std::vector<T> vec;
	T* data = ( T* )list.data;
	
	vec.insert( vec.begin(), data, data + list.count );

	return vec;
}