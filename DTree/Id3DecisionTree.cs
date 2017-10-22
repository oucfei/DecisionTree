using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.Distributions;

namespace DTree
{
    public class Id3DecisionTree
    {
        private const string TargetAttributeName = "Class";

        /// <summary>
        /// The recursive method to build the decision tree.
        /// </summary>
        /// <param name="sampleData">The sample data.</param>
        /// <param name="parent">The parent.</param>
        /// <param name="parentAttributeValue">The parent attribute value.</param>
        public TreeNode GrowTree(List<List<object>> sampleData, TreeNode parent, string parentAttributeValue)
        {
            //If all samples are classfied as True, stop growing the tree and return a leaf node with Label "True".
            if (IsAllSamplesTrue(sampleData))
            {
                return new TreeNode(new Attribute("True"), parent, parentAttributeValue, sampleData.Count); 
            }

            //If all samples are classfied as False, stop growing the tree and return a leaf node with Label "False".
            if (IsAllSamplesFalse(sampleData))
            {
                return new TreeNode(new Attribute("False"), parent, parentAttributeValue, sampleData.Count);
            }

            //If no attribute left, stop growing the tree and return a leaf node with label value = the most common value for the target attribute in those sample data.
            if (Data.RemainingAttributes.Count == 0)
            {
                var v = GetMostCommonValueForAttribute(sampleData, TargetAttributeName);
                var tCount = sampleData.Count(data => data[data.Count - 1].Equals("True"));
                return new TreeNode(new Attribute(v), parent, parentAttributeValue, v.Equals("True")? tCount : sampleData.Count - tCount);
            }

            //Seach for the next best attribute with the most gain ratio.
            double gainRatio;
            string mostCommonValue;
            var bestAttribute = FindBestAttribute(sampleData, out gainRatio, out mostCommonValue);
            if (gainRatio < 1e-10)
            {
                var v = GetMostCommonValueForAttribute(sampleData, TargetAttributeName);
                var tCount = sampleData.Count(data => data[data.Count - 1].Equals("True"));
                Console.WriteLine("best attribute less than mini gainRatio: " + bestAttribute.AttributeName);
                return new TreeNode(new Attribute(GetMostCommonValueForAttribute(sampleData, TargetAttributeName)), parent, parentAttributeValue, v.Equals("True") ? tCount : sampleData.Count - tCount);
            }

            //The best attribute should pass the chi square test. Otherwise it's not statistical significant so we stop splitting and return a leaf node.
            var pValue = CalculatePValue(sampleData, bestAttribute, mostCommonValue);
            if (pValue > (1 - Id3Test.ConfidenceLevel))
            {
                var v = GetMostCommonValueForAttribute(sampleData, TargetAttributeName);
                var tCount = sampleData.Count(data => data[data.Count - 1].Equals("True"));
                Console.WriteLine("best attribute less than Confidence level with pValue: " + pValue);
                return new TreeNode(new Attribute(GetMostCommonValueForAttribute(sampleData, TargetAttributeName)), parent, parentAttributeValue, v.Equals("True") ? tCount : sampleData.Count - tCount);
            }

            Console.WriteLine("Found best attribute: " + bestAttribute.AttributeName + " //gainRatio: " + gainRatio + " //P value:" + pValue);

            //Create a new decision node, the splitting attribute will be the best attribute found above.
            var newNode =
                new TreeNode(bestAttribute, parent, parentAttributeValue, -1)
                {
                    SplittingAttributeMostCommonValue = mostCommonValue
                };

            var bestAttributeCopy = new Attribute()
            {
                AttributeName = bestAttribute.AttributeName
            };

            bestAttributeCopy.PossibleValues.AddRange(bestAttribute.PossibleValues);          

            foreach (var value in bestAttribute.PossibleValues)
            {   
                //Get a subset of the training data that splitted by the current value of the best attribute.          
                var sampleSubset = GetSamplesHaveTheValue(sampleData, value, bestAttribute, mostCommonValue);
                if (sampleSubset.Count > 0)
                {
                    //Recursively grow the tree with the remaining attribute (remove the current best attribute) and the subset of the dataset.
                    Data.RemainingAttributes.Remove(
                        Data.RemainingAttributes.FirstOrDefault(a => a.AttributeName.Equals(bestAttribute.AttributeName)));

                    newNode.Children.Add(GrowTree(sampleSubset, newNode, value));
                    Data.RemainingAttributes.Add(bestAttributeCopy);                             
                }
                else
                {
                    var v = GetMostCommonValueForAttribute(sampleData, TargetAttributeName);
                    var tCount = sampleData.Count(data => data[data.Count - 1].Equals("True"));
                    //sampleSubset == 0, meaning no data left on this branch. Stop growing and return new leaf node.
                    newNode.Children.Add(new TreeNode(new Attribute(GetMostCommonValueForAttribute(sampleData, TargetAttributeName)), newNode, value, v.Equals("True") ? tCount : sampleData.Count - tCount)); //leaf node
                }
            }

            return newNode;
        }

        /// <summary>
        /// Calculates the p value from the chi square distribution.
        /// </summary>
        /// <param name="sampleData">The sample data.</param>
        /// <param name="candidateAttribute">The candidate attribute.</param>
        /// <param name="mostCommonValue">The most common value.</param>
        private double CalculatePValue(List<List<object>> sampleData, Attribute candidateAttribute, string mostCommonValue)
        {
            int totalTrueCount = GetPositiveSampleCount(sampleData);
            int totalFalseCount = sampleData.Count - totalTrueCount;
            double chiSquare = 0.0;

            foreach (var value in candidateAttribute.PossibleValues)
            {
                var trueCount = 0;
                var falseCount = 0;
                GetSplitCountForValue(sampleData, candidateAttribute, value, mostCommonValue, out trueCount, out falseCount);

                if (trueCount == 0 && falseCount == 0)
                {
                    continue;                 
                }

                double expectedTrue = totalTrueCount * (double) (trueCount + falseCount) / sampleData.Count;
                double expectedFalse = totalFalseCount * (double) (trueCount + falseCount) / sampleData.Count;

                chiSquare += (trueCount - expectedTrue) * (trueCount - expectedTrue) / expectedTrue +
                             (falseCount - expectedFalse) * (falseCount - expectedFalse) / expectedFalse;
            }

            var chiSquareCalculator = new ChiSquared(candidateAttribute.PossibleValues.Count - 1);
            var pValue = 1 - chiSquareCalculator.CumulativeDistribution(chiSquare);

            return pValue;
        }

        /// <summary>
        /// Gets the most common value for the attribute within a given sample dataset.
        /// </summary>
        /// <param name="sampleData">The sample data.</param>
        /// <param name="attributeName">Name of the attribute.</param>
        private string GetMostCommonValueForAttribute(List<List<object>> sampleData, string attributeName)
        {
            int attributeIndex = Data.AllAttributes.FindIndex(a => a.AttributeName.Equals(attributeName));

            var dictionary = new Dictionary<string, int>();
            foreach (var data in sampleData)
            {
                var value = (string)data[attributeIndex];
                if (value.Equals("?"))
                {
                    continue;
                }

                if (dictionary.ContainsKey(value))
                {
                    dictionary[value]++;
                }
                else
                {
                    dictionary.Add(value, 1);
                }
            }

            int maxCount = 0;
            string mostCommonValue = "";
            foreach (var key in dictionary.Keys)
            {
                var count = dictionary[key];
                if (count > maxCount)
                {
                    maxCount = count;
                    mostCommonValue = key;
                }
            }

            return mostCommonValue;
        }

        /// <summary>
        /// Determines whether [is all samples true] [the specified sample data].
        /// </summary>
        /// <param name="sampleData">The sample data.</param>
        /// <returns>
        ///   <c>true</c> if [is all samples true] [the specified sample data]; otherwise, <c>false</c>.
        /// </returns>
        public bool IsAllSamplesTrue(List<List<object>> sampleData)
        {
            foreach (var data in sampleData)
            {
                if (((string) data[data.Count - 1]).Equals("False"))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Determines whether [is all samples false] [the specified sample data].
        /// </summary>
        /// <param name="sampleData">The sample data.</param>
        /// <returns>
        ///   <c>true</c> if [is all samples false] [the specified sample data]; otherwise, <c>false</c>.
        /// </returns>
        public bool IsAllSamplesFalse(List<List<object>> sampleData)
        {
            foreach (var data in sampleData)
            {
                if (((string)data[data.Count - 1]).Equals("True"))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Finds the best attribute by calculating and comparing each attribute's gain ratio.
        /// </summary>
        /// <param name="sampleData">The sample data.</param>
        /// <param name="bestAttributeGainRatio">The best attribute gain ratio.</param>
        /// <param name="attributeMostCommonValue">The attribute most common value.</param>
        public Attribute FindBestAttribute(List<List<object>> sampleData, out double bestAttributeGainRatio, out string attributeMostCommonValue)
        {
            int positiveCount = GetPositiveSampleCount(sampleData);
            double entropyBeforeSplit = CalculateEntropy(positiveCount, sampleData.Count - positiveCount);

            double maxGainRatio = 0.0;
            var bestAttribute = new Attribute();
            attributeMostCommonValue = "";

            foreach (var attribute in Data.RemainingAttributes)
            {
                var mostCommonValue = GetMostCommonValueForAttribute(sampleData, attribute.AttributeName);
                double gainRatio = CalculateGainRatio(sampleData, attribute, entropyBeforeSplit, mostCommonValue);
                if (!double.IsInfinity(gainRatio) && !double.IsNaN(gainRatio) && gainRatio >= maxGainRatio)
                {
                    maxGainRatio = gainRatio;
                    bestAttribute = attribute;
                    attributeMostCommonValue = mostCommonValue;
                }
            }

            bestAttributeGainRatio = maxGainRatio;
            return bestAttribute;
        }

        /// <summary>
        /// Gets a subset of samples that have the input value for the attribute.
        /// </summary>
        /// <param name="sampleData">The sample data.</param>
        /// <param name="value">The value.</param>
        /// <param name="attribute">The attribute.</param>
        /// <param name="mostCommonValue">The most common value.</param>
        public List<List<object>> GetSamplesHaveTheValue(List<List<object>> sampleData, string value, Attribute attribute, string mostCommonValue)
        {
            int attributeIndex = Data.AllAttributes.FindIndex(a => a.AttributeName.Equals(attribute.AttributeName));
            var sampleSubset = new List<List<object>>();

            foreach (var data in sampleData)
            {
                var val = (string)data[attributeIndex];
                if (val.Equals("?"))
                {
                    val = mostCommonValue;
                }

                if (val.Equals(value))
                {
                    sampleSubset.Add(data);
                }
            }

            return sampleSubset;
        }

        /// <summary>
        /// Calculates the gain ratio.
        /// </summary>
        /// <param name="sampleData">The sample data.</param>
        /// <param name="attribute">The attribute.</param>
        /// <param name="entropyBeforeSplit">The entropy before split.</param>
        /// <param name="attributeMostCommonValue">The attribute most common value.</param>
        private double CalculateGainRatio(List<List<object>> sampleData, Attribute attribute, double entropyBeforeSplit, string attributeMostCommonValue)
        {
            var totalEntropyAfterSplit = 0.0;
            var splitInformation = 0.0;

            foreach (var value in attribute.PossibleValues)
            {
                var trueCount = 0;
                var falseCount = 0;
                GetSplitCountForValue(sampleData, attribute, value, attributeMostCommonValue, out trueCount, out falseCount);

                if (trueCount == 0 && falseCount == 0)
                {
                    continue; 
                }

                var valueEntropy = CalculateEntropy(trueCount, falseCount);
                var valueCountRatio = (double)(trueCount + falseCount) / sampleData.Count;

                totalEntropyAfterSplit += valueCountRatio * valueEntropy;

                if (!valueCountRatio.Equals(0.0))
                {
                    valueCountRatio = valueCountRatio * Math.Log(valueCountRatio, 2);
                }

                splitInformation -= valueCountRatio;
            }

            var informationGain = entropyBeforeSplit - totalEntropyAfterSplit;
            var gainRatio = informationGain / splitInformation;

            return gainRatio;
        }

        /// <summary>
        /// Gets the number of positive samples and negative samples.
        /// </summary>
        /// <param name="sampleData">The sample data.</param>
        /// <param name="attribute">The attribute.</param>
        /// <param name="attributeValue">The attribute value.</param>
        /// <param name="attributeMostCommonValue">The attribute most common value.</param>
        /// <param name="trueCount">The true count.</param>
        /// <param name="falseCount">The false count.</param>
        private void GetSplitCountForValue(List<List<object>> sampleData, Attribute attribute, string attributeValue, string attributeMostCommonValue, out int trueCount, out int falseCount)
        {
            trueCount = 0;
            falseCount = 0;

            int attributeIndex = Data.AllAttributes.FindIndex(a => a.AttributeName.Equals(attribute.AttributeName));
            foreach (var data in sampleData)
            {
                var value = ((string) data[attributeIndex]);
                if (value.Equals("?"))
                {
                    value = attributeMostCommonValue;
                }

                if (value.Equals(attributeValue))
                {
                    if (((string) data[data.Count - 1]).Equals("True"))
                    {
                        trueCount++;
                    }
                    else
                    {
                        falseCount++;
                    }
                }
            }
        }

        /// <summary>
        /// Calculates the entropy.
        /// </summary>
        /// <param name="trueCount">The true count.</param>
        /// <param name="falseCount">The false count.</param>
        private double CalculateEntropy(int trueCount, int falseCount)
        {
            var total = trueCount + falseCount;

            double trueRatio = (double) trueCount / total;
            double falseRatio = (double) falseCount / total;

            if (!trueRatio.Equals(0.0))
            {
                trueRatio = -trueRatio * Math.Log(trueRatio, 2);
            }

            if (!falseRatio.Equals(0.0))
            {
                falseRatio = -falseRatio * Math.Log(falseRatio, 2);
            }

            return trueRatio + falseRatio;
        }

        /// <summary>
        /// Gets the positive sample count.
        /// </summary>
        /// <param name="sampleData">The sample data.</param>
        private int GetPositiveSampleCount(List<List<object>> sampleData)
        {
            int count = 0;
            foreach (var data in sampleData)
            {
                if (((string) data[data.Count - 1]).Equals("True"))
                {
                    count++;
                }
            }

            return count;
        }
    }
}
