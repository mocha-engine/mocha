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
	std::vector<T> data;
	
	data.resize( list.count );
	data.insert( data.begin(), list.data, list.data + list.size );

	return data;
}