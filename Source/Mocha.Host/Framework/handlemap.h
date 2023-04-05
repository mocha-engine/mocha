#pragma once

#include <Framework/array.h>
#include <Misc/defs.h>
#include <algorithm>
#include <atomic>
#include <execution>
#include <functional>
#include <memory>
#include <shared_mutex>
#include <unordered_map>

namespace Mocha
{
	template <typename T>
	class HandleMap
	{
	private:
		std::shared_mutex m_mutex;

		std::vector<std::unique_ptr<T>> m_objects;
		std::unique_ptr<Mocha::IAllocator> m_allocator;

	public:
		HandleMap()
		{
			m_allocator = std::make_unique<Mocha::SystemAllocator>();
			// m_objects.Init( m_allocator.get(), 0, 0 );
		}

		void Remove( Handle handle );
		Handle Add( T object );

		const std::unique_ptr<T>& Get( Handle handle );

		template <typename T1>
		const std::unique_ptr<T1>& GetSpecific( Handle handle );

		template <typename T1>
		Handle AddSpecific( T1 object );

		void ForEach( std::function<void( const std::unique_ptr<T>& object )> func );
		void For( std::function<void( Handle handle, const std::unique_ptr<T>& object )> func );

		const std::unique_ptr<T>& operator[]( Handle handle );
		
		const std::unique_ptr<T>& Front() const;
		const std::unique_ptr<T>& Back() const;
	};

	template <typename T>
	inline Handle HandleMap<T>::Add( T object )
	{
		std::unique_lock lock( m_mutex );

		Handle handle = m_objects.size();

		auto objectPtr = std::make_unique<T>( object );
		m_objects.push_back( std::move( objectPtr ) );
		return handle;
	}

	template <typename T>
	inline const std::unique_ptr<T>& HandleMap<T>::Get( Handle handle )
	{
		std::shared_lock lock( m_mutex );
		std::unique_ptr<T>& object = m_objects[handle];

		return object;
	}

	template <typename T>
	template <typename T1>
	inline const std::unique_ptr<T1>& HandleMap<T>::GetSpecific( Handle handle )
	{
		static_assert( std::is_base_of<T, T1>::value, "T1 must be derived from T" );

		std::unique_ptr<T> object = Get( handle );

		return std::dynamic_pointer_cast<T1>( object );
	}

	template <typename T>
	template <typename T1>
	inline Handle HandleMap<T>::AddSpecific( T1 object )
	{
		static_assert( std::is_base_of<T, T1>::value, "T1 must be derived from T" );
		std::unique_lock lock( m_mutex );

		Handle handle = m_objects.size();

		auto objectPtr = std::make_unique<T1>( object );
		m_objects.push_back( std::move( objectPtr ) );
		return handle;
	}

	template <typename T>
	inline void HandleMap<T>::ForEach( std::function<void( const std::unique_ptr<T>& object )> func )
	{
		std::shared_lock lock( m_mutex );

		for ( const auto& [handle, object] : m_objects )
		{
			func( object );
		}
	}

	template <typename T>
	inline void HandleMap<T>::For( std::function<void( Handle handle, const std::unique_ptr<T>& object )> func )
	{
		std::shared_lock lock( m_mutex );

		for ( const auto& [handle, object] : m_objects )
		{
			func( handle, object );
		}
	}

	template <typename T>
	inline void HandleMap<T>::Remove( Handle handle )
	{
		std::unique_lock lock( m_mutex );

		m_objects.erase( handle );
	}

	template <typename T>
	inline const std::unique_ptr<T>& HandleMap<T>::operator[]( Handle handle )
	{
		return Get( handle );
	}

	template <typename T>
	inline const std::unique_ptr<T>& HandleMap<T>::Front() const
	{
		return m_objects.front();
	}
	
	template <typename T>
	inline const std::unique_ptr<T>& HandleMap<T>::Back() const
	{
		return m_objects.back();
	}
} // namespace Mocha