using Fams3Adapter.Dynamics.Duplicate;
using Fams3Adapter.Dynamics.Identifier;
using Fams3Adapter.Dynamics.SearchApiRequest;
using Fams3Adapter.Dynamics.SearchResult;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Simple.OData.Client;
using System;

namespace Fams3Adapter.Dynamics.Test.SearchResult
{
    public class PersonSearchResultServiceTest
    {
        private Mock<IODataClient> _odataClientMock;
        private Mock<BatchEngine> _batchEngineMock;
        private Mock<IDuplicateDetectionService> _duplicateServiceMock;
        private ODataBatch _oDataBatch;
        private Mock<ILogger<PersonSearchResultService>> _loggerMock;
       
        private PersonSearchResultService _sut;

        private SSG_SearchApiRequest _fakeSearchApiRequest;
        private int? _fakeDataProvider = 123456789;
        private SSG_Identifier _fakeIdentifier;
        private Mock<ODataClientSettings> settings;
        

        [SetUp]
        public void SetUp()
        { 
            settings = new Mock<ODataClientSettings>();
            
            _fakeSearchApiRequest = new SSG_SearchApiRequest { SearchRequestId = Guid.NewGuid(), SearchApiRequestId = Guid.NewGuid()};
            _fakeIdentifier = new SSG_Identifier { IdentifierId = Guid.NewGuid() };
            _batchEngineMock = new Mock<BatchEngine>();
            _odataClientMock = new Mock<IODataClient>();
            _oDataBatch = new ODataBatch(_odataClientMock.Object);
            _duplicateServiceMock = new Mock<IDuplicateDetectionService>();
            _loggerMock = new Mock<ILogger<PersonSearchResultService>>();
            _sut = new PersonSearchResultService(_odataClientMock.Object, _oDataBatch, _batchEngineMock.Object, _duplicateServiceMock.Object, _loggerMock.Object);
        }

        [Test]
        public void with_valid_person_should_batch_message_to_dynamics()
        {
            Assert.IsNotNull(_sut);
        }

    }
}
