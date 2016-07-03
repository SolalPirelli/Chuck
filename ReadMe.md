![Chuck Quizmo, from Paper Mario](http://www.mariowiki.com/images/8/8c/ChuckQuizmo_PM.png)

Quiz your code!
===============

A work-in-progress testing library for .NET.

**Goals**:

- Support testing in C# and F# with minimal ceremony
- Make code reuse easy, without boilerplate
- Use an easy-to-read assertion style
- Provide helpful mismatch messages
- Fail fast on common mistakes (e.g. async void)

**TODO**:

- [x] Basic testing support
- [x] Visual Studio test runner
- [ ] Locate (file/line) test methods
- [ ] Test the testing library!
- [ ] Pretty printing for values
- [ ] Matchers for common tasks
- [ ] Parallelize all the things
- [ ] XML doc
- [ ] Documentation, including design decisions and a detailed VS test runner explanation
- [ ] Find a better name, with a less obscure reference :)
- [ ] .NET Core runner
- [ ] Look at useful features from other libraries
- [ ] Consider factoring the assertions/matchers into a separate assembly


Tests
-----

Test assemblies must be marked with the `TestAssembly` attribute.

All public methods, including inherited and static ones, are considered as tests.  
To exclude a type, mark it with the `NoTests` attribute.

Methods can return `void` for synchronous tests or `Task` for asynchronous tests.

Methods and types may be skipped with the `Skip` attribute.

Assertions
----------

The `Assert` method is the single entry point into assertion logic.

Individual assertions are represented by a call to `That`, using the actual value and a matcher.

Parameterized tests
-------------------

Methods can be parameterized and annotated with data attributes; they will be run once per set of parameters.

Built-in data attributes are:

- `InlineData` to specify the parameters directly in the attribute definition.  
  This is the simplest option, useful for basic scenarios with primitive argument types such as integers and strings.
- `MethodData` to specify a static method returning `IEnumerable<object[]>`. Each item is a set of parameters.  
  This is more verbose, but enables generating large quantities of parameter sets, and using any kind of object.  
  The method may have arguments, which are resolved in the same way as class constructor arguments (see "Code reuse across tests").

Other data attributes can be defined by inheriting from `DataAttribute`.

Code reuse across tests
-----------------------

Classes are instantiated before each test, and disposed afterwards if they implement `IDisposable`.  
This includes parameterized methods: a method's class will be instantiated once per set of parameters.

Use class constructors and `IDisposable` to share simple logic across tests in the same class.

Classes may have constructor parameters, which will be instantiated once for the entire test run,
i.e. shared across all classes that ask for their type, and disposed at the end of the run if they
implement `IDisposable`. The parameters must have a public parameterless constructor.

Use class parameters to share expensive logic across tests, possibly in different classes.