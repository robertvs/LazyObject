/// <summary>
/// Will use dynamic proxy to intercept property calls and load property getters set by SetLazy
/// <para>User static Create() method to get lazy proxy instance</para>
/// </summary>
/// <typeparam name="TClass">The class to intercept with DynamicProxy and implement lazy loading on</typeparam>
public abstract class LazyObject<TClass> : IIntercept where TClass : class
{
    private Dictionary<string, Func<object>> _valueDelegates = new Dictionary<string, Func<object>>();
    private static ProxyGenerator _proxyGenerator = new ProxyGenerator();
    /// <summary>
    /// Will create a new Proxy instance of type TClass
    /// </summary>
    /// <returns></returns>
    public static TClass Create()
    {
        //return _proxyGenerator.CreateClassProxy<TClass>(new MiniProfilerInterceptor(), new GenericInterceptor());
        return _proxyGenerator.CreateClassProxy<TClass>(new GenericInterceptor());
    }

    /// <summary>
    /// Will add the valueGetter to an internal dictionary to be executed whan a property is beeing called
    /// </summary>
    /// <typeparam name="TProp">The type of property</typeparam>
    /// <param name="propertyGetter">A function to specify the property to set its value on</param>
    /// <param name="valueGetter">A function to get the properties value</param>
    public void SetLazy<TProp>(Expression<Func<TClass, TProp>> propertyGetter, Func<TProp> valueGetter)
    {
        //TODO: check of member is virtual
        var membExpr = propertyGetter.Body as MemberExpression;
        var propName = membExpr.Member.Name;
        _valueDelegates[propName] = () =>
        {
            return valueGetter();
        };
        //RemoveFromCache(propName);
    }

    /// <summary>
    /// Method will be called from DynamicProxy interceptor
    /// </summary>
    /// <param name="invocation"></param>
    public void Intercept(IInvocation invocation)
    {
        //if not GET/SET, proceed
        if (!invocation.Method.IsSpecialName)
        {
            invocation.Proceed();
            return;
        }
        if (IsGetMethod(invocation))
        {
            InterceptGet(invocation);
            return;
        }
        invocation.Proceed();
    }

    /// <summary>
    /// Will check if the method call is a SET call
    /// </summary>
    /// <param name="invocation"></param>
    /// <returns></returns>
    private bool IsSetMethods(IInvocation invocation)
    {
        //UGLY but works for now...
        string propertyName = invocation.Method.Name;
        return propertyName.StartsWith("set_");
    }

    /// <summary>
    /// Will check if the method call is a GET call
    /// </summary>
    /// <param name="invocation"></param>
    /// <returns></returns>
    private bool IsGetMethod(IInvocation invocation)
    {
        //UGLY but works for now...
        string propertyName = invocation.Method.Name;
        return propertyName.StartsWith("get_");
    }
    /// <summary>
    /// Intercepts GET calls to properties of this class
    /// <para>Will check the internal dictionary for Func<PropValue> to execute</para>
    /// </summary>
    /// <param name="invocation"></param>
    private void InterceptGet(IInvocation invocation)
    {
        string name = invocation.Method.Name.Replace("get_", string.Empty);
        if (!_valueDelegates.ContainsKey(name))
        {
            invocation.Proceed();
            return;
        }
        Func<object> valuegetter = null;
        _valueDelegates.TryGetValue(name, out valuegetter);
        if (valuegetter == null)
        {
            invocation.Proceed();
            return;
        }
        var val = valuegetter();
        invocation.ReturnValue = val;
        SetValue(invocation, name, val); //set the value of the property
        _valueDelegates.Remove(name);
    }

    /// <summary>
    /// Set the actual value of the property
    /// </summary>
    /// <param name="invocation">The invocation obj</param>
    /// <param name="propertyName">the name of the property of the target to set</param>
    /// <param name="val">the value of the target property</param>
    private static void SetValue(IInvocation invocation, string propertyName, object val)
    {
        var prop = invocation.TargetType.GetProperty(propertyName);
        prop.SetValue(invocation.InvocationTarget, val);
    }
}
