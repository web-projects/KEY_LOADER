using Devices.Core.Providers;
using Devices.Core.Cancellation;
using Xunit;

namespace Devices.Core.State.Providers.Tests
{
    public class DeviceCancellationBrokerProviderImplTests
    {
        readonly DeviceCancellationBrokerProviderImpl subject;

        public DeviceCancellationBrokerProviderImplTests()
        {
            subject = new DeviceCancellationBrokerProviderImpl();
        }

        [Fact]
        void ValidateProviderType_Matches_SubjectType()
           => Assert.IsType<DeviceCancellationBrokerImpl>(subject.GetDeviceCancellationBroker());
    }
}
