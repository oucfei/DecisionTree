using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTree
{
    public class Id3Test
    {
        public static void Main(string[] args)
        {
            Data.ReadSampleData(@"E:\MachineLearning\MyTestData\partSampleData.arff");

            Id3DecisionTree tree = new Id3DecisionTree();

            TreeNode root = tree.GrowTree(Data.AllSampleData);
        }
    }
}
