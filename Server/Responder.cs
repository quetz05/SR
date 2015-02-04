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


        private Task GetTask(Message.MessageType type, String semName, int client)
        {
            foreach (var x in taskList)
                if (x.client == client && x.type == type && x.semName == semName)
                    return x;

            return null;
        }

        private bool RemoveTask(Message.MessageType type, String semName, int client)
        {
            foreach (var x in taskList)
                if (x.client == client && x.type == type && x.semName == semName)
                {
                    taskList.Remove(x);
                    return true;
                }

            return false;
        }

        private int ChceckAliveness(List<Member> members)
        {
            int count = 0;
            foreach (var x in members)
                if (x.alive == true)
                    count++;
            return count;
        }

        private void SendToAll(List<Member> members, Message msg)
        {
            foreach (var x in members)
                if (x.alive == true)
                        x.session.Send(msg);
        }

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
                return Clients;
            else
                return null;
        }

        static public Message CreateMessage(Message receiveMsg, Message.MessageType type, Message.Response resp)
        {
            Message response = new Message();
            response.type = type;
            response.info = new Message.Info();
            response.info.ipIndex = Server.ipIndex;
            response.response = resp;

            if (type == Message.MessageType.SEM_CHECK || type == Message.MessageType.SEM_CREATE || type == Message.MessageType.SEM_DESTROY ||
                type == Message.MessageType.SEM_P || type == Message.MessageType.SEM_V)
            {
                response.semOption = new Message.SemOptions();
                response.semOption.name = receiveMsg.semOption.name;
                response.semOption.value = receiveMsg.semOption.value;
            }
            return response;
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
            Console.WriteLine(type + "::" + DateTime.Now + "> Receive SEM_CREATE from " + Members[index].name);
            Message response = null;

            if (type == "C")
            {
                // Semafor istnieje u nas lub u kogoś innego - ZWRÓC BLAD
                if (Server.semaphores.Exist(msg.semOption.name) || Server.fSemaphores.Exist(msg.semOption.name))
                {
                    response = CreateMessage(msg, Message.MessageType.SEM_CREATE, Message.Response.ERROR);
                    Members[index].session.Send(response);
                    Console.WriteLine(type + "::" + DateTime.Now + "> (SEM_CREATE) " + msg.semOption.name + " already exist!");
                }
                // Nie mamy wiedzy o semaforze
                else
                {
                    int serversAlive = ChceckAliveness(Servers);
                    // Wyślijmy ASKa do reszty serwerów o ile istnieją
                    if(serversAlive > 0)
                    {
                        taskList.Add(new Task(Message.MessageType.SEM_CREATE, msg.semOption.name, msg.info.ipIndex, serversAlive));
                        response = CreateMessage(msg, Message.MessageType.SEM_CREATE, Message.Response.ASK);
                        response.info.client = msg.info.ipIndex;
                        SendToAll(Servers, response);
                        Console.WriteLine(type + "::" + DateTime.Now + "> (SEM_CREATE) " + msg.semOption.name + " asking...");
                    }
                    // Utworzmy semafor - nie ma przeciwwskazań
                    else
                    {
                        Server.semaphores.CreateSemaphore(msg.semOption.name, msg.semOption.value);
                        response = CreateMessage(msg, Message.MessageType.SEM_CREATE, Message.Response.OK);
                        response.info.client = msg.info.ipIndex;
                        Members[index].session.Send(response);
                        Console.WriteLine(type + "::" + DateTime.Now + "> (SEM_CREATE) " + msg.semOption.name + " created.");
                    }
                    
                }

            }
            // Dostaliśmy wiadomość od serwera
            else if (type == "S")
            {
                // zapytanie od innego serwera
                if(msg.response == Message.Response.ASK)
                {
                    if(Server.semaphores.Exist(msg.semOption.name))
                        response = CreateMessage(msg, Message.MessageType.SEM_CREATE, Message.Response.NO);
                    else
                        response = CreateMessage(msg, Message.MessageType.SEM_CREATE, Message.Response.OK);

                    response.info.client = msg.info.client;
                    Members[index].session.Send(response);
                
                }
                // odpowiedź na nasze zapytanie
                else if(msg.response == Message.Response.OK || msg.response == Message.Response.NO)
                {
                    Task task = GetTask(msg.type, msg.semOption.name, msg.info.client);
                    
                    if(task != null)
                    {
                        // Semafor istnieje gdzie indziej
                        if(msg.response == Message.Response.NO)
                        {
                            RemoveTask(msg.type, msg.semOption.name, msg.info.client);
                            response = CreateMessage(msg, Message.MessageType.SEM_CREATE, Message.Response.ERROR);
                            Members[index].session.Send(response);
                            Console.WriteLine(type + "::" + DateTime.Now + "> (SEM_CREATE) " + msg.semOption.name + " already exist!");

                        }
                        // Pozwolenie 1 serwera na tworzenie semafora
                        else
                        {
                            task.servers--;
                            // dostaliśmy wszystkie odpowiedzi
                            if(task.servers == 0)
                            {
                                RemoveTask(msg.type, msg.semOption.name, msg.info.client);
                                Server.semaphores.CreateSemaphore(msg.semOption.name, msg.semOption.value);
                                response = CreateMessage(msg, Message.MessageType.SEM_CREATE, Message.Response.OK);
                                Members[index].session.Send(response);
                                Console.WriteLine(type + "::" + DateTime.Now + "> (SEM_CREATE) " + msg.semOption.name + " creating.");
                            }
                        }
                    }
                }
                // Typ ERROR - błąd
                else
                {
                    Console.WriteLine(type + "::" + DateTime.Now + "> ERROR MessageResponse from " + Members[index].name);
                }
            }
            else
            {
                Console.WriteLine(type + "::" + DateTime.Now + "> ERROR Bad typ!");
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
