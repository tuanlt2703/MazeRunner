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

        public void SetSuggestNE_Config(GameControl gc)
        {
            gc.tbPopSize.Text = "500";
            gc.tbPopSize.IsEnabled = true;
            gc.tbGens.Text = "1000";
            gc.tbGens.IsEnabled = true;
            gc.tbCXProb.Text = "0.5";
            gc.tbCXProb.IsEnabled = true;
            gc.tbMuProb.Text = "0.3";
            gc.tbMuProb.IsEnabled = true;
            gc.tbTopProb.Text = "0.15";
            gc.tbTopProb.IsEnabled = true;
            gc.tbLayers.Text = "2";
            gc.tbLayers.IsEnabled = true;
            gc.setHiddenLayers(new List<int>() { 3, 5 });

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
        private List<TextBox> HiddenLayers;

        private GA bot;
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
            HiddenLayers = new List<TextBox>();
        }

        #region Methods
        public delegate void Process(Chromosome Best, int i);
        public void ShowProcess(Chromosome Best, int i)
        {
            tbCurGens.Text = (i + 1).ToString();
            tbBestCh.Text = Best.Fitness.ToString();
        }

        public delegate void Finished();
        public void SaveTrainedGA()
        {
            using (Stream stream = File.Open("TrainedGA.bin", FileMode.Create))
            {
                BinaryFormatter bin = new BinaryFormatter();
                bin.Serialize(stream, bot);
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

        public void setHiddenLayers(List<int> Hiddens)
        {
            for (int i = 0; i < Hiddens.Count; i++)
            {
                HiddenLayers[i].Text = Hiddens[i].ToString();
            }
        }

        private List<int> getHiddenLayers()
        {
            List<int> tmp = new List<int>();
            foreach (var tb in HiddenLayers)
            {
                tmp.Add(Int32.Parse(tb.Text));
            }
            return tmp;
        }

        private void LoadLastGA_Config()
        {
            tbPopSize.Text = bot.Pop_Size.ToString();
            tbGens.Text = bot.Generations.ToString();
            tbCXProb.Text = bot.CrossOver_Prob.ToString();
            tbMuProb.Text = bot.Mutate_Prob.ToString();
            tbTopProb.Text = bot.TopologyMuate_Prob.ToString();
            tbLayers.Text = (bot.HLayers.Count - 2).ToString();

            int i = 1;
            foreach (var tb in HiddenLayers)
            {
                tb.Text = bot.HLayers[i++].ToString();
            }
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

        private void tbLayers_TextChanged(object sender, TextChangedEventArgs e)
        {
            HiddenLayers.Clear();
            spHiddenLayers.Children.Clear();
            int n = Int32.Parse(tbLayers.Text);
            for (int i = 0; i < n; i++)
            {
                TextBlock tmpTBL = new TextBlock();
                tmpTBL.Text = "H-Layer " + (i + 1) + ": ";
                tmpTBL.Margin = new Thickness(0, 3, 0, 0);

                TextBox tmpTB = new TextBox();
                tmpTB.Width = 28;
                tmpTB.PreviewTextInput += tb_PreviewTextInput;
                tmpTB.PreviewKeyDown += tb_PreviewKeyDown;
                HiddenLayers.Add(tmpTB);

                StackPanel tmpSP = new StackPanel();
                tmpSP.Orientation = Orientation.Horizontal;
                tmpSP.Children.Add(tmpTBL);
                tmpSP.Children.Add(tmpTB);
                spHiddenLayers.Children.Add(tmpSP);
            }
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

                //try to load previous GA
                //string path;
                if (File.Exists("TrainedGA.bin"))
                {
                    using (Stream stream = File.Open("TrainedGA.bin", FileMode.Open))
                    {
                        BinaryFormatter bin = new BinaryFormatter();
                        bot = (GA)bin.Deserialize(stream);
                    }

                    LoadLastGA_Config();
                }
                else if (File.Exists("Stopped GA.bin"))
                {
                    using (Stream stream = File.Open("Stopped GA.bin", FileMode.Open))
                    {
                        BinaryFormatter bin = new BinaryFormatter();
                        bot = (GA)bin.Deserialize(stream);
                    }
                    LoadLastGA_Config();
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
                if (bot == null)
                {
                    bot = new GA(Int32.Parse(tbPopSize.Text), Int32.Parse(tbGens.Text),
                    Double.Parse(tbMuProb.Text), Double.Parse(tbCXProb.Text), Double.Parse(tbTopProb.Text));
                    var HLayers = getHiddenLayers();
                    th = new Thread(() => bot.Excute(Main.Map.MapMatrix, HLayers, this));
                    th.Start();

                    btnTrain.Content = "Training...";
                    Started = true;
                    btnStartBot.IsEnabled = false;
                }
                else if (!bot.isFinished)                      
                {
                    if (MessageBox.Show("Countinue previous training?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                    {
                        th = new Thread(() => bot.Continue(Main.Map.MapMatrix, this));
                        th.Start();

                        btnTrain.Content = "Training...";
                        Started = true;
                        btnStartBot.IsEnabled = false;
                    }                    
                }
                else if (MessageBox.Show("GA have been trained, do you want to re-train it?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    bot = new GA(Int32.Parse(tbPopSize.Text), Int32.Parse(tbGens.Text),
                    Double.Parse(tbMuProb.Text), Double.Parse(tbCXProb.Text), Double.Parse(tbTopProb.Text));
                    var HLayers = getHiddenLayers();
                    th = new Thread(() => bot.Excute(Main.Map.MapMatrix, HLayers, this));
                    th.Start();

                    btnTrain.Content = "Training...";
                    Started = true;
                    btnStartBot.IsEnabled = false;
                }
            }
            else
            {
                if (MessageBox.Show("Training in process, do you want to save and stop the training?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes) 
                {
                    th.Abort();
                    using (Stream stream = File.Open("Stopped GA.bin", FileMode.Create))
                    {
                        BinaryFormatter bin = new BinaryFormatter();
                        bin.Serialize(stream, bot);
                    }

                    Started = false;
                    btnTrain.Content = "Start Training";
                    btnStartBot.IsEnabled = true;
                }
            }
        }
        #endregion
    }
}