namespace L.TicTacToe.Infrastructure.Exceptions;

public class ConcurrencyException(long expectedVersion, long actualVersion) : Exception
{
    public long ExpectedVersion => expectedVersion;
    public long ActualVersion => actualVersion;
}
