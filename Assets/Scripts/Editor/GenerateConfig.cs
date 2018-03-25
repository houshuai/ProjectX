using System;
using System.Collections.Generic;
using XLua;

public static class GenerateConfig
{
    [CSharpCallLua]
    public static List<Type> CSharpCallLua = new List<Type>() {
        typeof(Action),
    };
}
