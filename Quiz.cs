using Spectre.Console;
using Spectre.Console.Rendering;

namespace ConsoleQuizApplication
{
    public class Quiz
    {
        private readonly List<Question> _originalQuestions;
        private readonly HistoryManager _history;

        public Quiz(List<Question> questions)
        {
            _originalQuestions = questions;
            _history = new HistoryManager();
        }

        public void Run()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.CursorVisible = false;
            try { ShowMainMenu(); }
            finally { Console.CursorVisible = true; }
        }

        // ══════════════════════════════════════════════
        //  UI Helpers
        // ══════════════════════════════════════════════

        private static void RenderScreen(params IRenderable[] bodyItems)
        {
            AnsiConsole.Clear();

            var items = new List<IRenderable>
            {
                Align.Center(new Markup("[bold underline aqua]C O N S O L E   Q U I Z   A P P L I C A T I O N[/]")),
                new Text(""),
                new Rule().RuleStyle("magenta"),
            };

            items.AddRange(bodyItems);

            var panel = new Panel(new Rows(items.ToArray()))
            {
                Border = BoxBorder.Double,
                BorderStyle = new Style(Color.Magenta),
                Expand = true,
                Padding = new Padding(2, 1)
            };

            AnsiConsole.Write(panel);
        }
        private static int SelectVertical(IRenderable[] body, string[] choices)
        {
            int sel = 0;
            while (true)
            {
                var items = new List<IRenderable>(body);
                for (int i = 0; i < choices.Length; i++)
                {
                    string escaped = Markup.Escape(choices[i]);
                    items.Add(i == sel
                        ? new Markup($"    [black on aqua] ▸ {escaped} [/]")
                        : new Markup($"      {escaped}"));
                }
                RenderScreen(items.ToArray());

                switch (Console.ReadKey(true).Key)
                {
                    case ConsoleKey.UpArrow when sel > 0: sel--; break;
                    case ConsoleKey.DownArrow when sel < choices.Length - 1: sel++; break;
                    case ConsoleKey.Enter: return sel;
                }
            }
        }
        private static int SelectHorizontal(IRenderable[] body, string[] choices)
        {
            int sel = 0;
            while (true)
            {
                var items = new List<IRenderable>(body);

                var parts = new List<string>();
                for (int i = 0; i < choices.Length; i++)
                {
                    string escaped = Markup.Escape(choices[i]);
                    parts.Add(i == sel
                        ? $"[black on yellow] ▸ {escaped} [/]"
                        : $"  {escaped}  ");
                }
                items.Add(Align.Right(new Markup(string.Join("   ", parts))));

                RenderScreen(items.ToArray());

                switch (Console.ReadKey(true).Key)
                {
                    case ConsoleKey.LeftArrow when sel > 0: sel--; break;
                    case ConsoleKey.RightArrow when sel < choices.Length - 1: sel++; break;
                    case ConsoleKey.Enter: return sel;
                }
            }
        }
        private static int SelectWithSidebar(IRenderable leftContent, string[] choices)
        {
            int sel = 0;
            while (true)
            {
                var sidebarItems = new List<IRenderable>
                {
                    new Markup("[bold yellow]▸ Chọn bài làm:[/]"),
                    new Text("")
                };

                for (int i = 0; i < choices.Length; i++)
                {
                    string escaped = Markup.Escape(choices[i]);
                    sidebarItems.Add(i == sel
                        ? new Markup($"  [black on aqua] ▸ {escaped} [/]")
                        : new Markup($"    {escaped}"));
                }

                var sidebar = new Panel(new Rows(sidebarItems.ToArray()))
                {
                    Header = new PanelHeader("[bold]Danh sách[/]"),
                    Border = BoxBorder.Rounded,
                    BorderStyle = new Style(Color.Grey),
                    Expand = true,
                    Padding = new Padding(1, 1)
                };

                var leftPanel = new Panel(leftContent)
                {
                    Header = new PanelHeader("[bold]Lịch sử làm bài[/]"),
                    Border = BoxBorder.Rounded,
                    BorderStyle = new Style(Color.Grey),
                    Expand = true,
                    Padding = new Padding(1, 1)
                };

                var columns = new Columns(leftPanel, sidebar);
                columns.Expand = true;

                RenderScreen(new Text(""), columns);

                switch (Console.ReadKey(true).Key)
                {
                    case ConsoleKey.UpArrow when sel > 0: sel--; break;
                    case ConsoleKey.DownArrow when sel < choices.Length - 1: sel++; break;
                    case ConsoleKey.Enter: return sel;
                }
            }
        }

        private static string Classify(double percentage)
        {
            return (percentage / 10.0) switch
            {
                >= 9.0 => "Xuất sắc",
                >= 8.0 => "Giỏi",
                >= 7.0 => "Khá",
                >= 5.5 => "Trung bình khá",
                >= 5.0 => "Trung bình",
                >= 4.0 => "Yếu",
                _ => "Kém"
            };
        }

        private static string ScoreColor(double percentage)
        {
            return (percentage / 10.0) switch
            {
                >= 8.0 => "green",
                >= 5.0 => "yellow",
                _ => "red"
            };
        }

        private static Color ScoreToColor(double percentage) =>
            (percentage / 10.0) switch
            {
                >= 8.0 => Color.Green,
                >= 5.0 => Color.Yellow,
                _ => Color.Red
            };

        private static IRenderable[] BuildQuestionHeader(int current, int total, string content)
        {
            return
            [
                Align.Center(new Markup($"[bold yellow]{current} / {total}[/]")),
                new Rule().RuleStyle("grey"),
                new Markup($"  [bold cyan]Câu {current}: {Markup.Escape(content)}[/]"),
                new Text(""),
            ];
        }
        private static void ShowEmptyAndBack(string message)
        {
            SelectHorizontal(
                [Align.Center(new Markup($"[yellow]{message}[/]")), new Text("")],
                ["🔙 Quay lại"]);
        }

        private void ReviewAndContinue(QuizAttempt detail)
        {
            ReviewAttempt(detail);
            var action = ShowResult(detail);
            if (action == "play")
                PlayAndShowResult();
        }

        // ══════════════════════════════════════════════
        //  Main Menu
        // ══════════════════════════════════════════════

        private void ShowMainMenu()
        {
            while (true)
            {
                var body = new IRenderable[] { new Text("") };

                int choice = SelectVertical(body,
                [
                    "🎮  Chơi ngay",
                    "📋  Xem lịch sử làm bài",
                    "🏆  Kết quả cao nhất",
                    "🚪  Thoát"
                ]);

                switch (choice)
                {
                    case 0: PlayAndShowResult(); break;
                    case 1: ShowHistory(); break;
                    case 2: ShowBestResult(); break;
                    case 3:
                        AnsiConsole.Clear();
                        AnsiConsole.MarkupLine("[grey]Tạm biệt! 👋[/]");
                        return;
                }
            }
        }

        // ══════════════════════════════════════════════
        //  Play Quiz Flow
        // ══════════════════════════════════════════════

        private void PlayAndShowResult()
        {
            while (true)
            {
                var attempt = PlayQuiz();
                var action = ShowResult(attempt);
                if (action == "home") return;
                // action == "play" → loop: chơi lại
            }
        }

        private QuizAttempt PlayQuiz()
        {
            // Deep copy & shuffle
            var questions = _originalQuestions
                .Select(q => new Question(q.Content, (string[])q.Answers.Clone(), q.CorrectAnswerIndex))
                .OrderBy(_ => Random.Shared.Next())
                .ToList();

            foreach (var q in questions)
                ShuffleAnswers(q);

            int score = 0;
            var details = new List<QuestionResult>();

            for (int i = 0; i < questions.Count; i++)
            {
                var q = questions[i];
                bool isLast = i == questions.Count - 1;

                // ── Phase 1: Hiện câu hỏi + chọn đáp án ──
                var questionBody = BuildQuestionHeader(i + 1, questions.Count, q.Content);

                var answerLabels = q.Answers
                    .Select((a, idx) => $"{(char)('A' + idx)}. {a}")
                    .ToArray();

                int selIdx = SelectVertical(questionBody, answerLabels);
                bool correct = q.IsCorrect(selIdx);
                if (correct) score++;

                details.Add(new QuestionResult
                {
                    Content = q.Content,
                    Answers = (string[])q.Answers.Clone(),
                    SelectedIndex = selIdx,
                    CorrectIndex = q.CorrectAnswerIndex,
                    IsCorrect = correct
                });

                // ── Phase 2: Hiện kết quả + action ──
                var resultBody = BuildAnsweredView(i + 1, questions.Count, q, selIdx, correct);
                string actionLabel = isLast ? "📝 Nộp bài" : "▶ Tiếp tục";
                SelectHorizontal(resultBody, [actionLabel]);
            }

            double percent = Math.Round((double)score / questions.Count * 100, 2);
            var attempt = new QuizAttempt
            {
                DateTime = DateTime.Now,
                TotalQuestions = questions.Count,
                CorrectCount = score,
                Percentage = percent,
                Classification = Classify(percent),
                Details = details
            };

            _history.SaveAttempt(attempt);
            return attempt;
        }

        /// <summary>
        /// Render danh sách đáp án với màu sắc kết quả, dùng chung cho cả PlayQuiz và ReviewAttempt.
        /// </summary>
        private static List<IRenderable> RenderAnswerList(
            string[] answers, int selectedIndex, int correctIndex, bool isCorrect)
        {
            var items = new List<IRenderable>();
            for (int i = 0; i < answers.Length; i++)
            {
                string letter = $"{(char)('A' + i)}";
                string text = Markup.Escape(answers[i]);

                if (i == selectedIndex && isCorrect)
                    items.Add(new Markup($"    [black on green]  {letter}. {text}  [/]"));
                else if (i == selectedIndex && !isCorrect)
                    items.Add(new Markup($"    [white on red]  {letter}. {text}  [/]"));
                else if (i == correctIndex && !isCorrect)
                    items.Add(new Markup($"    [black on green]  {letter}. {text}  [/]"));
                else
                    items.Add(new Markup($"      {letter}. {text}"));
            }
            items.Add(new Text(""));
            items.Add(Align.Center(isCorrect
                ? new Markup("[bold green]✔ Đáp án đúng[/]")
                : new Markup("[bold red]✘ Đáp án sai[/]")));
            return items;
        }

        private static IRenderable[] BuildAnsweredView(int current, int total, Question q, int selIdx, bool correct)
        {
            var items = new List<IRenderable>(BuildQuestionHeader(current, total, q.Content));

            items.AddRange(RenderAnswerList(q.Answers, selIdx, q.CorrectAnswerIndex, correct));
            items.Add(new Rule().RuleStyle("grey"));

            return items.ToArray();
        }

        // ══════════════════════════════════════════════
        //  Result Screen
        // ══════════════════════════════════════════════

        /// <returns>"home" hoặc "play"</returns>
        private string ShowResult(QuizAttempt attempt)
        {
            while (true)
            {
                string color = ScoreColor(attempt.Percentage);

                var body = new IRenderable[]
                {
                    new Text(""),
                    Align.Center(new FigletText($"{attempt.Percentage:F2}%").Color(ScoreToColor(attempt.Percentage))),
                    Align.Center(new Markup($"[bold]Số câu đúng: {attempt.CorrectCount}/{attempt.TotalQuestions}[/]")),
                    Align.Center(new Markup($"[bold {color}]Xếp loại: {attempt.Classification}[/]")),
                    new Text(""),
                    new Rule().RuleStyle("grey")
                };

                int action = SelectVertical(body,
                [
                    "🔍  Xem lại bài làm",
                    "🏠  Quay lại trang chủ",
                    "🔄  Chơi lại"
                ]);

                switch (action)
                {
                    case 0: ReviewAttempt(attempt); break;
                    case 1: return "home";
                    case 2: return "play";
                }
            }
        }

        // ══════════════════════════════════════════════
        //  Review Mode
        // ══════════════════════════════════════════════

        private static void ReviewAttempt(QuizAttempt attempt)
        {
            for (int i = 0; i < attempt.Details.Count; i++)
            {
                var qr = attempt.Details[i];
                bool isLast = i == attempt.Details.Count - 1;

                var items = new List<IRenderable>(BuildQuestionHeader(i + 1, attempt.TotalQuestions, qr.Content));

                items.AddRange(RenderAnswerList(qr.Answers, qr.SelectedIndex, qr.CorrectIndex, qr.IsCorrect));
                items.Add(new Rule().RuleStyle("grey"));

                string actionLabel = isLast ? "📊 Xem kết quả" : "▶ Tiếp tục";
                SelectHorizontal(items.ToArray(), [actionLabel]);
            }
        }

        // ══════════════════════════════════════════════
        //  History Screen
        // ══════════════════════════════════════════════

        private void ShowHistory()
        {
            var attempts = _history.LoadAllAttempts();

            if (attempts.Count == 0)
            {
                ShowEmptyAndBack("Chưa có lịch sử làm bài.");
                return;
            }

            var table = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Grey)
                .AddColumn(new TableColumn("[bold]Lần[/]").Centered())
                .AddColumn(new TableColumn("[bold]Ngày giờ[/]").Centered())
                .AddColumn(new TableColumn("[bold]Điểm[/]").Centered())
                .AddColumn(new TableColumn("[bold]Xếp loại[/]").Centered());

            foreach (var a in attempts)
            {
                string c = ScoreColor(a.Percentage);
                table.AddRow(
                    $"{a.AttemptNumber}",
                    a.DateTime.ToString("dd/MM/yyyy HH:mm"),
                    $"[{c}]{a.Percentage:F2}%[/]",
                    $"[{c}]{a.Classification}[/]"
                );
            }

            var choices = attempts
                .Select(a => $"📄 Lần {a.AttemptNumber} – {a.Percentage:F2}%")
                .Append("🔙 Quay lại")
                .ToArray();

            int selected = SelectWithSidebar(table, choices);

            if (selected == choices.Length - 1) return;

            var detail = _history.LoadAttemptDetail(attempts[selected].AttemptNumber);
            if (detail != null)
                ReviewAndContinue(detail);
        }

        // ══════════════════════════════════════════════
        //  Best Result
        // ══════════════════════════════════════════════

        private void ShowBestResult()
        {
            var best = _history.GetBestAttempt();

            if (best == null)
            {
                ShowEmptyAndBack("Chưa có kết quả nào.");
                return;
            }

            string color = ScoreColor(best.Percentage);

            var bodyItems = new IRenderable[]
            {
                Align.Center(new Markup("[bold yellow]🏆 KẾT QUẢ CAO NHẤT 🏆[/]")),
                new Text(""),
                Align.Center(new Markup($"[bold {color}] {best.Percentage:F2}% [/]")),
                new Text(""),
                Align.Center(new Markup($"[bold]Lần làm bài: {best.AttemptNumber}[/]")),
                Align.Center(new Markup($"[bold]Ngày: {best.DateTime:dd/MM/yyyy HH:mm}[/]")),
                Align.Center(new Markup($"[bold]Số câu đúng: {best.CorrectCount}/{best.TotalQuestions}[/]")),
                Align.Center(new Markup($"[bold {color}]Xếp loại: {best.Classification}[/]")),
                new Text(""),
                new Rule().RuleStyle("grey")
            };

            int action = SelectVertical(bodyItems,
            [
                "🔍  Xem chi tiết bài làm",
                "🔙  Quay lại"
            ]);

            if (action == 0)
            {
                var detail = _history.LoadAttemptDetail(best.AttemptNumber);
                if (detail != null)
                    ReviewAndContinue(detail);
            }
        }

        // ══════════════════════════════════════════════
        //  Shuffle Helpers
        // ══════════════════════════════════════════════

        private static void ShuffleAnswers(Question question)
        {
            var paired = question.Answers
                .Select((ans, idx) => new { Text = ans, Correct = idx == question.CorrectAnswerIndex })
                .OrderBy(_ => Random.Shared.Next())
                .ToList();

            question.Answers = paired.Select(x => x.Text).ToArray();
            question.CorrectAnswerIndex = paired.FindIndex(x => x.Correct);
        }
    }
}