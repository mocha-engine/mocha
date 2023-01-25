using System.Data;
using System.Reflection;

namespace Mocha.Editor;

/// <summary>
/// An inspector capable of inspecting any C# object.
/// </summary>
[Inspector<object>]
public class ObjectInspector : BaseInspector
{
	/// <summary>
	/// The type name of the object.
	/// </summary>
	private readonly string objectName;
	/// <summary>
	/// The fully qualified name of the object.
	/// </summary>
	private readonly string objectFullName;

	/// <summary>
	/// Dictionary containing all the categories of property editors.
	/// </summary>
	private readonly Dictionary<string, List<BasePropertyEditor>> propertyEditors;

	private const int paddingSize = 4;
	private static readonly string s_padding = new( ' ', paddingSize );

	public ObjectInspector( object obj )
	{
		var objType = obj.GetType();

		// Setup names.
		objectName = objType.Name;
		objectFullName = objType.FullName ?? "Unknown full name";

		// Get all inspectable properties.
		var publicProperties = objType.GetProperties( BindingFlags.Instance | BindingFlags.Public )
			.Where( property => property.CanRead && property.GetCustomAttribute<HideInInspectorAttribute>() is null );
		var nonPublicProperties = objType.GetProperties( BindingFlags.Instance | BindingFlags.NonPublic )
			.Where( property => property.CanRead && property.GetCustomAttribute<ShowInInspectorAttribute>() is not null );

		var properties = publicProperties.Concat( nonPublicProperties )
			.OrderBy( property => property.GetCustomAttribute<CategoryAttribute>()?.Category ?? "Uncategorized" )
			.ToArray();

		propertyEditors = new Dictionary<string, List<BasePropertyEditor>>();
		for ( var i = 0; i < properties.Length; i++ )
		{
			var property = properties[i];
			var category = property.GetCustomAttribute<CategoryAttribute>()?.Category ?? "Uncategorized";
			if ( !propertyEditors.ContainsKey( category ) )
				propertyEditors.Add( category, new List<BasePropertyEditor>() );

			// Found a suitable property editor to display it.
			if ( TryGetPropertyEditorType( property.PropertyType, out var propertyEditorType ) )
			{
				var readOnly = property.GetCustomAttribute<ReadOnlyInInspectorAttribute>() is not null;
				propertyEditors[category].Add( (BasePropertyEditor)Activator.CreateInstance( propertyEditorType, obj, property, readOnly )! );
			}
			// No suitable editor found, display it as a read only string of the value.
			else
				propertyEditors[category].Add( (BasePropertyEditor)Activator.CreateInstance( typeof( StringPropertyEditor ), obj, property, true )! );
		}
	}

	public override void Draw()
	{
		// Header.
		ImGuiX.InspectorTitle(
			objectName,
			objectFullName,
			ResourceType.Default
		);

		// Filler.
		DrawButtons( "Unknown" );
		ImGuiX.Separator();

		// Properties.
		if ( ImGui.BeginChild( "##properties", Vector2.Zero, false, ImGuiWindowFlags.AlwaysVerticalScrollbar | ImGuiWindowFlags.NoBackground ) )
		{
			foreach ( var (editorCategory, propertyEditors) in propertyEditors )
			{
				ImGuiX.TextBold( s_padding + editorCategory );
				ImGuiX.Separator();

				foreach ( var propertyEditor in propertyEditors )
					propertyEditor.Draw();

				ImGuiX.Separator();
			}
		}

		ImGui.EndChild();
	}
}
