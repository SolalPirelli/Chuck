# Close VS & run this script to update the test runner.

foreach($dir in (".\bin-pack",".\packages", "$env:TEMP\VisualStudioTestExplorerExtensions")) {
  rd $dir -Recurse -Force -ErrorAction SilentlyContinue | out-null
}

mkdir .\bin-pack -ErrorAction SilentlyContinue | out-null
tools\NuGet.exe pack src\Chuck\Chuck.csproj -Symbols -Prop Configuration=Release -OutputDirectory .\bin-pack