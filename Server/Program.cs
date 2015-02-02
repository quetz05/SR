using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZMQ;

namespace SR
{
    class Program
    {
        public static void Main(string[] args)
        {

            Server server = new Server();
            server.Run();

            Console.ReadKey();


        }

    }
}
