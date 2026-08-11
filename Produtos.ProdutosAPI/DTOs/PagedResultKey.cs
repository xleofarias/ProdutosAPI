public record PagedResultKey<T>(
    IReadOnlyList<T> Items,
    int? NextCursor,
    bool HasNextPage);