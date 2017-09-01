using System.IO;
using System.Linq;
using System.Net;
using Jc3MpTsdGenerator.Models;
using Newtonsoft.Json;

namespace Jc3MpTsdGenerator
{
    public class Program
    {
        private const string ClientApiUrl = "https://gitlab.nanos.io/jc3mp-docs/scripting-api-docs/raw/master/client/client.json";
        private const string ServerApiUrl = "https://gitlab.nanos.io/jc3mp-docs/scripting-api-docs/raw/master/server/server.json";

        private const string ClientDefinitionFilePath = "client.d.ts";
        private const string ServerDefinitionFilePath = "server.d.ts";

        private const string ClientAppendFilePath = "Content/append_client.txt";
        private const string ServerAppendFilePath = "Content/append_server.txt";

        private const string ClientAdditionalFilesPath = "AdditionalFiles/client";
        private const string ServerAdditionalFilesPath = "AdditionalFiles/server";

        private const string ClientOutputPath = "definitions/jcmp-client";
        private const string ServerOutputPath = "definitions/jcmp-server";

        public static void Main()
        {
            var clientDefinition = DownloadApiDefinition(ClientApiUrl);
            var serverDefinition = DownloadApiDefinition(ServerApiUrl);

            GenerateDefinitions(clientDefinition, serverDefinition);
        }

        private static void GenerateDefinitions(ApiDefinition clientDefinition, ApiDefinition serverDefinition)
        {
            var clientOutputDirectory = Path.GetFullPath(ClientOutputPath);
            EnsureEmptyDirectory(clientOutputDirectory);

            var clientDefinitionsOutputPath = Path.Combine(clientOutputDirectory, ClientDefinitionFilePath);
            var clientDefinitionGenerator = new DefinitionGenerator(clientDefinition, clientDefinitionsOutputPath);
            clientDefinitionGenerator.Generate();

            AppendIfExists(ClientAppendFilePath, clientDefinitionsOutputPath);
            if (Directory.Exists(ClientAdditionalFilesPath))
                CopyDirectoryContents(ClientAdditionalFilesPath, clientOutputDirectory);

            var serverOutputDirectory = Path.GetFullPath(ServerOutputPath);
            EnsureEmptyDirectory(serverOutputDirectory);

            var serverDefinitionsOutputPath = Path.Combine(serverOutputDirectory, ServerDefinitionFilePath);
            var serverDefinitionGenerator = new DefinitionGenerator(serverDefinition, serverDefinitionsOutputPath);
            serverDefinitionGenerator.Generate();

            AppendIfExists(ServerAppendFilePath, serverDefinitionsOutputPath);
            if (Directory.Exists(ServerAdditionalFilesPath))
                CopyDirectoryContents(ServerAdditionalFilesPath, serverOutputDirectory);
        }

        private static void CopyDirectoryContents(string sourcePath, string targetPath)
        {
            foreach (var dirPath in Directory.GetDirectories(sourcePath, "*",
                SearchOption.AllDirectories))
                Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
            
            foreach (var newPath in Directory.GetFiles(sourcePath, "*.*",
                SearchOption.AllDirectories))
                File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
        }

        private static void AppendIfExists(string appendFile, string appendToFile)
        {
            if (!File.Exists(appendFile) || !File.Exists(appendToFile)) return;

            File.AppendAllText(appendToFile, File.ReadAllText(appendFile));
        }

        private static ApiDefinition DownloadApiDefinition(string url)
        {
            using (var webClient = new WebClient())
            {
                var downloadedJson = webClient.DownloadString(url);

                return JsonConvert.DeserializeObject<ApiDefinition>(downloadedJson);
            }
        }

        private static void EnsureEmptyDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                return;
            }

            Directory.GetFiles(path).ToList().ForEach(File.Delete);
            Directory.GetDirectories(path).ToList().ForEach(d => Directory.Delete(d, true));
        }
    }
}
