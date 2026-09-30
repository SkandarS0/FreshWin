namespace FreshWin.Core.Projects
{
    public static class ProjectLayoutWalker
    {
        public static IEnumerable<(string RelativePath, ProjectNode Node)> Walk(
            IEnumerable<ProjectNode> nodes, string prefix = "")
        {
            foreach (var node in nodes)
            {
                string relativePath = string.IsNullOrEmpty(prefix)
                    ? node.Name
                    : Path.Combine(prefix, node.Name);

                yield return (relativePath, node);

                if (node is not ProjectDirectoryNode dir)
                    continue;

                foreach (var item in Walk(dir.Children, relativePath))
                    yield return item;
            }
        }
    }
}
