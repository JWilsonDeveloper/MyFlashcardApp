using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyFlashcardApp.Interfaces
{
    public interface IUserInterface
    {
        void PrintDisplay(string[] directions);
        void PrintFlashcardProperties(IFlashcard flashcard, List<IFlashcard> allCards, bool show);
        void PrintErrorLog(List<string> errorLog);
        void PrintOptions(int selectedOptionIndex, string[] distinctOptions);
        string ChooseOption(List<string> options);
        ConsoleKeyInfo ReadKey();
        FlashcardResult Flashcard(bool showAnswer, List<IFlashcard> flashcards, int index);
    }
}
