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
        public string AttributeName { get; set; }

        public List<string> PossibleValues { get;} = new List<string>();

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
