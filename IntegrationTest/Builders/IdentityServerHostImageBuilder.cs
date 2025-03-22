using Docker.DotNet;
using Docker.DotNet.Models;
using DotNet.Testcontainers;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Images;
using IntegrationTest.Utils;
using Xunit.Abstractions;
using Environment = System.Environment;

namespace IntegrationTest.Builders;

public class IdentityServerHostImageBuilder(IMessageSink logger)
{
    public IFutureDockerImage? DockerImage { get; private set; }
	public static string ImageName { get; set; } = "identityserverhost";

	public async Task CreateIdentityServerHostFromDockerFileAsync()
    {
        logger.Log("Starting creation of Validator image");

        ImageName = "identityserverhost";
		using DockerClient? dockerClient = new DockerClientConfiguration().CreateClient();
		IList<ImagesListResponse>? images =
			await dockerClient.Images.ListImagesAsync(new ImagesListParameters { All = true });

		if (images == null || !images.Any(i => i.RepoTags.Any(tag => tag.Contains(ImageName))))
		{
			DockerImage = new ImageFromDockerfileBuilder()
            .WithDockerfileDirectory(CommonDirectoryPath.GetSolutionDirectory().DirectoryPath)
            .WithDockerfile("IdentityServer/Dockerfile")
            .WithName(ImageName)
            .WithDeleteIfExists(true)
            .WithCleanUp(true)
            .WithLogger(ConsoleLogger.Instance)
            .Build();

        try
        {
            await DockerImage.CreateAsync();
        }
        catch (Exception e)
        {
            logger.Log("Validator image creation has failed");
            logger.Log(e.Message);
            throw;
        }
		}
		else
		{
			logger.Log("Reusing already created Identity Server image");
		}
		logger.Log("Validator image has created successfully");
    }
}
