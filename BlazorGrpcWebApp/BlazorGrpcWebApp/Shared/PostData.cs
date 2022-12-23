using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace BlazorGrpcWebApp.Shared
{
    [DataContract]
    public class PostData
    {
        public PostData()
        {
        }
        public PostData(string meterusage, DateTime time)
        {
            Time = time;
            Meterusage = meterusage;
        }

        [DataMember(Order = 1)]
        public DateTime? Time { get; set; }

        [DataMember(Order = 2)]
        public string? Meterusage { get; set; }

    }
}
