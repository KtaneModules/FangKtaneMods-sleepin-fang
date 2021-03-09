using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TetrisSRS
{
    public enum RotationState
    {
        None = 0,
        Right = 1,
        Full180 = 2,
        Left = 3
    }

    public class TetrisBoard
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public BoardCell[][] Grid { get; private set; }

        private TextMesh _scoreBoard;
        private TextMesh _linesCounter;
        public int _score = 0;
        private int _linesCount;

        public TetrisBoard(int width, int height, GameObject[,] gridObjects, Material full, Material empty, Material error, TextMesh scoreBoard = null, TextMesh linesCounter = null, int hiddenRows = 6 )
        {
            Dictionary<Cell, Material> materialsList = new Dictionary<Cell, Material>()
            {
                { Cell.None, null },
                { Cell.BoxEmpty, empty },
                { Cell.BoxError, error },
                { Cell.BoxFull, full }
            };
            Width = width;
            Height = height + hiddenRows;
            Grid = Enumerable.Range(0, width).Select(i => Enumerable.Range(0, height + hiddenRows).Select(j => new BoardCell(Cell.None, j >= height ? null : gridObjects[i,j], materialsList)).ToArray()).ToArray();
            _scoreBoard = scoreBoard;
            _linesCounter = linesCounter;
        }

        public void Clear()
        {
            foreach (BoardCell[] cellArray in Grid)
                foreach (BoardCell cell in cellArray)
                    cell.SetQuad(Cell.None);
        }

        public void ShowAndSetLinesCount(int linesLeft)
        {
            _linesCount = linesLeft;
            UpdateLinesCounter();
        }
        public void DeductPoints(int scores)
        {
            _score -= scores;
            UpdateScore();
        }

        public static bool IsValid(TetrisBoard board, int x, int y, int[][] exceptions, int globalX, int globalY)
        {
            return x >= 0 && x < board.Width &&
                   y >= 0 && y < board.Height &&
                   (exceptions != null && exceptions.Any(arr => arr[0] + globalX == x && arr[1] + globalY == y) || board.Grid[x][y].Status != Cell.BoxFull);
        }

        public void AddScore(int score)
        {
            _score += score;
            UpdateScore();
        }

        private void UpdateScore()
        {
            _scoreBoard.text = _score.ToString();
        }

        private void UpdateLinesCounter()
        {
            _linesCounter.text = _linesCount.ToString();
        }
        private List<int> CompletedRows()
        {
            List<int> completedRows = new List<int>();
            for (int i = 0; i < Height; i++)
            {
                if (Grid.All(col => col[i].Status == Cell.BoxFull))
                    completedRows.Add(i);
            }
            switch (completedRows.Count)
            {
                case 1:
                    _score += 100;
                    break;
                case 2:
                    _score += 300;
                    break;
                case 3:
                    _score += 500;
                    break;
                case 4:
                    _score += 800;
                    break;
            }
            UpdateScore();
            _linesCount = Math.Max(_linesCount - completedRows.Count, 0);
            UpdateLinesCounter();
            return completedRows;
        }

        public void DeleteRows()
        {
            List<int> rowsToDelete = CompletedRows(); //Assumed to be sorted from lowest to highest
            if (rowsToDelete.Count == 0)
                return;
            foreach (int row in rowsToDelete)
            {
                foreach (BoardCell[] col in Grid)
                {
                    col[row].SetQuad(Cell.None);
                }
            }
            int firstRow = rowsToDelete[0];
            int secondRow = firstRow + 1;

            while (secondRow < Height)
            {
                if (rowsToDelete.Contains(secondRow))
                {
                    secondRow++;
                    continue;
                }
                ReplaceAndDeleteRow(firstRow, secondRow);
                firstRow++;
                secondRow++;
            }

        }

        private void ReplaceAndDeleteRow(int row1, int row2)
        {
            for (int i = 0; i < Width; i++)
            {
                Grid[i][row1].SetQuad(Grid[i][row2].Status);
                Grid[i][row2].SetQuad(Cell.None);
            }
        }
    }

    public enum Cell
    {
        None,
        BoxEmpty,
        BoxError,
        BoxFull
    }

    public class BoardCell
    {
        public Cell Status { get; private set; }

        private Dictionary<Cell, Material> _statusToMaterial;
        private GameObject _quad;

        public BoardCell(Cell status, GameObject quad, Dictionary<Cell, Material> statusToMaterial)
        {
            Status = status;
            _quad = quad;
            _statusToMaterial = statusToMaterial;
        }

        public void SetQuad(Cell newStatus)
        {
            Status = newStatus;
            if (_quad == null)
                return;
            if (newStatus == Cell.None)
                _quad.SetActive(false);
            else
            {
                _quad.SetActive(true);
                _quad.GetComponent<MeshRenderer>().material = _statusToMaterial[newStatus];
            }
        }
    }

    public static class DefaultTetrominoData
    {
        public static Dictionary<Tetromino, Mino[]> TetrominoStartingConfiguration = new Dictionary<Tetromino, Mino[]>()
        {
            { Tetromino.I, new Mino[] { new Mino(-1, 0), new Mino(0, 0), new Mino(1, 0), new Mino(2, 0) } },
            { Tetromino.O, new Mino[] { new Mino(0, 1), new Mino(1, 1), new Mino(1, 0), new Mino(0, 0) } },
            { Tetromino.T, new Mino[] { new Mino(-1, 0), new Mino(0, 0), new Mino(0, 1), new Mino(1, 0) } },
            { Tetromino.J, new Mino[] { new Mino(-1, 1), new Mino(-1, 0), new Mino(0, 0), new Mino(1, 0) } },
            { Tetromino.L, new Mino[] { new Mino(-1, 0), new Mino(0, 0), new Mino(1, 0), new Mino(1, 1) } },
            { Tetromino.S, new Mino[] { new Mino(-1, 0), new Mino(0, 0), new Mino(0, 1), new Mino(1, 1) } },
            { Tetromino.Z, new Mino[] { new Mino(-1, 1), new Mino(0, 1), new Mino(0, 0), new Mino(1, 0) } }
        };

        public static Dictionary<RotationState, int[][]> TetrominoOffsetsTable(Tetromino tetromino)
        {
            switch(tetromino)
            {
                case Tetromino.I:
                    return new Dictionary<RotationState, int[][]>() { { RotationState.None, new int[][] { new int[] { 0, 0 }, new int[] { -1, 0 }, new int[] { 2, 0 }, new int[] { -1, 0 }, new int[] { 2, 0 } } },
                                                                      { RotationState.Right, new int[][] { new int[] { -1, 0 }, new int[] { 0, 0 }, new int[] { 0, 0 }, new int[] { 0, 1 }, new int[] { 0, -2 } } },
                                                                      { RotationState.Full180, new int[][] { new int[] { -1, 1 }, new int[] { 1, 1 }, new int[] { -2, 1 }, new int[] { 1, 0 }, new int[] { -2, 0 } } },
                                                                      { RotationState.Left, new int[][] { new int[] { 0, 1 }, new int[] { 0, 1 }, new int[] { 0, 1 }, new int[] { 0, -1 }, new int[] { 0, 2 } } }
                    };
                case Tetromino.O:
                    return new Dictionary<RotationState, int[][]>() { { RotationState.None, new int[][] { new int[] { 0, 0 } } },
                                                                      { RotationState.Right, new int[][] { new int[] { 0, -1 } } },
                                                                      { RotationState.Full180, new int[][] { new int[] { -1, -1 } } },
                                                                      { RotationState.Left, new int[][] { new int[] { -1, 0 } } }
                    };
                default:
                    return new Dictionary<RotationState, int[][]>() { { RotationState.None, new int[][] { new int[] { 0, 0 }, new int[] { 0, 0 }, new int[] { 0, 0 }, new int[] { 0, 0 }, new int[] { 0, 0 } } },
                                                                      { RotationState.Right, new int[][] { new int[] { 0, 0 }, new int[] { 1, 0 }, new int[] { 1, -1 }, new int[] { 0, 2 }, new int[] { 1, 2 } } },
                                                                      { RotationState.Full180, new int[][] { new int[] { 0, 0 }, new int[] { 0, 0 }, new int[] { 0, 0 }, new int[] { 0, 0 }, new int[] { 0, 0 } } },
                                                                      { RotationState.Left, new int[][] { new int[] { 0, 0 }, new int[] { -1, 0 }, new int[] { -1, -1 }, new int[] { 0, 2 }, new int[] { -1, 2 } } }
                    };
            }
        }
    }

    public class TetrisPiece
    {
        public Tetromino PieceType { get; private set; }
        public int GlobalX { get; private set; }
        public int GlobalY { get; private set; }
        public bool CanSpawn { get; private set; }
        private TetrisBoard _board;
        private RotationState _rotation;
        private Mino[] _minos;
        private Dictionary<RotationState, int[][]> _offsetsTable;
        private int _lowestGlobalY;
        private int waitBeforePlace;
        public TetrisPiece(Tetromino pieceType, TetrisBoard board, int spawnX, int spawnY)
        {
            PieceType = pieceType;
            _minos = DefaultTetrominoData.TetrominoStartingConfiguration[pieceType].Select(x => (Mino) x.Clone()).ToArray();
            _rotation = RotationState.None;
            _board = board;
            _offsetsTable = DefaultTetrominoData.TetrominoOffsetsTable(pieceType);
            GlobalX = spawnX;
            GlobalY = spawnY;
            _lowestGlobalY = GlobalY;
            CheckSpawn();
            Fill(Cell.BoxFull);
        }

        private void CheckSpawn()
        {
            int[][] allOffsets = _minos.Select(mino => new[] { mino.OffsetX, mino.OffsetY }).ToArray();
            CanSpawn = allOffsets.All(offset => TetrisBoard.IsValid(_board, offset[0] + GlobalX, offset[1] + GlobalY, null, GlobalX, GlobalY));
            if (!CanSpawn)
                _board.DeductPoints(200);
        }
        private void Fill(Cell status)
        {
            int bottomY = FindBottom();
            foreach (Mino mino in _minos)
                _board.Grid[GlobalX + mino.OffsetX][bottomY + mino.OffsetY].SetQuad(status == Cell.BoxFull ? Cell.BoxEmpty : status);
            foreach (Mino mino in _minos)
                _board.Grid[GlobalX + mino.OffsetX][GlobalY + mino.OffsetY].SetQuad(status);
        }

        public void MoveHorizontal(bool right)
        {
            Fill(Cell.None);
            int[][] allOffsets = _minos.Select(mino => new[] { mino.OffsetX, mino.OffsetY }).ToArray();
            if (allOffsets.All(offset => TetrisBoard.IsValid(_board, offset[0] + GlobalX + (right ? 1 : -1), offset[1] + GlobalY, allOffsets, GlobalX, GlobalY)))
                GlobalX += right ? 1 : -1;
            Fill(Cell.BoxFull);
        }

        public bool SoftDrop()
        {
            Fill(Cell.None);
            int previousGlobalY = GlobalY;
            int[][] allOffsets = _minos.Select(mino => new[] { mino.OffsetX, mino.OffsetY }).ToArray();
            if (allOffsets.All(offset => TetrisBoard.IsValid(_board, offset[0] + GlobalX, offset[1] + GlobalY - 1, allOffsets, GlobalX, GlobalY)))
                GlobalY -= 1;
            Fill(Cell.BoxFull);
            if (GlobalY == previousGlobalY && waitBeforePlace >= 5)
            {
                _board.DeleteRows();
                waitBeforePlace = 0;
                return true;
            }
            else if (GlobalY == previousGlobalY) 
            {
                waitBeforePlace += 1;   
                return false;
            }
            else
            {
                if (GlobalY < _lowestGlobalY)
                {
                    waitBeforePlace = 0;
                    _lowestGlobalY = GlobalY;
                    _board.AddScore(1);
                }
                return false;
            }
        }

        private int FindBottom()
        {
            int globalY = GlobalY;
            int[][] allOffsets = _minos.Select(mino => new[] { mino.OffsetX, mino.OffsetY }).ToArray();
            while (globalY > 0 && allOffsets.All(offset => TetrisBoard.IsValid(_board, offset[0] + GlobalX, offset[1] + globalY - 1, allOffsets, GlobalX, globalY)))
                globalY -= 1;
            return globalY;
        }
        public void HardDrop()
        {
            Fill(Cell.None);
            GlobalY = FindBottom();
            Fill(Cell.BoxFull);
            if (GlobalY < _lowestGlobalY)
                _board.AddScore(2 * (_lowestGlobalY - GlobalY));
            _board.DeleteRows();
        }

        public void Rotate(bool clockwise)
        {
            Fill(Cell.None);
            IEnumerator[] rotateEnum = _minos.Select(mino => mino.Rotate(clockwise)).ToArray();
            foreach (IEnumerator enumerator in rotateEnum)
                enumerator.MoveNext();
            var newOffsets = rotateEnum.Select(enumerator => (int[])enumerator.Current).ToArray();
            bool success = false;
            RotationState nextState = (RotationState)(((int)_rotation + 4 + (clockwise ? 1 : -1)) % 4);
            for (int i = 0; i < _offsetsTable[RotationState.None].Length; i++)
            {
                int[] trueOffset = new int[] { _offsetsTable[_rotation][i][0] - _offsetsTable[nextState][i][0], _offsetsTable[_rotation][i][1] - _offsetsTable[nextState][i][1] };
                if (newOffsets.All(offset => TetrisBoard.IsValid(_board, offset[0] + GlobalX + trueOffset[0], offset[1] + GlobalY + trueOffset[1], new int[][] { trueOffset }, GlobalX, GlobalY)))
                {
                    success = true;
                    GlobalX += trueOffset[0];
                    GlobalY += trueOffset[1];
                    break;
                }
            }
            if (!success)
            {
                Fill(Cell.BoxFull);
                return;
            }
            if (GlobalY < _lowestGlobalY)
                _lowestGlobalY = GlobalY;
            _rotation = nextState;
            foreach (IEnumerator enumerator in rotateEnum)
                enumerator.MoveNext();
            Fill(Cell.BoxFull);
        }
    }

    public class Mino : ICloneable
    {
        public int OffsetX { get; private set; }
        public int OffsetY { get; private set; }

        public Mino(int offsetX, int offsetY)
        {
            OffsetX = offsetX;
            OffsetY = offsetY;
        }


        public object Clone()
        {
            return new Mino(OffsetX, OffsetY);
        }

        public IEnumerator Rotate(bool clockwise)
        {
            int newOffsetX, newOffsetY;
            if (clockwise)
            {
                newOffsetX = OffsetY;
                newOffsetY = -OffsetX;
            }
            else
            {
                newOffsetX = -OffsetY;
                newOffsetY = OffsetX;
            }
            yield return new int[] { newOffsetX, newOffsetY };
            OffsetX = newOffsetX;
            OffsetY = newOffsetY;
        }
    }

    public enum Tetromino
    {
        I,
        O,
        T,
        J,
        L,
        S,
        Z
    }
}
