namespace Mocha;

using ImGui = Glue.Editor;

public class Editor
{
	static bool hasClicked = false;

	const int MAX_INPUT_LENGTH = 512;
	static string inputBuf = "";

	public static void Draw()
	{
		ImGui.ShowDemoWindow();

		DrawEntityWindow();
		DrawPerformanceWindow();
		DrawConsoleWindow();
	}

	private static void DrawConsoleWindow()
	{
		if ( ImGui.Begin( "Console" ) )
		{
			if ( ImGui.BeginChild( "##console_output" ) )
			{
				ImGui.BeginTable( "##console_output_table", 3, 0 );

				ImGui.TableSetupColumn( "Time", 0, 64.0f );
				ImGui.TableSetupColumn( "Logger", 0, 64.0f );
				ImGui.TableSetupColumn( "Text", 0, -1f );

				foreach ( var item in Log.GetHistory() )
				{
					ImGui.TableNextRow();
					ImGui.TableNextColumn();

					ImGui.Text( item.time.ToString() );

					ImGui.TableNextColumn();

					ImGui.Text( item.logger.ToString() );

					ImGui.TableNextColumn();

					ImGui.Text( item.message );
				}

				ImGui.EndTable();
				ImGui.EndChild();
			}

			ImGui.SetNextItemWidth( -60 );
			inputBuf = ImGui.InputText( "##console_input", inputBuf, MAX_INPUT_LENGTH );

			ImGui.SameLine();
			if ( ImGui.Button( "Submit" ) )
			{
				Log.Trace( $"> {inputBuf}" );
				inputBuf = "";
			}
		}

		ImGui.End();
	}

	private static void DrawEntityWindow()
	{
		if ( ImGui.Begin( "Entities" ) )
		{
			BaseEntity.All.ForEach( x =>
			{
				if ( !x.IsValid() )
					return;

				if ( ImGui.CollapsingHeader( x.Name ) )
				{
					ImGui.Text( $"Position: {x.Position}" );
					ImGui.Text( $"Rotation: {x.Rotation}" );
					ImGui.Text( $"Scale: {x.Scale}" );
				}
			} );
		}

		ImGui.End();
	}

	private static void DrawPerformanceWindow()
	{
		if ( ImGui.BeginOverlay( "Time" ) )
		{
			var gpuName = ImGui.GetGPUName();

			ImGui.Text( $"GPU: {gpuName}" );
			ImGui.Text( $"Current time: {Time.Now}" );
			ImGui.Text( $"Frame time: {(Time.Delta * 1000f).CeilToInt()}ms" );

			float fps = 1.0f / Time.Delta;
			ImGui.Text( $"FPS: {fps.CeilToInt()}" );
		}

		ImGui.End();
	}
}
