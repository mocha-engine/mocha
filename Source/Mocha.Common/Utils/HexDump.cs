using System.Text;

namespace Mocha.Common;

public class HexDump
{
	public static string Dump( byte[] bytes, int bytesPerLine )
	{
		StringBuilder sb = new StringBuilder();
		int offset = 0;

		while ( offset < bytes.Length )
		{
			int remainingBytes = bytes.Length - offset;
			int lineBytes = Math.Min( bytesPerLine, remainingBytes );

			sb.AppendFormat( "{0:x8}  ", offset );

			for ( int i = 0; i < lineBytes; i++ )
			{
				sb.AppendFormat( "{0:x2} ", bytes[offset + i] );
			}

			if ( lineBytes < bytesPerLine )
			{
				sb.Append( new string( ' ', (bytesPerLine - lineBytes) * 3 ) );
			}

			sb.Append( "|" );

			for ( int i = 0; i < lineBytes; i++ )
			{
				char c = (char)bytes[offset + i];

				// If char isn't a letter, symbol, or number, replace it with a dot.
				if ( (int)c > 32 && (int)c < 127 )
				{
					sb.Append( c );
				}
				else
				{
					sb.Append( "." );
				}
			}

			sb.AppendLine( "|" );

			offset += lineBytes;
		}

		return sb.ToString();
	}
}
