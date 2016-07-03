Requirements for a Visual Studio Test Adapter using NuGet
=====

* The assembly's name has to end with `.TestAdapter.dll`.
* When updating the assembly with the same version number, the cache in `%temp%\VisualStudioTestExplorerExtensions` needs to be deleted.
* Loading tests in a different AppDomain is required to be able to unload the test assembly afterwards.  
  However, the created AppDomain cannot access the test runner assembly since it is not referenced by the project (and thus not present in the bin folder).  
  Thus, some basic remoting features must exist in the main assembly rather than the runner assembly.