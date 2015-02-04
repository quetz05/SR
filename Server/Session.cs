﻿using System;
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

    //class State : List<Tuple<Message.MessageType, String>>
    //{
    //    public bool Remove(Message.MessageType state, String sem)
    //    {
    //        return Remove(new Tuple<Message.MessageType, String>(state, sem));
    //    }

    //    public void Add(Message.MessageType state, String sem)
    //    {
    //        Add(new Tuple<Message.MessageType, String>(state, sem));
    //    }

    //    Tuple<Message.MessageType, String> GetLast()
    //    {
    //        return this.Last();
    //    }
    //}

    class Session
    {
        public Session(String ip)
        {
            this.ip = ip;
            sendMutex = new Mutex();
            isReady = false;
        }

        public void Connect()
        {
            context = new ZMQ.Context();
            socket = context.Socket(ZMQ.SocketType.DEALER);
            socket.Connect("tcp://" + ip + ":6666");
            
            //state = new State();

            HBThread = new Thread(Heartbeat);
            HBThread.Start();

            HBTimer = new System.Diagnostics.Stopwatch();
            HBTimer.Start();
            isReady = true;
        }

        public void Disconnect()
        {
            HBThread.Abort();
            HBTimer.Stop();
            HBTimer.Reset();
            socket.Dispose();
        }

        public void Send(protobuf.Message msg)
        {
            while (!isReady);

            MemoryStream outputStream = new MemoryStream();
            byte[] byteMsg;

            ProtoBuf.Serializer.Serialize(outputStream, msg);
            byteMsg = outputStream.ToArray();

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
                Console.WriteLine("C::" + DateTime.Now + "> Sending heartbeat to " + ip);
                Thread.Sleep(HB_TIME);
            }
        }



        public String ip;
        public ZMQ.Socket socket;
        public ZMQ.Context context;
        public System.Diagnostics.Stopwatch HBTimer;
        bool isReady;

        protected Thread HBThread;
        public const int HB_TIME = 30000;
        public const int WAIT_TIME = 5000; 
        Mutex sendMutex;

    }
}
