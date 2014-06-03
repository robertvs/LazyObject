/// <summary>
/// Will use dynamic proxy to intercept property calls and load property getters set by SetLazy
/// <para>User static Create() method to get lazy proxy instance</para>
/// </summary>
/// <typeparam name="TClass">The class to intercept with DynamicProxy and implement lazy loading on</typeparam>
public abstract class LazyObject<TClass> : IIntercept where TClass : class
{
    private Dictionary<string, Func<object>> _valueDelegates = new Dictionary<string, Func<object>>();
    private Dictionary<string, object> _cachedValues = new Dictionary<string, object>();
    private static ProxyGenerator _proxyGenerator = new ProxyGenerator();

    /// <summary>
    /// Will create a new Proxy instance of type TClass
    /// </summary>
    /// <returns></returns>
    public static TClass Create()
    {
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
        RemoveFromCache(propName);
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
        else if (IsSetMethods(invocation))
        {
            InterceptSet(invocation);
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
        if (_cachedValues.ContainsKey(name))
        {
            invocation.ReturnValue = _cachedValues[name];
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
        _cachedValues[name] = val;
        invocation.ReturnValue = val;
    }
    /// <summary>
    /// Intercepts SET calls to properties of this class
    /// <para>Wraps the values in a delegate so that it isn't executed at once</para>
    /// </summary>
    /// <param name="invocation"></param>
    private void InterceptSet(IInvocation invocation)
    {
        string name = invocation.Method.Name.Replace("set_", string.Empty);
        var value = invocation.GetArgumentValue(0);
        _valueDelegates[name] = () => { return value; };
        RemoveFromCache(name);
    }
    /// <summary>
    /// Will remove a cahed object from the cache dictionary
    /// </summary>
    /// <param name="name"></param>
    private void RemoveFromCache(string name)
    {
        if (_cachedValues.ContainsKey(name))
        {
            _cachedValues.Remove(name);
        }
    }
}
