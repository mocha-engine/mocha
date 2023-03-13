#include <Framework/handlemap.h>
#include <iostream>

const double CheckHandleMapSpeed();

int main()
{
	constexpr int count = 3;
	double totalDurationSeconds = 0.0;

	for ( int i = 0; i < count; ++i )
	{
		std::cout << "Running benchmark " << ( i + 1 ) << " of " << count << "... " << std::endl;

		double durationSeconds = CheckHandleMapSpeed();
		std::cout << "Took " << durationSeconds << " seconds" << std::endl;

		totalDurationSeconds += durationSeconds;
	}

	double averageDurationSeconds = totalDurationSeconds / static_cast<double>( count );

	std::cout << "Avg. time taken: " << averageDurationSeconds << " seconds" << std::endl;

	const double lookupsPerSecond = 1000000.0 / averageDurationSeconds;
	const double millionLookupsPerSecond = lookupsPerSecond / 1000000.0;
	std::cout << "HandleMap avg. lookup speed: " << millionLookupsPerSecond << "M/s" << std::endl;

	return 0;
}

const double CheckHandleMapSpeed()
{
	const int count = 1'000'000;

	// Instantiate the handlemap with values
	Mocha::HandleMap<int> handlemap;

	// Add values to the handlemap
	for ( int i = 0; i < count; i++ )
	{
		handlemap.Add( i );
	}

	// Calculate the time required to do lookups
	const auto start = std::chrono::high_resolution_clock::now();

	for ( int i = 0; i < count; i++ )
	{
		handlemap.Get( i );
	}

	const auto end = std::chrono::high_resolution_clock::now();

	// Convert the duration into seconds with high precision
	const auto duration = std::chrono::duration_cast<std::chrono::microseconds>( end - start );
	const double durationSeconds = duration.count() / static_cast<double>( count );
	return durationSeconds;
}