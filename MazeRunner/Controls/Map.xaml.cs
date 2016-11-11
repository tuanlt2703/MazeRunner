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
        private int[,] MapMatrix;
        private Runner runner;
        public bool MapLoaded;
        public bool isMoving;

        private Storyboard sb = new Storyboard();
        #endregion

        public Map()
        {
            InitializeComponent();

            this.Background = Map10x10;
        }

        #region Methods
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

        private void AddStart(int i, int j, double x, double y)
        {
            var start = new Grid();
            start.Background = Brushes.Red;
            start.Margin = new Thickness(y, x, 0, 0);
            start.HorizontalAlignment = HorizontalAlignment.Left;
            start.VerticalAlignment = VerticalAlignment.Top;
            start.Height = 45;
            start.Width = 45;

            runner = new Runner(i, j);
            runner.Margin = new Thickness(y, x, 0, 0);
                       
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

        private void DrawMap()
        {
            gdMap.Children.Clear();
            for (int i = 0; i < MapMatrix.GetLength(0); i++)
            {
                for (int j = 0; j < MapMatrix.GetLength(1); j++)
                {
                    if (MapMatrix[i, j] == 1)
                    {
                        AddObstacle((i + 1) * 45, (j + 1) * 45);
                    }
                    else if(MapMatrix[i,j] == 2)
                    {
                        AddStart(i, j, (i + 1) * 45, (j + 1) * 45);
                    }
                    else if (MapMatrix[i, j] == 3)
                    {
                        AddGoal((i + 1) * 45, (j + 1) * 45);
                    }
                }
            }

            var maxZ = gdMap.Children.OfType<UIElement>()
                .Where(x => x != runner)
                .Select(x => Panel.GetZIndex(x))
                .Max();
            Panel.SetZIndex(runner, maxZ + 1);
        }

        private void CreateRandomMap(int level = 0, int rows = 13, int cols = 13)
        {
            using (var writer = new StreamWriter(@"Maps\Stage " + level.ToString() + ".txt", false))
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

                #region random starting, goal point
                //[2]: runner
                //random runner's starting post
                do
                {
                    x = rnd.Next(rows);
                    y = rnd.Next(cols);
                } while (MapMatrix[x, y] != 1);
                MapMatrix[x, y] = 2;

                //[3]: goal
                //random chaser's starting post
                do
                {
                    x = rnd.Next(rows);
                    y = rnd.Next(cols);
                } while (MapMatrix[x, y] != 1 && MapMatrix[x, y] != 2);
                MapMatrix[x, y] = 3;
                #endregion

                //write to text file
                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < cols; j++)
                    {
                        writer.Write(MapMatrix[i, j].ToString() + " ");
                    }
                    writer.WriteLine();
                }
            }
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

        #region Game's Movements 
        private void CheckGoal()
        {
            if (MapMatrix[runner.x, runner.y] == 3)
            {
                Main.EndStage();
            }
        }

        private void StartAnimation()
        {
            //create animation
            ThicknessAnimation ta = new ThicknessAnimation();
            ta.From = runner.Margin;
            ta.To = new Thickness((runner.y + 1) * 45, (runner.x + 1) * 45, 0, 0);
            ta.Duration = new Duration(TimeSpan.FromMilliseconds(500));
            ta.Completed += Ta_Completed;
            //config storyboard
            Storyboard.SetTargetProperty(ta, new PropertyPath(Runner.MarginProperty));
            sb.Children.Add(ta);

            //begin animation
            isMoving = true;
            sb.Begin(runner, true);
        }

        private void Ta_Completed(object sender, EventArgs e)
        {
            isMoving = false;
        }

        public void MoveUp()
        {
            if (runner.x > 0)
            {
                if (MapMatrix[runner.x + -1, runner.y] != 1)
                {
                    runner.x -= 1;

                    StartAnimation();

                    CheckGoal();
                }
            }
        }

        public void MoveDown()
        {
            if (runner.x < 13)
            {
                if (MapMatrix[runner.x + 1, runner.y] != 1)
                {
                    runner.x += 1;

                    StartAnimation();

                    CheckGoal();
                }
            }
        }

        public void MoveLeft()
        {
            if (runner.y > 0)
            {
                if (MapMatrix[runner.x, runner.y - 1] != 1)
                {
                    runner.y -= 1;

                    StartAnimation();

                    CheckGoal();
                }
            }
        }

        public void MoveRight()
        {
            if (runner.y < 13)
            {
                if (MapMatrix[runner.x, runner.y + 1] != 1)
                {
                    runner.y += 1;

                    StartAnimation();

                    CheckGoal();
                }
            }
        }
        #endregion
        #endregion

        #region Events
        #endregion
    }
}
