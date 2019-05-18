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

        public void send(byte[] buf, int size)
        {
            int numberOfAOrB = 0;
            //string dataToSend = Encoding.ASCII.GetString(buf);
            for (int i = 0; i < buf.Length; ++i)
            {
                if (buf[i] == (byte)'A' | buf[i] == (byte)'B')
                    numberOfAOrB++;
            }
            byte[] sendBuf = new byte[size + 2 + numberOfAOrB];
            int x = 0;
            sendBuf[0] = (byte)'A';
            for (int i = 1; i < sendBuf.Length - 1; i++)
            {
                if (buf[i - 1 - x] == (byte)'A')
                {
                    sendBuf[i] = (byte)'B';
                    i++;
                    sendBuf[i] = (byte)'C';
                    x++;
                }
                else if (buf[i - 1 - x] == (byte)'B')
                {
                    sendBuf[i] = (byte)'B';
                    i++;
                    sendBuf[i] = (byte)'D';
                    x++;
                }
                else
                {
                    sendBuf[i] = buf[i - 1 - x];
                }
            }

            sendBuf[sendBuf.Length - 1] = (byte)'A';


            serialPort.Write(sendBuf, 0, size + 2 + numberOfAOrB);
        }

        /*
         Mathias send
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
            buffer = new byte[size * 2]; //Doesn't check for size, should be 1000
            buffer[0] = DELIMITER;
            int count = 1;

            for (int i = 0; i < size; i++)
            {
                if (buf[i] == DELIMITER)
                {
                    buffer[i + count] = (byte)'B';
                    buffer[i + ++count] = (byte)'C';               
                }
                else if (buf[i] == (byte)'B')
                {
                    buffer[i + count] = (byte)'B';
                    buffer[i + ++count] = (byte)'D';
                }
                else
                {
                    buffer[i + count] = buf[i];
                }
            }

            buffer[size + count] = DELIMITER;

            serialPort.Write(buffer, 0, size + count + 1);
        }
        */

        /*
        Tobycat send
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
            int count = 0; // counts bytes sent

            
            while (count < size) // while bytes sent is less that amount of bytes to send
            {
                buffer[0] = DELIMITER;
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
                
                buffer[buffer.Length - 1] = DELIMITER;
                
                serialPort.Write(buffer, 0, buffer.Length);
            }
        }*/

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
                        if (buffer[1 + 1] == (byte)'C')
                        {
                            //A
                            bytes.Add((byte)'A');
                            i++;
                        }
                        else if (buffer[1 + 1] == (byte)'C')
                        {
                            //B
                            bytes.Add((byte)'B');
                            i++;
                        }
                    }
                    else
                    {
                        bytes.Add(buffer[i]);
                    }
                }

                var temp = bytes.ToArray();

				Array.Copy(temp, buf, temp.Length);
               

            }
            catch (Exception)
            {
                Console.WriteLine("Link:: failed to receive");
                return 0;
            }

            return index;
        }
    }
}
