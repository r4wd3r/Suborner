using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Suborner.UI
{
    public sealed class ArgumentSemanticAnalyzer
    {
        private List<ArgumentDefinition> argumentDefinitions =
        new List<ArgumentDefinition>();
        private Dictionary<string, Action<Argument>> argumentActions =
        new Dictionary<string, Action<Argument>>();
        public ReadOnlyCollection<Argument> UnrecognizedArguments { get; private set; }
        public ReadOnlyCollection<Argument> MalformedArguments { get; private set; }
        public ReadOnlyCollection<Argument> RepeatedArguments { get; private set; }
        public ReadOnlyCollection<ArgumentDefinition> ArgumentDefinitions =>
        new ReadOnlyCollection<ArgumentDefinition>(argumentDefinitions);
        public IEnumerable<string> DefinedSwitches =>
        from argumentDefinition in argumentDefinitions
        select argumentDefinition.ArgumentSwitch;
        public void AddArgumentVerifier(ArgumentDefinition verifier) =>
        argumentDefinitions.Add(verifier);
        public void RemoveArgumentVerifier(ArgumentDefinition verifier)
        {
            var verifiersToRemove = from v in argumentDefinitions
                                    where v.ArgumentSwitch == verifier.ArgumentSwitch
                                    select v;
            foreach (var v in verifiersToRemove)
                argumentDefinitions.Remove(v);
        }
        public void AddArgumentAction(string argumentSwitch, Action<Argument> action) =>
                                    argumentActions.Add(argumentSwitch, action);
        public void RemoveArgumentAction(string argumentSwitch)
        {
            if (argumentActions.Keys.Contains(argumentSwitch))
                argumentActions.Remove(argumentSwitch);
        }
        public bool VerifyArguments(IEnumerable<Argument> arguments)
        {
            // no parameter to verify with, fail.
            if (!argumentDefinitions.Any())
                return false;
            // Identify if any of the arguments are not defined
            this.UnrecognizedArguments =
            (from argument in arguments
             where !DefinedSwitches.Contains(argument.Switch.ToUpper())
             select argument).ToList().AsReadOnly();
            if (this.UnrecognizedArguments.Any())
                return false;
            //Check for all the arguments where the switch matches a known switch,
            //but our well-formedness predicate is false.
            this.MalformedArguments = (from argument in arguments
                                       join argumentDefinition in argumentDefinitions
                                       on argument.Switch.ToUpper() equals
                                       argumentDefinition.ArgumentSwitch
                                       where !argumentDefinition.Verify(argument)
                                       select argument).ToList().AsReadOnly();
            if (this.MalformedArguments.Any())
                return false;
            //Sort the arguments into "groups" by their switch, count every group,
            //and select any groups that contain more than one element,
            //We then get a read-only list of the items.
            this.RepeatedArguments =
            (from argumentGroup in
                 from argument in arguments
                 where !argument.IsSimple
                 group argument by argument.Switch.ToUpper()
             where argumentGroup.Count() > 1
             select argumentGroup).SelectMany(ag => ag).ToList().AsReadOnly();
            if (this.RepeatedArguments.Any())
                return false;
            return true;
        }
        public void EvaluateArguments(IEnumerable<Argument> arguments)
        {
            //Now we just apply each action:
            foreach (Argument argument in arguments)
                argumentActions[argument.Switch.ToUpper()](argument);
        }
        public string InvalidArgumentsDisplay()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat($"Invalid arguments: {Environment.NewLine}");
            // Add the unrecognized arguments
            FormatInvalidArguments(builder, this.UnrecognizedArguments,
                "Unrecognized argument: {0}{1}");
            // Add the malformed arguments
            FormatInvalidArguments(builder, this.MalformedArguments,
                "Malformed argument: {0}{1}");
            // For the repeated arguments, we want to group them for the display,
            // so group by switch and then add it to the string being built.
            var argumentGroups = from argument in this.RepeatedArguments
                                 group argument by argument.Switch.ToUpper() into ag
                                 select new { Switch = ag.Key, Instances = ag };
            foreach (var argumentGroup in argumentGroups)
            {
                builder.AppendFormat($"Repeated argument:" +
                    $"{argumentGroup.Switch}{Environment.NewLine}");
                FormatInvalidArguments(builder, argumentGroup.Instances.ToList(),
                "\t{0}{1}");
            }
            return builder.ToString();
        }
        private void FormatInvalidArguments(StringBuilder builder,
        IEnumerable<Argument> invalidArguments, string errorFormat)
        {
            if (invalidArguments != null)
            {
                foreach (Argument argument in invalidArguments)
                {
                    builder.AppendFormat(errorFormat,
                    argument.Original, Environment.NewLine);
                }
            }
        }
    }
}
