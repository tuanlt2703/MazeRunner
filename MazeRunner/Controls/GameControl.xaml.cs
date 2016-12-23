using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using MazeRunner.Classes;
using System.Windows.Input;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;
using System.Linq;

namespace MazeRunner.Controls
{
    public struct Difficultity
    {
        public int Rows, Cols;
        public string Size { get; set; }
        public double MaxObs;
        public int NumberOfObsstacles
        {
            get
            {
                return (int)((Rows * Cols) * MaxObs);
            }
        }

        public static Difficultity VeryEasy = new Difficultity()
        { Rows = 3, Cols = 3, MaxObs = 0.2, Size = "VEasy 3x3" };

        public static Difficultity Easy = new Difficultity()
        { Rows = 6, Cols = 6, MaxObs = 0.2, Size = "Easy 6x6" };

        public static Difficultity Normal = new Difficultity()
        { Rows = 8, Cols = 8, MaxObs = 0.2, Size = "Normal 8x8" };

        public static Difficultity Hard = new Difficultity()
        { Rows = 10, Cols = 10, MaxObs = 0.25, Size = "Hard 10x10" };

        public static Difficultity Insane = new Difficultity()
        { Rows = 13, Cols = 13, MaxObs = 0.3, Size = "Insane 13x13" };

        public void SetSuggestNE_Config(GameControl gc)
        {
            gc.tbPopSize.Text = "100";
            gc.tbPopSize.IsEnabled = true;
            gc.tbGens.Text = "1000";
            gc.tbGens.IsEnabled = true;
            gc.tbCXProb.Text = "0.7";
            gc.tbCXProb.IsEnabled = true;
            gc.tbMuProb.Text = "0.5";
            gc.tbMuProb.IsEnabled = false;
            gc.tbPointProb.Text = "0.5";
            gc.tbPointProb.IsEnabled = true;
            gc.tbLinkProb.Text = "0.4";
            gc.tbLinkProb.IsEnabled = true;
            gc.tbNodeProb.Text = "0.3";
            gc.tbNodeProb.IsEnabled = true;
            gc.tbEDProb.Text = "0.3";
            gc.tbEDProb.IsEnabled = true;
            //if (Rows == 6)
            //{
            //}
            //else if (Rows == 8)
            //{
            //}
            //else if (Rows == 10)
            //{
            //}
            //else if (Rows == 13)
            //{
            //}
        }
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

        private NEAT bot;
        private Network net;

        private Thread th;
        private bool Started = false;

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
        public delegate void Process(int i);
        public void ShowProcess(int i)
        {
            tbCurGens.Text = (i + 1).ToString();
        }

        public delegate void Finished(Network net);
        public void SaveTrainedGA(Network net)
        {
            int lvl = cmbStages.SelectedIndex;
            using (Stream stream = File.Open("TrainedGA_Map " + lvl.ToString() + ".bin", FileMode.Create))
            {
                BinaryFormatter bin = new BinaryFormatter();
                bin.Serialize(stream, net);
            }

            btnTrain.Content = "Start Training";
            btnStartBot.IsEnabled = true;
        }

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
            MapSize.Add(Difficultity.VeryEasy);
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
        private void tb_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            char c = Convert.ToChar(e.Text);
            e.Handled = false;
            if (Char.IsNumber(c) || c == '.')
            {
                e.Handled = false;
            }
            else
                e.Handled = true;

            base.OnPreviewTextInput(e);
        }

        private void tb_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
                e.Handled = true;
        }

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
                MapSize[index].SetSuggestNE_Config(this);

                CurrentStageID = cmbStages.SelectedIndex;
                btnUndo.IsEnabled = false;
                btnNext.IsEnabled = false;

                if (CurrentStageID > 0)
                {
                    btnTrain.IsEnabled = true;
                }

                bot = new NEAT(Int32.Parse(tbPopSize.Text), Int32.Parse(tbGens.Text), Double.Parse(tbCXProb.Text),
                    Double.Parse(tbMuProb.Text), Double.Parse(tbPointProb.Text), Double.Parse(tbLinkProb.Text),
                        Double.Parse(tbNodeProb.Text), Double.Parse(tbEDProb.Text));

                //try to load previous GA
                string path = "TrainedGA_Map " + cmbStages.SelectedIndex.ToString() + ".bin";
                if (File.Exists(path))
                {
                    using (Stream stream = File.Open(path, FileMode.Open))
                    {
                        BinaryFormatter bin = new BinaryFormatter();
                        net = (Network)bin.Deserialize(stream);
                    }
                    bot.ANN = net;
                    btnStartBot.IsEnabled = true;
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

        private void btnTrain_Click(object sender, RoutedEventArgs e)
        {
            if (!Started)
            {
                if (bot.ANN == null)
                {
                    th = new Thread(() => bot.Execute(Main.Map.MapMatrix, this));
                    th.Start();

                    btnTrain.Content = "Training...";
                    Started = true;
                    btnStartBot.IsEnabled = false;
                }
                else //network trained
                {
                    if (MessageBox.Show("a bot for this map has been already trained, do you want to re-train it from scratch?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                    {
                        th = new Thread(() => bot.Execute(Main.Map.MapMatrix, this));
                        th.Start();

                        btnTrain.Content = "Training...";
                        Started = true;
                        btnStartBot.IsEnabled = false;
                    }
                }
            }
            else
            {
                if (MessageBox.Show("Training in process, do you want to save and stop the training?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    th.Abort();

                    bot.ANN = null; //delete halfway trained network

                    Started = false;
                    btnTrain.Content = "Start Training";
                    btnStartBot.IsEnabled = true;
                }
            }
        }

        private void btnStartBot_Click(object sender, RoutedEventArgs e)
        {
            double[] Movement = bot.ANN.Run(Main.Map.MapMatrix);
            Main.Map.NEATMove(Movement);          
        }
        #endregion

        private void TextBlock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            StartGame();
            Main.Map.StartBotRunnerMove();
        }        
    }
}