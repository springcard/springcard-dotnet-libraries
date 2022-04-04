using System;
using System.IO;
using System.Reflection;
using System.Globalization;
using System.Configuration;
using System.Collections.Generic;
using System.Resources;
using System.Threading;
using SpringCard.LibCs.GetText;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SpringCard.LibCs
{
    public static class T
    {
		public static string _(string text)
		{
			return Translation._(text);
		}

		public static string _(string text, params object[] args)
		{
			return Translation._(text, args);
		}

		public static string _n(string text, string pluralText, long n)
		{
			return Translation._n(text, pluralText, n);
		}

		public static string _n(string text, string pluralText, long n, params object[] args)
		{
			return Translation._n(text, pluralText, n, args);
		}

		public static string _p(string context, string text)
		{
			return Translation._p(context, text);
		}

		public static string _p(string context, string text, params object[] args)
		{
			return Translation._p(context, text, args);
		}

		public static string _pn(string context, string text, string pluralText, long n)
		{
			return Translation._pn(context, text, pluralText, n);
		}

		public static string _pn(string context, string text, string pluralText, long n, params object[] args)
		{
			return Translation._pn(context, text, pluralText, n, args);
		}
    }

	public static class Translation
	{
		public static bool Debug = false;
		private static Logger logger = new Logger(typeof(Translation).FullName);


		private static Dictionary<string, Catalog> catalogs = new Dictionary<string, Catalog>();
		private static CultureInfo defaultCulture = CultureInfo.InvariantCulture;

		public static bool DebugMissingTranslations;
		public static List<string> MissingTranslations = new List<string>();

		private static void MissingTranslation(string text)
		{
			if (!MissingTranslations.Contains(text))
			{
				logger.log(Debug ? Logger.Level.Error : Logger.Level.Debug, "Missing translation [{0}] {1}", culture.ToString(), text);
				MissingTranslations.Add(text);
			}
		}

		private static void MissingTranslation(string context, string text)
		{
			MissingTranslation(string.Format("{0}>>>{1}", context, text));
		}

		public static CultureInfo DefaultCulture
		{
			get
			{
				return defaultCulture;
			}
			set
			{
				logger.log(Debug ? Logger.Level.Info : Logger.Level.Debug, "Application default culture set to {0} ({1} aka {2})", value.ToString(), value.Name, value.NativeName);
				defaultCulture = value;
				DebugMissingTranslations = (defaultCulture.ToString() != culture.ToString());
			}
		}
		private static CultureInfo culture = CultureInfo.InvariantCulture;
		public static CultureInfo Culture
		{
			get
			{
				return culture;
			}
			set
			{
				logger.log(Debug ? Logger.Level.Info : Logger.Level.Debug, "Runtime culture set to {0} ({1} aka {2})", value.ToString(), value.Name, value.NativeName);
				culture = value;
				DebugMissingTranslations = (defaultCulture.ToString() != culture.ToString());
			}
		}


		public static void AddCatalogFromDirectory(string domain, string directory)
		{
			logger.log(Debug ? Logger.Level.Info : Logger.Level.Debug, "Loading translation catalog {0}/{1}/{2}.mo", directory, culture.ToString(), domain);
			Catalog catalog = new Catalog(domain, directory, culture);
			catalogs[domain] = catalog;
		}

		public static void AddCatalogFile(string domain, string filename)
		{
			try
			{
				logger.log(Debug ? Logger.Level.Info : Logger.Level.Debug, "Loading translations for {0} from {1}", domain, filename);
				Stream stream = File.OpenRead(filename);
				if (stream.Length > 0)
				{
					Catalog catalog = new Catalog(stream, culture);
					catalogs[domain] = catalog;
				}
			}
			catch (Exception e)
			{
				logger.log(Debug ? Logger.Level.Error : Logger.Level.Debug, "Failed to load translations from {0}", filename);
				logger.log(Debug ? Logger.Level.Error : Logger.Level.Debug, "Exception: {0}", e.Message);
			}			
		}

		public static bool TryAddCatalogFile(string domain, string filename)
		{
			if (Debug)
				logger.debug("Looking for {0} ...", filename);
			if (File.Exists(filename))
			{
				AddCatalogFile(domain, filename);
				return true;
			}
			return false;
		}

		public static bool AddCatalog(string domain)
		{
			string lang = culture.ToString();
			string filename;

			filename = Path.Combine(lang, domain + ".mo");
			if (TryAddCatalogFile(domain, filename)) return true;

			filename = Path.Combine("lang", domain + "." + lang + ".mo");
			if (TryAddCatalogFile(domain, filename)) return true;

			filename = Path.Combine("lang", lang, domain + ".mo");
			if (TryAddCatalogFile(domain, filename)) return true;

			filename = Path.Combine("po", lang, domain + ".mo");
			if (TryAddCatalogFile(domain, filename)) return true;

			if (lang == "fr")
			{
				string slang = "fr-FR";

				filename = Path.Combine(slang, domain + ".mo");
				if (TryAddCatalogFile(domain, filename)) return true;

				filename = Path.Combine("lang", domain + "." + slang + ".mo");
				if (TryAddCatalogFile(domain, filename)) return true;

				filename = Path.Combine("lang", slang, domain + ".mo");
				if (TryAddCatalogFile(domain, filename)) return true;

				filename = Path.Combine("po", slang, domain + ".mo");
				if (TryAddCatalogFile(domain, filename)) return true;
			}
			else if (lang == "en")
			{
				string slang = "en-US";

				filename = Path.Combine(slang, domain + ".mo");
				if (TryAddCatalogFile(domain, filename)) return true;

				filename = Path.Combine("lang", domain + "." + slang + ".mo");
				if (TryAddCatalogFile(domain, filename)) return true;

				filename = Path.Combine("lang", slang, domain + ".mo");
				if (TryAddCatalogFile(domain, filename)) return true;

				filename = Path.Combine("po", slang, domain + ".mo");
				if (TryAddCatalogFile(domain, filename)) return true;

				slang = "en-UK";

				filename = Path.Combine(slang, domain + ".mo");
				if (TryAddCatalogFile(domain, filename)) return true;

				filename = Path.Combine("lang", domain + "." + slang + ".mo");
				if (TryAddCatalogFile(domain, filename)) return true;

				filename = Path.Combine("lang", slang, domain + ".mo");
				if (TryAddCatalogFile(domain, filename)) return true;

				filename = Path.Combine("po", slang, domain + ".mo");
				if (TryAddCatalogFile(domain, filename)) return true;

				slang = "en-GB";

				filename = Path.Combine(slang, domain + ".mo");
				if (TryAddCatalogFile(domain, filename)) return true;

				filename = Path.Combine("lang", domain + "." + slang + ".mo");
				if (TryAddCatalogFile(domain, filename)) return true;

				filename = Path.Combine("lang", slang, domain + ".mo");
				if (TryAddCatalogFile(domain, filename)) return true;

				filename = Path.Combine("po", slang, domain + ".mo");
				if (TryAddCatalogFile(domain, filename)) return true;
			}


			logger.log((Debug || DebugMissingTranslations) ? Logger.Level.Error : Logger.Level.Debug, "No translations catalog [{0}] for {1}", lang, domain);
			return false;
		}

		public static string T(string text)
		{
			return _(text);
		}

		public static string T(string context, string text)
		{
			return _p(context, text);
		}

		public static string _(string text)
		{
			foreach (KeyValuePair<string, Catalog> entry in catalogs)
			{
				if (entry.Value.IsTranslationExist(text))
					return entry.Value.GetString(text);
			}

			MissingTranslation(text);
			return text;
		}

		public static string _(string text, params object[] args)
		{
			foreach (KeyValuePair<string, Catalog> entry in catalogs)
			{
				if (entry.Value.IsTranslationExist(text))
					return entry.Value.GetString(text, args);
			}

			MissingTranslation(text);
			return string.Format(text, args);
		}

		public static string _n(string text, string pluralText, long n)
		{
			foreach (KeyValuePair<string, Catalog> entry in catalogs)
			{
				if (n > 1)
				{
					if (entry.Value.IsTranslationExist(pluralText))
						return entry.Value.GetString(text, pluralText, n);
				}
				else
				{
					if (entry.Value.IsTranslationExist(text))
						return entry.Value.GetString(text, pluralText, n);
				}
			}

			if (n > 1)
			{
				MissingTranslation(pluralText);
				return string.Format(pluralText, n);
			}
			else
			{
				MissingTranslation(text);
				return string.Format(text, n);
			}
		}

		public static string _n(string text, string pluralText, long n, params object[] args)
		{
			foreach (KeyValuePair<string, Catalog> entry in catalogs)
			{
				if (n > 1)
				{
					if (entry.Value.IsTranslationExist(pluralText))
						return entry.Value.GetString(text, pluralText, n, args);
				}
				else
				{
					if (entry.Value.IsTranslationExist(text))
						return entry.Value.GetString(text, pluralText, n, args);
				}
			}

			if (n > 1)
			{
				MissingTranslation(pluralText);
				return string.Format(pluralText, args);
			}
			else
			{
				MissingTranslation(text);
				return string.Format(text, args);
			}
		}

		public static string _p(string context, string text)
		{
			if (catalogs.ContainsKey(context))
			{
				if (catalogs[context].IsTranslationExist(text))
					return catalogs[context].GetString(text);
			}

			foreach (KeyValuePair<string, Catalog> entry in catalogs)
			{
				string result = entry.Value.GetParticularString(context, text);
				if (result != text)
					return result;
			}

			MissingTranslation(context, text);
			return text;
		}

		public static string _p(string context, string text, params object[] args)
		{
			if (catalogs.ContainsKey(context))
			{
				if (catalogs[context].IsTranslationExist(text))
					return catalogs[context].GetString(text, args);
			}

			string defaultResult = string.Format(text, args);
			foreach (KeyValuePair<string, Catalog> entry in catalogs)
			{
				string result = entry.Value.GetParticularString(context, text, args);
				if (result != defaultResult)
					return result;
			}

			MissingTranslation(context, text);
			return defaultResult;
		}

		public static string _pn(string context, string text, string pluralText, long n)
		{
			if (catalogs.ContainsKey(context))
			{
				if (catalogs[context].IsTranslationExist(text))
					return catalogs[context].GetPluralString(text, pluralText, n);
			}

			string defaultResult;
			if (n > 1)
				defaultResult= string.Format(pluralText, n);
			else
				defaultResult = string.Format(text, n);
			foreach (KeyValuePair<string, Catalog> entry in catalogs)
			{
				string result = entry.Value.GetParticularPluralString(context, text, pluralText, n);
				if (result != defaultResult)
					return result;
			}

			if (n > 1)
			{
				MissingTranslation(context, pluralText);
			}
			else
			{
				MissingTranslation(context, text);
			}
			return defaultResult;
		}

		public static string _pn(string context, string text, string pluralText, long n, params object[] args)
		{
			if (catalogs.ContainsKey(context))
			{
				if (catalogs[context].IsTranslationExist(text))
					return catalogs[context].GetPluralString(text, pluralText, n, args);
			}

			string defaultResult;
			if (n > 1)
				defaultResult = string.Format(pluralText, args);
			else
				defaultResult = string.Format(text, args);
			foreach (KeyValuePair<string, Catalog> entry in catalogs)
			{
				string result = entry.Value.GetParticularPluralString(context, text, pluralText, n, args);
				if (result != defaultResult)
					return result;
			}

			if (n > 1)
			{
				MissingTranslation(context, pluralText);
			}
			else
			{
				MissingTranslation(context, text);
			}
			return defaultResult;
		}
	}
}
