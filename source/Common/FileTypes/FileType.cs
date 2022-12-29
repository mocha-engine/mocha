namespace Mocha.Common;

public struct FileType
{
	public FileType( string name, string extension, string iconLg, string iconSm, Vector4 color )
	{
		Name = name;
		Extension = extension;
		IconLg = iconLg;
		IconSm = iconSm;
		Color = color;
	}

	public string Name { get; set; }
	public string Extension { get; set; }
	public string IconLg { get; set; }
	public string IconSm { get; set; }
	public Vector4 Color { get; set; }

	public static FileType Shader =>
		new FileType( "Shader", "mshdr", "core/ui/icons/shader.mtex", FontAwesome.Glasses, MathX.GetColor( "#ffc710" ) );

	public static FileType Material =>
		new FileType( "Material", "mmat", "core/ui/icons/material.mtex", FontAwesome.Circle, MathX.GetColor( "#f7b239" ) );

	public static FileType Texture =>
		new FileType( "Texture", "mtex", "core/ui/icons/image.mtex", FontAwesome.Image, MathX.GetColor( "#5292fa" ) );

	public static FileType Model =>
		new FileType( "Model", "mmdl", "core/ui/icons/model.mtex", FontAwesome.Cube, MathX.GetColor( "#1ee3a5" ) );

	public static FileType Default =>
		new FileType( "Unknown", "*", "core/ui/icons/document.mtex", FontAwesome.File, MathX.GetColor( "#ffffff" ) );

	public static FileType Sound =>
		new FileType( "Sound", "msnd", "core/ui/icons/sound.mtex", FontAwesome.VolumeHigh, MathX.GetColor( "#fe646f" ) );

	public static FileType Font =>
		new FileType( "Font", "mfnt", "core/ui/icons/font.mtex", FontAwesome.Font, MathX.GetColor( "#acb4bc" ) );

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
