using Suborner.UI;
using Suborner.Core;

namespace Suborner
{
    class Program
    {   
        static void Main(string[] argumentStrings)
        {
            Logger.PrintHeader();

            //1. Parse arguments and context validation
            ArgumentParser.ParseArguments(argumentStrings);
            SubornerContext.Instance.ValidateContext();
            Logger.PrintContext();

            //2. Validate if user account can be created (privileges, account exists, etc.)
            Core.Suborner.CraftAccount();

            //3. Write changes
            Core.Suborner.WriteChanges();
        }
    }
}
