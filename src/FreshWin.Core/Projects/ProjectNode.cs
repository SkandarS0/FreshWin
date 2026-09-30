namespace FreshWin.Core.Projects
{
    public abstract record ProjectNode(string Name);

    public sealed record ProjectDirectoryNode : ProjectNode
    {
        public IReadOnlyList<ProjectNode> Children { get; }

        public ProjectDirectoryNode(string name, params ProjectNode[] children) : base(name)
        {
            Children = children;
        }
    }

    public sealed record ProjectFileNode(string Name, string? Content = null)
        : ProjectNode(Name);
}
