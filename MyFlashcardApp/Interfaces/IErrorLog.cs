using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyFlashcardApp.Interfaces
{
    public interface IErrorLog
    {
        void AddError(string error);
        List<string> GetErrorLog();
        bool ShouldPrint { get; set; }
    }
}
