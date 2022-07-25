using ImGuiNET;
using System.ComponentModel;
using System.Reflection;

namespace Mocha.Engine;

[EditorMenu( $"{FontAwesome.Globe} World/Outliner" )]
internal class OutlinerTab : BaseTab
{
	public static OutlinerTab Instance { get; set; }

	public OutlinerTab()
	{
		Instance = this;
		isVisible = true;
	}

	public void SelectItem( string name )
	{
		InspectorTab.SetSelectedObject( Entity.All.First( x => x.Name == name ) );
	}

	public override void Draw()
	{
		ImGui.Begin( "Outliner" );

		//
		// Hierarchy
		//
		{
			var groupedEntities = Entity.All.GroupBy( x => x.GetType().GetCustomAttribute<CategoryAttribute>() );

			EditorHelpers.Title(
				  $"{FontAwesome.Globe} World",
				"This is where all your entities live."
			);

			foreach ( var group in groupedEntities )
			{
				string icon = FontAwesome.Question;

				switch ( group.Key.Category )
				{
					case "Player":
						icon = FontAwesome.User;
						break;
					case "World":
						icon = FontAwesome.Globe;
						break;
				}

				EditorHelpers.TextBold( $"{icon} {group.Key?.Category ?? "Uncategorised"}" );

				{
					if ( ImGui.BeginTable( $"##table_entities", 2, ImGuiTableFlags.PadOuterX | ImGuiTableFlags.SizingStretchProp ) )
					{
						ImGui.TableSetupColumn( "Entity", ImGuiTableColumnFlags.WidthStretch, 1f );
						ImGui.TableSetupColumn( "Visibility", ImGuiTableColumnFlags.WidthFixed, 32 );

						foreach ( var entity in group )
						{
							ImGui.TableNextRow();
							ImGui.TableNextColumn();

							var str = $"{entity.Name}";

							if ( ImGui.Selectable( str ) )
							{
								SelectItem( str );
							}

							ImGui.TableNextColumn();

							ImGui.PushStyleVar( ImGuiStyleVar.FramePadding, new System.Numerics.Vector2( 4, 0 ) );
							ImGui.PushStyleColor( ImGuiCol.Button, System.Numerics.Vector4.Zero );

							ImGui.SmallButton( entity.Visible ? FontAwesome.Eye : FontAwesome.EyeSlash );

							ImGui.PopStyleColor();
							ImGui.PopStyleVar();
						}

						ImGui.EndTable();
					}
				}

				EditorHelpers.Separator();
			}
		}

		ImGui.End();
	}
}
