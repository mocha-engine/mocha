using System.Reflection;

namespace Mocha.Editor;

/// <summary>
/// A property editor with all of the preferences set.
/// </summary>
public class SimplePropertyEditor : BasePropertyEditor
{
	/// <summary>
	/// The default value of <see cref="Min"/>.
	/// </summary>
	protected const int DefaultMin = int.MinValue;
	/// <summary>
	/// The default value of <see cref="Max"/>.
	/// </summary>
	protected const int DefaultMax = int.MaxValue;
	/// <summary>
	/// The default value of <see cref="DisplayMode"/>.
	/// </summary>
	protected const DisplayMode DefaultDisplayMode = DisplayMode.Text;
	/// <summary>
	/// The default speed drag elements should be set to.
	/// </summary>
	protected const int DefaultDragSpeed = 1;

	/// <summary>
	/// The minimum value the proeprty should have.
	/// </summary>
	protected int Min { get; set; }
	/// <summary>
	/// The maximum value the property should have.
	/// </summary>
	protected int Max { get; set; }
	/// <summary>
	/// The mode that the element should be displayed in.
	/// </summary>
	protected DisplayMode DisplayMode { get; set; }

	public SimplePropertyEditor( object containingObject, PropertyInfo propertyInfo, bool readOnly )
		: base( containingObject, propertyInfo, readOnly )
	{
		var minMaxAttribute = propertyInfo.GetCustomAttribute<MinMaxAttribute>();

		if ( minMaxAttribute is not null )
		{
			Min = minMaxAttribute.Min;
			Max = minMaxAttribute.Max;
		}
		else
		{
			Min = DefaultMin;
			Max = DefaultMax;
		}

		DisplayMode = propertyInfo.GetCustomAttribute<DisplayModeAttribute>()?.DisplayMode ?? DefaultDisplayMode;
	}

	/// <inheritdoc/>
	public sealed override void Draw()
	{
		if ( ReadOnly )
			ImGui.BeginDisabled();

		DrawInput();

		if ( ReadOnly )
			ImGui.EndDisabled();
	}

	/// <summary>
	/// Draws the input element.
	/// </summary>
	protected virtual void DrawInput()
	{
	}
}
