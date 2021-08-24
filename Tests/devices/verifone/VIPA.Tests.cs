using Xunit;
using TestHelper;
using Devices.Verifone.VIPA;
using Common.XO.Private;
using System.Threading.Tasks;
using Devices.Verifone.Helpers;
using System.IO;
using System;
using System.Text;

namespace Devices.Verifone.Tests
{
    public class VIPATests
    {
        const int ResponseHandlerDelay = 100;

        readonly VIPAImpl subject;

        public VIPATests()
        {
            subject = new VIPAImpl();
        }

        [Theory]
        [InlineData("sphere.sphere.idle...199.m400.1.210823", "1")]
        [InlineData("sphere.sphere.vipa....m400.6_8_2_17.210714", "6_8_2_17")]
        [InlineData("sphere.sphere.emv.unattended.FD...6_8_2_19.210816", "6_8_2_19")]
        public void ProcessVersionString_Maps_Correctly_To_Version_Schema(string schemaValue, string expectedVersion)
        {
            DALBundleVersioning bundle = new DALBundleVersioning();

            Helper.CallPrivateMethod<int>("ProcessVersionString", subject, out int result, new object[] { bundle, schemaValue });

            Assert.Equal((int)VipaSW1SW2Codes.Success, result);
            Assert.False(string.IsNullOrEmpty(bundle.Version));
            Assert.Equal(expectedVersion, bundle.Version);
        }

        [Theory]
        [InlineData("idle_ver.txt", "sphere.sphere.idle...199.m400.1.210823", "1")]
        [InlineData("vipa_ver.txt", "sphere.sphere.vipa....m400.6_8_2_17.210810", "6_8_2_17")]
        [InlineData("emv_ver.txt", "sphere.sphere.emv.unattended.FD...6_8_2_19.210816", "6_8_2_19")]
        public void ProcessVersionString_Maps_Correctly_To_Version_Schema_WhenLoaded_FromFile(string fileVersion, string schemaValue, string expectedVersion)
        {
            string fileContexts;
            string fileName = Environment.CurrentDirectory;
            int position = fileName.IndexOf("bin");
            if (position > 0)
            { 
                fileName = Path.Combine(fileName.Substring(0, position), Path.Combine("Bundles", fileVersion));
            }

            using (var fileStream = new StreamReader(fileName, Encoding.UTF8))
            {
                fileContexts = fileStream.ReadToEnd();
            }

            Assert.Equal(schemaValue, fileContexts);

            DALBundleVersioning bundle = new DALBundleVersioning();

            Helper.CallPrivateMethod<int>("ProcessVersionString", subject, out int result, new object[] { bundle, fileContexts });

            Assert.Equal((int)VipaSW1SW2Codes.Success, result);
            Assert.False(string.IsNullOrEmpty(bundle.Version));
            Assert.Equal(expectedVersion, bundle.Version);
        }

        //[Theory]
        //[InlineData("m400", "sphere.sphere.vipa....m400.6_8_2_17.210714", false)]
        //public void VIPAVersions_ReturnsProperlyFormatted_Schema(string model, string schema, bool hmacEnabled)
        //{
        //    BinaryStatusObject binaryStatusObject = new BinaryStatusObject();

        //    // delay a little before triggering the response handler
        //    Task.Run(async () =>
        //    {
        //        // Device model/serial numbers and other info: ResetDevice
        //        await Task.Delay(ResponseHandlerDelay);
        //        subject.DeviceBinaryStatusInformation.TrySetResult((binaryStatusObject, (int)VipaSW1SW2Codes.Failure));


        //    });

        //    subject.VIPAVersions(model, hmacEnabled, "199");
        //}
    }
}
