using System.Reflection;

namespace Mocha.Editor;

/// <summary>
/// A property editor capable of handling a <see cref="string"/>.
/// </summary>
[PropertyEditor<string>]
public class StringPropertyEditor : SimplePropertyEditor
{
	/// <summary>
	/// The maximum string length the ImGui element can handle.
	/// </summary>
	protected const int MaxStringLength = 1024;

	public StringPropertyEditor( object containingObject, PropertyInfo propertyInfo, bool readOnly )
		: base( containingObject, propertyInfo, readOnly )
	{
		Max = Math.Clamp( Max, 1, MaxStringLength );
	}

	/// <inheritdoc/>
	protected override void DrawInput()
	{
		// Get an object since the property passed could be anything that didn't have a valid property editor.
		var obj = GetValue<object?>();

		if ( obj is string str )
		{
			if ( ImGui.InputText( $"##{FormattedPropertyName}", ref str, (uint)Max ) )
				SetValue( str );
		}
		else
		{
			var objStr = obj?.ToString() ?? "null";
			if ( ImGui.InputText( $"##{FormattedPropertyName}", ref objStr, (uint)Max ) )
				SetValue( objStr );
		}
	}
}
