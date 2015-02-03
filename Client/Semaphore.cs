using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    class ClientSemaphore
    {
        static public bool IsExist(String name)
        {
            // send query

            // wait on response

            throw new Exception("Can't check if Semaphore " + name + " exist");
        }

        static public bool CreateSemaphore(String name, int startValue)
        {
            // send query

            // wait on response
            throw new Exception("Can't create Semaphore" + name);
        }

        static public bool DestroySemaphore(String name)
        {
            // send query

            // wait on response
            throw new Exception("Can't destroy Semaphore" + name);

        }

        static public bool P(String name)
        {
            // send query

            // wait on response
            throw new Exception("Can't do P() on Semaphore " + name);
        }

        static public bool V(String name)
        {
            // send query

            // wait on response
            throw new Exception("Can't do V() on Semaphore " + name);
        }
    }

}
