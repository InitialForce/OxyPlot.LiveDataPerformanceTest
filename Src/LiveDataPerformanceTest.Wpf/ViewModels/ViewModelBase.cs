using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows.Threading;
using ActiveSharp.PropertyMapping;

namespace LiveDataPerformanceTest.Wpf.ViewModels
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        private PropertyChangedEventHandler _propertyChangeDelegate;

        protected ViewModelBase()
        {
            Owner = Dispatcher.CurrentDispatcher;
        }

        protected Dispatcher Owner { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged
        {
            add { Add(value); }
            remove { Remove(value); }
        }

        private void Add(PropertyChangedEventHandler value)
        {
            _propertyChangeDelegate += value;
            ;
        }

        private void Remove(PropertyChangedEventHandler value)
        {
            _propertyChangeDelegate -= value;
        }

        public static string GetPropertyName<T>(Expression<Func<T>> propertyExpresion)
        {
            var property = (MemberExpression) propertyExpresion.Body;
            return property.Member.Name;
        }

        //[NotifyPropertyChangedInvocator] 
        protected void RaisePropertyChanged(string propertyName)
        {
            NotifyPropertyChanged(this, propertyName);
        }

        protected void RaisePropertyChanged<T>(Expression<Func<T>> propertyExpresion)
        {
            RaisePropertyChanged(GetPropertyName(propertyExpresion));
        }


        private void NotifyPropertyChanged(object sender, string propertyName)
        {
            if (_propertyChangeDelegate != null)
                _propertyChangeDelegate(sender, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        ///     Sets the field to the new value and raises appropriate notification event
        /// </summary>
        /// <remarks>Does nothing if the new value equals the old</remarks>
        /// <returns>True if value changed</returns>
        protected bool SetValue<T>(ref T field, T value, bool forceNotifyPropertyChangedEvent = false)
            // note: must have generic type param on contain object, so we can propagate its type to propertyMap
        {
            // only act if the new value is in fact different from the old
            if (forceNotifyPropertyChangedEvent || !EqualityComparer<T>.Default.Equals(field, value))
            {
                PropertyInfo property = PropertyMap.GetProperty(this, ref field);
                // find the property first (i.e. if this blows up, we ain't doin' anything)
                field = value; // set the new value before we raise the notification
                NotifyPropertyChanged(this, property.Name); // notify that the property has changed
                return true;
            }
            return false;
        }
    }
}