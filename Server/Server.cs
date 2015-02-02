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



    class Server
    {

        private String[] ServerList = { "localhost" };
        private String[] clientsList;

        //List<Session> Clients;
        //List<Session> Servers;

        public Server()
        {
            clientsList = new String[128];

            context = new ZMQ.Context();

            recvServerSocket = context.Socket(ZMQ.SocketType.DEALER);
            recvServerSocket.Bind("tcp://*:5556");

            recvClientSocket = context.Socket(ZMQ.SocketType.DEALER);
            recvClientSocket.Bind("tcp://*:5557");

            tCliets = new Thread(ReceiveClient);
            tServers = new Thread(ReceiveServer);

            //foreach (var server in ServerList)
            //    Servers.Add(new Session(server));


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
                                if (clientsList[msg.info.ipIndex] == null)
                                {
                                    clientsList[msg.info.ipIndex] = "Server " + msg.info.ipIndex;

                                    Console.WriteLine("C::" + DateTime.Now + "> " + ServerList[msg.info.ipIndex] + " log in.");

                                }
                                else
                                    Console.WriteLine("S::" + DateTime.Now + "> Receive HB from " + ServerList[msg.info.ipIndex]); 
                            }
                            break;
                        case Message.MessageType.CHECK_BLOCK:
                            {

                                Console.WriteLine("S::" + DateTime.Now + "> Receive CHECK_BLOCK from " + ServerList[msg.info.ipIndex]);
                            }
                            break;
                        case Message.MessageType.SEM_CHECK:
                            {

                                Console.WriteLine("S::" + DateTime.Now + "> Receive SEM_CHECK from " + ServerList[msg.info.ipIndex]);
                            }
                            break;
                        case Message.MessageType.SEM_CREATE:
                            {

                                Console.WriteLine("S::" + DateTime.Now + "> Receive SEM_CREATE from " + ServerList[msg.info.ipIndex]);
                            }
                            break;
                        case Message.MessageType.SEM_DESTROY:
                            {

                                Console.WriteLine("S::" + DateTime.Now + "> Receive SEM_DESTROY from " + ServerList[msg.info.ipIndex]);
                            }
                            break;
                        case Message.MessageType.SEM_P:
                            {

                                Console.WriteLine("S::" + DateTime.Now + "> Receive SEM_P from " + ServerList[msg.info.ipIndex]);
                            }
                            break;
                        case Message.MessageType.SEM_V:
                            {

                                Console.WriteLine("S::" + DateTime.Now + "> Receive SEM_V from " + ServerList[msg.info.ipIndex]);
                            }
                            break;
                        default: Console.WriteLine("S::" + DateTime.Now + "> Invalid MsgType from " + ServerList[msg.info.ipIndex]);
                            break;

                    }
                }
                else
                    Console.WriteLine("S::" + DateTime.Now + "> Empty message from " + ServerList[msg.info.ipIndex]);

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
                                if (clientsList[msg.info.ipIndex] == null)
                                {
                                    clientsList[msg.info.ipIndex] = "Client " + msg.info.ipIndex;

                                    Console.WriteLine("C::" + DateTime.Now + "> "+ clientsList[msg.info.ipIndex]+ " log in.");

                                }
                                else
                                    Console.WriteLine("C::" + DateTime.Now + "> Receive HB from " + clientsList[msg.info.ipIndex]);
                            }
                            break;
                        case Message.MessageType.CHECK_BLOCK:
                            {

                                Console.WriteLine("C::" + DateTime.Now + "> Receive CHECK_BLOCK from " + clientsList[msg.info.ipIndex]);
                            }
                            break;
                        case Message.MessageType.SEM_CHECK:
                            {

                                Console.WriteLine("C::" + DateTime.Now + "> Receive SEM_CHECK from " + clientsList[msg.info.ipIndex]);
                            }
                            break;
                        case Message.MessageType.SEM_CREATE:
                            {

                                Console.WriteLine("C::" + DateTime.Now + "> Receive SEM_CREATE from " + clientsList[msg.info.ipIndex]);
                            }
                            break;
                        case Message.MessageType.SEM_DESTROY:
                            {

                                Console.WriteLine("C::" + DateTime.Now + "> Receive SEM_DESTROY from " + clientsList[msg.info.ipIndex]);
                            }
                            break;
                        case Message.MessageType.SEM_P:
                            {

                                Console.WriteLine("C::" + DateTime.Now + "> Receive SEM_P from " + clientsList[msg.info.ipIndex]);
                            }
                            break;
                        case Message.MessageType.SEM_V:
                            {

                                Console.WriteLine("C::" + DateTime.Now + "> Receive SEM_V from " + clientsList[msg.info.ipIndex]);
                            }
                            break;
                        default: Console.WriteLine("C::" + DateTime.Now + "> Invalid MsgType from " + clientsList[msg.info.ipIndex]);
                            break;
                    }          
                }
                else
                    Console.WriteLine("S::" + DateTime.Now + "> Empty message from " + clientsList[msg.info.ipIndex]);
            }
        }



        ZMQ.Socket recvClientSocket;
        ZMQ.Socket recvServerSocket;
        Thread tCliets;
        Thread tServers;
        ZMQ.Context context;
    }
}
