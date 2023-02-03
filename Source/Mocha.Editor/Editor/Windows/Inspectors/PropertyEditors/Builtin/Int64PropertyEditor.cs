using System.Reflection;

namespace Mocha.Editor;

/// <summary>
/// A property editor capable of handling a <see cref="long"/>.
/// </summary>
[PropertyEditor<long>]
public class Int64PropertyEditor : SimplePropertyEditor
{
	public Int64PropertyEditor( object containingObject, PropertyInfo propertyInfo, bool readOnly )
		: base( containingObject, propertyInfo, readOnly )
	{
	}

	/// <inheritdoc/>
	protected override void DrawInput()
	{
		var num = (int)GetValue<long>();
		switch ( DisplayMode )
		{
			case DisplayMode.Text:
				if ( ImGui.InputInt( $"##{FormattedPropertyName}", ref num ) && num >= Min && num <= Max )
					SetValue( (long)num );
				break;
			case DisplayMode.Drag:
				if ( ImGui.DragInt( $"##{FormattedPropertyName}", ref num, DefaultDragSpeed, Min, Max ) )
					SetValue( (long)num );
				break;
			case DisplayMode.Slider:
				if ( ImGui.SliderInt( $"##{FormattedPropertyName}", ref num, Min, Max ) )
					SetValue( (long)num );
				break;
		}
	}
}

/// <summary>
/// A property editor capable of handling a <see cref="ulong"/>.
/// </summary>
[PropertyEditor<ulong>]
public class UnsignedInt64PropertyEditor : SimplePropertyEditor
{
	public UnsignedInt64PropertyEditor( object containingObject, PropertyInfo propertyInfo, bool readOnly )
		: base( containingObject, propertyInfo, readOnly )
	{
	}

	/// <inheritdoc/>
	protected override void DrawInput()
	{
		var num = (int)GetValue<ulong>();
		switch ( DisplayMode )
		{
			case DisplayMode.Text:
				if ( ImGui.InputInt( FormattedPropertyName, ref num ) && num >= Min && num <= Max )
					SetValue( (ulong)num );
				break;
			case DisplayMode.Drag:
				if ( ImGui.DragInt( FormattedPropertyName, ref num, DefaultDragSpeed, Min, Max ) )
					SetValue( (ulong)num );
				break;
			case DisplayMode.Slider:
				if ( ImGui.SliderInt( FormattedPropertyName, ref num, Min, Max ) )
					SetValue( (ulong)num );
				break;
		}
	}
}
