using System;
using System.Data.SqlClient;
using System.Linq;
using NPoco.FluentMappings;
using NPoco.Tests.Common;
using NUnit.Framework;

namespace NPoco.Tests
{
    [TestFixture]
    public class JkReferenceTests
    {
        private IDatabase _database;

        [TestFixtureSetUp]
        public void Setup()
        {
            var dbfactory = new DatabaseFactory();
            dbfactory
                .Config()
                .UsingDatabase(() => new Database(""));
            
            _database = dbfactory.GetDatabase();
        }

        [Test]
        public void QueryWithInclude()
        {
            var items = _database.QueryWithInclude<UserX>().ToList();

            Assert.IsNotNull(items.FirstOrDefault());
            Assert.IsNotNull(items.FirstOrDefault().Country);
        }

    }

    public class UserX
    {
        public string Name { get; set; }
        [Reference(ReferenceType.Foreign, ColumnName = "Country", ReferenceMemberName = "CountryId")]
        public CountryClass Country { get; set; }
        public int[] Values { get; set; }

        public class CountryClass
        {
            public int PhoneId { get; set; }
            public string Value { get; set; }
        }
    }
}
