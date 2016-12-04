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
    public struct Difficultity
    {
        public int Rows, Cols;
        public string Size { get; set; }
        double MaxObs;
        public int NumberOfObsstacles
        {
            get
            {
                return (int)((Rows * Cols) * MaxObs);
            }
        }

        public static Difficultity Easy = new Difficultity()
        { Rows = 6, Cols = 6, MaxObs = 0.15, Size = "Easy 6x6" };

        public static Difficultity Normal = new Difficultity()
        { Rows = 8, Cols = 8, MaxObs = 0.2, Size = "Normal 8x8" };

        public static Difficultity Hard = new Difficultity()
        { Rows = 10, Cols = 10, MaxObs = 0.25, Size = "Hard 10x10" };

        public static Difficultity Insane = new Difficultity()
        { Rows = 13, Cols = 13, MaxObs = 0.3, Size = "Insane 13x13" };
    }
    /// <summary>
    /// Interaction logic for GameControl.xaml
    /// </summary>
    public partial class GameControl : UserControl
    {
        public MainWindow Main;
        private DispatcherTimer Timer;
        private DateTime TimerStart;
        private int CurrentStageID = -1;
        private List<Difficultity> MapSize;
        public GameControl()
        {
            InitializeComponent();

            Timer = new DispatcherTimer();
            Timer.Interval = new TimeSpan(0, 0, 1);
            Timer.Tick += Timer_Tick;

            LoadStageList();
            LoadMaSizepList();
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

        private void LoadMaSizepList()
        {
            MapSize = new List<Difficultity>();
            MapSize.Add(Difficultity.Easy);
            MapSize.Add(Difficultity.Normal);
            MapSize.Add(Difficultity.Hard);
            MapSize.Add(Difficultity.Insane);

            cmbSize.ItemsSource = MapSize;
            cmbSize.DisplayMemberPath = "Size";
            cmbSize.SelectedIndex = 0;
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

        private void btnRandom_Click(object sender, RoutedEventArgs e)
        {
            if (cmbSize.SelectedIndex >= 0)
            {
                var SelectedMap = (Difficultity)cmbSize.SelectedItem;
                Main.Map.LoadRandomMap(SelectedMap.Rows, SelectedMap.Cols, SelectedMap.NumberOfObsstacles);
                Main.UserSelectOtherStage();
                Timer.Stop();
                tbTime.Text = "00:00";
                cmbStages.SelectedIndex = 0;
                btnSave.IsEnabled = true;

                btnUndo.IsEnabled = false;
                btnNext.IsEnabled = false;
            }
            else
            {
                MessageBox.Show("Map size can't be emtype", "Error");
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if(MessageBox.Show("Do you want to save this maze as stage " + cmbStages.Items.Count.ToString(),"Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                Main.Map.SaveMap(cmbStages.Items.Count);
                cmbStages.SelectedIndex = cmbStages.Items.Count;
                cmbStages.Items.Add(cmbStages.Items.Count);
                btnSave.IsEnabled = false;
            }
        }

        private void cmbStages_DropDownClosed(object sender, EventArgs e)
        {
            if (CurrentStageID != cmbStages.SelectedIndex)
            {
                var tmp = Main.Map.Load(cmbStages.SelectedIndex);
                Main.UserSelectOtherStage();
                Timer.Stop();
                tbTime.Text = "00:00";

                int index = MapSize.FindIndex((x => x.Rows == tmp[0] && x.Cols == tmp[1]));
                cmbSize.SelectedIndex = index;

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
        }

        private void btnTrain_Click(object sender, RoutedEventArgs e)
        {
        }
        #endregion        
    }
}
