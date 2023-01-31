using System.Numerics;
using System.Reflection;

namespace Mocha.Editor;

/// <summary>
/// A property editor capable of handling a <see cref="Vector4"/>, <see cref="Quaternion"/>, and <see cref="Rotation"/>.
/// </summary>
[PropertyEditor<Vector4>]
[PropertyEditor<Quaternion>]
[PropertyEditor<Rotation>]
public class Float4PropertyEditor : SimplePropertyEditor
{
	public Float4PropertyEditor( object containingObject, PropertyInfo propertyInfo, bool readOnly )
		: base( containingObject, propertyInfo, readOnly )
	{
	}

	/// <inheritdoc/>
	protected override void DrawInput()
	{
		var float4 = GetVector();
		switch ( DisplayMode )
		{
			case DisplayMode.Text:
				if ( !ImGui.InputFloat4( $"##{FormattedPropertyName}", ref float4 ) )
					break;

				if ( float4.X < Min || float4.X > Max ||
					float4.Y < Min || float4.Y > Max ||
					float4.Z < Min || float4.Z > Max ||
					float4.W < Min || float4.Z > Max )
					break;

				SetVector( float4 );
				break;
			case DisplayMode.Drag:
				if ( ImGui.DragFloat4( $"##{FormattedPropertyName}", ref float4, DefaultDragSpeed, Min, Max ) )
					SetVector( float4 );
				break;
			case DisplayMode.Slider:
				if ( ImGui.SliderFloat4( $"##{FormattedPropertyName}", ref float4, Min, Max ) )
					SetVector( float4 );
				break;
		}
	}

	/// <summary>
	/// Gets a <see cref="Vector4"/> that can be used in the input.
	/// </summary>
	/// <returns>A vector that ImGui can work with.</returns>
	private Vector4 GetVector()
	{
		if ( Property.PropertyType == typeof( Vector4 ) )
			return GetValue<Vector4>();
		else if ( Property.PropertyType == typeof( Quaternion ) )
		{
			var quaternion = GetValue<Quaternion>();
			return new Vector4( quaternion.X, quaternion.Y, quaternion.Z, quaternion.W );
		}
		else
		{
			var rotation = GetValue<Rotation>();
			return new Vector4( rotation.X, rotation.Y, rotation.Z, rotation.W );
		}
	}

	/// <summary>
	/// Sets the property value from a <see cref="Vector4"/>.
	/// </summary>
	/// <param name="vector4">The vector to set in the property.</param>
	private void SetVector( Vector4 vector4 )
	{
		if ( Property.PropertyType == typeof( Vector4 ) )
			SetValue( vector4 );
		else if ( Property.PropertyType == typeof( Quaternion ) )
			SetValue( new Quaternion( vector4.X, vector4.Y, vector4.Z, vector4.W ) );
		else
			SetValue( new Rotation( vector4.X, vector4.Y, vector4.Z, vector4.W ) );
	}
}
