using Suborner.Core;
using System;
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
                    "/username:[string]",
                    "Username for the new suborner account. Default = <HOSTNAME>$",
                    x => x.IsCompoundSwitch));
            analyzer.AddArgumentVerifier(
                new ArgumentDefinition("password",
                    "/password:[string]",
                    "Password for the new suborner account. Default = Password.1",
                    x => x.IsCompoundSwitch));
            analyzer.AddArgumentVerifier(
                new ArgumentDefinition("rid",
                    "/rid:[decimal int]",
                    "RID for the new suborner account. Default = Next RID available",
                    x => x.IsCompoundSwitch));
            analyzer.AddArgumentVerifier(
                new ArgumentDefinition("ridhijack",
                    "/ridhijack:[decimal int]",
                    "RID of the account to impersonate. Default = 500 (Administrator)",
                    x => x.IsCompoundSwitch));
            analyzer.AddArgumentVerifier(
                new ArgumentDefinition("template",
                    "/template:[decimal int]",
                    "RID of the account to use as template for the new account creation. Default = 500 (Administrator)",
                    x => x.IsCompoundSwitch));
            analyzer.AddArgumentVerifier(
                new ArgumentDefinition("machineaccount",
                    "/machineaccount:[yes/no]",
                    "Forge as machine account for extra stealthiness. Default = yes",
                    x => x.IsCompoundSwitch));
            analyzer.AddArgumentVerifier(
                new ArgumentDefinition("debug",
                    "/debug",
                    "Enable debug mode for verbose logging. Default = disabled",
                    x => x.IsSimpleSwitch));

            if (!analyzer.VerifyArguments(arguments))
            {
                Logger.PrintError("Error parsing arguments");
                Logger.ShowUsage(analyzer);
                System.Environment.Exit(1);
                return;
            }

            // Holders with default parameter values
            string username = "<HOSTNAME>$";
            string password = "Password.1";
            string machineAccountSt = "";
            int rid = 0;
            int ridHijack = 500;
            int templateRid = 500;
            bool machineAccount = true;
            bool isDebug = false;

            // Parse arguments
            analyzer.AddArgumentAction("USERNAME", x => { username = x.SubArguments[0].Trim(); });
            analyzer.AddArgumentAction("PASSWORD", x => { password = x.SubArguments[0].Trim(); });
            analyzer.AddArgumentAction("RID", x => { rid = Convert.ToInt32(x.SubArguments[0].Trim()); });
            analyzer.AddArgumentAction("RIDHIJACK", x => { ridHijack = Convert.ToInt32(x.SubArguments[0].Trim()); });
            analyzer.AddArgumentAction("TEMPLATE", x => { templateRid = Convert.ToInt32(x.SubArguments[0].Trim()); });
            analyzer.AddArgumentAction("MACHINEACCOUNT", x => { machineAccountSt = x.SubArguments[0].Trim(); }); 
            analyzer.AddArgumentAction("DEBUG", x => { isDebug = true; }); // TODO: Make this false with flag
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
