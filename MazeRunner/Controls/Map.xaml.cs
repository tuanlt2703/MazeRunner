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

namespace MazeRunner.Controls
{
    public enum Cell : int
    {
        Moveable = 0, Obstacle = 1, Runner = 2, Goal = 3, Chaser = 4
    }

    public struct CharacterPos
    {
        public int RunnerX, RunnerY;
        public int ChaserX, ChaserY;

        public bool GotCaught
        {
            get
            {
                return (RunnerX == ChaserX && RunnerY == ChaserY);
            }
        }
    }

    public struct Position
    {
        public int X, Y;
        public double f, g, h;
        public int xpre, ypre;
        public Position(int x, int y, double f, double g)
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

    /// <summary>
    /// Interaction logic for Map.xaml
    /// </summary>
    public partial class Map : UserControl
    {
        private readonly ImageBrush Map10x10 = new ImageBrush(new BitmapImage(new Uri(@"pack://application:,,,/MazeRunner;component/Images/Floor.png")));

        #region Propeties
        private readonly string BrickPath = @"pack://application:,,,/MazeRunner;component/Images/Brick.png";
        private double CellSize;

        public MainWindow Main;
        public int[,] MapMatrix;
        private int MaxNumberOfObstacles;
        public bool MapLoaded, isMoving;

        private Runner runner;
        private Chaser chaser;

        //Undo controls
        public List<CharacterPos> LastMoves;
        private CharacterPos Last;
        public int PreviousMoveID;
        private bool isUndo;

        //animate
        private Storyboard sb = new Storyboard();
        #endregion

        public Map()
        {
            InitializeComponent();

            this.LastMoves = new List<CharacterPos>();
        }

        #region Methods
        #region Map
        private void AddObstacle(double x, double y)
        {
            var aBrick = new Image();
            aBrick.Source = new BitmapImage(new Uri(BrickPath));
            aBrick.Margin = new Thickness(y, x, 0, 0);
            aBrick.HorizontalAlignment = HorizontalAlignment.Left;
            aBrick.VerticalAlignment = VerticalAlignment.Top;
            aBrick.Height = CellSize;
            aBrick.Width = CellSize;

            gdMap.Children.Add(aBrick);
        }

        private void AddRunner(int i, int j, double x, double y)
        {
            var start = new Grid();
            start.Background = Brushes.Red;
            start.Margin = new Thickness(y, x, 0, 0);
            start.HorizontalAlignment = HorizontalAlignment.Left;
            start.VerticalAlignment = VerticalAlignment.Top;
            start.Height = CellSize;
            start.Width = CellSize;
            gdMap.Children.Add(start);

            runner = new Runner(i, j, CellSize, CellSize);
            runner.Margin = new Thickness(y + 4, x, 0, 0);            
            gdMap.Children.Add(runner);
        }

        private void AddGoal(double x, double y)
        {
            var goal = new Grid();
            goal.Background = Brushes.CornflowerBlue;
            goal.Margin = new Thickness(y, x, 0, 0);
            goal.HorizontalAlignment = HorizontalAlignment.Left;
            goal.VerticalAlignment = VerticalAlignment.Top;
            goal.Height = CellSize;
            goal.Width = CellSize;

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
            chaser = new Chaser(i, j, CellSize, CellSize);
            chaser.Margin = new Thickness(y, x, 0, 0);

            gdMap.Children.Add(chaser);
        }

        private void DrawBoundaryAndBackGround()
        {
            #region Boundary
            //top boundary
            for (int i = 0; i < MapMatrix.GetLength(0) + 2; i++)
            {
                AddObstacle(0, i * CellSize);
            }
            //right boundary
            for (int i = 1; i < MapMatrix.GetLength(1) + 2; i++)
            {
                AddObstacle(i * CellSize, (MapMatrix.GetLength(1) + 1) * CellSize);
            }
            //bottom boundary
            for (int i = 0; i < MapMatrix.GetLength(0) + 1; i++)
            {
                AddObstacle((MapMatrix.GetLength(0) + 1) * CellSize, i * CellSize);
            }
            //left boundary
            for (int i = 1; i < MapMatrix.GetLength(1) + 1; i++)
            {
                AddObstacle(i * CellSize, 0);
            }
            #endregion
            #region Background
            bool interleave = true;
            for (int i = 0; i < MapMatrix.GetLength(0); i++)
            {
                int j;
                for (j = 0; j < MapMatrix.GetLength(1); j++)
                {
                    if (interleave)
                    {
                        var tile = new Grid();
                        tile.Background = new SolidColorBrush(Color.FromRgb(65, 219, 42));
                        tile.Margin = new Thickness((j + 1) * CellSize, (i + 1) * CellSize, 0, 0);
                        tile.HorizontalAlignment = HorizontalAlignment.Left;
                        tile.VerticalAlignment = VerticalAlignment.Top;
                        tile.Height = CellSize;
                        tile.Width = CellSize;

                        gdMap.Children.Add(tile);

                        interleave = false;
                    }
                    else
                    {
                        interleave = true;
                    }
                }
                if (j % 2 == 0)
                {
                    interleave = !interleave;
                }                
            }
            #endregion
        }

        private void DrawMap()
        {
            //reset
            LastMoves.Clear();
            isUndo = false;
            PreviousMoveID = -1;

            CellSize = gdMap.Height / (MapMatrix.GetLength(0) + 2);
            //draw map
            gdMap.Children.Clear();
            DrawBoundaryAndBackGround();
            for (int i = 0; i < MapMatrix.GetLength(0); i++)
            {
                for (int j = 0; j < MapMatrix.GetLength(1); j++)
                {
                    if (MapMatrix[i, j] == (int)Cell.Obstacle)
                    {
                        AddObstacle((i + 1) * CellSize, (j + 1) * CellSize);
                    }
                    else if (MapMatrix[i, j] == (int)Cell.Runner)
                    {
                        AddRunner(i, j, (i + 1) * CellSize, (j + 1) * CellSize);
                    }
                    else if (MapMatrix[i, j] == (int)Cell.Goal)
                    {
                        AddGoal((i + 1) * CellSize, (j + 1) * CellSize);
                    }
                    else if (MapMatrix[i, j] == (int)Cell.Chaser)
                    {
                        AddChaser(i, j, (i + 1) * CellSize, (j + 1) * CellSize);
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

        private void CreateRandomMap(int rows, int cols, int obstacles)
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

            //Init MaxNumberOfObstacles = 30% of map size
            MaxNumberOfObstacles = obstacles;

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
            MapMatrix[x, y] = (int)Cell.Runner;

            //[3]: goal
            //random goal's post
            do
            {
                x = rnd.Next(rows);
                y = rnd.Next(cols);
            } while (MapMatrix[x, y] == 1 || MapMatrix[x, y] == 2);
            MapMatrix[x, y] = (int)Cell.Goal;

            //[4]: Mummy
            //random chaser's starting post
            do
            {
                x = rnd.Next(rows);
                y = rnd.Next(cols);
            } while (MapMatrix[x, y] == 1 || MapMatrix[x, y] == 2 || MapMatrix[x, y] == 3);
            MapMatrix[x, y] = (int)Cell.Chaser;
            #endregion
        }

        public void LoadRandomMap(int rows, int cols, int obstacles)
        {
            CreateRandomMap(rows, cols, obstacles);

            DrawMap();
            MapLoaded = true;
        }

        private int[] ReadMapFromFile(int level = 1)
        {
            var MapSize = new int[2];
            using (var reader = new StreamReader(@"Maps\Stage " + level.ToString() + ".txt"))
            {
                var line = reader.ReadLine().Split(' ');
                MapMatrix = new int[MapSize[0] = Int32.Parse(line[0]), MapSize[1] = Int32.Parse(line[1])];

                for (int i = 0; i < MapMatrix.GetLength(0); i++)
                {
                    var tmp = reader.ReadLine().Split(' ');
                    for (int j = 0; j < MapMatrix.GetLength(1); j++)
                    {
                        MapMatrix[i, j] = Int32.Parse(tmp[j]);
                    }
                }
            }

            return MapSize;
        }

        public int[] Load(int level = 1)
        {
            var tmp = ReadMapFromFile(level);

            DrawMap();
            MapLoaded = true;

            return tmp;
        }

        public void SaveMap(int level)
        {
            using (var writer = new StreamWriter(@"Maps\Stage " + level.ToString() + ".txt", false))
            {
                //write to text file
                writer.WriteLine(MapMatrix.GetLength(0) + " " + MapMatrix.GetLength(1));
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
        #endregion

        public bool CheckGoal()
        {
            if (MapMatrix[runner.x, runner.y] == (int)Cell.Goal)
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
            ////animate
            ThicknessAnimation r_ta = new ThicknessAnimation(runner.Margin,
                new Thickness((runner.y + 1) * CellSize + 4, (runner.x + 1) * CellSize, 0, 0), TimeSpan.FromMilliseconds(100));
            Storyboard.SetTargetProperty(r_ta, new PropertyPath(Runner.MarginProperty));
            sb_tmp.Children.Add(r_ta);
            sb_tmp.Begin(runner, true);

            //undo chaser movement
            chaser.x = LastMoves[PreviousMoveID].ChaserX;
            chaser.y = LastMoves[PreviousMoveID].ChaserY;
            ThicknessAnimation c_ta = new ThicknessAnimation(chaser.Margin,
                new Thickness((chaser.y + 1) * CellSize, (chaser.x + 1) * CellSize, 0, 0), TimeSpan.FromMilliseconds(100));
            Storyboard.SetTargetProperty(c_ta, new PropertyPath(Chaser.MarginProperty));
            sb_tmp.Children.Add(c_ta);
            sb_tmp.Begin(chaser, true);


            //mark current move
            isUndo = true;
            PreviousMoveID--;
            //Main.Control.btnNext.IsEnabled = true;
        }

        #region Runner's Movements
        ThicknessAnimation Runner_ta = new ThicknessAnimation();
        private void StartRunnerAnimation()
        {
            Runner_ta.From = runner.Margin;
            Runner_ta.To = new Thickness((runner.y + 1) * CellSize + 3, (runner.x + 1) * CellSize, 0, 0);
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
            if (runner.x < MapMatrix.GetLength(1) - 1)
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
            if (runner.y < MapMatrix.GetLength(0) - 1)
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

        #region Chaser's Movement
        public void StartChaserTurn()
        {
            //save last chaser pos
            //initializing Last is avoidable, for its earlier's success
            Last.ChaserX = chaser.x;
            Last.ChaserY = chaser.y;
            MapMatrix[Last.RunnerX, Last.RunnerY] = (int)Cell.Moveable;
            MapMatrix[runner.x, runner.y] = (int)Cell.Runner;

            chaser.Asmove(MapMatrix);

            MapMatrix[Last.ChaserX, Last.ChaserY] = (int)Cell.Moveable;
            MapMatrix[chaser.x, chaser.y] = (int)Cell.Chaser;

            StartChaserAnimation();
        }

        ThicknessAnimation Chaser_ta = new ThicknessAnimation();
        private void StartChaserAnimation()
        {
            Chaser_ta.From = chaser.Margin;
            Chaser_ta.To = new Thickness((chaser.y + 1) * CellSize, (chaser.x + 1) * CellSize, 0, 0);
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
        #endregion

        bool isBot = false;
        public void StartBotRunnerMove()
        {
            isBot = true;
            //Movement[1] = 0; Movement[0] = 0 => Down
            //Movement[1] = 0; Movement[0] = 1 => Up
            //Movement[1] = 1; Movement[0] = 0 => Right
            //Movement[1] = 1; Movement[0] = 1 => Left

            //Get movement from A*
            List<int> Movement = Chaser.Asmove3(MapMatrix);
            //Bởi vì hàm Asmove3 của m trả về 0 -1 nên t set cứng 1 số trước để test
            Movement[0] = 0;
            Movement[1] = 0;

            if (Movement[1] == 0 && Movement[0] == 0)
            {
                MoveDown();
            }
            else if (Movement[1] == 0 && Movement[0] == 1)
            {
                MoveUp();
            }
            else if (Movement[1] == 1 && Movement[0] == 0)
            {
                MoveRight();
            }
            else if (Movement[1] == 1 && Movement[0] == 1)
            {
                MoveLeft();
            }
        }

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
                Main.EndChaserTurn(isBot);
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
      
