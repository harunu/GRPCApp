﻿using System.Runtime.Serialization;

namespace BlazorGrpcWebApp.Shared;

[DataContract]
public class MyServiceRequest
{
	[DataMember(Order = 1)]
	public string? Text { get; set; }

	[DataMember(Order = 2)]
	public int Value { get; set; }
}
