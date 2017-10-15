using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTree
{
    public class Id3Test
    {
        private const string TrainingDataFile = @"D:\MachineLearning\Data\testingD\testingD.arff";

        public static void Main(string[] args)
        {
            Data.ReadSampleData(TrainingDataFile);

            Id3DecisionTree tree = new Id3DecisionTree();

            TreeNode root = tree.GrowTree(Data.AllSampleData);
        }
    }
}
