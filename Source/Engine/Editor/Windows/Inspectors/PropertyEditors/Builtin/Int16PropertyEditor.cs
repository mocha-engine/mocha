using System.Reflection;

namespace Mocha.Editor;

/// <summary>
/// A property editor capable of handling a <see cref="short"/>.
/// </summary>
[PropertyEditor<short>]
public class Int16PropertyEditor : SimplePropertyEditor
{
	public Int16PropertyEditor( object containingObject, PropertyInfo propertyInfo, bool readOnly )
		: base( containingObject, propertyInfo, readOnly )
	{
	}

	/// <inheritdoc/>
	protected override void DrawInput()
	{
		var num = (int)GetValue<short>();
		switch ( DisplayMode )
		{
			case DisplayMode.Text:
				if ( ImGui.InputInt( FormattedPropertyName, ref num ) && num >= Min && num <= Max )
					SetValue( (short)num );
				break;
			case DisplayMode.Drag:
				if ( ImGui.DragInt( FormattedPropertyName, ref num, DefaultDragSpeed, Min, Max ) )
					SetValue( (short)num );
				break;
			case DisplayMode.Slider:
				if ( ImGui.SliderInt( FormattedPropertyName, ref num, Min, Max ) )
					SetValue( (short)num );
				break;
		}
	}
}

/// <summary>
/// A property editor capable of handling a <see cref="ushort"/>.
/// </summary>
[PropertyEditor<ushort>]
public class UnsignedInt16PropertyEditor : SimplePropertyEditor
{
	public UnsignedInt16PropertyEditor( object containingObject, PropertyInfo propertyInfo, bool readOnly )
		: base( containingObject, propertyInfo, readOnly )
	{
	}

	/// <inheritdoc/>
	protected override void DrawInput()
	{
		var num = (int)GetValue<ushort>();
		switch ( DisplayMode )
		{
			case DisplayMode.Text:
				if ( ImGui.InputInt( FormattedPropertyName, ref num ) && num >= Min && num <= Max )
					SetValue( (ushort)num );
				break;
			case DisplayMode.Drag:
				if ( ImGui.DragInt( FormattedPropertyName, ref num, DefaultDragSpeed, Min, Max ) )
					SetValue( (ushort)num );
				break;
			case DisplayMode.Slider:
				if ( ImGui.SliderInt( FormattedPropertyName, ref num, Min, Max ) )
					SetValue( (ushort)num );
				break;
		}
	}
}
