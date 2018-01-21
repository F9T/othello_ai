using System;
using System.Collections.Generic;
using System.Linq;

namespace OthelloAI
{
    public class TreeNode
    {
        private const double WeightMobility = 90,
                            WeightFrontier = 80,
                            WeightNumberDisc = 15,
                            WeightCorner = 800,
                            WeightScore = 15,
                            WeightBadPostion = 390;

        private List<Tuple<int, int, List<Tuple<int, int>>>> cornersAndCloseCornerPosition;

        private int[,] boardScore;

        public Tuple<int, int> Move { get; set; }

        public Board Board { get; set; }

        public bool IsRoot { get; set; }

        public TileState TileState { get; set; }

        public TreeNode()
        {
            InitializeBoard();
        }

        private void InitializeBoard()
        {
            boardScore = new[,]
            {
                {40, -10, 15, 10, 10, 15, -10, 40},
                {-10, -20, -5, 0, 0, -5, -20, -10},
                {15, -5, 3, 3, 3, 3, -5, 15},
                {10, 0, 3, 10, 10, 3, 0, 10},
                {10, 0, 3, 10, 10, 3, 0, 10},
                {15, -5, 3, 3, 3, 3, -5, 15},
                {-10, -20, -5, 0, 0, -5, -20, -10},
                {40, -10, 15, 10, 10, 15, -10, 40}
            };
            cornersAndCloseCornerPosition = new List<Tuple<int, int, List<Tuple<int, int>>>>
            {
                new Tuple<int, int, List<Tuple<int, int>>>(0, 0, new List<Tuple<int, int>>
                {
                    new Tuple<int, int>(0, 1),
                    new Tuple<int, int>(1, 1),
                    new Tuple<int, int>(1, 0),
                }),
                new Tuple<int, int, List<Tuple<int, int>>>(7, 0, new List<Tuple<int, int>>
                {
                    new Tuple<int, int>(7, 1),
                    new Tuple<int, int>(6, 1),
                    new Tuple<int, int>(6, 0),
                }),
                new Tuple<int, int, List<Tuple<int, int>>>(0, 7, new List<Tuple<int, int>>
                {
                    new Tuple<int, int>(0, 6),
                    new Tuple<int, int>(1, 6),
                    new Tuple<int, int>(1, 7),
                }),
                new Tuple<int, int, List<Tuple<int, int>>>(7, 7, new List<Tuple<int, int>>
                {
                    new Tuple<int, int>(6, 7),
                    new Tuple<int, int>(6, 6),
                    new Tuple<int, int>(7, 6),
                }),
            };
        }

        private int CountFrontier(TileState _tileState)
        {
            int[,] tmpBoard = Board.GetBoard();
            int count = 0;
            for (int i = 0; i < tmpBoard.GetLength(0); i++)
            {
                for (int j = 0; j < tmpBoard.GetLength(1); j++)
                {
                    if (tmpBoard[i, j] == (int) _tileState)
                    {
                        count += CountFrontierDisk(i, j);
                    }
                }
            }
            return count;
        }

        private int CountFrontierDisk(int _line, int _column)
        {
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1 ; j++)
                {
                    int l = _line + i;
                    int c = _column + j;
                    if (l >= 0 && c >= 0 && l < Board.NumberCase && c < Board.NumberCase && (l != _line || c != _column))
                    {
                        if (Board.GetBoard()[l, c] == (int)TileState.Empty)
                        {
                            return 1;
                        }
                    }
                }
            }
            return 0;
        }

        private int CountBadCornerPosition(TileState _tileState)
        {
            int count = 0;
            var board = Board.GetBoard();
            foreach (var corner in cornersAndCloseCornerPosition)
            {
                if (board[corner.Item1, corner.Item2] == (int) TileState)
                {
                    foreach (var closeCorner in corner.Item3)
                    {
                        if (board[closeCorner.Item1, closeCorner.Item2] == (int) TileState)
                        {
                            count++;
                        }
                    }
                }
            }
            return count;
        }

        private int CountCorners(TileState _tileState)
        {
            return cornersAndCloseCornerPosition.Count(_corner => Board.GetBoard()[_corner.Item1, _corner.Item2] == (int) _tileState);
        }

        private int Score(TileState _tileState)
        {
            int score = 0;
            var board = Board.GetBoard();
            for (int i = 0; i < boardScore.GetLength(0); i++)
            {
                for (int j = 0; j < boardScore.GetLength(1); j++)
                {
                    if (board[i, j] == (int) _tileState)
                    {
                        score += boardScore[i, j];
                    }
                }
            }
            return score;
        }

        public double Evaluation()
        {
            if (Move == null)
            {
                return 0;
            }
            var enemyTileState = TileState.White;
            if (TileState == TileState.White)
            {
                enemyTileState = TileState.Black;
            }
            //Calculate score
            int myScore = Score(TileState);
            int enemyScore = Score(enemyTileState);
            int scoreValue = myScore - enemyScore;

            //Count disc
            int myNumberDisc = TileState == TileState.White ? Board.GetWhiteScore() : Board.GetBlackScore();
            int enemyNumerDisc = enemyTileState == TileState.White ? Board.GetWhiteScore() : Board.GetBlackScore();
            double discValue = 0.0;
            if (myNumberDisc + enemyNumerDisc != 0) //Check division by zero
            {
                discValue = (100.0 * myNumberDisc) / (myNumberDisc + enemyNumerDisc);
                //Negative
                if (myNumberDisc < enemyNumerDisc)
                {
                    discValue *= -1;
                }
            }

            //Frontier
            int myFrontierDisc = CountFrontier(TileState);
            int enemyFrontierDisc = CountFrontier(enemyTileState);
            double frontierValue = 0.0;
            if (myFrontierDisc + enemyFrontierDisc != 0) //Check division by zero
            {
                frontierValue = (100.0 * myFrontierDisc) / (myFrontierDisc + enemyFrontierDisc);
                //Negative if enemy has less frontier disc
                if (myFrontierDisc > enemyFrontierDisc)
                {
                    frontierValue *= -1;
                }
            }

            //Mobility
            int myMobility = GetPossibleMove(TileState == TileState.White).Count;
            int enemyMobility = GetPossibleMove(enemyTileState == TileState.White).Count;
            double mobilityValue = 0.0;
            if (myMobility + enemyMobility != 0) //Check division by zero
            {
                mobilityValue = (100.0 * myMobility) / (myMobility + enemyMobility);
                //Negative
                if (myMobility < enemyMobility)
                {
                    mobilityValue *= -1;
                }
            }

            //Corner 
            int myNumberCorner = CountCorners(TileState);
            int enemyNumberCorner = CountCorners(enemyTileState);
            double cornerValue = myNumberCorner - enemyNumberCorner;

            //Close corner
            int myNumberCloseCornerPosition = CountBadCornerPosition(TileState);
            int enemyNumberCloseCornerPosition = CountBadCornerPosition(enemyTileState);
            double closeCornerValue = -12.5 * (myNumberCloseCornerPosition - enemyNumberCloseCornerPosition);

            return (WeightFrontier * frontierValue) + (WeightMobility * mobilityValue) + (WeightBadPostion * closeCornerValue) +
                   (WeightNumberDisc * discValue) + (WeightScore * scoreValue) + (WeightCorner * cornerValue);
        }

        public bool IsFinal()
        {
            return GetPossibleMove(TileState == TileState.White).Count == 0;
        }

        public List<TreeNode> GetOperators()
        {
            var possibleMoves = GetPossibleMove(TileState == TileState.White);
            return possibleMoves.Select(_possibleMove => new TreeNode
            {
                    IsRoot = false,
                    TileState = TileState,
                    Move = _possibleMove,
                    Board = Board
            }).ToList();
        }

        public TreeNode Apply(TreeNode _nodeOp)
        {
            //Copy
            var node = new TreeNode
            {
                IsRoot = false,
                TileState = _nodeOp.TileState,
                Move = new Tuple<int, int>(_nodeOp.Move.Item1, _nodeOp.Move.Item2),
                Board = new Board(_nodeOp.Board.GetBoard())
            };
            node.Board.PlayMove(_nodeOp.Move.Item1, _nodeOp.Move.Item2, TileState == TileState.White);
            return node;
        }

        private List<Tuple<int, int>> GetPossibleMove(bool _whiteTurn)
        {
            var possibleMove = new List<Tuple<int, int>>();
            for (int x = 0; x < Board.NumberCase; x++)
            {
                for (int y = 0; y < Board.NumberCase; y++)
                {
                    if (Board.IsPlayable(y, x, _whiteTurn))
                    {
                        possibleMove.Add(new Tuple<int, int>(y, x));
                    }
                }
            }
            return possibleMove;
        }
    }
}
