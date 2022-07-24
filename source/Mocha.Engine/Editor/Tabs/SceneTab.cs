using ImGuiNET;
using System.ComponentModel;
using System.Reflection;

namespace Mocha.Engine;

[EditorMenu( "Scene/Outliner" )]
internal class OutlinerTab : BaseTab
{
	public static OutlinerTab Instance { get; set; }

	internal Entity? selectedEntity;

	public OutlinerTab()
	{
		Instance = this;
		isVisible = true;
	}

	public override void Draw()
	{
		ImGui.Begin( "Outliner", ref isVisible );

		//
		// Hierarchy
		//
		{
			var groupedEntities = Entity.All.GroupBy( x => x.GetType().GetCustomAttribute<CategoryAttribute>() );

			foreach ( var group in groupedEntities )
			{
				if ( ImGui.CollapsingHeader( $"{group.Key?.Category ?? "Uncategorised"}" ) )
				{
					if ( ImGui.BeginTable( $"##table_entities", 1, ImGuiTableFlags.PadOuterX | ImGuiTableFlags.SizingStretchProp ) )
					{
						ImGui.TableSetupColumn( "Entity", ImGuiTableColumnFlags.WidthStretch, 1f );

						foreach ( var entity in group )
						{
							ImGui.TableNextRow();
							ImGui.TableNextColumn();

							var str = $"{EditorHelpers.GetTypeIcon( entity.GetType() )} {entity.Name}";

							if ( ImGui.Selectable( str ) )
							{
								selectedEntity = entity;
							}
						}

						ImGui.EndTable();
					}
				}
			}
		}

		ImGui.End();
	}
}
