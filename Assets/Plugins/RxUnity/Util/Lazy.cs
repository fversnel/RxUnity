using System;

namespace RxUnity.Util {
    public class Lazy<T> {
        private readonly Func<T> _provider;
        private bool _isInitialized;
        private T _value;

        public Lazy(Func<T> provider) {
            _provider = provider;
            _isInitialized = false;
            _value = default(T);
        }

        public T Value {
            get {
                if (!_isInitialized) {
                    _value = _provider();
                    _isInitialized = true;
                }
                return _value;
            }
        }
    }
}