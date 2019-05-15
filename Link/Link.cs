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
            int count = 0; // counts bytes sent

            while (count < size) // while bytes sent is less that amount of bytes to send
            {
                buffer.SetValue(DELIMITER, 0);
                int j = 1; // number of extra characters due to swap of A's and B's
                for (int i = 0; i < buf.Length; i++)
                {
                    if (i < 1000 || size - (count + i) <= 0)
                    {
                        if (buf[i + count] == (byte)'A')
                        {
                            buffer[i + j + count] = (byte)'B';
                            j++;
                            buffer[i + j + count] = (byte)'C';
                        }
                        else if (buf[i + count] == (byte)'B')
                        {
                            buffer[i + j + count] = (byte)'B';
                            j++;
                            buffer[i + j + count] = (byte)'D';
                        }
                    }
                    else
                    {
                        if (size - (count + i) <= 0)
                        {
                            count = size;
                            break;
                        }
                        else
                        {
                            count += 1000;
                            break;
                        }
                    }
                }
                buffer[buffer.Length] = DELIMITER;

                serialPort.Write(buffer, 0, buffer.Length);
            }
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
        public int receive(ref byte[] buf)
        {
            try
            {
                serialPort.Open();

                serialPort.Read(buffer, 0, serialPort.BytesToRead);

                if (buffer[0].ToString() != "A" || buffer[buffer.Length].ToString() != "A")
                    return 0;

                else
                {
                    string stringBuf = buffer.ToString().Substring(1, buffer.Length - 1);


                    stringBuf = stringBuf.Replace("BC", "A");
                    stringBuf = stringBuf.Replace("BD", "B");

                    buf = Encoding.UTF8.GetBytes(stringBuf);
                }
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
