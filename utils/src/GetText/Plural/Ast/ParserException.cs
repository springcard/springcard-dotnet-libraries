﻿using System;

namespace SpringCard.LibCs.GetText.Plural.Ast
{
    public class ParserException : Exception
    {
		public ParserException() : base() { }
		public ParserException(string message) : base(message) { }
		public ParserException(string message, Exception innerException) : base(message, innerException) { }
	}
}
