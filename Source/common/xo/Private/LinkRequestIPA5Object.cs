using System;
using System.Collections.Generic;
using XO.Responses;

namespace XO.Private
{
    public partial class LinkRequestIPA5Object
    {
        public Guid? ClientGuid { get; set; }
        public Guid RequestID { get; set; }
        public List<LinkActionResponse> LinkActionResponseList { get; set; }
        public bool IdleRequest { get; set; }
    }
}
