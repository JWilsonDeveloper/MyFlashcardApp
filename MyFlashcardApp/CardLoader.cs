using MyFlashcardApp.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyFlashcardApp
{
    public class CardLoader
    {
        private List<IFlashcard> flashcards;
        private IUserInterface userInterface;
        private IErrorLog errorLog;
        private IFileReader fileReader;
        private IFlashcard sampleCard;
        private string filePath;
        private IEnvironmentWrapper environmentWrapper;

        public IFlashcard SampleCard
        {
            get { return sampleCard; }
            set { sampleCard = value; }
        }

        public List<IFlashcard> Flashcards
        {
            get { return flashcards; }
            set { flashcards = value; }
        }

        public IFileReader FileReader
        {
            get { return fileReader; }
            set { fileReader = value; }
        }

        public IEnvironmentWrapper EnvironmentWrapper
        {
            get { return environmentWrapper; }
            set { environmentWrapper = value; }
        }

        public CardLoader(string filePath, IFlashcard sampleCard, IUserInterface userInterface, IErrorLog errorLog)
        {
            this.filePath = filePath;
            this.userInterface = userInterface;
            this.errorLog = errorLog;
            this.sampleCard = sampleCard;
            this.fileReader = new FileReader();
            this.environmentWrapper = new EnvironmentWrapper();
        }

        public List<IFlashcard> DetermineAndGetFlashcards()
        {
            List<List<string>> uniqueCategoryLists = LoadUniqueCategoryListsFromCSV(filePath);
            List<List<string>> chosenCategoryLists = ChooseCategoryLists(uniqueCategoryLists);
            return LoadFlashcardsFromCSV(filePath, chosenCategoryLists, sampleCard);
        }

        public List<List<string>> LoadUniqueCategoryListsFromCSV(string filePath)
        {
            //Every flashcard has a list starting with its primary category, followed by its optional subcategory, sub-subcategory, etc.
            //This function returns every unique list.
            List<List<string>> uniqueCategoryLists = new List<List<string>>();
            string[] lines = fileReader.ReadAllLines(filePath);
            if (lines != null && lines.Length > 0)
            {
                for (int lineIndex = 1; lineIndex < lines.Length; lineIndex++)  // Skip header line
                {
                    string line = lines[lineIndex];
                    string[] columns = line.Split(',');
                    if (columns.Length > 2)     // Indices 0 and 1 are for Question and Answer
                    {   
                        List<string> categoryList = new List<string>();     // Get the flashcards category list as a list of strings
                        for (int i = 2; i < columns.Length; i++)
                        {
                            categoryList.Add(columns[i].Trim());
                        }
                        bool categoryExists = uniqueCategoryLists.Any(l => l.SequenceEqual(categoryList));  // Check if the list of unique category lists contains this list
                        if (!categoryExists)
                        {
                            uniqueCategoryLists.Add(categoryList);  // Add the category list if it isn't in uniqueCategoryLists
                        }
                    }
                    else
                    {
                        errorLog.AddError($"Invalid format on line {lineIndex + 1}. Skipping...");
                    }
                }
            }
            else
            {
                errorLog.AddError($"Error reading file or parsing data from {filePath}");
            }
            return uniqueCategoryLists;
        }

        public List<List<string>> ChooseCategoryLists(List<List<string>> uniqueCategoryLists)
        {
            List<List<string>> returnList = new List<List<string>>();
            List<List<string>> filteredListOfLists = new List<List<string>>(uniqueCategoryLists);
            List<string> categoriesSoFar = new List<string>();
            int i = 0;

            while (true)
            {
                List<string> options = new List<string> { "All" };

                foreach (List<string> categoryList in uniqueCategoryLists)
                {
                    if (i == 0 || categoriesSoFar.SequenceEqual(categoryList.GetRange(0, i).ToList()))
                    {
                        options.Add(categoryList[i]);
                    }
                }

                options.Add(i == 0 ? "Exit" : "Go Back");

                string chosenCategory = userInterface.ChooseOption(options);

                switch (chosenCategory)
                {
                    case "Exit":
                        environmentWrapper.Exit(0);
                        break;
                    case "Go Back":
                        options.Clear();
                        i--;
                        categoriesSoFar.RemoveAt(categoriesSoFar.Count - 1);
                        break;
                    case "All":
                        returnList = filteredListOfLists;
                        return returnList;
                    default:
                        categoriesSoFar.Add(chosenCategory);

                        filteredListOfLists = uniqueCategoryLists
                            .Where(list => categoriesSoFar.SequenceEqual(list.GetRange(0, i + 1).ToList()))
                            .ToList();

                        if (filteredListOfLists.Any(list => list.Count > i + 1))
                        {
                            options.Clear();
                            i++;
                        }
                        else
                        {
                            returnList = filteredListOfLists;
                            return returnList;
                        }
                        break;
                }
            }
        }

        public List<IFlashcard> LoadFlashcardsFromCSV(string filePath, List<List<string>> chosenCategoryLists, IFlashcard sampleCard)
        {
            bool chosenCategoryListsNullOrEmpty = chosenCategoryLists == null || chosenCategoryLists.Count == 0;
            List<IFlashcard> flashcards = new List<IFlashcard>();
            string[] lines = fileReader.ReadAllLines(filePath);
            if (lines != null && lines.Length > 0)
            {
                for (int lineIndex = 1; lineIndex < lines.Length; lineIndex++) // Skip header line
                {
                    string line = lines[lineIndex];
                        string[] columns = line.Split(',');
                    if(columns.Length > 2) 
                    {   
                        List<string> categoryList = new List<string>();  //Get the flashcards category list as a list of strings
                        for (int i = 2; i < columns.Length; i++)
                        {
                            categoryList.Add(columns[i].Trim());
                        }
                        if (chosenCategoryListsNullOrEmpty || chosenCategoryLists.Any(list => list.SequenceEqual(categoryList)))
                        {
                            foreach (List<string> l in chosenCategoryLists)
                            {
                                if (l.SequenceEqual(categoryList))
                                {
                                    IFlashcard flashcard = sampleCard.Clone();
                                    flashcard.Question = columns[0].Trim();
                                    flashcard.Answer = columns[1].Trim();
                                    flashcard.Categories = categoryList;
                                    flashcards.Add(flashcard);
                                    break;
                                }
                            }
                        }
                        else
                        {
                            errorLog.AddError($"Invalid format on line {lineIndex + 1}. Skipping...");
                        }
                    }
                    else
                    {
                        errorLog.AddError($"Invalid format on line {lineIndex + 1}. Skipping...");
                    }
                }
            }
            else
            {
                errorLog.AddError($"Error reading file or parsing data from {filePath}");
            }
            return flashcards;
        }

        public List<IFlashcard> LoadAllFlashcardsFromCSV(string filePath, IFlashcard sampleCard)
        {
            List<IFlashcard> flashcards = new List<IFlashcard>();
            string[] lines = fileReader.ReadAllLines(filePath);
            if (lines != null && lines.Length > 0)
            {
                for (int lineIndex = 1; lineIndex < lines.Length; lineIndex++) // Skip header line
                {
                    string line = lines[lineIndex];
                    string[] columns = line.Split(',');
                    if (columns.Length > 2)
                    {
                        List<string> categoryList = new List<string>();  //Get the flashcards category list as a list of strings
                        for (int i = 2; i < columns.Length; i++)
                        {
                            categoryList.Add(columns[i].Trim());
                        }
                        IFlashcard flashcard = sampleCard.Clone();
                        flashcard.Question = columns[0].Trim();
                        flashcard.Answer = columns[1].Trim();
                        List<string> categories = new List<string>();
                        for (int i = 2; i < columns.Length; i++)
                        {
                            categories.Add(columns[i].Trim());
                        }
                        flashcard.Categories = categories;
                        flashcards.Add(flashcard);
                        break;
                    }
                    else
                    {
                        errorLog.AddError($"Invalid format on line {lineIndex + 1}. Skipping...");
                    }
                }
            }
            else
            {
                errorLog.AddError($"Error reading file or parsing data from {filePath}");
            }
            return flashcards;
        }
    }
}
