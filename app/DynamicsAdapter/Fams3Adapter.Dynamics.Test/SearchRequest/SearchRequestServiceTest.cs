﻿using Fams3Adapter.Dynamics.Identifier;
using Fams3Adapter.Dynamics.SearchRequest;
using Moq;
using NUnit.Framework;
using Simple.OData.Client;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Fams3Adapter.Dynamics.Test.SearchRequest
{
    public class SearchRequestServiceTest
    {
        private Mock<IODataClient> odataClientMock;

        private readonly Guid testId = Guid.Parse("6AE89FE6-9909-EA11-B813-00505683FBF4");

        private SearchRequestService _sut;

        [SetUp]
        public void SetUp()
        {

            odataClientMock = new Mock<IODataClient>();

            odataClientMock.Setup(x => x.For<SSG_Identifier>(null).Set(It.Is<SSG_Identifier>(x => x.Identification == "identificationtest"))
            .InsertEntryAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(new SSG_Identifier()
            {
                Identification = "test"
            })
            );

            _sut = new SearchRequestService(odataClientMock.Object);
        }


        [Test]
        public async Task with_correct_searchRequestid_upload_identifier_should_success()
        {
            var identifier = new SSG_Identifier()
            {
                Identification = "identificationtest",
                IdentificationEffectiveDate = DateTime.Now,
                StateCode = 0,
                StatusCode = 1,
                SSG_SearchRequest = new SSG_SearchRequest() { SearchRequestId = testId }
            };

            var result = await _sut.UploadIdentifier(identifier, CancellationToken.None);

            Assert.AreEqual("test", result.Identification);
        }


    }
}
