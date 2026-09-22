namespace PaintInventory.Server.Models;

public enum ImportRowStatus
{
    New,
    Duplicate,
    Invalid
}

public enum ImportMode
{
    SkipDuplicates,
    UpdateDuplicates
}

public record ImportRowDto(
    int RowNumber,
    ImportRowStatus Status,
    string? Message,
    IReadOnlyDictionary<string, string?> Values);

public record ImportPreviewDto(
    string Entity,
    IReadOnlyList<string> Columns,
    int Total,
    int NewCount,
    int DuplicateCount,
    int InvalidCount,
    IReadOnlyList<ImportRowDto> Rows);

public record ImportResultDto(
    string Entity,
    int Inserted,
    int Updated,
    int Skipped,
    int Invalid,
    IReadOnlyList<string> Warnings);