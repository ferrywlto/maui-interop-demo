#if IOS
using System.Runtime.InteropServices;

public static class HelloWorldBridge
{
    [DllImport("__Internal", EntryPoint = "HelloWorldBridge_SayHello")]
    private static extern void _SayHelloStaticLib();

    [DllImport("__Internal", EntryPoint = "HelloWorldFramework_SayHello")]
    private static extern void _SayHelloFramework();

    public static void SayHelloStaticLib() => _SayHelloStaticLib();
    public static void SayHelloFramework() => _SayHelloFramework();
}
#endif