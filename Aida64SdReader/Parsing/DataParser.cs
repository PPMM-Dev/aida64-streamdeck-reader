using Aida64SDPlugin.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace Aida64SDPlugin.Parsing
{
	public class DataParser
	{
		private XmlReaderSettings _settings;

		public DataParser()
		{
			_settings = new XmlReaderSettings();
			_settings.Async = true;
			_settings.CheckCharacters = false;
			_settings.ConformanceLevel = ConformanceLevel.Document;

		}

		public static Dictionary<string, SensorDataModel> ParseAida64XmlData(string rawXml)
		{
			Dictionary<string, SensorDataModel> result = new Dictionary<string, SensorDataModel>();

			string cleanedXml = CleanRawData(rawXml);
			if (string.IsNullOrEmpty(cleanedXml))
				return result;
			try
			{
				XDocument xDocument = XDocument.Parse(cleanedXml);

				result = xDocument.Root!
						.Elements()
						.Where(item =>
							item.Element("id") != null &&
							item.Element("label") != null &&
							item.Element("value") != null)
						.Select(item => new SensorDataModel
						{
							Category = item.Name.LocalName,
							Id = item.Element("id")!.Value,
							Label = item.Element("label")!.Value,
							Value = item.Element("value")!.Value,
							LastUpdateTime = DateTime.UtcNow
						})
						.ToDictionary(s => s.Id);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				Console.WriteLine(rawXml);
				//Just return an empty list for the moment
				//TODO : Add logs
				return result;
			}

			return result;
		}

		//We need to clean the xml since it's not a clean one and there
		private static string CleanRawData(string rawXml)
		{
			if (String.IsNullOrEmpty(rawXml)) return string.Empty;

			string pattern = "<([a-zA-Z0-9_]+)>\\s*<id>([^<]+)</id>\\s*<label>([^<]+)</label>\\s*<value>([^<]+)</value>\\s*</\\1>";
			string result = string.Empty;
			var matches = Regex.Matches(rawXml, pattern, RegexOptions.Singleline);
			if (matches.Count > 0)
			{
				StringBuilder stringBuilder = new StringBuilder();
				foreach (Match match in matches)
				{
					stringBuilder.Append(match.Value);
				}

				//Adding root node so I can use the xml parser after
				result = "<root>" + stringBuilder.ToString() + "</root>";
			}
			return result;
		}
	}
}
