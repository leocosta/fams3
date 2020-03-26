﻿using Fams3Adapter.Dynamics.Address;
using Fams3Adapter.Dynamics.Employment;
using Fams3Adapter.Dynamics.Identifier;
using Fams3Adapter.Dynamics.Name;
using Fams3Adapter.Dynamics.Person;
using Fams3Adapter.Dynamics.PhoneNumber;
using Fams3Adapter.Dynamics.RelatedPerson;
using Fams3Adapter.Dynamics.SearchRequest;
using Fams3Adapter.Dynamics.Types;
using Moq;
using NUnit.Framework;
using Simple.OData.Client;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Fams3Adapter.Dynamics.Test.SearchRequest
{
    public class SearchRequestServiceTest
    {
        private Mock<IODataClient> odataClientMock;

        private readonly Guid testId = Guid.Parse("6AE89FE6-9909-EA11-B813-00505683FBF4");
        private readonly Guid testPersonId = Guid.Parse("6AE89FE6-9909-EA11-1111-00505683FBF4");

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

            odataClientMock.Setup(x => x.For<SSG_Country>(null)
                            .Filter(It.IsAny<Expression<Func<SSG_Country, bool>>>())
                            .FindEntryAsync(It.IsAny<CancellationToken>()))
                            .Returns(Task.FromResult<SSG_Country>(new SSG_Country()
                            {
                                CountryId = Guid.NewGuid(),
                                Name = "Canada"
                            }));

            odataClientMock.Setup(x => x.For<SSG_CountrySubdivision>(null)
                .Filter(It.IsAny<Expression<Func<SSG_CountrySubdivision, bool>>>())
                .FindEntryAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<SSG_CountrySubdivision>(new SSG_CountrySubdivision()
                {
                    CountrySubdivisionId = Guid.NewGuid(),
                    Name = "British Columbia"
                }));

            odataClientMock.Setup(x => x.For<SSG_Address>(null).Set(It.Is<SSG_Address>(x => x.AddressLine1 == "address full text"))
            .InsertEntryAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(new SSG_Address()
            {
                AddressLine1 = "test"
            })
            );

            odataClientMock.Setup(x => x.For<SSG_PhoneNumber>(null).Set(It.Is<SSG_PhoneNumber>(x => x.TelePhoneNumber == "4007678231"))
           .InsertEntryAsync(It.IsAny<CancellationToken>()))
           .Returns(Task.FromResult(new SSG_PhoneNumber()
           {
               TelePhoneNumber = "4007678231"
           })
           );

            odataClientMock.Setup(x => x.For<SSG_Aliase>(null).Set(It.Is<SSG_Aliase>(x => x.FirstName == "firstName"))
            .InsertEntryAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(new SSG_Aliase()
            {
               FirstName = "firstName"
            })
            );

            odataClientMock.Setup(x => x.For<SSG_Person>(null).Set(It.Is<PersonEntity>(x => x.FirstName == "First"))
          .InsertEntryAsync(It.IsAny<CancellationToken>()))
          .Returns(Task.FromResult(new SSG_Person()
          {
              FirstName = "FirstName",
              PersonId = testPersonId
          })
          );

            odataClientMock.Setup(x => x.For<SSG_Identity>(null).Set(It.Is<SSG_Identity>(x => x.FirstName == "First"))
            .InsertEntryAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(new SSG_Identity()
            {
              FirstName = "FirstName"
            })
            );

            odataClientMock.Setup(x => x.For<SSG_Employment>(null).Set(It.Is<EmploymentEntity>(x => x.BusinessOwner == "Business Owner"))
         .InsertEntryAsync(It.IsAny<CancellationToken>()))
         .Returns(Task.FromResult(new SSG_Employment()
         {
             BusinessOwner = "Business Owner"
         })
         );

            odataClientMock.Setup(x => x.For<SSG_EmploymentContact>(null).Set(It.Is<SSG_EmploymentContact>(x => x.PhoneNumber == "12345678"))
            .InsertEntryAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(new SSG_EmploymentContact()
            {
             PhoneNumber = "12345678"
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
                //IdentificationEffectiveDate = DateTime.Now,
                StateCode = 0,
                StatusCode = 1,
                SearchRequest = new SSG_SearchRequest() { SearchRequestId = testId },
                Person = new SSG_Person() { PersonId = testPersonId }
            };

            var result = await _sut.CreateIdentifier(identifier, CancellationToken.None);

            Assert.AreEqual("test", result.Identification);
        }


        [Test]
        public async Task with_correct_searchRequestid_upload_person_should_success()
        {
            var person = new PersonEntity()
            {
                FirstName = "First",
                LastName = "lastName",
                MiddleName = "middleName",
                ThirdGivenName = "Third",
                DateOfBirth = null,
                DateOfDeath = null,
                DateOfDeathConfirmed = false,
                Incacerated = 86000071,
                StateCode = 0,
                StatusCode = 1,
                SearchRequest = new SSG_SearchRequest() { SearchRequestId = testId }
            };

            var result = await _sut.SavePerson(person, CancellationToken.None);

            Assert.AreEqual("FirstName", result.FirstName);
            Assert.AreEqual(testPersonId, result.PersonId);
        }
        [Test]
        public async Task with_correct_searchRequestid_upload_phone_number_should_success()
        {
            var phone = new SSG_PhoneNumber()
            {

                Date1 = DateTime.Now,
                Date1Label = "Effective Date",
                Date2 = new DateTime(2001, 1, 1),
                Date2Label = "Expiry Date",
                TelePhoneNumber = "4007678231",
                StateCode = 0,
                StatusCode = 1,
                SearchRequest = new SSG_SearchRequest() { SearchRequestId = testId },
                Person = new SSG_Person() { PersonId = testPersonId }
            };

            var result = await _sut.CreatePhoneNumber(phone, CancellationToken.None);

            Assert.AreEqual("4007678231", result.TelePhoneNumber);
        }

        [Test]
        public async Task with_correct_searchRequestid_upload_address_should_success()
        {
            var address = new SSG_Address()
            {
                AddressLine1 = "address full text",
                CountryText = "canada",
                CountrySubdivisionText = "British Columbia",
                SearchRequest = new SSG_SearchRequest() { SearchRequestId = testId },
                Person = new SSG_Person() { PersonId = testPersonId }
            };

            var result = await _sut.CreateAddress(address, CancellationToken.None);

            Assert.AreEqual("test", result.AddressLine1);
        }

        [Test]
        public async Task with_correct_searchRequestid_upload_name_should_success()
        {
            var name = new SSG_Aliase()
            {
                FirstName = "firstName",
                LastName = "lastName",
                MiddleName = "middleName",
                Comments = "testComments",
                Type = PersonNameCategory.LegalName.Value,
                Notes = "notes",
                ThirdGivenName ="thirdName",
                SupplierTypeCode = "legal",
                Date1 = new DateTime(2001,1,1),
                Date1Label = "date1lable",
                SearchRequest = new SSG_SearchRequest() { SearchRequestId = testId },
                Person = new SSG_Person() { PersonId = testPersonId }
            };

            var result = await _sut.CreateName(name, CancellationToken.None);

            Assert.AreEqual("firstName", result.FirstName);
        }

        [Test]
        public async Task with_correct_searchRequestid_upload_related_person_should_success()
        {
            var relatedPerson = new SSG_Identity()
            {
                FirstName = "First",
                LastName = "lastName",
                MiddleName = "middleName",
                ThirdGivenName = "otherName",
                Type = PersonRelationType.Friend.Value,
                Notes = "notes",
                SupplierRelationType = "friend",
                Date1 = new DateTime(2001, 1, 1),
                Date1Label = "date1lable",
                Date2 = new DateTime(2005, 1, 1),
                Date2Label = "date2lable",
                Gender = GenderType.Female.Value,
                Description = "description",
                SearchRequest = new SSG_SearchRequest() { SearchRequestId = testId },
                Person = new SSG_Person() { PersonId = testPersonId }
            };

            var result = await _sut.CreateRelatedPerson(relatedPerson, CancellationToken.None);

            Assert.AreEqual("FirstName", result.FirstName);
        }


        [Test]
        public async Task with_correct_searchRequestid_upload_employment_should_succed()
        {
            var employment = new EmploymentEntity()
            {
                BusinessOwner= "Business Owner",
                BusinessName = "Business Name",
                Notes = "notes",
                Date1 = new DateTime(2001, 1, 1),
                Date1Label = "date1lable",
                Date2 = new DateTime(2005, 1, 1),
                Date2Label = "date2lable",
        
                SearchRequest = new SSG_SearchRequest() { SearchRequestId = testId },
                Person = new SSG_Person() { PersonId = testPersonId }
            };

            var result = await _sut.CreateEmployment(employment, CancellationToken.None);

            Assert.AreEqual("Business Owner", result.BusinessOwner);
        }

        [Test]
        public async Task with_correct_employmentid_upload_employmentcontact_should_succed()
        {
            var employmentContact = new SSG_EmploymentContact()
            {
                Employment = new SSG_Employment() { EmploymentId = testId },
                PhoneNumber = "12345678"
            };

            var result = await _sut.CreateEmploymentContact(employmentContact, CancellationToken.None);

            Assert.AreEqual("12345678", result.PhoneNumber);
        }
    }
}
