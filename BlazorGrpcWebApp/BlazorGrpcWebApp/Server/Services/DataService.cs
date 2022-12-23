using Grpc.Core;
using BlazorGrpcWebApp.Server.Protos;
using BlazorGrpcWebApp.Shared;
using Grpc.Net.Client;
using System.Text.Json;
using BlazorGrpcWebApp.Shared.Interfaces;

namespace BlazorGrpcWebApp.Server.Services
{
    public class StreamService : IDataService
    {
        public async Task<List<PostData>> GetDatasFromGrpc()
        {
            List<PostData>? Datas = new List<PostData>();
            var channel = GrpcChannel.ForAddress("https://localhost:7144");
            int Count = 0;
            try
            {
                var client = new DatasService.DatasServiceClient(channel);

                using var call = client.GetUsageData(new Request { Filters = "" }
                  , deadline: DateTime.UtcNow.AddSeconds(60)
                );

                await foreach (var item in call.ResponseStream.ReadAllAsync())
                {
                    DateTimeOffset dateOffset = item.Time.ToDateTimeOffset();
                    DateTime date = item.Time.ToDateTime();

                    Console.WriteLine(String.Format("New Data Receieved at {0}-{1}", item.Time, item.Meterusage));
                    Count++;

                    Datas.Add(new PostData(item.Meterusage, Convert.ToDateTime(date)));
                }
                //  var jsonString = JsonSerializer.Serialize(Datas.ToArray());
                /* File.WriteAllText(@"C:\Desktop\test.json", jsonString);
                 Console.WriteLine(jsonString);*/
                Console.WriteLine("Stream ended: Total Datas: " + Count.ToString());

            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.DeadlineExceeded)
            {
                Console.WriteLine("Service timeout.");
            }
           // Console.ReadLine();
            return Datas;
        }

        public Task<string> GetTest()
        {
            return Task.Delay(10000)
           .ContinueWith(t => "Hello");
        }
    }
}
