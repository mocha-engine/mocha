using Mocha;
using Mocha.Common;

namespace Mocha.Tests;

[TestClass]
public class EntityTickTests
{
	[TestMethod]
	public void TestEntityImplementsTickInterface()
	{
		// Create a test actor instance
		var actor = new TestActor();

		// Verify it implements IActor correctly
		Assert.IsInstanceOfType(actor, typeof(IActor));

		// Verify it has both Update and FrameUpdate methods
		var updateMethod = actor.GetType().GetMethod("Update");
		var frameUpdateMethod = actor.GetType().GetMethod("FrameUpdate");

		Assert.IsNotNull(updateMethod);
		Assert.IsNotNull(frameUpdateMethod);
	}

	[TestMethod]
	public void TestIActorInterfaceHasBothMethods()
	{
		// Verify IActor interface has both required methods
		var iactorType = typeof(IActor);
		var updateMethod = iactorType.GetMethod("Update");
		var frameUpdateMethod = iactorType.GetMethod("FrameUpdate");

		Assert.IsNotNull(updateMethod, "IActor interface should have Update method");
		Assert.IsNotNull(frameUpdateMethod, "IActor interface should have FrameUpdate method");
	}
}

// Test actor for validating interface compliance
public class TestActor : Actor
{
	public bool UpdateCalled { get; private set; } = false;
	public bool FrameUpdateCalled { get; private set; } = false;

	public override void Update()
	{
		UpdateCalled = true;
		base.Update();
	}

	public override void FrameUpdate()
	{
		FrameUpdateCalled = true;
		base.FrameUpdate();
	}
}