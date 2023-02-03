namespace Mocha.Common;

partial struct ResourceType
{
	public static ResourceType Shader => new ResourceType()
	{
		Name = "Shader",
		Extension = "mshdr",
		SourceExtensions = new[] { "mshdr" },
		IconLg = "ui/icons/shader.mtex",
		IconSm = FontAwesome.Glasses,
		Color = MathX.GetColor( "#ffc710" )
	};

	public static ResourceType Material => new ResourceType()
	{
		Name = "Material",
		Extension = "mmat",
		SourceExtensions = new[] { "mmat" },
		IconLg = "ui/icons/material.mtex",
		IconSm = FontAwesome.Circle,
		Color = MathX.GetColor( "#f7b239" )
	};

	public static ResourceType Texture => new ResourceType()
	{
		Name = "Texture",
		Extension = "mtex",
		SourceExtensions = new[] { "png", "jpg" },
		IconLg = "ui/icons/image.mtex",
		IconSm = FontAwesome.Image,
		Color = MathX.GetColor( "#5292fa" )
	};

	public static ResourceType Model => new ResourceType()
	{
		Name = "Model",
		Extension = "mmdl",
		SourceExtensions = new[] { "mmdl" },
		IconLg = "ui/icons/model.mtex",
		IconSm = FontAwesome.Cube,
		Color = MathX.GetColor( "#1ee3a5" )
	};

	public static ResourceType Sound => new ResourceType()
	{
		Name = "Sound",
		Extension = "msnd",
		SourceExtensions = { },
		IconLg = "ui/icons/sound.mtex",
		IconSm = FontAwesome.VolumeHigh,
		Color = MathX.GetColor( "#fe646f" )
	};

	public static ResourceType Font => new ResourceType()
	{
		Name = "Font",
		Extension = "mfnt",
		SourceExtensions = new[] { "ttf" },
		IconLg = "ui/icons/font.mtex",
		IconSm = FontAwesome.Font,
		Color = MathX.GetColor( "#acb4bc" )
	};

	public static ResourceType Default => new ResourceType()
	{
		Name = "Unknown",
		Extension = "*",
		IconLg = "ui/icons/document.mtex",
		IconSm = FontAwesome.File,
		Color = MathX.GetColor( "#ffffff" )
	};
}
