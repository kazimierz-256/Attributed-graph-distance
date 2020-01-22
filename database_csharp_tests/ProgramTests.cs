using System;
using Xunit;

namespace database_csharp_tests
{
    public class ProgramTests
    {
        [Theory]
        [InlineData("Mon, 7 May 2001 11:54:00 -0700 (PDT)", "7/5/2001 11:54:00 -0700")]
        [InlineData("Mon, 18 Dec 2000 12:22:00 -0800 (PST)", "18 Dec 2000 12:22:00 -0800")]
        [InlineData("Fri, 27 Oct 2000 03:30:00 -0700 (PDT)", "27 Oct 2000 03:30:00 -0700")]
        [InlineData("Tue, 2 Jan 2001 13:04:00 -0800 (PST)", "2 Jan 2001 13:04:00 -0800")]
        [InlineData("Fri, 23 Feb 2001 01:52:00 -0800 (PST)", "23 Feb 2001 01:52:00 -0800")]
        [InlineData("Thu, 5 Apr 2001 04:52:00 -0700 (PDT)", "5 Apr 2001 04:52:00 -0700")]
        [InlineData("Mon, 14 Aug 2000 06:58:00 -0700 (PDT)", "14 Aug 2000 06:58:00 -0700")]
        public void DateTest(string stringDate, string expectedDateString)
        {
            var expectedDate = Convert.ToDateTime(expectedDateString);
            var actualDate = database_csharp.Program.ParseDatabaseDate(stringDate);
            Assert.Equal(expectedDate, actualDate);
        }
    }
}
