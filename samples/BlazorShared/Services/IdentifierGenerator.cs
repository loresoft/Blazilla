namespace BlazorShared.Services;

public static class IdentifierGenerator
{
    private static int _currentId = 0;

    public static int GetNextId() => Interlocked.Increment(ref _currentId);
}
