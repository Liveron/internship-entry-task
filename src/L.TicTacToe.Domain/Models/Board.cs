using L.TicTacToe.Domain.Exceptions;
using System.Collections.ObjectModel;

namespace L.TicTacToe.Domain.Models;

public sealed class Board
{
    private readonly Mark[][] _cells;

    public IReadOnlyCollection<IReadOnlyCollection<Mark>> Cells => _cells.ToReadOnly();
    public int EmptyCells { get; private set; }
    public int BoardSize { get; private set; }
    public bool IsFull => EmptyCells == 0;

    public Board(int size)
    {
        if (size < 3)
            throw new DomainException($"Размер поля должен быть >= 3. Заданный размер: {size}");

        BoardSize = size;
        EmptyCells = size * size;

        _cells = InitializeCells();
    }



    private Mark[][] InitializeCells()
    {
        var cells = new Mark[BoardSize][];
        for (int row = 0; row < BoardSize; row++)
        {
            cells[row] = new Mark[BoardSize];
        }
        return cells;
    }

    public Mark GetCell(int row, int column) => _cells[row][column];

    public void SetCellX(int row, int column)
    {
        ValidateSet(row, column);
        _cells[row][column] = Mark.X;
        EmptyCells--;
    }

    public void SetCellO(int row, int column)
    {
        ValidateSet(row, column);
        _cells[row][column] = Mark.O;
        EmptyCells--;
    }

    private void ValidateSet(int row, int column)
    {
        if (row < 0 || row >= BoardSize || column < 0 || column >= BoardSize)
            throw new DomainException($"Недопустимые координаты хода: ({row}, {column}). Размер поля: {BoardSize}x{BoardSize}.");

        if (_cells[row][column] != Mark.Empty)
            throw new DomainException($"Клетка ({row}, {column}) уже занята. Выберите другую клетку.");
    }
}

public enum Mark { Empty, X, O };

public static class JaggedArrayExtensions
{
    public static ReadOnlyCollection<ReadOnlyCollection<T>> ToReadOnly<T>(this T[][] marks)
    {
        return new ReadOnlyCollection<ReadOnlyCollection<T>>(
            [.. marks.Select(subArr => subArr.AsReadOnly())]);
    }
}
