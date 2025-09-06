namespace Mocha.Common;

/// <summary>
/// We store all of the information required to call a C# method from a native
/// context here.
/// </summary>
public class CallbackDispatcher
{
	private static Dictionary<uint, CallbackInfo> _callbacks = new();
	private static uint _nextCallbackId = 0;

	public static uint RegisterCallback<T>( T callback ) where T : Delegate
	{
		var callbackId = _nextCallbackId++;
		_callbacks.Add( callbackId, new CallbackInfo( callback ) );

		return callbackId;
	}

	public static void Invoke( uint callbackId )
	{
		_callbacks[callbackId].Invoke();
	}

	public static void Invoke( uint callbackId, IntPtr arg )
	{
		_callbacks[callbackId].Invoke( arg );
	}
}
