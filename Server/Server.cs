using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Data.Common;
using System.Threading;
using System.IO;
using protobuf;


namespace SR
{


    class Member
    {
        public Member(String ip, String name, Session session, bool alive)
        {
            this.ip = ip;
            this.name = name;
            this.session = session;
            this.alive = alive;
        }

        public String ip;
        public Session session;
        public String name;
        public bool alive;
    }


    class Server
    {
        public const int ipIndex = 0;

        List<Member> Clients;
        List<Member> Servers;

        public Server()
        {
            // Lista serwerów
            Servers = new List<Member>();
            //Servers.Add(new Tuple<String, Session, bool>("localhost", new Session("localhost"), false));

            // Lista klientów
            Clients = new List<Member>();
            Clients.Add(new Member("localhost", "Lokalny ja", new Session("localhost"), false));
            //Clients.Add(new Member("localhost", "Parowa", new Session("localhost"), false));
            //Clients.Add(new Member("localhost", "Baryla", new Session("localhost"), false));
            //Clients.Add(new Member("localhost", "Sopel", new Session("localhost"), false));


            context = new ZMQ.Context();
            recvServerSocket = context.Socket(ZMQ.SocketType.DEALER);
            recvServerSocket.Bind("tcp://*:5556");

            recvClientSocket = context.Socket(ZMQ.SocketType.DEALER);
            recvClientSocket.Bind("tcp://*:5557");

            tCliets = new Thread(ReceiveClient);
            tServers = new Thread(ReceiveServer);


        }

        public void Run()
        {
            tServers.Start();
            tCliets.Start();
        }


        private protobuf.Message ReceiveClientMsg()
        {
            byte[] readBuffer = recvClientSocket.Recv(int.MaxValue);
            if (readBuffer.Length == 0)
                return null;

            MemoryStream inputStream = new MemoryStream(readBuffer);

            protobuf.Message msg = ProtoBuf.Serializer.Deserialize<protobuf.Message>(inputStream);

            return msg;
        }

        private protobuf.Message ReceiveServerMsg()
        {
            byte[] readBuffer = recvServerSocket.Recv(int.MaxValue);
            if (readBuffer.Length == 0)
                return null;

            MemoryStream inputStream = new MemoryStream(readBuffer);

            protobuf.Message msg = ProtoBuf.Serializer.Deserialize<protobuf.Message>(inputStream);

            return msg;
        }


        public void ReceiveServer()
        {
            Console.WriteLine("S::" + DateTime.Now + "> Listening started..."); 

            while (true)
            {
                Message msg = ReceiveServerMsg();
                if (msg != null)
                {
                    switch (msg.type)
                    {
                        case Message.MessageType.HB:
                            {
                                if (Servers[msg.info.ipIndex].alive== false)
                                {
                                    Servers[msg.info.ipIndex].alive = true;
                                    Servers[msg.info.ipIndex].session.Connect();
                                    Console.WriteLine("C::" + DateTime.Now + "> " + Servers[msg.info.ipIndex].name + " log in.");
                                }
                                else
                                    Console.WriteLine("S::" + DateTime.Now + "> Receive HB from " + Servers[msg.info.ipIndex].name); 
                            }
                            break;
                        case Message.MessageType.CHECK_BLOCK:
                            {

                                Console.WriteLine("S::" + DateTime.Now + "> Receive CHECK_BLOCK from " + Servers[msg.info.ipIndex].name);
                            }
                            break;
                        case Message.MessageType.SEM_CHECK:
                            {

                                Console.WriteLine("S::" + DateTime.Now + "> Receive SEM_CHECK from " + Servers[msg.info.ipIndex].name);
                            }
                            break;
                        case Message.MessageType.SEM_CREATE:
                            {

                                Console.WriteLine("S::" + DateTime.Now + "> Receive SEM_CREATE from " + Servers[msg.info.ipIndex].name);
                            }
                            break;
                        case Message.MessageType.SEM_DESTROY:
                            {

                                Console.WriteLine("S::" + DateTime.Now + "> Receive SEM_DESTROY from " + Servers[msg.info.ipIndex].name);
                            }
                            break;
                        case Message.MessageType.SEM_P:
                            {

                                Console.WriteLine("S::" + DateTime.Now + "> Receive SEM_P from " + Servers[msg.info.ipIndex].name);
                            }
                            break;
                        case Message.MessageType.SEM_V:
                            {

                                Console.WriteLine("S::" + DateTime.Now + "> Receive SEM_V from " + Servers[msg.info.ipIndex].name);
                            }
                            break;
                        default: Console.WriteLine("S::" + DateTime.Now + "> Invalid MsgType from " + Servers[msg.info.ipIndex].name);
                            break;

                    }
                }
                else
                    Console.WriteLine("S::" + DateTime.Now + "> Empty message from " + Servers[msg.info.ipIndex].name);

            }
        }

        public void ReceiveClient()
        {
            Console.WriteLine("C::" + DateTime.Now + "> Listening started..."); 

            while (true)
            {
                Message msg = ReceiveClientMsg();
                if (msg != null)
                {
                    switch (msg.type)
                    {
                        case Message.MessageType.HB:
                            {
                                if (Clients[msg.info.ipIndex].alive == false)
                                {
                                    Clients[msg.info.ipIndex].alive = true;
                                    Clients[msg.info.ipIndex].session.Connect();

                                    Console.WriteLine("C::" + DateTime.Now + "> "+ Clients[msg.info.ipIndex].name + " log in.");
                                }
                                else
                                    Console.WriteLine("C::" + DateTime.Now + "> Receive HB from " + Clients[msg.info.ipIndex].name);
                            }
                            break;
                        case Message.MessageType.CHECK_BLOCK:
                            {
                                // algorytm
                                Console.WriteLine("C::" + DateTime.Now + "> Receive CHECK_BLOCK from " + Clients[msg.info.ipIndex].name);
                            }
                            break;
                        case Message.MessageType.SEM_CHECK:
                            {

                                Console.WriteLine("C::" + DateTime.Now + "> Receive SEM_CHECK from " + Clients[msg.info.ipIndex].name);
                            }
                            break;
                        case Message.MessageType.SEM_CREATE:
                            {
                                String semName = msg.semOption.name;

                                



                                Console.WriteLine("C::" + DateTime.Now + "> Receive SEM_CREATE from " + Clients[msg.info.ipIndex].name);
                            }
                            break;
                        case Message.MessageType.SEM_DESTROY:
                            {

                                Console.WriteLine("C::" + DateTime.Now + "> Receive SEM_DESTROY from " + Clients[msg.info.ipIndex].name);
                            }
                            break;
                        case Message.MessageType.SEM_P:
                            {

                                Console.WriteLine("C::" + DateTime.Now + "> Receive SEM_P from " + Clients[msg.info.ipIndex].name);
                            }
                            break;
                        case Message.MessageType.SEM_V:
                            {

                                Console.WriteLine("C::" + DateTime.Now + "> Receive SEM_V from " + Clients[msg.info.ipIndex].name);
                            }
                            break;
                        default: Console.WriteLine("C::" + DateTime.Now + "> Invalid MsgType from " + Clients[msg.info.ipIndex].name);
                            break;
                    }          
                }
                else
                    Console.WriteLine("S::" + DateTime.Now + "> Empty message from " + Clients[msg.info.ipIndex].name);
            }
        }



        ZMQ.Socket recvClientSocket;
        ZMQ.Socket recvServerSocket;
        Thread tCliets;
        Thread tServers;
        ZMQ.Context context;
    }
}
