using CarinaStudio.Threading;
using System;
using System.ComponentModel;
using System.Threading;

namespace CarinaStudio.ULogViewer.ViewModels.Analysis;

/// <summary>
/// Analysis result of <see cref="DisplayableLog"/>.
/// </summary>
class DisplayableLogAnalysisResult : BaseApplicationObject<IULogViewerApplication>, INotifyPropertyChanged
{
    // Static fields.
    static readonly long DefaultMemorySize = (4 * IntPtr.Size) // Appliation, message, Analyzer, PropertyChanged
        + 4 // Id
        + 4 // isMessageValid
        + 4; // Type
    static int NextId = 1;


    // Fields.
    bool isMessageValid;
    string? message;


    /// <summary>
    /// Initialize new <see cref="DisplayableLogAnalysisResult"/> instance.
    /// </summary>
    /// <param name="analyzer"><see cref="IDisplayableLogAnalyzer"/> which generates this result.</param>
    /// <param name="type">Type of result.</param>
    /// <param name="log"><see cref="DisplayableLog"/> which relates to this result.</param>
    public DisplayableLogAnalysisResult(IDisplayableLogAnalyzer<DisplayableLogAnalysisResult> analyzer, DisplayableLogAnalysisResultType type, DisplayableLog? log) : base(analyzer.Application)
    {
        this.Analyzer = analyzer;
        this.Id = Interlocked.Increment(ref NextId);
        this.Log = log;
        this.Type = type;
    }


    /// <summary>
    /// Get <see cref="IDisplayableLogAnalyzer"/> which generates this result.
    /// </summary>
    public IDisplayableLogAnalyzer<DisplayableLogAnalysisResult> Analyzer { get; }


    /// <summary>
    /// Get unique ID of result.
    /// </summary>
    public int Id { get; }


    /// <summary>
    /// Invalidate and update message of result.
    /// </summary>
    public void InvalidateMessage()
    {
        this.VerifyAccess();
        if (this.isMessageValid)
        {
            var message = this.OnUpdateMessage();
            if (this.message != message)
            {
                this.message = message;
                this.OnPropertyChanged(nameof(Message));
            }
        }
    }


    /// <summary>
    /// Check whether type of result is <see cref="DisplayableLogAnalysisResultType.Error"/> or not.
    /// </summary>
    public bool IsError { get => this.Type == DisplayableLogAnalysisResultType.Error; }


    /// <summary>
    /// Check whether type of result is <see cref="DisplayableLogAnalysisResultType.Information"/> or not.
    /// </summary>
    public bool IsInformation { get => this.Type == DisplayableLogAnalysisResultType.Information; }


    /// <summary>
    /// Check whether type of result is <see cref="DisplayableLogAnalysisResultType.Warning"/> or not.
    /// </summary>
    public bool IsWarning { get => this.Type == DisplayableLogAnalysisResultType.Warning; }
    

    /// <summary>
    /// Get <see cref="DisplayableLog"/> which relates to this result.
    /// </summary>
    public DisplayableLog? Log { get; }


    /// <summary>
    /// Get memory size of the result instance in bytes.
    /// </summary>
    public virtual long MemorySize { get => DefaultMemorySize; }


    /// <summary>
    /// Get message of result.
    /// </summary>
    public string? Message
    { 
        get
        {
            if (!this.CheckAccess())
                return this.message;
            if (!this.isMessageValid)
            {
                this.message = this.OnUpdateMessage();
                this.isMessageValid = true;
            }
            return this.message;
        }
    }


    /// <summary>
    /// Raise <see cref="PropertyChanged"/> event.
    /// </summary>
    /// <param name="propertyName">Property name.</param>
    protected virtual void OnPropertyChanged(string propertyName) => 
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    

    /// <summary>
    /// Called to update message of result.
    /// </summary>
    /// <returns>Message of result.</returns>
    protected virtual string? OnUpdateMessage() => null;


    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;


    /// <inheritdoc/>
    public override string ToString() =>
        $"[{this.Type}]: {this.message}";


    /// <summary>
    /// Get type of result.
    /// </summary>
    public DisplayableLogAnalysisResultType Type { get; }
}


/// <summary>
/// Type of <see cref="DisplayableLogAnalysisResult"/>.
/// </summary>
enum DisplayableLogAnalysisResultType : uint
{
    /// <summary>
    /// Error.
    /// </summary>
    Error = 0x1,
    /// <summary>
    /// Warning.
    /// </summary>
    Warning = 0x2,
    /// <summary>
    /// Start of operation.
    /// </summary>
    OperationStart = 0x4,
    /// <summary>
    /// End of operation.
    /// </summary>
    OperationEnd = 0x8,
    /// <summary>
    /// Checkpoint.
    /// </summary>
    Checkpoint = 0x10,
    /// <summary>
    /// Time span.
    /// </summary>
    TimeSpan = 0x20,
    /// <summary>
    /// Performance.
    /// </summary>
    Performance = 0x40,
    /// <summary>
    /// Information.
    /// </summary>
    Information = 0x80,
}