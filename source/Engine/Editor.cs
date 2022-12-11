﻿namespace Mocha;

using ImGui = Glue.Editor;

public class Editor
{
	const int MAX_INPUT_LENGTH = 512;
	static string consoleInput = "";
	static string editorInput = "";

	public static void Draw()
	{
		ImGui.ShowDemoWindow();

		DrawEntityWindow();
		DrawPerformanceWindow();
		DrawConsoleWindow();
		DrawCameraWindow();
		DrawRaycastTestWindow();
	}

	private static void DrawRaycastTestWindow()
	{
		if ( ImGui.Begin( "Raycast Test" ) )
		{
			if ( ImGui.Button( "Raycast" ) )
			{
				var traceResult = Glue.Physics.TraceRay( Camera.Position, Vector3.Zero );

				Log.Info( $"{traceResult.startPosition} -> {traceResult.endPosition}" );
				Log.Info( $"Fraction {traceResult.fraction}" );
				Log.Info( $"Hit? {traceResult.hit}" );

				if ( traceResult.hit )
					Log.Info( $"Normal {traceResult.normal}" );
			}
		}

		ImGui.End();
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
				ImGui.TableSetupColumn( "Text", 0, 512.0f );

				foreach ( var item in Log.GetHistory() )
				{
					ImGui.TableNextRow();
					ImGui.TableNextColumn();

					ImGui.Text( item.time.ToString() );

					ImGui.TableNextColumn();

					ImGui.Text( item.logger.ToString() );

					ImGui.TableNextColumn();

					ImGui.TextWrapped( item.message );
				}

				ImGui.EndTable();
				ImGui.EndChild();
			}

			ImGui.SetNextItemWidth( -60 );
			consoleInput = ImGui.InputText( "##console_input", consoleInput, MAX_INPUT_LENGTH );

			ImGui.SameLine();
			if ( ImGui.Button( "Submit" ) )
			{
				Log.Trace( $"> {consoleInput}" );
				consoleInput = "";
			}
		}

		ImGui.End();
	}

	private static void DrawEntityWindow()
	{
		if ( ImGui.Begin( "Entities" ) )
		{
			ImGui.SetNextItemWidth( -1 );
			editorInput = ImGui.InputText( "##editor_input", editorInput, MAX_INPUT_LENGTH );

			BaseEntity.All.ForEach( x =>
			{
				if ( !x.IsValid() )
					return;

				if ( !string.IsNullOrEmpty( editorInput ) && !x.Name.Contains( editorInput, StringComparison.CurrentCultureIgnoreCase ) )
					return;

				if ( ImGui.CollapsingHeader( x.Name ) )
				{
					ImGui.Text( $"Position: {x.Position}" );
					ImGui.Text( $"Rotation: {x.Rotation}" );
					ImGui.Text( $"Scale: {x.Scale}" );

					if ( x is ModelEntity xM )
					{
						ImGui.Text( $"Velocity: {xM.Velocity}" );
					}
				}
			} );
		}

		ImGui.End();
	}

	private static void DrawCameraWindow()
	{
		if ( ImGui.Begin( "Camera" ) )
		{
			ImGui.Text( $"Position: {Camera.Position}" );
			ImGui.Text( $"Rotation: {Camera.Rotation}" );
			ImGui.Text( $"Field of View: {Camera.FieldOfView}" );
			ImGui.Text( $"ZNear: {Camera.ZNear}" );
			ImGui.Text( $"ZFar: {Camera.ZFar}" );
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
