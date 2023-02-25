using Mocha.Networking;
using MochaTool.AssetCompiler;

namespace Mocha.Tests;

[TestClass]
public class NetworkSerializerTests
{
	[TestMethod]
	public void TestArraySerialization()
	{
		Mocha.Common.Global.Log = new ConsoleLogger();

		var array = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
		var data = NetworkSerializer.Serialize( array );
		var result = NetworkSerializer.Deserialize<int[]>( data );
		Assert.IsTrue( array.SequenceEqual( result ) );
	}
}
