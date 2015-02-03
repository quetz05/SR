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

    class State : List<Tuple<int, String>>
    {
        public bool Remove(int state, String sem)
        {
            return Remove(new Tuple<int, String>(state, sem));
        }

        public void Add(int state, String sem)
        {
            Add(new Tuple<int, String>(state, sem));
        }

        Tuple<int, String> GetLast()
        {
            return this.Last();
        }
    }

    class Session
    {
        public Session(String ip)
        {
            this.ip = ip;
        }

        public void Connect()
        {
            context = new ZMQ.Context();

            socket = context.Socket(ZMQ.SocketType.DEALER);
            socket.Connect("tcp://" + ip + ":5557");

        }

        public void Disconnect()
        {
        }

        private void Send(protobuf.Message msg)
        {
            MemoryStream outputStream = new MemoryStream();
            byte[] byteMsg;

            ProtoBuf.Serializer.Serialize(outputStream, msg);
            
            using (BinaryReader br = new BinaryReader(outputStream))
            {
                byteMsg = br.ReadBytes((int)outputStream.Length);
            }
            socket.Send(byteMsg);
        }




        // Stworzenie nowego Timera, który co 30 sekund będzie wysyłał HB
        private void SenderTimerStart()
        {
            timerSend = new System.Timers.Timer(30000);
            timerSend.Elapsed += new ElapsedEventHandler(SendHeartbeat);
            timerSend.Start();
        }

        //resetowanie timera
        public void RecvTimerReset()
        {
            timerRecv.Stop();
            timerRecv.Start();
        }

        static void SendHeartbeat(object sender, ElapsedEventArgs e)
        {
            protobuf.Message msg = new protobuf.Message();
            msg.type = Message.MessageType.HB;
            //send
        }


        public String ip;
        //public Thread thread;
        public ZMQ.Socket socket;
        public ZMQ.Context context;
        public State state;
        public System.Timers.Timer timerSend;
        public System.Timers.Timer timerRecv;

    }
}
