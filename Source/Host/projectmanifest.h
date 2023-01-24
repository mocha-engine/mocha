#pragma once

#include <nlohmann/json.hpp>

#ifndef NLOHMANN_OPT_HELPER
#define NLOHMANN_OPT_HELPER
namespace nlohmann
{
	template <typename T>
	struct adl_serializer<std::shared_ptr<T>>
	{
		static std::shared_ptr<T> from_json( const json& j )
		{
			if ( j.is_null() )
				return std::make_shared<T>();
			else
				return std::make_shared<T>( j.get<T>() );
		}
	};
	template <typename T>
	struct adl_serializer<std::optional<T>>
	{
		static std::optional<T> from_json( const json& j )
		{
			if ( j.is_null() )
				return std::make_optional<T>();
			else
				return std::make_optional<T>( j.get<T>() );
		}
	};
} // namespace nlohmann
#endif

namespace ProjectManifest
{
	using nlohmann::json;

#ifndef NLOHMANN_UNTYPED_ProjectManifest_HELPERHELPER
#define NLOHMANN_UNTYPED_ProjectManifest_HELPERHELPER
	inline json get_untyped( const json& j, const char* property )
	{
		if ( j.find( property ) != j.end() )
		{
			return j.at( property ).get<json>();
		}
		return json();
	}

	inline json get_untyped( const json& j, std::string property )
	{
		return get_untyped( j, property.data() );
	}
#endif

#ifndef NLOHMANN_OPTIONAL_ProjectManifest_
#define NLOHMANN_OPTIONAL_ProjectManifest_
	template <typename T>
	inline std::shared_ptr<T> get_heap_optional( const json& j, const char* property )
	{
		auto it = j.find( property );
		if ( it != j.end() && !it->is_null() )
		{
			return j.at( property ).get<std::shared_ptr<T>>();
		}
		return std::shared_ptr<T>();
	}

	template <typename T>
	inline std::shared_ptr<T> get_heap_optional( const json& j, std::string property )
	{
		return get_heap_optional<T>( j, property.data() );
	}
	template <typename T>
	inline std::optional<T> get_stack_optional( const json& j, const char* property )
	{
		auto it = j.find( property );
		if ( it != j.end() && !it->is_null() )
		{
			return j.at( property ).get<std::optional<T>>();
		}
		return std::optional<T>();
	}

	template <typename T>
	inline std::optional<T> get_stack_optional( const json& j, std::string property )
	{
		return get_stack_optional<T>( j, property.data() );
	}
#endif
} // namespace ProjectManifest

struct ProjectClass
{
	std::string defaultNamespace;
	bool nullable;
};

struct Properties
{
	int64_t tickRate;
};

struct Resources
{
	std::string code;
	std::string content;
};

struct Project
{
	std::string name;
	std::string author;
	std::string version;
	std::string description;
	Resources resources;
	Properties properties;
	ProjectClass project;
};

namespace ProjectManifest
{
	void from_json( const json& j, ProjectClass& x );
	void from_json( const json& j, Properties& x );
	void from_json( const json& j, Resources& x );
	void from_json( const json& j, Project& x );

	inline void from_json( const json& j, ProjectClass& x )
	{
		x.defaultNamespace = get_stack_optional<std::string>( j, "defaultNamespace" ).value_or( "" );
		x.nullable = get_stack_optional<bool>( j, "nullable" ).value_or( false );
	}

	inline void from_json( const json& j, Properties& x )
	{
		x.tickRate = get_stack_optional<int64_t>( j, "tickRate" ).value_or( 60 );
	}

	inline void from_json( const json& j, Resources& x )
	{
		x.code = get_stack_optional<std::string>( j, "code" ).value_or( "" );
		x.content = get_stack_optional<std::string>( j, "content" ).value_or( "" );
	}

	inline void from_json( const json& j, Project& x )
	{
		x.name = get_stack_optional<std::string>( j, "name" ).value_or( "Unnamed" );
		x.author = get_stack_optional<std::string>( j, "author" ).value_or( "Unknown" );
		x.version = get_stack_optional<std::string>( j, "version" ).value_or( "1.0.0" );
		x.description = get_stack_optional<std::string>( j, "description" ).value_or( "" );

		from_json( get_untyped( j, "resources" ), x.resources );
		from_json( get_untyped( j, "properties" ), x.properties );
		from_json( get_untyped( j, "project" ), x.project );
	}
} // namespace ProjectManifest
