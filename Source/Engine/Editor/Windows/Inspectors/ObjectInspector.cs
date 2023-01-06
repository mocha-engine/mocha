using System.Reflection;

namespace Mocha.Editor;

/// <summary>
/// An inspector capable of inspecting any C# object.
/// </summary>
[Inspector<object>]
public class ObjectInspector : BaseInspector
{
	private object obj;

	public ObjectInspector( object obj )
	{
		this.obj = obj;
	}

	public override void Draw()
	{
		var objType = obj.GetType();

		ImGuiX.InspectorTitle(
			objType.Name,
			objType.FullName ?? "Unknown full name",
			ResourceType.Default
		);

		var objProperties = objType.GetProperties()
			.Where( property => property.CanRead && property.GetCustomAttribute<HideInInspectorAttribute>() is null )
			.ToArray();
		var properties = new (string, string)[objProperties.Length];

		for ( var i = 0; i < properties.Length; i++ )
		{
			var property = objProperties[i];
			properties[i] = (property.Name, property.GetValue( obj )?.ToString() ?? "null");
		}

		DrawProperties( "Properties", properties, "Unknown" );
	}
}
