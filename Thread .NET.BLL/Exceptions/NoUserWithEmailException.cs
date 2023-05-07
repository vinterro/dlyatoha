using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thread_.NET.BLL.Exceptions
{
    public sealed class NoUserWithEmailException : Exception
    {
        

        public NoUserWithEmailException() : base($"No user with this email was not found.") { }
    }
}
