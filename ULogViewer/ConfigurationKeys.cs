using CarinaStudio.Configuration;
using System;

namespace CarinaStudio.ULogViewer;

/// <summary>
/// Configuration keys.
/// </summary>
sealed class ConfigurationKeys
{
    /// <summary>
    /// Interval of updating continuous logs reading in milliseconds.
    /// </summary>
    public static readonly SettingKey<int> ContinuousLogsReadingUpdateInterval = new(nameof(ContinuousLogsReadingUpdateInterval), 100);
    /// <summary>
    /// Interval between each displayable log chunk filtering in milliseconds.
    /// </summary>
    public static readonly SettingKey<int> DisplayableLogChunkFilteringPaddingInterval = new(nameof(DisplayableLogChunkFilteringPaddingInterval), 50);
    /// <summary>
    /// Size of chunk of displayable log filtering.
    /// </summary>
    public static readonly SettingKey<int> DisplayableLogChunkFilteringSize = new(nameof(DisplayableLogChunkFilteringSize), 16384);
    /// <summary>
    /// Interval between each displayable log chunk processing in milliseconds.
    /// </summary>
    public static readonly SettingKey<int> DisplayableLogChunkProcessingPaddingIntervalBackground = new(nameof(DisplayableLogChunkProcessingPaddingIntervalBackground), 300);
    /// <summary>
    /// Interval between each displayable log chunk processing in milliseconds.
    /// </summary>
    public static readonly SettingKey<int> DisplayableLogChunkProcessingPaddingIntervalDefault = new(nameof(DisplayableLogChunkProcessingPaddingIntervalDefault), 100);
    /// <summary>
    /// Interval between each displayable log chunk processing in milliseconds.
    /// </summary>
    public static readonly SettingKey<int> DisplayableLogChunkProcessingPaddingIntervalRealtime = new(nameof(DisplayableLogChunkProcessingPaddingIntervalRealtime), 50);
    /// <summary>
    /// Size of chunk of displayable log processing.
    /// </summary>
    public static readonly SettingKey<int> DisplayableLogChunkProcessingSizeBackground = new(nameof(DisplayableLogChunkProcessingSizeBackground), 1024);
     /// <summary>
    /// Size of chunk of displayable log processing.
    /// </summary>
    public static readonly SettingKey<int> DisplayableLogChunkProcessingSizeDefault = new(nameof(DisplayableLogChunkProcessingSizeDefault), 4096);
     /// <summary>
    /// Size of chunk of displayable log processing.
    /// </summary>
    public static readonly SettingKey<int> DisplayableLogChunkProcessingSizeRealtime = new(nameof(DisplayableLogChunkProcessingSizeRealtime), 16384);
    /// <summary>
    /// Delay before start processing displayable logs in milliseconds.
    /// </summary>
    public static readonly SettingKey<int> DisplayableLogProcessinDelayBackground = new(nameof(DisplayableLogProcessinDelayBackground), 1000);
    /// <summary>
    /// Delay before start processing displayable logs in milliseconds.
    /// </summary>
    public static readonly SettingKey<int> DisplayableLogProcessinDelayDefault = new(nameof(DisplayableLogProcessinDelayDefault), 500);
    /// <summary>
    /// Delay before applying log analysis parameters in milliseconds.
    /// </summary>
    public static readonly SettingKey<int> LogAnalysisParamsUpdateDelay = new(nameof(LogAnalysisParamsUpdateDelay), 500);
    /// <summary>
    /// Size of chunk of non-continuous logs reading.
    /// </summary>
    public static readonly SettingKey<int> NonContinuousLogsReadingUpdateChunkSize = new(nameof(NonContinuousLogsReadingUpdateChunkSize), 32768);
    /// <summary>
    /// Interval of updating non-continuous logs reading in milliseconds.
    /// </summary>
    public static readonly SettingKey<int> NonContinuousLogsReadingUpdateInterval = new(nameof(NonContinuousLogsReadingUpdateInterval), 2000);
    /// <summary>
    /// Interval between non-continuous logs reading reporting in milliseconds.
    /// </summary>
    public static readonly SettingKey<int> NonContinuousLogsReadingPaddingInterval = new(nameof(NonContinuousLogsReadingPaddingInterval), 50);
    /// <summary>
    /// Size of chunk of progressive log clearing.
    /// </summary>
    public static readonly SettingKey<int> ProgressiveLogClearingChunkSize = new(nameof(ProgressiveLogClearingChunkSize), 8192);
    /// <summary>
    /// Interval of progressive log clearing in milliseconds.
    /// </summary>
    public static readonly SettingKey<int> ProgressiveLogClearingInterval = new(nameof(ProgressiveLogClearingInterval), 60);


    // Constructor.
    ConfigurationKeys() 
    { }
}