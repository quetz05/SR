using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Timers;
using System.IO;
using protobuf;
using System.Data.Common;

namespace SR
{

    class State : List<Tuple<Message.MessageType, String>>
    {
        public bool Remove(Message.MessageType state, String sem)
        {
            return Remove(new Tuple<Message.MessageType, String>(state, sem));
        }

        public void Add(Message.MessageType state, String sem)
        {
            Add(new Tuple<Message.MessageType, String>(state, sem));
        }

        Tuple<Message.MessageType, String> GetLast()
        {
            return this.Last();
        }
    }

    class Session
    {
        public Session(String ip)
        {
            this.ip = ip;
            state = new State();
            sendMutex = new Mutex();
            context = new ZMQ.Context();

        }

        public void Connect()
        {     
            socket = context.Socket(ZMQ.SocketType.DEALER);
            socket.Connect("tcp://" + ip + ":5557");

            HBThread = new Thread(Heartbeat);
            HBThread.Start();
        }

        public void Disconnect()
        {
            HBThread.Abort();
            socket.Dispose();
        }

        private void Send(protobuf.Message msg, Message.MessageType type = Message.MessageType.HB)
        {
            MemoryStream outputStream = new MemoryStream();
            byte[] byteMsg;

            ProtoBuf.Serializer.Serialize(outputStream, msg);
            
            using (BinaryReader br = new BinaryReader(outputStream))
            {
                byteMsg = br.ReadBytes((int)outputStream.Length);
            }

            if(type != Message.MessageType.HB)
            {
                state.Add(type, msg.semOption.name);
            }

            sendMutex.WaitOne();
            socket.Send(byteMsg);
            sendMutex.ReleaseMutex();
        }


        // Wysyłanie HB co 30 sekund
        private void Heartbeat()
        {
            protobuf.Message msg = new protobuf.Message();
            msg.type = Message.MessageType.HB;
            msg.info = new Message.Info();
            msg.info.ipIndex = Server.ipIndex;

            while(true)
            {
                Send(msg);
                Thread.Sleep(HB_TIME);
            }
        }



        public String ip;
        public ZMQ.Socket socket;
        public ZMQ.Context context;
        public State state;

        protected Thread HBThread;
        public const int HB_TIME = 30000; 
        Mutex sendMutex;

    }
}
