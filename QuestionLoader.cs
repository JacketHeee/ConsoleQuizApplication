using System.Text.Json;

namespace ConsoleQuizApplication
{
    public static class QuestionLoader
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        /// <summary>
        /// Đọc danh sách câu hỏi từ file JSON.
        /// File phải chứa mảng các object có cấu trúc:
        /// { "content": "...", "answers": ["..."], "correctAnswerIndex": 0 }
        /// </summary>
        public static List<Question> LoadFromJson(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Không tìm thấy file câu hỏi: {filePath}");

            var json = File.ReadAllText(filePath);
            var data = JsonSerializer.Deserialize<List<QuestionDto>>(json, JsonOptions)
                       ?? throw new InvalidDataException("File câu hỏi rỗng hoặc không hợp lệ.");

            return data.Select(d => new Question(d.Content, d.Answers, d.CorrectAnswerIndex)).ToList();
        }

        private class QuestionDto
        {
            public string Content { get; set; } = "";
            public string[] Answers { get; set; } = [];
            public int CorrectAnswerIndex { get; set; }
        }
    }
}
