using System.Runtime.Serialization;

namespace BlazorGrpcWebApp.Shared;
[DataContract]
public class MyServiceResult
{
	[DataMember(Order = 1)]
	public string? NewText { get; set; }

	[DataMember(Order = 2)]
	public int NewValue { get; set; }
}
