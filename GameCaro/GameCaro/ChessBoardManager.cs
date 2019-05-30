using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GameCaro
{
    public class ChessBoardManager
    {
        #region Properties
        private Panel chessBoard;

        public Panel ChessBoard
        {
            get { return chessBoard; }
            set { chessBoard = value; }
        }
        private List<Player> player;

        public List<Player> Player
        {
            get
            { return player; }

            set
            { player = value; }
        }

        public int CurrentPlayer
        {
            get
            {
                return currentPlayer;
            }

            set
            {
                currentPlayer = value;
            }
        }

        public TextBox PlayerName
        {
            get
            {
                return playerName;
            }

            set
            {
                playerName = value;
            }
        }

        public PictureBox PlayerMark
        {
            get
            {
                return playerMark;
            }

            set
            {
                playerMark = value;
            }
        }

        public List<List<Button>> Matrix
        {
            get
            {
                return matrix;
            }

            set
            {
                matrix = value;
            }
        }

        public Stack<PlayInfo> PlayTimeLine
        {
            get
            {
                return playTimeLine;
            }

            set
            {
                playTimeLine = value;
            }
        }

        public Label PlayerScore
        {
            get
            {
                return playerScore;
            }

            set
            {
                playerScore = value;
            }
        }

        public Label OpponentScore
        {
            get
            {
                return opponentScore;
            }

            set
            {
                opponentScore = value;
            }
        }

        public Stack<PlayInfo> UndoTimeLine
        {
            get
            {
                return undoTimeLine;
            }

            set
            {
                undoTimeLine = value;
            }
        }

        private int currentPlayer;

        private TextBox playerName;

        private PictureBox playerMark;

        private List<List<Button>> matrix;

        private Label playerScore;

        private Label opponentScore;

        private event EventHandler<ButtonClickEvent> playerMarked;
        public event EventHandler<ButtonClickEvent> PlayerMarked
        {
            add
            {
                playerMarked += value;
            }
            remove
            {
                playerMarked -= value;
            }
        }

        private event EventHandler endedGame;
        public event EventHandler EndedGame
        {
            add
            {
                endedGame += value;
            }
            remove
            {
                endedGame -= value;
            }
        }
        private Stack<PlayInfo> playTimeLine;
        private Stack<PlayInfo> undoTimeLine;
        #endregion

        #region Initialize
        public ChessBoardManager(Panel chessBoard, TextBox playerName, PictureBox mark, Label playerScore, Label opponentScore)
        {
            this.ChessBoard = chessBoard;
            this.PlayerName = playerName;        
            this.playerMark = mark;
            this.PlayerScore = playerScore;
            this.OpponentScore = opponentScore;
            this.Player = new List<Player>()
            {
                new Player("Player1", Image.FromFile(Application.StartupPath + "\\Resources\\x.png"), 0),
                new Player("Player2", Image.FromFile(Application.StartupPath + "\\Resources\\o.png"), 0)
            };

            
        }
        #endregion

        #region Methods
        public void DrawChessBoard()
        {
            ChessBoard.Enabled = true;
            ChessBoard.Controls.Clear();

            playTimeLine = new Stack<PlayInfo>();

            undoTimeLine = new Stack<PlayInfo>();

            CurrentPlayer = 0;

            ChangePlayer();

            Matrix = new List<List<Button>>();

            Button oldButton = new Button() { Width = 0, Location = new Point(0, 0) };
            for (int i = 0; i < Cons.CHESS_BOARD_HEIGHT; i++)
            {
                Matrix.Add(new List<Button>());
                for (int j = 0; j < Cons.CHESS_BOARD_WIDTH; j++)
                {
                    Button btn = new Button()
                    {
                        Width = Cons.CHESS_WIDTH,
                        Height = Cons.CHESS_HEIGHT,
                        Location = new Point(oldButton.Location.X + oldButton.Width, oldButton.Location.Y),
                        BackgroundImageLayout = ImageLayout.Stretch,
                        Tag = i.ToString()
                    };

                    btn.Click += btn_Click;

                    ChessBoard.Controls.Add(btn);

                    Matrix[i].Add(btn);

                    oldButton = btn;
                }
                oldButton.Location = new Point(0, oldButton.Location.Y + Cons.CHESS_HEIGHT);
                oldButton.Width = 0;
                oldButton.Height = 0;
            }
        }

        private void btn_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;

            if (btn.BackgroundImage != null)
                return;
            Mark(btn);

            PlayTimeLine.Push(new PlayInfo(GetChessPoint(btn), CurrentPlayer));
            UndoTimeLine.Push(new PlayInfo(GetChessPoint(btn), CurrentPlayer));
            CurrentPlayer = CurrentPlayer == 1 ? 0 : 1;
            ChangePlayer();

            if (playerMarked != null)
                playerMarked(this, new ButtonClickEvent(GetChessPoint(btn)));

            if (isEndGame(btn))
            {
                CurrentPlayer = CurrentPlayer == 1 ? 0 : 1;
                ChangePlayer();
                Player[CurrentPlayer].Score++;
                PlayerScore.Text = Player[CurrentPlayer].Score.ToString();
                EndGame();
            }
        }

        public void OtherPlayerMark(Point point)
        {
            Button btn = Matrix[point.Y][point.X];

            if (btn.BackgroundImage != null)
                return;

            Mark(btn);

            PlayTimeLine.Push(new PlayInfo(GetChessPoint(btn), CurrentPlayer));

            CurrentPlayer = CurrentPlayer == 1 ? 0 : 1;

            ChangePlayer();

            if (isEndGame(btn))
            {
                CurrentPlayer = CurrentPlayer == 1 ? 0 : 1;

                ChangePlayer();
                Player[CurrentPlayer].Score++;
                OpponentScore.Text = Player[CurrentPlayer].Score.ToString();
                EndGame();
            }
        }
        public void EndGame()
        {
            if (endedGame != null)
                endedGame(this, new EventArgs());
        }

        public bool Undo()
        {
            if (PlayTimeLine.Count <= 0)
                return false;

            bool isUndo1 = UndoStep();
            bool isUndo2 = UndoStep();

            PlayInfo oldPoint = PlayTimeLine.Peek();
            CurrentPlayer = oldPoint.CurrentPlayer == 1 ? 0 : 1;

            return isUndo1 && isUndo2;
        }

        private bool UndoStep()
        {
            if (PlayTimeLine.Count <= 0)
                return false;

            PlayInfo oldPoint = PlayTimeLine.Pop();
            UndoTimeLine.Push(oldPoint);
            Button btn = Matrix[oldPoint.Point.Y][oldPoint.Point.X];

            btn.BackgroundImage = null;

            if (PlayTimeLine.Count <= 0)
            {
                CurrentPlayer = 0;
            }
            else
            {
                oldPoint = PlayTimeLine.Peek();

            }

            ChangePlayer();

            return true;
        }

        public bool Redo()
        {
            if (UndoTimeLine.Count <= 0)
                return false;

            bool isRedo1 = RedoStep();
            bool isRedo2 = RedoStep();

            PlayInfo oldPoint = PlayTimeLine.Peek();
            CurrentPlayer = oldPoint.CurrentPlayer == 1 ? 0 : 1;

            return isRedo1 && isRedo2;
        }

        private bool RedoStep()
        {
            if (UndoTimeLine.Count <= 0)
                return false;

            PlayInfo oldPoint = UndoTimeLine.Pop();

            PlayTimeLine.Push(oldPoint);
            Button btn = Matrix[oldPoint.Point.Y][oldPoint.Point.X];

            btn.BackgroundImage = Player[CurrentPlayer].Mark;

            if (UndoTimeLine.Count <= 0)
            {
                CurrentPlayer = 0;
            }
            else
            {
                oldPoint = UndoTimeLine.Peek();
            }
            CurrentPlayer = CurrentPlayer == 1 ? 0 : 1;
            ChangePlayer();

            return true;
        }

        private bool isEndGame(Button btn)
        {
            return isEndHorizontal(btn) || isEndVertical(btn) || isEndPrimary(btn) || isEndSub(btn) || isSquare1(btn) || isSquare2(btn) || isSquare3(btn) || isSquare4(btn);
        }

        private Point GetChessPoint(Button btn)
        {
            int vertical = Convert.ToInt32(btn.Tag);
            int horizontal = Matrix[vertical].IndexOf(btn);

            Point point = new Point(horizontal, vertical);

            return point;
        }
        private bool isEndHorizontal(Button btn)
        {
            Point point = GetChessPoint(btn);

            int countLeft = 0;
            for (int i = point.X; i >= 0; i--)
            {
                if (Matrix[point.Y][i].BackgroundImage == btn.BackgroundImage)
                {
                    countLeft++;
                }
                else
                    break;
            }
            int countRight = 0;
            for (int i = point.X + 1; i < Cons.CHESS_BOARD_WIDTH; i++)
            {
                if (Matrix[point.Y][i].BackgroundImage == btn.BackgroundImage)
                {
                    countRight++;
                }
                else
                    break;
            }
            return countLeft + countRight == 5;
        }

        private bool isEndVertical(Button btn)
        {
            Point point = GetChessPoint(btn);

            int countTop = 0;
            for (int i = point.Y; i >= 0; i--)
            {
                if (Matrix[i][point.X].BackgroundImage == btn.BackgroundImage)
                {
                    countTop++;
                }
                else
                    break;
            }
            int countBottom = 0;
            for (int i = point.Y + 1; i < Cons.CHESS_BOARD_HEIGHT; i++)
            {
                if (Matrix[i][point.X].BackgroundImage == btn.BackgroundImage)
                {
                    countBottom++;
                }
                else
                    break;
            }
            return countTop + countBottom == 5;
        }

        private bool isEndPrimary(Button btn)
        {
            Point point = GetChessPoint(btn);

            int countTop = 0;
            for (int i = 0; i <= point.X; i++)
            {
                if (point.X - i < 0 || point.Y - i < 0)
                    break;

                if (Matrix[point.Y - i][point.X - i].BackgroundImage == btn.BackgroundImage)
                {
                    countTop++;
                }
                else
                    break;
            }
            int countBottom = 0;
            for (int i = 1; i <= Cons.CHESS_BOARD_WIDTH - point.X; i++)
            {
                if (point.Y + i >= Cons.CHESS_BOARD_HEIGHT || point.X + i >= Cons.CHESS_BOARD_WIDTH)
                    break;

                if (Matrix[point.Y + i][point.X + i].BackgroundImage == btn.BackgroundImage)
                {
                    countBottom++;
                }
                else
                    break;
            }
            return countTop + countBottom == 5;
        }

        private bool isEndSub(Button btn)
        {
            Point point = GetChessPoint(btn);

            int countTop = 0;
            for (int i = 0; i <= point.X; i++)
            {
                if (point.X + i > Cons.CHESS_BOARD_WIDTH || point.Y - i < 0)
                    break;

                if (Matrix[point.Y - i][point.X + i].BackgroundImage == btn.BackgroundImage)
                {
                    countTop++;
                }
                else
                    break;
            }
            int countBottom = 0;
            for (int i = 1; i <= Cons.CHESS_BOARD_WIDTH - point.X; i++)
            {
                if (point.Y + i >= Cons.CHESS_BOARD_HEIGHT || point.X - i < 0)
                    break;

                if (Matrix[point.Y + i][point.X - i].BackgroundImage == btn.BackgroundImage)
                {
                    countBottom++;
                }
                else
                    break;
            }
            return countTop + countBottom == 5;
        }

        private bool isSquare1(Button btn)
        {
            Point point = GetChessPoint(btn);

            if (point.Y - 1 < 0 || point.X - 1 < 0)
                return false;

            if (Matrix[point.Y - 1][point.X].BackgroundImage == btn.BackgroundImage && Matrix[point.Y][point.X - 1].BackgroundImage == btn.BackgroundImage && Matrix[point.Y - 1][point.X - 1].BackgroundImage == btn.BackgroundImage)
                return true;
            else
                return false;          
        }

        private bool isSquare2(Button btn)
        {
            Point point = GetChessPoint(btn);

            if (point.Y - 1 < 0 || point.X + 1 > Cons.CHESS_BOARD_WIDTH)
                return false;

            if (Matrix[point.Y - 1][point.X].BackgroundImage == btn.BackgroundImage && Matrix[point.Y][point.X + 1].BackgroundImage == btn.BackgroundImage && Matrix[point.Y - 1][point.X + 1].BackgroundImage == btn.BackgroundImage)
                return true;
            else
                return false;
        }

        private bool isSquare3(Button btn)
        {
            Point point = GetChessPoint(btn);

            if (point.X + 1 > Cons.CHESS_BOARD_WIDTH || point.Y + 1 >= Cons.CHESS_BOARD_HEIGHT)
                return false;

            if (Matrix[point.Y + 1][point.X].BackgroundImage == btn.BackgroundImage && Matrix[point.Y][point.X + 1].BackgroundImage == btn.BackgroundImage && Matrix[point.Y + 1][point.X + 1].BackgroundImage == btn.BackgroundImage)
                return true;
            else
                return false;
        }

        private bool isSquare4(Button btn)
        {
            Point point = GetChessPoint(btn);

            if (point.X - 1 < 0 || point.Y + 1 >= Cons.CHESS_BOARD_HEIGHT)
                return false;

            if (Matrix[point.Y + 1][point.X].BackgroundImage == btn.BackgroundImage && Matrix[point.Y][point.X - 1].BackgroundImage == btn.BackgroundImage && Matrix[point.Y + 1][point.X - 1].BackgroundImage == btn.BackgroundImage)
                return true;
            else
                return false;
        }

        private void Mark(Button btn)
        {
            btn.BackgroundImage = Player[CurrentPlayer].Mark;


        }
        private void ChangePlayer()
        {
            PlayerName.Text = Player[CurrentPlayer].Name;

            PlayerMark.Image = Player[CurrentPlayer].Mark;
        }
        #endregion

    }

    public class ButtonClickEvent : EventArgs
    {
        private Point clickedPoint;

        public Point ClickedPoint
        {
            get
            {
                return clickedPoint;
            }

            set
            {
                clickedPoint = value;
            }
        }

        public ButtonClickEvent(Point point)
        {
            this.ClickedPoint = point;
        }
    }
}
