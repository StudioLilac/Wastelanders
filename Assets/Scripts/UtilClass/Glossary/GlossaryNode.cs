using System;
using System.Collections.Generic;
using UnityEngine;

#nullable enable
namespace UtilClass
{
    /// <summary>
    /// Describes a card's rolled range. <paramref name="StatIcon"/> selects which status icon
    /// accompanies the range (e.g. damage vs defense); a null selector defaults to the damage icon.
    /// </summary>
    public record CardStats(int LowerBound, int UpperBound, Func<StatusIcons, Sprite>? StatIcon = null);

    /// <summary>
    /// Contains information about a Card, Status effect, or Keyword.
    ///
    /// You might think of a network of GlossaryNodes as a directed graph.
    /// </summary>
    public class GlossaryNode
    {
        public string Title { get; }
        public string Tooltip { get; }
        public CardStats? Stats { get; }
        public Func<StatusIcons, Sprite>? Icon { get; }
        public IReadOnlyList<GlossaryNode> Children { get; }

        public GlossaryNode(string title, string tooltip, Func<StatusIcons, Sprite>? icon = null, CardStats? stats = null, params GlossaryNode[] children)
        {
            Title = title;
            Tooltip = tooltip;
            Icon = icon;
            Stats = stats;
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