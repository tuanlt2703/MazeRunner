using MazeRunner.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MazeRunner.Classes
{
    public class Chromosome : IComparable<Chromosome>, ICloneable
    {
        public List<Node> Nodes;
        public int InputNodes;
        public List<Node> OutputNodes;
        public List<ConnectionNode> ConnectionGens;
        //private int PotentailMaxConnections
        //{
        //    get
        //    {
        //        return 0;
        //    }
        //}

        public double Fitness;

        #region Constructors
        public Chromosome(List<Node> input, List<Node> output, NEAT neat)
        {
            this.Nodes = input;
            this.InputNodes = this.Nodes.Count;
            this.OutputNodes = output;
            CreateRandomConnections(neat, 0.35);

            this.Fitness = 0;
        }

        public Chromosome(List<Node> output)
        {
            this.Nodes = new List<Node>();
            this.OutputNodes = output;
            this.ConnectionGens = new List<ConnectionNode>();
            this.Fitness = 0;
        }
        #endregion

        #region Methods
        private void CreateRandomConnections(NEAT neat, double Threshold)
        {
            Random rnd = new Random(DateTime.Now.Millisecond);
            ConnectionGens = new List<ConnectionNode>();
            
            foreach (var input in Nodes)
            {
                if (rnd.NextDouble() <= Threshold)
                {
                    var tmp = new ConnectionNode(input, OutputNodes[rnd.Next(2)], rnd.NextDouble());
                    neat.CheckNewConnection(tmp);
                    ConnectionGens.Add(tmp);                    
                }
            }
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

        #region Neural net starts
        private void GetInput(int [,] MapMatrix)
        {
            DeActiveAllNodes();
            int k = 0;
            for (int i = 0; i < MapMatrix.GetLength(0); i++)
            {
                for (int j = 0; j < MapMatrix.GetLength(1); j++)
                {
                    Nodes[k].Value = MapMatrix[i, j];
                    Nodes[k++].isActivated = true;
                }
            }
        }

        private void DeActiveAllNodes()
        {
            foreach (var node in Nodes)
            {
                node.isActivated = false;
            }
        }

        private int Run(int[,] MapMatrix)
        {
            GetInput(MapMatrix);           

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
        #endregion

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
            MazeRunner.Controls.CharacterPos CharPos = new MazeRunner.Controls.CharacterPos();
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

        public int CompareTo(Chromosome other)
        {
            return Fitness.CompareTo(other.Fitness);
        }

        public void AddConnection(ConnectionNode connection)
        {
            ConnectionGens.Add(connection);

            var from = Nodes.Find((x => x.NodeID == connection.From.NodeID));
            if (from == null)
            {
                Nodes.Add(connection.From);
            }

            var to = Nodes.Find((x => x.NodeID == connection.To.NodeID));
            if (to == null)
            {
                Nodes.Add(connection.To);
            }
        }

        private void LinkMutate(Random r, NEAT neat)
        {
            //merge two list
            List<Node> tmpNodes = new List<Node>();
            foreach (var nodes in Nodes)
            {
                tmpNodes.Add(nodes);
            }

            foreach (var nodes in OutputNodes)
            {
                tmpNodes.Add(nodes);
            }

            //
            Node From, To;
            do
            {
                From = tmpNodes[r.Next(0, tmpNodes.Count)];
                To = tmpNodes[r.Next(0, tmpNodes.Count)];
            } while (   (From.Type == NodeType.Input && To.Type == NodeType.Input) || //both nodes are input
                        (From.Type == NodeType.Output && To.Type == NodeType.Output) || //both nodes are output
                        (From.NodeID == To.NodeID) || //same nodes
                        (ConnectionGens.Find((x=>x.From.NodeID == From.NodeID && x.To.NodeID == To.NodeID)) != null) //same connection existed
                        );

            //swap if start node is output
            if (From.Type == NodeType.Output)
            {
                Node tmp = From;
                From = To;
                To = tmp;
            }

            //add link
            var newConnect = new ConnectionNode(From, To, r.NextDouble() * 4 - 2);
            neat.CheckNewConnection(newConnect);
            ConnectionGens.Add(newConnect);
        }

        private void NodeMutate(Random r, NEAT neat)
        {
            for (int i = 0; i < ConnectionGens.Count; i++)
            {
                if (r.NextDouble() <= NEAT.NodeMutate_Prob)
                {
                    Node newNode = new Node(neat.TotalNodes++, NodeType.Hidden);
                    newNode.Level = ConnectionGens[i].From.Level + 1;
                    ConnectionGens[i].Enabled = false;
                    ConnectionNode tmp1 = new ConnectionNode((Node)ConnectionGens[i].From.Clone(), newNode, 1);
                    neat.CheckNewConnection(tmp1);
                    ConnectionGens.Add(tmp1);

                    ConnectionNode tmp2 = new ConnectionNode(newNode, (Node)ConnectionGens[i].To.Clone(), ConnectionGens[i].Weight);
                    neat.CheckNewConnection(tmp2);
                    ConnectionGens.Add(tmp2);
                    break;
                }
            }
        }

        public void Mutate(Random r, NEAT neat)
        {
            for (int i = 0; i < ConnectionGens.Count; i++)
            {
                if (r.NextDouble() <= NEAT.Mutate_Prob)
                {
                    if (r.NextDouble() <= NEAT.PointMutate_Prob)
                    {
                        ConnectionGens[i].PointMutate(r);
                    }

                    if (r.NextDouble() <= NEAT.LinkMutate_Prob)
                    {
                        LinkMutate(r, neat);
                    }

                    if (r.NextDouble() <= NEAT.NodeMutate_Prob)
                    {
                        NodeMutate(r, neat);
                    }

                    if (r.NextDouble() <= NEAT.EnDisMutate_Prob)
                    {
                        ConnectionGens[i].Enable_Disable_Mutate();
                    }
                }
            }
        }

        public object Clone()
        {
            Chromosome newconnection = (Chromosome)this.MemberwiseClone();

            return newconnection;
        }
        #endregion
    }
}
