using L.TicTacToe.Domain.Exceptions;
using L.TicTacToe.Domain.Models;

namespace L.TicTacToe.UnitTests;

public sealed class TableTests
{
    [Fact]
    public void Constructor_ShouldThrowException_WhenInvalidTableSize()
    {
        Assert.Throws<DomainException>(() => new Board(2));
    }

    [Theory]
    [InlineData(3)]
    [InlineData(5)]
    [InlineData(10)]
    public void Constructor_ShouldInitializeTable(int size)
    {
        // Arrange && Act
        var board = new Board(size);

        // Assert
        Assert.Equal(size, board.BoardSize);
        
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                Assert.Equal(Mark.Empty, board.GetCell(i, j));
            }
        }
    }

    [Fact]
    public void GetCell_ReturnsCorrectMark()
    {
        // Arrange
        var board = new Board(3);

        // Act
        board.SetCellX(1, 1);
        board.SetCellO(0, 0);

        // Assert
        Assert.Equal(Mark.X, board.GetCell(1, 1));
        Assert.Equal(Mark.O, board.GetCell(0, 0));
        Assert.Equal(Mark.Empty, board.GetCell(2, 2));
    }

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(0, -1)]
    [InlineData(3, 0)]
    [InlineData(0, 3)]
    public void GetCell_ThrowsException_WhenInvalidCoordinates(int row, int column)
    {
        // Arrange
        var board = new Board(3);

        // Act & Assert
        Assert.Throws<IndexOutOfRangeException>(() => board.GetCell(row, column));
    }

    [Fact]
    public void SetCellX_ThrowsException_WhenOccupiedCell()
    {
        // Arrange
        var board = new Board(3);

        // Act
        board.SetCellX(0, 0);
        board.SetCellO(1, 1);

        // Assert
        Assert.Throws<DomainException>(() => board.SetCellX(0, 0));
        Assert.Throws<DomainException>(() => board.SetCellO(1, 1));
    }

    [Fact]
    public void SetCellO_ThrowsException_WhenOccupiedCell()
    {
        // Arrange
        var board = new Board(3);

        // Act
        board.SetCellO(0, 0);
        board.SetCellX(1, 1);

        // Assert
        Assert.Throws<DomainException>(() => board.SetCellO(0, 0));
        Assert.Throws<DomainException>(() => board.SetCellO(1, 1));
    }
}
