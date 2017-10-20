using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace DTree
{
    public class Attribute
    {
        /// <summary>
        /// Gets or sets the name of the attribute.
        /// </summary>
        public string AttributeName { get; set; }

        /// <summary>
        /// Gets the possible values of the attribute.
        /// </summary>
        public List<string> PossibleValues { get;} = new List<string>();

        /// <summary>
        /// Gets or sets the label. The label is only valid for leaf node. It should be NULL for non-leaf node.
        /// </summary>
        public string Label { get; set; }

        public Attribute(string label)
        {
            this.Label = label;
        }

        public Attribute()
        {
        }

    }
}
