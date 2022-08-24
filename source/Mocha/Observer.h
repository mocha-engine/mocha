#pragma once

#include "Assert.h"

enum Event
{
	CWINDOW_RESIZED,
};

class IObserver
{
public:
	virtual ~IObserver() {}
	virtual void OnNotify( Event event, void* data ) = 0;
};

class ISubject
{
private:
	inline static std::vector<IObserver*> mObservers = {};

public:
	void AddObserver( IObserver* observer ) { mObservers.push_back( observer ); }

	void RemoveObserver( IObserver* observer )
	{
		auto iter = std::find( mObservers.begin(), mObservers.end(), observer );
		ASSERT( iter != mObservers.end() );

		mObservers.erase( iter );
	}

	void Notify( Event event, void* data )
	{
		for ( size_t i = 0; i < mObservers.size(); i++ )
		{
			mObservers[i]->OnNotify( event, data );
		}
	}
};