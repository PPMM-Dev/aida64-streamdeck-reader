using System;

namespace Aida64SDPlugin.Models
{
	public class SensorDataModel
	{
		public required string Category {get; set;}
		public  required string Id { get; set; }
		public  required string Label { get; set; }
		public  required string Value { get; set; }
		public  DateTime LastUpdateTime { get; set; }
	}
}
