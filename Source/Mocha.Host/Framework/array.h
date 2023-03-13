#pragma once

#include <Framework/allocators.h>

namespace Mocha
{
	template <typename T>
	class Array
	{
	private:
		T* m_data{ nullptr };
		size_t m_size{ 0 };
		size_t m_capacity{ 0 };

		IAllocator* m_allocator{ nullptr };

	public:
		void Init( IAllocator* allocator, size_t capacity, size_t size );
		void Destroy();

		// ----------------------------------------

		void Push( const T& object );
		void Pop();
		T& PushUse();

		// ----------------------------------------

		T& operator[]( size_t index );
		const T& operator[]( size_t index ) const;

		T* Data();
		const T* Data() const;

		// ----------------------------------------

		size_t Size() const;
		size_t Capacity() const;

		void Clear();
		void Grow( size_t capacity );

		// ----------------------------------------

		T& Back();
		const T& Back() const;

		T& Front();
		const T& Front() const;
	};

	// ----------------------------------------------------------------------------------------------------------------------------

	template <typename T>
	inline void Array<T>::Init( IAllocator* allocator, size_t capacity, size_t size )
	{
		m_data = nullptr;
		m_size = size;

		m_capacity = 0;
		m_allocator = allocator;

		if ( capacity > 0 )
		{
			Grow( capacity );
		}
	}

	template <typename T>
	inline void Array<T>::Destroy()
	{
		if ( m_capacity > 0 )
		{
			m_allocator->Free( m_data );
		}

		m_data = nullptr;

		m_size = 0;
		m_capacity = 0;
	}

	template <typename T>
	inline void Array<T>::Push( const T& object )
	{
		if ( m_size >= m_capacity )
		{
			Grow( m_capacity + 1 );
		}

		m_data[m_size++] = object;
	}

	template <typename T>
	inline void Array<T>::Pop()
	{
		assert( m_size > 0 );
		--m_size;
	}

	template <typename T>
	inline T& Array<T>::PushUse()
	{
		if ( m_size >= m_capacity )
		{
			Grow( m_capacity + 1 );
		}

		++m_size;

		return Back();
	}

	template <typename T>
	inline T& Array<T>::operator[]( size_t index )
	{
		assert( index >= 0 && index < m_size );
		return m_data[index];
	}

	template <typename T>
	inline const T& Array<T>::operator[]( size_t index ) const
	{
		assert( index >= 0 && index < m_size );
		return m_data[index];
	}

	template <typename T>
	inline T* Array<T>::Data()
	{
		return m_data;
	}

	template <typename T>
	inline const T* Array<T>::Data() const
	{
		return m_data;
	}

	template <typename T>
	inline size_t Array<T>::Size() const
	{
		return m_size;
	}

	template <typename T>
	inline size_t Array<T>::Capacity() const
	{
		return m_capacity;
	}

	template <typename T>
	inline void Array<T>::Clear()
	{
		m_size = 0;
	}

	template <typename T>
	inline void Array<T>::Grow( size_t capacity )
	{
		assert( capacity > 0 );
		
		if ( capacity < m_capacity * 2 )
			capacity = capacity * 2;
		else if ( capacity < 4 )
			capacity = 4;

		T* newData = ( T* )m_allocator->Alloc( capacity * sizeof( T ), alignof( T ), 0 );

		if ( m_capacity )
		{
			memcpy_s( newData, capacity * sizeof( T ), m_data, m_size * sizeof( T ) );
			m_allocator->Free( m_data );
		}

		m_data = newData;
		m_capacity = capacity;
	}

	template <typename T>
	inline T& Array<T>::Back()
	{
		assert( m_size > 0 );
		return m_data[m_size - 1];
	}

	template <typename T>
	inline const T& Array<T>::Back() const
	{
		assert( m_size > 0 );
		return m_data[m_size - 1];
	}

	template <typename T>
	inline T& Array<T>::Front()
	{
		assert( m_size >= 0 );
		return m_data[0];
	}

	template <typename T>
	inline const T& Array<T>::Front() const
	{
		assert( m_size >= 0 );
		return m_data[0];
	}
} // namespace Mocha
