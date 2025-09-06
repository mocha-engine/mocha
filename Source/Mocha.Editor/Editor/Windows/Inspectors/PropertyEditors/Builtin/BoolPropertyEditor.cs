using System.Reflection;

namespace Mocha.Editor;

/// <summary>
/// A property editor capable of handling a <see cref="bool"/>.
/// </summary>
[PropertyEditor<bool>]
public class BoolPropertyEditor : SimplePropertyEditor
{
	public BoolPropertyEditor( object containingObject, PropertyInfo propertyInfo, bool readOnly )
		: base( containingObject, propertyInfo, readOnly )
	{
	}

	/// <inheritdoc/>
	protected override void DrawInput()
	{
		var boolean = GetValue<bool>();
		if ( ImGui.Checkbox( $"##FormattedPropertyName", ref boolean ) )
			SetValue( boolean );
	}
}
