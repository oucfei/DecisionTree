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

        public const double ConfidenceLevel = 0.0;

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

            //
            //Random forest approach
            //Id3DecisionTree randomForest = new Id3DecisionTree();
            //TreeNode tree1 = randomForest.GrowTree(Data.AllSampleData.GetRange(0, 20000), null, "");
            //TreeNode tree2 = randomForest.GrowTree(Data.AllSampleData.GetRange(5000, 20000), null, "");
            //TreeNode tree3 = randomForest.GrowTree(Data.AllSampleData.GetRange(10000, 20000), null, "");
            //TreeNode tree4 = randomForest.GrowTree(Data.AllSampleData.GetRange(15000, 20000), null, "");
            //TreeNode tree5 = randomForest.GrowTree(Data.AllSampleData.GetRange(20000, 20000), null, "");

            //var forest = new List<TreeNode>() {tree1, tree2, tree3, tree4, tree5 };
            //Console.WriteLine("Forest output for training data:");
            //TestWithRandomForest(forest, Data.AllSampleData);

            //Console.WriteLine("Forest output for test data:");
            //TestWithRandomForest(forest, Data.AllTestData);
            //
            //
            var diction = new Dictionary<TreeNode, Dictionary<string, int>>();
            LeafNodeSampleCount(root, diction);
            int trueMax = 0;
            int falseMax = 0;
            TreeNode trueNode = null;
            TreeNode falseNode = null;
            foreach (var key in diction.Keys)
            {
                var dicValue = diction[key];
                var label = dicValue.First().Key;
                var count = dicValue.First().Value;
                if (label.Equals("True") && count > trueMax)
                {
                    trueMax = count;
                    trueNode = key;
                }
                if (label.Equals("False") && count > falseMax)
                {
                    falseMax = count;
                    falseNode = key;
                }
            }

            Console.WriteLine("Printing true leaf to node path-----------------------");
            PrintLeafToRoot(trueNode);
            Console.WriteLine("Printing false leaf to node path-----------------------");
            PrintLeafToRoot(falseNode);

            Console.WriteLine("Press any key to finish");
            Console.ReadKey();
        }

        private static void PrintLeafToRoot(TreeNode node)
        {
            Console.WriteLine($"{node.SplittingAttribute.Label}, {node.NumberOfExamples}");

            while (node != null && node.Parent != null)
            {
                Console.Write($"{node.SplittingAttribute.AttributeName} <- {node.ParentAttributeValue}<-");
                node = node.Parent;
            }

            Console.WriteLine(" ");
        }

        private static void LeafNodeSampleCount(TreeNode root, Dictionary<TreeNode, Dictionary<string, int>> dic)
        {
            if (root.Children == null || root.Children.Count == 0 ||
                string.IsNullOrEmpty(root.SplittingAttribute.AttributeName))
            {
                var subDic = new Dictionary<string, int>() {{root.SplittingAttribute.Label, root.NumberOfExamples}};
                dic.Add(root, subDic);
            }

            foreach (var child in root.Children)
            {
                LeafNodeSampleCount(child, dic);
            }
        }

        private static void TestWithRandomForest(List<TreeNode> forest, List<List<object>> allData)
        {
            int realTrueOutputTrue = 0;
            int realFalseOutputTrue = 0;
            int realTrueOutputFalse = 0;
            int realFalseOutputFalse = 0;
            foreach (var data in allData)
            {
                var treeResults = new List<bool>();
                foreach (var treeRoot in forest)
                {
                    treeResults.Add(TestDataWithDecisionTree(treeRoot, data));
                }

                var trueCount = treeResults.Count(x => x.Equals(true));
                var falseCount = treeResults.Count - trueCount;

                var treeOutput = trueCount > falseCount;
                bool realValue = ((string)data[data.Count - 1]).Equals("True");
                if (realValue && treeOutput)
                {
                    realTrueOutputTrue++;
                }

                if (realValue && !treeOutput)
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

            Console.WriteLine($"Confidence level: {ConfidenceLevel}. True Positive: {realTrueOutputTrue}. True Negative: {realFalseOutputFalse}. False Positive: {realFalseOutputTrue}. False Negative: {realTrueOutputFalse}");
            Console.WriteLine($"Precision = {(double)realTrueOutputTrue / (realTrueOutputTrue + realFalseOutputTrue)}. Recall = {(double)realTrueOutputTrue / (realTrueOutputTrue + realTrueOutputFalse)}");
            Console.WriteLine($"Accuracy: {(double)(realTrueOutputTrue + realFalseOutputFalse) / allData.Count}");
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

        private static void TestDecisionTree(TreeNode root, List<List<object>> allData)
        {
            int realTrueOutputTrue = 0;
            int realFalseOutputTrue = 0;
            int realTrueOutputFalse = 0;
            int realFalseOutputFalse = 0;

            foreach (var data in allData)
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

            Console.WriteLine($"Confidence level: {ConfidenceLevel}. True Positive: {realTrueOutputTrue}. True Negative: {realFalseOutputFalse}. False Positive: {realFalseOutputTrue}. False Negative: {realTrueOutputFalse}");
            Console.WriteLine($"Precision = {(double)realTrueOutputTrue/(realTrueOutputTrue + realFalseOutputTrue)}. Recall = {(double)realTrueOutputTrue/(realTrueOutputTrue + realTrueOutputFalse)}");
            Console.WriteLine($"Accuracy: {(double)(realTrueOutputTrue + realFalseOutputFalse)/ allData.Count}");
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
