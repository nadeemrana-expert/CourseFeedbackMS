using System.Threading.Tasks;
using CourseFeedbackMS.Models.TokenAuth;
using CourseFeedbackMS.Web.Controllers;
using Shouldly;
using Xunit;

namespace CourseFeedbackMS.Web.Tests.Controllers
{
    public class HomeController_Tests: CourseFeedbackMSWebTestBase
    {
        [Fact]
        public async Task Index_Test()
        {
            await AuthenticateAsync(null, new AuthenticateModel
            {
                UserNameOrEmailAddress = "admin",
                Password = "123qwe"
            });

            //Act
            var response = await GetResponseAsStringAsync(
                GetUrl<HomeController>(nameof(HomeController.Index))
            );

            //Assert
            response.ShouldNotBeNullOrEmpty();
        }
    }
}