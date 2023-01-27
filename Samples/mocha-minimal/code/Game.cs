global using Mocha;
global using Mocha.Common;
global using System;

namespace Minimal;

public class Game : BaseGame
{
	[HotloadSkip]
	private UIManager Hud { get; set; }

	public override void Startup()
	{
		// Set up UI
		Hud = new UIManager();
		Hud.SetTemplate( "ui/Game.html" );

		// Spawn a model to walk around in
		var map = new ModelEntity( "models/dev/dev_map.mmdl" );
		map.SetMeshPhysics( "models/dev/dev_map.mmdl" );

		// Spawn a player
		var player = new Player();

		_abstract = new Implementation
		{
			Field = 123,
			Property = "!olleH dlroW"
		};

		_interface = new TestImplementation();

		_foo = new();
		_bar = new();

		_hello = new()
		{
			World = "Earth"
		};

		_foo.hello = _hello;
		_bar.hello = _hello;

		_hello.World = "Mars";
	}

	public Abstract _abstract;
	public abstract class Abstract { }

	public class Implementation : Abstract
	{
		public int Field;
		public string Property { get; set; }
	}

	public interface ITest
	{
		public string TestProperty { get; set; }
	}

	public class TestImplementation : ITest
	{
		public string TestProperty { get; set; }
	}

	private ITest _interface;
	private string _testField;

	public class Hello
	{
		public string World { get; set; }
	}

	public class Foo
	{
		public Hello hello;
	}

	public class Bar
	{
		public Hello hello;
	}

	private Foo _foo;
	private Foo _bar;

	private Hello _hello;

	public override void Update()
	{
		base.Update();

		DebugOverlay.ScreenText( $"_abstract: {_abstract.GetType()}" );
		DebugOverlay.ScreenText( $"_interface: {_interface.GetType()}" );
		DebugOverlay.ScreenText( $"_testField: {_testField}" );

		DebugOverlay.ScreenText( $"_foo.hello: {_foo.hello.GetHashCode()}" );
		DebugOverlay.ScreenText( $"_bar.hello: {_bar.hello.GetHashCode()}" );
		DebugOverlay.ScreenText( $"_hello: {_hello.GetHashCode()}" );

		DebugOverlay.ScreenText( $"Player.Local.GetHashCode(): {Player.Local?.GetHashCode()}" );

		DebugOverlay.ScreenText( "Loaded Assemblies:" );
		foreach ( var assembly in AppDomain.CurrentDomain.GetAssemblies() )
		{
			if ( !assembly.FullName.Contains( "Mocha" ) && !assembly.FullName.Contains( "Minimal" ) )
				continue;

			DebugOverlay.ScreenText( $"- {assembly.FullName}, hash {assembly.GetHashCode()}" );
		}

		DebugOverlay.ScreenText( "--------------------------------------------------------------------------------" );
		DebugOverlay.ScreenText( "Entities:" );

		foreach ( var entity in BaseEntity.All )
		{
			DebugOverlay.ScreenText( $"- {entity.Name} from assembly {entity.GetType().Assembly.GetHashCode()}" );
		}

		DebugOverlay.ScreenText( "--------------------------------------------------------------------------------" );
		DebugOverlay.ScreenText( $"Minimal assembly hash: {GetType().Assembly.GetHashCode()}" );
	}
}
