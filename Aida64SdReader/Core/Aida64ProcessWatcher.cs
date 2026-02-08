using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;

namespace Aida64SDPlugin.Core
{
	public class Aida64ProcessWatcher
	{
		private bool _isAidaRunning = false;
		private CancellationTokenSource? _cts;
		private Task? _currentTask = null;

		public event EventHandler? AidaStarted;
		public event EventHandler? AidaStopped;

		public bool IsAidaRunning
		{
			get { return _isAidaRunning; }
		}

		[SupportedOSPlatform("windows")]
		public void Start()
		{
			if (_currentTask != null && !_currentTask.IsCompleted)
				return;

			_cts = new CancellationTokenSource();
			_currentTask = StartProcessWatcher(_cts.Token);
		}

		[SupportedOSPlatform("windows")]
		public async Task StartProcessWatcher(CancellationToken token)
		{
			while (!token.IsCancellationRequested)
			{
				using var processFounded = Process.GetProcessesByName("aida64").FirstOrDefault();
				if (processFounded != null)
				{
					if (!_isAidaRunning)
					{
						Console.WriteLine("Running");
						_isAidaRunning = true;
						AidaStarted?.Invoke(this, EventArgs.Empty);
					}
				}
				else
				{
					if (_isAidaRunning)
					{
						Console.WriteLine("Not running");
						_isAidaRunning = false;
						AidaStopped?.Invoke(this, EventArgs.Empty);

					}

				}
				await Task.Delay(5, token); // Check every 5 seconds if the process is running. Can't use ManagementEventWatcher since we need administrator right for this.
			}
		}

		public async Task StopAsync()
		{
			if (_cts == null)
				return;

			_cts?.Cancel();
			try
			{
				if (_currentTask != null)
					await _currentTask;
			}
			catch (OperationCanceledException)
			{
				Console.WriteLine("Operation canceled");
			}
			finally
			{
				_cts?.Dispose();
				_cts = null;
				_currentTask = null;
			}
		}
	}
}
