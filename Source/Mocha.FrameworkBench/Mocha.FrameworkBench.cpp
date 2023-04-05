//
#include <iostream>
//
#include <Framework/array.h>
#include <Framework/handlemap.h>

const double CheckHandleMapSpeed_Single();
const double CheckArraySpeed_SystemAlloc_Single();
const double CheckArraySpeed_LinearAlloc_Single();
const double CheckVectorSpeed_Single();
const void CheckSpeed( const std::function<double()> func );

const int g_benchmarkCount = 1000;

int main()
{
	std::cout << "Running " << g_benchmarkCount << " benchmarks per type..." << std::endl;
	std::cout << "--------------------" << std::endl;
	std::cout << "[Mocha::HandleMap<T>]" << std::endl;
	CheckSpeed( CheckHandleMapSpeed_Single );

	std::cout << "--------------------" << std::endl;
	std::cout << "[Mocha::Array<T> - SystemAllocator]" << std::endl;
	CheckSpeed( CheckArraySpeed_SystemAlloc_Single );

	std::cout << "--------------------" << std::endl;
	std::cout << "[Mocha::Array<T> - LinearAllocator]" << std::endl;
	CheckSpeed( CheckArraySpeed_LinearAlloc_Single );

	std::cout << "--------------------" << std::endl;
	std::cout << "[std::vector<T>]" << std::endl;
	CheckSpeed( CheckVectorSpeed_Single );

	std::cout << "--------------------" << std::endl;
	return 0;
}

#define SPEED_TEST_BEGIN()

const void CheckSpeed( const std::function<double()> func )
{
	double totalDurationSeconds = 0.0;

	for ( int i = 0; i < g_benchmarkCount; ++i )
	{
		// std::cout << "\tRunning benchmark " << ( i + 1 ) << " of " << g_benchmarkCount << "... " << std::endl;

		double durationSeconds = func();
		// std::cout << "\tTook " << durationSeconds << " seconds" << std::endl;

		totalDurationSeconds += durationSeconds;
	}

	double averageDurationSeconds = totalDurationSeconds / static_cast<double>( g_benchmarkCount );

	// std::cout << "\tAvg. time taken: " << averageDurationSeconds << " seconds" << std::endl;

	const double lookupsPerSecond = 1000000.0 / averageDurationSeconds;
	const double millionLookupsPerSecond = lookupsPerSecond / 1000000.0;
	std::cout << "\tAvg. lookup speed: " << millionLookupsPerSecond << "M/s" << std::endl;
}

inline const std::chrono::steady_clock::time_point StartClock()
{
	return std::chrono::high_resolution_clock::now();
}

inline const double CalculateDurationSeconds( const std::chrono::steady_clock::time_point start, const int count )
{
	const auto end = std::chrono::high_resolution_clock::now();

	// Convert the duration into seconds with high precision
	const auto duration = std::chrono::duration_cast<std::chrono::microseconds>( end - start );
	const double durationSeconds = duration.count() / static_cast<double>( count );
	return durationSeconds;
}

const double CheckArraySpeed_SystemAlloc_Single()
{
	const int count = 1'000'000;

	using Mocha::Array;
	using Mocha::SystemAllocator;

	SystemAllocator alloc;
	Array<int> array;
	array.Init( &alloc, count, count );

	// Add values to the container
	for ( int i = 0; i < count; i++ )
	{
		array.Push( i );
	}

	const auto start = StartClock();

	for ( int i = 0; i < count; i++ )
	{
		int _ = array[i];
	}

	return CalculateDurationSeconds( start, count );
}

const double CheckArraySpeed_LinearAlloc_Single()
{
	const int count = 1'000'000;

	using Mocha::Array;
	using Mocha::LinearAllocator;

	LinearAllocator alloc( sizeof( int ) * count * 4 );
	Array<int> array;
	array.Init( &alloc, count, count );

	// Add values to the container
	for ( int i = 0; i < count; i++ )
	{
		array.Push( i );
	}

	const auto start = StartClock();

	for ( int i = 0; i < count; i++ )
	{
		int _ = array[i];
	}

	return CalculateDurationSeconds( start, count );
}

const double CheckVectorSpeed_Single()
{
	const int count = 1'000'000;

	std::vector<int> array;

	// Add values to the container
	for ( int i = 0; i < count; i++ )
	{
		array.push_back( i );
	}

	const auto start = StartClock();

	for ( int i = 0; i < count; i++ )
	{
		int _ = array[i];
	}

	return CalculateDurationSeconds( start, count );
}

const double CheckHandleMapSpeed_Single()
{
	const int count = 1'000;

	using Mocha::HandleMap;
	HandleMap<int> handleMap;

	// Add values to the container
	for ( int i = 0; i < count; i++ )
	{
		handleMap.Add( i );
	}

	const auto start = StartClock();

	for ( int i = 0; i < count; i++ )
	{
		auto _ = handleMap.Get( i );
	}

	return CalculateDurationSeconds( start, count );
}