using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTree
{
    public static class Data
    {
        //All 275 attributes in the training/test dataset.
        public static List<Attribute> AllAttributes { get; } = new List<Attribute>();

        //The remaining attributes in the process of growing trees. The next best attribute will be searched from this list.
        public static List<Attribute> RemainingAttributes { get; } = new List<Attribute>();

        //All data from training dataset.
        public static List<List<object>> AllSampleData { get; } = new List<List<object>>();

        //All data from test dataset.
        public static List<List<object>> AllTestData { get; } = new List<List<object>>();

        /// <summary>
        /// Reads the data. Assuming the data is in ARFF format.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="isSampleData">if set to <c>true</c> [is sample data].</param>
        public static void ReadData(string fileName, bool isSampleData)
        {
            string line;
            var file = new System.IO.StreamReader(fileName);
            var isData = false;

            //skip the first two line in the ARFF file because they're neither attributes nor data.
            for (var i = 0; i < 2; i++)
            {
                file.ReadLine();
            }

            Console.WriteLine("start reading attributes");
            while ((line = file.ReadLine()) != null)
            {
                if (line.Equals("@data"))
                {
                    isData = true;
                    line = file.ReadLine();
                    Console.WriteLine("start reading data");
                }

                if (!isData)
                {
                    PopulateAttributes(line, isSampleData);
                }
                else
                {
                    PopulateSampleData(line, isSampleData);
                }
            }

            //removing the last attribute because that's the target attribute ("Class").
            if (isSampleData)
            {
                RemainingAttributes.RemoveAt(RemainingAttributes.Count - 1);
            }
        }

        /// <summary>
        /// Populates the attributes.
        /// </summary>
        /// <param name="line">The line.</param>
        /// <param name="isSampleData">if set to <c>true</c> [is sample data].</param>
        public static void PopulateAttributes(string line, bool isSampleData)
        {
            if (!string.IsNullOrEmpty(line))
            {
                var attribute = new Attribute();

                //Parsing the attributes by removing the {} and then split by comma.
                var firstSpaceIndex = line.IndexOf(" ", StringComparison.Ordinal);
                var firstOpenParentIndex = line.IndexOf("{", StringComparison.Ordinal);
                var lastCloseParentIndex = line.LastIndexOf("}", StringComparison.Ordinal);

                attribute.AttributeName = line.Substring(firstSpaceIndex + 1, firstOpenParentIndex - firstSpaceIndex - 2).RemoveSingleQuoteIfAny();

                var valueParts = line.Substring(firstOpenParentIndex + 1,
                    lastCloseParentIndex - 1 - firstOpenParentIndex).Split(',');
                attribute.PossibleValues.AddRange(valueParts.Select(value => value.RemoveSingleQuoteIfAny()).ToList());

                if (isSampleData)
                {
                    AllAttributes.Add(attribute);
                    RemainingAttributes.Add(attribute);
                }
            }
        }

        /// <summary>
        /// Populates the sample data.
        /// </summary>
        /// <param name="line">The line.</param>
        /// <param name="isSampleData">if set to <c>true</c> [is sample data].</param>
        public static void PopulateSampleData(string line, bool isSampleData)
        {
            if (!string.IsNullOrEmpty(line))
            {
                var parts = line.Split(',');
                var data = parts.Select(value => value.RemoveSingleQuoteIfAny()).Cast<object>().ToList();

                if (isSampleData)
                {
                    AllSampleData.Add(data);
                }
                else
                {
                    AllTestData.Add(data);
                }
            }
        }
    }
}
