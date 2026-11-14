using FoodDatabase.App.Models;
using Xunit;

namespace FoodDatabase.Tests.Unit
{
    /// <summary>
    /// Unit-Tests für SchwierigkeitsgradAnzeige: Zuordnung gespeicherter Werte (Easy/Medium/Hard)
    /// zur deutschen Anzeige, inklusive unbekannter Werte und null.
    /// </summary>
    public class SchwierigkeitsgradAnzeigeTests
    {
        [Theory]
        [InlineData("Easy", "Leicht")]
        [InlineData("Medium", "Mittel")]
        [InlineData("Hard", "Schwer")]
        public void ZuDeutsch_Liefert_Deutsche_Bezeichnung(string gespeichert, string erwartet)
        {
            // Act
            string ergebnis = SchwierigkeitsgradAnzeige.ZuDeutsch(gespeichert);

            // Assert
            Assert.Equal(erwartet, ergebnis);
        }

        [Fact]
        public void ZuDeutsch_Gibt_Unbekannten_Wert_Unveraendert_Zurueck()
        {
            // Act
            string ergebnis = SchwierigkeitsgradAnzeige.ZuDeutsch("Expert");

            // Assert
            Assert.Equal("Expert", ergebnis);
        }

        [Fact]
        public void ZuDeutsch_Uebersetzt_Nur_Exakte_Gespeicherte_Werte()
        {
            // Act: gespeicherte Werte sind case-sensitiv (Easy, nicht easy)
            string ergebnis = SchwierigkeitsgradAnzeige.ZuDeutsch("easy");

            // Assert
            Assert.Equal("easy", ergebnis);
        }

        [Fact]
        public void ZuDeutsch_Liefert_Leeren_String_Bei_Null()
        {
            // Act
            string ergebnis = SchwierigkeitsgradAnzeige.ZuDeutsch(null!);

            // Assert
            Assert.Equal(string.Empty, ergebnis);
        }

        [Fact]
        public void AlleWerte_Enthaelt_Genau_Die_Gespeicherten_Werte_In_Reihenfolge()
        {
            // Act & Assert: die Werte müssen zu RezeptService.ValidateSchwierigkeitsgrad passen
            Assert.Equal(new[] { "Easy", "Medium", "Hard" }, SchwierigkeitsgradAnzeige.AlleWerte);
        }
    }
}
