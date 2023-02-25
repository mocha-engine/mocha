using MessagePack;
using Mocha.Common;

namespace Mocha.Networking;

[MessagePackObject]
/// <summary>
/// A snapshot update contains the delta between two snapshots.
/// </summary>
public class SnapshotUpdateMessage : IBaseNetworkMessage
{
	/// <summary>
	/// The timestamp of the previous snapshot.
	/// </summary>
	[Key( 0 )] public float PreviousTimestamp { get; set; }

	/// <summary>
	/// The timestamp of the current snapshot.
	/// </summary>
	[Key( 1 )] public float CurrentTimestamp { get; set; }

	/// <summary>
	/// The sequence number for this snapshot.
	/// </summary>
	[Key( 2 )] public int SequenceNumber { get; set; }

	[MessagePackObject]
	public struct EntityChange
	{
		[Key( 0 )] public NetworkId NetworkId;
		[Key( 1 )] public List<EntityMemberChange> MemberChanges;
		[Key( 2 )] public string TypeName;
	}

	[MessagePackObject]
	public struct EntityMemberChange
	{
		[Key( 0 )] public string FieldName;
		[Key( 1 )] public object Value;
	}

	/// <summary>
	/// A list of changes to entities since the last snapshot.
	/// </summary>
	[Key( 3 )] public List<EntityChange> EntityChanges { get; set; } = new();
}
