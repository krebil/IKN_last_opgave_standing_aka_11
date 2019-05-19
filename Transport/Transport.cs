using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using Linklaget;

/// <summary>
/// Transport.
/// </summary>
namespace Transportlaget
{
    /// <summary>
    /// Transport.
    /// </summary>
    public class Transport
    {
        /// <summary>
        /// The link.
        /// </summary>
        private Link link;
        /// <summary>
        /// The 1' complements checksum.
        /// </summary>
        private Checksum checksum;
        /// <summary>
        /// The buffer.
        /// </summary>
        private byte[] buffer;
        /// <summary>
        /// The seq no.
        /// </summary>
        private byte seqNo;
        /// <summary>
        /// The old_seq no.
        /// </summary>
        private byte old_seqNo;
        /// <summary>
        /// The error count.
        /// </summary>
        private int errorCount;
        /// <summary>
        /// The DEFAULT_SEQNO.
        /// </summary>
        private const int DEFAULT_SEQNO = 2;
        /// <summary>
        /// The data received. True = received data in receiveAck, False = not received data in receiveAck
        /// </summary>
        private bool dataReceived;
        /// <summary>
        /// The number of data the recveived.
        /// </summary>
        private int recvSize = 0;
        private string stringBuf;

        /// <summary>
        /// Initializes a new instance of the <see cref="Transport"/> class.
        /// </summary>
        public Transport(int BUFSIZE, string APP)
        {
            link = new Link(BUFSIZE + (int)TransSize.ACKSIZE, APP);
            checksum = new Checksum();
            buffer = new byte[BUFSIZE + (int)TransSize.ACKSIZE];
            seqNo = 0;
            old_seqNo = DEFAULT_SEQNO;
            errorCount = 0;
            dataReceived = false;
        }

        /// <summary>
        /// Receives the ack.
        /// </summary>
        /// <returns>
        /// The ack.
        /// </returns>
        private bool receiveAck()
        {
            recvSize = link.receive(ref buffer);
            dataReceived = true;

            if (recvSize == (int)TransSize.ACKSIZE)
            {
                dataReceived = false;
                if (!checksum.checkChecksum(buffer, (int)TransSize.ACKSIZE) ||
                  buffer[(int)TransCHKSUM.SEQNO] != seqNo ||
                  buffer[(int)TransCHKSUM.TYPE] != (int)TransType.ACK)
                {
                    return false;
                }
                seqNo = (byte)((buffer[(int)TransCHKSUM.SEQNO] + 1) % 2);
            }

            return true;
        }

        /// <summary>
        /// Sends the ack.
        /// </summary>
        /// <param name='ackType'>
        /// Ack type.
        /// </param>
        private void sendAck(bool ackType)
        {
            byte[] ackBuf = new byte[(int)TransSize.ACKSIZE];
            ackBuf[(int)TransCHKSUM.SEQNO] = (byte)
                (ackType ? (byte)buffer[(int)TransCHKSUM.SEQNO] : (byte)(buffer[(int)TransCHKSUM.SEQNO] + 1) % 2);
            ackBuf[(int)TransCHKSUM.TYPE] = (byte)(int)TransType.ACK;
            checksum.calcChecksum(ref ackBuf, (int)TransSize.ACKSIZE);
            link.send(ackBuf, (int)TransSize.ACKSIZE);
        }

        /// <summary>
        /// Send the specified buffer and size.
        /// </summary>
        /// <param name='buffer'>
        /// Buffer.
        /// </param>
        /// <param name='size'>
        /// Size.
        /// </param>
        public void send(byte[] buf, int size)
        {
            do
            {

                byte[] data = new byte[size + 4];
                Array.Copy(buf, 0, data, 4, size);

                data[2] = seqNo;
                data[3] = 0; //type = data
                checksum.calcChecksum(ref data, data.Length);

                link.send(data, data.Length);

            } while (receiveAck() == false);

        }

        /// <summary>
        /// Receive the specified buffer.
        /// </summary>
        /// <param name='buffer'>
        /// Buffer.
        /// </param>
        public int receive(ref byte[] buf)
        {
            recvSize = link.receive(ref buffer);

            while (!checksum.checkChecksum(buffer, recvSize))
            {

                sendAck(false);
                recvSize = link.receive(ref buffer);
            }

            sendAck(true);

            string stringBufTrans = Encoding.UTF8.GetString(buffer);
            Console.WriteLine("Transport::ACK and Data received");
            int count = 0;
            try
            {
                for (int i = 4; i < recvSize + 4; i++)
                {
                    if (buffer[i] != 0)
                    {
                        ++count;
                        buf[i - 4] = buffer[i];
                    }
                }
                string stringBufTrans2 = Encoding.UTF8.GetString(buf);
                return count;
            }
            catch (Exception)
            {
                Console.WriteLine("Transport::Failed to write to referenced byte");
                return 0;
            }


        }
    }
}
