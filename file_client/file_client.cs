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
        private const int BUFSIZE = 100000;
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
            string ip;
            string fileName;
            string path;

            if (args.Length > 1)
            {
                ip = args[0];
                fileName = args[1];
            }
            else
            {
                Console.WriteLine("No arguments found, using path");
                fileName = "Data.img";
            }
            path = "../../img/" + fileName;

            
            Console.WriteLine("requesting file: " + fileName);
            transport.send(Encoding.UTF8.GetBytes(path), Encoding.UTF8.GetBytes(path).Length);

			// changing path if file exists
			if (File.Exists(path))
			{
				path += DateTime.Now.Date.ToShortDateString().Replace('/', ':') + ":" + DateTime.Now.ToShortTimeString().Replace('/', ':');
			}

            Console.WriteLine("trying to fetch file...");
            var dataReceived = new byte[BUFSIZE];
            int count = transport.receive(ref dataReceived);
			var file = new byte[count];

			for (int i = 0; i < count; i ++){
				file[i] = dataReceived[i];
			}

			//File.Create(path, count);
			File.WriteAllBytes(path, file);


            Console.Write("file received!");
        }

        public static void Main(string[] args)
        {
            Console.WriteLine("Client starting...");
            new file_client(args);
        }
    }

}


