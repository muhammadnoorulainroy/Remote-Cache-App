using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Helper_Classes;

namespace Cache_Server
{
    class ClientHandler
    {
        private Socket client;
        public bool IsActive { get; set; } = false;
        private Messages message;
        private CacheOperations cacheOperations;
        private EventSubscription subscription;

        public ClientHandler(Socket client, EventSubscription subscription)
        {
            
            message = new Messages(client, subscription);
            cacheOperations = new CacheOperations();
            this.subscription = subscription;   
            HandleClient(client);
        }


        //Handle each client in a separate thread
        private void HandleClient(Socket client)
        {
            this.client = client;
            Thread thread = new Thread(ListenClient);
            thread.Start();
        }

        /// <summary>
        /// List to the client and receive requests
        /// perform action based on the dataModel
        /// </summary>
        private void ListenClient()
        {
            IsActive = true;
            DataModel dataModel;
            while (IsActive)
            {
                try
                {
                    dataModel = message.Receive();
                    //Log(dataModel.Value.ToString());
                    if (dataModel.Message.Contains("Exception Occurred") || dataModel.Message.Contains("Server is not connected"))
                    {
                        message.Send(new DataModel(null, null, dataModel.Message));
                        Log("Thread ID: " + Thread.CurrentThread.ManagedThreadId + " " + dataModel.Message);  
                        break;
                    }
                        
                    else if (dataModel.Message.ToLower().Equals("dispose"))
                    {
                        ClientActions(dataModel);
                        break;
                    }
                    ClientActions(dataModel);
                }
                catch (SocketException ex)
                {  
                    Log("Thread ID: " + Thread.CurrentThread.ManagedThreadId + " The Client " + client.RemoteEndPoint.ToString() + " was disconnected " + ex.Message);
                    client.Shutdown(SocketShutdown.Both);
                    client.Close();
                    break;
                }
                catch(Exception ex)
                {
                    Log("Thread ID: " + Thread.CurrentThread.ManagedThreadId + " The Client " + client.RemoteEndPoint.ToString() + " was disconnected " + ex.Message);
                    client.Shutdown(SocketShutdown.Both);
                    client.Close();
                    break;
                }
            }
        }

        private void ClientActions(DataModel dataModel)
        {
            string req = dataModel.Message;

            switch (req)
            {
                case "add":

                    //if the key is not already stored, add the data
                    if (!cacheOperations.isKeyExists(dataModel.Key))
                    {
                        cacheOperations.Add(dataModel.Key, dataModel.Value);
                        message.Send(new DataModel(dataModel.Key, null, "Data has been added successfully"));                   
                        //write action in log file
                        Log("Thread ID: " + Thread.CurrentThread.ManagedThreadId + ". ADD Request from Client: " + client.RemoteEndPoint + ". | Key: " + dataModel.Key + "| has been added");
                        //Send notification to the subscribers 
                        message.Notify("Add", new DataModel(dataModel.Key, null, "Following customer has been added"));
                    }
                    else
                    {
                        //if key is already present in the cache, dont add data and send back the response
                        message.Send(new DataModel(dataModel.Key, null, "Could not add the customer! " + dataModel.Key + " Key already exists"));
                        Log("Thread ID: " + Thread.CurrentThread.ManagedThreadId + ". ADD Request from Client: " + client.RemoteEndPoint + ". " + "Could not add the customer. Key: " + dataModel.Key + "\tKey already exists");
                    }
                    break;

                case "get":
                    object data = cacheOperations.Get(dataModel.Key);
                    if (!data.ToString().Equals("Not Found"))
                    {
                        message.Send(new DataModel(dataModel.Key, data, "Data found"));
                        Log("Thread ID: " + Thread.CurrentThread.ManagedThreadId + ". GET Request from Client: for " + client.RemoteEndPoint + ". | Key: " + dataModel.Key);
                    }
                    else
                    {
                        message.Send(new DataModel(null, null, "Could not get the data! Key not found")); 
                        Log("Thread ID: " + Thread.CurrentThread.ManagedThreadId + ". GET Request from Client: " + client.RemoteEndPoint + ". Key: " + dataModel.Key + " Not found");
                    }
                    break ;

                case "remove":
                    //if the key is valid, remove the key
                    if (cacheOperations.isKeyExists(dataModel.Key))
                    {
                        cacheOperations.Remove(dataModel.Key);
                        message.Send(new DataModel(dataModel.Key, null, "Data has been removed successfully"));
                        Log("Thread ID: " + Thread.CurrentThread.ManagedThreadId + ". REMOVE Request from Client: " + client.RemoteEndPoint + ". | Key: " + dataModel.Key + "| has been removed");
                        //Send notification subscribers
                        message.Notify("Remove", new DataModel(dataModel.Key, null, "Data has been removed"));
                    }

                    //if key is invalid
                    else
                    {
                        message.Send(new DataModel(dataModel.Key, null, "Could not remove the data. KEY NOT FOUND"));
                        Log("Thread ID: " + Thread.CurrentThread.ManagedThreadId + ". REMOVE Request from Client: " + client.RemoteEndPoint + ". Invalid remove request against the key " + dataModel.Key);
                    }
                    break;

                case "clear":
                    cacheOperations.Clear();
                    message.Send(new DataModel(null, null, "Cache has been cleared")); 
                    Log("Thread ID: " + Thread.CurrentThread.ManagedThreadId + ". CLEAR Request from Client: " + client.RemoteEndPoint + ".  Cache has been cleared");
                    //Notify the subscribers
                    message.Notify("Clear", new DataModel(null, null, "Cache has been cleared"));
                    break;

                case "dispose":
                    subscription.UnsubscribeAllEvents(client);              
                    Log("Thread ID: " + Thread.CurrentThread.ManagedThreadId + ". DISPOSE Request from Client: " + client.RemoteEndPoint + ". CLIENT DISPOSED");
                    Log("Client " + client.RemoteEndPoint.ToString() + " disconnected");
                    break;

                case "initialize":
                    cacheOperations.Initialize();   
                    Log("Thread ID: " + Thread.CurrentThread.ManagedThreadId + ". INITIALIZE Request from Client: " + client.RemoteEndPoint + ". Cache has been initialized");
                    break ;

                case "subscribe":
                    if (subscription.isAlreadySubscribed(client, dataModel.Key))
                    {
                        message.Send(new DataModel(null, null, "Already subscribed to " + dataModel.Key + " event"));
                        Log("Thread ID: " + Thread.CurrentThread.ManagedThreadId + ". SUBSCRIBE Request for " + dataModel.Key + " Event from Client: " + client.RemoteEndPoint + ". Already subscribed.");
                    }
                    else
                    {
                        if (dataModel.Key.Equals("All"))
                        {
                            subscription.SubscribeAllEvents(client);
                            message.Send(new DataModel(null, null, "Subscribed to " + dataModel.Key + " events successfully.")); 
                            Log("Thread ID: " + Thread.CurrentThread.ManagedThreadId + ". SUBSCRIBE Request for " + dataModel.Key + " Event from Client: " + client.RemoteEndPoint + ". Subscribed successfully.");
                        }
                        else
                        {
                            subscription.Subscribe(new EventRegisterationModel(dataModel.Key, client));
                            message.Send(new DataModel(null, null, "Subscribed to " + dataModel.Key + " event successfully."));
                            Log("Thread ID: " + Thread.CurrentThread.ManagedThreadId + ". SUBSCRIBE Request for " + dataModel.Key + " Event from Client: " + client.RemoteEndPoint + ". Subscribed successfully.");
                        }
                    }
                    break;

                case "unsubscribe":
                    if (subscription.isAlreadySubscribed(client, dataModel.Key) || dataModel.Key.Equals("All"))
                    {
                        if (dataModel.Key.Equals("All"))
                        {
                            subscription.UnsubscribeAllEvents(client);
                            message.Send(new DataModel(null, null, "Unsubscribed from " + dataModel.Key + " events successfully."));
                            Log("Thread ID: " + Thread.CurrentThread.ManagedThreadId + ". UNSUBSCRIBE Request for " + dataModel.Key + " Event from Client: " + client.RemoteEndPoint + ". Unsubscribed successfully.");
                        }
                        else
                        {
                            subscription.Unsubscribe(client, dataModel.Key);
                            message.Send(new DataModel(null, null, "Unsubscribed from " + dataModel.Key + " event successfully."));
                            Log("Thread ID: " + Thread.CurrentThread.ManagedThreadId + ". UNSUBSCRIBE Request for " + dataModel.Key + " Event from Client: " + client.RemoteEndPoint + ". Unsubscribed successfully.");

                        }
                    }
                    else
                    {
                        message.Send(new DataModel(null, null, "Couldn't Unsubscribed from " + dataModel.Key + " event. You have not subscribed for this event"));
                        Log("Thread ID: " + Thread.CurrentThread.ManagedThreadId + ". UNSUBSCRIBE Request for " + dataModel.Key + " Event from Client: " + client.RemoteEndPoint + ". Couldn't unsubscribe. Client has not subscribed to this event");
                    }
                    break;
            }
        }

        public static void Log(string logMessage)
        {
            using (StreamWriter w = File.AppendText("log.txt"))
            {
                w.WriteLine($"{DateTime.Now.ToLongTimeString()} {DateTime.Now.ToLongDateString()}");
                w.WriteLine("  :");
                w.WriteLine($"  :{logMessage}");
                w.WriteLine("-------------------------------\n");
            }
        }

    }
}