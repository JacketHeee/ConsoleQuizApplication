namespace ConsoleQuizApplication
{
    public class Question
    {
        public string Content { get; }
        public string[] Answers { get; set; }
        public int CorrectAnswerIndex { get; set; }

        public Question(string content, string[] answers, int correctAnswerIndex)
        {
            Content = content;
            Answers = answers;
            CorrectAnswerIndex = correctAnswerIndex;
        }

        public bool IsCorrect(int index)
        {
            return index == CorrectAnswerIndex;
        }
    }
}