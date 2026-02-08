using Aida64SDPlugin.Models;
using BarRaider.SdTools;
using BarRaider.SdTools.Wrappers;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;

namespace Aida64SdReader.Core
{
	public class GraphDrawer
	{
		private const int WIDTH = 144;
		private const int HEIGHT = 144;
		private const string FONT = "Calibri";
		private Queue<float> floats;

		[SupportedOSPlatform("windows")]
		private TitleParameters tpTitle = new TitleParameters(new FontFamily(FONT), FontStyle.Bold, 10, Color.White, true, TitleVerticalAlignment.Top);
		[SupportedOSPlatform("windows")]
		private TitleParameters tpValue = new TitleParameters(new FontFamily(FONT), FontStyle.Bold, 20, Color.White, true, TitleVerticalAlignment.Middle);


		public GraphDrawer()
		{
			Random rand = new Random();
			floats = new Queue<float>();
			for (int i = 0; i < 20; i++)
			{
				floats.Enqueue(0);
			}
		}

		[SupportedOSPlatform("windows")]
		public Bitmap DrawStringFrame(string title, string value)
		{
			Bitmap image = Tools.GenerateGenericKeyImage(out Graphics graphics);
			graphics.FillRectangle(Brushes.Black, 0, 0, image.Width, image.Height);

			if (!string.IsNullOrEmpty(value))
				graphics.AddTextPath(tpValue, image.Height, image.Width, value);

			if (!string.IsNullOrEmpty(title))
				graphics.AddTextPath(tpTitle, image.Height, image.Width, title);

			graphics.Dispose();
			return image;
		}

		[SupportedOSPlatform("windows")]
		public Bitmap DrawGraphFrame(SensorDataModel sensorData)
		{
			Bitmap image = Tools.GenerateGenericKeyImage(out Graphics graphics);
			graphics.FillRectangle(Brushes.Black, 0, 0, image.Width, image.Height);
			graphics.AddTextPath(tpValue, image.Height, image.Width, $"{sensorData?.Value}°" ?? "?");
			graphics.AddTextPath(tpTitle, image.Height, image.Width, sensorData?.Id ?? string.Empty);
			graphics.Dispose();
			return image;
		}
	}
}
