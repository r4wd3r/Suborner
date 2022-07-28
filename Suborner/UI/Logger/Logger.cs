using Suborner.Core;
using Suborner.UI.ProgramView;
using System;

namespace Suborner.UI
{
    /// <summary>
    /// Class <c>Logger</c> in charge of parsing output to stdout.
    /// </summary>
    public static class Logger
    {
        public static void PrintHeader()
        {
            Console.WriteLine(ProgramData.SEPARATOR_TITLE);
            Console.WriteLine(ProgramData.PROGRAM_HEADER);
            Console.WriteLine(ProgramData.SEPARATOR_TITLE);
        }
        public static void PrintUsage(ArgumentSemanticAnalyzer analyzer)
        {
            Console.WriteLine("{} allows the following arguments:", ProgramData.PROGRAM_HEADER);
            foreach (ArgumentDefinition definition in analyzer.ArgumentDefinitions)
            {
                Console.WriteLine($"\t{definition.ArgumentSwitch}:" +
                    $"({definition.Description}){Environment.NewLine}" +
                    $"\tSyntax: { definition.Syntax}");
            }
        }

        public static void PrintContext() {
            PrintSuccess("Suborner Account Data:");
            Console.WriteLine("- Username: {0} \n" +
                "- Password: {1} \n" +
                "- RID: {2} \n" +
                "- Template Account RID: {3}\n" +
                "- Account to hijack (RID): {4}\n" +
                "- Machine account: {5}",
                SubornerContext.Instance.User.Username,
                SubornerContext.Instance.User.Password,
                SubornerContext.Instance.User.RID,
                SubornerContext.Instance.TemplateAccountRID.ToString(),
                SubornerContext.Instance.User.FRID,
                SubornerContext.Instance.User.IsMachineAccount.ToString());
            Console.WriteLine("--------------------------------------------");
        }

        public static void PrintSuccess(string message)
        {
            Console.WriteLine("[+] {0}", message);
        }
        public static void PrintInfo(string message)
        {
            Console.WriteLine("[-] {0}", message);
        }
        public static void PrintError(string message) {
            Console.WriteLine("[x] {0}", message);
        }

        public static void PrintDebug(string message)
        {
            if (SubornerContext.Instance.IsDebug)
            {
                Console.WriteLine("[DEBUG] {0}", message);
            }
            
        }
        public static void PrintVArray(byte[] V)
        {
            Console.WriteLine(ProgramData.SEPARATOR_LOG);
            for (int i = 0; i < V.Length; i += 8)
            {
                Console.WriteLine(String.Format("{0:X4} | {1:X2} {2:X2} {3:X2} {4:X2} {5:X2} {6:X2} {7:X2} {8:X2} ", i, V[i], V[i + 1], V[i + 2], V[i + 3], V[i + 4], V[i + 5], V[i + 6], V[i + 7]));
            }
            Console.WriteLine(ProgramData.SEPARATOR_LOG);
        }

        public static void ShowUsage(ArgumentSemanticAnalyzer analyzer)
        {
            Console.WriteLine("\nDescription:\n\n    " + ProgramData.DESCRIPTION_USAGE);
            Console.WriteLine("\nParameters:\n");
            foreach (ArgumentDefinition definition in analyzer.ArgumentDefinitions)
            {
                Console.WriteLine($"    {definition.ArgumentSwitch}:" +
                    $" {definition.Description} {Environment.NewLine}" +
                    $"    Syntax: {definition.Syntax} \n");
            }
        }

    }
}
