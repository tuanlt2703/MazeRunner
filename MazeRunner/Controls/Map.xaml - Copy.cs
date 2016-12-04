using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media.Animation;
using MazeRunner.GA;

namespace MazeRunner.Controls
{
    public struct CharacterPos
    {
        public int RunnerX, RunnerY;
        public int ChaserX, ChaserY;
    }

    /// <summary>
    /// Interaction logic for Map.xaml
    /// </summary>
    public partial class Map : UserControl
    {
        #region Properties
        private readonly ImageBrush Map10x10 = new ImageBrush(new BitmapImage(new Uri(@"pack://application:,,,/MazeRunner;component/Images/Floor.png")));
        private readonly string BrickPath = @"pack://application:,,,/MazeRunner;component/Images/Brick.png";
        private readonly int MaxNumberOfObstacles = 40;

        public MainWindow Main;
        public int[,] MapMatrix;
        private Runner runner;
        private Chaser chaser;
        public bool MapLoaded;
        public bool isMoving;

        public List<CharacterPos> LastMoves;
        public int PreviousMoveID;
        private CharacterPos Last;
        private bool isUndo;

        public struct Pos
        {
            public int X, Y;
            public double f, g, h;
            public int xpre, ypre;
            public Pos(int x, int y, double f, double g)
            {
                this.X = x;
                this.Y = y;
                this.f = f;
                this.g = g;
                this.h = 0;
                this.xpre = 0;
                this.ypre = 0;
            }
        }
        public Pos Mummy, xrunner, Goal;

        //animate
        private Storyboard sb = new Storyboard();
        #endregion

        public Map()
        {
            InitializeComponent();

            this.Background = Map10x10;
            this.LastMoves = new List<CharacterPos>(3);
        }

        #region Methods
        #region Maps
        private void AddObstacle(double x, double y)
        {
            var aBrick = new Image();
            aBrick.Source = new BitmapImage(new Uri(BrickPath));
            aBrick.Margin = new Thickness(y, x, 0, 0);
            aBrick.HorizontalAlignment = HorizontalAlignment.Left;
            aBrick.VerticalAlignment = VerticalAlignment.Top;
            aBrick.Height = 45;
            aBrick.Width = 45;

            gdMap.Children.Add(aBrick);
        }

        private void AddRunner(int i, int j, double x, double y)
        {
            var start = new Grid();
            start.Background = Brushes.Red;
            start.Margin = new Thickness(y, x, 0, 0);
            start.HorizontalAlignment = HorizontalAlignment.Left;
            start.VerticalAlignment = VerticalAlignment.Top;
            start.Height = 45;
            start.Width = 45;

            runner = new Runner(i, j);
            runner.Margin = new Thickness(y + 3, x, 0, 0);

            gdMap.Children.Add(start);
            gdMap.Children.Add(runner);
        }

        private void AddGoal(double x, double y)
        {
            var goal = new Grid();
            goal.Background = Brushes.CornflowerBlue;
            goal.Margin = new Thickness(y, x, 0, 0);
            goal.HorizontalAlignment = HorizontalAlignment.Left;
            goal.VerticalAlignment = VerticalAlignment.Top;
            goal.Height = 45;
            goal.Width = 45;

            var text = new TextBlock();
            text.FontSize = 17;
            text.Text = "Goal";
            text.HorizontalAlignment = HorizontalAlignment.Center;
            text.VerticalAlignment = VerticalAlignment.Center;
            text.Margin = new Thickness(0, -2, 0, 0);
            goal.Children.Add(text);

            gdMap.Children.Add(goal);
        }

        private void AddChaser(int i, int j, double x, double y)
        {
            chaser = new Chaser(i, j);
            chaser.Margin = new Thickness(y, x, 0, 0);

            gdMap.Children.Add(chaser);
        }

        private void DrawMap()
        {
            //reset
            LastMoves.Clear();
            isUndo = false;
            PreviousMoveID = -1;

            //draw map
            gdMap.Children.Clear();
            for (int i = 0; i < MapMatrix.GetLength(0); i++)
            {
                for (int j = 0; j < MapMatrix.GetLength(1); j++)
                {
                    if (MapMatrix[i, j] == 1)
                    {
                        AddObstacle((i + 1) * 45, (j + 1) * 45);
                    }
                    else if (MapMatrix[i, j] == 2)
                    {
                        AddRunner(i, j, (i + 1) * 45, (j + 1) * 45);
                    }
                    else if (MapMatrix[i, j] == 3)
                    {
                        AddGoal((i + 1) * 45, (j + 1) * 45);
                    }
                    else if (MapMatrix[i, j] == 4)
                    {
                        AddChaser(i, j, (i + 1) * 45, (j + 1) * 45);
                    }
                }
            }

            var maxZ = gdMap.Children.OfType<UIElement>()
                .Where(x => x != runner)
                .Select(x => Panel.GetZIndex(x))
                .Max();
            Grid.SetZIndex(runner, (maxZ++) + 1);
            Grid.SetZIndex(chaser, maxZ + 1);
        }

        public void SaveStage(int level)
        {
            using (var writer = new StreamWriter(@"Maps\Stage " + level.ToString() + ".txt", false))
            {
                //write to text file
                for (int i = 0; i < MapMatrix.GetLength(0); i++)
                {
                    for (int j = 0; j < MapMatrix.GetLength(1); j++)
                    {
                        writer.Write(MapMatrix[i, j].ToString() + " ");
                    }
                    writer.WriteLine();
                }
            }
        }

        private void CreateRandomMap(int level = 0, int rows = 13, int cols = 13)
        {
            Random rnd = new Random(System.DateTime.Now.Millisecond);
            MapMatrix = new int[rows, cols];
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    MapMatrix[i, j] = -1;
                }
            }

            //random the maze            
            int x, y, CellCount = 0, ObstCount = 0;
            #region random a maze
            do
            {
                do //find a note/cell that hadn't been initialized
                {
                    x = rnd.Next(rows);
                    y = rnd.Next(cols);
                } while (MapMatrix[x, y] != -1);

                int tmp = rnd.Next(2); //[1]: obstacle - [0]: movable
                if (tmp != 1)
                {
                    MapMatrix[x, y] = tmp;
                }
                else
                {
                    if (ObstCount <= MaxNumberOfObstacles)
                    {
                        MapMatrix[x, y] = tmp;
                        ObstCount++;
                    }
                    else //MaxNumberOfObstacles reached
                    {
                        MapMatrix[x, y] = 0;
                    }
                }
                CellCount++;
            } while (CellCount < rows * cols);
            #endregion

            #region random starting, goal, chaser point
            //[2]: runner
            //random runner's starting post
            do
            {
                x = rnd.Next(rows);
                y = rnd.Next(cols);
            } while (MapMatrix[x, y] == 1);
            MapMatrix[x, y] = 2;
            xrunner = new Pos(x, y, 0, 0);

            //[3]: goal
            //random goal's post
            do
            {
                x = rnd.Next(rows);
                y = rnd.Next(cols);
            } while (MapMatrix[x, y] == 1 || MapMatrix[x, y] == 2);
            MapMatrix[x, y] = 3;
            Goal = new Pos(x, y, 0, 0);
            //[4]: Mummy
            //random chaser's starting post
            do
            {
                x = rnd.Next(rows);
                y = rnd.Next(cols);
            } while (MapMatrix[x, y] == 1 || MapMatrix[x, y] == 2 || MapMatrix[x, y] == 3);
            MapMatrix[x, y] = 4;
            Mummy = new Pos(x, y, 0, 0);
            #endregion

            SaveStage(level);
        }

        public void LoadRandomMap(int level = 0)
        {
            CreateRandomMap(level);

            DrawMap();
            MapLoaded = true;
        }

        private void ReadMapFromFile(int level = 1, int rows = 13, int cols = 13)
        {
            using (var reader = new StreamReader(@"Maps\Stage " + level.ToString() + ".txt"))
            {
                MapMatrix = new int[rows, cols];

                for (int i = 0; i < rows; i++)
                {
                    var tmp = reader.ReadLine().Split(' ');
                    for (int j = 0; j < cols; j++)
                    {
                        MapMatrix[i, j] = Int32.Parse(tmp[j]);
                    }
                }
            }
        }

        public void Load(int level = 1)
        {
            ReadMapFromFile(level);

            DrawMap();
            MapLoaded = true;
        }
        #endregion

        public bool CheckGoal()
        {
            if (MapMatrix[runner.x, runner.y] == 3)
            {
                Main.EndStage();
                return true;
            }
            else if (runner.x == chaser.x && runner.y == chaser.y)
            {
                Main.EndStage(false);
                return true;
            }
            return false;
        }

        public void Undo()
        {
            Storyboard sb_tmp = new Storyboard();
            //undo runner movement
            runner.x = LastMoves[PreviousMoveID].RunnerX;
            runner.y = LastMoves[PreviousMoveID].RunnerY;
            //runner.Margin = new Thickness((runner.y + 1) * 45 + 3, (runner.x + 1) * 45, 0, 0);
            ////animate
            ThicknessAnimation r_ta = new ThicknessAnimation(runner.Margin,
                new Thickness((runner.y + 1) * 45 + 3, (runner.x + 1) * 45, 0, 0), TimeSpan.FromMilliseconds(100));
            Storyboard.SetTargetProperty(r_ta, new PropertyPath(Runner.MarginProperty));
            sb_tmp.Children.Add(r_ta);
            sb_tmp.Begin(runner, true);

            //undo chaser movement
            chaser.x = LastMoves[PreviousMoveID].ChaserX;
            chaser.y = LastMoves[PreviousMoveID].ChaserY;
            //chaser.Margin = new Thickness((chaser.y + 1) * 45, (chaser.x + 1) * 45, 0, 0);
            ThicknessAnimation c_ta = new ThicknessAnimation(chaser.Margin,
                new Thickness((chaser.y + 1) * 45, (chaser.x + 1) * 45, 0, 0), TimeSpan.FromMilliseconds(100));
            Storyboard.SetTargetProperty(c_ta, new PropertyPath(Chaser.MarginProperty));
            sb_tmp.Children.Add(c_ta);
            sb_tmp.Begin(chaser, true);


            //mark current move
            isUndo = true;
            PreviousMoveID--;
            //Main.Control.btnNext.IsEnabled = true;
        }

        #region Chaser's Movement
        public void StartChaserTurn()
        {
            //save last chaser pos
            //initializing Last is avoidable, for its earlier's success
            Last.ChaserX = chaser.x;
            Last.ChaserY = chaser.y;
            MapMatrix[Last.RunnerX, Last.RunnerY] = 0;
            MapMatrix[runner.x, runner.y] = 2;

            int move = chaser.Asmove(MapMatrix); // Map didn't have latest pos of chaser;

            MapMatrix[Last.ChaserX, Last.ChaserY] = 0;
            MapMatrix[chaser.x, chaser.y] = 4;

            StartChaserAnimation();
        }

        ThicknessAnimation Chaser_ta = new ThicknessAnimation();
        private void StartChaserAnimation()
        {
            Chaser_ta.From = chaser.Margin;
            Chaser_ta.To = new Thickness((chaser.y + 1) * 45, (chaser.x + 1) * 45, 0, 0);
            Chaser_ta.Duration = new Duration(TimeSpan.FromMilliseconds(500));
            Chaser_ta.Completed += Chaser_ta_Completed;
            //config storyboard
            Storyboard.SetTargetProperty(Chaser_ta, new PropertyPath(Chaser.MarginProperty));
            sb.Children.Clear();
            sb.Children.Add(Chaser_ta);
            //begin animation
            sb.Begin(chaser, true);
        }
        #endregion

        #region Runner's Movements
        ThicknessAnimation Runner_ta = new ThicknessAnimation();
        private void StartRunnerAnimation()
        {
            Runner_ta.From = runner.Margin;
            Runner_ta.To = new Thickness((runner.y + 1) * 45 + 3, (runner.x + 1) * 45, 0, 0);
            Runner_ta.Duration = new Duration(TimeSpan.FromMilliseconds(500));
            Runner_ta.Completed += Runner_ta_Completed;

            //config storyboard
            Storyboard.SetTargetProperty(Runner_ta, new PropertyPath(Runner.MarginProperty));
            sb.Children.Clear();
            sb.Children.Add(Runner_ta);

            //begin animation
            isMoving = true;
            sb.Begin(runner, true);
        }

        public void MoveUp()
        {
            if (runner.x > 0)
            {
                if (MapMatrix[runner.x + -1, runner.y] != 1)
                {
                    //save runner last pos
                    Last = new CharacterPos();
                    Last.RunnerX = runner.x;
                    Last.RunnerY = runner.y;

                    runner.x -= 1;

                    StartRunnerAnimation();
                }
            }
        }

        public void MoveDown()
        {
            if (runner.x < 13)
            {
                if (MapMatrix[runner.x + 1, runner.y] != 1)
                {
                    //save runner last pos
                    Last = new CharacterPos();
                    Last.RunnerX = runner.x;
                    Last.RunnerY = runner.y;

                    runner.x += 1;

                    StartRunnerAnimation();
                }
            }
        }

        public void MoveLeft()
        {
            if (runner.y > 0)
            {
                if (MapMatrix[runner.x, runner.y - 1] != 1)
                {
                    //save runner last pos
                    Last = new CharacterPos();
                    Last.RunnerX = runner.x;
                    Last.RunnerY = runner.y;

                    runner.y -= 1;

                    StartRunnerAnimation();
                }
            }
        }

        public void MoveRight()
        {
            if (runner.y < 13)
            {
                if (MapMatrix[runner.x, runner.y + 1] != 1)
                {
                    //save runner last pos
                    Last = new CharacterPos();
                    Last.RunnerX = runner.x;
                    Last.RunnerY = runner.y;

                    runner.y += 1;

                    StartRunnerAnimation();
                }
            }
        }
        #endregion
        #endregion

        #region Events
        private void Runner_ta_Completed(object sender, EventArgs e)
        {
            Runner_ta.Completed -= Runner_ta_Completed;

            isMoving = false;
            if (!CheckGoal())
            {
                Main.EndRunnerTurn();
            }
        }

        private void Chaser_ta_Completed(object sender, EventArgs e)
        {
            Chaser_ta.Completed -= Chaser_ta_Completed;
            //Chaser_sb.Stop();
            if (!CheckGoal())
            {
                Main.EndChaserTurn();
            }

            //store last movement into list
            if (!isUndo) //completely new movement
            {
                //undo 3 times only
                if (LastMoves.Count < 3)
                {
                    LastMoves.Add(Last);
                }
                else
                {
                    LastMoves.RemoveAt(0);
                    LastMoves.Add(Last);
                }
                PreviousMoveID = LastMoves.Count - 1;
            }
            else //new movement after undo
            {
                //remove stored movement
                //calculate number of moves to remove
                int count;
                if (PreviousMoveID < 0)
                {
                    count = 1;
                }
                else
                {
                    count = LastMoves.Count - PreviousMoveID - 1;
                }
                LastMoves.RemoveRange(PreviousMoveID + 1, count);

                //re-store last move
                LastMoves.Add(Last);
                PreviousMoveID = LastMoves.Count - 1;

                //enable btnNext
                Main.Control.btnNext.IsEnabled = false;
            }
            Main.Control.btnUndo.IsEnabled = true;
        }
        #endregion      
    }
}
      
