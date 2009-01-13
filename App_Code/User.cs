using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace DEQServices
{
    /// <summary>
    /// Represents a user known to the system.
    /// </summary>
    [DataContract]
    public class User
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public DateTime LastMessageRequestTime { get; set; }

        [DataMember]
        public bool IsJoined { get; set; }

        [DataMember]
        public DateTime LastSeenTime { get; set; }
    }
}
