﻿using CarinaStudio.Threading;
using System;
using System.ComponentModel;
using System.Net;
using System.Text;

namespace CarinaStudio.ULogViewer.Logs.DataSources
{
	/// <summary>
	/// Provider of <see cref="ILogDataSource"/>.
	/// </summary>
	interface ILogDataSourceProvider : IApplicationObject, INotifyPropertyChanged, IThreadDependent
	{
		/// <summary>
		/// Get number of active <see cref="ILogDataSource"/> instances created by this provider.
		/// </summary>
		int ActiveSourceCount { get; }


		/// <summary>
		/// Check whether multiple active <see cref="ILogDataSource"/> instances created by this provider is allowed or not.
		/// </summary>
		bool AllowMultipleSources { get; }


		/// <summary>
		/// Create <see cref="ILogDataSource"/> instance.
		/// </summary>
		/// <param name="options">Options.</param>
		/// <returns><see cref="ILogDataSource"/> instance.</returns>
		ILogDataSource CreateSource(LogDataSourceOptions options);


		/// <summary>
		/// Get name for displaying purpose.
		/// </summary>
		string DisplayName { get; }


		/// <summary>
		/// Get unique name to identify this provider.
		/// </summary>
		string Name { get; }


		/// <summary>
		/// Get underlying source of log.
		/// </summary>
		UnderlyingLogDataSource UnderlyingSource { get; }
	}


	/// <summary>
	/// Options to create <see cref="ILogDataSource"/>.
	/// </summary>
	struct LogDataSourceOptions
	{
		/// <summary>
		/// Get or set command to start process.
		/// </summary>
		/// <remarks>Available for <see cref="UnderlyingLogDataSource.StandardOutput"/>.</remarks>
		public string? Command { get; set; }


		/// <summary>
		/// Create <see cref="LogDataSourceOptions"/> for <see cref="UnderlyingLogDataSource.File"/> case.
		/// </summary>
		/// <param name="fileName">File name.</param>
		/// <param name="encoding">Text encoding.</param>
		public static LogDataSourceOptions CreateForFile(string fileName, Encoding? encoding = null) => new LogDataSourceOptions()
		{
			Command = null,
			Encoding = encoding,
			FileName = fileName,
			Uri = null,
			WebRequestCredentials = null,
			WorkingDirectory = null,
		};


		/// <summary>
		/// Create <see cref="LogDataSourceOptions"/> for <see cref="UnderlyingLogDataSource.StandardOutput"/> case.
		/// </summary>
		/// <param name="command">Command.</param>
		/// <param name="workingDirectory">Working directory.</param>
		public static LogDataSourceOptions CreateForStandardOutput(string command, string? workingDirectory = null) => new LogDataSourceOptions()
		{
			Command = command,
			Encoding = null,
			FileName = null,
			Uri = null,
			WebRequestCredentials = null,
			WorkingDirectory = workingDirectory,
		};


		/// <summary>
		/// Create <see cref="LogDataSourceOptions"/> for <see cref="UnderlyingLogDataSource.WebRequest"/> case.
		/// </summary>
		/// <param name="uri">Uri of request.</param>
		/// <param name="credentials">Credentials.</param>
		public static LogDataSourceOptions CreateForWebRequest(Uri uri, ICredentials? credentials = null) => new LogDataSourceOptions()
		{
			Command = null,
			Encoding = null,
			FileName = null,
			Uri = uri,
			WebRequestCredentials = credentials,
			WorkingDirectory = null,
		};


		/// <summary>
		/// Get or set encoding of text.
		/// </summary>
		/// <remarks>Available for <see cref="UnderlyingLogDataSource.File"/>.</remarks>
		public Encoding? Encoding { get; set; }


		// Check equality.
		public override bool Equals(object? obj)
		{
			if (obj is LogDataSourceOptions options)
			{
				return this.Command == options.Command
					&& this.Encoding == options.Encoding
					&& this.FileName == options.FileName
					&& this.Uri == options.Uri
					&& (this.WebRequestCredentials?.Equals(options.WebRequestCredentials) ?? options.WebRequestCredentials == null)
					&& this.WorkingDirectory == options.WorkingDirectory;
			}
			return false;
		}


		/// <summary>
		/// Get or set name of file to open.
		/// </summary>
		/// <remarks>Available for <see cref="UnderlyingLogDataSource.File"/>.</remarks>
		public string? FileName { get; set; }


		// Get hash code.
		public override int GetHashCode()
		{
			if (this.Command != null)
				return this.Command.GetHashCode();
			if (this.FileName != null)
				return this.FileName.GetHashCode();
			if (this.Uri != null)
				return this.Uri.GetHashCode();
			return 0;
		}


		/// <summary>
		/// Get or set URI to connect.
		/// </summary>
		/// <remarks>Available for <see cref="UnderlyingLogDataSource.WebRequest"/>.</remarks>
		public Uri? Uri { get; set; }


		/// <summary>
		/// Get or set credentials.
		/// </summary>
		/// <remarks>Available for <see cref="UnderlyingLogDataSource.WebRequest"/>.</remarks>
		public ICredentials? WebRequestCredentials { get; set; }


		/// <summary>
		/// Path of working directory.
		/// </summary>
		/// <remarks>Available for <see cref="UnderlyingLogDataSource.StandardOutput"/>.</remarks>
		public string? WorkingDirectory { get; set; }
	}
}