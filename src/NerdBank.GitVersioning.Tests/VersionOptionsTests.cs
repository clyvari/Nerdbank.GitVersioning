﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nerdbank.GitVersioning;
using Xunit;

public class VersionOptionsTests
{
    [Fact]
    public void FromVersion()
    {
        var vo = VersionOptions.FromVersion(new Version(1, 2), "-pre");
        Assert.Equal(new Version(1, 2), vo.Version.Version);
        Assert.Equal("-pre", vo.Version.Prerelease);
        Assert.Null(vo.AssemblyVersion);
        Assert.Equal(0, vo.BuildNumberOffsetOrDefault);
    }

    [Fact]
    public void Equality()
    {
        var vo1a = new VersionOptions
        {
            Version = new SemanticVersion("1.2"),
            AssemblyVersion = new VersionOptions.AssemblyVersionOptions(new Version("1.3")),
            BuildNumberOffset = 2,
        };
        var vo1b = new VersionOptions
        {
            Version = new SemanticVersion("1.2"),
            AssemblyVersion = new VersionOptions.AssemblyVersionOptions(new Version("1.3")),
            BuildNumberOffset = 2,
        };

        var vo2VaryAV = new VersionOptions
        {
            Version = new SemanticVersion("1.2"),
            AssemblyVersion = new VersionOptions.AssemblyVersionOptions(new Version("1.4")),
        };
        var vo2VaryV = new VersionOptions
        {
            Version = new SemanticVersion("1.4"),
            AssemblyVersion = new VersionOptions.AssemblyVersionOptions(new Version("1.3")),
        };
        var vo2VaryO = new VersionOptions
        {
            Version = new SemanticVersion("1.2"),
            AssemblyVersion = new VersionOptions.AssemblyVersionOptions(new Version("1.3")),
            BuildNumberOffset = 3,
        };

        Assert.Equal(vo1a, vo1b);
        Assert.NotEqual(vo2VaryAV, vo1a);
        Assert.NotEqual(vo2VaryV, vo1a);
        Assert.NotEqual(vo2VaryO, vo1a);
    }

    [Fact]
    public void AssemblyVersionOptions_Equality()
    {
        var avo1a = new VersionOptions.AssemblyVersionOptions { };
        var avo1b = new VersionOptions.AssemblyVersionOptions { };
        Assert.Equal(avo1a, avo1b);
        Assert.NotEqual(avo1a, null);

        var avo2a = new VersionOptions.AssemblyVersionOptions
        {
            Version = new Version("1.5"),
        };
        var avo2b = new VersionOptions.AssemblyVersionOptions
        {
            Version = new Version("1.5"),
        };
        var avo3 = new VersionOptions.AssemblyVersionOptions
        {
            Version = new Version("2.5"),
        };
        Assert.Equal(avo2a, avo2b);
        Assert.NotEqual(avo2a, avo1a);

        var avo4 = new VersionOptions.AssemblyVersionOptions
        {
            Precision = VersionOptions.VersionPrecision.Build,
        };
        var avo5 = new VersionOptions.AssemblyVersionOptions
        {
            Precision = VersionOptions.VersionPrecision.Minor,
        };
        Assert.NotEqual(avo4, avo5);
    }

    [Fact]
    public void CloudBuildOptions_Equality()
    {
        var cbo1a = new VersionOptions.CloudBuildOptions { };
        var cbo1b = new VersionOptions.CloudBuildOptions { };
        Assert.Equal(cbo1a, cbo1b);

        var cbo2a = new VersionOptions.CloudBuildOptions
        {
            SetVersionVariables = !cbo1a.SetVersionVariablesOrDefault,
        };
        Assert.NotEqual(cbo2a, cbo1a);

        var cbo3a = new VersionOptions.CloudBuildOptions
        {
            BuildNumber = new VersionOptions.CloudBuildNumberOptions { },
        };
        Assert.Equal(cbo3a, cbo1a); // Equal because we haven't changed defaults.

        var cbo4a = new VersionOptions.CloudBuildOptions
        {
            BuildNumber = new VersionOptions.CloudBuildNumberOptions
            {
                Enabled = !cbo1a.BuildNumberOrDefault.EnabledOrDefault,
            },
        };
        Assert.NotEqual(cbo4a, cbo1a);
    }

    [Fact]
    public void CloudBuildNumberOptions_Equality()
    {
        var bno1a = new VersionOptions.CloudBuildNumberOptions { };
        var bno1b = new VersionOptions.CloudBuildNumberOptions { };
        Assert.Equal(bno1a, bno1b);

        var bno2a = new VersionOptions.CloudBuildNumberOptions
        {
            Enabled = !bno1a.EnabledOrDefault,
        };
        Assert.NotEqual(bno1a, bno2a);

        var bno3a = new VersionOptions.CloudBuildNumberOptions
        {
            IncludeCommitId = new VersionOptions.CloudBuildNumberCommitIdOptions { },
        };
        Assert.Equal(bno1a, bno3a); // we haven't changed any defaults, even if it's non-null.

        var bno4a = new VersionOptions.CloudBuildNumberOptions
        {
            IncludeCommitId = new VersionOptions.CloudBuildNumberCommitIdOptions { When = VersionOptions.CloudBuildNumberCommitWhen.Never },
        };
        Assert.NotEqual(bno1a, bno4a);
    }

    [Fact]
    public void CloudBuildNumberCommitIdOptions_Equality()
    {
        var cio1a = new VersionOptions.CloudBuildNumberCommitIdOptions();
        cio1a.Where = cio1a.WhereOrDefault;
        cio1a.When = cio1a.WhenOrDefault;
        var cio1b = new VersionOptions.CloudBuildNumberCommitIdOptions { };
        Assert.Equal(cio1a, cio1b);

        var cio2a = new VersionOptions.CloudBuildNumberCommitIdOptions
        {
            When = (VersionOptions.CloudBuildNumberCommitWhen)((int)cio1a.WhenOrDefault + 1),
        };
        Assert.NotEqual(cio1a, cio2a);

        var cio3a = new VersionOptions.CloudBuildNumberCommitIdOptions
        {
            Where = (VersionOptions.CloudBuildNumberCommitWhere)((int)cio1a.WhereOrDefault + 1),
        };
        Assert.NotEqual(cio1a, cio3a);
    }

    [Fact]
    public void CannotWriteToDefaultInstances()
    {
        var options = new VersionOptions();
        Assert.Throws<InvalidOperationException>(() => options.AssemblyVersionOrDefault.Precision = VersionOptions.VersionPrecision.Revision);
        Assert.Throws<InvalidOperationException>(() => options.CloudBuildOrDefault.BuildNumberOrDefault.Enabled = true);
        Assert.Throws<InvalidOperationException>(() => options.CloudBuildOrDefault.BuildNumberOrDefault.IncludeCommitIdOrDefault.When = VersionOptions.CloudBuildNumberCommitWhen.Always);
        Assert.Throws<InvalidOperationException>(() => options.CloudBuildOrDefault.BuildNumberOrDefault.IncludeCommitIdOrDefault.Where = VersionOptions.CloudBuildNumberCommitWhere.BuildMetadata);
        Assert.Throws<InvalidOperationException>(() => options.CloudBuildOrDefault.SetVersionVariables = true);
        Assert.Throws<InvalidOperationException>(() => options.NuGetPackageVersionOrDefault.SemVer = 2);
        Assert.Throws<InvalidOperationException>(() => options.ReleaseOrDefault.BranchName = "BranchName");
        Assert.Throws<InvalidOperationException>(() => options.ReleaseOrDefault.VersionIncrement = VersionOptions.ReleaseVersionIncrement.Major);
    }


    [Fact]
    public void ReleaseOptions_Equality()
    {
        var releaseOptions1 = new VersionOptions.ReleaseOptions() { };
        var releaseOptions2 = new VersionOptions.ReleaseOptions() { };
        var releaseOptions3 = new VersionOptions.ReleaseOptions()
        {
            BranchName = "branchName",
            VersionIncrement = VersionOptions.ReleaseVersionIncrement.Major
        };
        var releaseOptions4 = new VersionOptions.ReleaseOptions()
        {
            BranchName = "branchName",
            VersionIncrement = VersionOptions.ReleaseVersionIncrement.Major
        };
        var releaseOptions5 = new VersionOptions.ReleaseOptions()
        {
            BranchName = "branchName",
            VersionIncrement = VersionOptions.ReleaseVersionIncrement.Minor
        };

        Assert.Equal(releaseOptions1, releaseOptions2);
        Assert.Equal(releaseOptions3, releaseOptions4);

        Assert.NotEqual(releaseOptions1, releaseOptions3);
        Assert.NotEqual(releaseOptions1, releaseOptions5);
        Assert.NotEqual(releaseOptions3, releaseOptions5);
    }
}
