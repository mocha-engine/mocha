using Mocha.Common;

namespace Mocha.Networking;

/// <summary>
/// A snapshot update contains the delta between two snapshots.
/// </summary>
public class SnapshotUpdateMessage : IBaseNetworkMessage
{
	/// <summary>
	/// The timestamp of the previous snapshot.
	/// </summary>
	[Replicated] public float PreviousTimestamp { get; set; }

	/// <summary>
	/// The timestamp of the current snapshot.
	/// </summary>
	[Replicated] public float CurrentTimestamp { get; set; }

	/// <summary>
	/// The sequence number for this snapshot.
	/// </summary>
	[Replicated] public int SequenceNumber { get; set; }

	public struct EntityChange
	{
		[Replicated] public NetworkId NetworkId;
		[Replicated] public List<EntityMemberChange> MemberChanges;
		[Replicated] public string TypeName;
	}

	public struct EntityMemberChange
	{
		[Replicated] public string FieldName;
		[Replicated] public object Value;
	}

	/// <summary>
	/// A list of changes to entities since the last snapshot.
	/// </summary>
	[Replicated] public List<EntityChange> EntityChanges { get; set; } = new();
}
