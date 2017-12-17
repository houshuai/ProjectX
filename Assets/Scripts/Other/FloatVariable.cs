using System;
using UnityEngine;

[CreateAssetMenu]
public class FloatVariable : ScriptableObject
{
    [SerializeField]
    private float _value;
    public float Value
    {
        get { return _value; }
        set
        {
            _value = value;
            if (ValueChanged != null)
            {
                ValueChanged(this, new ValueChangedEventArgs(value));
            }
        }
    }

    public event EventHandler<ValueChangedEventArgs> ValueChanged;

}

public class ValueChangedEventArgs : EventArgs
{
    public float ValueChanged { get; private set; }

    public ValueChangedEventArgs(float valueChanged)
    {
        ValueChanged = valueChanged;
    }
}
