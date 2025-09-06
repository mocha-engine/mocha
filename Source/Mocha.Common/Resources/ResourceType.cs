namespace Mocha.Common;

public partial struct ResourceType
{
	/// <summary>
	/// The name of this resource. Should be human-readable
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	/// The resulting extension for a compiled asset of this type <b>WITHOUT</b> the "_c" suffix
	/// </summary>
	public string Extension { get; set; }

	/// <summary>
	/// A list of extensions at which a source file could exist
	/// </summary>
	public string[] SourceExtensions { get; set; }

	/// <summary>
	/// A path to a mtex resource containing a large icon
	/// </summary>
	public string IconLg { get; set; }

	/// <summary>
	/// A glyph or emoji used to represent this resource
	/// </summary>
	public string IconSm { get; set; }

	/// <summary>
	/// A color used to represent this resource (typically the average color for <see cref="IconLg" />)
	/// </summary>
	public Vector4 Color { get; set; }

	public static ResourceType[] All
	{
		get
		{
			return typeof( ResourceType ).GetProperties()
				.Where( x => x.GetMethod?.IsStatic ?? false )
				.Where( x => x.PropertyType == typeof( ResourceType ) )
				.Select( x => x.GetValue( null ) )
				.OfType<ResourceType>()
				.ToArray();
		}
	}

	/// <summary>
	/// Find a <see cref="ResourceType"/> that matches the extension given.
	/// </summary>
	/// <remarks>
	/// If no resource type was found, null will be returned.
	/// <para>
	/// In order to handle this, either use the null-coalescing operator to ensure that there is
	/// a valid resource type (you should use <see cref="Default" /> for this)
	/// or check for a value yourself with HasValue.
	/// </para>
	/// </remarks>
	public static ResourceType? GetResourceForExtension( string extension )
	{
		if ( extension.EndsWith( "_c", StringComparison.InvariantCultureIgnoreCase ) )
			extension = extension[..^2];

		if ( extension.StartsWith( "." ) )
			extension = extension[1..];

		foreach ( var resourceType in All )
		{
			if ( resourceType.Extension.Equals( extension, StringComparison.InvariantCultureIgnoreCase ) )
				return resourceType;
		}

		return null;
	}
}
