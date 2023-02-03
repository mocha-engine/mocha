using System.Reflection;

namespace Mocha.Editor;

/// <summary>
/// A property editor capable of handling a <see cref="System.Numerics.Vector3"/> and <see cref="Vector3"/>.
/// </summary>
[PropertyEditor<System.Numerics.Vector3>]
[PropertyEditor<Vector3>]
public class Float3PropertyEditor : SimplePropertyEditor
{
	public Float3PropertyEditor( object containingObject, PropertyInfo propertyInfo, bool readOnly )
		: base( containingObject, propertyInfo, readOnly )
	{
	}

	/// <inheritdoc/>
	protected override void DrawInput()
	{
		var float3 = GetVector();

		switch ( DisplayMode )
		{
			case DisplayMode.Text:
				if ( !ImGui.InputFloat3( $"##{FormattedPropertyName}", ref float3 ) )
					break;

				if ( float3.X < Min || float3.X > Max ||
					float3.Y < Min || float3.Y > Max ||
					float3.Z < Min || float3.Z > Max )
					break;

				SetVector( float3 );
				break;
			case DisplayMode.Drag:
				if ( ImGui.DragFloat3( $"##{FormattedPropertyName}", ref float3, DefaultDragSpeed, Min, Max ) )
					SetVector( float3 );
				break;
			case DisplayMode.Slider:
				if ( ImGui.SliderFloat3( $"##{FormattedPropertyName}", ref float3, Min, Max ) )
					SetVector( float3 );
				break;
		}
	}

	/// <summary>
	/// Gets a <see cref="System.Numerics.Vector3"/> that can be used in the input.
	/// </summary>
	/// <returns>A vector that ImGui can work with.</returns>
	private System.Numerics.Vector3 GetVector()
	{
		if ( Property.PropertyType == typeof( System.Numerics.Vector3 ) )
			return GetValue<System.Numerics.Vector3>();
		else
			return GetValue<Vector3>();
	}

	/// <summary>
	/// Sets the property value from a <see cref="System.Numerics.Vector3"/>.
	/// </summary>
	/// <param name="vector3">The vector to set in the property.</param>
	private void SetVector( System.Numerics.Vector3 vector3 )
	{
		if ( Property.PropertyType == typeof( System.Numerics.Vector3 ) )
			SetValue( vector3 );
		else
			SetValue<Vector3>( vector3 );
	}
}
