/// <summary>
/// Interceptor that checks if the proxied object inherits from IIntercept interface and executes it's Intercept() method
/// </summary>
public class GenericInterceptor : Castle.DynamicProxy.IInterceptor
{
    public void Intercept(IInvocation invocation)
    {
        if (invocation.Proxy is IIntercept)
        {
            ((IIntercept)invocation.Proxy).Intercept(invocation);
            return;
        }
        invocation.Proceed();
    }
}
