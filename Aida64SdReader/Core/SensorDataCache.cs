using Aida64SDPlugin.Core.Interfaces;
using Aida64SDPlugin.Models;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Aida64SDPlugin.Core
{
	public class SensorDataCache : ISensorCacheData
	{
		private ConcurrentDictionary<string, SensorDataModel> sensorDataCache = new ConcurrentDictionary<string, SensorDataModel>();

		public void AddToCache(Dictionary<string, SensorDataModel> sensorData)
		{
			foreach (var data in sensorData)
			{
				sensorDataCache.AddOrUpdate(data.Key, data.Value, (key, oldValue) => data.Value);
			}
		}

		public bool TryReadDataSensor(string sensorName, out SensorDataModel? result)
		{
			return sensorDataCache.TryGetValue(sensorName, out result);
		}

		public List<string> GetSensorsList()
		{
			return sensorDataCache.Keys.ToList();
		}
	}
}