using System.Text.Json;

namespace ConsoleQuizApplication
{
    public class HistoryManager
    {
        private readonly string _resultDir;
        private readonly string _historyFile;
        private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

        public HistoryManager()
        {
            _resultDir = Path.Combine(Directory.GetCurrentDirectory(), "result");
            _historyFile = Path.Combine(_resultDir, "history.json");

            if (!Directory.Exists(_resultDir))
                Directory.CreateDirectory(_resultDir);
        }

        public void SaveAttempt(QuizAttempt attempt)
        {
            var history = LoadAllAttempts();
            attempt.AttemptNumber = history.Count + 1;
            history.Add(attempt);

            File.WriteAllText(_historyFile, JsonSerializer.Serialize(history, JsonOptions));

            var attemptFile = Path.Combine(_resultDir, $"attempt_{attempt.AttemptNumber}.json");
            File.WriteAllText(attemptFile, JsonSerializer.Serialize(attempt, JsonOptions));
        }

        public List<QuizAttempt> LoadAllAttempts()
        {
            if (!File.Exists(_historyFile))
                return [];

            try
            {
                var json = File.ReadAllText(_historyFile);
                return JsonSerializer.Deserialize<List<QuizAttempt>>(json) ?? [];
            }
            catch
            {
                return [];
            }
        }

        public QuizAttempt? LoadAttemptDetail(int attemptNumber)
        {
            var file = Path.Combine(_resultDir, $"attempt_{attemptNumber}.json");
            if (!File.Exists(file)) return null;

            try
            {
                var json = File.ReadAllText(file);
                return JsonSerializer.Deserialize<QuizAttempt>(json);
            }
            catch
            {
                
                return null;
            }
        }

        public QuizAttempt? GetBestAttempt()
        {
            var history = LoadAllAttempts();
            return history.MaxBy(a => a.Percentage);
        }
    }
}
