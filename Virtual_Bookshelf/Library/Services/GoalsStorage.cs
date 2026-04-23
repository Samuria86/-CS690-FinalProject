using Library.Models;
using System.Text.Json;

namespace Library.Services
{
    public static class GoalsStorage
    {
        private const string GoalsFileName = "goals.dat";

        /// <summary>
        /// Get the file path for goal storage
        /// </summary>
        public static string GetFilePath() => GoalsFileName;

        /// <summary>
        /// Load all reading goals from storage
        /// </summary>
        public static List<ReadingGoal> LoadGoals()
        {
            if (!File.Exists(GoalsFileName))
            {
                return new List<ReadingGoal>();
            }

            string data = File.ReadAllText(GoalsFileName);
            if (string.IsNullOrWhiteSpace(data))
            {
                return new List<ReadingGoal>();
            }

            return JsonSerializer.Deserialize<List<ReadingGoal>>(data) ?? new List<ReadingGoal>();
        }

        /// <summary>
        /// Get the active reading goal (most recent)
        /// </summary>
        public static ReadingGoal? GetActiveGoal()
        {
            var goals = LoadGoals();
            return goals.OrderByDescending(g => g.LastModifiedDate).FirstOrDefault();
        }

        /// <summary>
        /// Save all reading goals to storage
        /// </summary>
        public static void SaveGoals(List<ReadingGoal> goals)
        {
            string jsonData = JsonSerializer.Serialize(goals, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(GoalsFileName, jsonData);
        }

        /// <summary>
        /// Add a new reading goal
        /// </summary>
        public static void AddGoal(ReadingGoal goal)
        {
            var goals = LoadGoals();
            goal.Id = goals.Count > 0 ? goals.Max(g => g.Id) + 1 : 1;
            goal.CreatedDate = DateTime.Now;
            goal.LastModifiedDate = DateTime.Now;
            goals.Add(goal);
            SaveGoals(goals);
        }

        /// <summary>
        /// Update an existing reading goal
        /// </summary>
        public static void UpdateGoal(ReadingGoal goal)
        {
            var goals = LoadGoals();
            var existingGoal = goals.FirstOrDefault(g => g.Id == goal.Id);
            if (existingGoal != null)
            {
                goal.LastModifiedDate = DateTime.Now;
                goals.Remove(existingGoal);
                goals.Add(goal);
                SaveGoals(goals);
            }
        }

        /// <summary>
        /// Delete a reading goal by ID
        /// </summary>
        public static void DeleteGoal(int goalId)
        {
            var goals = LoadGoals();
            var goalToDelete = goals.FirstOrDefault(g => g.Id == goalId);
            if (goalToDelete != null)
            {
                goals.Remove(goalToDelete);
                SaveGoals(goals);
            }
        }

        /// <summary>
        /// Clear all reading goals
        /// </summary>
        public static void ClearAllGoals()
        {
            SaveGoals([]);
        }

        /// <summary>
        /// Create or update the current active goal
        /// </summary>
        public static void SetActiveGoal(int? dailyPageGoal, int? weeklyPageGoal, int? monthlyBookGoal, int? yearlyBookGoal)
        {
            var activeGoal = GetActiveGoal();
            
            if (activeGoal != null)
            {
                activeGoal.DailyPageGoal = dailyPageGoal;
                activeGoal.WeeklyPageGoal = weeklyPageGoal;
                activeGoal.MonthlyBookGoal = monthlyBookGoal;
                activeGoal.YearlyBookGoal = yearlyBookGoal;
                activeGoal.LastModifiedDate = DateTime.Now;
                UpdateGoal(activeGoal);
            }
            else
            {
                var newGoal = new ReadingGoal
                {
                    DailyPageGoal = dailyPageGoal,
                    WeeklyPageGoal = weeklyPageGoal,
                    MonthlyBookGoal = monthlyBookGoal,
                    YearlyBookGoal = yearlyBookGoal
                };
                AddGoal(newGoal);
            }
        }
    }
}
