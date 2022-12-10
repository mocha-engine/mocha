using System.CodeDom.Compiler;

sealed class ManagedCodeGenerator : BaseCodeGenerator
{
	public ManagedCodeGenerator( List<IUnit> units ) : base( units )
	{
	}

	private List<string> GetUsings()
	{
		return new() { "System.Runtime.InteropServices" };
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

			if ( returnType == "string" )
				returnType = "IntPtr"; // Strings are handled specially - they go from pointer to string using InteropUtils.GetString

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

			if ( returnType == "string" )
				writer.Write( "InteropUtils.GetString( " );

			writer.Write( $"this._{name}( {functionCallArgs} )" );

			if ( returnType == "string" )
				writer.Write( ")" );

			writer.WriteLine( ";" );

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
			writer.WriteLine( $"public {Utils.GetManagedType( field.Type )} {field.Name};" );
		}

		writer.Indent--;
		writer.WriteLine( "}" );
	}

	private void GenerateNamespaceCode( ref IndentedTextWriter writer, Class sel )
	{
		//
		// Gather everything we need into nice lists
		//
		List<string> decls = new();

		foreach ( var method in sel.Methods )
		{
			var returnType = Utils.GetManagedType( method.ReturnType );
			var name = method.Name;

			if ( returnType == "string" )
				returnType = "IntPtr"; // Strings are handled specially - they go from pointer to string using InteropUtils.GetString

			var parameterTypes = method.Parameters.Select( x => "IntPtr" ); // Everything gets passed as a pointer
			var paramAndReturnTypes = parameterTypes.Append( returnType );

			var delegateTypeArguments = string.Join( ", ", paramAndReturnTypes );

			var delegateSignature = $"delegate* unmanaged< {delegateTypeArguments} >";

			//
			// With each method, we pass in a pointer to the instance, along with
			// any parameters. The return type is the last type argument passed to
			// the delegate.
			//
			decls.Add( $"private static {delegateSignature} _{name} = ({delegateSignature})Mocha.Common.Global.UnmanagedArgs.__{sel.Name}_{name}MethodPtr;" );
		}

		//
		// Write shit
		//
		writer.WriteLine( $"public static unsafe class {sel.Name}" );
		writer.WriteLine( "{" );
		writer.Indent++;

		writer.WriteLine();
		foreach ( var decl in decls )
		{
			writer.WriteLine( decl );
		}

		// Methods
		foreach ( var method in sel.Methods )
		{
			writer.WriteLine();

			var managedCallParams = string.Join( ", ", method.Parameters.Select( x => $"{Utils.GetManagedType( x.Type )} {x.Name}" ) );
			var name = method.Name;
			var returnType = Utils.GetManagedType( method.ReturnType );
			var accessLevel = (method.IsConstructor || method.IsDestructor) ? "private" : "public";

			writer.WriteLine( $"{accessLevel} static {returnType} {name}( {managedCallParams} ) " );
			writer.WriteLine( "{" );
			writer.Indent++;

			var @params = method.Parameters;
			var paramNames = @params.Select( x => "InteropUtils.GetPtr( " + x.Name + " )" );
			var functionCallArgs = string.Join( ", ", paramNames );

			if ( returnType != "void" )
				writer.Write( "return " );

			if ( returnType == "string" )
				writer.Write( "InteropUtils.GetString( " );

			writer.Write( $"_{name}( {functionCallArgs} )" );

			if ( returnType == "string" )
				writer.Write( " )" );

			writer.WriteLine( ";" );

			writer.Indent--;
			writer.WriteLine( "}" );
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
				if ( c.IsNamespace )
					GenerateNamespaceCode( ref writer, c );
				else
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
