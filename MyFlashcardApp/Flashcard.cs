using MyFlashcardApp.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyFlashcardApp
{
    public class Flashcard : IFlashcard
    {
        public string Question { get; set; }
        public string Answer { get; set; }
        public List<string> Categories { get; set; }

        public IFlashcard Clone()
        {
            return new Flashcard
            {
                Question = this.Question,
                Answer = this.Answer,
                Categories = this.Categories
            };
        }
    }
}
