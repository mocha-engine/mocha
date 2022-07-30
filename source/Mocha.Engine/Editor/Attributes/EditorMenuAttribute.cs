namespace Mocha.Engine;

[AttributeUsage( AttributeTargets.Class )]
public class EditorMenuAttribute : Attribute
{
	public string Path { get; set; }
	public string Icon { get; set; }

	public EditorMenuAttribute( string icon, string path )
	{
		Path = path;
		Icon = icon;
	}
}
