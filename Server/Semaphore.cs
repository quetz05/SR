using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SR
{

    class ForeignSemaphore
    {
        public ForeignSemaphore(String name, int servId)
        {
            this.name = name;
            this.servId = servId;
            this.clients = new List<int>();
            this.waitingClients = new List<int>();

        }

        public String name;
        public int servId;
        public List<int> clients;
        public List<int> waitingClients;
    }

    class ForeignSemaphores : Dictionary<String, ForeignSemaphore>
    {
        public bool Exist(String name)
        {
            return ContainsKey(name);
        }

        public bool AddSemaphore(String name, int serverId)
        {
            if (Exist(name))
                return false;

            Add(name, new ForeignSemaphore(name, serverId));
            return true;
        }

        public int GetServerId(String name)
        {
            return this[name].servId;
        }

        public bool DestroySemaphore(String name)
        {
            return Remove(name);
        }

        public void AddWaitingClient(String name, int client)
        {
            this[name].waitingClients.Add(client);
        }

        public void AddClient(String name, int client)
        {
            this[name].clients.Add(client);
        }

        public bool RemoveClient(String name, int client)
        {
            return this[name].clients.Remove(client);
        }

        public bool RemoveWaitingClient(String name, int client)
        {
            return this[name].waitingClients.Remove(client);
        }

    }

    class Semaphores : Dictionary<String, Semaphore>
    {
        public bool Exist(String name)
        {
            return ContainsKey(name);
        }

        public bool CreateSemaphore(String name, int startValue)
        {
            if (Exist(name))
                return false;

            Add(name, new Semaphore(name, startValue));
            return true;
        }

        public bool DestroySemaphore(String name)
        {
            return Remove(name);
        }

        public bool P(String name, int client)
        {
            return this[name].P(client);
        }

        public bool V(String name, int client)
        {
            return this[name].V(client);
        }

        public bool Free(String name)
        {
            return this[name].Free();
        }
    }


    class Semaphore
    {
        public Semaphore(String name, int startValue)
        {
            this.name = name;
            this.value = startValue;
            clients = new List<int>();
            waitingClients = new List<int>();
        }

        public bool P(int client)
        {
            if (value > 0)
            {
                value--;
                clients.Remove(client);
                return true;
            }
            else
            {
                waitingClients.Add(client);
                return false;
            }
        }

        public bool V(int client)
        {
            value++;
            clients.Add(client);
            return true;
        }

        public bool Free()
        {
            if ((clients.Count + waitingClients.Count) == 0)
                return true;
            else
                return false;
        }

        String name;
        int value;
        public List<int> clients;
        public List<int> waitingClients;
    }
}
