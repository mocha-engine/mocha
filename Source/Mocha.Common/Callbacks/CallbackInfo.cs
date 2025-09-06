namespace Mocha.Common;

public readonly struct CallbackInfo
{
	public Delegate Callback { get; init; }

	public CallbackInfo( Delegate callback )
	{
		Callback = callback;
	}

	public void Invoke( IntPtr arg )
	{
		Callback.DynamicInvoke( arg );
	}

	public void Invoke()
	{
		Callback.DynamicInvoke();
	}
}
