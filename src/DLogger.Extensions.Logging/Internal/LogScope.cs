// --------------------------------------------------------------------------------------------------------------------
// <copyright company=".NET Foundation" file="LogScope.cs">
// All rights reserved.
// </copyright>
// <summary>
// This file is a simplified version of Microsoft.Extensions.Logging.Console.ConsoleLogScope.cs
// </summary>
// --------------------------------------------------------------------------------------------------------------------

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
			get => _value.Value;
			set => _value.Value = value;
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
