using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Suborner.Suborner
{
    public sealed class Context
    {
        // Singleton session object for unique data
        private static readonly Lazy<Context> lazy =
        new Lazy<Context>(() => new Context());

        public static Context Instance { get { return lazy.Value; } }
        private Context()
        {
        }
    }
}
