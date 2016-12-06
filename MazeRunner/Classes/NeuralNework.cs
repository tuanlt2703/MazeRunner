using System;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra.Double;
using System.Runtime.Serialization;

namespace MazeRunner.Classes
{
    /// <summary>
    /// Particularly implemented to work with NEAT
    /// </summary>
    [Serializable]
    public class NeuralNework
    {
        #region Properies
        private double[,] InputLayer;
        private double[,] OutputLayerWeights; //weights
        private List<double[,]> HiddenLayers; //weights

        public List<int> Layers;
        public List<double> WeightsVector
        {
            get
            {
                List<double> tmp = new List<double>();
                foreach (var HiddenLayer in HiddenLayers)
                {
                    for (int i = 0; i < HiddenLayer.GetLength(0); i++)
                    {
                        for (int j = 0; j < HiddenLayer.GetLength(1); j++)
                        {
                            tmp.Add(HiddenLayer[i, j]);
                        }
                    }
                }
                for (int i = 0; i < OutputLayerWeights.GetLength(0); i++)
                {
                    for (int j = 0; j < OutputLayerWeights.GetLength(1); j++)
                    {
                        tmp.Add(OutputLayerWeights[i, j]);
                    }
                }
                return tmp;
            }
        }
        public int Innovation;
        #endregion

        #region Constructors
        /// <summary>
        /// Create a NN with specific config
        /// </summary>
        /// <param name="InputSize"></param>
        /// <param name="HiddenLayers"></param>
        public NeuralNework(int InputSize, List<int> HLayers, int OutputSize = 2)
        {
            this.InputLayer = new double[1, InputSize];

            //Create random weights for hidden layer and output layer
            Random rnd = new Random(System.DateTime.Now.Millisecond);
            ////Hidden layer
            this.HiddenLayers = new List<double[,]>();
            int m = 0;
            double[,] FirstHiddenLayer = new double[InputSize, HLayers[m++]];
            for (int i = 0; i < FirstHiddenLayer.GetLength(0); i++)
            {
                for (int j = 0; j < FirstHiddenLayer.GetLength(1); j++)
                {
                    FirstHiddenLayer[i, j] = rnd.NextDouble();
                }
            }
            HiddenLayers.Add(FirstHiddenLayer);

            for (; m < HLayers.Count; m++)
            {
                double[,] mLayer = new double[HiddenLayers[m - 1].GetLength(1), HLayers[m]];
                for (int i = 0; i < mLayer.GetLength(0); i++)
                {
                    for (int j = 0; j < mLayer.GetLength(1); j++)
                    {
                        mLayer[i, j] = rnd.NextDouble();
                    }
                }
                HiddenLayers.Add(mLayer);
            }

            ////Output layer
            this.OutputLayerWeights = new double[HiddenLayers[HiddenLayers.Count - 1].GetLength(1), OutputSize];
            for (int i = 0; i < OutputLayerWeights.GetLength(0); i++)
            {
                for (int j = 0; j < OutputLayerWeights.GetLength(1); j++)
                {
                    OutputLayerWeights[i, j] = rnd.NextDouble();
                }
            }

            Layers = new List<int>();

            Layers.Add(InputSize);
            foreach (var i in HLayers)
            {
                Layers.Add(i);
            }
            Layers.Add(OutputSize);
        }

        public NeuralNework(int InputSize, int FirstHiddenLayerSize = 3, int SecondHiddenLayerSize = 10, int OutputSize = 2)
        {
            this.InputLayer = new double[1, InputSize];

            //Create random weights for hidden layer and output layer
            Random rnd = new Random(System.DateTime.Now.Millisecond);
            ////Hidden layer
            this.HiddenLayers = new List<double[,]>();
            double[,] FirstHiddenLayer = new double[InputSize, FirstHiddenLayerSize];
            for (int i = 0; i < InputSize; i++)
            {
                for (int j = 0; j < FirstHiddenLayerSize; j++)
                {
                    FirstHiddenLayer[i, j] = rnd.NextDouble();
                }
            }
            double[,] SecondHiddenLayer = new double[FirstHiddenLayerSize, SecondHiddenLayerSize];
            for (int i = 0; i < FirstHiddenLayerSize; i++)
            {
                for (int j = 0; j < SecondHiddenLayerSize; j++)
                {
                    SecondHiddenLayer[i, j] = rnd.NextDouble();
                }
            }
            HiddenLayers.Add(FirstHiddenLayer);
            HiddenLayers.Add(SecondHiddenLayer);

            ////Output layer
            this.OutputLayerWeights = new double[HiddenLayers[HiddenLayers.Count - 1].GetLength(1), OutputSize];
            for (int i = 0; i < OutputLayerWeights.GetLength(0); i++)
            {
                for (int j = 0; j < OutputLayerWeights.GetLength(1); j++)
                {
                    OutputLayerWeights[i, j] = rnd.NextDouble();
                }
            }

            Layers = new List<int>() { InputSize, FirstHiddenLayerSize, SecondHiddenLayerSize, OutputSize };
        }

        /// <summary>
        /// new product of CrossOver two Chromosome
        /// </summary>
        /// <param name="layers"></param>
        public NeuralNework(List<int> layers)
        {
            int i = 0;
            this.InputLayer = new double[1, layers[i++]];

            this.HiddenLayers = new List<double[,]>();
            for (; i < layers.Count - 1; i++)
            {
                this.HiddenLayers.Add(new double[layers[i - 1], layers[i]]);
            }

            this.OutputLayerWeights = new double[HiddenLayers[HiddenLayers.Count - 1].GetLength(1), layers[i]];
        }
        #endregion

        #region Methods
        private void GetInput(int[,] MapMatrix, int Fraction = 4)
        {
            int k = 0;
            for (int i = 0; i < MapMatrix.GetLength(0); i++)
            {
                for (int j = 0; j < MapMatrix.GetLength(1); j++)
                {
                    InputLayer[0, k++] = (MapMatrix[i, j]*1.0 + 1) / Fraction;
                }
            }
        }

        private double SigMoidActivation(double Value)
        {
            return 1.0 / (1.0 + Math.Exp(-Value));
        }

        private int Binary(double Value, double Threshold = 1.08)
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

        private double[,] Forward(double[,] SourceLayer, double[,] TargetLayer, bool last = false)
        {
            //multiply two matrix
            var source = DenseMatrix.OfArray(SourceLayer);
            var target = DenseMatrix.OfArray(TargetLayer);
            var result = source * target;

            //activate the matrix result, each row is one example, each col is one targetlayer node
            double[,] activated = new double[result.RowCount, result.ColumnCount];
            if (!last)
            {
                for (int i = 0; i < result.RowCount; i++)
                {
                    for (int j = 0; j < result.ColumnCount; j++)
                    {
                        activated[i, j] = SigMoidActivation(result[i, j]);
                    }
                }
            }
            else
            {
                for (int i = 0; i < result.RowCount; i++)
                {
                    for (int j = 0; j < result.ColumnCount; j++)
                    {
                        activated[i, j] = Binary(result[i, j]);
                    }
                }
            }
            return activated;
        }

        public void AdjustWeights()
        {

        }

        public int Run(int[,] MapMatrix)
        {
            GetInput(MapMatrix);

            //Start forwarding
            int i = 0;
            ////firstly, the input forward first hidden layer
            var Z = Forward(InputLayer, HiddenLayers[i++]);
            for (; i <= HiddenLayers.Count - 1; i++)
            {
                Z = Forward(Z, HiddenLayers[i]);
            }

            var move = Forward(Z, OutputLayerWeights, true);
            if (move[0, 1] == 0 && move[0, 0] == 0)
            {
                return 0; //Down
            }
            else if (move[0, 1] == 0 && move[0, 0] == 1)
            {
                return 1; //Up
            }
            else if(move[0, 1] == 1 && move[0, 0] == 0)
            {
                return 2; //Right
            }
            else if(move[0, 1] == 1 && move[0, 0] == 1)
            {
                return 3; //Left
            }
            return -1;
        }

        public void DevelopPhenotype(List<double> Weights)
        {
            Layers = new List<int>();
            Layers.Add(InputLayer.GetLength(1));
            int m = 0;
            foreach (var layer in HiddenLayers)
            {
                for (int i = 0; i < layer.GetLength(0); i++)
                {
                    for (int j = 0; j < layer.GetLength(1); j++)
                    {
                        layer[i, j] = Weights[m++];
                    }
                }
                Layers.Add(layer.GetLength(1));
            }
            Layers.Add(OutputLayerWeights.GetLength(1));
            for (int i = 0; i < OutputLayerWeights.GetLength(0); i++)
            {
                for (int j = 0; j < OutputLayerWeights.GetLength(1); j++)
                {
                    OutputLayerWeights[i, j] = Weights[m++];
                }
            }
        }
        #endregion

        #region Serializer
        //Deserialization constructor
        public NeuralNework(SerializationInfo info, StreamingContext ctxt)
        {
            InputLayer = (double[,])info.GetValue("Input", typeof(double[,]));
            OutputLayerWeights = (double[,])info.GetValue("Output", typeof(double[,]));
            HiddenLayers = (List<double[,]>)info.GetValue("Hiddens", typeof(List<double[,]>));
            Layers = (List<int>)info.GetValue("Layers", typeof(List<int>));
            Innovation = (int)info.GetValue("Inov", typeof(int));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Input", InputLayer);
            info.AddValue("Output", OutputLayerWeights);
            info.AddValue("Hiddens", HiddenLayers);
            info.AddValue("Layers", Layers);
            info.AddValue("Inov", Innovation);
        }
        #endregion
    }
}