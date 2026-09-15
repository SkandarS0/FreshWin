namespace FreshWin.Core.Projects
{
    public enum ProjectLayoutIssueKind
    {
        MissingDirectory,
        MissingFile,
        WrongType
    }

    public sealed record ProjectLayoutIssue(string RelativePath, ProjectLayoutIssueKind Kind);

    public static class ProjectLayoutValidator
    {
        public static IReadOnlyList<ProjectLayoutIssue> Validate(IEnumerable<ProjectNode> nodes, string basePath)
        {
            var issues = new List<ProjectLayoutIssue>();

            foreach (var (relativePath, node) in ProjectLayoutWalker.Walk(nodes))
            {
                var path = Path.Combine(basePath, relativePath);

                switch (node)
                {
                    case ProjectDirectoryNode when File.Exists(path):
                        issues.Add(new(relativePath, ProjectLayoutIssueKind.WrongType));
                        break;
                    case ProjectDirectoryNode when !Directory.Exists(path):
                        issues.Add(new(relativePath, ProjectLayoutIssueKind.MissingDirectory));
                        break;
                    case ProjectFileNode when Directory.Exists(path):
                        issues.Add(new(relativePath, ProjectLayoutIssueKind.WrongType));
                        break;
                    case ProjectFileNode when !File.Exists(path):
                        issues.Add(new(relativePath, ProjectLayoutIssueKind.MissingFile));
                        break;
                }
            }

            return issues;
        }
    }
}
