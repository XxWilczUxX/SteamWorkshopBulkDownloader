using System;
using System.Diagnostics;
using System.Text.Json;
using System.Xml.Linq;
using Microsoft.VisualBasic;

namespace SteamWorkshopBulkDownloader
{
    class Data
    {
        private string pathToFile = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".SteamWorksopBulkDownloader", "data.json");
        public string steamCmdDirectory = String.Empty;


        public void Save()
        {
            string jsonString = JsonSerializer.Serialize(this);
            try
            {
                File.WriteAllText(pathToFile, jsonString);
            }
            catch (Exception e)
            {
                Console.WriteLine("There was an error saving cache data, don't worry it doesn't affect downloads:\n" + e.Message + "\n");
            }
        }
        public void Load()
        {
            string jsonString = File.ReadAllText(pathToFile);
            var deserialized = JsonSerializer.Deserialize<Data>(jsonString);
            if (deserialized != null && !string.IsNullOrEmpty(deserialized.steamCmdDirectory))
            {
                if (!string.IsNullOrEmpty(deserialized.steamCmdDirectory))
                    steamCmdDirectory = deserialized.steamCmdDirectory;
            }
        }
    }

    class Program
    {
        static void Main()
        {
            string cmdDir = String.Empty;
            Console.WriteLine("SteamCMD directory: ");
            cmdDir = Console.ReadLine();
            string gameID = String.Empty;
            Console.WriteLine("gameID: ");
            gameID = Console.ReadLine();

            while (true)
            {
                string workshopID = String.Empty;
                Console.WriteLine("workshopID: ");
                workshopID = Console.ReadLine();

                StartBackgroundDownloadAsync(cmdDir, gameID, workshopID);
            }
        }
        static async Task StartBackgroundDownloadAsync(string directory, string gameID, string workshopID)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "cmd",
                Arguments = $"/K cd /d \"{directory}\" && steamcmd.exe \"+login anonymous\" \"+workshop_download_item {gameID} {workshopID}\" \"+quit\" && exit",
                WorkingDirectory = directory,
                CreateNoWindow = true,
                UseShellExecute = false
            };

            using (Process process = new Process { StartInfo = startInfo })
            {
                process.Start();

                await process.WaitForExitAsync();
            }
        }
    }
}