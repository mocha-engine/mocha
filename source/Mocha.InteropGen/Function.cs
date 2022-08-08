namespace Mocha.InteropGen;

public struct Function
{
	public Class Class { get; set; } = default;
	public List<string> Flags { get; set; } = new();
	public VariableType Type { get; set; } = new( "void", "Unnamed" );
	public List<VariableType> Args { get; set; } = new();
	public bool IsDestructor { get; set; } = false;
	public bool IsConstructor { get; set; } = false;

	public List<VariableType> GetArgsWithInstance( Class @class )
	{
		var args = new List<VariableType>();

		if ( !IsConstructor && !@class.IsStatic )
			args.Add( new VariableType( $"{@class.Name}*", "instance" ) );

		args.AddRange( Args );

		return args;
	}

	public Function() { }

	public override string ToString()
	{
		return Type.Name;
	}
}
