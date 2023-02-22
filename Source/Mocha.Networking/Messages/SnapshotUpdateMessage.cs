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
	public float PreviousTimestamp { get; set; }

	/// <summary>
	/// The timestamp of the current snapshot.
	/// </summary>
	public float CurrentTimestamp { get; set; }

	/// <summary>
	/// The sequence number for this snapshot.
	/// </summary>
	public int SequenceNumber { get; set; }

	public record struct EntityFieldChange( string FieldName, object Value );
	public record struct EntityChange( NetworkId NetworkId, List<EntityFieldChange> FieldChanges, string TypeName );

	/// <summary>
	/// A list of changes to entities since the last snapshot.
	/// </summary>
	public List<EntityChange> EntityChanges { get; set; } = new();
}
