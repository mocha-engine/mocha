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
	private readonly string _objectName;
	/// <summary>
	/// The fully qualified name of the object.
	/// </summary>
	private readonly string _objectFullName;

	/// <summary>
	/// Dictionary containing all the categories of property editors.
	/// </summary>
	private readonly Dictionary<string, List<BasePropertyEditor>> _propertyEditors;

	private const int PaddingSize = 4;
	private static readonly string s_padding = new( ' ', PaddingSize );

	public ObjectInspector( object obj )
	{
		var objType = obj.GetType();

		// Setup names.
		_objectName = objType.Name;
		_objectFullName = objType.FullName ?? "Unknown full name";

		// Get all inspectable properties.
		var publicProperties = objType.GetProperties( BindingFlags.Instance | BindingFlags.Public )
			.Where( property => property.CanRead && property.GetCustomAttribute<HideInInspectorAttribute>() is null );
		var nonPublicProperties = objType.GetProperties( BindingFlags.Instance | BindingFlags.NonPublic )
			.Where( property => property.CanRead && property.GetCustomAttribute<ShowInInspectorAttribute>() is not null );

		var properties = publicProperties.Concat( nonPublicProperties )
			.OrderBy( property => property.GetCustomAttribute<CategoryAttribute>()?.Category ?? "Uncategorized" )
			.ToArray();

		_propertyEditors = new Dictionary<string, List<BasePropertyEditor>>();
		for ( var i = 0; i < properties.Length; i++ )
		{
			var property = properties[i];
			var category = property.GetCustomAttribute<CategoryAttribute>()?.Category ?? "Uncategorized";
			if ( !_propertyEditors.ContainsKey( category ) )
				_propertyEditors.Add( category, new List<BasePropertyEditor>() );

			// Found a suitable property editor to display it.
			if ( TryGetPropertyEditorType( property.PropertyType, out var propertyEditorType ) )
			{
				var readOnly = property.GetCustomAttribute<ReadOnlyInInspectorAttribute>() is not null;
				_propertyEditors[category].Add( (BasePropertyEditor)Activator.CreateInstance( propertyEditorType, obj, property, readOnly )! );
			}
			// No suitable editor found, display it as a read only string of the value.
			else
				_propertyEditors[category].Add( (BasePropertyEditor)Activator.CreateInstance( typeof( StringPropertyEditor ), obj, property, true )! );
		}
	}

	public override void Draw()
	{
		// Header.
		ImGuiX.InspectorTitle(
			_objectName,
			_objectFullName,
			ResourceType.Default
		);

		// Filler.
		DrawButtons( "Unknown" );
		ImGuiX.Separator();

		// Properties.
		if ( ImGui.BeginChild( "##properties", Vector2.Zero, false, ImGuiWindowFlags.AlwaysVerticalScrollbar | ImGuiWindowFlags.NoBackground ) )
		{
			foreach ( var (editorCategory, propertyEditors) in _propertyEditors )
			{
				if ( ImGui.CollapsingHeader( editorCategory, ImGuiTreeNodeFlags.DefaultOpen ) )
				{
					if ( ImGui.BeginTable( $"##property_table_{propertyEditors.GetHashCode()}", 2, ImGuiTableFlags.PadOuterX ) )
					{
						ImGui.TableSetupColumn( "Name", ImGuiTableColumnFlags.WidthStretch, 1f );
						ImGui.TableSetupColumn( "Value", ImGuiTableColumnFlags.WidthStretch, 2f );

						ImGui.TableNextRow();
						ImGui.TableNextColumn();

						foreach ( var propertyEditor in propertyEditors )
						{
							ImGui.Text( propertyEditor.FormattedPropertyName );
							ImGui.TableNextColumn();

							ImGui.SetNextItemWidth( -1f );
							propertyEditor.Draw();
							ImGui.TableNextColumn();
						}

						ImGui.EndTable();
					}
				}
			}
		}

		ImGui.EndChild();
	}
}
