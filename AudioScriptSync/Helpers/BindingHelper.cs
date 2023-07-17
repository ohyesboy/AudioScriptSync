namespace AudioScriptSync;

public class BindingHelper
{
    public static T GetAncestorBindingContext<T>(Element ele)
    {
        while (ele!=null)
        {
            if (ele.BindingContext is T ctx)
                return ctx;
            ele = ele.Parent;
        }
        return default(T);
    }
}
