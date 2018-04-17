using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Codestellation.Cepheid
{
    public class GitSemVersionTask : Task
    {
        public string WorkingDirectory { get; set; }

        [Output]
        public string Standard { get; set; }

        [Output]
        public string StandardWithDirty { get; set; }

        [Output]
        public string Full { get; set; }

        public override bool Execute()
        {
            int exitCode = Exec("git", "describe --abbrev=7 --first-parent --long --dirty --always", out string stdout, out string stderr);

            if (exitCode != 0)
            {
                Log.LogError($"Failed to define version with Git: exitCode={exitCode}, stderr='{stderr}'");
                return false;
            }

            Log.LogMessage($"Executing 'git describe' command: {stdout}");
            Parse(stdout);
            return true;
        }

        private int Exec(string command, string args, out string stdout, out string stderr)
        {
            var startInfo = new ProcessStartInfo(command, args)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            if (!string.IsNullOrWhiteSpace(WorkingDirectory))
            {
                startInfo.WorkingDirectory = WorkingDirectory;
            }

            using (var process = new Process())
            {
                process.StartInfo = startInfo;
                process.Start();
                process.WaitForExit();
                stdout = process.StandardOutput.ReadToEnd();
                stderr = process.StandardError.ReadToEnd();
                int exitCode = process.ExitCode;
                process.Close();
                return exitCode;
            }
        }

        private void Parse(string describe)
        {
            const string annotatedTagPattern = @"(?<major>[0-9]+).(?<minor>[0-9]+)-(?<patch>[0-9]+)-g(?<hash>[\w]+)-?(?<dirty>[\w]+)*";
            var parts = Regex.Match(describe, annotatedTagPattern);

            string major = "0";
            string minor = "0";
            string patch = "0";
            string hash = string.Empty;
            string dirty = string.Empty;

            if (parts.Success)
            {
                major = parts.Groups["major"].Value;
                minor = parts.Groups["minor"].Value;
                patch = parts.Groups["patch"].Value;
                hash = parts.Groups["hash"].Value;
                dirty = parts.Groups["dirty"].Value;
            }
            else
            {
                var tokens = describe.Split('-');
                hash = tokens[0].Trim();
                if (tokens.Length > 1)
                {
                    dirty = tokens[1].Trim();
                }
            }

            Standard = $"{major}.{minor}.{patch}";

            StandardWithDirty = string.IsNullOrWhiteSpace(dirty)
                ? Standard
                : $"{Standard}-{dirty}";

            Full = string.IsNullOrWhiteSpace(dirty)
                ? $"{Standard}-{hash}"
                : $"{Standard}-{hash}-{dirty}";
        }
    }
}