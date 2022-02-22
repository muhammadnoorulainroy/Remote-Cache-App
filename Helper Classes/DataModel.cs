using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helper_Classes
{
    [Serializable]
    public class DataModel
    {
        public string Key { get; set; }
        public object Value { get; set; }
        public string Message { get; set; }

        public DataModel(string key, object value, string msg)
        {
            Key = key;
            Value = value;
            Message = msg;
        }

        public override string ToString()
        {
            if (Message.Equals("Notification"))
            {
                return "NOTIFCATION: Key: "+ Key + ". Value: " + Value + ". Message " + Message;
            }
            else
            {
                return "Response from Server => \nKey:" + Key + "\nValue:" + Value + "\n Message:" + Message + "\n";
            }
            
        }
    }
}