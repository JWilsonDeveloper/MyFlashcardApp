using MyFlashcardApp.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyFlashcardApp
{
    public class UserInterface : IUserInterface
    {
        private static UserInterface instance;
        private IErrorLog errorLog;

        private UserInterface(IErrorLog errorLog) {
            this.errorLog = errorLog;
        }

        public static UserInterface GetInstance(IErrorLog errorLog)
        {
            if (instance == null)
            {
                instance = new UserInterface(errorLog);
            }
            return instance;
        }

        public void PrintDisplay(string[] directions)
        {
            Console.Clear();

            if (errorLog.ShouldPrint) { PrintErrorLog(errorLog.GetErrorLog()); }

            Console.WriteLine("Directions:");
            foreach (string direction in directions)
            {
                Console.WriteLine("\n\t" + direction);
            }
            Console.WriteLine();
        }

        public void PrintFlashcardProperties(IFlashcard flashcard, List<IFlashcard> allCards, bool show)
        {
            int currentIndex = allCards.IndexOf(flashcard);
            Console.WriteLine($"Current Card:\n\n\t{currentIndex + 1} / {allCards.Count}\n");
            Console.WriteLine($"Question:\n\n\t{flashcard.Question}");
            Console.WriteLine();

            for (int i = 0; i < flashcard.Categories.Count; i++)
            {
                string label = i == 0 ? "Category" : "Subcategory" + i.ToString();
                Console.WriteLine($"{label}:\n\n\t{flashcard.Categories[i]}");
                Console.WriteLine();
            }

            if (show)
            {
                Console.Write($"Answer:\n\n\t{allCards[currentIndex].Answer}");
            }
        }

        public void PrintErrorLog(List<string> errorLog)
        {
            Console.WriteLine("Error Log:");
            foreach (string error in errorLog)
            {
                Console.WriteLine("\t" + error);
            }
            Console.WriteLine();
        }

        public void PrintOptions(int selectedOptionIndex, string[] distinctOptions)
        {
            Console.WriteLine("Options:\n");

            for (int i = 0; i < distinctOptions.Length; i++)
            {
                if (i == selectedOptionIndex)
                {
                    Console.BackgroundColor = ConsoleColor.Gray;
                    Console.ForegroundColor = ConsoleColor.Black;
                }

                Console.WriteLine("\t" + distinctOptions[i] + "\n");

                Console.ResetColor();
            }
        }

        public string ChooseOption(List<string> options)
        {
            Console.CursorVisible = false;
            string[] distinctOptions = options.Distinct().ToArray();
            int selectedOptionIndex = 0;
            while (true)
            {
                PrintDisplay(new string[] { "Press enter to make a selection:" });
                PrintOptions(selectedOptionIndex, distinctOptions);

                ConsoleKeyInfo keyInfo = Console.ReadKey(true);

                if (keyInfo.Key == ConsoleKey.UpArrow && selectedOptionIndex > 0)
                {
                    selectedOptionIndex--;
                }
                else if (keyInfo.Key == ConsoleKey.DownArrow && selectedOptionIndex < distinctOptions.Length - 1)
                {
                    selectedOptionIndex++;
                }
                else if (keyInfo.Key == ConsoleKey.Enter)
                {
                    break;
                }
            }
            Console.CursorVisible = true;
            return distinctOptions[selectedOptionIndex];
        }

        public FlashcardResult Flashcard(bool showAnswer, List<IFlashcard> flashcards, int index)
        {
            bool downPressed = false;
            bool escapePressed = false;
            int newIndex = index;

            PrintDisplay(new string[] { "Esc to return to the main menu.", "Left/right keys to cycle through the cards.", "Down key to reveal/hide the answer." });
            PrintFlashcardProperties(flashcards[index], flashcards, showAnswer);

            ConsoleKeyInfo keyInfo = ReadKey();

            switch (keyInfo.Key)
            {
                case ConsoleKey.LeftArrow:
                    newIndex = (index == 0) ? flashcards.Count - 1 : index - 1;
                    break;
                case ConsoleKey.RightArrow:
                    newIndex = (index == flashcards.Count - 1) ? 0 : index + 1;
                    break;
                case ConsoleKey.DownArrow:
                    downPressed = true;
                    break;
                case ConsoleKey.Escape:
                    escapePressed = true;
                    break;
            }
            return new FlashcardResult(downPressed, escapePressed, newIndex);
        }

        public ConsoleKeyInfo ReadKey()
        {
            return Console.ReadKey();
        }
    }
}
