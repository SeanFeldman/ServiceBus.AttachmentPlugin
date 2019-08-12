using MarkdownSnippets;
using Xunit;

public class DocoUpdater
{
    [Fact]
    public void Run()
    {
        new DirectoryMarkdownProcessor(GitRepoDirectoryFinder.FindForFilePath()).Run();
    }
}