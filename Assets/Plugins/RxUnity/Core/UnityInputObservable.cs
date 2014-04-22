using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using RxUnity.Util;
using UnityEngine;

namespace RxUnity.Core {
    public static class UnityInputObservable {
        public static readonly IEnumerable<KeyCode> AllKeys = Enum.GetValues(typeof (KeyCode)) as KeyCode[];

        public static IObservable<Vector3> MouseMove(IUpdateBehaviour updateBehaviour) {
            return UnityObservable.CreateUpdate<Vector3>(updateBehaviour,
                observer => observer.OnNext(Input.mousePosition))
                .DistinctUntilChangedEquatable(Vector3Comparer.Instance);
        }

        public static IObservable<KeyEvent> KeyEvents(IUpdateBehaviour updateBehaviour,
            IEnumerable<KeyCode> pollableKeys) {
            return UnityObservable.CreateUpdate<KeyEvent>(updateBehaviour, observer => {
                foreach (KeyCode key in pollableKeys) {
                    if (Input.GetKeyDown(key)) {
                        observer.OnNext(new KeyEvent(KeyEvent.EventType.Down, key));
                    }
                    else if (Input.GetKeyUp(key)) {
                        observer.OnNext(new KeyEvent(KeyEvent.EventType.Up, key));
                    }
                }
            });
        }

        public static IObservable<KeyCode> KeyDown(IObservable<KeyEvent> keyEvents) {
            return from keyEvent in keyEvents
                where keyEvent.Type == KeyEvent.EventType.Down
                select keyEvent.KeyCode;
        }

        public static IObservable<KeyCode> KeyUp(IObservable<KeyEvent> keyEvents) {
            return from keyEvent in keyEvents
                where keyEvent.Type == KeyEvent.EventType.Up
                select keyEvent.KeyCode;
        }

        public static IObservable<HashSet<KeyCode>> KeysHeld(IObservable<KeyEvent> keyEvents) {
            return keyEvents.Scan(new HashSet<KeyCode>(), (set, @event) => {
                if (@event.Type == KeyEvent.EventType.Down) {
                    set.Add(@event.KeyCode);
                }
                else {
                    set.Remove(@event.KeyCode);
                }
                return set;
            });
        }
    }

    // Data
    public struct KeyEvent : IEquatable<KeyEvent> {
        public enum EventType {
            Up,
            Down
        }

        private readonly KeyCode _keyCode;
        private readonly EventType _type;

        public KeyEvent(EventType type, KeyCode keyCode) {
            _type = type;
            _keyCode = keyCode;
        }

        public EventType Type {
            get { return _type; }
        }

        public KeyCode KeyCode {
            get { return _keyCode; }
        }

        public bool Equals(KeyEvent other) {
            return _type == other._type && _keyCode == other._keyCode;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }
            return obj is KeyEvent && Equals((KeyEvent) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return ((int) _type*397) ^ (int) _keyCode;
            }
        }

        public override string ToString() {
            return string.Format("KeyCode: {0}, Type: {1}", _keyCode, _type);
        }
    }
}