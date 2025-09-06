namespace Mocha.Editor;

/// <summary>
/// Marks an inspector to be usable for <see ref="T"/>.
/// </summary>
/// <typeparam name="T">The type that the inspector can handle.</typeparam>
[AttributeUsage( AttributeTargets.Class, AllowMultiple = false, Inherited = false )]
public class InspectorAttribute<T> : Attribute
{
}
