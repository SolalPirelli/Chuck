using Chuck.Infrastructure;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace Chuck.VisualStudio
{
    public static class VsConverter
    {
        public static TestCase ConvertTestCase( Test test )
        {
            // TODO test when Assembly2.Derived extends Assembly1.Base -> DeclaringType = ???
            var fullName = test.Type.FullName + "." + test.Method.Name;

            return new TestCase( fullName, VsTestExecutor.Uri, test.Type.Assembly.Location )
            {
                DisplayName = test.Method.Name
            };
        }
    }
}