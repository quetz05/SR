using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;

namespace SR
{
    class Server
    {
        private List<Session> Sessions;
        



        public Server()
        {
            Sessions = new List<Session>();
            Sessions.Add(new Session("localhost"));

            
            //using (var file = File.Create("person.bin"))
            //{
            //    Serializer.Serialize(file, person);
            //}



        }
    }
}
