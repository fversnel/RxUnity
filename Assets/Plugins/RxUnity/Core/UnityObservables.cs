using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using RxUnity.Util;
using UnityEngine;

namespace RxUnity.Core {

    public static class UnityObservables {

        public static IObservable<float> UpdateTicks(Func<float> deltaTime) {
            return UnityObservable.EveryUpdate<float>(observer => observer.OnNext(deltaTime()));
        }

        public static IObservable<Unit> UpdateTicks() {
            return UnityObservable.EveryUpdate<Unit>(observer => observer.OnNext(Unit.Default));
        }

        public static IObservable<float> TimeElapsed(IObservable<float> deltas) {
            return deltas.Scan(0f, (timePassed, deltaTime) => timePassed + deltaTime);
        }

        public static IObservable<SubjectSelection> RaycastSelection(IObservable<RaycastHit> raycasts,
            IEnumerable<GameObject> gameObjects) {
            return gameObjects.Select(g => RaycastSelection(raycasts, g)).Merge();
        }

        public static IObservable<SubjectSelection> RaycastSelection(IObservable<RaycastHit> raycasts,
            GameObject subject) {
            IObservable<SubjectSelection.Event> selectionEvents = raycasts
                .Select(raycastHit => {
                    bool isSubjectHit = raycastHit.collider != null && raycastHit.collider.gameObject.Equals(subject);
                    return isSubjectHit ? SubjectSelection.Event.Selected : SubjectSelection.Event.Deselected;
                });

            // Capture deselection once
            IObservable<SubjectSelection.Event> deselection = selectionEvents
                .DistinctUntilChangedEquatable(EqualityOperatorComparer<SubjectSelection.Event>.Instance)
                .Where(@event => @event == SubjectSelection.Event.Deselected);
            // Capture selection every time there is a new raycast that points to the subject
            IObservable<SubjectSelection.Event> selection =
                selectionEvents.Where(@event => @event == SubjectSelection.Event.Selected);
            return selection.Merge(deselection)
                .Select(@event => new SubjectSelection(@event, subject));
        }

        public static IObservable<RaycastHit> Raycasts(IObservable<Ray> rays, int layerMask,
            float distance = Mathf.Infinity) {
            return rays.Select(ray => {
                RaycastHit hit;
                Physics.Raycast(ray, out hit, distance, layerMask);
                return hit;
            })
                // Make sure we don't do any unnecessary raycasts
                .Publish().RefCount();
        }

        /// <summary>
        ///     Can be useful for determining mouse movement velocity or any other movement velocity for that matter.
        /// </summary>
        /// <param name="movement"></param>
        /// <returns>the delta between the current and previous vector</returns>
        public static IObservable<Vector3> MovementDelta(IObservable<Vector3> movement) {
            return movement.Scan((prevPosition, newPosition) => newPosition - prevPosition);
        }

        /// <summary>
        ///     Can be used as a replacement for Observable.DistinctUntilChanged because it does not do
        ///     explicit boxing and unboxing of primitive types. The DistinctUntilChanged method uses .Equals for equality checking
        ///     which can result in unnessary allocation due to boxing/unboxing of primitive values.
        ///     Values from the observable in this method are compared with
        ///     the supplied IEqualityComparer allow you to avoid any implicit boxing of primitive types.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="comparer"></param>
        /// <returns></returns>
        public static IObservable<T> DistinctUntilChangedEquatable<T>(this IObservable<T> source,
            IEqualityComparer<T> comparer) {
            return Observable.Create<T>(observer => {
                bool isFirst = true;
                T lastValue = default(T);
                return source.Subscribe(value => {
                    if (!comparer.Equals(value, lastValue) || isFirst) {
                        isFirst = false;
                        lastValue = value;
                        observer.OnNext(value);
                    }
                }, observer.OnCompleted);
            });
        }

        public struct SubjectSelection {
            private readonly Event _eventType;
            private readonly GameObject _subject;

            public SubjectSelection(Event eventType, GameObject subject) {
                _eventType = eventType;
                _subject = subject;
            }

            public Event EventType {
                get { return _eventType; }
            }

            public GameObject Subject {
                get { return _subject; }
            }

            public sealed class Event {
                public static Event Selected = new Event();
                public static Event Deselected = new Event();
                private Event() {}
            }
        }
    }
}