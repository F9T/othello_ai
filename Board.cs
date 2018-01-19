using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace OthelloAI
{
    [Serializable]
    public class Board : IPlayable.IPlayable
    {
        public static int NumberCase = 8;
        private int blackScore, whiteScore;
        private int[,] board;

        public Board()
        {
            board = new int[NumberCase, NumberCase];
            Reset();
        }

        public Board(int[,] _copyBoard)
        {
            board = new int[NumberCase, NumberCase];
            for (int i = 0; i < _copyBoard.GetLength(0); i++)
            {
                for (int j = 0; j < _copyBoard.GetLength(1); j++)
                {
                    board[j, i] = _copyBoard[j, i];
                }
            }
            CalculateScore();
        }

        private void Reset()
        {
            blackScore = 0;
            whiteScore = 0;
            for (int i = 0; i < NumberCase; ++i)
            {
                for (int j = 0; j < NumberCase; ++j)
                {
                    board[i, j] = (int)TileState.Empty;
                }
            }
            int index = NumberCase / 2;
            board[index - 1, index - 1] = board[index, index] = (int)TileState.White;
            board[index - 1, index] = board[index, index - 1] = (int)TileState.Black;
            blackScore = whiteScore = 2;
        }

        public string GetName()
        {
            return "Ombang_Lovis";
        }

        public bool IsPlayable(int _column, int _line, bool _isWhite)
        {
            var color = _isWhite ? TileState.White : TileState.Black;
            var otherColor = _isWhite ? TileState.Black : TileState.White;
            if (board[_column, _line] == (int)TileState.Empty)
            {
                if (CheckIsPlayable(_column, _line, color, otherColor))
                {
                    return true;
                }
            }
            return false;
        }

        private bool CheckIsPlayable(int _column, int _line, TileState _color, TileState _otherColor)
        {
            return IsPlayableDirection(_column, _line, _color, _otherColor, LegalDirection.North) ||
                   IsPlayableDirection(_column, _line, _color, _otherColor, LegalDirection.South) ||
                   IsPlayableDirection(_column, _line, _color, _otherColor, LegalDirection.East) ||
                   IsPlayableDirection(_column, _line, _color, _otherColor, LegalDirection.West) ||
                   IsPlayableDirection(_column, _line, _color, _otherColor, LegalDirection.NorthEast) ||
                   IsPlayableDirection(_column, _line, _color, _otherColor, LegalDirection.SouthEast) ||
                   IsPlayableDirection(_column, _line, _color, _otherColor, LegalDirection.NorthWest) ||
                   IsPlayableDirection(_column, _line, _color, _otherColor, LegalDirection.SouthWest);
        }


        private bool IsPlayableDirection(int _column, int _line, TileState _player, TileState _otherPlayer, LegalDirection _direction, bool _turn = false)
        {
            int i = _line, j = _column;
            bool findPlayer = false;
            var turnPawnList = new List<Tuple<int, int>>();
            while (true)
            {
                if (_direction == LegalDirection.North)
                {
                    j--;
                }
                else if (_direction == LegalDirection.South)
                {
                    j++;
                }
                else if (_direction == LegalDirection.East)
                {
                    i++;
                }
                else if (_direction == LegalDirection.West)
                {
                    i--;
                }
                else if (_direction == LegalDirection.NorthWest)
                {
                    i--;
                    j--;
                }
                else if (_direction == LegalDirection.NorthEast)
                {
                    i++;
                    j--;
                }
                else if (_direction == LegalDirection.SouthWest)
                {
                    i--;
                    j++;
                }
                else if (_direction == LegalDirection.SouthEast)
                {
                    i++;
                    j++;
                }
                if (i < 0 || j < 0 || i >= NumberCase || j >= NumberCase)
                {
                    break;
                }
                int state = board[j, i];
                if (state == (int)_otherPlayer)
                {
                    turnPawnList.Add(new Tuple<int, int>(j, i));
                }
                else if (state == (int)_player)
                {
                    findPlayer = true;
                    break;
                }
                else if (state == (int)TileState.Empty)
                {
                    break;
                }
            }
            if (findPlayer && turnPawnList.Count > 0)
            {
                if (_turn)
                {
                    foreach (var tuple in turnPawnList)
                    {
                        Turn(tuple.Item1, tuple.Item2, (int)_player);
                    }
                }
                return true;
            }
            return false;
        }

        private void Turn(int _column, int _line, int _player)
        {
            board[_column, _line] = _player;
        }

        public bool PlayMove(int _column, int _line, bool _isWhite)
        {
            var color = _isWhite ? TileState.White : TileState.Black;
            var otherColor = _isWhite ? TileState.Black : TileState.White;
            Turn(_column, _line, (int)color);
            IsPlayableDirection(_column, _line, color, otherColor, LegalDirection.North, true);
            IsPlayableDirection(_column, _line, color, otherColor, LegalDirection.South, true);
            IsPlayableDirection(_column, _line, color, otherColor, LegalDirection.East, true);
            IsPlayableDirection(_column, _line, color, otherColor, LegalDirection.West, true);
            IsPlayableDirection(_column, _line, color, otherColor, LegalDirection.NorthWest, true);
            IsPlayableDirection(_column, _line, color, otherColor, LegalDirection.NorthEast, true);
            IsPlayableDirection(_column, _line, color, otherColor, LegalDirection.SouthWest, true);
            IsPlayableDirection(_column, _line, color, otherColor, LegalDirection.SouthEast, true);
            CalculateScore();
            return _isWhite;
        }

        public Tuple<int, int> GetNextMove(int[,] _game, int _level, bool _whiteTurn)
        {
            var root = new TreeNode
            {
                Board = this,
                TileState = _whiteTurn ? TileState.White : TileState.Black,
                IsRoot = true
            };
            var tuple = AlphaBeta(root, _level, 1, int.MaxValue);
            if (tuple.Item2 != null)
            {
                var choiceNode = tuple.Item2;
                return new Tuple<int, int>(choiceNode.Move.Item1, choiceNode.Move.Item2);
            }
            return new Tuple<int, int>(-1, -1);
        }

        private void CalculateScore()
        {
            blackScore = 0;
            whiteScore = 0;
            for (int i = 0; i < board.GetLength(0); i++)
            {
                for (int j = 0; j < board.GetLength(1); j++)
                {
                    if (board[j, i] == (int)TileState.White)
                    {
                        whiteScore++;
                    }
                    else if (board[j, i] == (int)TileState.Black)
                    {
                        blackScore++;
                    }
                }
            }
        }

        public Tuple<double, TreeNode> AlphaBeta(TreeNode _root, int _depth, int _minOrMax, double _parentValue)
        {
            if (_depth == 0 || _root.IsFinal())
                return new Tuple<double, TreeNode>(_root.Evaluation(), null);
            double optVal = _minOrMax * -int.MaxValue;
            TreeNode optOp = null;
            foreach (var op in _root.GetOperators())
            {
                var newNode = _root.Apply(op);
                var valDummy = AlphaBeta(newNode, _depth - 1, -_minOrMax, optVal);
                if (valDummy.Item1 * _minOrMax > optVal * _minOrMax)
                {
                    optVal = valDummy.Item1;
                    optOp = op;
                    if (optVal * _minOrMax > _parentValue * _minOrMax)
                        break;
                }
            }
            return new Tuple<double, TreeNode>(optVal, optOp);
        }

        public int[,] GetBoard()
        {
            return board;
        }

        public int GetWhiteScore()
        {
            return whiteScore;
        }

        public int GetBlackScore()
        {
            return blackScore;
        }
    }
}
