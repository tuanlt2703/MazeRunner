using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MazeRunner.Controls;

namespace MazeRunner.Classes
{
    public class Genotype : IComparable<Genotype>
    {
        public List<Node> NodeGenes;
        public int InputNodes;
        public List<Node> OutputNodes;
        public List<ConnectionNode> ConnectionGenes;
        public double Fitness;

        #region Constructors
        public Genotype()
        {
            this.NodeGenes = new List<Node>();
            this.OutputNodes = new List<Node>();
            this.ConnectionGenes = new List<ConnectionNode>();
        }

        public Genotype(int input, int output, NEAT neat)
        {
            this.NodeGenes = new List<Node>();
            this.OutputNodes = new List<Node>();
            int i;
            for (i = 0; i < input; i++)
            {
                NodeGenes.Add(new Node(i));
            }
            this.InputNodes = NodeGenes.Count;
            for (; i < input + output; i++)
            {
                Node tmpNode = new Node(i, NodeType.Output, ActivationMethod.Binary);
                NodeGenes.Add(tmpNode);
                OutputNodes.Add(tmpNode);
            }

            Random rand = new Random(DateTime.Now.Millisecond);
            this.ConnectionGenes = new List<ConnectionNode>();
            double Threshold = 0.35;
            for (int j = 0; j < InputNodes; j++)
            {
                if (rand.NextDouble() <= Threshold)
                {
                    ConnectionNode tmp = new ConnectionNode(NodeGenes[j], NodeGenes[rand.Next(InputNodes, InputNodes + 2)], rand.NextDouble());
                    neat.CheckNewConnection(tmp);
                    ConnectionGenes.Add(tmp);
                }
            }
        }
        #endregion

        #region Methods
        public Genotype Clone()
        {
            Genotype newGen = new Genotype();
            foreach (var node in this.NodeGenes)
            {
                var tmp = node.Clone();
                newGen.NodeGenes.Add(tmp);
            }
            newGen.InputNodes = this.InputNodes;

            foreach (var outnode in this.OutputNodes)
            {
                newGen.OutputNodes.Add(outnode.Clone());
            }

            foreach (var connection in this.ConnectionGenes)
            {
                Node from = newGen.NodeGenes.Find((x => x.ID == connection.From.ID));
                Node to = newGen.NodeGenes.Find((x => x.ID == connection.To.ID));

                ConnectionNode tmp = new ConnectionNode(from, to, connection.Weight, connection.Innovation, connection.isEnable);
                newGen.ConnectionGenes.Add(tmp);
            }
            newGen.Fitness = this.Fitness;

            return newGen;
        }

        public int CompareTo(Genotype other)
        {
            return Fitness.CompareTo(other.Fitness);
        }

        private void LinkMutate(Random rand, NEAT neat)
        {
            //random two valid node
            Node From, To;
            do
            {
                From = NodeGenes[rand.Next(0, NodeGenes.Count)];
                To = NodeGenes[rand.Next(0, NodeGenes.Count)];
            } while (   (From.Type == NodeType.Input && To.Type == NodeType.Input) || //both nodes are input
                        (From.Type == NodeType.Output && To.Type == NodeType.Output) || //both nodes are output
                        (From.ID == To.ID) //same nodes
                        );

            if (    (ConnectionGenes.Find((x => x.From.ID == From.ID && x.To.ID == To.ID)) != null) || //same connection existed
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

        //need to limit max number of node in each hidden layer
        //so that when i-th hidden layer is full, new hidden layer will be created
        private void NodeMutate(Random rand, NEAT neat)
        {
            //double prob = NEAT.NodeMutate_Prob;
            double prob = -1;
            int lastcnn = ConnectionGenes.Count;
            for (int i = 0; i < lastcnn; i++)
            {
                if (rand.NextDouble() <= prob)
                {
                    ConnectionGenes[i].isEnable = false;

                    Node newNode = new Node(neat.TotalNodes++, NodeType.Hidden);
                    NodeGenes.Add(newNode);

                    ConnectionNode tmp = new ConnectionNode(ConnectionGenes[i].From, newNode, 1);
                    neat.CheckNewConnection(tmp);
                    ConnectionGenes.Add(tmp);

                    ConnectionNode tmp2 = new ConnectionNode(newNode, ConnectionGenes[i].To, ConnectionGenes[i].Weight);
                    neat.CheckNewConnection(tmp2);
                    ConnectionGenes.Add(tmp2);
                }
                prob *= 0.1;
            }
        }

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

        #region Neural Start Evaluate
        NEAT test;
        private void ForwardtoNode(Node target)
        {
            double sum = 0;

            var connections = ConnectionGenes.Where((x => x.To.ID == target.ID)).ToList();
            int i = 0;
            foreach (var cnn in connections)
            {
                if (!cnn.From.isActivated)
                {
                    ForwardtoNode(cnn.From);
                }
                sum += cnn.Forward();
                i++;
            }
            target.Active(sum);
        }

        private void ForwardNN()
        {
            foreach (var output in OutputNodes)
            {
                ForwardtoNode(output);
            }
        }

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
        
        public void Evaluate(int[,] MapMatrix, NEAT tes)
        {
            test = tes;
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
                    break;
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
            double kq = 60 - Elapsed.TotalSeconds;
            double hs = Win == true ? 2 : 0.5;
            if (kq > 0)
            {
                kq = (kq + 10 / Steps) * hs;
            }
            Fitness = kq;
        }
        #endregion
        #endregion
    }
}
