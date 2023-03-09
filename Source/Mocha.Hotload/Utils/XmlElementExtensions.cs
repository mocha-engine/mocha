using System.Xml;

namespace Mocha.Hotload.Util;

/// <summary>
/// A collection of extension methods for <see cref="XmlElement"/>s.
/// </summary>
internal static class XmlElementExtensions
{
	/// <summary>
	/// Creates a new <see cref="XmlElement"/> with the provided name as a child of the parent.
	/// </summary>
	/// <param name="parent">The node to own the created child.</param>
	/// <param name="elementName">The name of the new child element.</param>
	/// <returns>The newly created child of the parent.</returns>
	/// <exception cref="ArgumentNullException">Thrown when the parent provided is not owned by a <see cref="XmlDocument"/>.</exception>
	internal static XmlElement CreateElement( this XmlNode parent, string elementName )
	{
		if ( parent.OwnerDocument is null )
			throw new ArgumentNullException( nameof( parent ), "Expected parent to have an owning document" );

		var element = parent.OwnerDocument.CreateElement( elementName );
		parent.AppendChild( element );

		return element;
	}

	/// <summary>
	/// Creates a new <see cref="XmlElement"/> with the provided name as a child of the parent with its inner text set.
	/// </summary>
	/// <param name="parent">The node to own the created child.</param>
	/// <param name="elementName">The name of the new child element.</param>
	/// <param name="innerText">The inner text to set in the child.</param>
	/// <returns>The newly created child of the parent.</returns>
	/// <exception cref="ArgumentNullException">Thrown when the parent provided is not owned by a <see cref="XmlDocument"/>.</exception>
	internal static XmlElement CreateElementWithInnerText( this XmlNode parent, string elementName, string innerText )
	{
		if ( parent.OwnerDocument is null )
			throw new ArgumentNullException( nameof( parent ), "Expected parent to have an owning document" );

		var element = parent.OwnerDocument.CreateElement( elementName );
		element.InnerText = innerText;

		parent.AppendChild( element );
		return element;
	}

	/// <summary>
	/// Creates a new <see cref="XmlElement"/> with the provided name as a child of the parent with N attributes set.
	/// </summary>
	/// <param name="parent">The node to own the created child.</param>
	/// <param name="elementName">The name of the new child element.</param>
	/// <param name="attributesAndValues">A multiple of two length array containing an attribute name and its value.</param>
	/// <returns>The newly created child of the parent.</returns>
	/// <exception cref="ArgumentNullException">Thrown when the parent provided is not owned by a <see cref="XmlDocument"/>.</exception>
	/// <exception cref="ArgumentException">Thrown when the <see ref="attributesAndValues"/> are not a multiple of two.</exception>
	internal static XmlElement CreateElementWithAttributes( this XmlNode parent, string elementName, params string[] attributesAndValues )
	{
		if ( parent.OwnerDocument is null )
			throw new ArgumentNullException( nameof( parent ), "Expected parent to have an owning document" );

		if ( attributesAndValues.Length % 2 != 0 )
			throw new ArgumentException( "Expected a multiple of two length in strings", nameof( attributesAndValues ) );

		var element = parent.OwnerDocument.CreateElement( elementName );
		parent.AppendChild( element );

		if ( attributesAndValues.Length == 0 )
			return element;

		for ( var i = 0; i < attributesAndValues.Length; i += 2 )
		{
			var attribute = attributesAndValues[i];
			var value = attributesAndValues[i + 1];

			element.SetAttribute( attribute, value );
		}

		return element;
	}
}
