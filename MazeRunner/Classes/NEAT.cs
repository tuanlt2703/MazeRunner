using System;
using System.Collections.Generic;
using MazeRunner.Controls;

namespace MazeRunner.Classes
{
    public class NEAT
    {
        private List<List<int>> TotalNodes;
        private int TotalNodesCount
        {
            get
            {
                int count = 0;
                foreach (var layer in TotalNodes)
                {
                    count += layer.Count;
                }
                return count;
            }
        }
        private List<ConnectionNode> Innovations;

        private List<Species> Population;
        private Chromosome Best;
        private int Pop_Size;
        private int Generations;

        private Network ANN;

        public static double CrossOver_Prob;
        public static double Mutate_Prob;
        public static double PointMutate_Prob;
        public static double LinkMutate_Prob;
        public static double NodeMutate_Prob;
        public static double EnDisMutate_Prob;
        public static int MaxHiddenNodePerLayer = 10;

        public bool isFinished;
        private int CurrentGen;

        #region Constructor
        public NEAT(int n_pop, int gens, double xProb, double mProb, double PProb, double LProb, double NodeProb, double EDProb)
        {
            this.TotalNodes = new List<List<int>>();
            this.Innovations = new List<ConnectionNode>();
            this.Population = new List<Species>();

            this.Pop_Size = n_pop;
            this.Generations = gens;
            NEAT.CrossOver_Prob = xProb;
            NEAT.Mutate_Prob = mProb;
            NEAT.PointMutate_Prob = PProb;
            NEAT.LinkMutate_Prob = LProb;
            NEAT.NodeMutate_Prob = NodeProb;
            NEAT.EnDisMutate_Prob = EDProb;
        }
        #endregion

        #region Methods
        public void CheckNewConnection(ConnectionNode newConnection)
        {
            var tmp = Innovations.Find((x => x.From.ID == newConnection.From.ID && x.To.ID == newConnection.To.ID));
            if (tmp == null)
            {
                Innovations.Add(newConnection);
                newConnection.Innovation = Innovations.Count;
            }
            else
            {
                newConnection.Innovation = tmp.Innovation;
            }
        }

        /// <summary>
        /// check the following cases:
        /// 1/ if nodes in i-th Layer is 0, create new node, return it
        /// 2/ if nodes in i-th Layer is full, return a node in it (i-th HDlayer) randomly
        /// 3/ if nodes in i-th Layer is not full, create a random number x: [0; 1]
        ///    3a/ if Connection.ithLayerNodes.Count smaller than ithLayerNodes.Count: return a node in it(i-th HDlayer) randomly
        ///    3b/ else: create new node, return it 
        /// </summary>
        /// <param name="ith_layer"></param>
        /// <param name="input_output_count"></param>
        /// <param name="ithLayerNodes"></param>
        /// <returns></returns>
        public Node getNewNode(int ith_layer, int input_output_count, List<Node> ithLayerNodes)
        {
            if (ith_layer == 0) //lớp ẩn thứ 0
            {
                if (TotalNodes.Count - 2 > ith_layer) //lớp ẩn thứ 0 ko rỗng
                {
                    if (TotalNodes[ith_layer+2].Count == NEAT.MaxHiddenNodePerLayer) //lớp ẩn thứ 0 fulled
                    {
                        int a = 1;
                    }
                }
                
            }
            int FirstNodeID = input_output_count + ith_layer * NEAT.MaxHiddenNodePerLayer;
            int EndNodeID = FirstNodeID + NEAT.MaxHiddenNodePerLayer - 1;

            Random rand = new Random(DateTime.Now.Millisecond + ith_layer);

            //new node in ith_layer => level of node = ith + 1
            Node newNode;
            if (TotalNodes.Count - 2 <= ith_layer) //case 1: i-th layer is empty
            {                
                newNode = new Node(FirstNodeID, NodeType.Hidden, ActivationMethod.Sigmoid, ith_layer + 1);

                List<int> newLayer = new List<int>();
                newLayer.Add(newNode.ID);

                TotalNodes.Add(newLayer);
                return newNode;
            }

            if (TotalNodes[ith_layer + 2].Count == NEAT.MaxHiddenNodePerLayer) //case 2: i-th layer is fulled
            {
                do
                {
                    newNode = new Node(TotalNodes[ith_layer + 2][rand.Next(0, TotalNodes[ith_layer + 2].Count)],
                                                        NodeType.Hidden, ActivationMethod.Sigmoid, ith_layer + 1);
                } while (ithLayerNodes.Find((x => x.ID == newNode.ID)) != null);
                return newNode;
            }
            else//case 3: i-th layer is not empty, not full
            {
                if (ithLayerNodes.Count < TotalNodes[ith_layer + 2].Count)//case 3a
                {
                    do
                    {
                        newNode = new Node(TotalNodes[ith_layer + 2][rand.Next(0, TotalNodes[ith_layer + 2].Count)],
                                                            NodeType.Hidden, ActivationMethod.Sigmoid, ith_layer + 1);
                    } while (ithLayerNodes.Find((x => x.ID == newNode.ID)) != null);

                    return newNode;
                }
                else //case 3b
                {
                    var ithLNodes = TotalNodes[ith_layer + 2];
                    newNode = new Node(ithLNodes[ithLNodes.Count - 1] + 1, NodeType.Hidden, ActivationMethod.Sigmoid, ith_layer + 1);
                    TotalNodes[ith_layer + 2].Add(newNode.ID);
                    return newNode;
                }
            }
        }

        private void Classify_Species(Chromosome child)
        {
            bool foundSpecies = false;
            foreach (var species in Population)
            {
                if (species.addNewChild(child))
                {
                    foundSpecies = true;
                    break;
                }
            }

            if (!foundSpecies)
            {
                Species newSpecies = new Species();
                newSpecies.Natives.Add(child);
                Population.Add(newSpecies);
            }
        }

        private void CreatePopulation(int[,] MapMatrix, int output = 2)
        {
            Population.Add(new Species());
            for (int i = 0; i < Pop_Size; i++)
            {
                Chromosome tmp = new Chromosome(MapMatrix.GetLength(0) * MapMatrix.GetLength(1), output, this);
                tmp.Evaluate(MapMatrix);

                Classify_Species(tmp);
            }

            //init nodes list
            int k;
            List<int> InputNodes = new List<int>();
            for (k = 0; k < MapMatrix.GetLength(0) * MapMatrix.GetLength(1); k++)
            {
                InputNodes.Add(k);
            }
            TotalNodes.Add(InputNodes);

            List<int> OutputNodes = new List<int>();
            for (; k < MapMatrix.GetLength(0) * MapMatrix.GetLength(1) + output; k++)
            {
                OutputNodes.Add(k);
            }
            TotalNodes.Add(OutputNodes);
        }               

        private void Selection()
        {
            Population[0].Selection();
            Best = Population[0].Natives[0];
            for (int i = 1; i < Population.Count; i++)
            {
                Population[i].Selection();
                if (Best.Fitness < Population[i].Natives[0].Fitness)
                {
                    Best = Population[i].Natives[0];
                }
            }


            Population.Sort();
            if (Population.Count > Pop_Size)
            {
                Population.RemoveRange(Pop_Size, Population.Count - Pop_Size);
            }
        }

        private void Breed(int[,] Map)
        {
            for (int i = 0; i < Population.Count; i++)
            {
                foreach (var ch in Population[i].Breed())
                {
                    ch.Evaluate(Map);
                    Classify_Species(ch);
                }
            }
        }

        private void Mutate(int [,] Map)
        {
            foreach (var species in Population)
            {
                species.Mutate(Map, this);
            }
        }

        #region ANN
        private int[,] CloneMap(int[,] MapMatrix, ref MazeRunner.Controls.CharacterPos CharPos)
        {
            int[,] tmp = new int[MapMatrix.GetLength(0), MapMatrix.GetLength(1)];
            for (int i = 0; i < MapMatrix.GetLength(0); i++)
            {
                for (int j = 0; j < MapMatrix.GetLength(1); j++)
                {
                    tmp[i, j] = MapMatrix[i, j];
                    if (tmp[i, j] == (int)MazeRunner.Controls.Cell.Runner)
                    {
                        CharPos.RunnerX = i;
                        CharPos.RunnerY = j;
                    }
                    else if (tmp[i, j] == (int)MazeRunner.Controls.Cell.Chaser)
                    {
                        CharPos.ChaserX = i;
                        CharPos.ChaserY = j;
                    }
                }
            }
            return tmp;
        }

        private void RunnerMove(List<int> Movement, int[,] Map, ref CharacterPos CharPos)
        {
            int move = 0;
            if (Movement[1] == 0 && Movement[0]== 0)
            {
                move = 0; //Down
            }
            else if (Movement[1] == 0 && Movement[0] == 1)
            {
                move = 1; //Up
            }
            else if (Movement[1] == 1 && Movement[0] == 0)
            {
                move = 2; //Right
            }
            else if (Movement[1] == 1 && Movement[0] == 1)
            {
                move = 3; //Left
            }


            if (move == 0) //Down
            {
                Map[CharPos.RunnerX, CharPos.RunnerY] = (int)Cell.Moveable;
                CharPos.RunnerX += 1;
                Map[CharPos.RunnerX, CharPos.RunnerY] = (int)Cell.Runner;
            }
            else if (move == 1) //Up
            {
                Map[CharPos.RunnerX, CharPos.RunnerY] = (int)Cell.Moveable;
                CharPos.RunnerX -= 1;
                Map[CharPos.RunnerX, CharPos.RunnerY] = (int)Cell.Runner;
            }
            else if (move == 2) //Right
            {
                Map[CharPos.RunnerX, CharPos.RunnerY] = (int)Cell.Moveable;
                CharPos.RunnerY += 1;
                Map[CharPos.RunnerX, CharPos.RunnerY] = (int)Cell.Runner;
            }
            else if (move == 3) //Left
            {
                Map[CharPos.RunnerX, CharPos.RunnerY] = (int)Cell.Moveable;
                CharPos.RunnerY -= 1;
                Map[CharPos.RunnerX, CharPos.RunnerY] = (int)Cell.Runner;
            }
        }

        private void RunnerMove(double[] Movement, int[,] Map, ref CharacterPos CharPos)
        {
            int move = 0;
            if (Movement[1] == 0 && Movement[0] == 0)
            {
                move = 0; //Down
            }
            else if (Movement[1] == 0 && Movement[0] == 1)
            {
                move = 1; //Up
            }
            else if (Movement[1] == 1 && Movement[0] == 0)
            {
                move = 2; //Right
            }
            else if (Movement[1] == 1 && Movement[0] == 1)
            {
                move = 3; //Left
            }


            if (move == 0) //Down
            {
                Map[CharPos.RunnerX, CharPos.RunnerY] = (int)Cell.Moveable;
                CharPos.RunnerX += 1;
                Map[CharPos.RunnerX, CharPos.RunnerY] = (int)Cell.Runner;
            }
            else if (move == 1) //Up
            {
                Map[CharPos.RunnerX, CharPos.RunnerY] = (int)Cell.Moveable;
                CharPos.RunnerX -= 1;
                Map[CharPos.RunnerX, CharPos.RunnerY] = (int)Cell.Runner;
            }
            else if (move == 2) //Right
            {
                Map[CharPos.RunnerX, CharPos.RunnerY] = (int)Cell.Moveable;
                CharPos.RunnerY += 1;
                Map[CharPos.RunnerX, CharPos.RunnerY] = (int)Cell.Runner;
            }
            else if (move == 3) //Left
            {
                Map[CharPos.RunnerX, CharPos.RunnerY] = (int)Cell.Moveable;
                CharPos.RunnerY -= 1;
                Map[CharPos.RunnerX, CharPos.RunnerY] = (int)Cell.Runner;
            }
        }

        private void ChaserMove(int[,] Map, ref CharacterPos CharPos)
        {
            //chaser's turn
            var chase = Chaser.Asmove2(Map);
            Map[CharPos.ChaserX, CharPos.ChaserY] = (int)Cell.Moveable;
            CharPos.ChaserX = chase[0];
            CharPos.ChaserY = chase[1];
            Map[CharPos.ChaserX, CharPos.ChaserY] = (int)Cell.Chaser;
        }

        private bool? isWon(int[,] Map, CharacterPos CharPos)
        {
            if (CharPos.GotCaught)
            {
                return false;
            }

            if (Map[CharPos.RunnerX, CharPos.RunnerY] == (int)Cell.Goal)
            {
                return true;
            }

            return null;
        }

        private bool TrainedSuccessful(int[,] MapMatrix, int last_steps)
        {
            CharacterPos CharPos = new CharacterPos();
            int[,] Map = CloneMap(MapMatrix, ref CharPos);

            for (int i = 0; i < last_steps; i++)
            {
                var Movement = ANN.Run(Map);
                RunnerMove(Movement, Map, ref CharPos);
                ChaserMove(Map, ref CharPos);

                var result = isWon(Map, CharPos);
                if (result == true)
                {
                    return true;
                }
                else if (result == false)
                {
                    return false;
                }
            }

            return false;
        }

        private void STL(int[,] MapMatrix, GameControl gc)
        {
            //config ANN with: 
            //1 input layer: <input> nodes
            //1 output layer: <output> nodes
            //tmp.Count hidden layer: each nodes_count in each layer is in tmp
            List<int> tmp = new List<int>() { 5 };
            int input = MapMatrix.GetLength(0) * MapMatrix.GetLength(1);
            int output = 2;
            ANN = new Network(input, output, tmp.Count, tmp);

            CharacterPos CharPos = new CharacterPos();
            int[,] Map = CloneMap(MapMatrix, ref CharPos);

            var trainingset = new NeuronDotNet.Core.TrainingSet(input, output);
            int trainedtmp = 0;

            while (true)
            {
                int i = 0;

                List<int> Movement = Chaser.Asmove3(Map);
                ANN.Learn(Map, Movement);
                RunnerMove(Movement, Map, ref CharPos);
                ChaserMove(Map, ref CharPos);

                i++;

                trainedtmp++;
                GameControl.Process pc = new GameControl.Process(gc.ShowProcess);
                gc.Dispatcher.Invoke(pc, new object[] { i });

                var result = isWon(Map, CharPos);
                if (result != null) //lose or win
                {
                    if (result == false) //lose
                    {
                        System.Windows.MessageBox.Show("failed");
                    }
                    else if (result == true) //won
                    {
                        //do sth...
                        if (TrainedSuccessful(MapMatrix, i))
                        {
                            System.Windows.MessageBox.Show("succeeded");
                            break;
                        }
                        else //A* ran succeeded, but neural hasn't learned yet.
                        {
                            Map = CloneMap(MapMatrix, ref CharPos);
                        }
                    }
                }

            }
        }
        #endregion

        public void Execute(int[,] MapMatrix, GameControl gc)
        {
            STL(MapMatrix, gc);
            //Population.Clear();

            ////Init population
            //CreatePopulation(MapMatrix);
            //Selection();

            //for (CurrentGen = 0; CurrentGen < Generations; CurrentGen++)
            //{
            //    //CrossOver - Mutate
            //    Breed(MapMatrix);
            //    Mutate(MapMatrix);

            //    Selection();

            //    //
            //    MazeRunner.Controls.GameControl.Process pc = new Controls.GameControl.Process(gc.ShowProcess);
            //    gc.Dispatcher.Invoke(pc, new object[] { Best, CurrentGen });
            //}

            ////finished
            //isFinished = true;
            //MazeRunner.Controls.GameControl.Finished fin = new Controls.GameControl.Finished(gc.SaveTrainedGA);
            //gc.Dispatcher.Invoke(fin);
        }
        #endregion
    }
}
