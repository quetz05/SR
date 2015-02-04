using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using protobuf;

namespace SR
{
    class Task
    {
        public Task(Message.MessageType type, String semName, int client, int servers)
        {
            this.type = type;
            this.client = client;
            this.servers = servers;
            this.semName = semName;
            timer = new System.Diagnostics.Stopwatch();
            timer.Start();
        }

        public bool isObsolete()
        {
            if (timer.ElapsedMilliseconds > Session.WAIT_TIME)
                return true;
            else
                return false;
        }

        public Message.MessageType type;
        public String semName;
        public int client;
        public int servers;
        public System.Diagnostics.Stopwatch timer;
    }
}
