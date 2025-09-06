using System.Reflection;

namespace Mocha.Editor;

/// <summary>
/// A property editor capable of handling a <see cref="float"/>.
/// </summary>
[PropertyEditor<float>]
public class FloatPropertyEditor : SimplePropertyEditor
{
	public FloatPropertyEditor( object containingObject, PropertyInfo propertyInfo, bool readOnly )
		: base( containingObject, propertyInfo, readOnly )
	{
	}

	/// <inheritdoc/>
	protected override void DrawInput()
	{
		var num = GetValue<float>();
		switch ( DisplayMode )
		{
			case DisplayMode.Text:
				if ( ImGui.InputFloat( $"##{FormattedPropertyName}", ref num ) && num >= Min && num <= Max )
					SetValue( num );
				break;
			case DisplayMode.Drag:
				if ( ImGui.DragFloat( $"##{FormattedPropertyName}", ref num, DefaultDragSpeed, Min, Max ) )
					SetValue( num );
				break;
			case DisplayMode.Slider:
				if ( ImGui.SliderFloat( $"##{FormattedPropertyName}", ref num, Min, Max ) )
					SetValue( num );
				break;
		}
	}
}
