/**
 *
 * \ingroup LibCs
 *
 * \copyright
 *   Copyright (c) 2008-2018 SpringCard - www.springcard.com
 *   All right reserved
 *
 * \author
 *   Johann.D et al. / SpringCard
 *
 */
/*
 * Read LICENSE.txt for license details and restrictions.
 */
using System;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Win32;
using System.IO;
using System.Collections.ObjectModel;
using System.Net;
using System.Windows.Markup;
using System.Runtime.Serialization.Formatters.Binary;

namespace SpringCard.LibCs
{
	public partial class Logger
	{
		public abstract class Sink
		{
			internal Level level;

			protected Sink(Level level)
			{
				this.level = level;
			}

			public void SetLevel(Level level)
			{
				this.level = level;
			}

			public Level GetLevel()
			{
				return this.level;
			}

			internal abstract void Send(Entry entry);

			public void Send(Level level, string context, string message, params object[] args)
			{
				Entry entry = new Entry(level, context, message, args);
				Send(entry);
			}
		}
	}
}
