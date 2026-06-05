extern alias RazorAdHoc;

using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using RazorLoginModel = RazorAdHoc::EqDemo.Areas.Identity.Pages.Account.LoginModel;
using RazorLogoutModel = RazorAdHoc::EqDemo.Areas.Identity.Pages.Account.LogoutModel;
using RazorRegisterModel = RazorAdHoc::EqDemo.Areas.Identity.Pages.Account.RegisterModel;

namespace EqSamples.Tests.RazorAdHocReporting;

public class IdentityPageModelsTests
{
    private static Mock<SignInManager<IdentityUser>> CreateMockSignInManager()
    {
        var userStoreMock = new Mock<IUserStore<IdentityUser>>();
        var userManager = new Mock<UserManager<IdentityUser>>(
            userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        return new Mock<SignInManager<IdentityUser>>(
            userManager.Object,
            new Mock<IHttpContextAccessor>().Object,
            new Mock<IUserClaimsPrincipalFactory<IdentityUser>>().Object,
            new Mock<IOptions<IdentityOptions>>().Object,
            new Mock<ILogger<SignInManager<IdentityUser>>>().Object,
            new Mock<IAuthenticationSchemeProvider>().Object,
            new Mock<IUserConfirmation<IdentityUser>>().Object);
    }

    private static Mock<UserManager<IdentityUser>> CreateMockUserManager()
    {
        var userStoreMock = new Mock<IUserStore<IdentityUser>>();
        return new Mock<UserManager<IdentityUser>>(
            userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);
    }

    private static PageContext CreatePageContext()
    {
        var httpContext = new DefaultHttpContext();
        var routeData = new RouteData();
        var pageActionDescriptor = new CompiledPageActionDescriptor();
        return new PageContext(new ActionContext(httpContext, routeData, pageActionDescriptor));
    }

    // ─── LoginModel Tests ───

    [Fact]
    public void LoginModel_InputModel_Properties()
    {
        var input = new RazorLoginModel.InputModel
        {
            Email = "test@example.com",
            Password = "P@ss1",
            RememberMe = true
        };
        input.Email.Should().Be("test@example.com");
        input.Password.Should().Be("P@ss1");
        input.RememberMe.Should().BeTrue();
    }

    [Fact]
    public void LoginModel_Properties_SetAndGet()
    {
        var signInMgr = CreateMockSignInManager();
        var logger = new Mock<ILogger<RazorLoginModel>>();
        var model = new RazorLoginModel(signInMgr.Object, logger.Object);

        model.ReturnUrl = "/dashboard";
        model.ReturnUrl.Should().Be("/dashboard");

        model.ErrorMessage = "Bad login";
        model.ErrorMessage.Should().Be("Bad login");

        model.ExternalLogins = new List<AuthenticationScheme>();
        model.ExternalLogins.Should().BeEmpty();
    }

    [Fact]
    public async Task LoginModel_OnPostAsync_InvalidModelState_ReturnsPage()
    {
        var signInMgr = CreateMockSignInManager();
        var logger = new Mock<ILogger<RazorLoginModel>>();
        var model = new RazorLoginModel(signInMgr.Object, logger.Object);
        model.PageContext = CreatePageContext();

        var urlHelper = new Mock<IUrlHelper>();
        urlHelper.Setup(u => u.Content(It.IsAny<string>())).Returns("/");
        model.Url = urlHelper.Object;

        model.ModelState.AddModelError("", "error");

        var result = await model.OnPostAsync();

        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public async Task LoginModel_OnGetAsync_WithEmptyErrorMessage_SetsReturnUrl()
    {
        var signInMgr = CreateMockSignInManager();
        signInMgr.Setup(s => s.GetExternalAuthenticationSchemesAsync())
            .ReturnsAsync(new List<AuthenticationScheme>());

        var logger = new Mock<ILogger<RazorLoginModel>>();
        var model = new RazorLoginModel(signInMgr.Object, logger.Object);

        var httpContext = new DefaultHttpContext();
        var serviceProvider = new Mock<IServiceProvider>();
        var authService = new Mock<IAuthenticationService>();
        authService.Setup(a => a.SignOutAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<AuthenticationProperties>()))
            .Returns(Task.CompletedTask);
        serviceProvider.Setup(sp => sp.GetService(typeof(IAuthenticationService)))
            .Returns(authService.Object);
        httpContext.RequestServices = serviceProvider.Object;

        model.PageContext = new PageContext { HttpContext = httpContext };

        var urlHelper = new Mock<IUrlHelper>();
        urlHelper.Setup(u => u.Content(It.IsAny<string>())).Returns("/");
        model.Url = urlHelper.Object;

        model.ErrorMessage = null;

        await model.OnGetAsync("/dashboard");

        model.ReturnUrl.Should().Be("/dashboard");
        model.ExternalLogins.Should().BeEmpty();
    }

    [Fact]
    public async Task LoginModel_OnGetAsync_WithErrorMessage_AddsModelError()
    {
        var signInMgr = CreateMockSignInManager();
        signInMgr.Setup(s => s.GetExternalAuthenticationSchemesAsync())
            .ReturnsAsync(new List<AuthenticationScheme>());

        var logger = new Mock<ILogger<RazorLoginModel>>();
        var model = new RazorLoginModel(signInMgr.Object, logger.Object);

        var httpContext = new DefaultHttpContext();
        var serviceProvider = new Mock<IServiceProvider>();
        var authService = new Mock<IAuthenticationService>();
        authService.Setup(a => a.SignOutAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<AuthenticationProperties>()))
            .Returns(Task.CompletedTask);
        serviceProvider.Setup(sp => sp.GetService(typeof(IAuthenticationService)))
            .Returns(authService.Object);
        httpContext.RequestServices = serviceProvider.Object;

        model.PageContext = new PageContext { HttpContext = httpContext };

        var urlHelper = new Mock<IUrlHelper>();
        urlHelper.Setup(u => u.Content(It.IsAny<string>())).Returns("/");
        model.Url = urlHelper.Object;

        model.ErrorMessage = "Bad creds";

        await model.OnGetAsync();

        model.ReturnUrl.Should().Be("/");
        model.ModelState.ErrorCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task LoginModel_OnPostAsync_ValidModelState_SuccessLogin_Redirects()
    {
        var signInMgr = CreateMockSignInManager();
        signInMgr.Setup(s => s.PasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

        var logger = new Mock<ILogger<RazorLoginModel>>();
        var model = new RazorLoginModel(signInMgr.Object, logger.Object);
        model.PageContext = CreatePageContext();
        model.Input = new RazorLoginModel.InputModel { Email = "a@b.c", Password = "pass", RememberMe = false };

        var urlHelper = new Mock<IUrlHelper>();
        urlHelper.Setup(u => u.Content(It.IsAny<string>())).Returns("/");
        urlHelper.Setup(u => u.IsLocalUrl(It.IsAny<string>())).Returns(true);
        model.Url = urlHelper.Object;

        var result = await model.OnPostAsync("/home");

        result.Should().BeOfType<LocalRedirectResult>();
    }

    [Fact]
    public async Task LoginModel_OnPostAsync_LockedOut_RedirectsToLockout()
    {
        var signInMgr = CreateMockSignInManager();
        signInMgr.Setup(s => s.PasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.LockedOut);

        var logger = new Mock<ILogger<RazorLoginModel>>();
        var model = new RazorLoginModel(signInMgr.Object, logger.Object);
        model.PageContext = CreatePageContext();
        model.Input = new RazorLoginModel.InputModel { Email = "a@b.c", Password = "pass", RememberMe = false };

        var urlHelper = new Mock<IUrlHelper>();
        urlHelper.Setup(u => u.Content(It.IsAny<string>())).Returns("/");
        model.Url = urlHelper.Object;

        var result = await model.OnPostAsync();

        result.Should().BeOfType<RedirectToPageResult>();
    }

    [Fact]
    public async Task LoginModel_OnPostAsync_TwoFactor_RedirectsToLoginWith2fa()
    {
        var signInMgr = CreateMockSignInManager();
        signInMgr.Setup(s => s.PasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.TwoFactorRequired);

        var logger = new Mock<ILogger<RazorLoginModel>>();
        var model = new RazorLoginModel(signInMgr.Object, logger.Object);
        model.PageContext = CreatePageContext();
        model.Input = new RazorLoginModel.InputModel { Email = "a@b.c", Password = "pass", RememberMe = false };

        var urlHelper = new Mock<IUrlHelper>();
        urlHelper.Setup(u => u.Content(It.IsAny<string>())).Returns("/");
        model.Url = urlHelper.Object;

        var result = await model.OnPostAsync();

        result.Should().BeOfType<RedirectToPageResult>();
    }

    [Fact]
    public async Task LoginModel_OnPostAsync_Failed_ReturnsPage()
    {
        var signInMgr = CreateMockSignInManager();
        signInMgr.Setup(s => s.PasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Failed);

        var logger = new Mock<ILogger<RazorLoginModel>>();
        var model = new RazorLoginModel(signInMgr.Object, logger.Object);
        model.PageContext = CreatePageContext();
        model.Input = new RazorLoginModel.InputModel { Email = "a@b.c", Password = "pass", RememberMe = false };

        var urlHelper = new Mock<IUrlHelper>();
        urlHelper.Setup(u => u.Content(It.IsAny<string>())).Returns("/");
        model.Url = urlHelper.Object;

        var result = await model.OnPostAsync();

        result.Should().BeOfType<PageResult>();
    }

    // ─── LogoutModel Tests ───

    [Fact]
    public void LogoutModel_OnGet_DoesNotThrow()
    {
        var signInMgr = CreateMockSignInManager();
        var logger = new Mock<ILogger<RazorLogoutModel>>();
        var model = new RazorLogoutModel(signInMgr.Object, logger.Object);

        var exception = Record.Exception(() => model.OnGet());
        exception.Should().BeNull();
    }

    [Fact]
    public async Task LogoutModel_OnPost_WithNullReturnUrl_ReturnsRedirectToPage()
    {
        var signInMgr = CreateMockSignInManager();
        var logger = new Mock<ILogger<RazorLogoutModel>>();
        var model = new RazorLogoutModel(signInMgr.Object, logger.Object);
        model.PageContext = CreatePageContext();

        // Configure mock URL helper
        var urlHelper = new Mock<IUrlHelper>();
        urlHelper.Setup(u => u.IsLocalUrl(It.IsAny<string>())).Returns(true);
        model.Url = urlHelper.Object;

        var result = await model.OnPost(null);

        signInMgr.Verify(s => s.SignOutAsync(), Times.Once);
        result.Should().BeOfType<RedirectToPageResult>();
    }

    [Fact]
    public async Task LogoutModel_OnPost_WithReturnUrl_ReturnsLocalRedirect()
    {
        var signInMgr = CreateMockSignInManager();
        var logger = new Mock<ILogger<RazorLogoutModel>>();
        var model = new RazorLogoutModel(signInMgr.Object, logger.Object);
        model.PageContext = CreatePageContext();

        var result = await model.OnPost("/home");

        signInMgr.Verify(s => s.SignOutAsync(), Times.Once);
        result.Should().BeOfType<LocalRedirectResult>();
    }

    // ─── RegisterModel Tests ───

    [Fact]
    public void RegisterModel_InputModel_Properties()
    {
        var input = new RazorRegisterModel.InputModel
        {
            Email = "new@example.com",
            Password = "P@ss1234",
            ConfirmPassword = "P@ss1234"
        };
        input.Email.Should().Be("new@example.com");
        input.Password.Should().Be("P@ss1234");
        input.ConfirmPassword.Should().Be("P@ss1234");
    }

    private static RazorAdHoc::EqDemo.Services.DefaultReportGenerator CreateReportGenerator()
    {
        var tmpDir = Path.Combine(Path.GetTempPath(), $"EqIdentity_{Guid.NewGuid()}");
        var seedDir = Path.Combine(tmpDir, $"App_Data\\Seed");
        Directory.CreateDirectory(seedDir);
        var mockEnv = new Mock<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>();
        mockEnv.Setup(e => e.ContentRootPath).Returns(tmpDir);
        var options = new DbContextOptionsBuilder<RazorAdHoc::EqDemo.AppDbContext>()
            .UseInMemoryDatabase($"Identity_{Guid.NewGuid()}")
            .Options;
        var ctx = new RazorAdHoc::EqDemo.AppDbContext(options);
        return new RazorAdHoc::EqDemo.Services.DefaultReportGenerator(mockEnv.Object, ctx);
    }

    [Fact]
    public void RegisterModel_OnGet_SetsReturnUrl()
    {
        var userMgr = CreateMockUserManager();
        var signInMgr = CreateMockSignInManager();
        var logger = new Mock<ILogger<RazorRegisterModel>>();
        var emailSender = new Mock<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender>();
        var reportGen = CreateReportGenerator();

        var model = new RazorRegisterModel(
            userMgr.Object, signInMgr.Object, logger.Object,
            emailSender.Object, reportGen);

        model.OnGet("/return");
        model.ReturnUrl.Should().Be("/return");
    }

    [Fact]
    public void RegisterModel_OnGet_NullReturnUrl()
    {
        var userMgr = CreateMockUserManager();
        var signInMgr = CreateMockSignInManager();
        var logger = new Mock<ILogger<RazorRegisterModel>>();
        var emailSender = new Mock<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender>();
        var reportGen = CreateReportGenerator();

        var model = new RazorRegisterModel(
            userMgr.Object, signInMgr.Object, logger.Object,
            emailSender.Object, reportGen);

        model.OnGet(null);
        model.ReturnUrl.Should().BeNull();
    }

    [Fact]
    public async Task RegisterModel_OnPostAsync_UserCreationFails_ReturnsPageWithErrors()
    {
        var userMgr = CreateMockUserManager();
        userMgr.Setup(u => u.CreateAsync(It.IsAny<IdentityUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Password too weak" }));

        var signInMgr = CreateMockSignInManager();
        var logger = new Mock<ILogger<RazorRegisterModel>>();
        var emailSender = new Mock<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender>();
        var reportGen = CreateReportGenerator();

        var model = new RazorRegisterModel(
            userMgr.Object, signInMgr.Object, logger.Object,
            emailSender.Object, reportGen);
        model.PageContext = CreatePageContext();
        model.Input = new RazorRegisterModel.InputModel
        {
            Email = "a@b.c", Password = "P@ss1234", ConfirmPassword = "P@ss1234"
        };

        var urlHelper = new Mock<IUrlHelper>();
        urlHelper.Setup(u => u.Content(It.IsAny<string>())).Returns("/");
        model.Url = urlHelper.Object;

        var result = await model.OnPostAsync();
        result.Should().BeOfType<PageResult>();
        model.ModelState.ErrorCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task RegisterModel_OnPostAsync_Success_RedirectsLocal()
    {
        var userMgr = CreateMockUserManager();
        userMgr.Setup(u => u.CreateAsync(It.IsAny<IdentityUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        userMgr.Setup(u => u.AddToRoleAsync(It.IsAny<IdentityUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        userMgr.Setup(u => u.GenerateEmailConfirmationTokenAsync(It.IsAny<IdentityUser>()))
            .ReturnsAsync("token123");

        var signInMgr = CreateMockSignInManager();
        var logger = new Mock<ILogger<RazorRegisterModel>>();
        var emailSender = new Mock<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender>();
        emailSender.Setup(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        var reportGen = CreateReportGenerator();

        var model = new RazorRegisterModel(
            userMgr.Object, signInMgr.Object, logger.Object,
            emailSender.Object, reportGen);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Scheme = "https";
        model.PageContext = new PageContext
        {
            HttpContext = httpContext
        };

        model.Input = new RazorRegisterModel.InputModel
        {
            Email = "new@test.com", Password = "P@ss1234", ConfirmPassword = "P@ss1234"
        };

        var urlHelper = new Mock<IUrlHelper>();
        urlHelper.Setup(u => u.Content(It.IsAny<string>())).Returns("/");
        urlHelper.Setup(u => u.IsLocalUrl(It.IsAny<string>())).Returns(true);
        urlHelper.Setup(u => u.RouteUrl(It.IsAny<UrlRouteContext>()))
            .Returns("/confirm?code=token123");
        var actionContext = new ActionContext(httpContext, new RouteData(), new CompiledPageActionDescriptor());
        urlHelper.Setup(u => u.ActionContext).Returns(actionContext);
        model.Url = urlHelper.Object;

        var result = await model.OnPostAsync("/home");

        result.Should().BeOfType<LocalRedirectResult>();
        signInMgr.Verify(s => s.SignInAsync(It.IsAny<IdentityUser>(), false, null), Times.Once);
    }

    [Fact]
    public async Task RegisterModel_OnPostAsync_Success_AddToRoleFails_StillRedirects()
    {
        var userMgr = CreateMockUserManager();
        userMgr.Setup(u => u.CreateAsync(It.IsAny<IdentityUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        userMgr.Setup(u => u.AddToRoleAsync(It.IsAny<IdentityUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed());
        userMgr.Setup(u => u.GenerateEmailConfirmationTokenAsync(It.IsAny<IdentityUser>()))
            .ReturnsAsync("token");

        var signInMgr = CreateMockSignInManager();
        var logger = new Mock<ILogger<RazorRegisterModel>>();
        var emailSender = new Mock<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender>();
        emailSender.Setup(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        var reportGen = CreateReportGenerator();

        var model = new RazorRegisterModel(
            userMgr.Object, signInMgr.Object, logger.Object,
            emailSender.Object, reportGen);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Scheme = "https";
        model.PageContext = new PageContext { HttpContext = httpContext };
        model.Input = new RazorRegisterModel.InputModel
        {
            Email = "a@b.c", Password = "P@ss1234", ConfirmPassword = "P@ss1234"
        };

        var urlHelper = new Mock<IUrlHelper>();
        urlHelper.Setup(u => u.Content(It.IsAny<string>())).Returns("/");
        urlHelper.Setup(u => u.RouteUrl(It.IsAny<UrlRouteContext>()))
            .Returns("/confirm");
        var actionContext = new ActionContext(httpContext, new RouteData(), new CompiledPageActionDescriptor());
        urlHelper.Setup(u => u.ActionContext).Returns(actionContext);
        model.Url = urlHelper.Object;

        var result = await model.OnPostAsync("/home");
        result.Should().BeOfType<LocalRedirectResult>();
    }

    [Fact]
    public async Task RegisterModel_OnPostAsync_InvalidModelState_ReturnsPage()
    {
        var userMgr = CreateMockUserManager();
        var signInMgr = CreateMockSignInManager();
        var logger = new Mock<ILogger<RazorRegisterModel>>();
        var emailSender = new Mock<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender>();
        var reportGen = CreateReportGenerator();

        var model = new RazorRegisterModel(
            userMgr.Object, signInMgr.Object, logger.Object,
            emailSender.Object, reportGen);
        model.PageContext = CreatePageContext();

        var urlHelper = new Mock<IUrlHelper>();
        urlHelper.Setup(u => u.Content(It.IsAny<string>())).Returns("/");
        model.Url = urlHelper.Object;

        model.ModelState.AddModelError("", "error");

        var result = await model.OnPostAsync();
        result.Should().BeOfType<PageResult>();
    }
}
