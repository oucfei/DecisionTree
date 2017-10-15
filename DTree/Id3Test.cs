using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTree
{
    public class Id3Test
    {
        private const string TrainingDataFile = @"E:\MachineLearning\training_subsetD\training_subsetD.arff";

        public static void Main(string[] args)
        {
            Data.ReadSampleData(TrainingDataFile);

            Id3DecisionTree tree = new Id3DecisionTree();

            TreeNode root = tree.GrowTree(Data.AllSampleData, null, "");

            TestDecisionTreeWithSampleData(root);
        }

        private static void TestDecisionTreeWithSampleData(TreeNode root)
        {
            int realTrueOutputTrue = 0;
            int realFalseOutputTrue = 0;
            int realTrueOutputFalse = 0;
            int realFalseOutputFalse = 0;

            foreach (var data in Data.AllSampleData)
            {
                bool treeOutput = TestDataWithDecisionTree(root, data);
                bool realValue = ((string) data[data.Count - 1]).Equals("True");

                if ( realValue && treeOutput)
                {
                    realTrueOutputTrue++;
                }

                if ( realValue && !treeOutput)
                {
                    realTrueOutputFalse++;
                }

                if (!realValue && treeOutput)
                {
                    realFalseOutputTrue++;
                }

                if (!realValue && !treeOutput)
                {
                    realFalseOutputFalse++;
                }
            }

            Console.WriteLine(realFalseOutputFalse + ", " + realTrueOutputTrue + ", " + realFalseOutputTrue + ", " + realTrueOutputFalse);
        }

        private static bool TestDataWithDecisionTree(TreeNode root, List<object> data)
        {
            while (root.Children != null && root.Children.Count > 0)
            {
                var splittingAttributeIndex =
                    Data.AllAttributes.FindIndex(a => a.AttributeName.Equals(root.SplittingAttribute.AttributeName));

                var value = (string) data[splittingAttributeIndex];
                if (value.Equals("?"))
                {
                    value = root.SplittingAttributeMostCommonValue;
                }

                foreach (var child in root.Children)
                {
                    if (child.ParentAttributeValue.Equals(value))
                    {
                        root = child;
                        break;
                    }
                }
            }

            return root.SplittingAttribute.Label.Equals("True");
        }
    }
}
