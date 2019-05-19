using System;
using System.IO;
using System.Text;
using Transportlaget;
using Library;

namespace Application
{
    class file_server
    {
        /// <summary>
        /// The BUFSIZE
        /// </summary>
        private const int BUFSIZE = 1000;
        private const string APP = "FILE_SERVER";

        /// <summary>
        /// Initializes a new instance of the <see cref="file_server"/> class.
        /// </summary>
        private file_server()
        {
            Transport transport = new Transport(BUFSIZE, APP);

            while (true)
            {
                Console.WriteLine("waiting for connection...");
                byte[] buff = new byte[BUFSIZE + 10];

                int count = transport.receive(ref buff);

                byte[] fileNameBuf = new byte[count];

                for (int i = 0; i < count; i++)
                {
                    fileNameBuf[i] = buff[i];
                }
                string filename = Encoding.UTF8.GetString(fileNameBuf);


                sendFile(filename, 0, transport);

            }
        }

        /// <summary>
        /// Sends the file.
        /// </summary>
        /// <param name='fileName'>
        /// File name.
        /// </param>
        /// <param name='fileSize'>
        /// File size.
        /// </param>
        /// <param name='tl'>
        /// Tl.
        /// </param>
        private void sendFile(String fileName, long fileSize, Transport transport)
        {
            int packetsToSend;
            Console.WriteLine("Checking if file exists");

            if (File.Exists(fileName))
            {
                Console.WriteLine("File exists");
                Console.WriteLine("Sending file: " + fileName);

                int finalBufSize;
                byte[] fileToSend = File.ReadAllBytes(fileName);

                string sendByteString = fileToSend.Length.ToString();
                byte[] fileSizeByte = Encoding.UTF8.GetBytes(sendByteString);

                transport.send(fileSizeByte, fileSizeByte.Length);

                //Send actual file in packets
                packetsToSend = fileToSend.Length / 1000;
                finalBufSize = fileToSend.Length % 1000;
                if (finalBufSize != 0)
                    ++packetsToSend;

                byte[] filePackage = new byte[BUFSIZE];


                for (int i = 0; i < packetsToSend; i++)
                {
                    if (i != packetsToSend - 1) //if not last packet
                    {
                        //Copy array
                        for (int ind = 0; ind < 1000; ind++)
                        {
                            filePackage[ind] = fileToSend[(i * 1000) + ind];
                        }
                        Console.WriteLine($"Sending packet: {i}");
                        transport.send(filePackage, BUFSIZE);
                        Console.WriteLine("Packet sent");
                    }
                    else //if last packet
                    {
                        filePackage = new byte[finalBufSize];

                        for (int ind = 0; ind < finalBufSize - 1; ind++)
                        {
                            filePackage[ind] = fileToSend[(i * 1000) + ind];
                        }

                        Console.WriteLine($"Sending final packet: {i}");
                        transport.send(filePackage, finalBufSize);
                        Console.WriteLine("Packet sent");
                    }
                }


                Console.WriteLine("Sending file...");
                transport.send(fileToSend, fileToSend.Length);
                Console.WriteLine("File sent!");

            }
            else
            {
                byte[] failedByte = new byte[1];
                failedByte[0] = (byte)0;
                transport.send(failedByte, 1);
                Console.WriteLine(fileName + " does not exist");
            }
        }

        /// <summary>
        /// The entry point of the program, where the program control starts and ends.
        /// </summary>
        /// <param name='args'>
        /// The command-line arguments.
        /// </param>
        public static void Main(string[] args)
        {
            file_server fs = new file_server();
        }
    }
}