using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyFlashcardApp
{
    public class FileReader : IFileReader
    {
        public string[] ReadAllLines(string path)
        {
            return File.ReadAllLines(path);
        }
    }
}
