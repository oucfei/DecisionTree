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

        public TreeNode Parent { get; set; }

        public string ParentAttributeValue { get; set; }

        public string SplittingAttributeMostCommonValue { get; set; }

        public TreeNode(Attribute splittingAttribute, TreeNode parent, string parentValue)
        {
            this.SplittingAttribute = splittingAttribute;
            this.Children = new List<TreeNode>();
            this.Parent = parent;
            this.ParentAttributeValue = parentValue;
        }

        public TreeNode()
        {
        }
    }
}
