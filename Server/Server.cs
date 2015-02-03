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

    class Task
    {
        Task(Message.MessageType type, int client, int val)
        {


        }



    }

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
        Responder clientResponder;
        Responder serverResponder;

        bool CheckServersAlive()
        {
            foreach (var x in Servers)
                if (x.alive)
                    return true;

            return false;
        }

        Semaphores semaphores;
        ForeignSemaphores fSemaphores;

        ZMQ.Context context;
        ZMQ.Socket recvClientSocket;
        ZMQ.Socket recvServerSocket;
        Thread tCliets;
        Thread tServers;
        Thread tCResponder;
        Thread tSResponder;


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


            clientResponder = new Responder("C", Clients);
            serverResponder = new Responder("S", Servers);

            semaphores = new Semaphores();
            fSemaphores = new ForeignSemaphores();

            context = new ZMQ.Context();
            recvClientSocket = context.Socket(ZMQ.SocketType.DEALER);
            recvServerSocket.Bind("tcp://*:5556");

            recvClientSocket = context.Socket(ZMQ.SocketType.DEALER);
            recvClientSocket.Bind("tcp://*:5557");

            tCliets = new Thread(ReceiveClient);
            tServers = new Thread(ReceiveServer);
            tCResponder = new Thread(clientResponder.Run);
            tSResponder = new Thread(serverResponder.Run);

        }

        public void Run()
        {
            tCResponder.Start();
            tSResponder.Start();
            tServers.Start();
            tCliets.Start();
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
                serverResponder.Add(msg);
            }
        }

        public void ReceiveClient()
        {
            Console.WriteLine("C::" + DateTime.Now + "> Listening started...");
            while (true)
            {
                Message msg = Receive(recvClientSocket);
                clientResponder.Add(msg);
            }
        }





    }
}
