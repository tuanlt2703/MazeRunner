using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        /// 1/ if nodes in i-th HDlayer is 0, create new node, return it
        /// 2/ if nodes in i-th HDlayer is full, return a node in it (i-th HDlayer) randomly
        /// 3/ if nodes in i-th HDlayer is not full, create a random number x: [0; 1]
        ///    3a/ return a node in i-th HDlayer randomly
        ///    3b/ x > 0.5: create new node, return it 
        /// </summary>
        /// <param name="ith_layer"></param>
        /// <param name="input_output_count"></param>
        /// <param name="ithLayerNodes"></param>
        /// <returns></returns>
        public Node getNewNode(int ith_layer, int input_output_count, List<Node> ithLayerNodes)
        {
            int FirstNodeID = input_output_count + ith_layer * NEAT.MaxHiddenNodePerLayer;
            int EndNodeID = FirstNodeID + NEAT.MaxHiddenNodePerLayer - 1;

            Random rand = new Random(DateTime.Now.Millisecond + ith_layer);

            //new node in ith_layer => level of node = ith + 1
            Node newNode;
            if (TotalNodes[ith_layer].Count == 0) //case 1: i-th layer is empty
            {
                newNode = new Node(TotalNodesCount, NodeType.Hidden, ActivationMethod.Sigmoid, ith_layer + 1);
                TotalNodes[ith_layer].Add(newNode.ID);
                return newNode;
            }

            if (TotalNodes[ith_layer].Count == NEAT.MaxHiddenNodePerLayer) //case 2: i-th layer is fulled
            {
                do
                {
                    newNode = new Node(TotalNodes[ith_layer][rand.Next(0, TotalNodes[ith_layer].Count)],
                    NodeType.Hidden, ActivationMethod.Sigmoid, ith_layer + 1);
                } while (ithLayerNodes.Find((x => x.ID == newNode.ID)) != null);
                return newNode;
            }
            else//case 3: i-th layer is not empty, not full
            {
                if (rand.NextDouble() <= 0.5)//branch 3a
                {
                    do
                    {
                        newNode = new Node(TotalNodes[ith_layer][rand.Next(0, TotalNodes[ith_layer].Count)],
                        NodeType.Hidden, ActivationMethod.Sigmoid, ith_layer + 1);
                    } while (ithLayerNodes.Find((x => x.ID == newNode.ID)) != null);

                    return newNode;
                }
                else //branch 3b
                {
                    newNode = new Node(TotalNodesCount, NodeType.Hidden, ActivationMethod.Sigmoid, ith_layer + 1);
                    TotalNodes[ith_layer].Add(newNode.ID);
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

            List<int> OutputNodes = new List<int>();
            for (; k < MapMatrix.GetLength(0) * MapMatrix.GetLength(1) + output; k++)
            {
                OutputNodes.Add(k);
            }
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

        public void Execute(int[,] MapMatrix, MazeRunner.Controls.GameControl gc)
        {
            Population.Clear();

            //Init population
            CreatePopulation(MapMatrix);
            Selection();

            for (CurrentGen = 0; CurrentGen < Generations; CurrentGen++)
            {
                //CrossOver - Mutate
                Breed(MapMatrix);
                Mutate(MapMatrix);

                Selection();

                //
                MazeRunner.Controls.GameControl.Process pc = new Controls.GameControl.Process(gc.ShowProcess);
                gc.Dispatcher.Invoke(pc, new object[] { Best, CurrentGen });
            }

            //finished
            isFinished = true;
            MazeRunner.Controls.GameControl.Finished fin = new Controls.GameControl.Finished(gc.SaveTrainedGA);
            gc.Dispatcher.Invoke(fin);
        }
        #endregion
    }
}
