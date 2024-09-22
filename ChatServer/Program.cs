using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace ChatServer
{
    class Program
    {
        // List to store connected clients
        static List<TcpClient> clients = new List<TcpClient>();
        static void Main(string[] args)
        {
            //Create a TCP Listener that listens on localhost(127.0.0.1) and port 5002.
            TcpListener server = new TcpListener(IPAddress.Parse("127.0.0.1"), 5002);
            server.Start(); //Start the server.
            Console.WriteLine("Server started on 127.0.0.1:5002");

            while (true)
            {
                //Accept a new client connection.
                TcpClient client = server.AcceptTcpClient();
                clients.Add(client); // Add the client to the list of connected clients.
                Console.WriteLine("New client added");

                //Start a new thread to handle communication with the connected client.
                Thread thread = new Thread(() => HandleClient(client));
                thread.Start();

            }
        }

        static void HandleClient(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            byte[] data = new byte[256];

            try
            {
                while (true)
                {
                    int bytesRead = stream.Read(data, 0, data.Length);
                    if (bytesRead == 0) break; //Client Disconnected

                    //Convert the received bytes to a string
                    string message = Encoding.UTF8.GetString(data, 0, bytesRead);
                    Console.WriteLine("Received: " + message);

                    //Broadcast the message to all connected clients
                    Broadcast(message);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.Message);
            }
            finally
            {
                //Client Disconnected, remove from client list
                clients.Remove(client);
                client.Close();
                Console.WriteLine("Client Disconnected");
            }
        }

        static void Broadcast(string message)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);

            //Loop through all connected clients and send the message
            foreach(TcpClient client in clients)
            {
                try
                {
                    NetworkStream stream = client.GetStream();
                    stream.Write(data, 0, data.Length); //Send the message
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error broadcasting message: " + e.Message);
                }
            }
        }
    }
}
