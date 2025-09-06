using System.Collections.Immutable;

namespace MochaTool.InteropGen.Parsing;

/// <summary>
/// Builds a representation of a C++ container.
/// </summary>
internal sealed class ContainerBuilder
{
	/// <summary>
	/// The type of container that is being built.
	/// </summary>
	internal ContainerType Type { get; }

	/// <summary>
	/// The name of the container.
	/// </summary>
	internal string Name { get; }

	/// <summary>
	/// Whether or not the container has any items within it.
	/// </summary>
	internal bool IsEmpty => fields.Count == 0 && methods.Count == 0;

	private readonly ImmutableArray<Variable>.Builder fields;
	private readonly ImmutableArray<Method>.Builder methods;

	/// <summary>
	/// Does a method with this name already exist in this container?
	/// </summary>
	public bool HasMethod( string name ) => methods.Any( x => x.Name == name );

	/// <summary>
	/// Finds an available name for a function. If one isn't available, follows
	/// the function name up with a counter.
	/// </summary>
	public string FindFreeName( string desiredName )
	{
		var name = desiredName;
		var index = 0;

		while ( HasMethod( name ) )
		{
			index++;
			name = $"{desiredName}{index}";
		}

		return desiredName;
	}

	/// <summary>
	/// Initializes a new instance of <see cref="ContainerBuilder"/>.
	/// </summary>
	/// <param name="type">The type of the container.</param>
	/// <param name="name">The name of the container.</param>
	internal ContainerBuilder( ContainerType type, string name )
	{
		Type = type;
		Name = name;

		fields = ImmutableArray.CreateBuilder<Variable>();
		methods = ImmutableArray.CreateBuilder<Method>();
	}

	/// <summary>
	/// Adds a new field to the container.
	/// </summary>
	/// <param name="field">The field to add.</param>
	/// <returns>The same instance of <see cref="ContainerBuilder"/>.</returns>
	internal ContainerBuilder AddField( Variable field )
	{
		fields.Add( field );
		return this;
	}

	/// <summary>
	/// Adds a new method to the container.
	/// </summary>
	/// <param name="method">The method to add.</param>
	/// <returns>The same instance of <see cref="ContainerBuilder"/>.</returns>
	internal ContainerBuilder AddMethod( Method method )
	{
		methods.Add( method );
		return this;
	}

	/// <summary>
	/// Constructs a new instance of the container.
	/// </summary>
	/// <returns>A new container instance that implements the <see cref="IContainerUnit"/> interface.</returns>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when trying to build a container with an invalid type.</exception>
	internal IContainerUnit Build()
	{
		fields.Capacity = fields.Count;
		methods.Capacity = methods.Count;

		return Type switch
		{
			ContainerType.Class => Class.Create( Name, fields.MoveToImmutable(), methods.MoveToImmutable() ),
			ContainerType.Namespace => Namespace.Create( Name, fields.MoveToImmutable(), methods.MoveToImmutable() ),
			ContainerType.Struct => Struct.Create( Name, fields.MoveToImmutable(), methods.MoveToImmutable() ),
			_ => throw new ArgumentOutOfRangeException()
		};
	}
}
