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

        public static void Main()
        {
            var clientDefinition = DownloadApiDefinition(ClientApiUrl);
            var serverDefinition = DownloadApiDefinition(ServerApiUrl);

            var outputDirectory = Path.GetFullPath(Path.Combine(".", "definitions"));
            EnsureEmptyDirectory(outputDirectory);

            var parser = new DefinitionsGenerator(clientDefinition, serverDefinition, outputDirectory);
            parser.Generate();
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
