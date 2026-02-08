using Aida64SdReader.Core;
using BarRaider.SdTools;
using BarRaider.SdTools.Wrappers;
using NLog.LayoutRenderers.Wrappers;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Drawing;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Aida64SDPlugin.KeyActions
{


	[PluginActionId("com.ppmm.aida64reader.sensorreader.keyaction")]
	public class SensorReaderKeyAction : KeypadBase
	{

		private GraphDrawer graphDrawer = new GraphDrawer();

		public SensorReaderKeyAction(ISDConnection connection, InitialPayload payload) : base(connection, payload)
		{

		}

		public override void Dispose()
		{
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
			var value = Aida64Plugin.Instance.GetDataFromCache("TTEMP1");
			if (value == null)
			{
				var defaultImage = graphDrawer.DrawStringFrame(string.Empty,"?");
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
		}

		public override void ReceivedSettings(ReceivedSettingsPayload payload)
		{

		}

	}
}
