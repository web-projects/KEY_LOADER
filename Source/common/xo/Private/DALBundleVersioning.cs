namespace XO.Private
{
    /// <summary>
    /// schema(7):
    /// {app},{ type},{ terminal_type},{ front_end},{ entity},{ version},{ date_code}
    /// 
    /// example:
    ///             firmware   : "Sphere,VIPA,,,,11,210517"
    ///                        : "NJT,VIPA,,,,11,210517"
    ///             
    ///             emv config : ",emv,attended,FD,,11,210517"
    ///                        : ",emv,unattended,FD,,17,210517"
    ///             
    ///             idle screen: ",idle,,,199,1,210517"
    ///                        : ",idle,,,250,1,210517"
    /// </summary>
    public class DALBundleVersioning
    {
        public string Application { get; set; }
        public string Type { get; set; }
        public string TerminalType { get; set; }
        public string FrontEnd { get; set; }
        public string Entity { get; set; }
        public string Version { get; set; }
        public string DateCode { get; set; }
    }
}
