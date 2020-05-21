using System;
using System.Collections.Generic;
using System.Text;

namespace XO.Responses
{
    public class LinkDeviceResponse
    {
        public List<LinkErrorValue> Errors { get; set; }

        public string Manufacturer { get; set; }
        public string Model { get; set; }
        public string TerminalId { get; set; }
        public string SerialNumber { get; set; }
        public List<string> Features { get; set; }
        public List<string> Configurations { get; set; }
        //CardWorkflowControls only used when request Action = 'DALStatus'; can be null
        //public LinkCardWorkflowControls CardWorkflowControls { get; set; }
    }
}
