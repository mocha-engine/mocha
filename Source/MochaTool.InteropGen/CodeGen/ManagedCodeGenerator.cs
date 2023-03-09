using System.CodeDom.Compiler;
using System.Collections.Immutable;

namespace MochaTool.InteropGen;

sealed class ManagedCodeGenerator : BaseCodeGenerator
{
	public ManagedCodeGenerator( List<IUnit> units ) : base( units )
	{
	}

	private List<string> GetUsings()
	{
		return new() { "System.Runtime.InteropServices", "System.Runtime.Serialization", "Mocha.Common" };
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

		foreach ( var method in sel.Methods )
		{
			var returnType = Utils.GetManagedType( method.ReturnType );
			var name = method.Name;

			var returnsPointer = Utils.IsPointer( method.ReturnType ) && !method.IsConstructor && !method.IsDestructor;

			if ( returnsPointer )
				returnType = "IntPtr";

			if ( returnType == "string" )
				returnType = "IntPtr"; // Strings are handled specially - they go from pointer to string using InteropUtils.GetString

			if ( method.IsConstructor || method.IsDestructor )
				returnType = "IntPtr"; // Ctor/dtor handled specially too

			var parameterTypes = method.Parameters.Select( x => "IntPtr" ); // Everything gets passed as a pointer
			var paramAndReturnTypes = parameterTypes.Append( returnType );

			if ( !method.IsStatic )
				paramAndReturnTypes = paramAndReturnTypes.Prepend( "IntPtr" ); // Pointer to this class's instance

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
		writer.WriteLine( $"public unsafe class {sel.Name} : INativeGlue" );
		writer.WriteLine( "{" );
		writer.Indent++;

		writer.WriteLine( "public IntPtr NativePtr { get; set; }" );

		// Decls
		writer.WriteLine();
		foreach ( var decl in decls )
		{
			writer.WriteLine( decl );
		}
		writer.WriteLine();

		// Ctor
		if ( sel.Methods.Any( x => x.IsConstructor ) )
		{
			var ctor = sel.Methods.First( x => x.IsConstructor );
			var managedCtorArgs = string.Join( ", ", ctor.Parameters.Select( x => $"{Utils.GetManagedType( x.Type )} {x.Name}" ) );

			writer.WriteLine( $"public {sel.Name}( {managedCtorArgs} )" );
			writer.WriteLine( "{" );
			writer.Indent++;

			var ctorCallArgs = string.Join( ", ", ctor.Parameters.Select( x => x.Name ) );
			writer.WriteLine( $"this.NativePtr = this.Ctor( {ctorCallArgs} );" );

			writer.Indent--;
			writer.WriteLine( "}" );
		}

		// Methods
		foreach ( var method in sel.Methods )
		{
			writer.WriteLine();

			//
			// Gather function signature
			//
			// Call parameters as comma-separated string
			var managedCallParams = string.Join( ", ", method.Parameters.Select( x => $"{Utils.GetManagedType( x.Type )} {x.Name}" ) );
			var name = method.Name;

			// We return a pointer to the created object if it's a ctor/dtor, but otherwise we'll do auto-conversions to our managed types
			var returnType = (method.IsConstructor || method.IsDestructor) ? "IntPtr" : Utils.GetManagedType( method.ReturnType );

			var returnsPointer = Utils.IsPointer( method.ReturnType ) && !method.IsConstructor && !method.IsDestructor;

			// If this is a ctor or dtor, we don't want to be able to call the method manually
			var accessLevel = (method.IsConstructor || method.IsDestructor) ? "private" : "public";

			if ( method.IsStatic )
				accessLevel += " static";

			// Write function signature
			writer.WriteLine( $"{accessLevel} {returnType} {name}( {managedCallParams} ) " );
			writer.WriteLine( "{" );
			writer.Indent++;

			// Spin up a MemoryContext instance
			writer.WriteLine( $"using var ctx = new MemoryContext( \"{sel.Name}.{name}\" );" );

			//
			// Gather function body
			//
			var paramsAndInstance = method.Parameters;

			// We need to pass the instance in if this is not a static method
			if ( !method.IsStatic )
				paramsAndInstance = paramsAndInstance.Prepend( new Variable( "NativePtr", "IntPtr" ) ).ToImmutableArray();

			// Gather function call arguments. Make sure that we're passing in a pointer for everything
			var paramNames = paramsAndInstance.Select( x => "ctx.GetPtr( " + x.Name + " )" );

			// Function call arguments as comma-separated string
			var functionCallArgs = string.Join( ", ", paramNames );

			if ( returnsPointer )
			{
				// If we want to return a pointer:
				writer.WriteLine( $"var ptr = _{name}( {functionCallArgs} );" );
				writer.WriteLine( $"var obj = FormatterServices.GetUninitializedObject( typeof( {returnType} ) ) as {returnType};" );
				writer.WriteLine( $"obj.NativePtr = ptr;" );
				writer.WriteLine( $"return obj;" );
			}
			else
			{
				// If we want to return a value:
				if ( returnType != "void" )
					writer.Write( "return " );

				// This is a pretty dumb and HACKy way of handling strings
				if ( returnType == "string" )
					writer.Write( "ctx.GetString( " );

				// Call the function..
				writer.Write( $"_{name}( {functionCallArgs} )" );

				// Finish string
				if ( returnType == "string" )
					writer.Write( ")" );

				writer.WriteLine( ";" );
			}

			writer.Indent--;
			writer.WriteLine( "}" );
		}

		writer.Indent--;
		writer.WriteLine( "}" );
	}

	private void GenerateStructCode( ref IndentedTextWriter writer, Struct sel )
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

			var returnsPointer = Utils.IsPointer( method.ReturnType ) && !method.IsConstructor && !method.IsDestructor;

			if ( returnType == "string" || returnsPointer )
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
			var returnsPointer = Utils.IsPointer( method.ReturnType ) && !method.IsConstructor && !method.IsDestructor;

			writer.WriteLine( $"{accessLevel} static {returnType} {name}( {managedCallParams} ) " );
			writer.WriteLine( "{" );
			writer.Indent++;

			// Spin up a MemoryContext instance
			writer.WriteLine( $"using var ctx = new MemoryContext( \"{sel.Name}.{name}\" );" );

			var @params = method.Parameters;
			var paramNames = @params.Select( x => "ctx.GetPtr( " + x.Name + " )" );
			var functionCallArgs = string.Join( ", ", paramNames );

			if ( returnsPointer )
			{
				// If we want to return a pointer:
				writer.WriteLine( $"var ptr = _{name}( {functionCallArgs} );" );
				writer.WriteLine( $"var obj = FormatterServices.GetUninitializedObject( typeof( {returnType} ) ) as {returnType};" );
				writer.WriteLine( $"obj.instance = ptr;" );
				writer.WriteLine( $"return obj;" );
			}
			else
			{
				// If we want to return a value:
				if ( returnType != "void" )
					writer.Write( "return " );

				// This is a pretty dumb and HACKy way of handling strings
				if ( returnType == "string" )
					writer.Write( "ctx.GetString( " );

				// Call the function..
				writer.Write( $"_{name}( {functionCallArgs} )" );

				// Finish string
				if ( returnType == "string" )
					writer.Write( ")" );

				writer.WriteLine( ";" );
			}

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

			if ( unit is Struct s )
			{
				GenerateStructCode( ref writer, s );
			}

			writer.WriteLine();
		}

		return baseTextWriter.ToString();
	}
}
