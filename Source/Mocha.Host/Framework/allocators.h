#pragma once
#include <cassert>
#include <memory>

namespace Mocha
{
	/// <summary>
	/// Base allocator interface
	/// </summary>
	class IAllocator
	{
	public:
		virtual void* Alloc( const size_t size, const size_t alignment, const size_t offset ) = 0;
		virtual void Free( void* ptr ) = 0;
		virtual void Reset() = 0;
	};

	/// <summary>
	/// Basic linear allocator. Allocates memory in a linear fashion, and can be reset to free all memory.
	/// </summary>
	class LinearAllocator : public IAllocator
	{
	private:
		char* m_start{ nullptr };
		char* m_end{ nullptr };
		char* m_current{ nullptr };

	public:
		LinearAllocator( void* start, void* end )
		{
			m_start = ( char* )start;
			m_end = ( char* )end;
			Reset();
		}

		explicit LinearAllocator( size_t size )
		{
			m_start = new char[size];
			m_end = m_start + size;
			Reset();
		}

		/// <summary>
		/// Increments a value indicating the current buffer offset
		/// </summary>
		inline void* Alloc( const size_t size, const size_t alignment, const size_t offset )
		{
			void* ptr = m_current + offset;
			size_t space = m_end - m_current;

			m_current = ( char* )std::align( alignment, size, ptr, space ) - offset;

			void* result = m_current;
			m_current += size;

			assert( m_current < m_end );

			return ptr;
		}

		/// <summary>
		/// Does nothing
		/// </summary>
		void Free( void* ptr )
		{
			//
		}

		/// <summary>
		/// Free all allocated memory and return the allocator to its original initialized state
		/// </summary>
		void Reset()
		{
			m_current = m_start;
		}
	};

	/// <summary>
	/// Basic passthrough allocator, wraps malloc & free
	/// </summary>
	class SystemAllocator : public IAllocator
	{
		/// <summary>
		/// Allocate a block of memory.
		/// Alignment and offset do not do anything here.
		/// </summary>
		inline void* Alloc( const size_t size, const size_t alignment, const size_t offset )
		{
			return malloc( size );
		}

		/// <summary>
		/// Free a block of memory.
		/// </summary>
		inline void Free( void* ptr )
		{
			return free( ptr );
		}

		/// <summary>
		/// Does nothing
		/// </summary>
		inline void Reset()
		{
			//
		}
	};
} // namespace Mocha