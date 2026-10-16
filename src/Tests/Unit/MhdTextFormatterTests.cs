using FoodDatabase.App.Models;
using Xunit;

namespace FoodDatabase.Tests.Unit
{
    /// <summary>
    /// Unit-Tests für MhdTextFormatter — testet alle fünf Codezweige:
    /// Singular und Plural für Vergangenheit (< 0), Heute (= 0) und Zukunft (> 0).
    /// </summary>
    public class MhdTextFormatterTests
    {
        [Theory]
        [InlineData(-5, "vor 5 Tagen abgelaufen")]
        [InlineData(-1, "vor 1 Tag abgelaufen")]
        [InlineData(0, "Heute ablaufend")]
        [InlineData(1, "1 Tag verbleibend")]
        [InlineData(5, "5 Tage verbleibend")]
        public void GetMhdText_ReturnsCorrectLocalizedText(int days, string expectedText)
        {
            // Act
            string result = MhdTextFormatter.GetMhdText(days);

            // Assert
            Assert.Equal(expectedText, result);
        }
    }
}
