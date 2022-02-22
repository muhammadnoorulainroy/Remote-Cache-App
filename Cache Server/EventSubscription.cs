using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Helper_Classes;
using System.Net.Sockets;

namespace Cache_Server
{
    public class EventSubscription
    {
        //store clients subscriptions for events
        private Dictionary<string, EventRegisterationModel> Subscriptions;

        public EventSubscription()
        {
            Subscriptions = new Dictionary<string,EventRegisterationModel>();
        }
        
        //Get subscriptions of clients
        public Dictionary<string, EventRegisterationModel> GetSubscriptions()
        {
            return Subscriptions;
        }

        public void Dispose()
        {
            Subscriptions = null;
        }

        /// <summary>
        /// Subscribe to any event if not already subscribed
        /// </summary>
        public void Subscribe(EventRegisterationModel eventRegisteration)
        {
            if (!Subscriptions.ContainsKey(eventRegisteration.Key))
            {
                Subscriptions.Add(eventRegisteration.Key, eventRegisteration);
            }
        }

        /// <summary>
        /// Unsubscribe from any event if subscribed
        /// </summary>
        public void Unsubscribe(Socket client, string req)
        {
            if (Subscriptions.ContainsKey(req + client.RemoteEndPoint.ToString()))
            {
                Subscriptions.Remove(req + client.RemoteEndPoint.ToString());
            }
        }

        /// <summary>
        /// Check if a client has already subscribed to a event or not
        /// </summary>
        public bool isAlreadySubscribed(Socket client, string req)
        {
            if (Subscriptions.ContainsKey(req + client.RemoteEndPoint.ToString()))
                return true;
            else
                return false;
        }


        /// <summary>
        /// Subscribe to all events
        /// </summary>
        public void SubscribeAllEvents(Socket client)
        {
            if (!isAlreadySubscribed(client, "Add"))
                Subscribe(new EventRegisterationModel("Add", client));
            if (!isAlreadySubscribed(client, "Remove"))
                Subscribe(new EventRegisterationModel("Remove", client));
            if (!isAlreadySubscribed(client, "Clear"))
                Subscribe(new EventRegisterationModel("Clear", client));
        }

        /// <summary>
        /// Unsubscribe from all events
        /// </summary>
        public void UnsubscribeAllEvents(Socket client)
        {
            if (Subscriptions.ContainsKey("Add" + client.RemoteEndPoint.ToString()))
            {
                Subscriptions.Remove("Add" + client.RemoteEndPoint.ToString());
            }
            if (Subscriptions.ContainsKey("Remove" + client.RemoteEndPoint.ToString()))
            {
                Subscriptions.Remove("Remove" + client.RemoteEndPoint.ToString());
            }
            if (Subscriptions.ContainsKey("Clear" + client.RemoteEndPoint.ToString()))
            {
                Subscriptions.Remove("Clear" + client.RemoteEndPoint.ToString());
            }
        }

       

    }
}
