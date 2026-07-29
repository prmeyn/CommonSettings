using uSignIn.CommonSettings.DTOs;

namespace uSignIn.CommonSettings.Tests
{
    public class HistoryExtensionsTests
    {
        [Fact]
        public void LatestValue_ShouldReturnMostRecentValue()
        {
            // Arrange
            var now = DateTimeOffset.UtcNow;
            var history = new List<History<string>>
            {
                new History<string> { Value = "Old", TimeStamp = now.AddMinutes(-10) },
                new History<string> { Value = "New", TimeStamp = now },
                new History<string> { Value = "Older", TimeStamp = now.AddMinutes(-20) }
            };

            // Act
            var result = history.LatestValue();

            // Assert
            Assert.Equal("New", result);
        }

        [Fact]
        public void LatestValue_ShouldReturnDefault_WhenListIsEmpty()
        {
            // Arrange
            var history = new List<History<string>>();

            // Act
            var result = history.LatestValue();

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void LatestValue_ShouldReturnDefault_WhenSequenceIsNull()
        {
            // Arrange
            IEnumerable<History<string>>? history = null;

            // Act
            var result = history.LatestValue();

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void LatestRecord_ShouldReturnMostRecentRecord()
        {
            // Arrange
            var now = DateTimeOffset.UtcNow;
            var recent = new History<int> { Value = 100, TimeStamp = now };
            var old = new History<int> { Value = 50, TimeStamp = now.AddMinutes(-10) };
            var history = new List<History<int>> { old, recent };

            // Act
            var result = history.LatestRecord();

            // Assert
            Assert.Same(recent, result);
        }

        [Fact]
        public void LatestRecord_ShouldReturnNull_WhenSequenceIsNull()
        {
            // Arrange
            IEnumerable<History<int>>? history = null;

            // Act
            var result = history.LatestRecord();

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void LatestRecord_ShouldReturnNull_WhenListIsEmpty()
        {
            // Act
            var result = new List<History<int>>().LatestRecord();

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void LatestRecord_ShouldReturnFirstOfTiedRecords_WhenTimeStampsAreEqual()
        {
            // Arrange
            var timeStamp = DateTimeOffset.UtcNow;
            var first = new History<int> { Value = 1, TimeStamp = timeStamp };
            var second = new History<int> { Value = 2, TimeStamp = timeStamp };
            var history = new List<History<int>> { first, second };

            // Act
            var result = history.LatestRecord();

            // Assert
            Assert.Same(first, result);
        }
    }
}
