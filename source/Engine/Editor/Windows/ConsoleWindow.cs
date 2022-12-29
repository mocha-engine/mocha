using Mocha.UI;

namespace Mocha.Editor;

[Title( "Console" )]
public class ConsoleWindow : EditorWindow
{
	private const int MAX_INPUT_LENGTH = 512;
	private static string consoleInput = "";
	private Vector4 graphColor = Theme.Green;

	private void DrawOutput()
	{
		if ( !ImGui.BeginChild( "##console_output", new Vector2( -1, -32 ) ) )
			return;

		if ( ImGui.BeginTable( "##console_output_table", 3, ImGuiTableFlags.Resizable | ImGuiTableFlags.RowBg ) )
		{
			ImGui.TableSetupColumn( "Time", ImGuiTableColumnFlags.WidthFixed, 64.0f );
			ImGui.TableSetupColumn( "Logger", ImGuiTableColumnFlags.WidthFixed, 64.0f );
			ImGui.TableSetupColumn( "Text", ImGuiTableColumnFlags.WidthStretch, 1.0f );

			foreach ( var item in Log.GetHistory() )
			{
				ImGui.TableNextRow();
				ImGui.TableNextColumn();

				ImGui.PushStyleColor( ImGuiCol.Text, Theme.Green );
				ImGui.TableSetBgColor( ImGuiTableBgTarget.CellBg, ImGui.GetColorU32( Theme.Green.ToBackground() ), -1 );
				ImGuiX.TextMonospace( item.time.ToString() );
				ImGui.PopStyleColor();

				ImGui.TableNextColumn();

				ImGui.PushStyleColor( ImGuiCol.Text, Theme.Blue );
				ImGui.TableSetBgColor( ImGuiTableBgTarget.CellBg, ImGui.GetColorU32( Theme.Blue.ToBackground() ), -1 );
				ImGuiX.TextMonospace( item.logger.ToString() );
				ImGui.PopStyleColor();

				ImGui.TableNextColumn();

				ImGuiX.TextMonospace( item.message );
			}

			ImGui.EndTable();
		}

		ImGui.EndChild();
	}

	private void DrawInput()
	{
		ImGui.SetNextItemWidth( -68 );
		bool pressed = ImGui.InputText( "##console_input", ref consoleInput, MAX_INPUT_LENGTH, ImGuiInputTextFlags.EnterReturnsTrue );

		ImGui.SameLine();
		if ( ImGui.Button( "Submit" ) || pressed )
		{
			Log.Trace( $"> {consoleInput}" );

			ConsoleSystem.Run( consoleInput );

			consoleInput = "";
		}
	}

	private void DrawPerformance()
	{
		var targetGraphColor = Theme.Green;

		if ( Time.FPS < 60 )
			targetGraphColor = Theme.Orange;

		if ( Time.FPS < 30 )
			targetGraphColor = Theme.Red;

		graphColor = Vector4.Lerp( graphColor, targetGraphColor, Time.Delta * 10f );

		// TODO:
		Glue.Editor.DrawGraph( "##performance", graphColor.ToBackground(), Time.FPSHistory.Select( x => (float)x ).ToList().ToInterop() );
	}

	private void DrawEntityList()
	{
		if ( ImGui.BeginTabItem( $"{FontAwesome.Ghost}" ) )
		{
			foreach ( var entity in BaseEntity.All )
			{
				ImGui.Selectable( entity.Name );
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

				DrawPerformance();
				DrawOutliner();

				ImGui.TableNextColumn();

				DrawOutput();
				DrawInput();

				ImGui.EndTable();
			}
		}

		ImGui.End();
	}
}
