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
		private file_server ()
		{
            Transport transport = new Transport(BUFSIZE,APP);
            
            while(true)
            {
                string path = "../../";
                Console.WriteLine("waiting for connection...");
                Byte[] buff = new Byte[BUFSIZE+10];

                transport.receive(ref buff);

                string filename = Encoding.UTF8.GetString(buff);
				Console.WriteLine(filename);
                Console.WriteLine("checking if file exists");
                if(File.Exists(path + filename))
                {
					Console.WriteLine("File exists ");
                    Console.WriteLine("Sending file: " + filename);
                    Byte[] fileToSend = File.ReadAllBytes(path + filename);

                    Console.WriteLine("Sending file...");
                    transport.send(fileToSend, fileToSend.Length);
                    Console.WriteLine("File sent!");

                }
                else
                {
                    Console.WriteLine(filename + " does not exist");
                }
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
			// TO DO Your own code
		}

		/// <summary>
		/// The entry point of the program, where the program control starts and ends.
		/// </summary>
		/// <param name='args'>
		/// The command-line arguments.
		/// </param>
		public static void Main (string[] args)
		{
			file_server fs = new file_server();
		}
	}
}