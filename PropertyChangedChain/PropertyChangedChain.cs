using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;

namespace Ailes.PropertyChangedChain
{
    public class PropertyChangedChain<TSource>
        : IDisposable
        where TSource : INotifyPropertyChanged
    {
        private TSource _source;
        private List<IEventPattern> _patterns;

        public PropertyChangedChain(TSource source)
        {
            _patterns = new List<IEventPattern>();
            _source = source;
            _source.PropertyChanged += _source_PropertyChanged;
        }

        public IObservable<TValue> From<TValue>(Expression<Func<TSource, TValue>> propertyExpr)
        {
            var memExpr = propertyExpr as MemberExpression;
            var propertyName = memExpr.Member.Name;
            var getter = propertyExpr.Compile();
            var pattern = new EventPattern<TValue>(propertyName, getter);
            _patterns.Add(pattern);
            return pattern;
        }

        void _source_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            foreach (var pattern in _patterns.Where(_ => _.SourcePropertyName == e.PropertyName))
            {
                pattern.OnNext(_source);
            }
        }

        #region IDisposable メンバー

        public bool Disposed { get; private set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        //~PropertyChangedChain()
        //{
        //    Dispose(false);
        //}

        protected virtual void Dispose(bool disposing)
        {
            if (!Disposed)
            {
                if (disposing)
                {
                    _source.PropertyChanged -= _source_PropertyChanged;
                }
                Disposed = true;
            }
        }

        #endregion

        private interface IEventPattern
        {
            string SourcePropertyName { get; }
            bool HasObservers { get; }
            void OnNext(TSource source);
        }

        private class EventPattern<TValue> : IEventPattern, IObservable<TValue>
        {
            private Func<TSource, TValue> _getter;
            public AsyncSubject<TValue> _subject;

            public EventPattern(string propertyName, Func<TSource, TValue> getter)
            {
                SourcePropertyName = propertyName;
                _getter = getter;
                _subject = new AsyncSubject<TValue>();
            }

            #region IEventPattern<TSource> メンバー

            public string SourcePropertyName { get; private set; }

            public bool HasObservers { get { return _subject.HasObservers; } }

            public void OnNext(TSource source)
            {
                _subject.OnNext(_getter(source));
            }

            #endregion

            #region IObservable<TValue> メンバー

            public IDisposable Subscribe(IObserver<TValue> observer)
            {
                return _subject.Subscribe(observer);
            }

            #endregion
        }

    }

    public static class SetPropertyExtensions
    {
        public static IDisposable AssignTo<TTarget, TVavle>(this IObservable<TVavle> source, TTarget obj, Expression<Func<TTarget, TVavle>> propertyExpr)
        {
            var memExpr = propertyExpr as MemberExpression;
            var propertyName = memExpr.Member.Name;
            var propInfo = obj.GetType().GetProperty(propertyName);
            return source.Subscribe(_ =>
            {
                propInfo.SetValue(obj, _);
            });
        }
        public static IDisposable RaizePropertyChanged<TTarget, TVavle>(this IObservable<TVavle> source, TTarget obj, Expression<Func<TTarget, TVavle>> propertyExpr)
        {
            var memExpr = propertyExpr as MemberExpression;
            var propertyName = memExpr.Member.Name;
            var fieldInfo = obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic).FirstOrDefault(_ => _.FieldType == typeof(PropertyChangedEventHandler));
            return source.Subscribe(_ =>
            {
                var field = fieldInfo.GetValue(obj);
                var e = new PropertyChangedEventArgs(propertyName);
                var p = new object[] { obj, e };
                foreach (var d in ((MulticastDelegate)field).GetInvocationList())
                {
                    d.Method.Invoke(obj, p);
                }
            });
        }
        public static IDisposable AssignAndRaisePropertyChanged<TTarget, TValue>(this IObservable<TValue> source, TTarget target, Expression<Func<TTarget, TValue>> propertyExpr)
        {
            var memExpr = propertyExpr as MemberExpression;
            var propertyName = memExpr.Member.Name;
            var targetType = target.GetType();
            var propInfo = targetType.GetProperty(propertyName);
            var fieldInfo = targetType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic).FirstOrDefault(_ => _.FieldType == typeof(PropertyChangedEventHandler));
            return source.Subscribe(_ =>
            {
                propInfo.SetValue(target, _);
                var field = fieldInfo.GetValue(target);
                var e = new PropertyChangedEventArgs(propInfo.Name);
                var p = new object[] { target, e };
                foreach (var d in ((MulticastDelegate)field).GetInvocationList())
                {
                    d.Method.Invoke(target, p);
                }
            });
        }
    }
}
