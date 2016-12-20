using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MazeRunner.Controls;

namespace MazeRunner.Classes
{
    //Chromosome is a collection of genes (node_genes and connection_genes)
    public class Chromosome : IComparable<Chromosome>
    {
        #region Properties
        //used by others
        public List<Node> NodeGenes;
        public List<ConnectionNode> ConnectionGenes;
        public double Fitness;

        //
        public int InputNodesCount;
        public List<Node> OutputNodes;

        //
        private int Input_Output_Count
        {
            get { return InputNodesCount + OutputNodes.Count; }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Create completely new Chromosome with infomation about genes in it
        /// </summary>
        /// <param name="input_count"></param>
        /// <param name="output_count"></param>
        /// <param name="neat"></param>
        public Chromosome(int input_count, int output_count, NEAT neat)
        {
            int i = 0;
            //create input nodes
            this.NodeGenes = new List<Node>();            
            for (; i < input_count; i++)
            {
                NodeGenes.Add(new Node(i));
            }
            this.InputNodesCount = NodeGenes.Count;

            //create output nodes
            this.OutputNodes = new List<Node>();
            for (; i < input_count + output_count; i++)
            {
                Node tmpNode = new Node(i, NodeType.Output, ActivationMethod.Binary, Int32.MinValue);
                NodeGenes.Add(tmpNode);
                OutputNodes.Add(tmpNode);
            }

            //create first random connections
            Random rand = new Random(DateTime.Now.Millisecond);
            this.ConnectionGenes = new List<ConnectionNode>();
            double Threshold = 0.35;
            for (int j = 0; j < InputNodesCount; j++)
            {
                if (rand.NextDouble() <= Threshold)
                {
                    ConnectionNode tmp = new ConnectionNode(NodeGenes[j], OutputNodes[rand.Next(0, output_count)], rand.NextDouble());
                    neat.CheckNewConnection(tmp);
                    ConnectionGenes.Add(tmp);
                }
            }
        }

        /// <summary>
        /// Create new Chromosome with no infomation
        /// </summary>
        public Chromosome()
        {
            this.NodeGenes = new List<Node>();
            this.OutputNodes = new List<Node>();
            this.ConnectionGenes = new List<ConnectionNode>();
        }
        #endregion

        #region Methods
        public Chromosome Clone()
        {
            Chromosome newGen = new Chromosome();

            //copy all nodes (input, output, hidden)
            foreach (var node in this.NodeGenes)
            {
                newGen.NodeGenes.Add(node.Clone());
            }

            //copy input nodes count
            newGen.InputNodesCount = this.InputNodesCount;

            //copy output nodes
            foreach (var outnode in this.OutputNodes)
            {
                newGen.OutputNodes.Add(outnode.Clone());
            }

            //copy all connections
            foreach (var connection in this.ConnectionGenes)
            {
                Node from = newGen.NodeGenes.Find((x => x.ID == connection.From.ID));
                Node to = newGen.NodeGenes.Find((x => x.ID == connection.To.ID));

                ConnectionNode tmp = new ConnectionNode(from, to, connection.Weight, connection.Innovation, connection.isEnable);
                newGen.ConnectionGenes.Add(tmp);
            }

            //copy fitness
            newGen.Fitness = this.Fitness;

            return newGen;
        }

        public int CompareTo(Chromosome other)
        {
            return other.Fitness.CompareTo(Fitness);
        }

        #region Mutate Algorithms
        private void LinkMutate(Random rand, NEAT neat)
        {
            //random two valid node
            Node From, To;
            do
            {
                From = NodeGenes[rand.Next(0, NodeGenes.Count)];
                To = NodeGenes[rand.Next(0, NodeGenes.Count)];
            } while ((From.Type == NodeType.Input && To.Type == NodeType.Input) || //both nodes are input
                        (From.Type == NodeType.Output && To.Type == NodeType.Output) || //both nodes are output
                        (From.ID == To.ID) //same nodes
                        );

            if ((ConnectionGenes.Find((x => x.From.ID == From.ID && x.To.ID == To.ID)) != null) || //same connection existed
                    (ConnectionGenes.Find((x => x.From.ID == To.ID && x.To.ID == From.ID)) != null) //same connection existed)
                    )
            {
                return;
            }

            //swap if start node is output
            if (From.Type == NodeType.Output)
            {
                Node tmp = From;
                From = To;
                To = tmp;
            }

            //add link
            var newConnect = new ConnectionNode(From, To, rand.NextDouble() * 4 - 2);
            neat.CheckNewConnection(newConnect);
            ConnectionGenes.Add(newConnect);
        }

        private int ith_Layer_Of_Node(Node target)
        {
            if (target.ID < Input_Output_Count) //this is input (ouput node should not be appear in Connection.From)
            {
                return -1;
            }

            return (target.ID - Input_Output_Count) / 10;
        }

        private List<Node> Get_ithLayerNodes(int i)
        {
            int FirstNodeID = Input_Output_Count + i * NEAT.MaxHiddenNodePerLayer;
            int EndNodeID = FirstNodeID + NEAT.MaxHiddenNodePerLayer - 1;

            var ith_HDLayerNodes = NodeGenes.Where((x => x.ID >= FirstNodeID && x.ID <= EndNodeID)).ToList();

            return ith_HDLayerNodes;
        }

        private bool isValidConnectionToMutateNode(ConnectionNode connection, int ithLayerOf_From, int ithLayerOf_NewTo)
        {

            int ithLayerOf_NodeTo = ith_Layer_Of_Node(connection.To);

            if (ithLayerOf_NodeTo == ithLayerOf_NewTo) //this connection has alrdy linked to intended layer of newNode
            {
                return false;
            }

            return true;
        }

        private void NodeMutate(Random rand, NEAT neat)
        {
            double prob = NEAT.NodeMutate_Prob;
            int lastcnn = ConnectionGenes.Count;
            for (int i = 0; i < lastcnn; i++)
            {
                if (rand.NextDouble() <= prob)
                {
                    int ithLayer_Of_NodeFrom = ith_Layer_Of_Node(ConnectionGenes[i].From); //number of layer contains FromNode

                    int ithLayer_Of_NewNodeTo = ithLayer_Of_NodeFrom + 1; //will alway >= 0
                    var ithLayerNodes = Get_ithLayerNodes(ithLayer_Of_NewNodeTo);
                    //if next layer hasn't reached the maximum node
                    if (ithLayerNodes.Count < NEAT.MaxHiddenNodePerLayer)
                    {
                        //if (isValidConnectionToMutateNode(ConnectionGenes[i], ithLayer_Of_NodeFrom, ithLayer_Of_NewNodeTo))
                        //{
                        //    ConnectionGenes[i].isEnable = false;
                        //}
                        if (ConnectionGenes[i].To.Type ==  NodeType.Output)
                        {
                            ConnectionGenes[i].isEnable = false;

                            Node newNode = neat.getNewNode(ithLayer_Of_NewNodeTo, Input_Output_Count, ithLayerNodes);
                            NodeGenes.Add(newNode);

                            ConnectionNode tmp = new ConnectionNode(ConnectionGenes[i].From, newNode, 1);
                            neat.CheckNewConnection(tmp);
                            ConnectionGenes.Add(tmp);

                            ConnectionNode tmp2 = new ConnectionNode(newNode, ConnectionGenes[i].To, ConnectionGenes[i].Weight);
                            neat.CheckNewConnection(tmp2);
                            ConnectionGenes.Add(tmp2);
                        }
                    }
                }
            }
        }
        #endregion

        public void Mutate(NEAT neat)
        {
            Random rand = new Random(DateTime.Now.Millisecond);

            int lastcnn = ConnectionGenes.Count;
            for (int i = 0; i < lastcnn; i++)
            {
                if (rand.NextDouble() <= NEAT.PointMutate_Prob)
                {
                    ConnectionGenes[i].PointMutate(rand);
                }

                if (rand.NextDouble() <= NEAT.LinkMutate_Prob)
                {
                    LinkMutate(rand, neat);
                }

                if (rand.NextDouble() <= NEAT.NodeMutate_Prob)
                {
                    NodeMutate(rand, neat);
                }

                if (rand.NextDouble() <= NEAT.EnDisMutate_Prob)
                {
                    ConnectionGenes[i].Enable_Disable_Mutate();
                }
            }
        }

        #region Neural Starts Evaluate
        private void GetInput(int[,] MapMatrix)
        {
            int k = 0;
            for (int i = 0; i < MapMatrix.GetLength(0); i++)
            {
                for (int j = 0; j < MapMatrix.GetLength(1); j++)
                {
                    NodeGenes[k].Value = MapMatrix[i, j];
                    NodeGenes[k++].isActivated = true;
                }
            }
        }

        private int FinalizeOutput()
        {
            int LargestLayer;
            if (NodeGenes.Count <= Input_Output_Count) //this neural network doesn't have any hidden layer
            {
                LargestLayer = -1;
            }
            else
            {
                LargestLayer = NodeGenes.Max(x => x.Level);
            }

            foreach (var output in OutputNodes)
            {
                output.Level = LargestLayer + 1;
            }

            return LargestLayer + 1;
        }

        private List<ConnectionNode> ConnectionListFrom(int i)
        {
            if (i == -1) //FromNode is input
            {
                return ConnectionGenes.Where((x => x.From.ID >= 0 && x.From.ID < InputNodesCount)).ToList();
            }
            else
            {
                int FirstNodeID = Input_Output_Count + i * NEAT.MaxHiddenNodePerLayer;
                int EndNodeID = FirstNodeID + NEAT.MaxHiddenNodePerLayer - 1;

                return ConnectionGenes.Where((x => x.From.ID >= FirstNodeID && x.From.ID <= EndNodeID)).ToList();
            }
        }

        private void ForwardNN()
        {
            int LastLayer = FinalizeOutput();

            for (int i = -1; i < LastLayer; i++)
            {
                foreach (var cnn in ConnectionListFrom(i))
                {
                    cnn.Forward();
                }
            }

            for (int i = Input_Output_Count; i < NodeGenes.Count; i++)
            {
                NodeGenes[i].Value = 0;
            }
        }     

        private int Run(int[,] MapMatrix)
        {
            GetInput(MapMatrix);

            ForwardNN();

            if (OutputNodes[1].Value == 0 && OutputNodes[0].Value == 0)
            {
                return 0; //Down
            }
            else if (OutputNodes[1].Value == 0 && OutputNodes[0].Value == 1)
            {
                return 1; //Up
            }
            else if (OutputNodes[1].Value == 1 && OutputNodes[0].Value == 0)
            {
                return 2; //Right
            }
            else if (OutputNodes[1].Value == 1 && OutputNodes[0].Value == 1)
            {
                return 3; //Left
            }
            return -1;
        }

        private int[,] Clone(int[,] MapMatrix, ref MazeRunner.Controls.CharacterPos CharPos)
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

        private bool isValidMove(int Movement, int[,] Map, ref CharacterPos CharPos)
        {
            if (Movement == 0) //Down
            {
                if (CharPos.RunnerX < Map.GetLength(1) - 1)
                {
                    if (Map[CharPos.RunnerX + 1, CharPos.RunnerY] != (int)Cell.Obstacle
                        && Map[CharPos.RunnerX + 1, CharPos.RunnerY] != (int)Cell.Chaser)
                    {
                        Map[CharPos.RunnerX, CharPos.RunnerY] = (int)Cell.Moveable;
                        CharPos.RunnerX += 1;
                        Map[CharPos.RunnerX, CharPos.RunnerY] = (int)Cell.Runner;
                        return true;
                    }
                }
            }
            else if (Movement == 1) //Up
            {
                if (CharPos.RunnerX > 0)
                {
                    if (Map[CharPos.RunnerX - 1, CharPos.RunnerY] != (int)Cell.Obstacle
                        && Map[CharPos.RunnerX - 1, CharPos.RunnerY] != (int)Cell.Chaser)
                    {
                        Map[CharPos.RunnerX, CharPos.RunnerY] = (int)Cell.Moveable;
                        CharPos.RunnerX -= 1;
                        Map[CharPos.RunnerX, CharPos.RunnerY] = (int)Cell.Runner;
                        return true;
                    }
                }
            }
            else if (Movement == 2) //Right
            {
                if (CharPos.RunnerY < Map.GetLength(0) - 1)
                {
                    if (Map[CharPos.RunnerX, CharPos.RunnerY + 1] != (int)Cell.Obstacle
                        && Map[CharPos.RunnerX, CharPos.RunnerY + 1] != (int)Cell.Chaser)
                    {
                        Map[CharPos.RunnerX, CharPos.RunnerY] = (int)Cell.Moveable;
                        CharPos.RunnerY += 1;
                        Map[CharPos.RunnerX, CharPos.RunnerY] = (int)Cell.Runner;
                        return true;
                    }
                }
            }
            else if (Movement == 3) //Left
            {
                if (CharPos.RunnerY > 0)
                {
                    if (Map[CharPos.RunnerX, CharPos.RunnerY - 1] != (int)Cell.Obstacle
                        && Map[CharPos.RunnerX, CharPos.RunnerY - 1] != (int)Cell.Chaser)
                    {
                        Map[CharPos.RunnerX, CharPos.RunnerY] = (int)Cell.Moveable;
                        CharPos.RunnerY -= 1;
                        Map[CharPos.RunnerX, CharPos.RunnerY] = (int)Cell.Runner;
                        return true;
                    }
                }
            }
            return false;
        }

        public void Evaluate(int[,] MapMatrix)
        {
            CharacterPos CharPos = new CharacterPos();
            int[,] Map = Clone(MapMatrix, ref CharPos);
            int Steps = 1;
            bool Win = false;

            DateTime Start = DateTime.Now;
            var Elapsed = DateTime.Now - Start;

            while (true)
            {
                //runner's turn
                int Movement = Run(Map);
                if (!isValidMove(Movement, Map, ref CharPos))
                {
                    //break;
                }
                Steps++;

                //chaser's turn
                var chase = Chaser.Asmove2(Map);
                Map[CharPos.ChaserX, CharPos.ChaserY] = (int)Cell.Moveable;
                CharPos.ChaserX = chase[0];
                CharPos.ChaserY = chase[1];
                Map[CharPos.ChaserX, CharPos.ChaserY] = (int)Cell.Chaser;

                //calculate score
                Elapsed = DateTime.Now - Start;
                if (Elapsed.TotalMinutes > 1)
                {
                    break;
                }
                if (CharPos.GotCaught)
                {
                    break;
                }
                if (Map[CharPos.RunnerX, CharPos.RunnerY] == (int)Cell.Goal)
                {
                    Win = true;
                    break;
                }
            }

            Elapsed = DateTime.Now - Start;
            double kq = Elapsed.TotalSeconds;
            double hs = Win == true ? 2 : 0.5;
            if (kq > 0)
            {
                kq = (kq + Steps) * hs;
            }
            Fitness = kq;
        }
        #endregion
        #endregion
    }
}
