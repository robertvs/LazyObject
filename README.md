LazyObject
==========

Using Castle.DynamicProxy to implement lazy loading for object members

###Example

```
public class TestObj : LazyObject<TestObj>
{
    public string Text { get; set; }
    public DateTime Date { get; set; }
}
```

The above class implements LazyObject

```
var testobj = TestObj.Create();
testobj.SetLazy(p => p.Date, ()=>
{
    //Do some long running work here
    return DateTime.Now;
});
```
Using the static _Create_ method we tell LazyObject<>to create a proxy instance of the class
Now we can use the **SetLazy** method to map delegates to the objects properties! Yey!

We can access the properties as usual and DynamicProxy will intecept the call and return and execute the delegate

```DateTime testDate = testObj.Date; //this is when the delegate will be called```
