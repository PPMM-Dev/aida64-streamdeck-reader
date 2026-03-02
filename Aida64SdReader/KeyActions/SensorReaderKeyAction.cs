using Aida64SDPlugin.Models;
using Aida64SdReader.Core;
using BarRaider.SdTools;
using BarRaider.SdTools.Events;
using BarRaider.SdTools.Wrappers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog.LayoutRenderers;
using NLog.LayoutRenderers.Wrappers;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Aida64SDPlugin.KeyActions
{


	[PluginActionId("com.ppmm.aida64reader.sensorreader.keyaction")]
	public class SensorReaderKeyAction : KeypadBase
	{

		[SupportedOSPlatform("windows")]
		private class PluginSettings
		{
			[JsonProperty(PropertyName = "sensors")]
			public List<SensorData>? Sensors { get; set; }

			[JsonProperty(PropertyName = "sensorValue")]
			public string? SensorValue { get; set; }

			public static PluginSettings CreateDefaultSettings()
			{
				PluginSettings instance = new PluginSettings();
				instance.Sensors = new List<SensorData>();

				instance.Sensors = GetSensorsDataList();
				return instance;
			}
		}

		private class SensorData
		{
			[JsonProperty(PropertyName = "sensorName")]
			public string? SensorName { get; set; }
		}

		private GraphDrawer graphDrawer = new GraphDrawer();

		[SupportedOSPlatform("windows")]
		private PluginSettings? _settings;

		[SupportedOSPlatform("windows")]
		public SensorReaderKeyAction(ISDConnection connection, InitialPayload payload) : base(connection, payload)
		{

			if (payload.Settings == null || payload.Settings.Count == 0)
			{
				this._settings = PluginSettings.CreateDefaultSettings();
				Connection.SetSettingsAsync(JObject.FromObject(_settings));
			}
			else
			{
				this._settings = payload.Settings.ToObject<PluginSettings>();
			}

			Connection.OnSendToPlugin += OnSendToPlugin;
		}

		[SupportedOSPlatform("windows")]
		private async void OnSendToPlugin(object? sender, SDEventReceivedEventArgs<SendToPlugin> e)
		{
			var payload = e.Event.Payload;
			if (payload.ContainsKey("method_to_start"))
			{
				var value = payload["method_to_start"]!.ToString();
				if (value == "refreshSensorList")
				{

					await RefreshSensorsList();
				}

			}
			Logger.Instance.LogMessage(TracingLevel.INFO, "Test");
		}

		[SupportedOSPlatform("windows")]
		public override void Dispose()
		{
			Connection.OnSendToPlugin -= OnSendToPlugin;
		}

		[SupportedOSPlatform("windows")]
		public override void KeyPressed(KeyPayload payload)
		{

		}

		public override void KeyReleased(KeyPayload payload)
		{
			Connection.ShowAlert();
		}

		[SupportedOSPlatform("windows")]
		public async override void OnTick()
		{
			SensorDataModel? value = null;
			if(!String.IsNullOrEmpty(this._settings?.SensorValue))
			{
				value = Aida64Plugin.Instance.GetDataFromCache(this._settings.SensorValue);
			}
			else
			{
				var defaultImage = graphDrawer.DrawStringFrame(string.Empty, "?");
				await Connection.SetImageAsync(defaultImage);
				defaultImage.Dispose();
				return;
			}

			if (value == null)
			{
				var defaultImage = graphDrawer.DrawStringFrame(string.Empty, "?");
				await Connection.SetImageAsync(defaultImage);
				defaultImage.Dispose();
				return;
			}

			var image = graphDrawer.DrawGraphFrame(value);
			await Connection.SetImageAsync(image);
			image.Dispose();
		}

		public override void ReceivedGlobalSettings(ReceivedGlobalSettingsPayload payload)
		{
			Logger.Instance.LogMessage(TracingLevel.INFO, $"Settings received {payload.Settings}");
		}

		[SupportedOSPlatform("windows")]
		public async override void ReceivedSettings(ReceivedSettingsPayload payload)
		{
			Logger.Instance.LogMessage(TracingLevel.INFO, $"Settings received {payload.Settings}");

			var settings = payload.Settings.ToObject<PluginSettings>();
			if (settings != null &&
			   this._settings != null &&
			   this._settings.SensorValue != settings.SensorValue)
			{
				this._settings.SensorValue = settings.SensorValue;
				await Connection.SetSettingsAsync(JObject.FromObject(_settings));
			}
		}


		[SupportedOSPlatform("windows")]
		private async Task RefreshSensorsList()
		{
			Logger.Instance.LogMessage(TracingLevel.INFO, "Refresh sensors list");

			if(_settings != null)
			{
				_settings.Sensors = new List<SensorData>();
				_settings.Sensors.AddRange(GetSensorsDataList());
				await Connection.SetSettingsAsync(JObject.FromObject(_settings));
			}
		}

		[SupportedOSPlatform("windows")]
		private static List<SensorData> GetSensorsDataList()
		{
			List<SensorData> sensorsDatas = new List<SensorData>();
			var sensors = Aida64Plugin.Instance.GetSensorsList();
			foreach(var sensor in sensors)
			{
				SensorData sensorData = new SensorData
				{
					SensorName = sensor,		
				};
				sensorsDatas.Add(sensorData);
			}

			return sensorsDatas;
		}
	}
}
