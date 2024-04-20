using System.Net.Sockets;

public static class StreamUtil
    {
        public static void Write(NetworkStream pStream, byte[] pBytes)
        {
            pStream.Write(BitConverter.GetBytes(pBytes.Length), 0, 4);
            pStream.Write(pBytes, 0, pBytes.Length);
        }

        public static byte[]? Read(NetworkStream pStream)
        {
            int byteCountToRead = BitConverter.ToInt32(Read(pStream, 4), 0);
            return Read(pStream, byteCountToRead);
        }

        private static byte[]? Read(NetworkStream pStream, int pByteCount)
        {
            byte[] bytes = new byte[pByteCount];
            int bytesRead = 0;
            int totalBytesRead = 0;

            try
            {
                while (totalBytesRead != pByteCount &&
                      (bytesRead = pStream.Read(bytes, totalBytesRead, pByteCount - totalBytesRead)) > 0)
                {
                    totalBytesRead += bytesRead;
                }
            }
            catch { }

            return (totalBytesRead == pByteCount) ? bytes : null;
        }
    }




