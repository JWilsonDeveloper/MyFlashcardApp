using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyFlashcardApp.Interfaces
{
    public interface IFlashcard
    {
        string Question { get; set; }
        string Answer { get; set; }
        List<string> Categories { get; set; }
        IFlashcard Clone();
    }
}
