﻿using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Foundation.Test.Api.Middlewares.WebApi.Tests
{
    [TestClass]
    public class WebApiODataSecurityTests
    {
        [TestMethod]
        [TestCategory("WebApi"), TestCategory("Security")]
        public async Task LoggedInUsersMustHaveAccessToODataWebApis()
        {
            using (TestEnvironment testEnvironment = new TestEnvironment())
            {
                OAuthToken token = testEnvironment.Server.Login("ValidUserName", "ValidPassword");

                HttpResponseMessage getMetadataResponse = await testEnvironment.Server.GetHttpClient(token)
                        .GetAsync("/odata/Foundation/$metadata");

                Assert.AreEqual(HttpStatusCode.OK, getMetadataResponse.StatusCode);
            }
        }

        [TestMethod]
        [TestCategory("WebApi"), TestCategory("Security")]
        public async Task WebApiResponsesMustHaveSecurityHeaders()
        {
            using (TestEnvironment testEnvironment = new TestEnvironment())
            {
                OAuthToken token = testEnvironment.Server.Login("ValidUserName", "ValidPassword");

                HttpResponseMessage getMetadataResponse = await testEnvironment.Server.GetHttpClient(token)
                        .GetAsync("/odata/Foundation/$metadata");

                Assert.AreEqual(true, getMetadataResponse.Headers.Contains("X-Content-Type-Options"));
            }
        }

        [TestMethod]
        [TestCategory("WebApi"), TestCategory("Security")]
        public async Task NotLoggedInUsersMustHaveAccessToMetadataAnyway()
        {
            using (TestEnvironment testEnvironment = new TestEnvironment())
            {
                HttpResponseMessage getMetadataResponse = await testEnvironment.Server.GetHttpClient()
                        .GetAsync("/odata/Test/$metadata");

                Assert.AreEqual(HttpStatusCode.OK, getMetadataResponse.StatusCode);
            }
        }

        [TestMethod]
        [TestCategory("WebApi"), TestCategory("Security")]
        public async Task NotLoggedInUsersMustNotHaveAccessToProtectedResources()
        {
            using (TestEnvironment testEnvironment = new TestEnvironment())
            {
                HttpResponseMessage getMetadataResponse = await testEnvironment.Server.GetHttpClient()
                        .GetAsync("/odata/Test/ValidationSamples/");

                Assert.AreEqual(HttpStatusCode.Unauthorized, getMetadataResponse.StatusCode);
            }
        }

        [TestMethod]
        [TestCategory("WebApi"), TestCategory("Security")]
        public async Task NotLoggedInUsersMustHaveAccessToUnProtectedWebApis()
        {
            using (TestEnvironment testEnvironment = new TestEnvironment())
            {
                HttpResponseMessage getTestModels = await testEnvironment.Server.GetHttpClient()
                        .GetAsync("/odata/Test/TestModels");

                Assert.AreEqual(HttpStatusCode.OK, getTestModels.StatusCode);
            }
        }
    }
}
