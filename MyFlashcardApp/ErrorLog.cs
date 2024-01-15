using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyFlashcardApp.Interfaces;

namespace MyFlashcardApp
{
    public class ErrorLog : IErrorLog
    {
        public static ErrorLog instance;
        public bool shouldPrint;
        public List<string> errors;
        public bool ShouldPrint
        {
            get { return shouldPrint; }
            set { shouldPrint = value; }
        }

        private ErrorLog()
        {
            errors = new List<string>();
            shouldPrint = false;
        }

        public static ErrorLog GetInstance()
        {
            if (instance == null)
            {
                instance = new ErrorLog();
            }
            return instance;
        }

        public void AddError(string error)
        {
            string callingMethod = new StackTrace().GetFrame(1).GetMethod().Name;
            errors.Add($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}: {callingMethod}:\n\t{error}");
        }
        public List<string> GetErrorLog()
        {
            return errors;
        }
    }
}
