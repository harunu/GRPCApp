using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace BlazorGrpcWebApp.Shared.Interfaces
{
    [ServiceContract]
    public interface IDataService
    {
        Task<List<PostData>> GetDatasFromGrpc();
        Task<string> GetTest();
    }
}
