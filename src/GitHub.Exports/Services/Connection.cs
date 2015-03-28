using GitHub.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GitHub.Primitives;
using System.ComponentModel.Composition;

namespace GitHub.Services
{
    public class Connection : IConnection
    {
        public HostAddress HostAddress { get; set; }
        public string Username { get; set; }
    }
}
