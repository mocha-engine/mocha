//using Mocha.Glue;

//namespace Mocha.Editor;

//[Title( "Memory" )]
//public class MemoryWindow : EditorWindow
//{
//	public override void Draw()
//	{
//		if ( ImGui.Begin( "Memory" ) )
//		{
//			if ( ImGui.BeginChild( "##memory", -1, -1 ) )
//			{
//				ImGui.BeginTable( "##allocations_table", 4, 0 );

//				ImGui.TableSetupStretchColumn( "Name" );
//				ImGui.TableSetupFixedColumn( "Allocated", 32.0f );
//				ImGui.TableSetupFixedColumn( "Freed", 32.0f );
//				ImGui.TableSetupFixedColumn( "Dangling", 32.0f );
//				ImGui.TableHeaders();

//				foreach ( var item in MemoryLogger.Entries.ToList().OrderBy( x => -x.Value.Allocations ) )
//				{
//					var (allocated, freed) = item.Value;
//					var dangling = (allocated - freed);

//					ImGui.TableNextRow();
//					ImGui.TableNextColumn();

//					ImGui.Text( item.Key );

//					ImGui.TableNextColumn();

//					ImGui.Text( allocated.ToString() );

//					ImGui.TableNextColumn();

//					ImGui.Text( freed.ToString() );

//					ImGui.TableNextColumn();

//					ImGui.Text( dangling.ToString() );
//				}

//				ImGui.EndTable();
//				ImGui.EndChild();
//			}
//		}

//		ImGui.End();
//	}
//}
