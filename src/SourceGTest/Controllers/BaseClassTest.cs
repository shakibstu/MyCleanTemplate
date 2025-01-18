using Microsoft.AspNetCore.Mvc;
using ServiceGenerator;

namespace SourceGTest.Controllers
{
    public class BaseClassTest : BaseClassTest1
    {
    }

    public class BaseClassTest1 : TestClass
    {
    }

    [InheritableScopedDependency(typeof(string),typeof(int))]
    [InheritableScopedDependency<object>]
    public class TestClass : ControllerBase
    {

    }
}
