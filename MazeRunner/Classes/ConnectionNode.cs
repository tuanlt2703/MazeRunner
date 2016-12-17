using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MazeRunner.Classes
{
    public enum NodeType : int
    {
        Input = 0, Hidden = 1, Output = 2
    }

    public enum ActivationMethod
    {
        Sigmoid, Binary
    }

    public class Node
    {
        public int ID;
        public NodeType Type;
        public double Value;
        public ActivationMethod Activator;
        public bool isActivated;

        #region Constructors
        public Node(int id, NodeType type = NodeType.Input, ActivationMethod method = ActivationMethod.Sigmoid)
        {
            this.ID = id;
            this.Type = type;
            this.Activator = method;
            this.Value = 0;
            this.isActivated = false;
        }
        #endregion

        #region Methods
        private double Sigmoid(double val)
        {
            return this.Value = 1.0 / (1.0 + Math.Exp(-val));
        }

        private double BinaryActivation(double Value, double Threshold = 0.5)
        {
            if (Value >= Threshold)
            {
                return this.Value = 1;
            }
            else
            {
                return this.Value = 0;
            }
        }

        public double Active(double val)
        {
            if (Activator == ActivationMethod.Sigmoid)
            {
                isActivated = true;
                return Sigmoid(val);
            }
            else if (Activator == ActivationMethod.Binary)
            {
                isActivated = true;
                return BinaryActivation(val);
            }

            return -1;
        }

        public Node Clone()
        {
            return new Node(this.ID, this.Type, this.Activator);
        }
        #endregion
    }

    public class ConnectionNode
    {
        public Node From, To;
        public double Weight;
        public int Innovation;
        public bool isEnable;

        #region Constructors
        public ConnectionNode(Node from, Node to, double weight = 0, int innov = 0, bool enable = true)
        {
            this.From = from;
            this.To = to;
            this.Weight = weight;
            this.Innovation = innov;
            this.isEnable = enable;
        }
        #endregion

        #region Methods
        public double Forward()
        {
            if (isEnable)
            {
                return From.Value * Weight;
            }

            return 0;
        }

        public void PointMutate(Random rand, int min = -2, int max = 2)
        {
            this.Weight += rand.NextDouble() * (max - min) + min;
        }

        public void Enable_Disable_Mutate()
        {
            this.isEnable = !this.isEnable;
        }
        //public ConnectionNode Clone()
        //{
        //    return new ConnectionNode(this.From.Clone(), this.To.Clone(),this.Weight, this.Innovation, this.isEnable);         
        //}
        #endregion
    }
}
