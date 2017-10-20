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
        /// <summary>
        /// Gets or sets the splitting attribute.
        /// </summary>
        public Attribute SplittingAttribute { get; set; }

        /// <summary>
        /// Gets or sets the children.
        /// </summary>
        public List<TreeNode> Children { get; set; }

        /// <summary>
        /// Gets or sets the parent of the node. 
        /// </summary>
        public TreeNode Parent { get; set; }

        /// <summary>
        /// Gets or sets the parent attribute value. This is the value that sorted to this node from parent node.
        /// </summary>
        public string ParentAttributeValue { get; set; }

        /// <summary>
        /// Gets or sets the splitting attribute most common value. Storing this value because in the test data if a value is missing, we should use this value.
        /// </summary>
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
