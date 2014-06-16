public static class ProxyObjectFactory
{
    private static ProxyGenerator _proxyGenerator = new ProxyGenerator();
    public static T Create<T>() where T : class, IIntercept
    {
        return _proxyGenerator.CreateClassProxy<T>(new GenericInterceptor());
    }
}
