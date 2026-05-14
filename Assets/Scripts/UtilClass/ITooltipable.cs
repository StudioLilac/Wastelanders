using System.Collections.Generic;

namespace UtilClass
{
    public interface ITooltipable
    {
        public string Tooltip { get; }
        public IReadOnlyList<ITooltipable> Children { get; }

        /// <summary>
        /// Returns a list of all the children of the given parent, recursively.
        ///
        /// The order of the children is determined by BFS (closer children come first).
        /// There is no guarantee on the order between children that are the same distance
        /// from the parent.
        ///
        /// The first element of the list is the parent itself.
        /// </summary>
        public static List<ITooltipable> GetAllSubchildren(ITooltipable parent)
        {
            Queue<ITooltipable> queue = new Queue<ITooltipable>();
            List<ITooltipable> visited = new List<ITooltipable>();
            queue.Enqueue(parent);

            while (queue.Count > 0)
            {
                ITooltipable curr = queue.Dequeue();
                if (visited.Contains(curr)) continue;

                visited.Add(curr);
                foreach (ITooltipable child in curr.Children)
                {
                    queue.Enqueue(child);
                }
            }

            return visited;
        }
    }
}