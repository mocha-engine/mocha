using System.Reflection;
using System.Text.RegularExpressions;

namespace Mocha.Editor;

/// <summary>
/// An ImGui element that can read/write a property.
/// </summary>
public partial class BasePropertyEditor
{
	/// <summary>
	/// The object that owns the instance.
	/// </summary>
	protected object ContainingObject { get; }
	/// <summary>
	/// The property that the editor is displaying.
	/// </summary>
	protected PropertyInfo Property { get; }
	/// <summary>
	/// Whether or not the editor should be read only.
	/// </summary>
	protected bool ReadOnly { get; }
	/// <summary>
	/// The nicely formatted name of the property.
	/// </summary>
	protected string FormattedPropertyName { get; }

	/// <summary>
	/// The Regex responsible for creating the <see ref="FormattedPropertyName"/>.
	/// </summary>
	/// <see href="https://stackoverflow.com/a/3216204"/>
	[GeneratedRegex( "(?<=[A-Z])(?=[A-Z][a-z])|(?<=[^A-Z])(?=[A-Z])|(?<=[A-Za-z])(?=[^A-Za-z])" )]
	protected static partial Regex FormattedPropertyNameRegex();

	public BasePropertyEditor( object containingObject, PropertyInfo propertyInfo, bool readOnly )
	{
		ContainingObject = containingObject;
		Property = propertyInfo;
		ReadOnly = readOnly;

		// If we cannot write to it then it must be read only.
		if ( !propertyInfo.CanWrite )
			ReadOnly = true;

		if ( propertyInfo.GetCustomAttribute<TitleAttribute>() is TitleAttribute titleAttribute )
			FormattedPropertyName = titleAttribute.title;
		else
			FormattedPropertyName = FormattedPropertyNameRegex().Replace( propertyInfo.Name, " " );
	}

	/// <summary>
	/// Draws the ImGui element.
	/// </summary>
	public virtual void Draw()
	{
	}

	/// <summary>
	/// Gets the value of the property and casts it to <see ref="T"/>.
	/// </summary>
	/// <typeparam name="T">The type to cast the returned object into.</typeparam>
	/// <returns>The value of the property casted to <see ref="T"/>.</returns>
	protected T GetValue<T>()
	{
		return (T)Property.GetValue( ContainingObject )!;
	}

	/// <summary>
	/// Sets the value of the property.
	/// </summary>
	/// <typeparam name="T">The type of the passed value.</typeparam>
	/// <param name="value">The value to set in the property.</param>
	/// <exception cref="InvalidOperationException">Thrown when trying to set the value of the property when the element is read only.</exception>
	protected void SetValue<T>( T value )
	{
		if ( ReadOnly )
			throw new InvalidOperationException( "You cannot set the value of a property on a readonly editor" );

		Property.SetValue( ContainingObject, value );
	}
}
