using Aida64SDPlugin.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aida64SDPlugin.Core.Interfaces
{
	public interface ISensorCacheData
	{
		void AddToCache(Dictionary<string, SensorDataModel> sensorData);
		bool TryReadDataSensor(string sensorName, out SensorDataModel? result);
	}
}
