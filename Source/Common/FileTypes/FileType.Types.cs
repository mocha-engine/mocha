namespace Mocha.Common;

partial struct FileType
{
	public static FileType Shader => new FileType()
	{
		Name = "Shader",
		Extension = "mshdr",
		SourceExtensions = new[] { "mshdr" },
		IconLg = "core/ui/icons/shader.mtex",
		IconSm = FontAwesome.Glasses,
		Color = MathX.GetColor( "#ffc710" )
	};

	public static FileType Material => new FileType()
	{
		Name = "Material",
		Extension = "mmat",
		SourceExtensions = new[] { "mmat" },
		IconLg = "core/ui/icons/material.mtex",
		IconSm = FontAwesome.Circle,
		Color = MathX.GetColor( "#f7b239" )
	};

	public static FileType Texture => new FileType()
	{
		Name = "Texture",
		Extension = "mtex",
		SourceExtensions = new[] { "png", "jpg" },
		IconLg = "core/ui/icons/image.mtex",
		IconSm = FontAwesome.Image,
		Color = MathX.GetColor( "#5292fa" )
	};

	public static FileType Model => new FileType()
	{
		Name = "Model",
		Extension = "mmdl",
		SourceExtensions = new[] { "mmdl" },
		IconLg = "core/ui/icons/model.mtex",
		IconSm = FontAwesome.Cube,
		Color = MathX.GetColor( "#1ee3a5" )
	};

	public static FileType Sound => new FileType()
	{
		Name = "Sound",
		Extension = "msnd",
		SourceExtensions = { },
		IconLg = "core/ui/icons/sound.mtex",
		IconSm = FontAwesome.VolumeHigh,
		Color = MathX.GetColor( "#fe646f" )
	};

	public static FileType Font => new FileType()
	{
		Name = "Font",
		Extension = "mfnt",
		SourceExtensions = new[] { "ttf" },
		IconLg = "core/ui/icons/font.mtex",
		IconSm = FontAwesome.Font,
		Color = MathX.GetColor( "#acb4bc" )
	};

	public static FileType Default => new FileType()
	{
		Name = "Unknown",
		Extension = "*",
		IconLg = "core/ui/icons/document.mtex",
		IconSm = FontAwesome.File,
		Color = MathX.GetColor( "#ffffff" )
	};
}
