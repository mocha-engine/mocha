using System.Runtime.InteropServices;

namespace Mocha.Common;

[StructLayout( LayoutKind.Sequential )]
public struct ProjectClass
{
	string defaultNamespace;
	bool nullable;
};

[StructLayout( LayoutKind.Sequential )]
public struct Properties
{
	long tickRate;
};

[StructLayout( LayoutKind.Sequential )]
public struct Resources
{
	string code;
	string content;
};

[StructLayout( LayoutKind.Sequential )]
public struct Project
{
	string name;
	string author;
	string version;
	string description;

	Resources resources;
	Properties properties;
	ProjectClass project;
};
