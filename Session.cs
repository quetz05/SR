using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Data.Common;
using System.Threading;

namespace SR
{
    class Session
    {

        private System.Timers.Timer timer;
        public String TargetIP;
        ZMQ.Socket socket;
        Thread rcvThread;
        Thread sndThread;
        ZMQ.Context context;

        public Session(String ip)
        {
            TargetIP = ip;
            Connect();
            TimerStart();

            sndThread = new Thread(Send);
            rcvThread = new Thread(Receive);

            //sndThread.Start();
            rcvThread.Start();

        }

        // Stworzenie nowego Timera, który co 30 sekund będzie wysyłał HB
        private void TimerStart()
        {
            timer = new System.Timers.Timer(5000);
            timer.Elapsed += new ElapsedEventHandler(SendHeartbeat);
            timer.Start();
        }

        private bool Connect()
        {
            context = new ZMQ.Context();

            socket = context.Socket(ZMQ.SocketType.PAIR);
            socket.Connect("tcp://" + TargetIP + ":5556");
            return true;
        }

        private void Send()
        {
            while(true)
            {
                Console.WriteLine("S: PISZE!");
                socket.Send("HI!!", Encoding.UTF8);
                Thread.Sleep(4000);
            }   
        }


        private void Receive()
        {
            while(true)
            {
                Console.WriteLine("R: ODBIERAM...");
                String readBuffer = socket.Recv(Encoding.UTF8);
                Console.WriteLine(readBuffer);

            }



        }

        public void TimerReset()
        {
            timer.Stop();
            timer.Start();
        }

        static void SendHeartbeat(object sender, ElapsedEventArgs e)
        {
            Console.WriteLine("(" + DateTime.Now + ") HB");
        }




    }
}
