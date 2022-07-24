using ImGuiNET;
using System.Reflection;

namespace Mocha.Engine;

internal static class EditorHelpers
{
	public static string Align( string str ) => str.PadRight( 16, ' ' );

	public static void DrawColoredText( string str, System.Numerics.Vector4 col, bool align = true )
	{
		ImGui.PushStyleColor( ImGuiCol.Text, col );

		if ( align )
			str = Align( str );
		ImGui.Text( str );

		ImGui.PopStyleColor();
	}

	public static void ApplyPadding()
	{
		var padding = new System.Numerics.Vector2( 4, 2 );
		ImGui.SetCursorPos( ImGui.GetCursorPos() + padding );
	}

	public static void Image( Texture texture, Vector2 size )
	{
		var texPtr = Editor.Instance.Renderer.GetImGuiBinding( texture );

		ImGui.Image( texPtr, size,
			new System.Numerics.Vector2( 0, 0 ), new System.Numerics.Vector2( 1, 1 ) );
	}

	public static void Separator()
	{
		ImGui.Dummy( new( 0, 4 ) );
		ImGui.Separator();
		ImGui.Dummy( new( 0, 4 ) );
	}

	public static string GetDisplayName( string name )
	{
		string str = "";

		for ( int i = 0; i < name.Length; ++i )
		{
			char c = name[i];
			if ( i != 0 && char.IsUpper( c ) )
				str += " ";

			str += c;
		}

		return str;
	}

	public static bool Vector3Input( string v, ref System.Numerics.Vector3 sysVec3 )
	{
		float x = sysVec3.X;
		float y = sysVec3.Y;
		float z = sysVec3.Z;

		float itemWidth = ( ImGui.GetColumnWidth() / 3.0f ) - 7f;

		float buttonWidth = 5.0f;
		float dragFloatWidth = itemWidth - buttonWidth;

		ImGui.PushStyleVar( ImGuiStyleVar.ItemInnerSpacing, new System.Numerics.Vector2( 0, 0 ) );

		{
			ImGui.PushStyleColor( ImGuiCol.Text, OneDark.Background );
			ImGui.PushStyleColor( ImGuiCol.Button, OneDark.Error );
			ImGui.Button( $"X##{v}", new( buttonWidth, 0 ) );
			ImGui.SameLine();
			ImGui.PopStyleColor( 2 );

			ImGui.SetNextItemWidth( dragFloatWidth );
			ImGui.DragFloat( $"##{v}_x", ref x );
			ImGui.SameLine();
		}

		{
			ImGui.PushStyleColor( ImGuiCol.Text, OneDark.Background );
			ImGui.PushStyleColor( ImGuiCol.Button, OneDark.String );
			ImGui.Button( $"Y##{v}", new( buttonWidth, 0 ) );
			ImGui.SameLine();
			ImGui.PopStyleColor( 2 );

			ImGui.SetNextItemWidth( dragFloatWidth );
			ImGui.DragFloat( $"##{v}_y", ref y );
			ImGui.SameLine();
		}

		{
			ImGui.PushStyleColor( ImGuiCol.Text, OneDark.Background );
			ImGui.PushStyleColor( ImGuiCol.Button, OneDark.Info );
			ImGui.Button( $"Z##{v}", new( buttonWidth, 0 ) );
			ImGui.SameLine();
			ImGui.PopStyleColor(2);

			ImGui.SetNextItemWidth( dragFloatWidth );
			ImGui.DragFloat( $"##{v}_z", ref z );
		}

		ImGui.PopStyleVar();

		var vec3 = new Vector3( x, y, z );
		bool changed = !(sysVec3.X == vec3.X && sysVec3.Y  == vec3.Y && sysVec3.Z == vec3.Z);

		if ( changed )
			sysVec3 = vec3;

		return changed;
	}

	public static string GetTypeTitle( Type type )
	{
		var titleAttribute = type.GetCustomAttribute<TitleAttribute>();

		string str = "";
		if ( titleAttribute != null )
			str = titleAttribute.title;

		if ( string.IsNullOrEmpty( str ) )
			str = type.ToString();

		return str;
	}

	public static string GetTypeIcon( Type type )
	{
		var iconAttribute = type.GetCustomAttribute<IconAttribute>();

		if ( iconAttribute != null )
			return iconAttribute.icon;

		return "";
	}

	public static string GetTypeDisplayName( Type type )
	{
		var titleAttribute = type.GetCustomAttribute<TitleAttribute>();
		var iconAttribute = type.GetCustomAttribute<IconAttribute>();

		string str = "";
		if ( titleAttribute != null )
			str = titleAttribute.title;

		if ( iconAttribute != null )
			str = iconAttribute.icon + " " + str;

		if ( string.IsNullOrEmpty( str ) )
			str = type.ToString();

		return str;
	}

	public static void DockSpaceOverViewport()
	{
		var viewport = ImGui.GetMainViewport();

		ImGui.SetNextWindowPos( viewport.WorkPos);
		ImGui.SetNextWindowSize( viewport.WorkSize );
		ImGui.SetNextWindowViewport( viewport.ID );

		var flags =
			ImGuiWindowFlags.NoTitleBar |
			ImGuiWindowFlags.NoCollapse |
			ImGuiWindowFlags.NoResize |
			ImGuiWindowFlags.NoMove |
			ImGuiWindowFlags.NoDocking |
			ImGuiWindowFlags.NoBringToFrontOnFocus |
			ImGuiWindowFlags.NoNavFocus |
			ImGuiWindowFlags.NoBackground;

		ImGui.PushStyleVar( ImGuiStyleVar.WindowRounding, 0 );
		ImGui.PushStyleVar( ImGuiStyleVar.WindowBorderSize, 0 );
		ImGui.PushStyleVar( ImGuiStyleVar.WindowPadding, System.Numerics.Vector2.Zero );

		if ( ImGui.Begin( "DockSpaceViewport_main", flags ) )
		{
			var dockspaceId = ImGui.GetID( "DockSpace" );
			ImGui.DockSpace( dockspaceId, new System.Numerics.Vector2( 0, 0 ), ImGuiDockNodeFlags.PassthruCentralNode );
			ImGui.End();
		}

		ImGui.PopStyleVar( 3 );
	}
}
