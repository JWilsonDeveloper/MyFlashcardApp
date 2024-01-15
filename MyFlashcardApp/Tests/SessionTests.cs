using Moq;
using MyFlashcardApp.Interfaces;
using System.Globalization;

namespace MyFlashcardApp.Tests
{
    public class SessionTests
    {
        private Session session;
        private Mock<IUserInterface> mockUserInterface;
        private Mock<IErrorLog> mockErrorLog;
        private List<IFlashcard> flashcards;

        [SetUp]
        public void Setup()
        {
            string filePath = "path/to/file";
            var mockSampleCard = new Mock<IFlashcard>();
            IFlashcard sampleCard = mockSampleCard.Object;
            mockUserInterface = new Mock<IUserInterface>();
            IUserInterface userInterface = mockUserInterface.Object;
            mockErrorLog = new Mock<IErrorLog>();
            IErrorLog errorLog = mockErrorLog.Object;

            session = new Session(filePath, sampleCard, errorLog, userInterface);

            var mockFlashcards = new List<Mock<IFlashcard>>(); // Create a list of mock IFlashcard objects
            for (int i = 0; i < 52; i++)
            {
                mockFlashcards.Add(new Mock<IFlashcard>());
            }
            flashcards = new List<IFlashcard>();
            foreach (var mockCard in mockFlashcards)
            {
                flashcards.Add(mockCard.Object);
            }
        }

        [Test]
        public void ShuffleFlashcards_ShufflesProperly()
        {
            // Arrange

            var originalFlashcards = new List<IFlashcard>(flashcards); // Create a copy of the original flashcards

            // Act
            session.ShuffleFlashcards(flashcards);

            // Assert
            CollectionAssert.AreNotEqual(originalFlashcards, flashcards, "The lists should not be in the same order");
            CollectionAssert.AreEquivalent(originalFlashcards, flashcards, "The lists should contain the same cards");
        }

        [Test]
        public void SelectMode__DebugMode_ReturnsTrue()
        {
            // Arrange
            mockUserInterface.Setup(ui => ui.ChooseOption(It.IsAny<List<string>>())).Returns("Debug Mode");

            // Act
            bool shouldPrint = session.SelectMode();

            // Assert
            Assert.IsTrue(shouldPrint, "SelectMode should have returned true");
        }

        [Test]
        public void SelectMode_NormalMode_ReturnsFalse()
        {
            // Arrange
            mockUserInterface.Setup(ui => ui.ChooseOption(It.IsAny<List<string>>())).Returns("Normal Mode");

            // Act
            bool shouldPrint = session.SelectMode();

            // Assert
            Assert.IsFalse(shouldPrint, "SelectMode should have returned false");
        }

        [Test]
        public void GoThroughFlashcards_FlashcardReturnsNewIndex_IndexUpdates()
        {
            // Arrange
            mockUserInterface.SetupSequence(ui => ui.Flashcard(It.IsAny<bool>(), It.IsAny<List<IFlashcard>>(), It.IsAny<int>()))
                             .Returns(new FlashcardResult(false, false, 1))
                             .Returns(new FlashcardResult(false, true, 1));

            // Act
            session.GoThroughFlashcards(flashcards);

            // Assert
            mockUserInterface.Verify(ui => ui.Flashcard(It.IsAny<bool>(), flashcards, It.IsAny<int>()), Times.Exactly(2), "The loop should execute 2 times total and exit");
            mockUserInterface.Verify(ui => ui.Flashcard(false, flashcards, 0), Times.Once, "The loop should execute the first time with standard arguments for Flashcard()");
            mockUserInterface.Verify(ui => ui.Flashcard(false, flashcards, 1), Times.Once, "The loop should execute a second time with an updated index argument for Flashcard()");
        }

        [Test]
        public void GoThroughFlashcards_FlashcardReturnsDownPressed_ShowAnswerUpdates()
        {
            // Arrange
            mockUserInterface.SetupSequence(ui => ui.Flashcard(It.IsAny<bool>(), It.IsAny<List<IFlashcard>>(), It.IsAny<int>()))
                             .Returns(new FlashcardResult(true, false, 0))
                             .Returns(new FlashcardResult(false, true, 0));

            // Act
            session.GoThroughFlashcards(flashcards);

            // Assert
            mockUserInterface.Verify(ui => ui.Flashcard(It.IsAny<bool>(), flashcards, It.IsAny<int>()), Times.Exactly(2), "The loop should execute 2 times total and exit");
            mockUserInterface.Verify(ui => ui.Flashcard(false, flashcards, 0), Times.Once, "The loop should execute the first time with standard arguments for Flashcard()");
            mockUserInterface.Verify(ui => ui.Flashcard(true, flashcards, 0), Times.Once, "The loop should execute a second time with an updated showAnswer argument for Flashcard()");
        }

        [Test]
        public void GoThroughFlashcards_HandlesManyLoops()
        {
            // Arrange
            List<FlashcardResult> results = new List<FlashcardResult>();
            for (int i = 0; i < 305; i++)
            {
                bool isEscapePressed = i == 299; // Set EscapePressed to true for the 21st iteration
                bool isDownPressed = i % 2 == 0;
                Random random = new Random();
                int randomInt = random.Next(-40000, 40001);
                results.Add(new FlashcardResult(isDownPressed, isEscapePressed, randomInt));
            }

            int index = 0;
            mockUserInterface.Setup(ui => ui.Flashcard(It.IsAny<bool>(), It.IsAny<List<IFlashcard>>(), It.IsAny<int>()))
                             .Returns(() => results[index++]);

            // Act
            session.GoThroughFlashcards(flashcards);

            // Assert
            mockUserInterface.Verify(ui => ui.Flashcard(It.IsAny<bool>(), flashcards, It.IsAny<int>()), Times.Exactly(300), "The loop should execute 300 times total and exit");
        }

    }
}