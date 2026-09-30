namespace FreshWin.Core.Projects
{
    public sealed class ProjectPathMap
    {
        private readonly IReadOnlyDictionary<ProjectNode, string> _map;

        private ProjectPathMap(IReadOnlyDictionary<ProjectNode, string> map) => _map = map;

        public string Get(ProjectNode node) => _map[node];

        public static ProjectPathMap Build(IEnumerable<ProjectNode> root) =>
            new(ProjectLayoutWalker.Walk(root).ToDictionary(x => x.Node, x => x.RelativePath));
    }
}
