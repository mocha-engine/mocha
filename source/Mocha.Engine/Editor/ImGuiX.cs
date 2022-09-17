using ImGuiNET;
using Mocha.Common;

namespace Mocha.Engine;

internal static class ImGuiX
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
			ImGui.PushStyleColor( ImGuiCol.Button, Colors.Red );
			ImGui.Button( $"X##{v}", new( buttonWidth, 0 ) );
			ImGui.SameLine();
			ImGui.PopStyleColor();

			ImGui.SetNextItemWidth( dragFloatWidth );
			ImGui.DragFloat( $"##{v}_x", ref x );
			ImGui.SameLine();
		}

		{
			ImGui.PushStyleColor( ImGuiCol.Button, Colors.Green );
			ImGui.Button( $"Y##{v}", new( buttonWidth, 0 ) );
			ImGui.SameLine();
			ImGui.PopStyleColor();

			ImGui.SetNextItemWidth( dragFloatWidth );
			ImGui.DragFloat( $"##{v}_y", ref y );
			ImGui.SameLine();
		}

		{
			ImGui.PushStyleColor( ImGuiCol.Button, Colors.Blue );
			ImGui.Button( $"Z##{v}", new( buttonWidth, 0 ) );
			ImGui.SameLine();
			ImGui.PopStyleColor();

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
		ImGui.PushStyleColor( ImGuiCol.Text, Colors.LightText );
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

	public static void InspectorTitle( string text, string subtext, FileType fileType )
	{
		var colorA = fileType.Color * 0.75f;
		var colorB = fileType.Color * 0.75f;
		colorB.W = 0.0f;

		var windowPos = ImGui.GetWindowPos();
		var windowWidth = ImGui.GetWindowWidth();

		var min = windowPos;
		var max = windowPos + new System.Numerics.Vector2( windowWidth, 72 );

		var drawList = ImGui.GetWindowDrawList();

		drawList.AddRectFilledMultiColor(
			min,
			max,
			ImGui.GetColorU32( colorA ),
			ImGui.GetColorU32( colorB ),
			ImGui.GetColorU32( colorB ),
			ImGui.GetColorU32( colorA )
		);

		min = windowPos + new System.Numerics.Vector2( 0, 72 );
		max = max + new System.Numerics.Vector2( 0, 32 );

		drawList.AddRectFilledMultiColor(
			min,
			max,
			ImGui.GetColorU32( colorA * 1.25f ),
			ImGui.GetColorU32( colorB * 1.25f ),
			ImGui.GetColorU32( colorB * 1.25f ),
			ImGui.GetColorU32( colorA * 1.25f )
		);

		var cursorPos = ImGui.GetCursorPos();
		SetCursorPosXRelative( 4 );
		SetCursorPosYRelative( 4 );
		Image( Texture.Builder.FromPath( fileType.IconLg ).Build(), new( 96 / 2.0f ) );

		ImGui.SetCursorPos( cursorPos );
		SetCursorPosXRelative( 96 / 2.0f );
		SetCursorPosXRelative( 8 );

		if ( ImGui.BeginChild( $"title##{text}{subtext}", new System.Numerics.Vector2( 0, 63 ), true, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoBackground ) )
		{
			TextSubheading( text );

			ImGui.Dummy( new System.Numerics.Vector2( 0, 0 ) );
			ImGui.Text( subtext );

			ImGui.EndChild();
		}
	}

	public static void Title( string text, string subtext, Vector4? _color = null, bool drawSubpanel = false )
	{
		TextSubheading( text );
		TextLight( subtext );

		ImGui.Dummy( new System.Numerics.Vector2( 0, 4 ) );
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

	public static bool GradientButton( string text )
	{
		//
		// This is so unbelievably shit, and so unbelievably
		// hacky, that I think I'm going to write my own GUI
		// solution that actually does support gradients
		// properly because I can't deal with this anymore.
		//

		var cursorPos = ImGui.GetCursorPos();
		ImGui.PushStyleColor( ImGuiCol.Button, Vector4.Zero );
		bool res = ImGui.Button( text );
		ImGui.PopStyleColor();

		var min = ImGui.GetItemRectMin();
		var max = ImGui.GetItemRectMax();

		var drawList = ImGui.GetWindowDrawList();

		drawList.AddRectFilledMultiColor(
			min,
			max,
			ImGui.GetColorU32( MathX.GetColor( "#727272" ) ),
			ImGui.GetColorU32( MathX.GetColor( "#727272" ) ),
			ImGui.GetColorU32( MathX.GetColor( "#333333" ) ),
			ImGui.GetColorU32( MathX.GetColor( "#333333" ) )
		);

		drawList.AddRect(
			min - new System.Numerics.Vector2( 2 ),
			max + new System.Numerics.Vector2( 2 ),
			ImGui.GetColorU32( ImGuiCol.WindowBg ),
			6,
			ImDrawFlags.None,
			3 // rounding - 1px
		);

		drawList.AddRect(
			min - new System.Numerics.Vector2( 0 ),
			max + new System.Numerics.Vector2( 1 ),
			ImGui.GetColorU32( Colors.DarkGray ),
			4,
			ImDrawFlags.None,
			1
		);

		ImGui.SetCursorPos( cursorPos );
		ImGui.PushStyleColor( ImGuiCol.Button, Vector4.Zero );
		ImGui.Button( text );
		ImGui.PopStyleColor();

		return res;
	}
}
