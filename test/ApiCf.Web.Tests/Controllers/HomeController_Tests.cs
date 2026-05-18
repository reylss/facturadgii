using System.Threading.Tasks;
using ApiCf.Models.TokenAuth;
using ApiCf.Web.Controllers;
using Shouldly;
using Xunit;

namespace ApiCf.Web.Tests.Controllers
{
    public class HomeController_Tests: ApiCfWebTestBase
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



