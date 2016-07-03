using System;
using System.IO;
using System.Security;
using System.Security.Permissions;
using Chuck.Remoting;

namespace Chuck.VisualStudio
{
    public sealed class AppDomainManager : IDisposable
    {
        private readonly AppDomain _domain;


        public AppDomainManager( string assemblyPath )
        {
            var setup = new AppDomainSetup
            {
                ApplicationBase = Path.GetDirectoryName( Path.GetFullPath( assemblyPath ) ),
                ApplicationName = Guid.NewGuid().ToString(),
                ShadowCopyFiles = "true" // yes, it's a string...
            };
            
            _domain = AppDomain.CreateDomain(
                setup.ApplicationName,
                AppDomain.CurrentDomain.Evidence,
                setup,
                new PermissionSet( PermissionState.Unrestricted )
            );
        }


        public RemoteTestProxy CreateProxy()
        {
            return (RemoteTestProxy) _domain.CreateInstanceAndUnwrap(
                typeof( RemoteTestProxy ).Assembly.FullName,
                typeof( RemoteTestProxy ).FullName
            );
        }

        public void Dispose()
        {
            AppDomain.Unload( _domain );
        }
    }
}