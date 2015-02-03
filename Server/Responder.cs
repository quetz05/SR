using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using protobuf;


namespace SR
{
    class Responder
    {
        private Queue<Tuple<String, Message> > queue;
        private Mutex mutex;
        List<Member> Clients;
        List<Member> Servers;
        List<Task> taskList;


        public Responder(List<Member> clients, List<Member> servers)
        {
            this.queue = new Queue<Tuple<String, Message> >();
            this.mutex = new Mutex();
            this.taskList = new List<Task>();
    
            this.Clients = clients;
            this.Servers = servers;
        }

        public void Run()
        {
            while(true)
            {
                CheckHeartbeats(Servers, "S");
                CheckHeartbeats(Clients, "C");
                CheckTasks();
                Respond();

                Thread.Sleep(1);
            }
        }

        public void Add(String type, Message msg)
        {
            mutex.WaitOne();
            queue.Enqueue(new Tuple<String, Message>(type, msg));
            mutex.ReleaseMutex();
        }

        private Tuple<String, Message> Get()
        {          
            mutex.WaitOne();
            Tuple<String, Message> msg = queue.Dequeue();
            mutex.ReleaseMutex();
            return msg;
        }


        private void CheckHeartbeats(List<Member> members, String type)
        {
            foreach (var x in members)
               if(x.alive)
                   if(x.session.HBTimer.ElapsedMilliseconds > Session.HB_TIME + Session.WAIT_TIME)
                   {
                       x.alive = false;
                       x.session.Disconnect();
                       Console.WriteLine(type + "::" + DateTime.Now + "> " + x.name + " disconnected.");
                   }
        }

        private void CheckTasks()
        {
            foreach (var task in taskList)
                if (task.isObsolete())
                {

                    // wysłanie klientowi powiadomienia

                    taskList.Remove(task);
                }


        }

        private List<Member> ChooseMembers(String type)
        {
            if (type == "S")
                return Servers;
            else if (type == "C")
                return Servers;
            else
                return null;
        }

        private void Respond()
        {
            while(queue.Count != 0)
            {
                Tuple<String, Message> tuple = Get();
                Message msg = tuple.Item2;
                String type = tuple.Item1;
                List<Member> members = ChooseMembers(type);

                if (msg == null)
                {
                    Console.WriteLine(type + "::" + DateTime.Now + "> Empty message!");
                    return;
                }
                
                int index = 0;
                if (type == "C")
                    index = msg.info.ipIndex - 10;
                else
                    index = msg.info.ipIndex - 100;

                    switch (msg.type)
                    {
                        case Message.MessageType.HB:
                            {
                                HB(msg, members, type, index);
                            }
                            break;
                        case Message.MessageType.CHECK_BLOCK:
                            {
                                CHECK_BLOCK(msg, members, type, index);
                            }
                            break;
                        case Message.MessageType.SEM_CHECK:
                            {
                                SEM_CHECK(msg, members, type, index);
                            }
                            break;
                        case Message.MessageType.SEM_CREATE:
                            {

                                SEM_CREATE(msg, members, type, index);
                            }
                            break;
                        case Message.MessageType.SEM_DESTROY:
                            {
                                SEM_DESTROY(msg, members, type, index);
                            }
                            break;
                        case Message.MessageType.SEM_P:
                            {
                                SEM_P(msg, members, type, index);
                            }
                            break;
                        case Message.MessageType.SEM_V:
                            {
                                SEM_V(msg, members, type, index);
                            }
                            break;
                        default: Console.WriteLine(type + "::" + DateTime.Now + "> Invalid MsgType from " + members[index].name);
                            break;
                    }
            }
        }

        private void HB(Message msg, List<Member> Members, String type, int index)
        {
            if (Members[index].alive == false)
            {
                Members[index].alive = true;
                Members[index].session.Connect();
                Console.WriteLine(type + "::" + DateTime.Now + "> " + Members[index].name + " log in.");
            }
            else
            {
                Members[index].session.HBTimer.Restart();
                Console.WriteLine(type + "::" + DateTime.Now + "> Receive HB from " + Members[index].name); 
            }
        }

        private void SEM_CREATE(Message msg, List<Member> Members, String type, int index)
        {
            Console.WriteLine(type + "::" + DateTime.Now + "> Receive CHECK_BLOCK from " + Members[index].name);

            if(Server.semaphores.Exist(msg.semOption.name))
            {
                Message response = new Message();
                response.info = new Message.Info();
                response.info.ipIndex = Server.ipIndex;

                response.semOption = new Message.SemOptions();
                response.semOption.name = msg.semOption.name;
                response.semOption.value = msg.semOption.value;

                if(type == "C")
                {
                    response.response = Message.Response.ERROR;
                    Members[index].session.Send(msg);

                }
                else if(type == "S")
                {
                    // ERROR


                }



            }
            else if (Server.fSemaphores.Exist(msg.semOption.name))
            {
                //semafor istnieje na innym znanym serwerze


            }
            else
            {
                // nowy task + zapytanie do innych serwerów


            }

        }

        private void SEM_DESTROY(Message msg, List<Member> Members, String type, int index)
        {
            Console.WriteLine(type + "::" + DateTime.Now + "> Receive SEM_CHECK from " + Members[index].name);

        }

        private void SEM_P(Message msg, List<Member> Members, String type, int index)
        {
            Console.WriteLine(type + "::" + DateTime.Now + "> Receive SEM_CREATE from " + Members[index].name);

        }

        private void SEM_V(Message msg, List<Member> Members, String type, int index)
        {
            Console.WriteLine(type + "::" + DateTime.Now + "> Receive SEM_DESTROY from " + Members[index].name);

        }

        private void SEM_CHECK(Message msg, List<Member> Members, String type, int index)
        {
            Console.WriteLine(type + "::" + DateTime.Now + "> Receive SEM_P from " + Members[index].name);

        }

        private void CHECK_BLOCK(Message msg, List<Member> Members, String type, int index)
        {
            Console.WriteLine(type + "::" + DateTime.Now + "> Receive SEM_V from " + Members[index].name);

        }

    }
}
