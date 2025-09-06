namespace Mocha.Common;

/// <summary>
/// Marks a property to only be read from in the inspector.
/// </summary>
[AttributeUsage( AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true )]
public class ReadOnlyInInspectorAttribute : Attribute
{
}
