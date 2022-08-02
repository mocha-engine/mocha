using ImGuiNET;
using Microsoft.Toolkit.HighPerformance.Buffers;
using System.Reflection;
using System.Reflection.Metadata;

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
		SetCursorPosRelative( padding );
	}

	public static bool ImageButton( Texture texture, Vector2 size )
	{
		var texPtr = Editor.Instance.Renderer.GetImGuiBinding( texture );

		return ImGui.ImageButton( texPtr, size,
			new System.Numerics.Vector2( 0, 0 ), new System.Numerics.Vector2( 1, 1 ) );
	}

	public static void Image( Texture texture, Vector2 size )
	{
		var texPtr = Editor.Instance.Renderer.GetImGuiBinding( texture );

		ImGui.Image( texPtr, size,
			new System.Numerics.Vector2( 0, 0 ), new System.Numerics.Vector2( 1, 1 ) );
	}

	public static void Image( Texture texture, Vector2 size, Vector4 tint )
	{
		var texPtr = Editor.Instance.Renderer.GetImGuiBinding( texture );

		ImGui.Image( texPtr, size,
			new System.Numerics.Vector2( 0, 0 ), new System.Numerics.Vector2( 1, 1 ),
			tint );
	}

	public static void Separator()
	{
		ImGui.Dummy( new( 0, 4 ) );
		ImGui.PushStyleColor( ImGuiCol.Separator, new System.Numerics.Vector4( 0.28f, 0.28f, 0.28f, 0.29f ) );
		ImGui.Separator();
		ImGui.PopStyleColor();
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

		float itemWidth = (ImGui.GetColumnWidth() / 3.0f) - 7f;

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
			ImGui.PopStyleColor( 2 );

			ImGui.SetNextItemWidth( dragFloatWidth );
			ImGui.DragFloat( $"##{v}_z", ref z );
		}

		ImGui.PopStyleVar();

		var vec3 = new Vector3( x, y, z );
		bool changed = !(sysVec3.X == vec3.X && sysVec3.Y == vec3.Y && sysVec3.Z == vec3.Z);

		if ( changed )
			sysVec3 = vec3;

		return changed;
	}

	public static void DockSpaceOverViewport()
	{
		var viewport = ImGui.GetMainViewport();

		ImGui.SetNextWindowPos( viewport.WorkPos );
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
			ImGui.DockSpace( dockspaceId, new System.Numerics.Vector2( 0, 0 ),
				ImGuiDockNodeFlags.PassthruCentralNode | ImGuiDockNodeFlags.AutoHideTabBar );

			ImGui.End();
		}

		var io = ImGui.GetIO();
		io.ConfigDockingAlwaysTabBar = true;

		ImGui.PopStyleVar( 3 );
	}

	public static void TextSubheading( string text )
	{
		ImGui.PushFont( Editor.SubheadingFont );
		ImGui.Dummy( new System.Numerics.Vector2( 0, 2 ) );
		ImGui.Text( text );
		ImGui.PopFont();
	}

	public static void TextLight( string text )
	{
		ImGui.PushStyleColor( ImGuiCol.Text, OneDark.Generic );
		ImGui.Dummy( new System.Numerics.Vector2( 0, 2 ) );
		ImGui.Text( text );
		ImGui.PopStyleColor();
	}

	public static void TextBold( string text )
	{
		ImGui.PushFont( Editor.BoldFont );
		ImGui.Dummy( new System.Numerics.Vector2( 0, 2 ) );
		ImGui.Text( text );
		ImGui.Dummy( new System.Numerics.Vector2( 0, 2 ) );
		ImGui.PopFont();
	}

	public static void Title( string text, string subtext )
	{
		TextSubheading( text );
		TextLight( subtext );

		Separator();
	}

	public static void SetCursorPosXRelative( float relativePos )
	{
		ImGui.SetCursorPosX( ImGui.GetCursorPosX() + relativePos );
	}

	public static void SetCursorPosYRelative( float relativePos )
	{
		ImGui.SetCursorPosY( ImGui.GetCursorPosY() + relativePos );
	}

	public static void SetCursorPosRelative( System.Numerics.Vector2 relativePos )
	{
		ImGui.SetCursorPos( ImGui.GetCursorPos() + relativePos );
	}

	public static List<string> MenusSubmittedThisFrame { get; } = new();

	public static bool BeginMenu( string name )
	{
		SetCursorPosXRelative( 4 );
		ImGui.SetNextWindowSize( new System.Numerics.Vector2( 250, -1 ) );
		bool result = ImGui.BeginMenu( name );

		return result;
	}

	public static bool MenuItem( string icon, string text, bool enabled = false )
	{
		SetCursorPosYRelative( -4 );

		var drawList = ImGui.GetForegroundDrawList();
		var windowPos = ImGui.GetWindowPos();
		var windowSize = ImGui.GetWindowSize();

		var padding = new System.Numerics.Vector2( 8, 8 );

		var size = new System.Numerics.Vector2( windowSize.X - (padding.X + 16), ImGui.CalcTextSize( text ).Y ) + padding;
		bool result = ImGui.InvisibleButton( $"##menu_{text}", size );
		SetCursorPosYRelative( -size.Y );

		var p0 = ImGui.GetCursorPos() + windowPos - new System.Numerics.Vector2( 0, 2 );
		var p1 = p0 + size + new System.Numerics.Vector2( 0, 4 );

		uint col = ImGui.GetColorU32( new System.Numerics.Vector4( 0, 0, 0, 0.1f ) );

		if ( ImGui.IsItemHovered() )
			drawList.AddRectFilled( p0, p1, col );

		SetCursorPosXRelative( padding.X * 0.5f );

		ImGui.PushFont( Editor.SubheadingFont );
		ImGui.Text( icon );
		ImGui.SameLine();
		ImGui.PopFont();

		SetCursorPosYRelative( 4 );
		ImGui.Text( text );

		if ( enabled )
		{
			SetCursorPosYRelative( -4 );
			ImGui.SameLine();
			ImGui.SetCursorPosX( 210 );
			ImGui.PushStyleColor( ImGuiCol.Text, new System.Numerics.Vector4( 1, 1, 1, 0.5f ) );

			ImGui.Text( FontAwesome.Check );
			ImGui.PopStyleColor();
		}

		if ( result )
		{
			ImGui.CloseCurrentPopup();
		}

		return result;
	}

	public static void EndMenu()
	{
		ImGui.EndMenu();
	}
}
