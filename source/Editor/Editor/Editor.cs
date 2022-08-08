namespace Mocha.Editor;

partial class Editor
{
	bool wasClicked = false;

	public void Render()
	{
		EditorUI.ShowDemoWindow();

		if ( EditorUI.Begin( "Test" ) )
		{
			EditorUI.TextHeading( $"{FontAwesome.Globe} Hello World!" );
			EditorUI.TextSubheading( $"{FontAwesome.Poo} Pee pee poo poo haha" );
			EditorUI.TextLight( $"{FontAwesome.FaceGrinStars} Light text :3" );

			EditorUI.Separator();

			EditorUI.Text( $"{FontAwesome.ThumbsUp} I am normal" );
			EditorUI.TextBold( $"{FontAwesome.WeightScale} I am bold" );
			EditorUI.TextMonospace( $"I am code :D" );

			EditorUI.Separator();

			if ( EditorUI.Button( wasClicked ? ":3" : "Click me!" ) )
			{
				wasClicked = !wasClicked;
			}

			EditorUI.End();
		}
	}
}

