using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thread_.NET.BLL.Exceptions
{
    internal class EmailTakenException: Exception
    {
        public EmailTakenException() : base("This email is already taken")
        {
        }
    }
}
