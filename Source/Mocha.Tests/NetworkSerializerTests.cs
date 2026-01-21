using Mocha.Common;
using Mocha.Networking;
using MochaTool.AssetCompiler;

namespace Mocha.Tests;

[TestClass]
public class NetworkSerializerTests
{
	[TestMethod( "Array Serialization" )]
	public void TestArraySerialization()
	{
		Global.Log = new ConsoleLogger();

		var array = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
		var data = NetworkSerializer.Serialize( array );
		var result = NetworkSerializer.Deserialize<int[]>( data );
		Assert.IsTrue( array.SequenceEqual( result ) );
	}

	[TestMethod( "List Serialization" )]
	public void TestListSerialization()
	{
		Global.Log = new ConsoleLogger();

		var list = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
		var data = NetworkSerializer.Serialize( list );
		var result = NetworkSerializer.Deserialize<List<int>>( data );
		Assert.IsTrue( list.SequenceEqual( result ) );
	}

	[TestMethod( "String Serialization" )]
	public void TestStringSerialization()
	{
		Global.Log = new ConsoleLogger();

		var str = "Hello World!";
		var data = NetworkSerializer.Serialize( str );
		var result = NetworkSerializer.Deserialize<string>( data );
		Assert.AreEqual( str, result );
	}

	[TestMethod( "Vector3 Serialization" )]
	public void TestVector3Serialization()
	{
		Global.Log = new ConsoleLogger();

		var vec = new Vector3( 1, 2, 3 );
		var data = NetworkSerializer.Serialize( vec );
		var result = NetworkSerializer.Deserialize<Vector3>( data );
		Assert.AreEqual( vec, result );
	}

	[TestMethod( "Rotation Serialization" )]
	public void TestRotationSerialization()
	{
		Global.Log = new ConsoleLogger();

		var rot = new Rotation( 1, 2, 3, 4 );
		var data = NetworkSerializer.Serialize( rot );
		var result = NetworkSerializer.Deserialize<Rotation>( data );
		Assert.AreEqual( rot, result );
	}

	[TestMethod( "NetworkId Serialization" )]
	public void TestNetworkIdSerialization()
	{
		Global.Log = new ConsoleLogger();

		var id = NetworkId.CreateNetworked();
		var data = NetworkSerializer.Serialize( id );
		var result = NetworkSerializer.Deserialize<NetworkId>( data );
		Assert.AreEqual( id, result );
	}

	[TestMethod( "SnapshotUpdateMessage Serialization" )]
	public void TestSnapshotUpdateMessageSerialization()
	{
		Global.Log = new ConsoleLogger();

		var message = new SnapshotUpdateMessage
		{
			CurrentTimestamp = 0,
			PreviousTimestamp = 0,
			EntityChanges = new List<SnapshotUpdateMessage.EntityChange>()
			{
				new SnapshotUpdateMessage.EntityChange()
				{
					MemberChanges = new List<SnapshotUpdateMessage.EntityMemberChange>()
					{
						new SnapshotUpdateMessage.EntityMemberChange()
						{
							FieldName = "Test",
							Data = NetworkSerializer.Serialize( "Hello World!" )
						}
					},
					NetworkId = NetworkId.CreateNetworked(),
					TypeName = "Test"
				}
			},
			SequenceNumber = 0
		};

		var data = NetworkSerializer.Serialize( message );
		var result = NetworkSerializer.Deserialize<SnapshotUpdateMessage>( data );

		Assert.AreEqual( message.CurrentTimestamp, result.CurrentTimestamp );
		Assert.AreEqual( message.PreviousTimestamp, result.PreviousTimestamp );
		Assert.AreEqual( message.EntityChanges.Count, result.EntityChanges.Count );
		Assert.AreEqual( message.EntityChanges[0].MemberChanges.Count, result.EntityChanges[0].MemberChanges.Count );
		Assert.AreEqual( message.EntityChanges[0].MemberChanges[0].FieldName, result.EntityChanges[0].MemberChanges[0].FieldName );
		Assert.IsTrue( message.EntityChanges[0].MemberChanges[0].Data.SequenceEqual( result.EntityChanges[0].MemberChanges[0].Data ) );
		Assert.AreEqual( message.EntityChanges[0].NetworkId, result.EntityChanges[0].NetworkId );
		Assert.AreEqual( message.EntityChanges[0].TypeName, result.EntityChanges[0].TypeName );
		Assert.AreEqual( message.SequenceNumber, result.SequenceNumber );
	}
}
