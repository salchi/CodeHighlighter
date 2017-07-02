﻿using SourceCollectorWPF.BusinessLogic.SyntaxHighlighting;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SourceCollectorWPF.BusinessLogic
{
    internal class FileHandler
    {
        public async Task CreateHighlightedHtmlOfContentsAsync(string sourceDirectory, string searchPattern, string skipPattern, string outputFile, IProgress<double> progress)
        {
            if (string.IsNullOrEmpty(sourceDirectory) || string.IsNullOrEmpty(outputFile))
            {
                return;
            }

            File.WriteAllText(outputFile, "");

            var files = GetAllFilesIn(sourceDirectory, searchPattern, skipPattern);
            var totalFiles = files.Count;
            var handledFiles = 0;

            using (var fileExtensionToLexerMapper = new FileExtensionToLexerMapper())
            {
                foreach (var file in files)
                {
                    var lineBreaks = $"{Environment.NewLine}{Environment.NewLine}{Environment.NewLine}";
                    var fileContent = File.ReadAllText(file);

                    File.AppendAllText(outputFile, $"{lineBreaks}{Path.GetFileName(file)}{lineBreaks}");
                    File.AppendAllText(outputFile,
                        await SyntaxHighlighterFactory.CreateNew(
                            await fileExtensionToLexerMapper.Map(Path.GetExtension(file))
                        ).HighlightCodeAsync(fileContent));

                    handledFiles++;
                    progress.Report(handledFiles * 100.0 / totalFiles);
                }
            }
        }

        private string[] PatternToArray(string pattern)
        {
            return pattern.Split('|');
        }
        public ICollection<string> GetAllFilesIn(string path, string searchPattern, string skipPattern)
        {
            var files = new List<string>();
            if (!Directory.Exists(path))
            {
                return files;
            }

            var skipPatterns = PatternToArray(string.IsNullOrEmpty(skipPattern) ? string.Empty : skipPattern);

            foreach (var pattern in PatternToArray(string.IsNullOrEmpty(searchPattern) ? "*" : searchPattern))
            {
                foreach (string file in Directory.EnumerateFiles(path, pattern, SearchOption.AllDirectories))
                {
                    if (skipPattern.Length > 0 && !skipPatterns.Any(x => file.Contains(x)))
                    {
                        files.Add(file);
                    }
                }
            }

            return files;
        }
    }
}