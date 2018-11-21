using Docker.DotNet;
using Docker.DotNet.Models;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace DockerDotNetLogsIssue
{
    public class Program : IProgress<string>, IProgress<JSONMessage>
    {
        public static Task Main(string[] args) => new Program().MainAsync(args);

        public async Task MainAsync(string[] args)
        {
            #region Boilerplate
            DockerClientConfiguration config;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                config = new DockerClientConfiguration(new Uri("npipe://./pipe/docker_engine"));
            else
                config = new DockerClientConfiguration(new Uri("unix:/var/run/docker.sock"));

            Console.WriteLine("Connecting to Docker...");
            var client = config.CreateClient();
            var images = client.Images;
            var containers = client.Containers;

            Console.WriteLine("Checking for the hello-world image...");
            var imagesFound = await images.ListImagesAsync(new ImagesListParameters { MatchName = "hello-world" });
            if (imagesFound.Count == 0)
            {
                Console.WriteLine("Pulling hello-world...");
                await images.CreateImageAsync(new ImagesCreateParameters { FromImage = "hello-world" }, null, this);
            }

            Console.WriteLine("Creating hello-world container...");
            var created = await containers.CreateContainerAsync(new CreateContainerParameters { Image = "hello-world" });
            var id = created.ID;

            Console.WriteLine("Starting & waiting for hello-world container to end...");
            await containers.StartContainerAsync(id, new ContainerStartParameters());
            await containers.WaitContainerAsync(id);
            #endregion Boilerplate

            Console.WriteLine("Logs obtained from hello-world:");
            Console.WriteLine();
            await containers.GetContainerLogsAsync(id, new ContainerLogsParameters { Follow = false, ShowStdout = true, ShowStderr = true }, CancellationToken.None, this);

            #region Boilerplate
            Console.WriteLine();
            Console.WriteLine("Removing hello-world container...");
            await containers.RemoveContainerAsync(id, new ContainerRemoveParameters());

            Console.WriteLine("Done. Press any key to exit");
            Console.ReadLine();
            #endregion Boilerplate
        }

        public void Report(JSONMessage value) { }
        public void Report(string value) => Console.WriteLine(value);
    }
}
