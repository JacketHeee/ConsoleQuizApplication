namespace ConsoleQuizApplication
{
    public class QuestionResult
    {
        public string Content { get; set; } = "";
        public string[] Answers { get; set; } = [];
        public int SelectedIndex { get; set; }
        public int CorrectIndex { get; set; }
        public bool IsCorrect { get; set; }
    }

    public class QuizAttempt
    {
        public int AttemptNumber { get; set; }
        public DateTime DateTime { get; set; }
        public int TotalQuestions { get; set; }
        public int CorrectCount { get; set; }
        public double Percentage { get; set; }
        public string Classification { get; set; } = "";
        public List<QuestionResult> Details { get; set; } = [];
    }
}
