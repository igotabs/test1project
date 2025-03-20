using Docker.DotNet;
using DotNet.Testcontainers;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Images;
using IntegrationTest.Utils;
using Xunit.Abstractions;
using Environment = System.Environment;

namespace IntegrationTest.Builders;

public class HelloWorldApiImageBuilder(IMessageSink logger)
{
    public IFutureDockerImage? DockerImage { get; private set; }
	public static string ImageName { get; set; } = "helloworldapi";

	public async Task CreateHelloWorldApiFromDockerFileAsync()
    {
        logger.Log("Starting creation of Validator image");

        ImageName = "helloworldapi";
        using DockerClient? dockerClient = new DockerClientConfiguration().CreateClient();

        DockerImage = new ImageFromDockerfileBuilder()
            .WithDockerfileDirectory(CommonDirectoryPath.GetSolutionDirectory().DirectoryPath)
            .WithDockerfile("HelloWorldApi/Dockerfile")
            .WithName(ImageName)
            .WithDeleteIfExists(false)
            .WithCleanUp(false)
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

        logger.Log("Validator image has created successfully");
    }
}
