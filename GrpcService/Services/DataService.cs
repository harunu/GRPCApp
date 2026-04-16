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

            string path = Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "meterusage.csv");

            using var reader = new StreamReader(path);
            string line; bool isFirstLine = true;
            while ((line = reader.ReadLine()) != null)
            {
                if (isFirstLine)
                {
                    isFirstLine = false;
                    continue;
                }

                var pieces = line.Split(',');
                
                // FIX #2: Validate CSV structure before processing
                if (pieces.Length < 2)
                {
                    // Skip malformed lines instead of crashing
                    continue;
                }

                var _model = new Protos.UsageDataModel();

                // FIX #2: Graceful date parsing - skip invalid records
                if (DateTime.TryParse(pieces[0], out DateTime dateTime))
                {
                    _model.Time = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(dateTime.ToUniversalTime());
                }
                else
                {
                    // Skip lines with invalid dates instead of using DateTime.MinValue
                    continue;
                }

                _model.Meterusage = pieces[1];

                try
                {
                    await responseStream.WriteAsync(_model);
                }
                catch (Exception ex)
                {
                    // FIX #2: Do not expose full stack trace to clients
                    throw new RpcException(new Status(StatusCode.Internal, "Failed to stream data"));
                }
            }
        }
    } 
}