using System.Reflection;

namespace Mocha.Editor;

/// <summary>
/// A property editor capable of handling a <see cref="sbyte"/>.
/// </summary>
[PropertyEditor<sbyte>]
public class Int8PropertyEditor : SimplePropertyEditor
{
	public Int8PropertyEditor( object containingObject, PropertyInfo propertyInfo, bool readOnly )
		: base( containingObject, propertyInfo, readOnly )
	{
	}

	/// <inheritdoc/>
	protected override void DrawInput()
	{
		var num = (int)GetValue<sbyte>();
		switch ( DisplayMode )
		{
			case DisplayMode.Text:
				if ( ImGui.InputInt( FormattedPropertyName, ref num ) && num >= Min && num <= Max )
					SetValue( (sbyte)num );
				break;
			case DisplayMode.Drag:
				if ( ImGui.DragInt( FormattedPropertyName, ref num, DefaultDragSpeed, Min, Max ) )
					SetValue( (sbyte)num );
				break;
			case DisplayMode.Slider:
				if ( ImGui.SliderInt( FormattedPropertyName, ref num, Min, Max ) )
					SetValue( (sbyte)num );
				break;
		}
	}
}

/// <summary>
/// A property editor capable of handling a <see cref="byte"/>.
/// </summary>
[PropertyEditor<byte>]
public class UnsignedInt8PropertyEditor : SimplePropertyEditor
{
	public UnsignedInt8PropertyEditor( object containingObject, PropertyInfo propertyInfo, bool readOnly )
		: base( containingObject, propertyInfo, readOnly )
	{
	}

	/// <inheritdoc/>
	protected override void DrawInput()
	{
		var num = (int)GetValue<byte>();
		switch ( DisplayMode )
		{
			case DisplayMode.Text:
				if ( ImGui.InputInt( FormattedPropertyName, ref num ) && num >= Min && num <= Max )
					SetValue( (byte)num );
				break;
			case DisplayMode.Drag:
				if ( ImGui.DragInt( FormattedPropertyName, ref num, DefaultDragSpeed, Min, Max ) )
					SetValue( (byte)num );
				break;
			case DisplayMode.Slider:
				if ( ImGui.SliderInt( FormattedPropertyName, ref num, Min, Max ) )
					SetValue( (byte)num );
				break;
		}
	}
}
