using System.Linq;
using Content.Server.NodeContainer.Nodes;

namespace Content.Server.NodeContainer.NodeGroups
{
    /// <summary>
    ///     Maintains a collection of <see cref="Node"/>s, and performs operations requiring a list of
    ///     all connected <see cref="Node"/>s.
    /// </summary>
    public interface INodeGroup
    {
        bool Remaking { get; }

        /// <summary>
        ///     The list of nodes currently in this group.
        /// </summary>
        IReadOnlyList<Node> Nodes { get; }

        void Create(NodeGroupID groupId);

        void Initialize(Node sourceNode, IEntityManager entMan);

        void RemoveNode(Node node);

        void LoadNodes(List<Node> groupNodes);

        // In theory, the SS13 curse ensures this method will never be called.
        void AfterRemake(IEnumerable<IGrouping<INodeGroup?, Node>> newGroups);

        /// <summary>
        ///     Return any additional data to display for the node-visualizer debug overlay.
        /// </summary>
        string? GetDebugData();
    }

    [NodeGroup(NodeGroupID.Default, NodeGroupID.WireNet)]
    [Virtual]
    public class BaseNodeGroup : INodeGroup
    {
        public bool Remaking { get; set; }

        IReadOnlyList<Node> INodeGroup.Nodes => Nodes;

        /// <summary>
        ///     The list of nodes in this group.
        /// </summary>
        [ViewVariables] public readonly List<Node> Nodes = new();

        [ViewVariables] public int NodeCount => Nodes.Count;

        /// <summary>
        ///     Debug variable to indicate that this NodeGroup should not be being used by anything.
        /// </summary>
        [ViewVariables]
        public bool Removed { get; set; } = false;

        /// <summary>
        ///     Network ID of this group for client-side debug visualization of nodes.
        /// </summary>
        [ViewVariables]
        public int NetId;

        [ViewVariables]
        public NodeGroupID GroupId { get; private set; }

        public void Create(NodeGroupID groupId)
        {
            GroupId = groupId;
        }

        public virtual void Initialize(Node sourceNode, IEntityManager entMan)
        {
        }

        /// <summary>
        ///     Called when a node has been removed from this group via deletion of the node.
        /// </summary>
        /// <remarks>
        ///     Note that this always still results in a complete remake of the group later,
        ///     but hooking this method is good for book keeping.
        /// </remarks>
        /// <param name="node">The node that was deleted.</param>
        public virtual void RemoveNode(Node node)
        {
        }

        /// <summary>
        ///     Called to load this newly created group up with new nodes.
        /// </summary>
        /// <param name="groupNodes">The new nodes for this group.</param>
        public virtual void LoadNodes(
            List<Node> groupNodes)
        {
            Nodes.AddRange(groupNodes);
        }

        /// <summary>
        ///     Called after the nodes in this group have been made into one or more new groups.
        /// </summary>
        /// <remarks>
        ///     Use this to split in-group data such as pipe gas mixtures into newly split nodes.
        /// </remarks>
        /// <param name="newGroups">A list of new groups for this group's former nodes.</param>
        public virtual void AfterRemake(IEnumerable<IGrouping<INodeGroup?, Node>> newGroups) { }

        public virtual string? GetDebugData()
        {
            return null;
        }
    }
}
