namespace Mocha.Editor;

/// <summary>
/// Marks a property editor to be usable for <see cref="T"/>.
/// </summary>
/// <typeparam name="T">The type that the property editor can handle.</typeparam>
[AttributeUsage( AttributeTargets.Class, AllowMultiple = true, Inherited = false )]
public class PropertyEditorAttribute<T> : Attribute
{
}
