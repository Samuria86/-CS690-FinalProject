using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Virtual_Bookshelf.Library.Models;

namespace Virtual_Bookshelf.Library.Services
{
    public static class StatusLabelManager
    {
        private const string DefaultStatusLabelsFileName = "StatusLabels.json";

        /// Gets the full file path for status labels based on the library file name.
        public static string GetStatusLabelsFilePath(string libraryFileName)
        {
            var directory = Path.GetDirectoryName(libraryFileName) ?? "./";
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(libraryFileName);
            return Path.Combine(directory, $"{fileNameWithoutExtension}_StatusLabels.json");
        }

        /// Load all custom status labels for a specific library.
        public static List<StatusLabel> LoadStatusLabels(string libraryFileName)
        {
            var filePath = GetStatusLabelsFilePath(libraryFileName);

            if (!File.Exists(filePath))
            {
                return InitializeDefaultLabels(libraryFileName);
            }

            try
            {
                var json = File.ReadAllText(filePath);
                var labels = JsonSerializer.Deserialize<List<StatusLabel>>(json) ?? new List<StatusLabel>();
                return labels;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading status labels: {ex.Message}");
                return InitializeDefaultLabels(libraryFileName);
            }
        }

        /// Save all custom status labels for a specific library.
        public static void SaveStatusLabels(List<StatusLabel> labels, string libraryFileName)
        {
            var filePath = GetStatusLabelsFilePath(libraryFileName);

            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(labels, options);
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving status labels: {ex.Message}");
                throw;
            }
        }

        /// Initialize default status labels if none exist.
        private static List<StatusLabel> InitializeDefaultLabels(string libraryFileName)
        {
            var defaultLabels = new List<StatusLabel>
            {
                new StatusLabel { Name = "Not started", Description = "Haven't started reading yet" },
                new StatusLabel { Name = "In progress", Description = "Currently reading" },
                new StatusLabel { Name = "Completed", Description = "Finished reading" },
                new StatusLabel { Name = "Paused", Description = "Temporarily paused" },
                new StatusLabel { Name = "DNF", Description = "Did not finish" }
            };

            SaveStatusLabels(defaultLabels, libraryFileName);
            return defaultLabels;
        }

        /// Add a new custom status label.
        public static void AddStatusLabel(string libraryFileName, string name, string description)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Label name cannot be empty.", nameof(name));
            }

            var labels = LoadStatusLabels(libraryFileName);

            if (labels.Any(l => l.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException($"A label named '{name}' already exists.");
            }

            var newLabel = new StatusLabel
            {
                Name = name,
                Description = description
            };

            labels.Add(newLabel);
            SaveStatusLabels(labels, libraryFileName);
        }

        /// Update an existing custom status label.
        public static void UpdateStatusLabel(string libraryFileName, string oldName, string newName, string description)
        {
            if (string.IsNullOrWhiteSpace(newName))
            {
                throw new ArgumentException("Label name cannot be empty.", nameof(newName));
            }

            var labels = LoadStatusLabels(libraryFileName);
            var label = labels.FirstOrDefault(l => l.Name.Equals(oldName, StringComparison.OrdinalIgnoreCase)) ?? throw new InvalidOperationException($"Label '{oldName}' not found.");

            // Check if new name already exists (unless it's the same label)
            if (!newName.Equals(oldName, StringComparison.OrdinalIgnoreCase) &&
                labels.Any(l => l.Name.Equals(newName, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException($"A label named '{newName}' already exists.");
            }

            label.Name = newName;
            label.Description = description;
            label.DateModified = DateTime.Now;

            SaveStatusLabels(labels, libraryFileName);
        }


        /// Delete a custom status label.
        public static void DeleteStatusLabel(string libraryFileName, string name)
        {
            var labels = LoadStatusLabels(libraryFileName);
            var label = labels.FirstOrDefault(l => l.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) ?? throw new InvalidOperationException($"Label '{name}' not found.");
            labels.Remove(label);
            SaveStatusLabels(labels, libraryFileName);
        }


        /// Get a specific status label by name.
        public static StatusLabel? GetStatusLabel(string libraryFileName, string name)
        {
            var labels = LoadStatusLabels(libraryFileName);
            return labels.FirstOrDefault(l => l.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }


        /// Check if a status label exists.
        public static bool LabelExists(string libraryFileName, string name)
        {
            return GetStatusLabel(libraryFileName, name) != null;
        }

        /// Get all available status labels as a list of names.
        public static List<string> GetAllStatusLabelNames(string libraryFileName)
        {
            var labels = LoadStatusLabels(libraryFileName);
            return labels.Select(l => l.Name).ToList();
        }
    }
}
