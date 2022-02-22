using Helper_Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Cache_Client
{
    class Messages
    {
        private Socket server;
        private Serializer serializer;


        public Messages(Socket server, EventHandler<ResponseEvent> handler)
        {
            this.server = server;
            serializer = new Serializer();
            responseEvent += handler;
        }

        //Event Handler
        public event EventHandler<ResponseEvent> responseEvent;

        public virtual void OnresponseEvent(ResponseEvent args)
        {
            if (responseEvent != null)
            {
                responseEvent(this, args);
            }
        }

        /// <summary>
        /// Receive message from client and deserialize it
        /// </summary>
        /// 
        public DataModel Receive()
        {
            try
            {
                byte[] bytesLength = new byte[4];
                server.Receive(bytesLength, 4, SocketFlags.None);

                byte[] bytes = new byte[BitConverter.ToInt32(bytesLength, 0)];
                server.Receive(bytes, bytes.Length, SocketFlags.None);

                return serializer.Deserialize(bytes);
            }
            catch (ObjectDisposedException ex)
            {
                return new DataModel(null, null, "Server is not connected " + ex.Message);
            }
            catch (Exception ex)
            {
                return new DataModel(null, null, "Exception Occurred " + ex.Message);
            }
        }

        /// <summary>
        /// Serialize the message and send it to client
        /// </summary>
        public void Send(DataModel request)
        {
            
            byte[] bytes = serializer.Serialize(request);
            byte[] bytesLength = BitConverter.GetBytes(bytes.Length);
            try
            {
                server.Send(bytesLength);
                server.Send(bytes);
            }
            catch (SocketException ex)
            {
                OnresponseEvent(new ResponseEvent("Server is not Connected " + ex.Message));
            }
        }
    }
}