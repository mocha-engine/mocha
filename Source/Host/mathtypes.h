#pragma once

#include <algorithm>
#include <cmath>
#include <glm/glm.hpp>
#include <glm/gtc/quaternion.hpp>
#include <glm/gtx/quaternion.hpp>
#include <imgui.h>

struct Size2D
{
	uint32_t x;
	uint32_t y;
};

struct Vector2
{
	float x;
	float y;

	inline glm::vec2 ToGLM() { return glm::vec2( x, y ); }
	inline ImVec2 ToImGUI() { return { x, y }; };

	static Vector2 Lerp( Vector2 a, Vector2 b, float t )
	{
		Vector2 res = {};

		res.x = std::lerp( a.x, b.x, t );
		res.y = std::lerp( a.y, b.y, t );

		return res;
	}
};

struct Vector3
{
	float x;
	float y;
	float z;

	inline glm::vec3 ToGLM() { return glm::vec3( x, y, z ); }

	static Vector3 Lerp( Vector3 a, Vector3 b, float t )
	{
		Vector3 res = {};

		res.x = std::lerp( a.x, b.x, t );
		res.y = std::lerp( a.y, b.y, t );
		res.z = std::lerp( a.z, b.z, t );

		return res;
	}
};

struct Vector4
{
	float x;
	float y;
	float z;
	float w;

	inline glm::vec4 ToGLM() { return glm::vec4( x, y, z, w ); }

	static Vector4 Lerp( Vector4 a, Vector4 b, float t )
	{
		Vector4 res = {};

		res.x = std::lerp( a.x, b.x, t );
		res.y = std::lerp( a.y, b.y, t );
		res.z = std::lerp( a.z, b.z, t );
		res.w = std::lerp( a.w, b.w, t );

		return res;
	}
};

struct Quaternion
{
	float x;
	float y;
	float z;
	float w;

	inline glm::quat ToGLM() { return glm::quat( w, x, y, z ); }

	static float Dot( Quaternion a, Quaternion b ) { return a.x * b.x + a.y * b.y + a.z * b.z + a.w * b.w; }

	Quaternion operator/( float b )
	{
		Quaternion res = {};
		res.x = x / b;
		res.y = y / b;
		res.z = z / b;
		res.w = w / b;

		return res;
	}

	Quaternion operator*( float b )
	{
		Quaternion res = {};
		res.x = x * b;
		res.y = y * b;
		res.z = z * b;
		res.w = w * b;

		return res;
	}

	Quaternion operator-( Quaternion b )
	{
		Quaternion res = {};
		res.x = x - b.x;
		res.y = y - b.y;
		res.z = z - b.z;
		res.w = w - b.w;

		return res;
	}

	Quaternion operator+( Quaternion b )
	{
		Quaternion res = {};
		res.x = x + b.x;
		res.y = y + b.y;
		res.z = z + b.z;
		res.w = w + b.w;

		return res;
	}

	Quaternion Normalized()
	{
		float mag = sqrt( Dot( *this, *this ) );

		if ( mag < FLT_EPSILON )
			return Quaternion{ 0.0f, 0.0f, 0.0f, 1.0f };

		Quaternion res = {};
		res.x = x / mag;
		res.y = y / mag;
		res.z = z / mag;
		res.w = w / mag;

		return res;
	}

	static Quaternion Slerp( Quaternion a, Quaternion b, float t )
	{
		// Calculate the dot product of the quaternions
		float dot = Dot( a, b );

		// Clamp the dot product to the range [-1, 1]
		dot = std::clamp( dot, -1.0f, 1.0f );

		// If the dot product is negative, negate one of the quaternions to ensure that we interpolate
		// in the shortest possible arc.
		if ( dot < 0 )
		{
			b.x = -b.x;
			b.y = -b.y;
			b.z = -b.z;
			b.w = -b.w;

			dot = -dot;
		}

		// Calculate the angle between the quaternions
		float theta = acos( dot ) * t;

		// Calculate the interpolated quaternion
		Quaternion ad = a * dot;
		Quaternion bSubAd = b - ad;
		Quaternion q = bSubAd.Normalized();
		return a * cos( theta ) + q * sin( theta );
	}
};

struct Transform
{
	Vector3 position = { 0.0f, 0.0f, 0.0f };
	Quaternion rotation = { 0.0f, 0.0f, 0.0f, 1.0f };
	Vector3 scale = { 1.0f, 1.0f, 1.0f };

	glm::mat4x4 GetModelMatrix()
	{
		glm::mat4x4 model = glm::mat4{ 1.0f };
		model *= glm::translate( glm::mat4{ 1.0f }, position.ToGLM() );
		model *= glm::mat4_cast( rotation.ToGLM() );
		model *= glm::scale( glm::mat4{ 1.0f }, scale.ToGLM() );

		return model;
	}

	static Transform Lerp( Transform a, Transform b, float t )
	{
		Transform res = {};

		res.position = res.position.Lerp( a.position, b.position, t );
		res.rotation = res.rotation.Slerp( a.rotation, b.rotation, t );
		res.scale = res.scale.Lerp( a.scale, b.scale, t );

		return res;
	}
};
