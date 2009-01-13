using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;
using System.ComponentModel;

namespace DEQServices
{
    /// <summary>
    /// Summary description for Message
    /// </summary>
    [DataContract]
    public class Message
    {
        [DataMember]
        public string User { get; set; }

        [DataMember]
        public string Text { get; set; }

        [DataMember]
        public DateTime Time { get; set; }
    }
}
