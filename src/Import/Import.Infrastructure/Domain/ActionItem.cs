﻿using Import.Infrastructure.Abstractions;
using static Import.Infrastructure.Configuration.Constants;

namespace Import.Infrastructure.Domain;

public class ActionItem : IEquatable<ActionItem?>
{
    public ActionItem(
        int priority,
        string actionName,
        string targetName,
        string? targetScope,
        string? targetDataType,
        string? exchangeCode,
        ImportCycle? cycle,
        string mode = Modes.Economy)
    {
        GlobalId = Guid.NewGuid();
        Priority = priority;
        ActionName = actionName;
        TargetName = targetName;
        TargetScope = targetScope;
        TargetDataType = targetDataType;
        Status = ImportActionStatus.NotStarted;
        ExchangeCode = exchangeCode;
        Cycle = cycle;
        Mode = string.IsNullOrWhiteSpace(mode) ? Modes.Economy : mode;
    }

    public Guid GlobalId { get; }

    public int Priority { get; }

    public ImportCycle? Cycle { get; internal set; }

    public string Mode { get; }

    public string? ExchangeCode { get; }

    public string ActionName { get; }

    public string TargetName { get; }

    public string? TargetScope { get; }

    internal int? TargetScopeValue => TargetScope switch
    {
        DataTypeScopes.Full => 0,
        DataTypeScopes.TryBulkThenFull => 1,
        DataTypeScopes.Bulk => 2,
        _ => null
    };

    public string? TargetDataType { get; }

    /// <summary>
    /// This is the order in which the actions will be sorted.
    /// </summary>
    internal int? TargetDataTypeSortValue => TargetDataType switch
    {
        DataTypes.Symbols => 0,
        DataTypes.Prices => 1,
        DataTypes.Options => 2,
        DataTypes.Dividends => 3,
        DataTypes.Splits => 4,
        DataTypes.Fundamentals => 5,
        _ => null
    };

    public ImportActionStatus Status { get; internal set; }

    public DateTime UtcCreated { get; }

    public DateTime? UtcStarted { get; internal set; }

    public DateTime? UtcCompleted { get; internal set; }

    public string? Details { get; internal set; }

    public int? EstimatedCost { get; internal set; }

    public Exception? Exception { get; internal set; }

    public void Start()
    {
        UtcStarted = DateTime.UtcNow;
        Status = ImportActionStatus.InProgress;
        Exception = null;
    }

    public void Complete()
    {
        UtcCompleted = DateTime.UtcNow;
        Status = ImportActionStatus.Completed;
        Exception = null;
    }

    public void Error(Exception? exception = null)
    {
        UtcCompleted = DateTime.UtcNow;
        Status = ImportActionStatus.Error;
        Exception = exception;
    }

    public void Quit(ImportActionStatus status = ImportActionStatus.UsageRequirementMet)
    {
        UtcCompleted = DateTime.UtcNow;
        Status = status;
        Exception = null;
    }

    public override string ToString()
    {
        string targetScope = TargetScope ?? "";
        if (DateTime.TryParse(targetScope, out DateTime dt))
        {
            targetScope = dt.ToString("yyyy-MM-dd");
        }

        List<string> elements = new()
        {
            ActionName.PadRight(8),
            targetScope.PadRight(10),
            TargetName.PadRight(11),
            TargetDataType ?? ""
        };

        elements.RemoveAll(string.IsNullOrWhiteSpace);

        return string.Join(' ', elements).Trim();
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as ActionItem);
    }

    public bool Equals(ActionItem? other)
    {
        return other is not null &&
               GlobalId.Equals(other.GlobalId);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(GlobalId);
    }
}