using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace Helper_Classes
{
    [Serializable]
    public class EventRegisterationModel
    {
        public string Key { get; set; }
        public string Event { get; set; }
        public Socket Subscriber { get; set; }

        public EventRegisterationModel(string evnt, Socket subscriber)
        {
            Event = evnt;
            Subscriber = subscriber; 
            Key = evnt + subscriber.RemoteEndPoint.ToString();
        }
    }


}
