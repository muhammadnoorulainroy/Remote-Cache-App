using Cache_Client;
using Helper_Classes;
using User;
using System;
using System.Configuration;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Test_Application
{
    class Program
    {
        private static Client client;
        private static Client client2;
        static void Main(string[] args)
        {
            int port = Convert.ToInt32(ConfigurationManager.AppSettings["Port"]);

            IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Parse(ConfigurationManager.AppSettings["IPADDRESS"]), port);
            try
            {
                client = new Client(iPEndPoint, DisplayResponseEvents, DisplayDataChangeEvents);
                client.Initialize();

                client2 = new Client(iPEndPoint, DisplayResponseEvents, DisplayDataChangeEvents);
                client2.Initialize();
                /*client2 = new Client(iPEndPoint, DisplayCustomEvents, DisplayCustomDataModelEvents);
                client2.Initialize();
                Thread thread1 = new Thread(() => {
                    DataModel data = (DataModel)client.Get("bcv");
                    Customer customer = (Customer)data.Value;
                    if (data != null && data.Value != null)
                    {
                        Console.WriteLine("Key={0}\nCustomer Name={1}\nCustomer Age{2}", data.Key, customer.Name, customer.Age);
                        Console.ReadKey();
                    }
                    else
                    {
                        Console.WriteLine("Key not found");
                    }

                });*/
                Thread thread2 = new Thread(() => client.Add("3232", new Customer { Name = "Zeeshan", Age = 23 }));
                Thread thread3 = new Thread(() => client.Add("90878", new Customer { Name = "Zeeshan", Age = 23 }));


                Thread thread4 = new Thread(() => client2.Add("65464", new Customer { Name = "Zeeshan", Age = 23 }));
                Thread thread5 = new Thread(() => client2.Add("hgfh", new Customer { Name = "Zeeshan", Age = 23 }));
                Thread thread6 = new Thread(() => client2.Remove("3232"));

                //thread1.Start();
                thread2.Start();
                thread3.Start();
                thread4.Start();
                thread5.Start();
                thread6.Start();
                Console.WriteLine("thread 2 ID: " + thread2.ManagedThreadId);
                Console.WriteLine("thread 3 ID: " + thread3.ManagedThreadId);
                Console.WriteLine("thread 4 ID: " + thread4.ManagedThreadId);
                Console.WriteLine("thread 5 ID: " + thread5.ManagedThreadId);
                Console.WriteLine("thread 6 ID: " + thread6.ManagedThreadId);


                /*Thread[] threads = new Thread[20];

                for (int i = 0; i < threads.Length; i++)
                {
                    if (i % 2 == 0)
                    {
                        client = new Client(iPEndPoint, DisplayCustomEvents, DisplayCustomDataModelEvents);
                        client.Initialize();
                        Thread thread = new Thread(() => client.Add(new Random().Next(100).ToString(), i));
                        threads[i] = thread;
                        thread.Start(); 
                    }
                    else
                    {
                        client = new Client(iPEndPoint, DisplayCustomEvents, DisplayCustomDataModelEvents);
                        client.Initialize();
                        Thread thread = new Thread(() => client.Get(new Random().Next(100).ToString()));
                        threads[i] = thread;
                        thread.Start();
                    }
                }*/

                /* for (int i = 0; i < threads.Length; i++)
                 {
                     threads[i].Start();
                 }*/

                Console.ReadKey();

                //StartClient();
            }
            catch (SocketException)
            {
                Console.WriteLine("\nServer is disconnected");
                Console.ReadKey();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception Occurred! " + e.Message);
                Console.ReadKey();
            }
        }
        private static void StartClient()
        {
            bool isActive = true;

            while (isActive)
            {
                Thread.Sleep(500);
                Console.WriteLine("\n======= Main Menu =======\n");

                Console.WriteLine("1. ADD Customer in Cache");
                Console.WriteLine("2. REMOVE Customer from Cache");
                Console.WriteLine("3. GET Data from Cache");
                Console.WriteLine("4. CLEAR Cache Data");
                Console.WriteLine("5. DISPOSE");
                Console.WriteLine("6. SUBSCRIBE for event");
                Console.WriteLine("7. UNSUBSCRIBE from event");

                Console.Write("Enter your choice: ");

                string inputString = Console.ReadLine();


                if (Int32.TryParse(inputString, out int input))
                {
                    switch (input)
                    {
                        case 1:
                            Add();
                            break;
                        case 2:
                            Remove();
                            break;
                        case 3:
                            DataModel data = (DataModel)Get();
                            if(data != null && data.Value != null)
                            {
                                //Console.WriteLine("Found");
                                Customer customer = (Customer)data.Value;
                                Console.WriteLine("Key = {0}\nCustomer Name = {1}\nCustomer Age = {2}", data.Key, customer.Name, customer.Age);
                            }
                            else
                            {
                                Console.WriteLine("Key not found");
                            }
                            break;
                        case 4:
                            client.Clear();
                            break;
                        case 5:
                            client.Dispose();
                            isActive = false;
                            break;
                        case 6:
                            ShowSubMenu();
                            break;
                        case 7:
                            ShowUnsubMenu();
                            break;
                        default:
                            Console.WriteLine("Please enter number from the given choices");
                            break;
                    }
                }
                else
                    Console.WriteLine("Please enter valid input\n");
            }
        }
        private static void Add()
        {
            string key;
            Customer customer = new Customer();

            Console.Write("\nEnter key:");
            key = Console.ReadLine();

            Console.Write("\nEnter Customer Name:");
            customer.Name = Console.ReadLine();

            Console.Write("\nEnter Customer Age:");
            customer.Age = int.Parse(Console.ReadLine());

            client.Add(key, customer);
        }

        private static void Remove()
        {
            string key;
            Console.Write("\nEnter key:");
            key = Console.ReadLine();
            client.Remove(key);
        }

        private static object Get()
        {
            string key;
            Console.Write("\nEnter key:");
            key = Console.ReadLine();
            return client.Get(key);
        }

        private static void DisplayResponseEvents(object sender, ResponseEvent args)
        {
            Console.WriteLine(args.Message);
        }

        private static void DisplayDataChangeEvents(object sender, DataChangeEvent args)
        {
            DataModel dataModel = args.dataModel;
            string msg = dataModel.Message;
            
            if (msg.Equals("Notification"))
            {
                DataModel notification = (DataModel)dataModel.Value;
                if (notification.Value == null && notification.Key != null)
                {
                    Console.WriteLine("\nThis is a notification.\n" + notification.Message + "\nKey\t" + notification.Key);
                }   
                else
                {
                    Console.WriteLine("\nThis is a notification. \n" + notification.Message);
                }
            }
        }

        private static void ShowSubMenu()
        {
            Thread.Sleep(1000);

            Console.WriteLine("\n======= Subscription Menu =======\n");

            Console.WriteLine("1. SUBSCRIBE to ADD Event");
            Console.WriteLine("2. SUBSCRIBE to REMOVE Event");
            Console.WriteLine("3. SUBSCRIBE tO CLEAR Event");
            Console.WriteLine("4. SUBSCRIBE to ALL Events");

            Console.Write("Enter Input:");
            string inputString = Console.ReadLine();


            if (Int32.TryParse(inputString, out int input))
            {
                switch (input)
                {
                    case 1:
                        client.Sub("Add");
                        break;
                    case 2:
                        client.Sub("Remove");
                        break;
                    case 3:
                        client.Sub("Clear");
                        break;
                    case 4:
                        client.Sub("All");
                        break;
                    default:
                        Console.WriteLine("\n Please enter a number from show menu only:\n");
                        break;
                }
            }

            else
                Console.WriteLine("Enter a valid Number");
        }

        private static void ShowUnsubMenu()
        {
            Thread.Sleep(1000);
            Console.WriteLine("\n======= Unsubscription Menu =======\n");

            Console.WriteLine("1. UNSUBSCRIBE from ADD Event");
            Console.WriteLine("2. UNSUBSCRIBE from REMOVE Event");
            Console.WriteLine("3  UNSUBSCRIBE from CLEAR Event");
            Console.WriteLine("4. UNSUBSCRIBE from ALL Events");

            Console.Write("Enter Input:");
            string inputString = Console.ReadLine();


            if (Int32.TryParse(inputString, out int input))
            {
                switch (input)
                {
                    case 1:
                        client.UnSub("Add");
                        break;
                    case 2:
                        client.UnSub("Remove");
                        break;
                    case 3:
                        client.UnSub("Clear");
                        break;
                    case 4:
                        client.UnSub("All");
                        break;
                    default:
                        Console.WriteLine("\n Please enter a number from shown menu only:\n");
                        break;
                }
            }

            else
                Console.WriteLine("Enter a valid Number");
        }
    }
}