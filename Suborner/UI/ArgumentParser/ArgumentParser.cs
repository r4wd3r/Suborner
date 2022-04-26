using Suborner.Core;
using Suborner.Module;
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
                    "The RID of the new suborner account. Default = Next RID available",
                    x => x.IsCompoundSwitch));
            analyzer.AddArgumentVerifier(
                new ArgumentDefinition("ridhijack",
                    "/ridhijack:[Impersonated account RID]",
                    "The RID of the account to impersonate. Default = 500",
                    x => x.IsCompoundSwitch));
            analyzer.AddArgumentVerifier(
                new ArgumentDefinition("template",
                    "/template:[Account Name]",
                    "RID of the template account to use for the new suborner. Default = Administrator (500)",
                    x => x.IsCompoundSwitch));
            analyzer.AddArgumentVerifier(
                new ArgumentDefinition("machineaccount",
                    "/machineaccount:[yes/no]",
                    "Set as machine account. If no, you will lose some stealthiness Default = yes",
                    x => x.IsCompoundSwitch));
            analyzer.AddArgumentVerifier(
                new ArgumentDefinition("debug",
                    "/debug",
                    "Enables the debug mode for extra logging",
                    x => x.IsSimpleSwitch));


            if (!analyzer.VerifyArguments(arguments))
            {
                // string invalidArguments = analyzer.InvalidArgumentsDisplay();
                Printer.PrintError("Error parsing arguments");
                Printer.ShowUsage(analyzer);
                System.Environment.Exit(1);
                return;
            }

            // Set up holders for the command line parsing results with default values
            string username = "<HOSTNAME>$";
            string password = "Password.1";
            string machineAccountSt = "";
            int rid = 0;
            int ridHijack = 500;
            int templateRid = 500;
            bool machineAccount = true;
            bool isDebug = false;
           

            //For each parsed argument, we want to apply an action,
            // so add them to the analyzer.
            analyzer.AddArgumentAction("USERNAME", x => { username = x.SubArguments[0].Trim(); });
            analyzer.AddArgumentAction("PASSWORD", x => { password = x.SubArguments[0].Trim(); });
            analyzer.AddArgumentAction("RID", x => { rid = Convert.ToInt32(x.SubArguments[0].Trim()); });
            analyzer.AddArgumentAction("RIDHIJACK", x => { ridHijack = Convert.ToInt32(x.SubArguments[0].Trim()); });
            analyzer.AddArgumentAction("TEMPLATE", x => { templateRid = Convert.ToInt32(x.SubArguments[0].Trim()); });
            analyzer.AddArgumentAction("MACHINEACCOUNT", x => { machineAccountSt = x.SubArguments[0].Trim(); }); 
            analyzer.AddArgumentAction("DEBUG", x => { isDebug = true; }); // TODO: Check how to make this false with flag
            analyzer.EvaluateArguments(arguments);

            if (machineAccountSt.ToLower().Equals("no")) {
                machineAccount = false;
            }


            // Load evaluated arguments to Suborner context
            SubornerContext.Instance.TemplateAccountRID = templateRid;
            SubornerContext.Instance.User.Username = username;
            SubornerContext.Instance.User.Password = password;
            SubornerContext.Instance.User.RID = rid;
            SubornerContext.Instance.User.FRID = ridHijack;
            SubornerContext.Instance.User.IsMachineAccount = machineAccount;
            SubornerContext.Instance.IsDebug = isDebug;
        }
        

    }
}
