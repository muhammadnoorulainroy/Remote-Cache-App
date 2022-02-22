using Helper_Classes;
using User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cache_Client
{
    public class Client : ICache
    {
        private bool isActive = false;
        private Socket client;
        private Messages messages;
        private ManualResetEvent resetEvent;
        private DataModel dataModel;
        IPEndPoint iPEndPoint;
        private Serializer serializer;

        public Client(IPEndPoint iPEndPoint, EventHandler<ResponseEvent> responseEventhandler, EventHandler<DataChangeEvent> dataModelHandler)
        {
            //Creating a socket for client
            client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            messages = new Messages(client, responseEventhandler);
            resetEvent = new ManualResetEvent(false);
            dataModel = null;
            this.iPEndPoint = iPEndPoint;
            responseEvent += responseEventhandler;
            dataChangeEvent += dataModelHandler;
            serializer = new Serializer();
            //StartClient(iPEndPoint);
        }

        public event EventHandler<ResponseEvent> responseEvent;
        public event EventHandler<DataChangeEvent> dataChangeEvent;

        public virtual void OnResponseEvent(ResponseEvent args)
        {
            responseEvent?.Invoke(this, args);
        }

        public virtual void OnDataChangeEvent(DataChangeEvent args)
        {
            dataChangeEvent?.Invoke(this, args);
        }

        public void StartClient(IPEndPoint serverEndPoint)
        {
            client.Connect(serverEndPoint);
            OnResponseEvent(new ResponseEvent("Client " + client.LocalEndPoint + " Connected to " + client.RemoteEndPoint));
            Thread thread = new Thread(ListenServer);
            thread.Start();
        }

        private void ListenServer()
        {
            OnResponseEvent(new ResponseEvent(Thread.CurrentThread.ManagedThreadId + " Client " + client.LocalEndPoint + " Client is now listening to server...."));
            isActive = true;
            DataModel currentDataModel;
            while (isActive)
            {
                try
                {
                    currentDataModel = messages.Receive();
                    if (currentDataModel.Message.Contains("Exception Occurred") || currentDataModel.Message.Contains("Server is not connected"))
                    {
                        OnResponseEvent(new ResponseEvent(currentDataModel.Message));
                        break;
                    }
                    else if (currentDataModel.Message.Equals("Data found"))
                    {
                        dataModel = currentDataModel;
                        resetEvent.Set();
                    }
                    else if (currentDataModel.Message.Equals("Could not get the data! Key not found"))
                    {
                        resetEvent.Set();
                    }
                    else
                    {
                        OnResponseEvent(new ResponseEvent(Thread.CurrentThread.ManagedThreadId + " Client: " + client.LocalEndPoint + " " + currentDataModel.Message));
                        OnDataChangeEvent(new DataChangeEvent(currentDataModel));
                    }
                }
                catch (Exception ex)
                {
                    OnResponseEvent(new ResponseEvent("Server is disconnected " + ex + ex.Message));
                    break;
                }
            }
        }

        /// <summary>
        /// Implement ICache interface methods
        /// </summary>
        public void Add(string key, object value)
        {
            messages.Send(new DataModel(key, serializer.SerializeData(value), "add"));
        }

        public void Clear()
        {
            messages.Send(new DataModel(null, null, "clear"));
            dataModel = null;
        }

        public void Dispose()
        {
            messages.Send(new DataModel(null, null, "dispose"));
            isActive = false;
            //client.Shutdown(SocketShutdown.Both);
            client.Close();
        }

        public object Get(string key)
        {
            resetEvent.Reset();
            messages.Send(new DataModel(key, null, "get"));
            resetEvent.WaitOne(2000);

            if (dataModel != null && dataModel.Key.Equals(key))
            {
                DataModel responseData = dataModel;
                responseData.Value = serializer.DeserializeData((byte[])responseData.Value);
                dataModel = null;
                return responseData;
            }
            else
            {
                return new DataModel(key, null, "Could not get the data");
            }
        }

        public void Initialize()
        {
            StartClient(iPEndPoint);
            //messages.Send(new DataModel(null, null, "initialize"));
        }

        public void Remove(string key)
        {
            messages.Send(new DataModel(key, null, "remove"));   
        }

        public void Sub(string action)
        {
            messages.Send(new DataModel(action, null, "subscribe"));
        }

        public void UnSub(string action)
        {
            messages.Send(new DataModel(action, null, "unsubscribe"));
        }
    }
}