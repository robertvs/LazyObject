public class TestObj : LazyObject<TestObj>
{
    public string Text { get; set; }
    public DateTime Date { get; set; }
}


[TestClass]
public class LazyObjectTest
{
    public static bool _getDateAndWait5SecondsHasBeenCalled = false;

    [TestMethod]
    public void Test_That_Delegate_Is_Not_Called()
    {
        var testobj = TestObj.Create();
        testobj.SetLazy(p => p.Date, GetDate_And_Wait_5_Seconds);
        Assert.IsFalse(_getDateAndWait5SecondsHasBeenCalled);

    }
    [TestMethod]
    public void Test_That_Delegate_Is_Called_With_Lazy()
    {
        var testobj = TestObj.Create();
        testobj.SetLazy(p => p.Date, GetDate_And_Wait_5_Seconds);
        
        var date = testobj.Date; //accessing the member will make a call to the delegate
        Assert.IsTrue(_getDateAndWait5SecondsHasBeenCalled);

    }

    [TestMethod]
    public void Test_That_Delegate_Is_Called_Without_Lazy()
    {
        var testobj = TestObj.Create();
        testobj.Date = GetDate_And_Wait_5_Seconds();
        Assert.IsTrue(_getDateAndWait5SecondsHasBeenCalled);

    }

    private DateTime GetDate_And_Wait_5_Seconds()
    {
        _getDateAndWait5SecondsHasBeenCalled = true;
        System.Threading.Thread.Sleep(5000);
        return new DateTime(1983, 07, 14);
    }
}
