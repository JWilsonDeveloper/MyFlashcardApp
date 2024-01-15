using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyFlashcardApp
{
    public interface IFileReader
    {
        string[] ReadAllLines(string path);
    }
}
