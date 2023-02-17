namespace Mocha.Common;

public readonly struct CallbackInfo
{
	public Delegate Callback { get; init; }

	public CallbackInfo( Delegate callback )
	{
		Callback = callback;
	}

	public void Invoke( params object[] args )
	{
		Callback.DynamicInvoke( args );
	}

	public void Invoke()
	{
		Callback.DynamicInvoke();
	}
}
