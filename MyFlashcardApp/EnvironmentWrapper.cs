using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyFlashcardApp
{
    public class EnvironmentWrapper : IEnvironmentWrapper
    {
        public void Exit(int exitCode) { 
            Environment.Exit(exitCode);
        }
    }
}
