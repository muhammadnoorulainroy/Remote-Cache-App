using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using Helper_Classes;

namespace Cache_Server
{
    class Serializer
    {
        public byte[] Serialize(DataModel dataModel)
        {
            using (var stream = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(stream, dataModel);
                return stream.ToArray();
            }
        }

        public DataModel Deserialize(byte[] bytes)
        {
            using (MemoryStream stream = new MemoryStream(bytes))
            {
                var formatter = new BinaryFormatter();
                return (DataModel)formatter.Deserialize(stream);
            }
        }
    }
}