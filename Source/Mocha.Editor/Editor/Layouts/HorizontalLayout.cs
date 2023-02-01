namespace Mocha.Editor;

enum LayoutAlignment
{
	Start,
	Center,
	End
}

internal class HorizontalLayout : IDisposable
{
	private Vector2 _childSize;
	private string _name;

	/// <summary>
	/// A list of child sizes for each layout, used for alignment.
	/// </summary>
	private static Dictionary<string, Vector2> _childSizes = new();

	public HorizontalLayout( string name, LayoutAlignment alignment = LayoutAlignment.Start )
	{
		_name = name;

		Vector2 targetSize;

		//
		// Did we calculate the size of all children last frame? If so, we can start doing
		// alignment calculations
		//
		if ( _childSizes.TryGetValue( name, out targetSize ) )
		{
			float availableWidth = ImGui.GetContentRegionAvail().X;

			if ( alignment == LayoutAlignment.Center )
			{
				float centerPos = availableWidth - targetSize.X;
				float bumpPos = centerPos / 2f;
				ImGuiX.BumpCursorX( bumpPos );
			}
			else if ( alignment == LayoutAlignment.End )
			{
				float centerPos = availableWidth - targetSize.X;
				float bumpPos = centerPos;
				ImGuiX.BumpCursorX( bumpPos );
			}
		}
		else
		{
			targetSize = new Vector2( 1, 1 );
		}

		ImGui.BeginChild( _name, targetSize, false, ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoBackground );
	}

	public void Add( Action widgetAction )
	{
		widgetAction.Invoke();
		Add( false );
	}

	public bool Add( bool widgetFunction )
	{
		ImGui.SameLine();

		_childSize += (Vector2)ImGui.GetItemRectSize();

		return widgetFunction;
	}

	public void Dispose()
	{
		ImGui.EndChild();

		float padding = 8;
		_childSizes[_name] = _childSize + new Vector2( padding, 0 );
	}
}
