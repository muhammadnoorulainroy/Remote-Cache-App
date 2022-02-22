using Helper_Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Cache_Client
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

        public byte[] SerializeData(object data)
        {
            using (var stream = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(stream, data);
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

        public object DeserializeData(byte[] bytes)
        {
            using (MemoryStream stream = new MemoryStream(bytes))
            {
                var formatter = new BinaryFormatter();
                return (object)formatter.Deserialize(stream);
            }
        }
    }
}