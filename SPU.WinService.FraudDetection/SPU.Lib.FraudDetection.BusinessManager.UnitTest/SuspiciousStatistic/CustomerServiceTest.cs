namespace FD.Lib.BusinessManager.UnitTest.SuspiciousStatistic
{
    using System.Collections.Generic;
    using System.Messaging;
    using Criteria.SuspiciousStatistic;
    using DTO;
    using Fanex.Data.Configuration;
    using NUnit.Framework;
    using Service.Implement;

    [TestFixture]
    public class CustomerServiceTest
    {
        [SetUp]
        public void SetUp()
        {
            DbSettingProviderManager.Current.Start();
        }
        
        [Test]
        [Ignore("Ignore a fixture")]
        public void TestGetUsernameByCustId()
        {
            var sut = new CustomerService();

            var abao = sut.GetUsernameByCustId(new CustomerSelectCriteria
            {
                ListCustIds = "1,2,3,4,5,6"
            });
        }

        [Test]
        public void TestFD_GetMatchInfoFromDB02()
        {
            var sut = new MatchService();

            var abao = sut.GetMatchDetails(new MatchSelectCriteria
            {
                ListMatchIds = "9689236,9908275,9908536,11880163",
                IsDbArchieve = true
            });
        }

        [Test]
        public void TestGetSuspiciousCustomerStatistics()
        {
            var sut = new SuspiciousStatisticsReportService();

            IList<SuspiciousInfo> list = new List<SuspiciousInfo>
            {
                new SuspiciousInfo
                {
                    CustId = 1280,
                    MatchId = 13606626
                },
                new SuspiciousInfo
                {
                    CustId = 1280,
                    MatchId = 13659022
                },
                new SuspiciousInfo
                {
                    CustId = 1280,
                    MatchId = 13675244
                }
            };

            var abao = sut.GetSuspiciousCustomerStatistics(list);
        }
    }
}