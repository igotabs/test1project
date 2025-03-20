using ServiceTests.Fixtures;

namespace ServiceTests
{
	public class UnitTest1 : IClassFixture<HostTestFixture>
	{
		private readonly HostTestFixture _fixture;

		public UnitTest1(HostTestFixture fixture)
		{
			_fixture = fixture;
		}
		[Fact]
		public void Test1() 
		{

		}
	}
}