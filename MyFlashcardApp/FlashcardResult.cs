using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyFlashcardApp
{
    public class FlashcardResult
    {
        private bool downPressed;
        private bool escapePressed;
        private int newIndex;
        public FlashcardResult(bool downPressed, bool escapePressed, int newIndex) 
        { 
            this.downPressed = downPressed;
            this.escapePressed = escapePressed;
            this.newIndex = newIndex;
        }
        public bool DownPressed { get { return downPressed; } }
        public bool EscapePressed { get { return escapePressed; } }
        public int NewIndex { get { return newIndex; } }
    }
}
