//using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ILRuntime.Runtime.Enviorment;
using System.IO;
public delegate void SomeDelegate(int a, float b);
public delegate bool SomeFunction(int a, float b);

public class HotFixMgr : MonoBehaviour
{
    System.Func<int, float, bool> act1;
    System.Action<int, float> act;
    public AppDomain appdomain;
    System.IO.MemoryStream fs;
    System.IO.MemoryStream p;
    static HotFixMgr _Instance;
    public static HotFixMgr Instance
    {
        get
        {
            if (_Instance == null)
            {
                _Instance = new GameObject("HotFixMgr").AddComponent<HotFixMgr>();
                _Instance.LoadHotFixAssembly();
            }
            return _Instance;
        }
    }

    void LoadHotFixAssembly()
    {
        //首先实例化ILRuntime的AppDomain，AppDomain是一个应用程序域，每个AppDomain都是一个独立的沙盒
        appdomain = new ILRuntime.Runtime.Enviorment.AppDomain();
        //正常项目中应该是自行从其他地方下载dll，或者打包在AssetBundle中读取，平时开发以及为了演示方便直接从StreammingAssets中读取，
        //正式发布的时候需要大家自行从其他地方读取dll

        //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        //这个DLL文件是直接编译HotFix_Project.sln生成的，已经在项目中设置好输出目录为StreamingAssets，在VS里直接编译即可生成到对应目录，无需手动拷贝
        //工程目录在Assets\Samples\ILRuntime\1.6\Demo\HotFix_Project~
        //以下加载写法只为演示，并没有处理在编辑器切换到Android平台的读取，需要自行修改
#if UNITY_ANDROID
        WWW www = new WWW(Application.streamingAssetsPath + "/HotFix_Project.dll");
#else
        WWW www = new WWW("file:///" + Application.streamingAssetsPath + "/HotFix_Project.dll");
#endif
        while (!www.isDone)
            System.Threading.Thread.Sleep(100);
        if (!string.IsNullOrEmpty(www.error))
            UnityEngine.Debug.LogError(www.error);
        byte[] dll = www.bytes;
        www.Dispose();

        //PDB文件是调试数据库，如需要在日志中显示报错的行号，则必须提供PDB文件，不过由于会额外耗用内存，正式发布时请将PDB去掉，下面LoadAssembly的时候pdb传null即可
#if UNITY_ANDROID
        www = new WWW(Application.streamingAssetsPath + "/HotFix_Project.pdb");
#else
        www = new WWW("file:///" + Application.streamingAssetsPath + "/HotFix_Project.pdb");
#endif
        while (!www.isDone)
            System.Threading.Thread.Sleep(100);
        if (!string.IsNullOrEmpty(www.error))
            UnityEngine.Debug.LogError(www.error);
        byte[] pdb = www.bytes;
        fs = new MemoryStream(dll);
        p = new MemoryStream(pdb);
        try
        {
            appdomain.LoadAssembly(fs, p, new ILRuntime.Mono.Cecil.Pdb.PdbReaderProvider());
        }
        catch
        {
            Debug.LogError("加载热更DLL失败，请确保已经通过VS打开Assets/Samples/ILRuntime/1.6/Demo/HotFix_Project/HotFix_Project.sln编译过热更DLL");
        }

        InitializeILRuntime();
    }

    private void InitializeILRuntime()
    {
#if DEBUG && (UNITY_EDITOR || UNITY_ANDROID || UNITY_IPHONE)
        //由于Unity的Profiler接口只允许在主线程使用，为了避免出异常，需要告诉ILRuntime主线程的线程ID才能正确将函数运行耗时报告给Profiler
        appdomain.UnityMainThreadID = System.Threading.Thread.CurrentThread.ManagedThreadId;
#endif
        //这里做一些ILRuntime的注册，HelloWorld示例暂时没有需要注册的
        appdomain.DebugService.StartDebugService(56000); //启动调试服务器
        appdomain.DelegateManager.RegisterFunctionDelegate<int, float, bool>();
        appdomain.DelegateManager.RegisterDelegateConvertor<SomeDelegate>((action) =>
        {
            return new SomeDelegate((a, b) =>
            {
                ((System.Action<int, float>)action)(a, b);
            });
        });
        appdomain.DelegateManager.RegisterDelegateConvertor<SomeFunction>((action) =>
        {
            return new SomeFunction((a, b) =>
            {
                return ((System.Func<int, float, bool>)action)(a, b);
            });
        });
    }
}
