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
using System.Windows.Threading;
using MazeRunner;
namespace MazeRunner.Controls
{
    /// <summary>
    /// Interaction logic for GameControl.xaml
    /// </summary>
    public partial class GameControl : UserControl
    {
        public MainWindow Main;
        private DispatcherTimer Timer;
        private DateTime TimerStart;
        private int CurrentStageID = -1;
        private MazeRunner.Evoluion ga;
        public GameControl()
        {
            InitializeComponent();

            Timer = new DispatcherTimer();
            Timer.Interval = new TimeSpan(0, 0, 1);
            Timer.Tick += Timer_Tick;

            LoadStageList();
        }        

        #region Methods
        private void LoadStageList()
        {
            var Files = Directory.GetFiles(@"Maps");
            for (int i = 0; i < Files.Length; i++)
            {
                cmbStages.Items.Add(i);
            }
        }

        public void StartGame()
        {
            TimerStart = DateTime.Now;
            Timer.Start();
            btnReset.IsEnabled = true;
        }

        public void EndGame()
        {
            Timer.Stop();
        }
        #endregion

        #region Events
        private void Timer_Tick(object sender, EventArgs e)
        {
            var totalSeconds = DateTime.Now - TimerStart;

            tbTime.Text = totalSeconds.ToString(@"mm\:ss");
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            Main.StartGame();
        }

        private void btnRandom_Click(object sender, RoutedEventArgs e)
        {
            Main.Map.LoadRandomMap();
            Main.UserSelectOtherStage();
            Timer.Stop();
            tbTime.Text = "00:00";
            cmbStages.SelectedIndex = 0;
            btnSave.IsEnabled = true;

            btnUndo.IsEnabled = false;
            btnNext.IsEnabled = false;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if(MessageBox.Show("Do you want to save this maze as stage " + cmbStages.Items.Count.ToString(),"Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                Main.Map.SaveStage(cmbStages.Items.Count);
                cmbStages.SelectedIndex = cmbStages.Items.Count;
                cmbStages.Items.Add(cmbStages.Items.Count);
                btnSave.IsEnabled = false;
            }
        }

        private void cmbStages_DropDownClosed(object sender, EventArgs e)
        {
            if (CurrentStageID != cmbStages.SelectedIndex)
            {
                Main.Map.Load(cmbStages.SelectedIndex);
                Main.UserSelectOtherStage();
                Timer.Stop();
                tbTime.Text = "00:00";

                CurrentStageID = cmbStages.SelectedIndex;
                btnUndo.IsEnabled = false;
                btnNext.IsEnabled = false;

                if (CurrentStageID > 0)
                {
                    btnTrain.IsEnabled = true;
                }
            }
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            Main.Map.Load(cmbStages.SelectedIndex);
            btnUndo.IsEnabled = false;
            btnNext.IsEnabled = false;
        }

        private void btnUndo_Click(object sender, RoutedEventArgs e)
        {
            Main.Map.Undo();

            if (Main.Map.PreviousMoveID < 0)
            {
                btnUndo.IsEnabled = false;
            }
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            Main.Map.Next();

            if(Main.Map.PreviousMoveID > Main.Map.LastMoves.Count)
            {
                btnNext.IsEnabled = false;
            }
        }
        #endregion

        private void btnTrain_Click(object sender, RoutedEventArgs e)
        {
            ga = new Evoluion();
            this.ga.Mainflow(10, Main.Map.MapMatrix, 100);
            double x = this.ga.Best.fitness;
            MessageBox.Show(x.ToString()+" ........................... ");
        }
    }
}
