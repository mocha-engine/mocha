namespace MochaTool.AssetCompiler;

[AttributeUsage( AttributeTargets.Field | AttributeTargets.Property, Inherited = false, AllowMultiple = true )]
sealed class ShaderSectionAttribute : Attribute
{
	public readonly string SectionName;

	public ShaderSectionAttribute( string sectionName )
	{
		SectionName = sectionName;
	}
}
