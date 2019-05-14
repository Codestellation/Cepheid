using System;
using System.Diagnostics;
using System.IO;

namespace Codestellation.Cepheid.Tests
{
    public class TestFolder : IDisposable
    {
        public readonly string Path;

        public TestFolder()
        {
            string name = SomeString();
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), name);

            Directory.CreateDirectory(Path);
            Console.WriteLine($"{Path} created");
        }

        public void Exec(string command, string args = "")
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = command,
                Arguments = args,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WorkingDirectory = Path,
            };

            using (var process = new Process())
            {
                process.StartInfo = startInfo;
                process.OutputDataReceived += Log;
                process.ErrorDataReceived += Log;

                Console.WriteLine($"$ {command} {args}");

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();
                process.Close();
            }
        }

        public void GitInit()
        {
            Exec("git", "init");

            string name = SomeString();
            string email = $"{SomeString()}@localhost";

            Exec("git", $"config user.name '{name}'");
            Exec("git", $"config user.email '{email}'");
        }

        public void GitAddAll()
        {
            Exec("git", "add --all");
        }

        public void GitTag(string tag)
        {
            Exec("git", $"tag -a {tag} -m '{tag}'");
        }

        public void GitCommit()
        {
            Exec("git", $"commit -m '{SomeString()}'");
        }

        public void CreateRandomFile()
        {
            string name = SomeString();
            string filePath = System.IO.Path.Combine(Path, name);
            string content = SomeString();
            File.AppendAllText(filePath, content);
        }

        private static void Log(object sender, DataReceivedEventArgs e)
        {
            Console.WriteLine(e.Data);
        }

        private static string SomeString()
        {
            return Guid.NewGuid().ToString("N");
        }

        public void Dispose()
        {
            try
            {
                if (Directory.Exists(Path))
                {
                    DeleteDirectoryRecursive(Path);
                    Console.WriteLine($"{Path} deleted");
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private static void DeleteDirectoryRecursive(string path)
        {
            foreach (var file in Directory.GetFiles(path))
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            foreach (var directory in Directory.GetDirectories(path))
            {
                DeleteDirectoryRecursive(directory);
            }

            Directory.Delete(path);
        }
    }
}