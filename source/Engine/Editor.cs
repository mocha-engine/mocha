﻿namespace Mocha;

using ImGui = Glue.Editor;

public class Editor
{
	const int MAX_INPUT_LENGTH = 512;
	static string consoleInput = "";
	static string editorInput = "";

	static bool drawPerformanceWindow = false;
	static bool drawEntityWindow = false;
	static bool drawConsoleWindow = false;
	static bool drawCameraWindow = false;
	static bool drawPhysicsTestWindow = false;
	static bool drawInputWindow = false;
	static bool drawPlayerWindow = false;
	static bool drawViewmodelWindow = false;

	public static void Draw()
	{
		DrawMenuBar();

		if ( drawPerformanceWindow )
			DrawPerformanceOverlay();

		if ( drawEntityWindow )
			DrawEntityWindow();

		if ( drawConsoleWindow )
			DrawConsoleWindow();

		if ( drawCameraWindow )
			DrawCameraWindow();

		if ( drawPhysicsTestWindow )
			DrawPhysicsTestWindow();

		if ( drawInputWindow )
			DrawInputWindow();

		if ( drawPlayerWindow )
			DrawPlayerWindow();

		if ( drawViewmodelWindow )
			DrawViewmodelWindow();
	}

	private static void DrawViewmodelWindow()
	{
		if ( ImGui.Begin( "Viewmodel" ) )
		{
			var player = Player.Local;
			var viewModel = player.ViewModel;

			void DrawOffset( ref ViewModelOffset offset )
			{
				offset.Position = ImGui.DragFloat3( $"Position##{offset.GetHashCode()}", offset.Position );
				offset.EulerRotation = ImGui.DragFloat3( $"Rotation##{offset.GetHashCode()}", offset.EulerRotation );
			}

			foreach ( var key in viewModel.Offsets.Keys )
			{
				var offset = viewModel.Offsets[key];

				if ( ImGui.CollapsingHeader( key ) )
				{
					var offsetCopy = offset;

					DrawOffset( ref offsetCopy );

					viewModel.Offsets[key] = offsetCopy;
				}
			}

		}

		ImGui.End();
	}

	private static void DrawMenuBar()
	{
		if ( ImGui.BeginMainMenuBar() )
		{
			if ( ImGui.BeginMenu( "Window" ) )
			{
				if ( ImGui.MenuItem( "Performance" ) )
					drawPerformanceWindow = !drawPerformanceWindow;

				if ( ImGui.MenuItem( "Entities" ) )
					drawEntityWindow = !drawEntityWindow;

				if ( ImGui.MenuItem( "Console" ) )
					drawConsoleWindow = !drawConsoleWindow;

				if ( ImGui.MenuItem( "Camera" ) )
					drawCameraWindow = !drawCameraWindow;

				if ( ImGui.MenuItem( "Physics Test" ) )
					drawPhysicsTestWindow = !drawPhysicsTestWindow;

				if ( ImGui.MenuItem( "Input" ) )
					drawInputWindow = !drawInputWindow;

				if ( ImGui.MenuItem( "Player" ) )
					drawPlayerWindow = !drawPlayerWindow;

				if ( ImGui.MenuItem( "Viewmodel" ) )
					drawViewmodelWindow = !drawViewmodelWindow;

				ImGui.EndMenu();
			}

			ImGui.RenderViewDropdown();
		}

		ImGui.EndMainMenuBar();
	}

	private static void DrawInputWindow()
	{
		if ( ImGui.Begin( "Input" ) )
		{
			ImGui.Text( $"Left click: {Input.Left}" );
			ImGui.Text( $"Right click: {Input.Right}" );

			ImGui.Text( $"Mouse position: {Input.MousePosition}" );
			ImGui.Text( $"Mouse delta: {Input.MouseDelta}" );

			ImGui.Text( $"Direction: {Input.Direction}" );
			ImGui.Text( $"Rotation: {Input.Rotation}" );
		}

		ImGui.End();
	}

	private static void DrawPlayerWindow()
	{
		if ( ImGui.Begin( "Player" ) )
		{
			if ( ImGui.Button( "Respawn Player" ) )
			{
				Player.Local.Respawn();
			}

			ImGui.Text( $"Player is grounded: {Player.Local.IsGrounded}" );
			ImGui.Text( $"Ground Entity: {Player.Local.GroundEntity?.Name ?? "None"}" );
			ImGui.Text( $"Player velocity: {Player.Local.Velocity}" );
			ImGui.Text( $"Player position: {Player.Local.Position}" );
		}

		ImGui.End();
	}

	private static void DrawPhysicsTestWindow()
	{
		if ( ImGui.Begin( "Physics Test" ) )
		{
			if ( ImGui.Button( "Raycast (log to console)" ) )
			{
				var localPlayer = Player.Local;

				var tr = Cast.Ray( localPlayer.EyeRay, 100f )
					.Ignore( localPlayer )
					.Run();

				Log.Info( $"{tr.StartPosition} -> {tr.EndPosition}" );
				Log.Info( $"Fraction {tr.Fraction}" );
				Log.Info( $"Hit? {tr.Hit}" );

				if ( tr.Hit )
					Log.Info( $"Normal {tr.Normal}" );
			}

			if ( ImGui.Button( "Spawn a ball at 0,0,10" ) )
			{
				var ball = new ModelEntity( "core/models/dev/dev_ball.mmdl" );
				ball.Position = new( 0, 0, 10f );
				ball.Restitution = 1.0f;
				ball.Friction = 1.0f;
				ball.Mass = 10.0f;
				ball.SetSpherePhysics( 0.5f, false );
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

	private static void DrawPerformanceOverlay()
	{
		if ( ImGui.BeginOverlay( "Time" ) )
		{
			var gpuName = ImGui.GetGPUName();

			ImGui.Text( $"GPU: {gpuName}" );

			float fps = 1.0f / Time.Delta;
			ImGui.Text( $"FPS: {fps.CeilToInt()}" );

			ImGui.Separator();

			ImGui.Text( $"Current time: {Time.Now}" );
			ImGui.Text( $"Frame time: {(Time.Delta * 1000f).CeilToInt()}ms" );

			ImGui.Separator();

			ImGui.Text( $"F10 to toggle cursor" );
		}

		ImGui.End();
	}
}
