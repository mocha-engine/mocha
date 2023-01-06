namespace Mocha.Common;

/// <summary>
/// Marks an item to be hidden in the inspector window.
/// </summary>
[AttributeUsage( AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true )]
public class HideInInspectorAttribute : Attribute
{
}
