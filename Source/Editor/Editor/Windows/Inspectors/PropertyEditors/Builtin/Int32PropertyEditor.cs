using System.Reflection;

namespace Mocha.Editor;

/// <summary>
/// A property editor capable of handling a <see cref="int"/>.
/// </summary>
[PropertyEditor<int>]
public class Int32PropertyEditor : SimplePropertyEditor
{
	public Int32PropertyEditor( object containingObject, PropertyInfo propertyInfo, bool readOnly )
		: base( containingObject, propertyInfo, readOnly )
	{
	}

	/// <inheritdoc/>
	protected override void DrawInput()
	{
		var num = GetValue<int>();
		switch ( DisplayMode )
		{
			case DisplayMode.Text:
				if ( ImGui.InputInt( FormattedPropertyName, ref num ) && num >= Min && num <= Max )
					SetValue( num );
				break;
			case DisplayMode.Drag:
				if ( ImGui.DragInt( FormattedPropertyName, ref num, DefaultDragSpeed, Min, Max ) )
					SetValue( num );
				break;
			case DisplayMode.Slider:
				if ( ImGui.SliderInt( FormattedPropertyName, ref num, Min, Max ) )
					SetValue( num );
				break;
		}
	}
}

/// <summary>
/// A property editor capable of handling a <see cref="uint"/>.
/// </summary>
[PropertyEditor<uint>]
public class UnsignedInt32PropertyEditor : SimplePropertyEditor
{
	public UnsignedInt32PropertyEditor( object containingObject, PropertyInfo propertyInfo, bool readOnly )
		: base( containingObject, propertyInfo, readOnly )
	{
	}

	/// <inheritdoc/>
	protected override void DrawInput()
	{
		var num = (int)GetValue<uint>();
		switch ( DisplayMode )
		{
			case DisplayMode.Text:
				if ( ImGui.InputInt( FormattedPropertyName, ref num ) && num >= Min && num <= Max )
					SetValue( (uint)num );
				break;
			case DisplayMode.Drag:
				if ( ImGui.DragInt( FormattedPropertyName, ref num, DefaultDragSpeed, Min, Max ) )
					SetValue( (uint)num );
				break;
			case DisplayMode.Slider:
				if ( ImGui.SliderInt( FormattedPropertyName, ref num, Min, Max ) )
					SetValue( (uint)num );
				break;
		}
	}
}
