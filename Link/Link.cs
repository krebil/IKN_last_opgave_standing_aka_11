using System;
using System.IO.Ports;
using System.Text;

/// <summary>
/// Link.
/// </summary>
namespace Linklaget
{
    /// <summary>
    /// Link.
    /// </summary>
    public class Link
    {
        /// <summary>
        /// The DELIMITE for slip protocol.
        /// </summary>
        const byte DELIMITER = (byte)'A';
        /// <summary>
        /// The buffer for link.
        /// </summary>
        private byte[] buffer;
        /// <summary>
        /// The serial port.
        /// </summary>
        SerialPort serialPort;

        /// <summary>
        /// Initializes a new instance of the <see cref="link"/> class.
        /// </summary>
        public Link(int BUFSIZE, string APP)
        {
            // Create a new SerialPort object with default settings.
#if DEBUG
            if (APP.Equals("FILE_SERVER"))
            {
                serialPort = new SerialPort("/dev/ttySn0", 115200, Parity.None, 8, StopBits.One);
            }
            else
            {
                serialPort = new SerialPort("/dev/ttySn1", 115200, Parity.None, 8, StopBits.One);
            }
#else
				serialPort = new SerialPort("/dev/ttyS1",115200,Parity.None,8,StopBits.One);
#endif
            if (!serialPort.IsOpen)
                serialPort.Open();

            buffer = new byte[(BUFSIZE * 2)];

          
            serialPort.ReadTimeout = 500;

            serialPort.DiscardInBuffer();
            serialPort.DiscardOutBuffer();
        }

        /// <summary>
        /// Send the specified buf and size.
        /// </summary>
        /// <param name='buf'>
        /// Buffer.
        /// </param>
        /// <param name='size'>
        /// Size.
        /// </param>
        public void send(byte[] buf, int size)
        {
            buffer.SetValue(DELIMITER, 0);
            int j = 1;
            for (int i = 0; i < buf.Length; i++)
            {
                if (buf[i] == (byte)'A')
                {
                    buffer[i + j] = (byte)'B';
                    j++;
                    buffer[i + j] = (byte)'C';
                }
                else if (buf[i] == (byte)'B')
                {
                    buffer[i + j] = (byte)'B';
                    j++;
                    buffer[i + j] = (byte)'D';
                }
            }
            buffer[buffer.Length] = DELIMITER;

            serialPort.Write(buffer, 0, buffer.Length);

        }

		/// <summary>
		/// Receive the specified buf and size.
		/// </summary>
		/// <param name='buf'>
		/// Buffer.
		/// </param>
		/// <param name='size'>
		/// Size.
		/// </param>
		public int receive (ref byte[] buf)
        {
            try
            {
                serialPort.Open();

                while (serialPort.ReadChar() != (int) DELIMITER)
                {
                }

                byte read = new byte();
                int index = 0;
                while (read != DELIMITER)
                {
                    read = (byte)serialPort.ReadByte();
                    if (read != DELIMITER)
                    {
                        buffer[index++] = read;
                    }
                }

                string stringBuf = buffer.ToString();

                stringBuf = stringBuf.Replace("BC", "A");
                stringBuf = stringBuf.Replace("BD", "B");

                buf = Encoding.UTF8.GetBytes(stringBuf);

            }
            catch (Exception)
            {
                return 0;
            }
            finally
            {
                serialPort.Close();
            }

            return buf.Length;
        }
	}
}
