using Microsoft.AspNetCore.Mvc;
using ServiceGenerator;

namespace SourceGTest.Controllers;

public class BaseClassTest : BaseClassTest1;

public class BaseClassTest1 : TestClass;

[InheritableScoped(typeof(string),typeof(int))]
[InheritableScoped<object>]
public class TestClass : ControllerBase;