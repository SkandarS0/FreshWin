namespace FreshWin.Core.Projects
{
    public static class ProjectLayout
    {
        public static readonly ProjectDirectoryNode CustomRegistry = new("Registry");
        public static readonly ProjectDirectoryNode CustomDrivers = new("Drivers");
        public static readonly ProjectDirectoryNode CustomUpdates = new("Updates");
        public static readonly ProjectDirectoryNode CustomAnswer = new("Answer",
            new ProjectFileNode("autounattend.xml"), new ProjectFileNode("unattend.xml"));
        public static readonly ProjectDirectoryNode Custom = new("Custom",
            CustomRegistry, CustomDrivers, CustomUpdates, CustomAnswer);

        public static readonly ProjectDirectoryNode IsoIn = new("In");
        public static readonly ProjectDirectoryNode IsoOut = new("Out");
        public static readonly ProjectDirectoryNode Iso = new("Iso", IsoIn, IsoOut);

        public static readonly ProjectDirectoryNode Logs = new("Logs");
        public static readonly ProjectDirectoryNode Mount = new("Mount");
        public static readonly ProjectDirectoryNode Source = new("Source");
        public static readonly ProjectDirectoryNode Temp = new("Temp");

        public static readonly IReadOnlyList<ProjectNode> Root = [Custom, Iso, Logs, Mount, Source, Temp];

        public static readonly ProjectPathMap Paths = ProjectPathMap.Build(Root);
    }
}
