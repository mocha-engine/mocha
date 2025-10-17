﻿using Mocha.UI;

namespace Mocha.Editor;

[Title( "Console" )]
public class ConsoleWindow : EditorWindow
{
	//
	// ImGUI input variables
	//
	private const int MaxInputLength = 512;
	private static string s_currentInput = "";

	/// <summary>
	/// Has the console just changed? If so, set this to true and
	/// we'll scroll to the bottom.
	/// </summary>
	private bool _isDirty = false;

	private static Vector4 LogLevelToColor( LogLevel level ) => level switch
	{
		LogLevel.Trace => Theme.Blue,
		LogLevel.Debug => Theme.Blue,
		LogLevel.Info => Theme.LightGray,
		LogLevel.Warning => Theme.Orange,
		LogLevel.Error => Theme.Red,
		LogLevel.Critical => Theme.Red,
		_ => Theme.LightGray,
	};

	private void DrawOutput()
	{
		if ( !ImGui.BeginChild( "##console_output", new Vector2( -1, -32 ) ) )
			return;

		if ( _isDirty )
		{
			ImGui.SetScrollY( ImGui.GetScrollMaxY() );

			_isDirty = false;
		}

		if ( ImGui.BeginTable( "##console_output_table", 3, ImGuiTableFlags.RowBg ) )
		{
			ImGui.TableSetupColumn( "Time", ImGuiTableColumnFlags.WidthFixed, 64.0f );
			ImGui.TableSetupColumn( "Logger", ImGuiTableColumnFlags.WidthFixed, 64.0f );
			ImGui.TableSetupColumn( "Text", ImGuiTableColumnFlags.WidthStretch, 1.0f );

			foreach ( NativeLogger.LogEntry item in Log.GetHistory() )
			{
				ImGui.TableNextRow();
				ImGui.TableNextColumn();

				static void ColoredText( string text, Vector4 color )
				{
					ImGui.PushStyleColor( ImGuiCol.Text, color );
					ImGui.TableSetBgColor( ImGuiTableBgTarget.CellBg, ImGui.GetColorU32( color.ToBackground() ), -1 );
					ImGuiX.TextMonospace( text );
					ImGui.PopStyleColor();
				}

				ColoredText( item.time, Theme.Green );
				ImGui.TableNextColumn();

				ColoredText( item.logger, Theme.Blue );
				ImGui.TableNextColumn();

				var level = Enum.Parse<LogLevel>( item.level );
				var color = LogLevelToColor( level );
				ColoredText( item.message, color );
			}

			ImGui.EndTable();
		}

		ImGui.EndChild();
	}

	private void DrawInput()
	{
		ImGui.SetNextItemWidth( -68 );
		bool pressed = ImGui.InputText( "##console_input", ref s_currentInput, MaxInputLength, ImGuiInputTextFlags.EnterReturnsTrue );

		ImGui.SameLine();
		if ( ImGui.Button( "Submit" ) || pressed )
		{
			Log.Info( $"] {s_currentInput}" );

			ConsoleSystem.Run( s_currentInput );

			_isDirty = true;
			s_currentInput = "";
		}
	}

	private void DrawEntityList()
	{
		if ( ImGui.BeginTabItem( $"{FontAwesome.Ghost}" ) )
		{
			foreach ( var entity in BaseEntity.All )
			{
				if ( ImGui.Selectable( entity.Name ) )
					InspectorWindow.SetSelectedObject( entity );
			}

			ImGui.EndTabItem();
		}
	}

	private void DrawUIList()
	{
		if ( ImGui.BeginTabItem( $"{FontAwesome.VectorSquare}" ) )
		{
			void ShowNode( LayoutNode node )
			{
				if ( node.StyledNode.Node is ElementNode element )
				{
					var name = $"{element.Data}##{element.GetHashCode()}";

					if ( ImGui.TreeNodeEx( name, node.Children.Count == 0 ? ImGuiTreeNodeFlags.Leaf : ImGuiTreeNodeFlags.None ) )
					{
						foreach ( var child in node.Children.ToArray() )
						{
							ShowNode( child );
						}

						ImGui.TreePop();
					}
				}
				else
				{
					if ( ImGui.TreeNodeEx( "[Text Node]", ImGuiTreeNodeFlags.Leaf ) )
						ImGui.TreePop();
				}
			}

			ShowNode( UIManager.Instance.RootPanel );

			ImGui.EndTabItem();
		}
	}

	private void DrawOutliner()
	{
		if ( !ImGui.BeginChild( "##console_tabs_outer", new Vector2( -1, -1 ), false, ImGuiWindowFlags.AlwaysUseWindowPadding ) )
			return;

		if ( ImGui.BeginTabBar( "##console_tabs" ) )
		{
			DrawEntityList();
			DrawUIList();

			ImGui.EndTabBar();
		}

		ImGui.EndChild();
	}

	public override void Draw()
	{
		if ( ImGuiX.BeginWindow( "Console", ref isVisible ) )
		{
			if ( ImGui.BeginTable( "##console_output", 2, ImGuiTableFlags.Resizable ) )
			{
				ImGui.TableSetupColumn( "Entities", ImGuiTableColumnFlags.WidthFixed, 128.0f );
				ImGui.TableSetupColumn( "Console Output", ImGuiTableColumnFlags.WidthStretch );

				ImGui.TableNextRow();
				ImGui.TableNextColumn();

				DrawOutliner();

				ImGui.TableNextColumn();

				DrawOutput();
				DrawInput();

				ImGui.EndTable();
			}

			ImGui.End();
		}
	}
}
