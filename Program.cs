using System;
using System.Diagnostics;
using System.Text.Json;
using System.Xml.Linq;
using Microsoft.VisualBasic;

namespace SteamWorkshopBulkDownloader
{
    class Data
    {
        private readonly string appDirectory = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".SteamWorksopBulkDownloader");
        private readonly string pathToFile = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".SteamWorksopBulkDownloader", "data.json");
        public string? steamCmdDirectory { get; set; } = string.Empty;

        public void Save()
        {
            ensureFileExists();
            string jsonString = JsonSerializer.Serialize(this);
            Console.WriteLine(jsonString);
            try
            {
                File.WriteAllText(pathToFile, jsonString);
            }
            catch (Exception e)
            {
                Console.WriteLine($"There was an error saving cache data, don't worry it doesn't affect downloads:\n{e.Message}\n");
            }
        }
        public void Load()
        {
            ensureFileExists();
            try
            {
                string jsonString = File.ReadAllText(pathToFile);
                var deserialized = JsonSerializer.Deserialize<Data>(jsonString);
                if (deserialized != null && !string.IsNullOrEmpty(deserialized.steamCmdDirectory))
                {
                    if (!string.IsNullOrEmpty(deserialized.steamCmdDirectory))
                        steamCmdDirectory = deserialized.steamCmdDirectory;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"There was an error loading cache data, don't worry it doesn't affect downloads:\n{e.Message}\n");
            }
        }

        private void ensureFileExists()
        {
            if (!Directory.Exists(appDirectory))
            {
                Directory.CreateDirectory(appDirectory);
                if (!File.Exists(pathToFile))
                {
                    File.Create(pathToFile).Close();
                }
            }
        }
    }

    class Program
    {
        static void Main()
        {
            var data = new Data();
            data.Load();
            string? gameID = string.Empty;
            string? workshopID = string.Empty;

            Console.WriteLine(data.steamCmdDirectory);

            if (data.steamCmdDirectory == null || data.steamCmdDirectory == string.Empty)
            {
                Console.WriteLine("(folder path) SteamCMD directory: ");
            }
            else
            {
                Console.WriteLine($"Do you want to use recent steamCMD directory? ({data.steamCmdDirectory}) y/n (default: y)");
                var ans = Console.ReadLine();
                if (ans == "n")
                {
                    data.steamCmdDirectory = string.Empty;
                    Console.WriteLine("(folder path) Input new SteamCMD directory: ");
                }
            }
            while (data.steamCmdDirectory == null || data.steamCmdDirectory == string.Empty)
            {
                data.steamCmdDirectory = Console.ReadLine();
            }

            Console.WriteLine("gameID: ");
            while (gameID == null || gameID == string.Empty)
            {
                gameID = Console.ReadLine();
            }

            while (true)
            {
                workshopID = string.Empty;
                Console.WriteLine("workshopID: ");
                while (workshopID == null || workshopID == string.Empty)
                {
                    workshopID = Console.ReadLine();
                    workshopID = unfoldURL(workshopID);
                    if (workshopID?.ToLower() == "exit")
                    {
                        data.Save();
                        return;
                    }
                }

                _ = StartBackgroundDownloadAsync(data.steamCmdDirectory, gameID, workshopID);
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

        static string unfoldURL(string? url)
        {
            if (url == null) { return "0"; } // null check

            int idStart = url.LastIndexOf('/') + 5;
            int idEnd = idStart + 1;
            for (int i = idEnd; i < url.Length; i++)
            {
                if (int.TryParse(url[i].ToString(), out _))
                {
                    idEnd = i;
                }
                else
                {
                    break;
                }
            }

            return url.Substring(idStart, idEnd - idStart + 1);
        }
    }
}