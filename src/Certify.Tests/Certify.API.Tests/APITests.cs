﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Certify.SharedUtils;
using Microsoft.AspNetCore.Builder;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Certify.API.Tests
{

    public class Startup
    {
        public void Configure(IApplicationBuilder app)
        {

        }
    }

    [TestClass]
    public class APITests
    {
        Process _apiService;
        CertifyServerClient _client;

        [TestInitialize]
        public async Task InitTests()
        {
            _client = new CertifyServerClient(new ServiceConfigManager());

            // TODO : create API server instance instead of invoking directly
            if (_apiService == null)
            {
                var svcpath = Path.Combine(AppContext.BaseDirectory ,"..\\..\\..\\..\\..\\Certify.Service\\bin\\Debug\\net462\\CertifySSLManager.Service.exe");

                _apiService = Process.Start(svcpath);

                await Task.Delay(2000);
            }
        }

        [TestCleanup]
        public void TestCleanup()
        {
            if (_apiService != null)
            {
                _apiService.Kill();
                _apiService.WaitForExit();
            }
        }

        [TestMethod]
        public async Task CanConnectToAPIServer()
        {
            var result = await _client.IsAPIAvailable();
            Assert.IsTrue(result.IsSuccess, result.Message);
        }

        [TestMethod]
        public async Task CanFetchServerAPIVersion()
        {
            var result = await _client.GetSystemVersion();
            Assert.IsTrue(result.IsSuccess, result.Message);
        }

        [TestMethod]
        public async Task CanCreateAndDeleteManagedCertificate()
        {
            var request = new ManagedCertificateCreateOptions("Test", new List<string> {
                    "test.com",
                    "www.test.com"
                });

            var result = await _client.CreateManagedCertificate(request);

            var createdOK = result.IsSuccess;

            if (createdOK)
            {
                // delete test
                var deleteResult = await _client.DeleteManagedCertificate(result.Result.Id);

                Assert.IsTrue(deleteResult.IsSuccess, "Could not delete item");
            }

            Assert.IsTrue(createdOK, "Could not create item");

        }

        [TestMethod]
        public async Task CanRenewAllManagedCertificate()
        {
            var result = await _client.RenewAll();

            Assert.IsTrue(result.IsSuccess);
        }

    }
}
