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
        private Queue<Message> queue;
        private Mutex mutex;
        private String type;
        List<Member> Clients;
        List<Member> Servers;
        List<Task> taskList;


        public Responder(String type, List<Member> clients, List<Member> servers)
        {
            this.queue = new Queue<Message>();
            this.mutex = new Mutex();
            this.type = type;
            this.Clients = clients;
            this.Servers = servers;
        }

        public void Run()
        {
            while(true)
            {
                CheckHeartbeats(Servers);
                CheckHeartbeats(Clients);
                CheckTasks();
                Respond();

                Thread.Sleep(1);
            }


        }

        public void Add(Message msg)
        {
            mutex.WaitOne();
            queue.Enqueue(msg);
            mutex.ReleaseMutex();
        }

        private Message Get()
        {          
            mutex.WaitOne();
            Message msg = queue.Dequeue();
            mutex.ReleaseMutex();
            return msg;
        }


        private void CheckHeartbeats(List<Member> members)
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




        }

        private List<Member> ChooseMembers()
        {


            return Servers;
        }

        private void Respond()
        {
            while(queue.Count != 0)
            {
                Message msg = Get();

                if (msg != null)
                {
                    List<Member> members = ChooseMembers();

                    switch (msg.type)
                    {
                        case Message.MessageType.HB:
                            {
                                HB(msg, members);
                            }
                            break;
                        case Message.MessageType.CHECK_BLOCK:
                            {
                                CHECK_BLOCK(msg, members);
                            }
                            break;
                        case Message.MessageType.SEM_CHECK:
                            {
                                SEM_CHECK(msg, members);
                            }
                            break;
                        case Message.MessageType.SEM_CREATE:
                            {

                                SEM_CREATE(msg, members);
                            }
                            break;
                        case Message.MessageType.SEM_DESTROY:
                            {
                                SEM_DESTROY(msg, members);
                            }
                            break;
                        case Message.MessageType.SEM_P:
                            {
                                SEM_P(msg, members);
                            }
                            break;
                        case Message.MessageType.SEM_V:
                            {
                                SEM_V(msg, members);
                            }
                            break;
                        default: Console.WriteLine(type + "::" + DateTime.Now + "> Invalid MsgType from " + members[msg.info.ipIndex].name);
                            break;
                    }
                }
                else
                    Console.WriteLine(type + "::" + DateTime.Now + "> Empty message from " + members[msg.info.ipIndex].name);
            }


        }

        private void HB(Message msg, List<Member> Members)
        {
            if (Members[msg.info.ipIndex].alive == false)
            {
                Members[msg.info.ipIndex].alive = true;
                Members[msg.info.ipIndex].session.Connect();
                Console.WriteLine(type + "::" + DateTime.Now + "> " + Members[msg.info.ipIndex].name + " log in.");
            }
            else
            {
                Members[msg.info.ipIndex].session.HBTimer.Restart();
                Console.WriteLine(type + "::" + DateTime.Now + "> Receive HB from " + Members[msg.info.ipIndex].name); 
            }
        }

        private void SEM_CREATE(Message msg, List<Member> Members)
        {
            Console.WriteLine(type + "::" + DateTime.Now + "> Receive CHECK_BLOCK from " + Members[msg.info.ipIndex].name);

            if(Server.semaphores.Exist(msg.semOption.name))
            {
                //semafor istnieje na serwerze


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

        private void SEM_DESTROY(Message msg, List<Member> Members)
        {
            Console.WriteLine(type + "::" + DateTime.Now + "> Receive SEM_CHECK from " + Members[msg.info.ipIndex].name);

        }

        private void SEM_P(Message msg, List<Member> Members)
        {
            Console.WriteLine(type + "::" + DateTime.Now + "> Receive SEM_CREATE from " + Members[msg.info.ipIndex].name);

        }

        private void SEM_V(Message msg, List<Member> Members)
        {
            Console.WriteLine(type + "::" + DateTime.Now + "> Receive SEM_DESTROY from " + Members[msg.info.ipIndex].name);

        }

        private void SEM_CHECK(Message msg, List<Member> Members)
        {
            Console.WriteLine(type + "::" + DateTime.Now + "> Receive SEM_P from " + Members[msg.info.ipIndex].name);

        }

        private void CHECK_BLOCK(Message msg, List<Member> Members)
        {
            Console.WriteLine(type + "::" + DateTime.Now + "> Receive SEM_V from " + Members[msg.info.ipIndex].name);

        }

    }
}
