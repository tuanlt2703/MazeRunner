using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
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
        public ActivationMethod Activator;
        public double Value;
        public bool isActivated;
        public int Level;

        #region Constructors
        public Node(int id, NodeType type = NodeType.Input, ActivationMethod method = ActivationMethod.Sigmoid, int level = -1, bool activated = true)
        {
            this.ID = id;
            this.Type = type;
            this.Activator = method;
            this.Value = 0;
            this.isActivated = activated;
            this.Level = level;
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

        public double Active()
        {
            if (Activator == ActivationMethod.Sigmoid)
            {
                isActivated = true;
                return Sigmoid(this.Value);
            }
            else if (Activator == ActivationMethod.Binary)
            {
                isActivated = true;
                return BinaryActivation(this.Value);
            }

            return -1;
        }

        public Node Clone()
        {
            return new Node(this.ID, this.Type, this.Activator, this.Level, this.isActivated);
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
                To.Value += From.Value * Weight;
                return To.Value;
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

    [Serializable]
    public class BinaryLayer : NeuronDotNet.Core.Backpropagation.ActivationLayer
    {
        /// <summary>
        /// Constructs a new BinaryLayer containing specified number of neurons
        /// </summary>
        /// <param name="neuronCount">
        /// The number of neurons
        /// </param>
        /// <exception cref="ArgumentException">
        /// If <c>neuronCount</c> is zero or negative
        /// </exception>
        public BinaryLayer(int neuronCount)
            : base(neuronCount)
        {
            this.initializer = new NeuronDotNet.Core.Initializers.NguyenWidrowFunction();
        }

        /// <summary>
        /// Binary activation function
        /// </summary>
        /// <param name="input">
        /// Current input to the neuron
        /// </param>
        /// <param name="previousOutput">
        /// The previous output at the neuron
        /// </param>
        /// <returns>
        /// The activated value
        /// </returns>
        public override double Activate(double input, double previousOutput)
        {
            return input >= 0 ? 1 : 0;
        }

        /// <summary>
        /// Derivative of sigmoid function
        /// </summary>
        /// <param name="input">
        /// Current input to the neuron
        /// </param>
        /// <param name="output">
        /// Current output (activated) at the neuron
        /// </param>
        /// <returns>
        /// The result of derivative of activation function
        /// </returns>
        public override double Derivative(double input, double output)
        {
            if (output != 0)
            {
                return 0;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Deserialization constructor
        /// </summary>
        /// <param name="info">
        /// The info to deserialize
        /// </param>
        /// <param name="context">
        /// The serialization context to use
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// If <c>info</c> is <c>null</c>
        /// </exception>
        public BinaryLayer(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }

    [Serializable]
    public class Network : ISerializable
    {
        public NeuronDotNet.Core.Backpropagation.BackpropagationNetwork network;
        private int Epochs;

        #region Constructor
        public Network(int inputcount, int outputcount, int number_of_hidden_layers, List<int> HiddenLayerCount, 
            double learnrate = 0.35, int epochs = 500)
        {
            var inputLayer = new NeuronDotNet.Core.Backpropagation.SigmoidLayer(inputcount);

            List<NeuronDotNet.Core.Backpropagation.SigmoidLayer> HDLs = new List<NeuronDotNet.Core.Backpropagation.SigmoidLayer>();
            for (int i = 0; i < number_of_hidden_layers; i++)
            {
                HDLs.Add(new NeuronDotNet.Core.Backpropagation.SigmoidLayer(HiddenLayerCount[i]));
            }

            var outputlayer = new BinaryLayer(outputcount);

            int k = 0;
            new NeuronDotNet.Core.Backpropagation.BackpropagationConnector(inputLayer, HDLs[k++]).Initializer = new NeuronDotNet.Core.Initializers.NguyenWidrowFunction();
            for (; k < number_of_hidden_layers - 1; k++)
            {
                new NeuronDotNet.Core.Backpropagation.BackpropagationConnector(HDLs[k - 1], HDLs[k]).Initializer = new NeuronDotNet.Core.Initializers.RandomFunction(0d, 0.3d);
            }
            new NeuronDotNet.Core.Backpropagation.BackpropagationConnector(HDLs[HDLs.Count-1], outputlayer).Initializer = new NeuronDotNet.Core.Initializers.RandomFunction(0d, 0.3d);

            network = new NeuronDotNet.Core.Backpropagation.BackpropagationNetwork(inputLayer, outputlayer);
            network.SetLearningRate(learnrate);

            Epochs = epochs;
        }
        #endregion

        #region Methods
        public void Learn(int[,] Input, List<int> Output)
        {
            var InputCount = Input.GetLength(0) * Input.GetLength(1);
            double[] _input = new double[InputCount];
            int k = 0;
            for (int i = 0; i < Input.GetLength(0); i++)
            {
                for (int j = 0; j < Input.GetLength(1); j++)
                {
                    _input[k] = Input[i, j];
                }
            }

            double[] _output = new double[Output.Count];
            for (int i = 0; i < Output.Count; i++)
            {
                _output[i] = Output[i];
            }


            NeuronDotNet.Core.TrainingSet trainingset = new NeuronDotNet.Core.TrainingSet(_input.GetLength(0), _output.GetLength(0));
            trainingset.Add(new NeuronDotNet.Core.TrainingSample(_input, _output));

            network.Learn(trainingset, Epochs);
        }

        public double[] Run(int[,] Input)
        {
            var InputCount = Input.GetLength(0) * Input.GetLength(1);
            double[] _input = new double[InputCount];
            int k = 0;
            for (int i = 0; i < Input.GetLength(0); i++)
            {
                for (int j = 0; j < Input.GetLength(1); j++)
                {
                    _input[k] = Input[i, j];
                }
            }

            var result = network.Run(_input);
            return result;
        }
        #endregion

        #region Serialize
        //Deserialization constructor
        public Network(SerializationInfo info, StreamingContext ctxt)
        {
            network = (NeuronDotNet.Core.Backpropagation.BackpropagationNetwork)info.GetValue("Net", typeof(NeuronDotNet.Core.Backpropagation.BackpropagationNetwork));
            Epochs = (int)info.GetValue("epo", typeof(int));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Net", network);
            info.AddValue("epo", Epochs);
        }
        #endregion
    }
}
