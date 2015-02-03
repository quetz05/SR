using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SR
{
    class Member
    {
        public Member(String ip, String name, Session session, bool alive)
        {
            this.ip = ip;
            this.name = name;
            this.session = session;
            this.alive = alive;
        }

        public String ip;
        public Session session;
        public String name;
        public bool alive;
    }
}
