using Helper_Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;


namespace Cache_Server
{
    class Messages
    {
        private Socket client;
        private Serializer serializer;
        private EventSubscription subscription;


        public Messages(Socket client, EventSubscription subscription )
        {
            this.client = client;
            this.subscription = subscription;
            serializer = new Serializer();
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
                client.Receive(bytesLength, 4, SocketFlags.None);

                byte[] bytes = new byte[BitConverter.ToInt32(bytesLength, 0)];
                client.Receive(bytes, bytes.Length, SocketFlags.None);
                
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
        /// Serialize the message and send it client
        /// </summary>
        public void Send(DataModel dataModel)
        {
            byte[] bytes = serializer.Serialize(dataModel);
            byte[] byteslength = BitConverter.GetBytes(bytes.Length);
            try
            {
                client.Send(byteslength);
                client.Send(bytes);
                
            }
            catch (SocketException ex)
            { 
                ClientHandler.Log("Client Disconnected! " + ex.Message);
            }
        }


        /// <summary>
        /// Method to send notification to the subscriber
        /// </summary>
        public void SendNotification(Socket client, DataModel dataModel)
        {
            byte[] bytes = serializer.Serialize(dataModel);  
            byte[] byteslength = BitConverter.GetBytes(bytes.Length);
            try
            {
                client.Send(byteslength);
                client.Send(bytes);

                ClientHandler.Log("Notification sent to " + client.RemoteEndPoint.ToString());
            }
            catch (SocketException ex)
            {
                ClientHandler.Log("Could not send notification. Subscriber " + client.RemoteEndPoint.ToString() + " is not connected! " + ex.Message);
            }
        }

        public void Notify(string action, DataModel data)
        {
            Dictionary<string, EventRegisterationModel> eventSubscription = subscription.GetSubscriptions();
            var dataModel = new DataModel(null, data, "Notification"); 
            foreach (var subs in eventSubscription)
            {
                EventRegisterationModel reg= subs.Value;
                if (reg.Event == action)
                {
                    SendNotification(reg.Subscriber, dataModel);
                }
            }
        }
    }
}