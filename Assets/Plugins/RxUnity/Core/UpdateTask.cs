using System;

namespace RxUnity.Core {
    /// <summary>
    ///     An action that can be used to run inside a MonoBehaviour on every update tick.
    ///     The onDestroy action is called whenever the MonoBehaviour itself is destroyed.
    /// </summary>
    public struct UpdateTask {
        private readonly Action _onDestroyFn;
        private readonly Action _updateFn;

        public UpdateTask(Action updateFn, Action onDestroyFn) {
            _updateFn = updateFn;
            _onDestroyFn = onDestroyFn;
        }

        public Action UpdateFn {
            get { return _updateFn; }
        }

        public Action OnDestroyFn {
            get { return _onDestroyFn; }
        }
    }
}