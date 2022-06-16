﻿using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Reflection;
using Veldrid;

namespace Mocha;

public class Entity
{
	public static List<Entity> All { get; set; } = Assembly.GetCallingAssembly().GetTypes().OfType<Entity>().ToList();

	//
	// Transform
	// These aren't properties because we want to be able to add to them
	//

	/// <summary>
	/// Right, Up, Forward (FLU)
	/// </summary>
	public Vector3 position;

	public Rotation rotation;
	public Vector3 scale = Vector3.One;

	public string Name { get; set; }

	public Matrix4x4 ModelMatrix
	{
		get
		{
			var matrix = Matrix4x4.CreateScale( scale );
			matrix *= Matrix4x4.CreateTranslation( position );
			matrix *= Matrix4x4.CreateFromQuaternion( rotation.GetSystemQuaternion() );
			matrix *= Matrix4x4.CreateFromYawPitchRoll(
				rotation.Y.DegreesToRadians(),
				rotation.X.DegreesToRadians(),
				rotation.Z.DegreesToRadians() );

			return matrix;
		}
	}

	public Entity()
	{
		All.Add( this );
		Name = $"{this.GetType().Name} {All.Count}";
	}

	public virtual void Render( CommandList commandList ) { }
	public virtual void Update() { }

	public virtual void Delete() { }

	public bool Equals( Entity x, Entity y ) => x.GetHashCode() == y.GetHashCode();
	public int GetHashCode( [DisallowNull] Entity obj ) => base.GetHashCode();
}
