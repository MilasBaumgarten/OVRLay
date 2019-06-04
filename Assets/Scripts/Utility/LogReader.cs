using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace Utility {
    public static class LogReader {
        public static async Task<(string info, string content)> ReadLogAsync(string fileName) {
            var filePath = Application.dataPath + "/TrackingFiles/" + fileName;

            // check if file exists
            if (!new FileInfo(filePath).Exists) {
                throw new FileNotFoundException();
            }

            var fileStream = File.Open(filePath, FileMode.Open, FileAccess.Read);
            var streamReader = new StreamReader(fileStream);

            var tasks = new Task<string>[2];
            tasks[0] = streamReader.ReadLineAsync();
            tasks[1] = streamReader.ReadToEndAsync();

            var data = await Task.WhenAll(tasks);

            streamReader.Close();
            fileStream.Close();

            return (data[0], data[1]);
        }

        public static async Task<(string info, string content)> ReadLogAsync(TextAsset asset) {
            var stringReader = new StringReader(asset.ToString());

            var tasks = new Task<string>[2];
            tasks[0] = stringReader.ReadLineAsync();
            tasks[1] = stringReader.ReadToEndAsync();

            var data = await Task.WhenAll(tasks);

            stringReader.Close();

            return (data[0], data[1]);
        }
    }
}