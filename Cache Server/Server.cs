using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Helper_Classes;
using System.IO;

namespace Cache_Server
{
    class Server
    {
        private IPEndPoint iPEndPoint;
        private Socket server;
        EventSubscription subscription;
        private List<ClientHandler> clients;

        public Server(IPEndPoint iPEndPoint)
        {
            this.iPEndPoint = iPEndPoint;
            clients = new List<ClientHandler>();
            subscription = new EventSubscription();
        }

        public void StartServer()
        {
            //create a server socket
            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            server.Bind(iPEndPoint);

            try
            {
                server.Listen(10);
                ClientHandler.Log("server started! waiting for connections.....");
                while (true)
                {
                    Socket clientSocket = server.Accept();;
                    ClientHandler.Log("Client " + clientSocket.RemoteEndPoint + " Connected");
                    var clientHandler = new ClientHandler(clientSocket, subscription);
                    clients.Add(clientHandler);
                }
            }
            catch (SocketException e)
            {
                ClientHandler.Log("Socket exception occured: Error while attempting to access the socket\n" + e.Message);
            }
            catch (ObjectDisposedException e)
            {
                ClientHandler.Log("Object disposed exception occured: The socket has been closed\n" + e.Message);
            }
            catch (InvalidOperationException e)
            {
                ClientHandler.Log("Invalid operation exception occured: \n" + e.Message);
            }

            catch (Exception e)
            {
                ClientHandler.Log(e.Message);
            }

        }

        public void StopServer()
        {
            try
            {
                ClientHandler.Log("Server Stopped");
                foreach (var client in clients)
                {
                    client.IsActive = false;
                }
                server.Close();
                server.Dispose();
                
            }
            catch (SocketException e)
            {
                ClientHandler.Log(e.Message);
            }
        }
    }
}