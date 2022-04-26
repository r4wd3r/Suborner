using System;

namespace Suborner.UI
{
    public sealed class ArgumentDefinition
    {
        public string ArgumentSwitch { get; }
        public string Syntax { get; }
        public string Description { get; }
        public Func<Argument, bool> Verifier { get; }
        public ArgumentDefinition(string argumentSwitch,
                                string syntax,
                                string description,
                                Func<Argument, bool> verifier)
        {
            ArgumentSwitch = argumentSwitch.ToUpper();
            Syntax = syntax;
            Description = description;
            Verifier = verifier;
        }
        public bool Verify(Argument arg) => Verifier(arg);
    }
}
