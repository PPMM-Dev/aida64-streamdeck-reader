using Aida64SDPlugin.Core.Interfaces;
using Aida64SDPlugin.Parsing;
using BarRaider.SdTools;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Aida64SDPlugin.Core
{
	public class Aida64MemorySharedReader
	{
		const int BUFFER_SIZE = 64 * 1024;

		private volatile int _pollingInterval = 1000;
		private volatile bool readerRunning = false;
		private byte[] _buffer = new byte[BUFFER_SIZE];
		private CancellationTokenSource? _cts;
		private Task? _readerTask;

		private MemoryMappedFile? _memoryMappedFile = null;
		private MemoryMappedViewAccessor? _viewAccessor = null;
		private ISensorCacheData _cacheData;

		public Aida64MemorySharedReader(ISensorCacheData cacheData)
		{
			_cacheData = cacheData;
		}

		public bool ReaderRunning
		{
			get { return readerRunning; }
		}

		public int PollingInterval
		{
			get { return _pollingInterval; }
			set { _pollingInterval = value; }
		}


		[SupportedOSPlatform("windows")]
		public void StartReadSharedMemory()
		{
			if (_readerTask != null)
				return;

			_cts = new CancellationTokenSource();
			_readerTask = Task.Run(() => ReadSharedMemory(_cts.Token));
		}

		[SupportedOSPlatform("windows")]
		private async Task ReadSharedMemory(CancellationToken token)
		{
			try
			{
				readerRunning = true;
				while (!token.IsCancellationRequested)
				{
					
					if (_viewAccessor == null)
					{
						if (!TryOpenSharedMemory())
						{
							await Task.Delay(5000, token);  //if the data are not shared yet we will try later.
							continue;
						}
					}
					_viewAccessor?.ReadArray(0, _buffer, 0, _buffer.Length);

					string data = Encoding.ASCII.GetString(_buffer).TrimEnd('\0');

					var result = DataParser.ParseAida64XmlData(data);
					_cacheData.AddToCache(result);

					//TODO remove this
					Console.WriteLine($"Value count : {result.Count}");
					foreach (var item in result)
					{
						Console.WriteLine($" Id : {item.Key}, Value : {item.Value.Value}");
					}

					await Task.Delay(_pollingInterval, token);
				}
			}
			catch (Exception ex)
			{
				Logger.Instance.LogMessage(TracingLevel.ERROR, ex.Message);
			}
			finally
			{
				readerRunning = false;
			}
		}

		[SupportedOSPlatform("windows")]
		private bool TryOpenSharedMemory()
		{
			try
			{
				_memoryMappedFile = MemoryMappedFile.OpenExisting("AIDA64_SensorValues", MemoryMappedFileRights.Read);
				_viewAccessor = _memoryMappedFile.CreateViewAccessor(0, 0, MemoryMappedFileAccess.Read);
				return true;

			}
			catch (FileNotFoundException ex)
			{
				//TODO Adding logs
				Console.WriteLine($"AIDA64_SensorValues Not found {ex.Message}");
				Dispose();
				return false;
			}
			catch
			{
				//TODO adding logs
				return false;
			}
		}

		public async Task StopReadSharedMemory()
		{
			_cts?.Cancel();
			try
			{
				if (_readerTask != null)
					await _readerTask;
			}
			catch (Exception ex)
			{
				Logger.Instance.LogMessage(TracingLevel.ERROR, ex.Message);
			}
			finally
			{
				_cts?.Dispose();
				_cts = null;
				_readerTask = null;
				Dispose();
			}
		}

		private void Dispose()
		{
			_viewAccessor?.Dispose();
			_memoryMappedFile?.Dispose();
			_viewAccessor = null;
			_memoryMappedFile = null;
		}
	}
}
