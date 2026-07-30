using Bunit;
using FoodDatabase.App.Components.Layout;
using Microsoft.AspNetCore.Components;
using Xunit;

namespace FoodDatabase.Tests.Ui
{
    public class MainLayoutTests : TestContext
    {
        public MainLayoutTests()
        {
            JSInterop.Mode = JSRuntimeMode.Loose;
        }

        [Fact]
        public void RendersSidebarAndMain()
        {
            // Arrange
            var cut = RenderComponent<MainLayout>();

            // Assert
            var sidebars = cut.FindAll(".sidebar");
            Assert.Single(sidebars);

            var mains = cut.FindAll("main");
            Assert.Single(mains);
        }

        [Fact]
        public void BodyIsRenderedInsideContentArticle()
        {
            // Arrange
            // Act
            var cut = RenderComponent<MainLayout>();

            // Assert
            var article = cut.Find("main > article.content");
            Assert.NotNull(article);
        }

        [Fact]
        public void DoesNotContainMicrosoftTemplateLink()
        {
            // Arrange
            // Act
            var cut = RenderComponent<MainLayout>();

            // Assert
            Assert.DoesNotContain("learn.microsoft.com", cut.Markup);
        }
    }
}
