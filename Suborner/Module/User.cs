namespace Suborner.Module
{
    public class User
    {
        public string Username{ get; set; }
        public string Password{ get; set; }
        public string Description{ get; set; }
        public int RID{ get; set; }
        public int FRID{ get; set; }    // For RID Hijacking
        public string Comment{ get; set; }
        public string UserComment{ get; set; }
        public string Homedir{ get; set; }
        public string Homedirconnect{ get; set; }
        public string ScriptPath{ get; set; }
        public string Workstations{ get; set; }
        public string HoursAllowed{ get; set; }
        public bool IsMachineAccount{ get; set; }
    }
}
