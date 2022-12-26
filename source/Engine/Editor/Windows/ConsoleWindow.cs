using Mocha.UI;

namespace Mocha.Editor;

[Title( "Console" )]
public class ConsoleWindow : EditorWindow
{
	const int MAX_INPUT_LENGTH = 512;
	static string consoleInput = "";
	private Vector4 graphColor = Theme.Green;

	public override void Draw()
	{
		if ( ImGui.Begin( "Console" ) )
		{
			if ( ImGui.BeginTable( "##console_output", 2, 0 ) )
			{
				ImGui.TableSetupFixedColumn( "Entities", 128.0f );
				ImGui.TableSetupStretchColumn( "Console Output" );

				ImGui.TableNextRow();
				ImGui.TableNextColumn();

				{
					var targetGraphColor = Theme.Green;

					if ( Time.FPS < 60 )
						targetGraphColor = Theme.Orange;

					if ( Time.FPS < 30 )
						targetGraphColor = Theme.Red;

					graphColor = Vector4.Lerp( graphColor, targetGraphColor, Time.Delta * 10f );

					if ( ImGui.BeginChild( "##console_list", -1, -1 ) )
					{
						ImGui.DrawGraph( "##performance", graphColor.ToBackground(), Time.FPSHistory.Select( x => (float)x ).ToList().ToInterop() );

						if ( ImGui.BeginTabBar( "##console_tabs" ) )
						{
							if ( ImGui.BeginTabItem( $"{FontAwesome.Ghost}" ) )
							{
								foreach ( var entity in BaseEntity.All )
								{
									ImGui.Selectable( entity.Name );
								}

								ImGui.EndTabItem();
							}

							if ( ImGui.BeginTabItem( $"{FontAwesome.VectorSquare}" ) )
							{
								void ShowNode( LayoutNode node )
								{
									if ( node.StyledNode.Node is ElementNode element )
									{
										var name = $"{element.Data}##{element.GetHashCode()}";

										if ( ImGui.TreeNode( name, node.Children.Count == 0 ) )
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
										if ( ImGui.TreeNode( "[Text Node]", true ) )
											ImGui.TreePop();
									}
								}

								ShowNode( UIManager.Instance.RootPanel );

								ImGui.EndTabItem();
							}

							ImGui.EndTabBar();
						}

						ImGui.EndChild();
					}
				}

				ImGui.TableNextColumn();

				{
					if ( ImGui.BeginChild( "##console_output", -1, -32 ) )
					{
						if ( ImGui.BeginTable( "##console_output_table", 3, 0 ) )
						{
							ImGui.TableSetupFixedColumn( "Time", 64.0f );
							ImGui.TableSetupFixedColumn( "Logger", 64.0f );
							ImGui.TableSetupStretchColumn( "Text" );

							foreach ( var item in Log.GetHistory() )
							{
								ImGui.TableNextRow();
								ImGui.TableNextColumn();

								ImGui.PushColor( ImGuiCol.Text, Theme.Green );
								ImGui.TableSetBgColor( ImGuiTableBgTarget.CellBg, Theme.Green.ToBackground(), -1 );
								ImGui.TextMonospace( item.time.ToString() );
								ImGui.PopColor();

								ImGui.TableNextColumn();

								ImGui.PushColor( ImGuiCol.Text, Theme.Blue );
								ImGui.TableSetBgColor( ImGuiTableBgTarget.CellBg, Theme.Blue.ToBackground(), -1 );
								ImGui.TextMonospace( item.logger.ToString() );
								ImGui.PopColor();

								ImGui.TableNextColumn();

								ImGui.TextMonospace( item.message );
							}

							ImGui.EndTable();
						}
					}

					ImGui.EndChild();

					ImGui.SetNextItemWidth( -68 );
					consoleInput = ImGui.InputText( "##console_input", consoleInput, MAX_INPUT_LENGTH );

					ImGui.SameLine();
					if ( ImGui.Button( "Submit" ) )
					{
						Log.Trace( $"> {consoleInput}" );

						ConsoleSystem.Run( consoleInput );

						consoleInput = "";
					}
				}

				ImGui.EndTable();
			}
		}

		ImGui.End();
	}
}
