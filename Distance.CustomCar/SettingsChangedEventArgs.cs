using System;

namespace Distance.CustomCar
{
    public class SettingsChangedEventArgs : EventArgs
    {
        public string Key { get; }

        public object OldValue { get; }
        public object NewValue { get; }

        public SettingsChangedEventArgs(string key, object oldValue = null, object newValue = null)
        {
            Key = key;

            OldValue = oldValue;
            NewValue = newValue;
        }
    }
}
