global using Mocha;
global using Mocha.Common;
global using Mocha.UI;
global using System;
global using System.ComponentModel;

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
		var map = new ModelEntity( "core/models/dev/dev_map.mmdl" );
		map.SetMeshPhysics( "core/models/dev/dev_map.mmdl" );

		// Spawn a player
		var player = new Player();

		// Set testing values
		IntField = 0x0D06F00D;
		IntProperty = 0x0D06F00D;

		FloatField = 0.69420f;
		FloatProperty = 0.69420f;

		StringField = "Hello World!";
		StringProperty = "Hello World!";

		DateTimeField = DateTime.Now;
		DateTimeProperty = DateTime.Now;

		TimeSinceField = 0f;
		TimeSinceProperty = 0f;

		ArrayField = new int[] { 1, 2, 3, 4, 5 };
		ArrayProperty = new int[] { 1, 2, 3, 4, 5 };

		ClassField = new()
		{
			Hello = "World"
		};

		ClassProperty = new()
		{
			Hello = "World"
		};
	}

	//
	// These are assigned to in Startup(), which is only called when the
	// game starts
	//
	private int IntField;
	private float FloatField;
	private string StringField;
	private DateTime DateTimeField;
	private TimeSince TimeSinceField;

	private int[] ArrayField;

	private int IntProperty { get; set; }
	private float FloatProperty { get; set; }
	private string StringProperty { get; set; }
	private DateTime DateTimeProperty { get; set; }
	private TimeSince TimeSinceProperty { get; set; }

	private int[] ArrayProperty { get; set; }

	class TestClass
	{
		public string Hello;
	}

	private TestClass ClassField;
	private TestClass ClassProperty { get; set; }

	public override void Update()
	{
		base.Update();

		// These values are only set *once* but should persist between hot reloads
		DebugOverlay.ScreenText( $"IntField: {IntField}" );
		DebugOverlay.ScreenText( $"FloatField: {FloatField}" );
		DebugOverlay.ScreenText( $"StringField: {StringField}" );
		DebugOverlay.ScreenText( $"DateTimeField: {DateTimeField}" );
		DebugOverlay.ScreenText( $"TimeSinceField: {TimeSinceField}" );
		DebugOverlay.ScreenText( $"ClassField: {ClassField?.Hello}" );
		DebugOverlay.ScreenText( $"ArrayField: {string.Join( ", ", ArrayField ?? Array.Empty<int>() )}" );

		DebugOverlay.ScreenText( "--------------------------------------------------------------------------------" );

		DebugOverlay.ScreenText( $"IntProperty: {IntProperty}" );
		DebugOverlay.ScreenText( $"FloatProperty: {FloatProperty}" );
		DebugOverlay.ScreenText( $"StringProperty: {StringProperty}" );
		DebugOverlay.ScreenText( $"DateTimeProperty: {DateTimeProperty}" );
		DebugOverlay.ScreenText( $"TimeSinceProperty: {TimeSinceProperty}" );
		DebugOverlay.ScreenText( $"ClassProperty: {ClassProperty?.Hello}" );
		DebugOverlay.ScreenText( $"ArrayProperty: {string.Join( ", ", ArrayProperty ?? Array.Empty<int>() )}" );

		DebugOverlay.ScreenText( "--------------------------------------------------------------------------------" );

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

		DebugOverlay.ScreenText( $"Minimal assembly hash: {ClassProperty.GetType().Assembly.GetHashCode()}" );
	}
}
