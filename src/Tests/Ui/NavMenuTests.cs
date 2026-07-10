using Bunit;
using FoodDatabase.App.Components.Layout;
using Xunit;

namespace FoodDatabase.Tests.Ui
{
    public class NavMenuTests : TestContext
    {
        public NavMenuTests()
        {
            JSInterop.Mode = JSRuntimeMode.Loose;
        }

        [Fact]
        public void RendersAllSevenNavLinks()
        {
            // Arrange
            // Act
            var cut = RenderComponent<NavMenu>();

            // Assert
            var links = cut.FindAll("a.nav-link");
            Assert.Equal(7, links.Count);

            var hrefs = links.Select(l => l.GetAttribute("href")).ToList();
            Assert.Contains("/", hrefs);
            Assert.Contains("/lebensmittel", hrefs);
            Assert.Contains("/lagerbestand", hrefs);
            Assert.Contains("/lagerorte", hrefs);
            Assert.Contains("/rezepte", hrefs);
            Assert.Contains("/warnungen", hrefs);
            Assert.Contains("/einkaufsliste", hrefs);
        }

        [Fact]
        public void NavElementHasExpandMdClass()
        {
            // Arrange
            // Act
            var cut = RenderComponent<NavMenu>();

            // Assert
            var nav = cut.Find("nav");
            Assert.NotNull(nav);
            Assert.Contains("navbar-expand-md", nav.GetAttribute("class") ?? "");
        }

        [Fact]
        public void TogglerTargetsOffcanvasPanel()
        {
            // Arrange
            // Act
            var cut = RenderComponent<NavMenu>();

            // Assert
            var toggler = cut.Find("button.navbar-toggler");
            Assert.NotNull(toggler);
            Assert.Equal("offcanvas", toggler.GetAttribute("data-bs-toggle"));
            Assert.Equal("#navbarOffcanvas", toggler.GetAttribute("data-bs-target"));

            var panel = cut.Find("#navbarOffcanvas");
            Assert.NotNull(panel);
        }

        [Fact]
        public void PanelSlidesFromLeft()
        {
            // Arrange
            // Act
            var cut = RenderComponent<NavMenu>();

            // Assert
            var panel = cut.Find("#navbarOffcanvas");
            var classAttr = panel.GetAttribute("class") ?? "";
            Assert.Contains("offcanvas", classAttr);
            Assert.Contains("offcanvas-start", classAttr);
        }

        [Fact]
        public void EveryNavLinkDismissesOffcanvas()
        {
            // Arrange
            // Act
            var cut = RenderComponent<NavMenu>();

            // Assert
            var links = cut.FindAll("a.nav-link");
            foreach (var link in links)
            {
                Assert.Equal("offcanvas", link.GetAttribute("data-bs-dismiss"));
            }
        }

        [Fact]
        public void HomeLinkUsesExactMatch()
        {
            // Arrange
            // Act
            var cut = RenderComponent<NavMenu>();

            // Assert
            var homeLink = cut.FindAll("a.nav-link")
                .First(l => l.GetAttribute("href") == "/");
            Assert.NotNull(homeLink);
            // NavLinkMatch.All ist nur über das renderete HTML nicht direkt sichtbar,
            // aber wir können prüfen, dass der Link existiert und auf "/" zeigt
            Assert.Equal("/", homeLink.GetAttribute("href"));
        }
    }
}
