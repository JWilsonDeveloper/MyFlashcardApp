using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyFlashcardApp.Interfaces;

namespace MyFlashcardApp
{
    public class Session
    {
        public string filePath;
        public IFlashcard sampleCard;
        private IErrorLog errorLog;
        public IUserInterface userInterface;

        public IErrorLog ErrorLog
        {
            get { return errorLog; }
            set { errorLog = value; }
        }

        public Session(string filePath, IFlashcard sampleCard, IErrorLog errorLog, IUserInterface userInterface)
        {
            this.filePath = filePath;
            this.sampleCard = sampleCard;
            this.errorLog = errorLog;
            this.userInterface = userInterface;
        }

        public void RunSession()
        {
            SelectMode();
            while (true)
            {
                CardLoader cardLoader = new CardLoader(filePath, sampleCard, userInterface, errorLog);
                List<IFlashcard> flashcards = cardLoader.DetermineAndGetFlashcards();
                ShuffleFlashcards(flashcards);
                GoThroughFlashcards(flashcards);
            }
        }

        public bool SelectMode()
        {
            List<string> options = new List<string> { "Normal Mode", "Debug Mode" };
            string choice = userInterface.ChooseOption(options);
            errorLog.ShouldPrint = choice.Equals("Debug Mode");
            return choice.Equals("Debug Mode");
        }

        public void GoThroughFlashcards(List<IFlashcard> flashcards)
        {
            int i = 0;
            bool downPressed = false;
            bool showAnswer = false;
            while (true)
            {
                showAnswer = downPressed ? !showAnswer : false;
                downPressed = false;
                FlashcardResult result = userInterface.Flashcard(showAnswer, flashcards, i);
                if(result.EscapePressed)
                {
                    break;
                }
                downPressed = result.DownPressed;
                i = result.NewIndex;
            }
        }

        public void ShuffleFlashcards(List<IFlashcard> flashcards)
        {
            Random random = new Random();
            int n = flashcards.Count;
            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);
                IFlashcard value = flashcards[k];
                flashcards[k] = flashcards[n];
                flashcards[n] = value;
            }
        }
    }
}
