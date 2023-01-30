using System.Reflection;

namespace Mocha.Editor;

/// <summary>
/// A property editor capable of handling a <see cref="double"/>.
/// </summary>
[PropertyEditor<double>]
public class DoublePropertyEditor : SimplePropertyEditor
{
	public DoublePropertyEditor( object containingObject, PropertyInfo propertyInfo, bool readOnly )
		: base( containingObject, propertyInfo, readOnly )
	{
	}

	/// <inheritdoc/>
	protected override void DrawInput()
	{
		var num = GetValue<double>();
		if ( ImGui.InputDouble( FormattedPropertyName, ref num ) && num >= Min && num <= Max )
			SetValue( num );
	}
}
