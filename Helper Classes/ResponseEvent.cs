using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helper_Classes
{
    public class ResponseEvent : EventArgs
    {
        public string Message { get; set; }
        
        public ResponseEvent(string msg)
        {
            Message = msg;
        }
    }
}
