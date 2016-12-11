using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MazeRunner.Classes
{
    public class ConnectionNode : ICloneable
    {
        public Node From;
        public Node To;
        public double Weight;
        public bool Enabled;
        public int Innovation;

        #region Constructors
        public ConnectionNode(Node from, Node To, double weight, bool isEnable = true)
        {
            this.From = from;
            this.To = To;
            this.Weight = weight;
            this.Enabled = isEnable;
        }
        #endregion

        #region Methods
        public double Foward()
        {
            if (Enabled)
            {
                return From.Value * Weight;
            }

            return 0;
        }

        public object Clone()
        {
            ConnectionNode newconnection = (ConnectionNode)this.MemberwiseClone();

            return newconnection;
        }

        public void PointMutate(Random r, int min = -2, int max = 2)
        {
            Weight += r.NextDouble() * (max - min) + min;
        }

        public void Enable_Disable_Mutate()
        {
            Enabled = !Enabled;
        }
        #endregion
    }
}
