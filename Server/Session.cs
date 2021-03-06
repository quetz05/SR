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
    class Session
    {

        public String ip;
        public String port;
        public ZMQ.Socket socket;
        public ZMQ.Context context;
        public System.Diagnostics.Stopwatch HBTimer;
        bool isReady;
        bool HBSending;

        protected Thread HBThread;
        public const int HB_TIME = 30000;
        public const int WAIT_TIME = 5000;
        Mutex sendMutex;

        public Session(String ip, String port)
        {
            this.ip = ip;
            this.port = port;
            HBSending = false;
            sendMutex = new Mutex();
            isReady = false;
            HBTimer = new System.Diagnostics.Stopwatch();
            
        }

        public void Connect(bool first = false)
        {
            HBSending = true;
            HBThread = new Thread(Heartbeat);
            context = new ZMQ.Context();
            socket = context.Socket(ZMQ.SocketType.DEALER);
            socket.Connect("tcp://" + ip + ":" + port);
            isReady = true;


            if (!first)
            {
                HBThread.Start();
                HBTimer.Start();
            }


        }

        public void Disconnect()
        {
            HBSending = false;
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

            while(HBSending)
            {
                Send(msg);
                Console.WriteLine("C::" + DateTime.Now + "> Sending heartbeat to " + ip);
                Thread.Sleep(HB_TIME);
            }
        }




    }
}
