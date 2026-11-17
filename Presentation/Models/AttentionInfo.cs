namespace Presentation.Models;

public sealed record AttentionInfo(IReadOnlyList<string> Events, bool PendingDeadline);
