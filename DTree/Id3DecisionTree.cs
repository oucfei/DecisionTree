﻿using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.Distributions;

namespace DTree
{
    public class Id3DecisionTree
    {
        private const string TargetAttributeName = "Class";

        public const double ConfidenceLevel = 0.0;

        public TreeNode GrowTree(List<List<object>> sampleData, TreeNode parent, string parentAttributeValue)
        {
            if (IsAllSamplesTrue(sampleData))
            {
                return new TreeNode(new Attribute("True"), parent, parentAttributeValue); //leaf node
            }

            if (IsAllSamplesFalse(sampleData))
            {
                return new TreeNode(new Attribute("False"), parent, parentAttributeValue); //leaf node
            }

            if (Data.RemainingAttributes.Count == 0)
            {
                return new TreeNode(new Attribute(GetMostCommonValueForAttribute(sampleData, TargetAttributeName)), parent, parentAttributeValue); //leaf node
            }

            double gainRatio;
            string mostCommonValue;
            var bestAttribute = FindBestAttribute(sampleData, out gainRatio, out mostCommonValue);
            if (gainRatio < 1e-10)
            {
                Console.WriteLine("best attribute less than mini gainRatio: " + bestAttribute.AttributeName);
                return new TreeNode(new Attribute(GetMostCommonValueForAttribute(sampleData, TargetAttributeName)), parent, parentAttributeValue); //leaf node
            }

            var pValue = CalculatePValue(sampleData, bestAttribute, mostCommonValue);
            if (pValue > (1 - ConfidenceLevel))
            {
                Console.WriteLine("best attribute less than Confidence level with pValue: " + pValue);
                return new TreeNode(new Attribute(GetMostCommonValueForAttribute(sampleData, TargetAttributeName)), parent, parentAttributeValue); //leaf node
            }

            Console.WriteLine("Found best attribute: " + bestAttribute.AttributeName + " //gainRatio: " + gainRatio);

            var newNode =
                new TreeNode(bestAttribute, parent, parentAttributeValue)
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
                var sampleSubset = GetSamplesHaveTheValue(sampleData, value, bestAttribute, mostCommonValue);
                if (sampleSubset.Count > 0)
                {
                    Data.RemainingAttributes.Remove(
                        Data.RemainingAttributes.FirstOrDefault(a => a.AttributeName.Equals(bestAttribute.AttributeName)));

                    newNode.Children.Add(GrowTree(sampleSubset, newNode, value));
                    Data.RemainingAttributes.Add(bestAttributeCopy);                             
                }
                else
                {
                    newNode.Children.Add(new TreeNode(new Attribute(GetMostCommonValueForAttribute(sampleData, TargetAttributeName)), newNode, value)); //leaf node
                }
            }

            return newNode;
        }

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
