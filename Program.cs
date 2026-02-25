namespace ConsoleQuizApplication
{
    class Program
    {
        static void Main()
        {
            var questionsFile = Path.Combine(Directory.GetCurrentDirectory(), "questions.json");
            var questions = QuestionLoader.LoadFromJson(questionsFile);

            var quiz = new Quiz(questions);
            quiz.Run();
        }
    }
}