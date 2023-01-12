using System.Reflection;

namespace Mocha.Editor;

/// <summary>
/// A property editor capable of handling a <see cref="System.Numerics.Vector2"/> and <see cref="Vector2"/>.
/// </summary>
[PropertyEditor<System.Numerics.Vector2>]
[PropertyEditor<Vector2>]
public class Float2PropertyEditor : SimplePropertyEditor
{
	public Float2PropertyEditor( object containingObject, PropertyInfo propertyInfo, bool readOnly )
		: base( containingObject, propertyInfo, readOnly )
	{
	}

	/// <inheritdoc/>
	protected override void DrawInput()
	{
		var float2 = GetVector();

		switch ( DisplayMode )
		{
			case DisplayMode.Text:
				if ( !ImGui.InputFloat2( FormattedPropertyName, ref float2 ) )
					break;

				if ( float2.X < Min || float2.X > Max ||
					float2.Y < Min || float2.Y > Max )
					break;

				SetVector( float2 );
				break;
			case DisplayMode.Drag:
				if ( ImGui.DragFloat2( FormattedPropertyName, ref float2, DefaultDragSpeed, Min, Max ) )
					SetVector( float2 );
				break;
			case DisplayMode.Slider:
				if ( ImGui.SliderFloat2( FormattedPropertyName, ref float2, Min, Max ) )
					SetVector( float2 );
				break;
		}
	}

	/// <summary>
	/// Gets a <see cref="System.Numerics.Vector2"/> that can be used in the input.
	/// </summary>
	/// <returns>A vector that ImGui can work with.</returns>
	private System.Numerics.Vector2 GetVector()
	{
		if ( Property.PropertyType == typeof( System.Numerics.Vector2 ) )
			return GetValue<System.Numerics.Vector2>();
		else
			return GetValue<Vector2>();
	}

	/// <summary>
	/// Sets the property value from a <see cref="System.Numerics.Vector2"/>.
	/// </summary>
	/// <param name="vector2">The vector to set in the property.</param>
	private void SetVector( System.Numerics.Vector2 vector2 )
	{
		if ( Property.PropertyType == typeof( System.Numerics.Vector2 ) )
			SetValue( vector2 );
		else
			SetValue<Vector2>( vector2 );
	}
}
