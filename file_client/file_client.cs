using System;
using System.IO;
using System.Text;
using Transportlaget;
using Library;

namespace Application
{
    class file_client
    {
        /// <summary>
        /// The BUFSIZE.
        /// </summary>
        private const int BUFSIZE = 1000;
        private const string APP = "FILE_CLIENT";

        /// <summary>
        /// Initializes a new instance of the <see cref="file_client"/> class.
        /// 
        /// file_client metoden opretter en peer-to-peer forbindelse
        /// Sender en forspÃ¸rgsel for en bestemt fil om denne findes pÃ¥ serveren
        /// Modtager filen hvis denne findes eller en besked om at den ikke findes (jvf. protokol beskrivelse)
        /// Lukker alle streams og den modtagede fil
        /// Udskriver en fejl-meddelelse hvis ikke antal argumenter er rigtige
        /// </summary>
        /// <param name='args'>
        /// Filnavn med evtuelle sti.
        /// </param>
        Transport transport;
        private file_client(String[] args)
        {
            transport = new Transport(BUFSIZE, APP);
            string fileName;
            string path;

            if (args.Length > 1)
            {
                fileName = args[1];
            }
            else
            {
                Console.WriteLine("No arguments found, using path");
                fileName = "Data.jpg";
            }
            path = "../../img/" + fileName;



            transport.send(Encoding.UTF8.GetBytes(path), Encoding.UTF8.GetBytes(path).Length);

            if (File.Exists(path))
            {
                path += DateTime.Now.ToShortDateString().Replace("/", "-").Replace("\\", "_");
            }

            var lengthBytes = new byte[100];


            transport.receive(ref lengthBytes);
            var FileLength = Encoding.UTF8.GetString(lengthBytes);
            int length = 0;
            try
            {
                length = int.Parse(FileLength);
            }
            catch
            {
                Console.WriteLine("length is empty");
            }
            if (length < 1)
            {
                Console.WriteLine("File does not exist");
            }
            else
            {
                int fLength = int.Parse(FileLength);
                Console.WriteLine("Getting File");
                var FileData = new Byte[fLength];

                int packetsToSend = fLength / 1000;
                int finalBufSize = fLength % 1000;
                if (finalBufSize != 0)
                    ++packetsToSend;

                byte[] tempByte = new byte[BUFSIZE];
                for (int i = 0; i < packetsToSend; i++)
                {
                    int tempInt = transport.receive(ref tempByte);

                    for (int j = 0; j < tempInt; j++)
                    {
                        FileData[j + i * 1000] = tempByte[j];
                    }
                    Console.WriteLine($"Received packet {i}");
                }

                Console.WriteLine("Creating file");
                FileStream fs = File.Create(path);

                fs.Write(FileData, 0, FileData.Length);
                fs.Close();

                Console.WriteLine("File created at: " + path);
            }
            Console.WriteLine("Client closing");

        }

        public static void Main(string[] args)
        {
            Console.WriteLine("Client starting...");
            new file_client(args);
        }
    }

}


