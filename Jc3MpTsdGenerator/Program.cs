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

        public static void Main()
        {
            var clientDefinition = DownloadApiDefinition(ClientApiUrl);
            var serverDefinition = DownloadApiDefinition(ServerApiUrl);

            GenerateDefinitions(clientDefinition, serverDefinition);
        }

        private static void GenerateDefinitions(ApiDefinition clientDefinition, ApiDefinition serverDefinition)
        {
            var outputDirectory = Path.GetFullPath(Path.Combine(".", "definitions"));
            EnsureEmptyDirectory(outputDirectory);

            var clientDefinitionsOutputPath = Path.Combine(outputDirectory, ClientDefinitionFilePath);
            var clientDefinitionGenerator = new DefinitionGenerator(clientDefinition, clientDefinitionsOutputPath);
            clientDefinitionGenerator.Generate();

            AppendIfExists(ClientAppendFilePath, clientDefinitionsOutputPath);

            var serverDefinitionsOutputPath = Path.Combine(outputDirectory, ServerDefinitionFilePath);
            var serverDefinitionGenerator = new DefinitionGenerator(serverDefinition, serverDefinitionsOutputPath);
            serverDefinitionGenerator.Generate();

            AppendIfExists(ServerAppendFilePath, serverDefinitionsOutputPath);
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
