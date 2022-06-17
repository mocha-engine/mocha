﻿using ImGuiNET;

namespace Mocha.Engine;

internal class FilePreviewTab : BaseTab
{
	private BaseFileHandler FileHandler { get; set; }
	private byte[] FileData { get; set; }

	public FilePreviewTab( byte[] fileData, BaseFileHandler fileHandler )
	{
		this.FileData = fileData;
		this.FileHandler = fileHandler;

		visible = true;
	}

	public override void Draw()
	{
		base.Draw();

		if ( !visible )
			return;

		ImGui.SetNextWindowSizeConstraints( new System.Numerics.Vector2( 512, 512 ), new System.Numerics.Vector2( float.MaxValue, float.MaxValue ) );
		ImGui.Begin( $"File Preview##{GetHashCode()}", ref visible );

		FileHandler.Draw();

		ImGui.End();
	}
}
