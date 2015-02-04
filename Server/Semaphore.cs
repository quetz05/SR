using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SR
{
    class ForeignSemaphores : Dictionary<String, Tuple<String, int>>
    {
        public bool Exist(String name)
        {
            return ContainsKey(name);
        }

        public bool CreateSemaphore(String name, int startValue)
        {

            // send to other servers and wait

            return true;
        }

        public bool DestroySemaphore(String name)
        {
            // send to other servers and wait

            return true;

        }

        public bool P(String name)
        {
            // send to other servers and wait

            return true;
        }

        public bool V(String name)
        {
            // send to other servers and wait

            return true;
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

        public bool P(String name)
        {
            return this[name].P();
        }

        public bool V(String name)
        {
            return this[name].V();
        }

    }


    class Semaphore
    {
        public Semaphore(String name, int startValue)
        {
            this.name = name;
            this.value = startValue;
        }

        public bool P()
        {
            if (value > 0)
            {
                value--;
                return true;
            }
            else
                return false;
        }

        public bool V()
        {
            value++;
            return true;
        }

        String name;
        int value;
    }
}
