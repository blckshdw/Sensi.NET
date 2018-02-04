using System;
using System.Collections.Generic;
using System.Text;

namespace Sensi.Response
{
    public class NegotiateResponse
    {
        public string Url { get; set; }
        public string ConnectionToken { get; set; }
        public string ConnectionId { get; set; }
        public double KeepAliveTimeout { get; set; }
        public double DisconnectTimeout { get; set; }
        public bool TryWebSockets { get; set; }
        public string ProtocolVersion { get; set; }
    }
}
