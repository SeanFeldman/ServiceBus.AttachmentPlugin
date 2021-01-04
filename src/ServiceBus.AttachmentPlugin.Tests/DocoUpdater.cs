namespace ServiceBus.AttachmentPlugin.Tests
{
    using MarkdownSnippets;
    using Xunit;

    public class DocoUpdater
    {
        [Fact]
        public void Run()
        {
            new DirectoryMarkdownProcessor(GitRepoDirectoryFinder.FindForFilePath(), shouldIncludeDirectory: path => path.EndsWith("src") || path.EndsWith("src\\ServiceBus.AttachmentPlugin.Tests")).Run();
        }
    }

}