namespace Mocha.Hotload.Util;

/// <summary>
/// Contains utility methods for file IO.
/// </summary>
internal static class FileUtil
{
	/// <summary>
	/// Returns whether or not a file is in use by another process.
	/// </summary>
	/// <param name="filePath">The path to a file to check.</param>
	/// <returns>Whether or not a file is in use by another process.</returns>
	internal static bool IsFileInUse( string filePath )
	{
		try
		{
			using var fileStream = new FileStream( filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None );
			return false;
		}
		catch ( IOException )
		{
			return true;
		}
	}
}
