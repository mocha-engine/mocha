using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Mocha.Editor;

[Icon( FontAwesome.MagnifyingGlass ), Title( "Inspector" ), Category( "Game" )]
public class InspectorWindow : EditorWindow
{
	private BaseInspector Inspector { get; set; }
	private static Dictionary<Type, Type> inspectorTypeCache = new();

	public static InspectorWindow Instance { get; set; }

	public InspectorWindow()
	{
		Instance = this;
	}

	public override void Draw()
	{
		if ( ImGuiX.BeginWindow( $"Inspector", ref isVisible ) )
		{
			Inspector?.Draw();
		}

		ImGui.End();
	}

	/// <summary>
	/// Attempts to get a suitable inspector for <see ref="objType"/>.
	/// </summary>
	/// <param name="objType">The type to find the inspector for.</param>
	/// <param name="type">The type of the inspector that was found suitable.</param>
	/// <returns>Whether or not a suitable inspector was found.</returns>
	private static bool TryGetInspector( Type objType, [NotNullWhen( true )] out Type? type )
	{
		if ( inspectorTypeCache.TryGetValue( objType, out var cachedType ) )
		{
			type = cachedType;
			return true;
		}

		var inspectorType = typeof( InspectorAttribute<> ).MakeGenericType( objType );
		// TODO: Search all assemblies, there could be custom inspectors laying around.
		var inspectors = Assembly.GetExecutingAssembly().GetTypes()
			.Where( type => type.IsAssignableTo( typeof( BaseInspector ) ) )
			.ToList();

		foreach ( var inspector in inspectors )
		{
			if ( inspector.GetCustomAttribute( inspectorType ) is null )
				continue;

			inspectorTypeCache.Add( objType, inspector );

			type = inspector;
			return true;
		}

		if ( objType.BaseType is not null )
		{
			var result = TryGetInspector( objType.BaseType, out var nestedInspectorType );
			if ( result )
				inspectorTypeCache.Add( objType, nestedInspectorType! );

			type = nestedInspectorType;
			return result;
		}

		type = null;
		return false;
	}

	/// <summary>
	/// Sets the active item to display in the inspector window.
	/// </summary>
	/// <param name="obj">The item to display.</param>
	public static void SetSelectedObject( object obj )
	{
		if ( !TryGetInspector( obj.GetType(), out var inspectorType ) )
		{
			Log.Warning( $"Failed to find an inspector for {obj}" );
			return;
		}

		Instance.Inspector = (BaseInspector)Activator.CreateInstance( inspectorType, obj )!;
	}
}
