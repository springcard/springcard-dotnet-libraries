﻿using System;
using System.IO;
using SpringCard.LibCs.GetText.Plural;

namespace SpringCard.LibCs.GetText.Loaders
{
	/// <summary>
	/// A catalog loader that loads data from files in the GNU/Gettext MO file format and generates
	/// a plural form rule using <see cref="AstPluralRuleGenerator"/>.
	/// </summary>
	public class MoAstPluralLoader : MoLoader
	{
#if !NETSTANDARD1_0
		/// <summary>
		/// Initializes a new instance of the <see cref="MoAstPluralLoader"/> class which will try to load a MO file
		/// that will be located in the localeDir using the domain name and catalog's culture info.
		/// <see cref="AstPluralRuleGenerator"/> will be used to generate a plural form rule.
		/// </summary>
		/// <param name="domain"></param>
		/// <param name="localeDir"></param>
		/// <param name="parser"></param>
		public MoAstPluralLoader(string domain, string localeDir, MoFileParser parser)
			: base(domain, localeDir, new AstPluralRuleGenerator(), parser)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MoLoader"/> class which will try to load a MO file
		/// from the specified path.
		/// <see cref="AstPluralRuleGenerator"/> will be used to generate a plural form rule.
		/// </summary>
		/// <param name="filePath"></param>
		/// <param name="parser"></param>
		public MoAstPluralLoader(string filePath, MoFileParser parser)
			: base(filePath, new AstPluralRuleGenerator(), parser)
		{
		}
#endif

		/// <summary>
		/// Initializes a new instance of the <see cref="MoLoader"/> class which will try to load a MO file
		/// from the specified stream.
		/// <see cref="AstPluralRuleGenerator"/> will be used to generate a plural form rule.
		/// </summary>
		/// <param name="moStream"></param>
		/// <param name="parser"></param>
		public MoAstPluralLoader(Stream moStream, MoFileParser parser)
			: base(moStream, new AstPluralRuleGenerator(), parser)
		{
		}

#if !NETSTANDARD1_0
		/// <summary>
		/// Initializes a new instance of the <see cref="MoLoader"/> class which will try to load a MO file
		/// that will be located in the localeDir using the domain name and catalog's culture info.
		/// <see cref="AstPluralRuleGenerator"/> will be used to generate a plural form rule.
		/// </summary>
		/// <param name="domain"></param>
		/// <param name="localeDir"></param>
		public MoAstPluralLoader(string domain, string localeDir)
			: base(domain, localeDir, new AstPluralRuleGenerator())
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MoLoader"/> class which will try to load a MO file
		/// from the specified path.
		/// <see cref="AstPluralRuleGenerator"/> will be used to generate a plural form rule.
		/// </summary>
		/// <param name="filePath"></param>
		public MoAstPluralLoader(string filePath)
			: base(filePath, new AstPluralRuleGenerator())
		{
		}
#endif

		/// <summary>
		/// Initializes a new instance of the <see cref="MoLoader"/> class which will try to load a MO file
		/// from the specified stream.
		/// <see cref="AstPluralRuleGenerator"/> will be used to generate a plural form rule.
		/// </summary>
		/// <param name="moStream"></param>
		public MoAstPluralLoader(Stream moStream)
			: base(moStream, new AstPluralRuleGenerator())
		{
		}
	}
}