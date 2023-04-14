using MessagePack;
using Mocha.Common;

namespace Mocha.Networking;

[MessagePackObject( true )]
/// <summary>
/// A snapshot update contains the delta between two snapshots.
/// </summary>
public class SnapshotUpdateMessage : IBaseNetworkMessage
{
	[IgnoreMember]
	public MessageID MessageID => MessageID.SnapshotUpdate;

	/// <summary>
	/// The timestamp of the previous snapshot.
	/// </summary>
	public float PreviousTimestamp { get; set; }

	/// <summary>
	/// The timestamp of the current snapshot.
	/// </summary>
	public float CurrentTimestamp { get; set; }

	/// <summary>
	/// The sequence number for this snapshot.
	/// </summary>
	public int SequenceNumber { get; set; }

	[MessagePackObject( true )]
	public struct EntityChange
	{
		public NetworkId NetworkId;
		public List<EntityMemberChange> MemberChanges;
		public string TypeName;
	}

	[MessagePackObject( true )]
	public struct EntityMemberChange
	{
		public string FieldName;
		public byte[] Data;
	}

	/// <summary>
	/// A list of changes to entities since the last snapshot.
	/// </summary>
	public List<EntityChange> EntityChanges { get; set; } = new();
}
