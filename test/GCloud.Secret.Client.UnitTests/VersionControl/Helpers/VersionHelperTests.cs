using Xunit;

namespace GCloud.Secret.Client.VersionControl.Helpers;

public class VersionHelperTests
{
    [Fact]
    public void RuntimeVersion_ShouldFormattedWithMajorMinorAndBuildVersions_AndAddedVPrefix()
    {
        var expectedAssemblyMajorVersion = 1;
        var expectedAssemblyMinorVersion = 0;
        var expectedAssemblBuildVersion = 3;
        
        var expectedCurrentVersion = $"v{expectedAssemblyMajorVersion}.{expectedAssemblyMinorVersion}.{expectedAssemblBuildVersion}";
        
        Assert.Equal(
            expectedCurrentVersion,
            VersionHelper.RuntimeVersion);
    }
    
    [Fact]
    public void RuntimeVersion_ShouldReturnFromAssemblyVersion()
    {
        var expectedAssemblyMajorVersion = 1;
        var expectedAssemblyMinorVersion = 0;
        var expectedAssemblBuildVersion = 3;
        
        var assemblyVersion = typeof(VersionHelper).Assembly.GetName().Version!;
        var expectedCurrentVersion = $"v{assemblyVersion.Major}.{assemblyVersion.Minor}.{assemblyVersion.Build}";
        
        Assert.Equal(expectedAssemblyMajorVersion, assemblyVersion.Major);
        Assert.Equal(expectedAssemblyMinorVersion, assemblyVersion.Minor);
        Assert.Equal(expectedAssemblBuildVersion, assemblyVersion.Build);
        
        Assert.Equal(
            expectedCurrentVersion,
            VersionHelper.RuntimeVersion);

    }
}