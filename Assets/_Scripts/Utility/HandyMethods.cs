using System.Collections;

public static class HandyMethods
{
    public static bool HasMethod(this object objectToCheck, string methodName)
    {
        var type = objectToCheck.GetType();
        return type.GetMethod(methodName) != null;
    }
}
