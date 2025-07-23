namespace L.TicTacToe.API.Options;

public sealed class GameOptions
{
    public const string SectionKey = "GameOptions";
    public int TableSize { get; init; }
    public int WinLength { get; init; }
}
