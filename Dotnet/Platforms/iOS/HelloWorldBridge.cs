#if IOS
using System.Runtime.InteropServices;

public static class HelloWorldBridge
{
    [DllImport("__Internal", EntryPoint = "HelloWorldBridge_SayHello")]
    private static extern void _SayHello();

    public static void SayHello() => _SayHello();
}
#endif