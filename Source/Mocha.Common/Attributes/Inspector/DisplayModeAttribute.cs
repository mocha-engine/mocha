namespace Mocha.Common;

/// <summary>
/// Marks a property to override the default display method of its property editor with the given option.
/// </summary>
[AttributeUsage( AttributeTargets.Property, AllowMultiple = false, Inherited = true )]
public class DisplayModeAttribute : Attribute
{
	public DisplayMode DisplayMode { get; }

	public DisplayModeAttribute( DisplayMode displayMode = DisplayMode.Text )
	{
		DisplayMode = displayMode;
	}
}
