using System;
using System.Collections.Generic;
using System.Linq;

namespace Suborner.UI
{
    /// <summary>
    /// Class <c>ArgumentParser</c> processes the arguments received from the command line to 
    /// </summary>
    public static class ArgumentParser
    {
        public static void ParseArguments(string[] argumentStrings)
        {
            var arguments = (from argument in argumentStrings
                             select new Argument(argument)).ToArray();

            Console.Write("Command line: ");
            foreach (Argument a in arguments)
            {
                Console.Write($"{a.Original} ");
            }
            Console.WriteLine("");

            ArgumentSemanticAnalyzer analyzer = new ArgumentSemanticAnalyzer();
            analyzer.AddArgumentVerifier(
                new ArgumentDefinition("username",
                    "/username:[username for new account]",
                    "Specifies the username for the new suborner account. Default = <HOSTNAME>$",
                    x => x.IsCompoundSwitch));
            analyzer.AddArgumentVerifier(
                new ArgumentDefinition("password",
                    "/password:[password for new account]",
                    "Specifies the password for the new suborner account. Default = Password.1",
                    x => x.IsCompoundSwitch));
            analyzer.AddArgumentVerifier(
                new ArgumentDefinition("rid",
                    "/rid:[RID for new account]",
                    "The RID of the new suborner account. Default = Next RID available.",
                    x => x.IsCompoundSwitch));
            analyzer.AddArgumentVerifier(
                new ArgumentDefinition("ridhijack",
                    "/ridhijack:[Impersonated account RID]",
                    "The RID of the account to impersonate. Default = 500.",
                    x => x.IsCompoundSwitch));
            analyzer.AddArgumentVerifier(
                new ArgumentDefinition("machineaccount",
                    "/machineaccount",
                    "If this is specified, will create the account as a machine account. Default = True.",
                    x => x.IsSimpleSwitch));


            analyzer.AddArgumentVerifier(
                new ArgumentDefinition("trialMode",
                    "/trialmode",
                    "If this is specified it places the product into trial mode",
                    x => x.IsSimpleSwitch));
            analyzer.AddArgumentVerifier(
                new ArgumentDefinition("DEBUGOUTPUT",
                    "/debugoutput:[value1];[value2];[value3]",
                    "A listing of the files the debug output " +
                    "information will be written to",
                    x => x.IsComplexSwitch));
            analyzer.AddArgumentVerifier(
                new ArgumentDefinition("",
                    "[literal value]",
                    "A literal value",
                    x => x.IsSimple));

            if (!analyzer.VerifyArguments(arguments))
            {
                string invalidArguments = analyzer.InvalidArgumentsDisplay();
                Console.WriteLine(invalidArguments);
                ShowUsage(analyzer);
                return;
            }
            // Set up holders for the command line parsing results
            string username = string.Empty;
            string password = string.Empty;
            bool trialmode = false;
            IEnumerable<string> debugOutput = null;
            List<string> literals = new List<string>();

            //For each parsed argument, we want to apply an action,
            // so add them to the analyzer.
            analyzer.AddArgumentAction("USERNAME", x => { username = x.SubArguments[0]; });
            analyzer.AddArgumentAction("PASSWORD", x => { password = x.SubArguments[1]; });
            analyzer.AddArgumentAction("TRIALMODE", x => { trialmode = true; });
            analyzer.AddArgumentAction("DEBUGOUTPUT", x =>
            {
                debugOutput = x.SubArguments;
            });
            analyzer.AddArgumentAction("", x => { literals.Add(x.Original); });
            // check the arguments and run the actions
            analyzer.EvaluateArguments(arguments);

        }
        public static void ShowUsage(ArgumentSemanticAnalyzer analyzer)
        {
            Console.WriteLine("Program.exe allows the following arguments:");
            foreach (ArgumentDefinition definition in analyzer.ArgumentDefinitions)
            {
                Console.WriteLine($"\t{definition.ArgumentSwitch}:" +
                    $"({definition.Description}){Environment.NewLine}" +
                    $"\tSyntax: { definition.Syntax}");
            }
        }

    }
}
