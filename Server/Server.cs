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
        public const int ipIndex = 100;

        List<Member> Clients;
        List<Member> Servers;
        Responder responder;

        bool CheckServersAlive()
        {
            foreach (var x in Servers)
                if (x.alive)
                    return true;

            return false;
        }

        static public Semaphores semaphores;
        static public ForeignSemaphores fSemaphores;

        ZMQ.Context context;
        ZMQ.Socket recvClientSocket;
        ZMQ.Socket recvServerSocket;
        Thread tCliets;
        Thread tServers;
        Thread tResponder;


        private void HBToServers()
        {
            foreach(var x in Servers)
            {
                if(x.ip != "xxx")
                {
                    x.session.Connect(true);
                }
            }

            Console.WriteLine("S::" + DateTime.Now + "> Sending HB to other servers..."); 

            Message msg = new Message();
            msg.info = new Message.Info();
            msg.info.ipIndex = ipIndex;
            msg.type = Message.MessageType.HB;


            foreach (var x in Servers)
                 x.session.Send(msg);

        }

        public Server()
        {
            // Lista serwerów (100 - 103)
            Servers = new List<Member>();
            Servers.Add(new Member("xxx", "Serwer ja", new Session("xxx", "6666"), false));
            Servers.Add(new Member("172.20.10.3", "Sopel", new Session("172.20.10.3", "5555"), false));
            Servers.Add(new Member("172.20.10.12", "Parowa", new Session("172.20.10.12", "5555"), false));
            Servers.Add(new Member("172.20.10.14", "Baryla", new Session("172.20.10.14", "5555"), false));

            // Lista klientów (10 - 13)
            Clients = new List<Member>();
            Clients.Add(new Member("localhost", "Klient ja", new Session("localhost", "6666"), false));




            responder = new Responder(Clients, Servers);

            semaphores = new Semaphores();
            fSemaphores = new ForeignSemaphores();

            context = new ZMQ.Context();
            recvServerSocket = context.Socket(ZMQ.SocketType.DEALER);
            recvServerSocket.Bind("tcp://*:5555");

            recvClientSocket = context.Socket(ZMQ.SocketType.DEALER);
            recvClientSocket.Bind("tcp://*:5556");
            tCliets = new Thread(ReceiveClient);
            tServers = new Thread(ReceiveServer);
            tResponder= new Thread(responder.Run);

        }

        public void Run()
        {
            tResponder.Start();
            tServers.Start();
            tCliets.Start();
            Thread.Sleep(1000);
            HBToServers();
        }

        public void Stop()
        {
            tResponder.Abort();
            tServers.Abort();
            tCliets.Abort();
        }


        private Message Receive(ZMQ.Socket s)
        {
            byte[] readBuffer = s.Recv(int.MaxValue);
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
                Message msg = Receive(recvServerSocket);
                responder.Add("S", msg);
            }
        }

        public void ReceiveClient()
        {
            Console.WriteLine("C::" + DateTime.Now + "> Listening started...");
            while (true)
            {
                Message msg = Receive(recvClientSocket);
                responder.Add("C", msg);
            }
        }





    }
}
