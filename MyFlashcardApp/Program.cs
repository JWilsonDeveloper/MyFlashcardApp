using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyFlashcardApp.Interfaces;

namespace MyFlashcardApp
{
    class Program
    {
        static void Main(string[] args)
        {
            string filePath = "CardInfo.csv";
            IFlashcard flashcard = new Flashcard();
            IErrorLog errorLog = ErrorLog.GetInstance();
            IUserInterface userInterface = UserInterface.GetInstance(errorLog);

            Session session = new Session(filePath, flashcard, errorLog, userInterface);

            session.RunSession();
        }
    }
}
