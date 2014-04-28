using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using RxUnity.Core;
using RxUnity.Schedulers;
using RxUnity.Util;
using UnityEngine;
using System.Collections;

public class AsyncTaskExample : MonoBehaviour
{
	void Start ()
	{
	    AsyncUtil.RunAsync(() =>
	    {
	        Debug.Log("Creating Async value...");
            Thread.Sleep(3000);
	        return 42;
	    })
        .ObserveOnUnityThread()
        .Subscribe(x =>
        {
            Debug.Log("Value " + x + " received in the Unity thread, creating GameObject");
            new GameObject("AsyncTaskExample " + x);
        });
	}
}
