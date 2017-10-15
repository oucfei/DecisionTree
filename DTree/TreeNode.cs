using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTree
{
    public class TreeNode
    {
        public Attribute SplittingAttribute { get; set; }

        public List<TreeNode> Children { get; set; } 

        public TreeNode(Attribute splittingAttribute)
        {
            this.SplittingAttribute = splittingAttribute;
            this.Children = new List<TreeNode>();           
        }

        public TreeNode()
        {
        }
    }
}
