﻿using GirafRest.Controllers;using GirafRest.Models;using GirafRest.Models.DTOs.AccountDTOs;using GirafRest.Models.DTOs;using GirafRest.Test.Mocks;using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;using Microsoft.AspNetCore.Mvc;using Moq;using System.Collections.Generic;using System.Linq;using Xunit;using Xunit.Abstractions;using static GirafRest.Test.UnitTestExtensions;namespace GirafRest.Test{    public class AccountControllerTest    {
        private readonly ITestOutputHelper _outputHelpter;
        private TestContext _testContext;
        private const int USER = 0;        private const int DEPARTMENT_ZERO = 0;
        public AccountControllerTest(ITestOutputHelper outputHelpter)
        {
            _outputHelpter = outputHelpter;
        }

        private AccountController initializeTest()
        {
            _testContext = new TestContext();
            var context = new Mock<HttpContext>();
            var contextAccessor = new Mock<IHttpContextAccessor>();
            contextAccessor.Setup(x => x.HttpContext).Returns(context.Object);
            AccountController ac = new AccountController(_testContext.MockUserManager, new MockSignInManager(_testContext.MockUserManager ,contextAccessor.Object), null, null, _testContext.MockLoggerFactory.Object);
            _testContext.MockHttpContext = ac.MockHttpContext();
            return ac;
        }

        [Fact]
        public void Login_CredentialsOk_ExpectOK()
        {
            Assert.True(false, "Not implemented");
            var accountController = initializeTest();
            // Indsæt korrekt password
            var res = accountController.Login(new LoginDTO()
            { Username =  _testContext.MockUsers[USER].UserName, Password = ""});
            Assert.IsType<OkResult>(res.Result);
        }

        [Fact]
        public void Login_CredentialsNotOk_ExpectUnauthorized()
        {
            var accountController = initializeTest();
            var res = accountController.Login(new LoginDTO()
            { Username = "CredentialsNotOk", Password = "CredentialsNotOk" });
            Assert.IsType<UnauthorizedResult>(res.Result);
        }

        [Fact]
        public void Register_InputOk_ExpectOK()
        {
            var accountController = initializeTest();
            var res = accountController.Register( new RegisterDTO()
            { Username = "InputOk", Password = "InputOk", ConfirmPassword = "InputOk", DepartmentId = DEPARTMENT_ZERO});
            Assert.IsType<OkResult>(res.Result);
        }

        [Fact]
        public void Register_InputExist_ExpectBadRequest()
        {
            Assert.True(false, "Not implemented");
            // Indsæt korrekt password
            var accountController = initializeTest();
            var res = accountController.Register(new RegisterDTO()
            { Username = _testContext.MockUsers[USER].UserName, Password = "", ConfirmPassword = "", DepartmentId = DEPARTMENT_ZERO });
            Assert.IsType<BadRequestResult>(res.Result);
        }

        [Fact]
        public void Logout_UserLoggedIn_ExpectOK()
        {
            var accountController = initializeTest();
            _testContext.MockUserManager.MockLoginAsUser(_testContext.MockUsers[USER]);
            var res = accountController.Logout();
            Assert.IsType<OkResult>(res.Result);
        }

        [Fact]
        public void Logout_UserNotLoggedIn_ExpectBadRequest()
        {
            var accountController = initializeTest();
            _testContext.MockUserManager.MockLogout();
            var res = accountController.Logout();
            Assert.IsType<BadRequestResult>(res.Result);
        }

        [Fact]
        public void ForgotPassword_UserExist_ExpectOk()
        {
            Assert.True(false, "Not implemented");
            var accountController = initializeTest();
            // Indsæt korrekt email
            var res = accountController.ForgotPassword(new ForgotPasswordDTO()
            { Username = _testContext.MockUsers[USER].UserName, Email = "" });
            Assert.IsType<OkResult>(res.Result);
        }

        [Fact]
        public void ForgotPassword_UserDoNotExist_ExpectNotFound()
        {
            var accountController = initializeTest();
            var res = accountController.ForgotPassword(new ForgotPasswordDTO()
            { Username = "UserDoNotExist", Email = "UserDoNotExist@UserDoNotExist.com" });
            Assert.IsType<OkResult>(res.Result);
        }

        [Fact]
        public void ResetPassword_() // hvad skal der testes ved den?
        {
            Assert.True(false, "Not implemented");
        }

        [Fact]
        public void ResetPasswordConfirmation_ExpectViewReturned()
        {
            Assert.True(false, "Not implemented");
        }

        [Fact]
        public void AccessDenied_ExpectUnauthorized()
        {
            Assert.True(false, "Not implemented");
        }
    }}