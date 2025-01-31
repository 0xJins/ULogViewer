﻿using CarinaStudio.AppSuite.Data;
using CarinaStudio.Collections;
using CarinaStudio.Configuration;
using CarinaStudio.Threading;
using CarinaStudio.ULogViewer.Cryptography;
using CarinaStudio.ULogViewer.Logs.DataSources;
using CarinaStudio.ULogViewer.ViewModels.Categorizing;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CarinaStudio.ULogViewer.Logs.Profiles
{
	/// <summary>
	/// Log profile.
	/// </summary>
	class LogProfile : BaseProfile<IULogViewerApplication>
	{
		// Constants.
		const string EmptyId = "Empty";


		// Static fields.
		static readonly CultureInfo defaultTimestampCultureInfoForReading = CultureInfo.GetCultureInfo("en-US");


		// Fields.
		bool allowMultipleFiles = true;
		string builtInName = "";
		LogColorIndicator colorIndicator = LogColorIndicator.None;
		LogDataSourceOptions dataSourceOptions;
		ILogDataSourceProvider dataSourceProvider = LogDataSourceProviders.Empty;
		string? description;
		bool hasDescription;
		LogProfileIcon icon = LogProfileIcon.File;
		bool isAdministratorNeeded;
		bool isContinuousReading;
		bool isPinned;
		SettingKey<bool>? isPinnedSettingKey;
		bool isWorkingDirectoryNeeded;
		Dictionary<string, LogLevel> logLevelMapForReading = new Dictionary<string, LogLevel>();
		Dictionary<LogLevel, string> logLevelMapForWriting = new Dictionary<LogLevel, string>();
		IList<LogPattern> logPatterns = new LogPattern[0];
		LogStringEncoding logStringEncodingForReading = LogStringEncoding.Plane;
		LogStringEncoding logStringEncodingForWriting = LogStringEncoding.Plane;
		string? logWritingFormat;
		IDictionary<string, LogLevel> readOnlyLogLevelMapForReading;
		IDictionary<LogLevel, string> readOnlyLogLevelMapForWriting;
		long restartReadingDelay;
		SortDirection sortDirection = SortDirection.Ascending;
		LogSortKey sortKey = LogSortKey.Timestamp;
		CultureInfo timeSpanCultureInfoForReading = defaultTimestampCultureInfoForReading;
		CultureInfo timeSpanCultureInfoForWriting = defaultTimestampCultureInfoForReading;
		LogTimeSpanEncoding timeSpanEncodingForReading = LogTimeSpanEncoding.Custom;
		IList<string> timeSpanFormatsForReading = new string[0];
		string? timeSpanFormatForDisplaying;
		string? timeSpanFormatForWriting;
		TimestampDisplayableLogCategoryGranularity timestampCategoryGranularity = TimestampDisplayableLogCategoryGranularity.Hour;
		CultureInfo timestampCultureInfoForReading = defaultTimestampCultureInfoForReading;
		CultureInfo timestampCultureInfoForWriting = defaultTimestampCultureInfoForReading;
		LogTimestampEncoding timestampEncodingForReading = LogTimestampEncoding.Custom;
		string? timestampFormatForDisplaying;
		string? timestampFormatForWriting;
		IList<string> timestampFormatsForReading = new string[0];
		IList<LogProperty> visibleLogProperties = new LogProperty[0];


		// Constructor.
		LogProfile(IULogViewerApplication app, string id, bool isBuiltIn) : base(app, id, isBuiltIn)
		{
			this.isPinnedSettingKey = isBuiltIn ? new($"BuiltInProfile.{id}.IsPinned") : null;
			this.readOnlyLogLevelMapForReading = new ReadOnlyDictionary<string, LogLevel>(this.logLevelMapForReading);
			this.readOnlyLogLevelMapForWriting = new ReadOnlyDictionary<LogLevel, string>(this.logLevelMapForWriting);
			if (isBuiltIn)
				this.UpdateBuiltInNameAndDescription();
		}


		/// <summary>
		/// Initialize new <see cref="LogProfile"/> instance.
		/// </summary>
		/// <param name="app">Application.</param>
		public LogProfile(IULogViewerApplication app) : this(app, LogProfileManager.Default.GenerateProfileId(), false)
		{ }


		/// <summary>
		/// Initialize new <see cref="LogProfile"/> instance.
		/// </summary>
		/// <param name="template">Template profile.</param>
		public LogProfile(LogProfile template) : this(template.Application, LogProfileManager.Default.GenerateProfileId(), false)
		{
			this.allowMultipleFiles = template.allowMultipleFiles;
			this.colorIndicator = template.colorIndicator;
			this.dataSourceOptions = template.dataSourceOptions;
			this.dataSourceProvider = template.dataSourceProvider;
			this.description = template.description;
			this.hasDescription = template.hasDescription;
			this.icon = template.icon;
			this.isAdministratorNeeded = template.isAdministratorNeeded;
			this.isContinuousReading = template.isContinuousReading;
			this.isPinned = template.isPinned;
			this.isWorkingDirectoryNeeded = template.isWorkingDirectoryNeeded;
			this.logLevelMapForReading.AddAll(template.logLevelMapForReading);
			this.logLevelMapForWriting.AddAll(template.logLevelMapForWriting);
			this.logPatterns = template.logPatterns;
			this.logStringEncodingForReading = template.logStringEncodingForReading;
			this.logStringEncodingForWriting = template.logStringEncodingForWriting;
			this.logWritingFormat = template.logWritingFormat;
			this.Name = template.Name;
			this.sortDirection = template.sortDirection;
			this.sortKey = template.sortKey;
			this.timeSpanCultureInfoForReading = template.timeSpanCultureInfoForReading;
			this.timeSpanEncodingForReading = template.timeSpanEncodingForReading;
			this.timeSpanFormatForDisplaying = template.timeSpanFormatForDisplaying;
			this.timeSpanFormatForWriting = template.timeSpanFormatForWriting;
			this.timeSpanFormatsForReading = template.timeSpanFormatsForReading;
			this.timestampCategoryGranularity = template.timestampCategoryGranularity;
			this.timestampCultureInfoForReading = template.timestampCultureInfoForReading;
			this.timestampCultureInfoForWriting = template.timestampCultureInfoForWriting;
			this.timestampEncodingForReading = template.timestampEncodingForReading;
			this.timestampFormatForDisplaying = template.timestampFormatForDisplaying;
			this.timestampFormatForWriting = template.timestampFormatForWriting;
			this.timestampFormatsForReading = template.timestampFormatsForReading;
			this.visibleLogProperties = template.visibleLogProperties;
		}


		/// <summary>
		/// Get or set whether multiple files are allowed or not.
		/// </summary>
		public bool AllowMultipleFiles
		{
			get => this.allowMultipleFiles;
			set
			{
				this.VerifyAccess();
				this.VerifyBuiltIn();
				if (this.allowMultipleFiles == value)
					return;
				this.allowMultipleFiles = value;
				this.OnPropertyChanged(nameof(AllowMultipleFiles));
			}
		}


		/// <summary>
		/// Change ID of profile.
		/// </summary>
		internal void ChangeId()
		{
			this.VerifyAccess();
			this.VerifyBuiltIn();
			this.Id = LogProfileManager.Default.GenerateProfileId();
		}


		/// <summary>
		/// Get or set color indicator of log.
		/// </summary>
		public LogColorIndicator ColorIndicator
		{
			get => this.colorIndicator;
			set
			{
				this.VerifyAccess();
				this.VerifyBuiltIn();
				if (this.colorIndicator == value)
					return;
				this.colorIndicator = value;
				this.OnPropertyChanged(nameof(ColorIndicator));
			}
		}


		/// <summary>
		/// Create built-in empty log profile.
		/// </summary>
		/// <param name="app">Application.</param>
		/// <returns>Built-in empty log profile.</returns>
		internal static LogProfile CreateEmptyBuiltInProfile(IULogViewerApplication app) => new LogProfile(app, EmptyId, true);


		/// <summary>
		/// Get or set <see cref="LogDataSourceOptions"/> to create <see cref="ILogDataSource"/>.
		/// </summary>
		public LogDataSourceOptions DataSourceOptions
		{
			get => this.dataSourceOptions;
			set
			{
				this.VerifyAccess();
				this.VerifyBuiltIn();
				if (this.dataSourceOptions == value)
					return;
				this.dataSourceOptions = value;
				this.OnPropertyChanged(nameof(DataSourceOptions));
			}
		}


		/// <summary>
		/// Get or set <see cref="ILogDataSourceProvider"/> to build <see cref="ILogDataSource"/> instances for logs reading.
		/// </summary>
		public ILogDataSourceProvider DataSourceProvider
		{
			get => this.dataSourceProvider;
			set
			{
				this.VerifyAccess();
				this.VerifyBuiltIn();
				if (this.dataSourceProvider == value)
					return;
				if (value.IsSourceOptionRequired(nameof(LogDataSourceOptions.FileName)) && this.isContinuousReading)
					this.IsContinuousReading = false;
				this.dataSourceProvider = value;
				this.OnPropertyChanged(nameof(DataSourceProvider));
				this.Validate();
			}
		}


		/// <summary>
		/// Get or set description of profile.
		/// </summary>
		public string? Description
		{
			get => this.description;
			set
			{
				this.VerifyAccess();
				this.VerifyBuiltIn();
				if (string.IsNullOrWhiteSpace(value))
					value = null;
				if (this.description == value)
					return;
				this.description = value;
				this.HasDescription = (value != null);
				this.OnPropertyChanged(nameof(Description));
			}
		}


		/// <inheritdoc/>
		public override bool Equals(IProfile<IULogViewerApplication>? profile) =>
			this.Id == profile?.Id;


		/// <summary>
		/// Get whether <see cref="Description"/> contains non-whitespace content or not.
		/// </summary>
		public bool HasDescription 
		{
			get => this.hasDescription;
			private set
            {
				if (this.hasDescription == value)
					return;
				this.hasDescription = value;
				this.OnPropertyChanged(nameof(HasDescription));
            }
		}


		/// <summary>
		/// Get or set icon.
		/// </summary>
		public LogProfileIcon Icon
		{
			get => this.icon;
			set
			{
				this.VerifyAccess();
				this.VerifyBuiltIn();
				if (this.icon == value)
					return;
				this.icon = value;
				this.OnPropertyChanged(nameof(Icon));
			}
		}


		/// <summary>
		/// Get or set whether application should run as administrator/superuser or not.
		/// </summary>
		public bool IsAdministratorNeeded
		{
			get => this.isAdministratorNeeded;
			set
			{
				this.VerifyAccess();
				this.VerifyBuiltIn();
				if (this.isAdministratorNeeded == value)
					return;
				this.isAdministratorNeeded = value;
				this.OnPropertyChanged(nameof(IsAdministratorNeeded));
			}
		}


		/// <summary>
		/// Get or set whether should be read continuously or not.
		/// </summary>
		public bool IsContinuousReading
		{
			get => this.isContinuousReading;
			set
			{
				this.VerifyAccess();
				this.VerifyBuiltIn();
				if (this.isContinuousReading == value)
					return;
				if (value && this.dataSourceProvider.IsSourceOptionRequired(nameof(LogDataSourceOptions.FileName)))
					return;
				this.isContinuousReading = value;
				this.OnPropertyChanged(nameof(IsContinuousReading));
			}
		}


		/// <summary>
		/// Check whether internal data has been just upgraded or not.
		/// </summary>
		public bool IsDataUpgraded { get; private set; }


		/// <summary>
		/// Get or set whether profile should be pinned at quick access area or not.
		/// </summary>
		public bool IsPinned
		{
			get => this.isPinned;
			set
			{
				this.VerifyAccess();
				if (this.isPinned == value)
					return;
				this.isPinned = value;
				if (this.IsBuiltIn)
					this.Application.PersistentState.SetValue<bool>(this.isPinnedSettingKey.AsNonNull(), value);
				this.OnPropertyChanged(nameof(IsPinned));
			}
		}


		/// <summary>
		/// Check whether properties of profile is valid or not.
		/// </summary>
		public bool IsValid { get; private set; }


		/// <summary>
		/// Get or set whether working directory is needed to be set before reading logs or not.
		/// </summary>
		public bool IsWorkingDirectoryNeeded
		{
			get => this.isWorkingDirectoryNeeded;
			set
			{
				this.VerifyAccess();
				this.VerifyBuiltIn();
				if (this.isWorkingDirectoryNeeded == value)
					return;
				this.isWorkingDirectoryNeeded = value;
				this.OnPropertyChanged(nameof(IsWorkingDirectoryNeeded));
			}
		}


		/// <summary>
		/// Load profile asynchronously.
		/// </summary>
		/// <param name="app">Application.</param>
		/// <param name="fileName">Name of profile file.</param>
		/// <returns>Task of loading operation.</returns>
		public static async Task<LogProfile> LoadAsync(IULogViewerApplication app, string fileName)
		{
			// load JSON document
			var jsonDocument = await Task.Run(() =>
			{
				using var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
				return JsonDocument.Parse(stream);
			});
			var element = jsonDocument.RootElement;
			if (element.ValueKind != JsonValueKind.Object)
				throw new ArgumentException("Root element must be an object.");

			// get ID
			var id = element.TryGetProperty(nameof(Id), out var jsonProperty) && jsonProperty.ValueKind == JsonValueKind.String
				? jsonProperty.GetString()!
				: LogProfileManager.Default.GenerateProfileId();

			// load profile
			var profile = new LogProfile(app, id, false);
			profile.Load(element);

			// validate
			profile.Validate();

			// complete
			return profile;
		}


		/// <summary>
		/// Load built-in profile asynchronously.
		/// </summary>
		/// <param name="app">Application.</param>
		/// <param name="id">ID of built-in profile.</param>
		/// <returns>Task of loading operation.</returns>
		public static async Task<LogProfile> LoadBuiltInAsync(IULogViewerApplication app, string id)
		{
			// load JSON document
			var jsonDocument = await Task.Run(() =>
			{
				using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"CarinaStudio.ULogViewer.Logs.Profiles.BuiltIn.{id}.json") ?? throw new ArgumentException($"Cannot find built-in profile '{id}'.");
				return JsonDocument.Parse(stream);
			});

			// load profile
			var profile = new LogProfile(app, id, true);
			profile.Load(jsonDocument.RootElement);
			profile.isPinned = app.PersistentState.GetValueOrDefault(profile.isPinnedSettingKey.AsNonNull());

			// validate
			profile.Validate();

			// complete
			return profile;
		}


		// Load ILogDataSourceProvider from JSON element.
		void LoadDataSourceFromJson(JsonElement dataSourceElement)
		{
			// find provider
			var providerName = dataSourceElement.GetProperty("Name").GetString() ?? throw new ArgumentException("No name of data source.");
			if (!LogDataSourceProviders.TryFindProviderByName(providerName, out var provider) || provider == null)
				throw new ArgumentException($"Cannot find data source '{providerName}'.");

			// get opetions
			var options = new LogDataSourceOptions();
			if (dataSourceElement.TryGetProperty("Options", out var jsonValue))
				options = LogDataSourceOptions.Load(jsonValue);
			else
			{
				// get options by old way
				var crypto = (Crypto?)null;
				try
				{
					foreach (var jsonProperty in dataSourceElement.EnumerateObject())
					{
						switch (jsonProperty.Name)
						{
							case nameof(LogDataSourceOptions.Category):
								options.Category = jsonProperty.Value.GetString();
								break;
							case nameof(LogDataSourceOptions.Command):
								options.Command = jsonProperty.Value.GetString();
								break;
							case nameof(LogDataSourceOptions.Encoding):
								options.Encoding = Encoding.GetEncoding(jsonProperty.Value.GetString().AsNonNull());
								break;
							case nameof(LogDataSourceOptions.FileName):
								options.FileName = jsonProperty.Value.GetString();
								break;
							case "Name":
								continue;
							case nameof(LogDataSourceOptions.Password):
								if (crypto == null)
									crypto = new Crypto(this.Application);
								options.Password = crypto.Decrypt(jsonProperty.Value.GetString().AsNonNull());
								break;
							case nameof(LogDataSourceOptions.QueryString):
								options.QueryString = jsonProperty.Value.GetString();
								break;
							case nameof(LogDataSourceOptions.SetupCommands):
								options.SetupCommands = new List<string>().Also(it =>
								{
									foreach (var jsonElement in jsonProperty.Value.EnumerateArray())
										it.Add(jsonElement.GetString().AsNonNull());
								});
								break;
							case nameof(LogDataSourceOptions.TeardownCommands):
								options.TeardownCommands = new List<string>().Also(it =>
								{
									foreach (var jsonElement in jsonProperty.Value.EnumerateArray())
										it.Add(jsonElement.GetString().AsNonNull());
								});
								break;
							case nameof(LogDataSourceOptions.Uri):
								options.Uri = new Uri(jsonProperty.Value.GetString().AsNonNull());
								break;
							case nameof(LogDataSourceOptions.UserName):
								if (crypto == null)
									crypto = new Crypto(this.Application);
								options.UserName = crypto.Decrypt(jsonProperty.Value.GetString().AsNonNull());
								break;
							case nameof(LogDataSourceOptions.WorkingDirectory):
								options.WorkingDirectory = jsonProperty.Value.GetString();
								break;
							default:
								this.Logger.LogWarning($"Unknown property of DataSource: {jsonProperty.Name}");
								continue;
						}
						this.IsDataUpgraded = true;
					}
				}
				finally
				{
					crypto?.Dispose();
				}
			}

			// complete
			this.dataSourceOptions = options;
			this.dataSourceProvider = provider;
		}


		// Load log level map from JSON.
		void LoadLogLevelMapForReadingFromJson(JsonElement logLevelMapElement)
		{
			this.logLevelMapForReading.Clear();
			foreach (var jsonProperty in logLevelMapElement.EnumerateObject())
			{
				var key = jsonProperty.Name;
				var logLevel = Enum.Parse<LogLevel>(jsonProperty.Value.GetString() ?? "");
				this.logLevelMapForReading[key] = logLevel;
			}
		}


		// Load log level map from JSON.
		void LoadLogLevelMapForWritingFromJson(JsonElement logLevelMapElement)
		{
			this.logLevelMapForWriting.Clear();
			foreach (var jsonProperty in logLevelMapElement.EnumerateObject())
			{
				var logLevel = Enum.Parse<LogLevel>(jsonProperty.Name);
				this.logLevelMapForWriting[logLevel] = jsonProperty.Value.GetString().AsNonNull();
			}
		}


		// Load log patterns from JSON.
		void LoadLogPatternsFromJson(JsonElement logPatternsElement)
		{
			var logPatterns = new List<LogPattern>();
			foreach (var logPatternElement in logPatternsElement.EnumerateArray())
			{
				var ignoreCase = logPatternElement.TryGetProperty(nameof(RegexOptions.IgnoreCase), out var jsonProperty) && jsonProperty.ValueKind == JsonValueKind.True;
				var regex = new Regex(logPatternElement.GetProperty("Regex").GetString()!, ignoreCase ? RegexOptions.IgnoreCase : RegexOptions.None);
				var isRepeatable = false;
				var isSkippable = false;
				if (logPatternElement.TryGetProperty(nameof(LogPattern.IsRepeatable), out jsonProperty))
					isRepeatable = jsonProperty.GetBoolean();
				if (logPatternElement.TryGetProperty(nameof(LogPattern.IsSkippable), out jsonProperty))
					isSkippable = jsonProperty.GetBoolean();
				logPatterns.Add(new LogPattern(regex, isRepeatable, isSkippable));
			}
			this.logPatterns = logPatterns.AsReadOnly();
		}


		// Load visible log properties from JSON.
		void LoadVisibleLogPropertiesFromJson(JsonElement visibleLogPropertiesElement)
		{
			var logProperties = new List<LogProperty>();
			foreach (var logPropertyElement in visibleLogPropertiesElement.EnumerateArray())
			{
				var name = logPropertyElement.GetProperty("Name").GetString().AsNonNull();
				var displayName = (string?)null;
				var width = (int?)null;
				if (logPropertyElement.TryGetProperty("DisplayName", out var jsonElement))
					displayName = jsonElement.GetString();
				if (logPropertyElement.TryGetProperty("Width", out jsonElement))
					width = jsonElement.GetInt32();
				logProperties.Add(new LogProperty(name, displayName, width));
			}
			this.visibleLogProperties = logProperties.AsReadOnly();
		}


		/// <summary>
		/// Get or set map of conversion from string to <see cref="LogLevel"/>.
		/// </summary>
		public IDictionary<string, LogLevel> LogLevelMapForReading
		{
			get => this.readOnlyLogLevelMapForReading;
			set
			{
				this.VerifyAccess();
				this.VerifyBuiltIn();
				if (this.logLevelMapForReading.SequenceEqual(value))
					return;
				this.logLevelMapForReading.Clear();
				this.logLevelMapForReading.AddAll(value);
				this.OnPropertyChanged(nameof(LogLevelMapForReading));
			}
		}


		/// <summary>
		/// Get or set map of conversion from <see cref="LogLevel"/> to string.
		/// </summary>
		public IDictionary<LogLevel, string> LogLevelMapForWriting
		{
			get => this.readOnlyLogLevelMapForWriting;
			set
			{
				this.VerifyAccess();
				this.VerifyBuiltIn();
				if (this.logLevelMapForWriting.SequenceEqual(value))
					return;
				this.logLevelMapForWriting.Clear();
				this.logLevelMapForWriting.AddAll(value);
				this.OnPropertyChanged(nameof(LogLevelMapForWriting));
			}
		}


		/// <summary>
		/// Get of set list of <see cref="LogPattern"/> to parse log data.
		/// </summary>
		public IList<LogPattern> LogPatterns
		{
			get => this.logPatterns;
			set
			{
				this.VerifyAccess();
				this.VerifyBuiltIn();
				if (this.logPatterns.SequenceEqual(value))
					return;
				this.logPatterns = new List<LogPattern>(value).AsReadOnly();
				this.OnPropertyChanged(nameof(LogPatterns));
				this.Validate();
			}
		}


		/// <summary>
		/// Get of set string encoding of logs for reading.
		/// </summary>
		public LogStringEncoding LogStringEncodingForReading
		{
			get => this.logStringEncodingForReading;
			set
			{
				this.VerifyAccess();
				this.VerifyBuiltIn();
				if (this.logStringEncodingForReading == value)
					return;
				this.logStringEncodingForReading = value;
				this.OnPropertyChanged(nameof(LogStringEncodingForReading));
			}
		}


		/// <summary>
		/// Get of set string encoding of logs for writing.
		/// </summary>
		public LogStringEncoding LogStringEncodingForWriting
		{
			get => this.logStringEncodingForWriting;
			set
			{
				this.VerifyAccess();
				this.VerifyBuiltIn();
				if (this.logStringEncodingForWriting == value)
					return;
				this.logStringEncodingForWriting = value;
				this.OnPropertyChanged(nameof(LogStringEncodingForWriting));
			}
		}


		/// <summary>
		/// Get of set format to write log.
		/// </summary>
		public string? LogWritingFormat
		{
			get => this.logWritingFormat;
			set
			{
				this.VerifyAccess();
				this.VerifyBuiltIn();
				if (this.logWritingFormat == value)
					return;
				this.logWritingFormat = value;
				this.OnPropertyChanged(nameof(LogWritingFormat));
			}
		}


		/// <summary>
		/// Get of set name of profile.
		/// </summary>
		public override string? Name
		{
			get => this.IsBuiltIn ? this.builtInName : base.Name;
			set => base.Name = value;
		}


		/// <summary>
		/// Called when application string resources updated.
		/// </summary>
		public void OnApplicationStringsUpdated() => this.UpdateBuiltInNameAndDescription();


		/// <inheritdoc/>
		protected override void OnLoad(JsonElement element)
		{
			foreach (var jsonProperty in element.EnumerateObject())
			{
				switch (jsonProperty.Name)
				{
					case nameof(AllowMultipleFiles):
						this.allowMultipleFiles = jsonProperty.Value.ValueKind != JsonValueKind.False;
						break;
					case "DataSource":
						this.LoadDataSourceFromJson(jsonProperty.Value);
						break;
					case nameof(ColorIndicator):
						this.colorIndicator = Enum.Parse<LogColorIndicator>(jsonProperty.Value.GetString().AsNonNull());
						break;
					case nameof(Description):
						this.description = jsonProperty.Value.GetString();
						if (string.IsNullOrWhiteSpace(this.description))
							this.description = null;
						this.hasDescription = (this.description != null);
						break;
					case nameof(Icon):
						if (Enum.TryParse<LogProfileIcon>(jsonProperty.Value.GetString(), out var profileIcon))
							this.icon = profileIcon;
						break;
					case nameof(Id):
						break;
					case nameof(IsAdministratorNeeded):
						this.isAdministratorNeeded = jsonProperty.Value.GetBoolean();
						break;
					case nameof(IsContinuousReading):
						this.isContinuousReading = jsonProperty.Value.GetBoolean();
						break;
					case nameof(IsPinned):
						this.isPinned = jsonProperty.Value.GetBoolean();
						break;
					case nameof(IsWorkingDirectoryNeeded):
						this.isWorkingDirectoryNeeded = jsonProperty.Value.GetBoolean();
						break;
					case nameof(LogLevelMapForReading):
						this.LoadLogLevelMapForReadingFromJson(jsonProperty.Value);
						break;
					case nameof(LogLevelMapForWriting):
						this.LoadLogLevelMapForWritingFromJson(jsonProperty.Value);
						break;
					case nameof(LogPatterns):
						this.LoadLogPatternsFromJson(jsonProperty.Value);
						break;
					case nameof(LogStringEncodingForReading):
						if (Enum.TryParse<LogStringEncoding>(jsonProperty.Value.GetString(), out var encoding))
							this.logStringEncodingForReading = encoding;
						break;
					case nameof(LogStringEncodingForWriting):
						if (Enum.TryParse<LogStringEncoding>(jsonProperty.Value.GetString(), out encoding))
							this.logStringEncodingForWriting = encoding;
						break;
					case nameof(LogWritingFormat):
						this.logWritingFormat = jsonProperty.Value.GetString();
						break;
					case nameof(Name):
						if (this.IsBuiltIn)
							this.builtInName = jsonProperty.Value.GetString()!;
						else
							this.Name = jsonProperty.Value.GetString();
						break;
					case nameof(RestartReadingDelay):
						this.restartReadingDelay = Math.Max(0, jsonProperty.Value.GetInt64());
						break;
					case nameof(SortDirection):
						this.sortDirection = Enum.Parse<SortDirection>(jsonProperty.Value.GetString().AsNonNull());
						break;
					case nameof(SortKey):
						this.sortKey = Enum.Parse<LogSortKey>(jsonProperty.Value.GetString().AsNonNull());
						break;
					case nameof(TimeSpanCultureInfoForReading):
						this.timeSpanCultureInfoForReading = CultureInfo.GetCultureInfo(jsonProperty.Value.GetString().AsNonNull());
						break;
					case nameof(TimeSpanCultureInfoForWriting):
						this.timeSpanCultureInfoForWriting = CultureInfo.GetCultureInfo(jsonProperty.Value.GetString().AsNonNull());
						break;
					case nameof(TimeSpanEncodingForReading):
						if (Enum.TryParse<LogTimeSpanEncoding>(jsonProperty.Value.GetString(), out var timeSpanEncoding))
							this.timeSpanEncodingForReading = timeSpanEncoding;
						break;
					case nameof(TimeSpanFormatForDisplaying):
						this.timeSpanFormatForDisplaying = jsonProperty.Value.GetString();
						break;
					case nameof(TimeSpanFormatForWriting):
						this.timeSpanFormatForWriting = jsonProperty.Value.GetString();
						break;
					case nameof(TimeSpanFormatsForReading):
						this.timeSpanFormatsForReading = new List<string>().Also(list =>
						{
							foreach (var jsonValue in jsonProperty.Value.EnumerateArray())
								list.Add(jsonValue.GetString().AsNonNull());
						}).AsReadOnly();
						break;
					case nameof(TimestampCategoryGranularity):
						if (Enum.TryParse<TimestampDisplayableLogCategoryGranularity>(jsonProperty.Value.GetString(), out var timestampCategoryGranularity))
							this.timestampCategoryGranularity = timestampCategoryGranularity;
						break;
					case nameof(TimestampCultureInfoForReading):
						this.timestampCultureInfoForReading = CultureInfo.GetCultureInfo(jsonProperty.Value.GetString().AsNonNull());
						break;
					case nameof(TimestampCultureInfoForWriting):
						this.timestampCultureInfoForWriting = CultureInfo.GetCultureInfo(jsonProperty.Value.GetString().AsNonNull());
						break;
					case nameof(TimestampEncodingForReading):
						if (Enum.TryParse<LogTimestampEncoding>(jsonProperty.Value.GetString(), out var timestampEncoding))
							this.timestampEncodingForReading = timestampEncoding;
						break;
					case nameof(TimestampFormatForDisplaying):
						this.timestampFormatForDisplaying = jsonProperty.Value.GetString();
						break;
					case "TimestampFormatForReading":
						this.timestampFormatsForReading = new string[] { jsonProperty.Value.GetString().AsNonNull() };
						this.IsDataUpgraded = true;
						break;
					case nameof(TimestampFormatForWriting):
						this.timestampFormatForWriting = jsonProperty.Value.GetString();
						break;
					case nameof(TimestampFormatsForReading):
						this.timestampFormatsForReading = new List<string>().Also(list =>
						{
							foreach (var jsonValue in jsonProperty.Value.EnumerateArray())
								list.Add(jsonValue.GetString().AsNonNull());
						}).AsReadOnly();
						break;
					case nameof(VisibleLogProperties):
						this.LoadVisibleLogPropertiesFromJson(jsonProperty.Value);
						break;
					default:
						this.Logger.LogWarning($"Unknown property of profile: {jsonProperty.Name}");
						break;
				}
			}
		}


		/// <inheritdoc/>
		protected override void OnSave(Utf8JsonWriter writer, bool includeId)
		{
			writer.WriteStartObject();
			writer.WriteBoolean(nameof(AllowMultipleFiles), this.allowMultipleFiles);
			writer.WritePropertyName("DataSource");
			this.SaveDataSourceToJson(writer);
			writer.WriteString(nameof(ColorIndicator), this.colorIndicator.ToString());
			writer.WriteString(nameof(Description), this.description);
			writer.WriteString(nameof(Icon), this.icon.ToString());
			if (!this.IsBuiltIn && includeId)
				writer.WriteString(nameof(Id), this.Id);
			if (this.isAdministratorNeeded)
				writer.WriteBoolean(nameof(IsAdministratorNeeded), true);
			if (this.isContinuousReading)
				writer.WriteBoolean(nameof(IsContinuousReading), true);
			if (this.isPinned)
				writer.WriteBoolean(nameof(IsPinned), true);
			if (this.isWorkingDirectoryNeeded)
				writer.WriteBoolean(nameof(IsWorkingDirectoryNeeded), true);
			writer.WritePropertyName(nameof(LogLevelMapForReading));
			this.SaveLogLevelMapForReadingToJson(writer);
			writer.WritePropertyName(nameof(LogLevelMapForWriting));
			this.SaveLogLevelMapForWritingToJson(writer);
			writer.WritePropertyName(nameof(LogPatterns));
			this.SaveLogPatternsToJson(writer);
			writer.WriteString(nameof(LogStringEncodingForReading), this.logStringEncodingForReading.ToString());
			writer.WriteString(nameof(LogStringEncodingForWriting), this.logStringEncodingForWriting.ToString());
			this.logWritingFormat?.Let(it => writer.WriteString(nameof(LogWritingFormat), it));
			writer.WriteString(nameof(Name), this.Name);
			writer.WriteNumber(nameof(RestartReadingDelay), this.restartReadingDelay);
			writer.WriteString(nameof(SortDirection), this.sortDirection.ToString());
			writer.WriteString(nameof(SortKey), this.sortKey.ToString());
			writer.WriteString(nameof(TimeSpanCultureInfoForReading), this.timeSpanCultureInfoForReading.ToString());
			writer.WriteString(nameof(TimeSpanCultureInfoForWriting), this.timeSpanCultureInfoForWriting.ToString());
			writer.WriteString(nameof(TimeSpanEncodingForReading), this.timestampEncodingForReading.ToString());
			this.timeSpanFormatForDisplaying?.Let(it => writer.WriteString(nameof(TimeSpanFormatForDisplaying), it));
			this.timeSpanFormatForWriting?.Let(it => writer.WriteString(nameof(TimeSpanFormatForWriting), it));
			if (timeSpanFormatsForReading.IsNotEmpty())
			{
				writer.WritePropertyName(nameof(TimeSpanFormatsForReading));
				writer.WriteStartArray();
				foreach (var format in timeSpanFormatsForReading)
					writer.WriteStringValue(format);
				writer.WriteEndArray();
			}
			writer.WriteString(nameof(TimestampCategoryGranularity), this.timestampCategoryGranularity.ToString());
			writer.WriteString(nameof(TimestampCultureInfoForReading), this.timestampCultureInfoForReading.ToString());
			writer.WriteString(nameof(TimestampCultureInfoForWriting), this.timestampCultureInfoForWriting.ToString());
			writer.WriteString(nameof(TimestampEncodingForReading), this.timestampEncodingForReading.ToString());
			this.timestampFormatForDisplaying?.Let(it => writer.WriteString(nameof(TimestampFormatForDisplaying), it));
			this.timestampFormatForWriting?.Let(it => writer.WriteString(nameof(TimestampFormatForWriting), it));
			if (timestampFormatsForReading.IsNotEmpty())
			{
				writer.WritePropertyName(nameof(TimestampFormatsForReading));
				writer.WriteStartArray();
				foreach (var format in timestampFormatsForReading)
					writer.WriteStringValue(format);
				writer.WriteEndArray();
			}
			writer.WritePropertyName(nameof(VisibleLogProperties));
			this.SaveVisibleLogPropertiesToJson(writer);
			writer.WriteEndObject();
		}


		/// <summary>
		/// Get or set delay before restarting logs reading for continuous reading case in milliseconds.
		/// </summary>
		public long RestartReadingDelay
		{
			get => this.restartReadingDelay;
			set
			{
				this.VerifyAccess();
				this.VerifyBuiltIn();
				if (this.restartReadingDelay == value)
					return;
				if (!this.isContinuousReading)
					return;
				if (value < 0)
					throw new ArgumentOutOfRangeException();
				this.restartReadingDelay = value;
				this.OnPropertyChanged(nameof(RestartReadingDelay));
			}
		}


		// Save data source info in JSON format.
		void SaveDataSourceToJson(Utf8JsonWriter writer)
		{
			var provider = this.dataSourceProvider;
			var options = this.dataSourceOptions;
			writer.WriteStartObject();
			writer.WriteString("Name", provider.Name);
			writer.WritePropertyName("Options");
			options.Save(writer);
			writer.WriteEndObject();
		}


		// Save log level map in JSON format.
		void SaveLogLevelMapForReadingToJson(Utf8JsonWriter writer)
		{
			var map = this.logLevelMapForReading;
			writer.WriteStartObject();
			foreach (var kvPair in map)
				writer.WriteString(kvPair.Key, kvPair.Value.ToString());
			writer.WriteEndObject();
		}


		// Save log level map in JSON format.
		void SaveLogLevelMapForWritingToJson(Utf8JsonWriter writer)
		{
			var map = this.logLevelMapForWriting;
			writer.WriteStartObject();
			foreach (var kvPair in map)
				writer.WriteString(kvPair.Key.ToString(), kvPair.Value);
			writer.WriteEndObject();
		}


		// Save log patterns in JSON format.
		void SaveLogPatternsToJson(Utf8JsonWriter writer)
		{
			var patterns = this.logPatterns;
			writer.WriteStartArray();
			foreach (var pattern in patterns)
			{
				writer.WriteStartObject();
				writer.WriteString(nameof(LogPattern.Regex), pattern.Regex.ToString());
				if ((pattern.Regex.Options & RegexOptions.IgnoreCase) != 0)
					writer.WriteBoolean(nameof(RegexOptions.IgnoreCase), true);
				if (pattern.IsRepeatable)
					writer.WriteBoolean(nameof(LogPattern.IsRepeatable), true);
				if (pattern.IsSkippable)
					writer.WriteBoolean(nameof(LogPattern.IsSkippable), true);
				writer.WriteEndObject();
			}
			writer.WriteEndArray();
		}


		// Save visible log properties in JSON format.
		void SaveVisibleLogPropertiesToJson(Utf8JsonWriter writer)
		{
			var properties = this.visibleLogProperties;
			writer.WriteStartArray();
			foreach (var property in properties)
			{
				writer.WriteStartObject();
				if (property.DisplayName != property.Name)
					writer.WriteString(nameof(LogProperty.DisplayName), property.DisplayName);
				writer.WriteString(nameof(LogProperty.Name), property.Name);
				property.Width?.Let(it => writer.WriteNumber(nameof(LogProperty.Width), it));
				writer.WriteEndObject();
			}
			writer.WriteEndArray();
		}


		/// <summary>
		/// Get of set direction of log sorting.
		/// </summary>
		public SortDirection SortDirection
		{
			get => this.sortDirection;
			set
			{
				this.VerifyAccess();
				this.VerifyBuiltIn();
				if (this.sortDirection == value)
					return;
				this.sortDirection = value;
				this.OnPropertyChanged(nameof(SortDirection));
			}
		}


		/// <summary>
		/// Get of set key of log sorting.
		/// </summary>
		public LogSortKey SortKey
		{
			get => this.sortKey;
			set
			{
				this.VerifyAccess();
				this.VerifyBuiltIn();
				if (this.sortKey == value)
					return;
				this.sortKey = value;
				this.OnPropertyChanged(nameof(SortKey));
			}
		}


		/// <summary>
		/// Get or set <see cref="CultureInfo"/> of time span for reading logs.
		/// </summary>
		public CultureInfo TimeSpanCultureInfoForReading
		{
			get => this.timeSpanCultureInfoForReading;
			set
			{
				this.VerifyAccess();
				this.VerifyBuiltIn();
				if (this.timeSpanCultureInfoForReading.Equals(value))
					return;
				this.timeSpanCultureInfoForReading = value;
				this.OnPropertyChanged(nameof(TimeSpanCultureInfoForReading));
			}
		}


		/// <summary>
		/// Get or set <see cref="CultureInfo"/> of time span for writing logs.
		/// </summary>
		public CultureInfo TimeSpanCultureInfoForWriting
		{
			get => this.timeSpanCultureInfoForWriting;
			set
			{
				this.VerifyAccess();
				this.VerifyBuiltIn();
				if (this.timeSpanCultureInfoForWriting.Equals(value))
					return;
				this.timeSpanCultureInfoForWriting = value;
				this.OnPropertyChanged(nameof(TimeSpanCultureInfoForWriting));
			}
		}


		/// <summary>
		/// Get or set encoding of time span for reading logs.
		/// </summary>
		public LogTimeSpanEncoding TimeSpanEncodingForReading
		{
			get => this.timeSpanEncodingForReading;
			set
			{
				this.VerifyAccess();
				this.VerifyBuiltIn();
				if (this.timeSpanEncodingForReading == value)
					return;
				this.timeSpanEncodingForReading = value;
				this.OnPropertyChanged(nameof(TimeSpanEncodingForReading));
			}
		}


		/// <summary>
		/// Get or set format of time span for displaying logs.
		/// </summary>
		public string? TimeSpanFormatForDisplaying
		{
			get => this.timeSpanFormatForDisplaying;
			set
			{
				this.VerifyAccess();
				this.VerifyBuiltIn();
				if (this.timeSpanFormatForDisplaying == value)
					return;
				this.timeSpanFormatForDisplaying = value;
				this.OnPropertyChanged(nameof(TimeSpanFormatForDisplaying));
			}
		}


		/// <summary>
		/// Get or set format of time span for writing logs.
		/// </summary>
		public string? TimeSpanFormatForWriting
		{
			get => this.timeSpanFormatForWriting;
			set
			{
				this.VerifyAccess();
				this.VerifyBuiltIn();
				if (this.timeSpanFormatForWriting == value)
					return;
				this.timeSpanFormatForWriting = value;
				this.OnPropertyChanged(nameof(TimeSpanFormatForWriting));
			}
		}


		/// <summary>
		/// Get or set list of format of time span for reading logs if <see cref="TimeSpanEncodingForReading"/> is <see cref="LogTimeSpanEncoding.Custom"/>.
		/// </summary>
		public IList<string> TimeSpanFormatsForReading
		{
			get => this.timeSpanFormatsForReading;
			set
			{
				this.VerifyAccess();
				this.VerifyBuiltIn();
				if (this.timeSpanFormatsForReading.SequenceEqual(value))
					return;
				this.timeSpanFormatsForReading = value.ToArray().AsReadOnly();
				this.OnPropertyChanged(nameof(TimeSpanFormatsForReading));
			}
		}


		/// <summary>
		/// Get or set granularity of categorizing logs by timestamp.
		/// </summary>
		public TimestampDisplayableLogCategoryGranularity TimestampCategoryGranularity
		{
			get => this.timestampCategoryGranularity;
			set
			{
				this.VerifyAccess();
				this.VerifyBuiltIn();
				if (this.timestampCategoryGranularity == value)
					return;
				this.timestampCategoryGranularity = value;
				this.OnPropertyChanged(nameof(timestampCategoryGranularity));
			}
		}


		/// <summary>
		/// Get or set <see cref="CultureInfo"/> of timestamp for reading logs.
		/// </summary>
		public CultureInfo TimestampCultureInfoForReading
		{
			get => this.timestampCultureInfoForReading;
			set
			{
				this.VerifyAccess();
				this.VerifyBuiltIn();
				if (this.timestampCultureInfoForReading.Equals(value))
					return;
				this.timestampCultureInfoForReading = value;
				this.OnPropertyChanged(nameof(TimestampCultureInfoForReading));
			}
		}


		/// <summary>
		/// Get or set <see cref="CultureInfo"/> of timestamp for writing logs.
		/// </summary>
		public CultureInfo TimestampCultureInfoForWriting
		{
			get => this.timestampCultureInfoForWriting;
			set
			{
				this.VerifyAccess();
				this.VerifyBuiltIn();
				if (this.timestampCultureInfoForWriting.Equals(value))
					return;
				this.timestampCultureInfoForWriting = value;
				this.OnPropertyChanged(nameof(TimestampCultureInfoForWriting));
			}
		}


		/// <summary>
		/// Get or set encoding of timestamp for reading logs.
		/// </summary>
		public LogTimestampEncoding TimestampEncodingForReading
		{
			get => this.timestampEncodingForReading;
			set
			{
				this.VerifyAccess();
				this.VerifyBuiltIn();
				if (this.timestampEncodingForReading == value)
					return;
				this.timestampEncodingForReading = value;
				this.OnPropertyChanged(nameof(TimestampEncodingForReading));
			}
		}


		/// <summary>
		/// Get or set format of timestamp for displaying logs.
		/// </summary>
		public string? TimestampFormatForDisplaying
		{
			get => this.timestampFormatForDisplaying;
			set
			{
				this.VerifyAccess();
				this.VerifyBuiltIn();
				if (this.timestampFormatForDisplaying == value)
					return;
				this.timestampFormatForDisplaying = value;
				this.OnPropertyChanged(nameof(TimestampFormatForDisplaying));
			}
		}


		/// <summary>
		/// Get or set format of timestamp for writing logs.
		/// </summary>
		public string? TimestampFormatForWriting
		{
			get => this.timestampFormatForWriting;
			set
			{
				this.VerifyAccess();
				this.VerifyBuiltIn();
				if (this.timestampFormatForWriting == value)
					return;
				this.timestampFormatForWriting = value;
				this.OnPropertyChanged(nameof(TimestampFormatForWriting));
			}
		}


		/// <summary>
		/// Get or set list of format of timestamp for reading logs if <see cref="TimestampEncodingForReading"/> is <see cref="LogTimestampEncoding.Custom"/>.
		/// </summary>
		public IList<string> TimestampFormatsForReading
		{
			get => this.timestampFormatsForReading;
			set
			{
				this.VerifyAccess();
				this.VerifyBuiltIn();
				if (this.timestampFormatsForReading.SequenceEqual(value))
					return;
				this.timestampFormatsForReading = value.ToArray().AsReadOnly();
				this.OnPropertyChanged(nameof(TimestampFormatsForReading));
			}
		}


		// Update name and description of built-in profile.
		void UpdateBuiltInNameAndDescription()
		{
			if (!this.IsBuiltIn)
				return;
			var name = this.Application.GetStringNonNull($"BuiltInProfile.{this.Id}", this.Id);
			if (this.builtInName != name)
			{
				this.builtInName = name;
				this.OnPropertyChanged(nameof(Name));
			}
			var description = this.Application.GetString($"BuiltInProfile.{this.Id}.Description");
			if (this.description != description)
			{
				this.description = description;
				this.HasDescription = (description != null);
				this.OnPropertyChanged(nameof(Description));
			}
		}


		// Check whether properties of profile is valid or not.
		void Validate()
		{
			var isValid = this.dataSourceProvider is not EmptyLogDataSourceProvider
				&& this.logPatterns.IsNotEmpty()
				&& this.visibleLogProperties.Let(it =>
				{
					if (it.IsEmpty())
						return false;
					foreach (var property in it)
					{
						if (!Log.HasProperty(property.Name))
							return false;
					}
					return true;
				});
			if (this.IsValid == isValid)
				return;
			this.IsValid = isValid;
			this.OnPropertyChanged(nameof(IsValid));
		}


		// <summary>
		/// Get of set list of log properties to be shown to user.
		/// </summary>
		public IList<LogProperty> VisibleLogProperties
		{
			get => this.visibleLogProperties;
			set
			{
				this.VerifyAccess();
				this.VerifyBuiltIn();
				if (this.visibleLogProperties.SequenceEqual(value))
					return;
				this.visibleLogProperties = new List<LogProperty>(value).AsReadOnly();
				this.OnPropertyChanged(nameof(VisibleLogProperties));
				this.Validate();
			}
		}
	}
}
