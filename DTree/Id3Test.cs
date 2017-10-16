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

        private const string TestDataFile = @"E:\MachineLearning\testingD\testingD.arff";

        //private const string TrainingDataFile = @"D:\MachineLearning\Data\SampleData.txt";

        public static void Main(string[] args)
        {
            Data.ReadData(TrainingDataFile, true);

            Id3DecisionTree tree = new Id3DecisionTree();

            TreeNode root = tree.GrowTree(Data.AllSampleData, null, "");

            Console.WriteLine("Decision tree output for training data:");
            TestDecisionTree(root, Data.AllSampleData);

            Data.ReadData(TestDataFile, false);

            Console.WriteLine("Decision tree output for test data:");
            TestDecisionTree(root, Data.AllTestData);
        }

        private static int NumberOfDecisionNodes(TreeNode root)
        {
            if (root.Children == null || root.Children.Count == 0 ||
                string.IsNullOrEmpty(root.SplittingAttribute.AttributeName))
            {
                return 0;
            }

            int sum = 0;
            foreach (var child in root.Children)
            {
                sum += NumberOfDecisionNodes(child);
            }

            return sum + 1;
        }

        private static void TestDecisionTree(TreeNode root, List<List<object>> AllData)
        {
            int realTrueOutputTrue = 0;
            int realFalseOutputTrue = 0;
            int realTrueOutputFalse = 0;
            int realFalseOutputFalse = 0;

            foreach (var data in AllData)
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

            Console.WriteLine("Confidence level: " + Id3DecisionTree.ConfidenceLevel + ". " + realFalseOutputFalse + ", " + realTrueOutputTrue + ", " + realFalseOutputTrue + ", " + realTrueOutputFalse);
            Console.WriteLine("Num of decision nodes: " + NumberOfDecisionNodes(root));
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
