﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using SpringCard.LibCs;
using SpringCard.LibCs.GetText.Loaders;
using SpringCard.LibCs.GetText.Plural;

namespace SpringCard.LibCs.GetText
{
	/// <summary>
	/// Represents a Gettext catalog instance.
	/// </summary>
	public class Catalog : ICatalog
	{
		private IPluralRule _PluralRule;

		/// <summary>
		/// Context glue (&lt;EOT&gt; symbol)
		/// </summary>
		public const char CONTEXT_GLUE = '\u0004';

		/// <summary>
		/// Current catalog locale.
		/// </summary>
		public CultureInfo CultureInfo { get; protected set; }

		/// <summary>
		/// Loaded raw translation strings.
		/// (msgctxt&lt;EOT&gt;)msgid => msgstr[]
		/// </summary>
		public Dictionary<string, string[]> Translations { get; protected set; }

		/// <summary>
		/// Gets or sets current plural form rule.
		/// </summary>
		public IPluralRule PluralRule
		{
			get { return this._PluralRule; }
			set
			{
				if (value == null)
					throw new ArgumentNullException("value");
				this._PluralRule = value;
			}
		}

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="Catalog"/> class that has no translations
		/// using the current UI culture info and plural rule generated by DefaultPluralRuleGenerator for the current UI culture.
		/// </summary>
		public Catalog()
			: this(CultureInfo.CurrentUICulture)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Catalog"/> class that has no translations
		/// using given culture info and plural rule generated by DefaultPluralRuleGenerator for given culture.
		/// </summary>
		/// <param name="cultureInfo">Culture info.</param>
		public Catalog(CultureInfo cultureInfo)
		{
			this.CultureInfo = cultureInfo;
			this.Translations = new Dictionary<string, string[]>();
			this.PluralRule = (new DefaultPluralRuleGenerator()).CreateRule(cultureInfo);
		}
			
		/// <summary>
		/// Initializes a new instance of the <see cref="Catalog"/> class using the current UI culture info
		/// and loads data using given loader.
		/// </summary>
		/// <param name="loader">Loader instance.</param>
		public Catalog(ILoader loader)
			: this(loader, CultureInfo.CurrentUICulture)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Catalog"/> class using given culture info
		/// and loads data using given loader.
		/// </summary>
		/// <param name="loader">Loader instance.</param>
		/// <param name="cultureInfo">Culture info.</param>
		public Catalog(ILoader loader, CultureInfo cultureInfo)
			: this(cultureInfo)
		{
			try
			{
				this.Load(loader);
			}
#if NETSTANDARD1_0
			catch (FileNotFoundException) { }
#else
#if DEBUG
			catch (FileNotFoundException exception)
			{
				// Suppress FileNotFound exceptions
				Logger.Debug("Translation file loading fail: {0}", exception.Message);
			}
#else
			catch (FileNotFoundException) {}
#endif
#endif
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Catalog"/> class using the current UI culture info
		/// and loads data from specified stream using a new <see cref="MoLoader"/> instance.
		/// </summary>
		/// <param name="moStream">Stream that contain binary data in the MO file format.</param>
		public Catalog(Stream moStream)
			: this(new MoLoader(moStream))
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Catalog"/> class using given culture info
		/// and loads data from specified stream using a new <see cref="MoLoader"/> instance.
		/// </summary>
		/// <param name="moStream">Stream that contain binary data in the MO file format.</param>
		/// <param name="cultureInfo">Culture info.</param>
		public Catalog(Stream moStream, CultureInfo cultureInfo)
			: this(new MoLoader(moStream), cultureInfo)
		{
		}

#if !NETSTANDARD1_0
		/// <summary>
		/// Initializes a new instance of the <see cref="Catalog"/> class using the current UI culture info
		/// and loads data for specified domain and locale directory using a new <see cref="MoLoader"/> instance.
		/// </summary>
		/// <param name="domain">Catalog domain name.</param>
		/// <param name="localeDir">Directory that contains gettext localization files.</param>
		public Catalog(string domain, string localeDir)
			: this(new MoLoader(domain, localeDir))
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Catalog"/> class using given culture info
		/// and loads data for specified domain and locale directory using a new <see cref="MoLoader"/> instance.
		/// </summary>
		/// <param name="domain">Catalog domain name.</param>
		/// <param name="localeDir">Directory that contains gettext localization files.</param>
		/// <param name="cultureInfo">Culture info.</param>
		public Catalog(string domain, string localeDir, CultureInfo cultureInfo)
			: this(new MoLoader(domain, localeDir), cultureInfo)
		{
		}
#endif

		#endregion

		/// <summary>
		/// Loads data to the current catalog using specified loader instance.
		/// </summary>
		/// <param name="loader">Loader instance.</param>
		public void Load(ILoader loader)
		{
			if (loader == null)
				throw new ArgumentNullException("loader");

			loader.Load(this);
		}

#region ICatalog implementation

		/// <summary>
		/// Returns <paramref name="text"/> translated into the selected language.
		/// Similar to <c>gettext</c> function.
		/// </summary>
		/// <param name="text">Text to translate.</param>
		/// <returns>Translated text.</returns>
		public virtual string GetString(string text)
		{
			return this.GetStringDefault(text, text);
		}

		/// <summary>
		/// Returns <paramref name="text"/> translated into the selected language.
		/// Similar to <c>gettext</c> function.
		/// </summary>
		/// <param name="text">Text to translate.</param>
		/// <param name="args">Optional arguments for <see cref="System.String.Format(string, object[])"/> method.</param>
		/// <returns>Translated text.</returns>
		public virtual string GetString(string text, params object[] args)
		{
			return String.Format(this.CultureInfo, this.GetStringDefault(text, text), args);
		}

		/// <summary>
		/// Returns the plural form for <paramref name="n"/> of the translation of <paramref name="text"/>.
		/// Similar to <c>ngettext</c> function.
		/// </summary>
		/// <param name="text">Singular form of message to translate.</param>
		/// <param name="pluralText">Plural form of message to translate.</param>
		/// <param name="n">Value that determines the plural form.</param>
		/// <returns>Translated text.</returns>
		public virtual string GetPluralString(string text, string pluralText, long n)
		{
			return this.GetPluralStringDefault(text, text, pluralText, n);
		}

		/// <summary>
		/// Returns the plural form for <paramref name="n"/> of the translation of <paramref name="text"/>.
		/// Similar to <c>ngettext</c> function.
		/// </summary>
		/// <param name="text">Singular form of message to translate.</param>
		/// <param name="pluralText">Plural form of message to translate.</param>
		/// <param name="n">Value that determines the plural form.</param>
		/// <param name="args">Optional arguments for <see cref="System.String.Format(string, object[])"/> method.</param>
		/// <returns>Translated text.</returns>
		public virtual string GetPluralString(string text, string pluralText, long n, params object[] args)
		{
			return String.Format(this.CultureInfo, this.GetPluralStringDefault(text, text, pluralText, n), args);
		}

		/// <summary>
		/// Returns <paramref name="text"/> translated into the selected language using given <paramref name="context"/>.
		/// Similar to <c>pgettext</c> function.
		/// </summary>
		/// <param name="context">Context.</param>
		/// <param name="text">Text to translate.</param>
		/// <returns>Translated text.</returns>
		public virtual string GetParticularString(string context, string text)
		{
			return this.GetStringDefault(context + CONTEXT_GLUE + text, text);
		}

		/// <summary>
		/// Returns <paramref name="text"/> translated into the selected language using given <paramref name="context"/>.
		/// Similar to <c>pgettext</c> function.
		/// </summary>
		/// <param name="context">Context.</param>
		/// <param name="text">Text to translate.</param>
		/// <param name="args">Optional arguments for <see cref="System.String.Format(string, object[])"/> method.</param>
		/// <returns>Translated text.</returns>
		public virtual string GetParticularString(string context, string text, params object[] args)
		{
			return String.Format(this.CultureInfo, this.GetStringDefault(context + CONTEXT_GLUE + text, text), args);
		}

		/// <summary>
		/// Returns the plural form for <paramref name="n"/> of the translation of <paramref name="text"/> using given <paramref name="context"/>.
		/// Similar to <c>npgettext</c> function.
		/// </summary>
		/// <param name="context">Context.</param>
		/// <param name="text">Singular form of message to translate.</param>
		/// <param name="pluralText">Plural form of message to translate.</param>
		/// <param name="n">Value that determines the plural form.</param>
		/// <returns>Translated text.</returns>
		public virtual string GetParticularPluralString(string context, string text, string pluralText, long n)
		{
			return this.GetPluralStringDefault(context + CONTEXT_GLUE + text, text, pluralText, n);
		}

		/// <summary>
		/// Returns the plural form for <paramref name="n"/> of the translation of <paramref name="text"/> using given <paramref name="context"/>.
		/// Similar to <c>npgettext</c> function.
		/// </summary>
		/// <param name="context">Context.</param>
		/// <param name="text">Singular form of message to translate.</param>
		/// <param name="pluralText">Plural form of message to translate.</param>
		/// <param name="n">Value that determines the plural form.</param>
		/// <param name="args">Optional arguments for <see cref="System.String.Format(string, object[])"/> method.</param>
		/// <returns>Translated text.</returns>
		public virtual string GetParticularPluralString(string context, string text, string pluralText, long n, params object[] args)
		{
			return String.Format(this.CultureInfo, this.GetPluralStringDefault(context + CONTEXT_GLUE + text, text, pluralText, n), args);
		}

#endregion

		/// <summary>
		/// Returns translated string for given <paramref name="messageId"/> or <paramref name="defaultMessage"/> on fail.
		/// </summary>
		/// <param name="messageId">Message ID</param>
		/// <param name="defaultMessage">Default message</param>
		/// <returns>Translated string</returns>
		public virtual string GetStringDefault(string messageId, string defaultMessage)
		{
			var translations = this.GetTranslations(messageId);

			if (translations == null || translations.Length == 0)
			{
#if DEBUG && !NETSTANDARD1_0
				Logger.Debug("Translation not found for message id \"{0}\".", messageId);
#endif
				return defaultMessage;
			}

			return translations[0];
		}

		/// <summary>
		/// Returns translated plural string for given <paramref name="messageId"/> or 
		/// <paramref name="defaultMessage"/> or <paramref name="defaultPluralMessage"/> on fail.
		/// </summary>
		/// <param name="messageId">Message ID</param>
		/// <param name="defaultMessage">Default message singular form</param>
		/// <param name="defaultPluralMessage">Default message plural form</param>
		/// <param name="n">Value that determines the plural form</param>
		/// <returns>Translated string</returns>
		public virtual string GetPluralStringDefault(string messageId, string defaultMessage, string defaultPluralMessage, long n)
		{
			var translations = this.GetTranslations(messageId);
			var pluralIndex = this.PluralRule.Evaluate(n);
			if (pluralIndex < 0 || pluralIndex >= this.PluralRule.NumPlurals)
			{
				throw new IndexOutOfRangeException(String.Format(
					"Calculated plural form index ({0}) is out of allowed range (0~{1}).",
					pluralIndex,
					this.PluralRule.NumPlurals - 1
				));
			}

			if (translations == null || translations.Length <= pluralIndex)
			{
#if DEBUG && !NETSTANDARD1_0
				Logger.Debug("Translation not found (plural) for message id \"{0}\".", messageId);
#endif
				return (n == 1) ? defaultMessage : defaultPluralMessage;
			}

			return translations[pluralIndex];
		}

		/// <summary>
		/// Returns all translations for given <paramref name="messageId"/>.
		/// </summary>
		/// <param name="messageId"></param>
		/// <returns>Returns all translations for given <paramref name="messageId"/> or null if not found.</returns>
		public virtual string[] GetTranslations(string messageId)
		{
			if (!this.IsTranslationExist(messageId)) return null;

			return this.Translations[messageId];
		}

		/// <summary>
		/// Checks whether given message ID is present in the loaded translation list.
		/// </summary>
		/// <param name="messageId"></param>
		/// <returns></returns>
		public virtual bool IsTranslationExist(string messageId)
		{
			if (String.IsNullOrEmpty(messageId)) return false;
			
			return this.Translations.ContainsKey(messageId);
		}
	}
}