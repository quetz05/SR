using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using protobuf;
using System.IO;

namespace Client
{
    class Client
    {
        public Client(String ip)
        {
            this.ip = ip;

            context = new ZMQ.Context();

            socket = context.Socket(ZMQ.SocketType.DEALER);
            socket.Connect("tcp://" + ip + ":5557");

            Test();

        }

        public void Test()
        {

            while(true)
            {
                Message msg = new Message();

                msg.info = new Message.Info();
                msg.info.ipIndex = 0;
                msg.type = Message.MessageType.HB;

                Send(msg);


                msg = ReceiveMsg();

                

                Thread.Sleep(5000);
            }

        }


        public void Send(Message msg)
        {
            MemoryStream outputStream = new MemoryStream();
            byte[] byteMsg;

            ProtoBuf.Serializer.Serialize(outputStream, msg);
            byteMsg = outputStream.ToArray();

            socket.Send(byteMsg);
        }


        private protobuf.Message ReceiveMsg()
        {
            byte[] readBuffer = socket.Recv(int.MaxValue);
            if (readBuffer.Length == 0)
                return null;

            MemoryStream inputStream = new MemoryStream(readBuffer);
            protobuf.Message msg = ProtoBuf.Serializer.Deserialize<protobuf.Message>(inputStream);
            return msg;
        }


        public void Receive()
        {

            Console.WriteLine("S::" + DateTime.Now + "> Listening started...");

            while (true)
            {
                Message msg = ReceiveMsg();
                if (msg != null)
                {
                    switch (msg.type)
                    {
                        case Message.MessageType.HB:
                            {

                                Console.WriteLine("S::" + DateTime.Now + "> Receive HB from Server");
                            }
                            break;
                        case Message.MessageType.CHECK_BLOCK:
                            {

                                Console.WriteLine("S::" + DateTime.Now + "> Receive CHECK_BLOCK from Server");
                            }
                            break;
                        case Message.MessageType.SEM_CHECK:
                            {

                                Console.WriteLine("S::" + DateTime.Now + "> Receive SEM_CHECK from Server");
                            }
                            break;
                        case Message.MessageType.SEM_CREATE:
                            {

                                Console.WriteLine("S::" + DateTime.Now + "> Receive SEM_CREATE from Server");
                            }
                            break;
                        case Message.MessageType.SEM_DESTROY:
                            {

                                Console.WriteLine("S::" + DateTime.Now + "> Receive SEM_DESTROY from Server");
                            }
                            break;
                        case Message.MessageType.SEM_P:
                            {

                                Console.WriteLine("S::" + DateTime.Now + "> Receive SEM_P from Server");
                            }
                            break;
                        case Message.MessageType.SEM_V:
                            {

                                Console.WriteLine("S::" + DateTime.Now + "> Receive SEM_V from Server");
                            }
                            break;
                        default: Console.WriteLine("S::" + DateTime.Now + "> Invalid MsgType from Server");
                            break;

                    }
                }
            }

        }

        public String ip;
        public ZMQ.Socket socket;
        public ZMQ.Context context;

        //public System.Timers.Timer timerSend;
    }
}
