using System;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Collections.Generic;

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
                serialPort = new SerialPort("/dev/ttyS1", 115200, Parity.None, 8, StopBits.One);
            }
            else
            {
                serialPort = new SerialPort("/dev/ttyS1", 115200, Parity.None, 8, StopBits.One);
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
        /// <param name='buf'>S
        /// Buffer.
        /// </param>
        /// <param name='size'>
        /// Size.
        /// </param>
        public void send(byte[] buf, int size)
        {
            byte[] sendBuffer = new byte[size * 2];
            sendBuffer[0] = DELIMITER;
            int count = 1;

            for (int i = 0; i < size; i++)
            {
                if (buf[i] == DELIMITER)
                {
                    sendBuffer[i + count] = (byte)'B';
                    sendBuffer[i + count + 1] = (byte)'C';
                    count++;
                }
                else if (buf[i] == (byte)'B')
                {
                    sendBuffer[i + count] = (byte)'B';
                    sendBuffer[i + count + 1] = (byte)'D';
                    count++;
                }
                else
                {
                    sendBuffer[i + count] = buf[i];
                }
            }

            sendBuffer[size + count] = DELIMITER;

            serialPort.Write(sendBuffer, 0, size + count + 1);
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
            int index = 0;
            int count = 0;
            try
            {
                while (serialPort.BytesToRead == 0)
                {
                    Console.WriteLine("Link: Waiting for data...");
                    Thread.Sleep(2500);
                }


                while (serialPort.ReadChar() != (int)DELIMITER)
                {
                }

                byte read = new byte();

                while (read != DELIMITER)
                {
                    read = (byte)serialPort.ReadByte();
                    if (read != DELIMITER)
                    {
                        buffer[index++] = read;
                    }
                }


                List<byte> bytes = new List<byte>();
                for (int i = 0; i < index; i++)
                {
                    if (buffer[i] == (byte)'B')
                    {
                        if (buffer[i + 1] == (byte)'C')
                        {
                            //A
                            bytes.Add((byte)'A');
                            i++;
                        }
                        else if (buffer[i + 1] == (byte)'D')
                        {
                            //B
                            bytes.Add((byte)'B');
                            i++;
                        }
                        count++;
                    }
                    else
                    {
                        bytes.Add(buffer[i]);
                        count++;
                    }
                }

                var temp = bytes.ToArray();

                Array.Copy(temp, buf, temp.Length);
                return count;

            }
            catch (Exception)
            {
                Console.WriteLine("Link:: failed to receive");
                return 0;
            }
        }
    }
}
