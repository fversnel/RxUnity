using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using RxUnity.Schedulers;
using RxUnity.Util;
using UnityEngine;

namespace RxUnity.Utils
{
    class UnityApplicationLifecycle : MonoBehaviour
    {
        private static readonly UnitySingleton<UnityApplicationLifecycle> UnityApplicationLifecycleSingleton =
            new UnitySingleton<UnityApplicationLifecycle>("UnityApplicationLifecycle");

        public static UnityApplicationLifecycle Instance
        {
            get
            {
                return UnityApplicationLifecycleSingleton.Instance;
            }
        }

        private readonly ISubject<Unit> _onQuit = new Subject<Unit>();

        private UnityApplicationLifecycle() {}

        void OnApplicationQuit() {
            _onQuit.OnNext(Unit.Default);
        }

        void OnDestroy() {
            _onQuit.OnCompleted();
        }

        public IObservable<Unit> OnQuit {
            get { return _onQuit.Take(1); }
        }
    }
}
