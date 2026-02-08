using Aida64SDPlugin.Core;
using Aida64SDPlugin.Models;
using Aida64SDPlugin.Parsing;
using System;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;

namespace Aida64SDPlugin
{
	public class Aida64Plugin
	{
		[SupportedOSPlatform("windows")]
		private static readonly Lazy<Aida64Plugin> _instance = new Lazy<Aida64Plugin>(() => new Aida64Plugin(), true); 

		private Aida64ProcessWatcher? _processWatcher;
		private Aida64MemorySharedReader? _memorySharedReader;
		private SensorDataCache? _sensorDataCache;
		private DataParser? _dataParser;
		private SemaphoreSlim _semaphore = new SemaphoreSlim(1,1);

		[SupportedOSPlatform("windows")]
		public static Aida64Plugin Instance { get { return _instance.Value; } }

		[SupportedOSPlatform("windows")]
		public Aida64Plugin()
		{
			_processWatcher = new Aida64ProcessWatcher();
			_sensorDataCache = new SensorDataCache();
			_memorySharedReader = new Aida64MemorySharedReader(_sensorDataCache);
			_dataParser = new DataParser();

			_processWatcher.AidaStarted += OnAidaStarted;
			_processWatcher.AidaStopped += OnAidaStopped;
		}


		[SupportedOSPlatform("windows")]
		public async Task Start()
		{
			await _semaphore.WaitAsync();
			try
			{
				_processWatcher?.Start();
			}
			finally
			{
				_semaphore.Release();
			}
			
		}

		public async Task StopAsync()
		{
			await _semaphore.WaitAsync();
			try
			{
				if (_processWatcher != null)
					await _processWatcher.StopAsync();
			}
			finally
			{
				_semaphore.Release();
			}
		}

		public SensorDataModel? GetDataFromCache(string sensorDataName)
		{
			if (_sensorDataCache == null)
				return null;

			if (_sensorDataCache.TryReadDataSensor(sensorDataName, out var sensorData))
				return sensorData;

			return null;
		}

		[SupportedOSPlatform("windows")]
		private async void OnAidaStarted(object? sender, EventArgs args)
		{
			await _semaphore.WaitAsync();
			try
			{
				_memorySharedReader?.StartReadSharedMemory();
			}
			finally
			{
				_semaphore.Release();
			}
		}

		[SupportedOSPlatform("windows")]
		private async void OnAidaStopped(object? sender, EventArgs args)
		{
			await _semaphore.WaitAsync();
			try
			{
				if(_memorySharedReader != null)
					await _memorySharedReader.StopReadSharedMemory();
			}
			finally
			{
				_semaphore.Release();
			}
			
		}

		private bool IsAida64Running()
		{
			return _processWatcher?.IsAidaRunning ?? false;
		}
	}
}
