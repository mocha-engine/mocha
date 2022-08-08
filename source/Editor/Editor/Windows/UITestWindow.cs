namespace Mocha.Editor;

[Icon( FontAwesome.Vial ), Title( "UI Test Window" ), Category( "Engine" )]
internal class UITestWindow : BaseEditorWindow
{
	bool wasClicked = false;

	public override void Render()
	{
		if ( EditorUI.Begin( "Test" ) )
		{
			EditorUI.Title(
				$"{FontAwesome.FaceSurprise} This is a title!",
				$"Blah blah blah boring text describing stuff here."
			);

			EditorUI.TextHeading( $"{FontAwesome.Globe} I am a heading" );
			EditorUI.TextSubheading( $"{FontAwesome.Poo} I am a subheading" );
			EditorUI.TextLight( $"{FontAwesome.FaceGrinStars} I am light text :3" );

			EditorUI.Text( $"{FontAwesome.ThumbsUp} I am normal" );
			EditorUI.TextBold( $"{FontAwesome.WeightScale} I am bold" );
			EditorUI.TextMonospace( $"I am code :D" );

			EditorUI.Separator();

			if ( EditorUI.Button( wasClicked ? ":3" : "Click me!" ) )
			{
				wasClicked = !wasClicked;
			}
		}

		EditorUI.End();
	}
}
