using Moq;
using MyFlashcardApp.Interfaces;
using System.Globalization;

namespace MyFlashcardApp.Tests
{
    public class CardLoaderTests
    {
        private CardLoader cardLoader;
        private Mock<IUserInterface> mockUserInterface;
        private Mock<IErrorLog> mockErrorLog;
        string filePath;
        string[] fileContent;
        List<List<string>> expectedResults;
        List<List<string>> uniqueCategoryLists;
        IFlashcard sampleCard;

        [SetUp]
        public void Setup()
        {
            filePath = "path/to/file";
            var mockSampleCard = new Mock<IFlashcard>();
            sampleCard = mockSampleCard.Object;
            mockUserInterface = new Mock<IUserInterface>();
            IUserInterface userInterface = mockUserInterface.Object;
            mockErrorLog = new Mock<IErrorLog>();
            IErrorLog errorLog = mockErrorLog.Object;

            cardLoader = new CardLoader(filePath, sampleCard, userInterface, errorLog);

            fileContent = new[]
            {
                "Question,Answer,Category,Subcategory1,Subcategory2",
                "Q1,A1,Cat1,SubCat1_1",
                "Q2,A2,Cat1,SubCat1_2",
                "",
                "Q3,A3,Cat2,SubCat1_1",
                "Q4,A4,Cat2,SubCat1_2",
                "",
                "Q5,A5,Cat3,SubCat1_1,SubCat2_1",
                "Q6,A6,Cat3,SubCat1_1,SubCat2_2",
                "Q7,A7,Cat3,SubCat1_1,SubCat2_3",
                "Q8,A8,Cat3,SubCat1_2",
                "Q9,A9,Cat3,SubCat1_3",
                "",
                "Q10,A10,Cat4,SubCat1_1",
                "Q11,A11,Cat4,SubCat1_2",
                "",
                "Q12,A12,Cat5,SubCat1_1",
                "Q13,A13,Cat5,SubCat1_2",
                "Q14,A14,Cat5,SubCat1_3",
                "",
                "Q15,A15,Cat6,SubCat1_1",
                "Q16,A16,Cat6,SubCat1_2",
                "",
                "Q17,A17,Cat7,SubCat1_1",
                "Q18,A18,Cat7,SubCat1_2",
                "Q19,A19,Cat7,SubCat1_3",
                "",
                "Q20,A20,Cat8,SubCat1_1"
            };

            expectedResults = fileContent
                .Where(line => !string.IsNullOrEmpty(line))
                .Skip(1) // Skip the header line
                .Select(line =>
                {
                    string[] values = line.Split(',');
                    return values.Skip(2).ToList(); // Assuming the last two comma-separated values
                })
                .ToList();

            // Arrange
            uniqueCategoryLists = new List<List<string>>
            {
                new List<string> {"Cat1", "SubCat1_1", "SubCat2_1"},
                new List<string> {"Cat1", "SubCat1_1", "SubCat2_2"},
                new List<string> {"Cat1", "SubCat1_2", "SubCat2_1"},
                new List<string> {"Cat2", "SubCat1_1", "SubCat2_1"},
                new List<string> {"Cat3", "SubCat1_1", "SubCat2_1" }
            };
        }

        [Test]
        public void LoadUniqueCategoryListsFromCSV_ValidFile_ReturnsCorrectList()
        {
            // Arrange
            var fakeFileReader = new Mock<IFileReader>();
            fakeFileReader.Setup(x => x.ReadAllLines(filePath)).Returns(fileContent);
            cardLoader.FileReader = fakeFileReader.Object;
            
            // Act
            List<List<string>> result = cardLoader.LoadUniqueCategoryListsFromCSV(filePath);
            
            // Assert
            Assert.IsNotNull(result);
            Assert.That(result, Is.EqualTo(expectedResults));
        }
        
        [Test]
        public void LoadUniqueCategoryListsFromCSV_InvalidLine_FormatErrorLogged()
        {
            // Arrange
            var fakeFileReader = new Mock<IFileReader>();
            fakeFileReader.Setup(x => x.ReadAllLines(filePath)).Returns(fileContent);
            cardLoader.FileReader = fakeFileReader.Object;

            // Act
            List<List<string>> result = cardLoader.LoadUniqueCategoryListsFromCSV(filePath);

            // Assert
            // Verify that the error log was called with the specific string parameter
            mockErrorLog.Verify(x => x.AddError(It.Is<string>(s => s.Contains("Invalid format on line 4"))), Times.Once);
            mockErrorLog.Verify(x => x.AddError(It.Is<string>(s => s.Contains("Invalid format on line 7"))), Times.Once);
            mockErrorLog.Verify(x => x.AddError(It.Is<string>(s => s.Contains("Invalid format on line 13"))), Times.Once);
            mockErrorLog.Verify(x => x.AddError(It.Is<string>(s => s.Contains("Invalid format on line 16"))), Times.Once);
            mockErrorLog.Verify(x => x.AddError(It.Is<string>(s => s.Contains("Invalid format on line 20"))), Times.Once);
            mockErrorLog.Verify(x => x.AddError(It.Is<string>(s => s.Contains("Invalid format on line 23"))), Times.Once);
            mockErrorLog.Verify(x => x.AddError(It.Is<string>(s => s.Contains("Invalid format on line 27"))), Times.Once);
        }
        
        [Test]
        public void LoadUniqueCategoryListsFromCSV_NullFile_ReadingErrorLogged()
        {
            // Arrange
            var fakeFileReader = new Mock<IFileReader>();
            fakeFileReader.Setup(x => x.ReadAllLines(filePath)).Returns((string[]) null);
            cardLoader.FileReader = fakeFileReader.Object;

            // Act
            List<List<string>> result = cardLoader.LoadUniqueCategoryListsFromCSV(filePath);

            // Assert
            // Verify that the error log was called with the specific string parameter
            mockErrorLog.Verify(x => x.AddError(It.Is<string>(s => s.Contains("Error reading file"))), Times.Once);
        }

        [Test]
        public void ChooseCategoryLists_Returns_FilteredList()
        {
            // Arrange
            mockUserInterface.SetupSequence(x => x.ChooseOption(It.IsAny<List<string>>()))
                .Returns("Cat2")
                .Returns("Go Back")
                .Returns("Cat3")
                .Returns("SubCat1_1")
                .Returns("Go Back")
                .Returns("Go Back")
                .Returns("Cat1")
                .Returns("SubCat1_2")
                .Returns("Go Back")
                .Returns("SubCat1_1")
                .Returns("SubCat2_1");

            // Act
            List<List<string>> result = cardLoader.ChooseCategoryLists(uniqueCategoryLists);

            // Assert
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0], Is.EqualTo(new List<string> { "Cat1", "SubCat1_1", "SubCat2_1" }));
        }

        [Test]
        public void ChooseCategoryLists_ExitReturned_EndsProgram()
        {
            // Arrange
            mockUserInterface.SetupSequence(x => x.ChooseOption(It.IsAny<List<string>>()))
                .Returns("Cat2")
                .Returns("Go Back")
                .Returns("Cat3")
                .Returns("SubCat1_1")
                .Returns("Go Back")
                .Returns("Go Back")
                .Returns("Exit");

            var mockEnvironmentWrapper = new Mock<IEnvironmentWrapper>();
            cardLoader.EnvironmentWrapper = mockEnvironmentWrapper.Object;

            // Act
            cardLoader.ChooseCategoryLists(uniqueCategoryLists);

            // Assert
            mockEnvironmentWrapper.Verify(x => x.Exit(0), Times.Once);
        }

        [Test]
        public void ChooseCategoryLists_MultipleSelected_ReturnsMultiple()
        {
            // Arrange
            mockUserInterface.SetupSequence(x => x.ChooseOption(It.IsAny<List<string>>()))
                .Returns("Cat1")
                .Returns("SubCat1_1")
                .Returns("All");

            // Act
            List<List<string>> result = cardLoader.ChooseCategoryLists(uniqueCategoryLists);

            // Assert
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[0], Is.EqualTo(new List<string> { "Cat1", "SubCat1_1", "SubCat2_1" }));
            Assert.That(result[1], Is.EqualTo(new List<string> { "Cat1", "SubCat1_1", "SubCat2_2" }));
        }
        
        [Test]
        public void LoadFlashcardsFromCSV_ValidInput_ReturnsCorrectFlashcards()
        {
            // Arrange
            List<List<string>> chosenCategoryLists = new List<List<string>>
            {
                new List<string> {"Cat3", "SubCat1_1", "SubCat2_1"},
                new List<string> {"Cat3", "SubCat1_1", "SubCat2_2"},
                new List<string> {"Cat3", "SubCat1_1", "SubCat2_3"},
                new List<string> {"Cat3", "SubCat1_2"},
                new List<string> {"Cat3", "SubCat1_3"}
            };

            var mockSampleCard = new Mock<IFlashcard>();
            mockSampleCard.SetupProperty(f => f.Question);
            mockSampleCard.SetupProperty(f => f.Answer);
            mockSampleCard.SetupProperty(f => f.Categories);
            mockSampleCard.Setup(x => x.Clone()).Returns(() => GetNewMockFlashcard().Object);
            sampleCard = mockSampleCard.Object;

            var fakeFileReader = new Mock<IFileReader>();
            fakeFileReader.Setup(x => x.ReadAllLines(filePath)).Returns(fileContent);
            cardLoader.FileReader = fakeFileReader.Object;
            
            // Act
            List<IFlashcard> result = cardLoader.LoadFlashcardsFromCSV(filePath, chosenCategoryLists, sampleCard);

            // Assert
            Assert.NotNull(result);
            Assert.That(result[0].Categories, Is.EqualTo(chosenCategoryLists[0]));
            Assert.That(result[1].Categories, Is.EqualTo(chosenCategoryLists[1]));
            Assert.That(result[2].Categories, Is.EqualTo(chosenCategoryLists[2]));
            Assert.That(result[3].Categories, Is.EqualTo(chosenCategoryLists[3]));
            Assert.That(result[4].Categories, Is.EqualTo(chosenCategoryLists[4]));
        }

        [Test]
        public void LoadFlashcardsFromCSV_InvalidLine_FormatErrorLogged()
        {
            // Arrange
            List<List<string>> chosenCategoryLists = new List<List<string>>
            {
                new List<string> {"Cat3", "SubCat1_1", "SubCat2_1"},
                new List<string> {"Cat3", "SubCat1_1", "SubCat2_2"},
                new List<string> {"Cat3", "SubCat1_1", "SubCat2_3"},
                new List<string> {"Cat3", "SubCat1_2"},
                new List<string> {"Cat3", "SubCat1_3"}
            };

            var mockSampleCard = new Mock<IFlashcard>();
            mockSampleCard.SetupProperty(f => f.Question);
            mockSampleCard.SetupProperty(f => f.Answer);
            mockSampleCard.SetupProperty(f => f.Categories);
            mockSampleCard.Setup(x => x.Clone()).Returns(() => GetNewMockFlashcard().Object);
            sampleCard = mockSampleCard.Object;

            var fakeFileReader = new Mock<IFileReader>();
            fakeFileReader.Setup(x => x.ReadAllLines(filePath)).Returns(fileContent);
            cardLoader.FileReader = fakeFileReader.Object;

            // Act
            List<IFlashcard> result = cardLoader.LoadFlashcardsFromCSV(filePath, chosenCategoryLists, sampleCard);

            // Assert
            mockErrorLog.Verify(x => x.AddError(It.Is<string>(s => s.Contains("Invalid format on line 4"))), Times.Once);
            mockErrorLog.Verify(x => x.AddError(It.Is<string>(s => s.Contains("Invalid format on line 7"))), Times.Once);
            mockErrorLog.Verify(x => x.AddError(It.Is<string>(s => s.Contains("Invalid format on line 13"))), Times.Once);
            mockErrorLog.Verify(x => x.AddError(It.Is<string>(s => s.Contains("Invalid format on line 16"))), Times.Once);
            mockErrorLog.Verify(x => x.AddError(It.Is<string>(s => s.Contains("Invalid format on line 20"))), Times.Once);
            mockErrorLog.Verify(x => x.AddError(It.Is<string>(s => s.Contains("Invalid format on line 23"))), Times.Once);
            mockErrorLog.Verify(x => x.AddError(It.Is<string>(s => s.Contains("Invalid format on line 27"))), Times.Once);
        }

        [Test]
        public void LoadUniqueCategoryListsFromCSV_Null_ReadingErrorLogged()
        {
            // Arrange
            var fakeFileReader = new Mock<IFileReader>();
            fakeFileReader.Setup(x => x.ReadAllLines(filePath)).Returns((string[])null);
            cardLoader.FileReader = fakeFileReader.Object;

            // Act
            List<List<string>> result = cardLoader.LoadUniqueCategoryListsFromCSV(filePath);

            // Assert
            // Verify that the error log was called with the specific string parameter
            mockErrorLog.Verify(x => x.AddError(It.Is<string>(s => s.Contains("Error reading file"))), Times.Once);
        }

        [Test]
        public void LoadFlashcardsFromCSV_NullFile_ReadingErrorLogged()
        {
            // Arrange
            List<List<string>> chosenCategoryLists = new List<List<string>>
            {
                new List<string> {"Cat3", "SubCat1_1", "SubCat2_1"},
                new List<string> {"Cat3", "SubCat1_1", "SubCat2_2"},
                new List<string> {"Cat3", "SubCat1_1", "SubCat2_3"},
                new List<string> {"Cat3", "SubCat1_2"},
                new List<string> {"Cat3", "SubCat1_3"}
            };

            var mockSampleCard = new Mock<IFlashcard>();
            mockSampleCard.SetupProperty(f => f.Question);
            mockSampleCard.SetupProperty(f => f.Answer);
            mockSampleCard.SetupProperty(f => f.Categories);
            mockSampleCard.Setup(x => x.Clone()).Returns(() => GetNewMockFlashcard().Object);
            sampleCard = mockSampleCard.Object;

            var fakeFileReader = new Mock<IFileReader>();
            cardLoader.FileReader = fakeFileReader.Object;

            // Act
            List<IFlashcard> result = cardLoader.LoadFlashcardsFromCSV(filePath, chosenCategoryLists, sampleCard);

            // Assert
            mockErrorLog.Verify(x => x.AddError(It.Is<string>(s => s.Contains("Error reading file"))), Times.Once);
        }

        [Test]
        public void LoadFlashcardsFromCSV_NullChosenCategoryLists_ReadingErrorLogged()
        {
            // Arrange
            List<List<string>> chosenCategoryLists = new List<List<string>>();
            int expectedCount = fileContent.Count(str => str.Split(',').Length >= 3); // The number of cards that should be created

            var mockSampleCard = new Mock<IFlashcard>();
            mockSampleCard.SetupProperty(f => f.Question);
            mockSampleCard.SetupProperty(f => f.Answer);
            mockSampleCard.SetupProperty(f => f.Categories);
            mockSampleCard.Setup(x => x.Clone()).Returns(() => GetNewMockFlashcard().Object);
            sampleCard = mockSampleCard.Object;

            var fakeFileReader = new Mock<IFileReader>();
            cardLoader.FileReader = fakeFileReader.Object;

            // Act
            List<IFlashcard> result = cardLoader.LoadFlashcardsFromCSV(filePath, chosenCategoryLists, sampleCard);

            // Assert
            Assert.That(result.Count(), Is.EqualTo(expectedCount));
        }

        public Mock<IFlashcard> GetNewMockFlashcard()
        {
            var mockFlashcard = new Mock<IFlashcard>();
            mockFlashcard.SetupProperty(f => f.Question);
            mockFlashcard.SetupProperty(f => f.Answer);
            mockFlashcard.SetupProperty(f => f.Categories);
            return mockFlashcard;
        }
    }
}