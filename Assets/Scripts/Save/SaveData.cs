using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class SaveData : ScriptableObject
{
    public class KeyValuePair<T>
    {
        private List<string> keys = new List<string>();
        private List<T> values = new List<T>();

        public void SetValue(string key, T value)
        {
            int index = keys.FindIndex(x => x == key);
            if (index > -1)
            {
                values[index] = value;
            }
            else
            {
                keys.Add(key);
                values.Add(value);
            }
        }

        public T GetValue(string key)
        {
            int index = keys.FindIndex(x => x == key);
            if (index > -1)
            {
                return values[index];
            }
            else
            {
                throw new KeyNotFoundException("not found the key");
            }
        }
    }

    private KeyValuePair<int> intPair = new KeyValuePair<int>();
    private KeyValuePair<float> floatPair = new KeyValuePair<float>();
    private KeyValuePair<string> stringPair = new KeyValuePair<string>();

    private void SetValue<T>(KeyValuePair<T> pair, string key, T value)
    {
        pair.SetValue(key, value);
    }

    private T GetValue<T>(KeyValuePair<T> pair, string key)
    {
        return pair.GetValue(key);
    }

    public void SetValue(string key, int value)
    {
        SetValue(intPair, key, value);
    }

    public void GetValue(string key, out int value)
    {
        value = GetValue(intPair, key);
    }

    public void SetValue(string key, float value)
    {
        SetValue(floatPair, key, value);
    }

    public void GetValue(string key, out float value)
    {
        value = GetValue(floatPair, key);
    }

    public void SetValue(string key, string value)
    {
        SetValue(stringPair, key, value);
    }

    public void GetValue(string key, out string value)
    {
        value = GetValue(stringPair, key);
    }
}
