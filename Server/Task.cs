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
        public Task(Message.MessageType type, int client, int value)
        {
            this.type = type;
            this.client = client;
            this.value = value;
            timer.Start();
        }

        public Message.MessageType type;
        public int client;
        public int value;
        public System.Diagnostics.Stopwatch timer;
    }
}
