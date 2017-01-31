// Copyright (c) .NET Foundation. All rights reserved.
// This file is a simplified version of Microsoft.Extensions.Logging.Console.ConsoleLogScope.cs

using System;
using System.Threading;

namespace DLogger.Extensions.Logging.Internal
{
	internal class LogScope
	{
		private static AsyncLocal<LogScope> _value = new AsyncLocal<LogScope>();
		private string _scope;

		public static LogScope Current
		{
			set
			{
				_value.Value = value;
			}
			get
			{
				return _value.Value;
			}
		}

		public LogScope Parent { get; private set; }

		public LogScope(string scope)
		{
			_scope = scope;
		}

		public static IDisposable Push(string scope)
		{
			var temp = Current;
			Current = new LogScope(scope);
			Current.Parent = temp;

			return new DisposableScope();
		}

		public override string ToString()
		{
			return _scope;
		}

		private class DisposableScope : IDisposable
		{
			public void Dispose()
			{
				Current = Current.Parent;
			}
		}
	}
}
