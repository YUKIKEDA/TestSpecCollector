namespace TestSpecCollectorTests.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class TestIdAttribute : Attribute
    {
        public string Id { get; }

        public TestIdAttribute(string id)
        {
            Id = id;
        }
    }
}
