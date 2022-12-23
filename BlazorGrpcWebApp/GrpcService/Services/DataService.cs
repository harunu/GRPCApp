using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Hosting.Internal;
using System.Text.Json;


namespace GrpcService.Services
{
    public class DataService : Protos.DatasService.DatasServiceBase
    {

        public override async Task
            GetUsageData(Protos.Request request,
            IServerStreamWriter<Protos.UsageDataModel> responseStream, ServerCallContext context)
        {

            string path = Directory.GetCurrentDirectory() + "\\App_Data\\meterusage.csv";

            using var reader = new StreamReader(Path.Combine(path));
            string line; bool isFirstLine = true;
            while ((line = reader.ReadLine()) != null)
            {
                var pieces = line.Split(',');

                var _model = new Protos.UsageDataModel();

                try
                {
                    if (isFirstLine)
                    {
                        isFirstLine = false;
                        continue;
                    }
                    _model.Time = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime
                         ((DateTime.TryParse(pieces[0], out DateTime _dateShip) ? _dateShip : DateTime.MinValue).ToUniversalTime());
                    _model.Meterusage = pieces[1];

                    await responseStream.WriteAsync(_model);

                }
                catch (Exception ex)
                {
                    throw new RpcException(new Status(StatusCode.Internal, ex.ToString()));
                }
            }
        }
    } 
}
