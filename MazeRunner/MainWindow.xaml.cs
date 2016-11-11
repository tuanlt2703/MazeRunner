using System;
using System.Collections.Generic;
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

namespace MazeRunner
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Properties
        private bool GameStarted;
        #endregion

        public MainWindow()
        {
            InitializeComponent();

            this.GameStarted = false;

            this.Control.Main = this;
            this.Map.Main = this;
        }

        #region Methods
        public void StartGame()
        {
            GameStarted = true;
        }
        #endregion

        #region Events
        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (Map.MapLoaded && !Map.isMoving)
            {
                if (e.Key == Key.A || e.Key == Key.Left)
                {
                    if (!GameStarted)
                    {
                        StartGame();
                    }

                    Map.MoveLeft();
                }
                else if (e.Key == Key.D || e.Key == Key.Right)
                {
                    if (!GameStarted)
                    {
                        StartGame();
                    }
                    Map.MoveRight();
                }
                else if (e.Key == Key.W || e.Key == Key.Up)
                {
                    if (!GameStarted)
                    {
                        StartGame();
                    }

                    Map.MoveUp();
                }
                else if (e.Key == Key.S || e.Key == Key.Down)
                {
                    if (!GameStarted)
                    {
                        StartGame();
                    }

                    Map.MoveDown();
                }
            }
        }

        public void EndStage()
        {
            MessageBox.Show("You Won");
            Map.MapLoaded = false;
        }
        #endregion
    }
}
