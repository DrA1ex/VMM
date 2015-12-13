using System;
using System.Net;

namespace VMM.Helper
{
    public class TimedOutWebClient : WebClient
    {
        public TimeSpan TimeOut { get; set; }
        public TimedOutWebClient(TimeSpan timeOut)
        {
            TimeOut = timeOut;
        }
        protected override WebRequest GetWebRequest(Uri address)
        {
            var request = base.GetWebRequest(address);
            if(request != null)
            {
                request.Timeout = (int)TimeOut.TotalMilliseconds;
            }

            return request;
        }
    }
}
