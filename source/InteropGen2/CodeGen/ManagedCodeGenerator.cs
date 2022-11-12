using System.CodeDom.Compiler;

sealed class ManagedCodeGenerator : BaseCodeGenerator
{
	public ManagedCodeGenerator( List<IUnit> units ) : base( units )
	{
	}

	private List<string> GetUsings()
	{
		// return new() { "System.Runtime.InteropServices" };
		return new();
	}

	private string GetNamespace()
	{
		return "Mocha.Glue";
	}

	private void GenerateClassCode( ref IndentedTextWriter writer, Class sel )
	{
		//
		// Gather everything we need into nice lists
		//
		List<string> decls = new();
		List<string> defs = new();

		foreach ( var method in sel.Methods )
		{
			var returnType = Utils.GetManagedType( method.ReturnType );
			var name = method.Name;

			var parameterTypes = method.Parameters.Select( x => "IntPtr" ); // Everything gets passed as a pointer
			var paramAndReturnTypes = parameterTypes.Append( returnType );
			paramAndReturnTypes = paramAndReturnTypes.Prepend( "IntPtr" ); // Pointer to this class's instance

			var delegateTypeArguments = string.Join( ", ", paramAndReturnTypes );

			var delegateSignature = $"delegate* unmanaged< {delegateTypeArguments} >";

			//
			// With each method, we pass in a pointer to the instance, along with
			// any parameters. The return type is the last type argument passed to
			// the delegate.
			//
			decls.Add( $"private {delegateSignature} _{name};" );

			defs.Add( $"this._{name} = ({delegateSignature})args.__{sel.Name}_{method.Name}MethodPtr;" );
		}

		//
		// Write shit
		//
		writer.WriteLine( $"public unsafe class {sel.Name}" );
		writer.WriteLine( "{" );
		writer.Indent++;

		writer.WriteLine( "private IntPtr instance;" );
		writer.WriteLine( "public IntPtr NativePtr => instance;" );

		// Decls
		writer.WriteLine();
		foreach ( var decl in decls )
		{
			writer.WriteLine( decl );
		}
		writer.WriteLine();

		// Ctor
		var ctor = sel.Methods.First( x => x.IsConstructor );
		var managedCtorArgs = string.Join( ", ", ctor.Parameters.Select( x => $"{Utils.GetManagedType( x.Type )} {x.Name}" ) );

		writer.WriteLine( $"public {sel.Name}( {managedCtorArgs} )" );
		writer.WriteLine( "{" );
		writer.Indent++;

		writer.WriteLine( "var args = Mocha.Common.Global.UnmanagedArgs;" );
		writer.WriteLine();

		foreach ( var def in defs )
		{
			writer.WriteLine( def );
		}

		writer.WriteLine();

		var ctorCallArgs = string.Join( ", ", ctor.Parameters.Select( x => x.Name ) );
		writer.WriteLine( $"this.instance = this.Ctor( {ctorCallArgs} );" );

		writer.Indent--;
		writer.WriteLine( "}" );

		// Methods
		foreach ( var method in sel.Methods )
		{
			writer.WriteLine();

			var managedCallParams = string.Join( ", ", method.Parameters.Select( x => $"{Utils.GetManagedType( x.Type )} {x.Name}" ) );
			var name = method.Name;
			var returnType = Utils.GetManagedType( method.ReturnType );
			var accessLevel = (method.IsConstructor || method.IsDestructor) ? "private" : "public";

			writer.WriteLine( $"{accessLevel} {returnType} {name}( {managedCallParams} ) " );
			writer.WriteLine( "{" );
			writer.Indent++;

			var paramsAndInstance = method.Parameters.Prepend( new Variable( "instance", "IntPtr" ) );
			var paramNames = paramsAndInstance.Select( x => "InteropUtils.GetPtr( " + x.Name + " )" );
			var functionCallArgs = string.Join( ", ", paramNames );

			if ( returnType != "void" )
				writer.Write( "return " );

			writer.WriteLine( $"this._{name}( {functionCallArgs} );" );

			writer.Indent--;
			writer.WriteLine( "}" );
		}

		writer.Indent--;
		writer.WriteLine( "}" );
	}

	private void GenerateStructCode( ref IndentedTextWriter writer, Structure sel )
	{
		writer.WriteLine( $"[StructLayout( LayoutKind.Sequential )]" );
		writer.WriteLine( $"public struct {sel.Name}" );
		writer.WriteLine( "{" );
		writer.Indent++;

		foreach ( var field in sel.Fields )
		{
			writer.WriteLine( $"public {field};" );
		}

		writer.Indent--;
		writer.WriteLine( "}" );
	}

	public string GenerateManagedCode()
	{
		var (baseTextWriter, writer) = Utils.CreateWriter();

		writer.WriteLine( GetHeader() );
		writer.WriteLine();

		foreach ( var usingStatement in GetUsings() )
			writer.WriteLine( $"using {usingStatement};" );

		writer.WriteLine();
		writer.WriteLine( $"namespace {GetNamespace()};" );
		writer.WriteLine();

		foreach ( var unit in Units )
		{
			if ( unit is Class c )
			{
				GenerateClassCode( ref writer, c );
			}

			if ( unit is Structure s )
			{
				GenerateStructCode( ref writer, s );
			}

			writer.WriteLine();
		}

		return baseTextWriter.ToString();
	}
}
