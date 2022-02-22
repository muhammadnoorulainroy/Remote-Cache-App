using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helper_Classes
{
    [Serializable]
    public class DataChangeEvent : EventArgs
    {
        public DataModel dataModel{ get; set; }
        public DataChangeEvent(DataModel data)
        {
            dataModel = data;
        }
    }
}
