using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace M5.Common
{
    public class Permissions
    {
        public bool read = false;
        public bool write = false;
        public bool delete = false;
        public bool audit = false;
        public bool all = false;
        public Permissions(UserInfo u)
        {
            if (u.roleList.IndexOf(1) > -1) read = write = delete = audit = all = true;
            else if (u.roleList.IndexOf(2) > -1) read = write = delete = true;
            else if (u.roleList.IndexOf(3) > -1) read = write = true;
            else if (u.roleList.IndexOf(4) > -1) read = audit = true;
        }
    }
}
