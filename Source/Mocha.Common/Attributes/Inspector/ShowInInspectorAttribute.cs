namespace Mocha.Common;

/// <summary>
/// Marks a property that would otherwise be hidden in the inspector to be shown instead.
/// </summary>
[AttributeUsage( AttributeTargets.Property, AllowMultiple = false, Inherited = true )]
public class ShowInInspectorAttribute : Attribute
{
}
