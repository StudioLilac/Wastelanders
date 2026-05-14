using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace UtilClass
{
    /// <summary>
    /// Contains information about a Card, Status effect, or Keyword.
    ///
    /// You might think of a network of GlossaryNodes as a directed graph.
    /// </summary>
    public class GlossaryNode
    {
        public string Title { get; }
        public string Tooltip { get; }
        [CanBeNull] public Sprite Icon { get; }
        public IReadOnlyList<GlossaryNode> Children { get; }
        
        public GlossaryNode(string title, string tooltip, Sprite icon = null, params GlossaryNode[] children)
        {
            Title = title;
            Tooltip = tooltip;
            Icon = icon;
            Children = children;
        }

        /// <summary>
        /// Returns a list of all the children of the given parent, recursively.
        ///
        /// The order of the children is determined by BFS (closer children come first).
        /// There is no guarantee on the order between children that are the same distance
        /// from the parent.
        ///
        /// The first element of the list is the parent itself.
        /// </summary>
        public static List<GlossaryNode> GetAllSubchildren(GlossaryNode parent)
        {
            Queue<GlossaryNode> queue = new Queue<GlossaryNode>();
            List<GlossaryNode> visited = new List<GlossaryNode>();
            queue.Enqueue(parent);

            while (queue.Count > 0)
            {
                GlossaryNode curr = queue.Dequeue();
                if (visited.Contains(curr)) continue;

                visited.Add(curr);
                foreach (GlossaryNode child in curr.Children)
                {
                    queue.Enqueue(child);
                }
            }

            return visited;
        }
    }
}