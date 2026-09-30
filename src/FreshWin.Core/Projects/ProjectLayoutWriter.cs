namespace FreshWin.Core.Projects
{
    public static class ProjectLayoutWriter
    {
        public static void Materialize(IEnumerable<ProjectNode> nodes, string basePath)
        {
            Directory.CreateDirectory(basePath);

            foreach ((string relativePath, var node) in ProjectLayoutWalker.Walk(nodes))
            {
                string path = Path.Combine(basePath, relativePath);

                switch (node)
                {
                    case ProjectDirectoryNode:
                        Directory.CreateDirectory(path);
                        break;
                    case ProjectFileNode file:
                        File.WriteAllText(path, file.Content ?? string.Empty);
                        break;
                }
            }
        }
    }
}
