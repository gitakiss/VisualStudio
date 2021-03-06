﻿using System.Reactive.Linq;
using System.Threading.Tasks;
using NSubstitute;
using Xunit;
using UnitTests;
using GitHub.Models;
using System;
using GitHub.Services;
using GitHub.ViewModels;
using LibGit2Sharp;

public class PullRequestCreationViewModelTests : TempFileBaseClass
{
    [Fact]
    public async Task NullDescriptionBecomesEmptyBody()
    {
        var serviceProvider = Substitutes.ServiceProvider;
        var service = new PullRequestService();
        var notifications = Substitute.For<INotificationService>();

        var host = serviceProvider.GetRepositoryHosts().GitHubHost;
        var ms = Substitute.For<IModelService>();
        host.ModelService.Returns(ms);

        var repository = new SimpleRepositoryModel("name", new GitHub.Primitives.UriString("http://github.com/github/stuff"));
        var title = "a title";

        var vm = new PullRequestCreationViewModel(host, repository, service, notifications);
        vm.SourceBranch = new BranchModel() { Name = "source" };
        vm.TargetBranch = new BranchModel() { Name = "target" };
        vm.PRTitle = title;

        await vm.CreatePullRequest.ExecuteAsync();
        var unused = ms.Received().CreatePullRequest(repository, vm.PRTitle, String.Empty, vm.SourceBranch, vm.TargetBranch);
    }

    [Fact]
    public void TemplateIsUsedIfPresent()
    {
        var serviceProvider = Substitutes.ServiceProvider;
        var service = serviceProvider.GetPullRequestsService();
        var notifications = Substitute.For<INotificationService>();

        var gitService = serviceProvider.GetGitService();
        var repo = Substitute.For<IRepository>();
        repo.Head.Returns(Substitute.For<Branch>());
        gitService.GetRepo(Arg.Any<string>()).Returns(repo);

        var host = serviceProvider.GetRepositoryHosts().GitHubHost;
        var ms = Substitute.For<IModelService>();
        var master = Substitute.For<IBranch>();
        master.Name.Returns("master");
        ms.GetBranches(Arg.Any<ISimpleRepositoryModel>()).Returns(Observable.Return(master));
        host.ModelService.Returns(ms);

        var repository = new SimpleRepositoryModel("name", new GitHub.Primitives.UriString("http://github.com/github/stuff"));
        service.GetPullRequestTemplate(repository).Returns("Test PR template");

        var vm = new PullRequestCreationViewModel(host, repository, service, notifications);
        vm.Initialize(null);

        Assert.Equal("Test PR template", vm.Description);
    }
}
