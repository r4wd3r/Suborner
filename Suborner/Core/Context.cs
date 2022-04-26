using Suborner.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Suborner.Suborner
{
    public sealed class Context
    {
        public User user { get; set; }
        public int templateAccountRID { get; set; }
        private Context()
        {
        }
        // Singleton session object for unique data
        private static readonly Lazy<Context> lazy =
        new Lazy<Context>(() => new Context());

        public static Context Instance { get { return lazy.Value; } }
        
    }
}
