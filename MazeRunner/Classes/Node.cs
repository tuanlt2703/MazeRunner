using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MazeRunner.Classes
{
    public enum NodeType : int
    {
        Input, Hidden, Output
    }

    public enum ActivationMethod
    {
        Sigmoid, Binary
    }

    public class Node : ICloneable
    {
        public int NodeID;
        public NodeType Type;
        private ActivationMethod Activator;
        public double Value;
        public bool isActivated;
        public int Level;

        #region Constructors
        public Node(int id, NodeType type = NodeType.Input, ActivationMethod method = ActivationMethod.Sigmoid)
        {
            this.NodeID = id;
            this.Type = type;
            this.Activator = method;
            this.isActivated = false;
            this.Level = 0;
        }
        #endregion

        #region Methods
        private double SigMoidActivation(double Value)
        {
            return 1.0 / (2.0 + Math.Exp(-Value));
        }

        private int BinaryActivation(double Value, double Threshold = 0.5)
        {
            if (Value >= Threshold)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        public void Activate(double value)
        {
            isActivated = true;
            if (Activator == ActivationMethod.Sigmoid)
            {
                Value = SigMoidActivation(value);
            }
            else if (Activator == ActivationMethod.Binary)
            {
                Value = BinaryActivation(value);
            }
        }

        public object Clone()
        {
            Node newNode = (Node)this.MemberwiseClone();

            return newNode;
        }
        #endregion
    }
}
