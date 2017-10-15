﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTree
{
    public static class Data
    {
        public static List<Attribute> AllAttributes { get; } = new List<Attribute>();

        public static List<Attribute> RemainingAttributes { get; } = new List<Attribute>();

        public static List<List<object>> AllSampleData { get; } = new List<List<object>>();

        public static void ReadSampleData(string fileName)
        {
            string line;
            var file = new System.IO.StreamReader(fileName);
            var isData = false;

            //skip the first two line in the ARFF file because they're neither attributes nor data.
            for (var i = 0; i < 2; i++)
            {
                file.ReadLine();
            }

            while ((line = file.ReadLine()) != null)
            {
                if (line.Equals("@data"))
                {
                    isData = true;
                    line = file.ReadLine();
                }

                if (!isData)
                {
                    PopulateAttributes(line);
                }
                else
                {
                    PopulateSampleData(line);
                }
            }

            //removing the last attribute because that's the target attribute.
            AllAttributes.RemoveAt(AllAttributes.Count - 1);
            RemainingAttributes.RemoveAt(RemainingAttributes.Count - 1);
        }

        public static void PopulateAttributes(string line)
        {
            if (!string.IsNullOrEmpty(line))
            {
                var attribute = new Attribute();

                var firstSpaceIndex = line.IndexOf(" ", StringComparison.Ordinal);
                var firstOpenParentIndex = line.IndexOf("{", StringComparison.Ordinal);
                var lastCloseParentIndex = line.LastIndexOf("}", StringComparison.Ordinal);

                attribute.AttributeName = line.Substring(firstSpaceIndex + 1, firstOpenParentIndex - firstSpaceIndex - 2).RemoveSingleQuoteIfAny();

                var valueParts = line.Substring(firstOpenParentIndex + 1,
                    lastCloseParentIndex - 1 - firstOpenParentIndex).Split(',');
                attribute.PossibleValues.AddRange(valueParts.Select(value => value.RemoveSingleQuoteIfAny()).ToList());

                AllAttributes.Add(attribute);
                RemainingAttributes.Add(attribute);
            }
        }

        public static void PopulateSampleData(string line)
        {
            if (!string.IsNullOrEmpty(line))
            {
                var parts = line.Split(',');
                var data = parts.Select(value => value.RemoveSingleQuoteIfAny()).Cast<object>().ToList();

                AllSampleData.Add(data);
            }
        }
    }
}