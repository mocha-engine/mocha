namespace Mocha.Common;

public partial struct FileType
{
	public string Name { get; set; }
	public string Extension { get; set; }
	public string[] SourceExtensions { get; set; }
	public string IconLg { get; set; }
	public string IconSm { get; set; }
	public Vector4 Color { get; set; }

	public static FileType[] All
	{
		get
		{
			return typeof( FileType ).GetProperties()
				.Where( x => x.GetMethod?.IsStatic ?? false )
				.Where( x => x.PropertyType == typeof( FileType ) )
				.Select( x => x.GetValue( null ) )
				.OfType<FileType>()
				.ToArray();
		}
	}

	public static FileType? GetFileTypeForExtension( string extension )
	{
		if ( extension.EndsWith( "_c", StringComparison.InvariantCultureIgnoreCase ) )
			extension = extension[..^2];

		if ( extension.StartsWith( "." ) )
			extension = extension[1..];

		foreach ( var fileType in All )
		{
			if ( fileType.Extension.Equals( extension, StringComparison.InvariantCultureIgnoreCase ) )
				return fileType;
		}

		return null;
	}
}
