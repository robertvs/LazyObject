/// <summary>
/// Will use dynamic proxy to intercept property calls and load property getters set by SetLazy
/// <para>User static Create() method to get lazy proxy instance</para>
/// </summary>
/// <typeparam name="TClass">The class to intercept with DynamicProxy and implement lazy loading on</typeparam>
public abstract class LazyObject<TClass> : IIntercept where TClass : class,IIntercept
{
    private Dictionary<string, Func<object>> _valueDelegates = new Dictionary<string, Func<object>>();
    private static ProxyGenerator _proxyGenerator = new ProxyGenerator();
    /// <summary>
    /// Will create a new Proxy instance of type TClass
    /// </summary>
    /// <returns></returns>
    public static TClass Create()
    {
        return ProxyObjectFactory.Create<TClass>();
    }

    /// <summary>
    /// Will add the valueGetter to an internal dictionary to be executed whan a property is beeing called
    /// </summary>
    /// <typeparam name="TProp">The type of property</typeparam>
    /// <param name="propertyGetter">A function to specify the property to set its value on</param>
    /// <param name="valueGetter">A function to get the properties value</param>
    public void SetLazy<TProp>(Expression<Func<TClass, TProp>> propertyGetter, Func<TProp> valueGetter)
    {
        var membExpr = propertyGetter.Body as MemberExpression;
        var propName = membExpr.Member.Name;
        //if it's virtual we can intercept and set our delegate
        if (IsVirtual(propName))
        {
            _valueDelegates[propName] = () =>
            {
                return valueGetter();
            };
        } //otherwise set property as normal
        else
        {
            var prop = this.GetType().GetProperty(propName);
            if (prop == null)
                return;

            prop.SetValue(this, valueGetter());
        }
    }
    /// <summary>
    /// Returns true if the property is virtual
    /// </summary>
    /// <param name="propName"></param>
    /// <returns></returns>
    private bool IsVirtual(string propName)
    {
        var prop = this.GetType().GetProperty(propName);
        if (prop == null)
            return false;
        return prop.GetGetMethod().IsVirtual;
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
        if (IsSetMethods(invocation))
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
        string propertyName = invocation.Method.Name.Replace("get_", string.Empty);
        if (!_valueDelegates.ContainsKey(propertyName))
        {
            invocation.Proceed();
            return;
        }
        Func<object> valuegetter = null;
        _valueDelegates.TryGetValue(propertyName, out valuegetter);
        if (valuegetter == null)
        {
            invocation.Proceed();
            return;
        }
        var val = valuegetter();
        invocation.ReturnValue = val;
        SetValue(invocation, propertyName, val); //set the value of the property
    }

    /// <summary>
    /// Intercepts SET calls to properties of this class
    /// <para>Sets the property value</para>
    /// </summary>
    /// <param name="invocation"></param>
    private void InterceptSet(IInvocation invocation)
    {
        string propertyName = invocation.Method.Name.Replace("set_", string.Empty);
        _valueDelegates.Remove(propertyName);
        invocation.Proceed();
    }

    /// <summary>
    /// Set the actual value of the property
    /// </summary>
    /// <param name="invocation">The invocation obj</param>
    /// <param name="propertyName">the name of the property of the target to set</param>
    /// <param name="val">the value of the target property</param>
    private void SetValue(IInvocation invocation, string propertyName, object val)
    {
        if (!_valueDelegates.ContainsKey(propertyName))
        {
            return; //this is here to prevent stackoverflow..
        }
        _valueDelegates.Remove(propertyName);
        var prop = invocation.TargetType.GetProperty(propertyName);
        prop.SetValue(invocation.InvocationTarget, val);
    }
}
