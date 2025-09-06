namespace Mocha.UI;

[System.AttributeUsage( AttributeTargets.Property, Inherited = false, AllowMultiple = false )]
sealed class StylePropertyAttribute : Attribute
{
	public string CssName { get; }

	public StylePropertyAttribute( string cssName )
	{
		CssName = cssName;
	}
}

public class StyleValues
{
	//
	// Sizing
	//
	[StyleProperty( "width" )]
	public LengthValue Width { get; set; }

	[StyleProperty( "height" )]
	public LengthValue Height { get; set; }

	[StyleProperty( "max-width" )]
	public LengthValue MaxWidth { get; set; }

	[StyleProperty( "max-height" )]
	public LengthValue MaxHeight { get; set; }

	[StyleProperty( "min-width" )]
	public LengthValue MinWidth { get; set; }

	[StyleProperty( "min-height" )]
	public LengthValue MinHeight { get; set; }

	[StyleProperty( "position" )]
	public EnumValue Position { get; set; }

	[StyleProperty( "top" )]
	public LengthValue Top { get; set; }

	[StyleProperty( "bottom" )]
	public LengthValue Bottom { get; set; }

	[StyleProperty( "left" )]
	public LengthValue Left { get; set; }

	[StyleProperty( "right" )]
	public LengthValue Right { get; set; }

	//
	// Padding
	//
	[StyleProperty( "padding" )]
	public LengthValue Padding { get; set; }

	[StyleProperty( "padding-left" )]
	public LengthValue PaddingLeft { get; set; }

	[StyleProperty( "padding-right" )]
	public LengthValue PaddingRight { get; set; }

	[StyleProperty( "padding-top" )]
	public LengthValue PaddingTop { get; set; }

	[StyleProperty( "padding-bottom" )]
	public LengthValue PaddingBottom { get; set; }

	//
	// Margin
	//
	[StyleProperty( "margin" )]
	public LengthValue Margin { get; set; }

	[StyleProperty( "margin-left" )]
	public LengthValue MarginLeft { get; set; }

	[StyleProperty( "margin-right" )]
	public LengthValue MarginRight { get; set; }

	[StyleProperty( "margin-top" )]
	public LengthValue MarginTop { get; set; }

	[StyleProperty( "margin-bottom" )]
	public LengthValue MarginBottom { get; set; }

	//
	// Text
	//
	[StyleProperty( "font-family" )]
	public StringValue FontFamily { get; set; }
	[StyleProperty( "font-size" )]
	public LengthValue FontSize { get; set; }
	[StyleProperty( "font-weight" )]
	public LengthValue FontWeight { get; set; }

	//
	// Aesthetics
	//
	[StyleProperty( "border-radius" )]
	public LengthValue BorderRadius { get; set; }

	[StyleProperty( "display" )]
	public EnumValue Display { get; set; }

	[StyleProperty( "color" )]
	public ColorValue Color { get; set; }

	[StyleProperty( "background-color" )]
	public ColorValue BackgroundColor { get; set; }

	[StyleProperty( "background-image" )]
	public StringValue BackgroundImage { get; set; }

	//
	// Flex
	//
	[StyleProperty( "align-items" )]
	public EnumValue AlignItems { get; set; }

	[StyleProperty( "justify-content" )]
	public EnumValue JustifyContent { get; set; }

	[StyleProperty( "flex-direction" )]
	public EnumValue FlexDirection { get; set; }

	[StyleProperty( "flex-wrap" )]
	public EnumValue FlexWrap { get; set; }

	[StyleProperty( "aspect-ratio" )]
	public LengthValue AspectRatio { get; set; }

	[StyleProperty( "flex-grow" )]
	public LengthValue FlexGrow { get; set; }

	[StyleProperty( "flex-shrink" )]
	public LengthValue FlexShrink { get; set; }

	public StyleValues CombineWith( StyleValues other )
	{
		var result = this;
		foreach ( var property in typeof( StyleValues ).GetProperties() )
		{
			var value = property.GetValue( other );

			if ( value != null )
			{
				property.SetValue( result, value );
			}
		}

		return result;
	}
}
