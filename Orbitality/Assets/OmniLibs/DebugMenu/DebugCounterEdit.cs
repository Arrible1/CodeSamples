using System;
using UnityEngine;
using UnityEngine.UI;
using ZergRush.ReactiveCore;

public class DebugCounterEdit : MonoBehaviour
{
    public Text text;
    public Button more;
    public Button less;

    void Awake()
    {
        more.ClickStream().Subscribe(More);
        less.ClickStream().Subscribe(Less);
    }

    IValueRW<int> value;
    public IDisposable Show(ICellRW<int> value)
    {
        Show(value);
        return value.Bind(v => text.text = v.ToString());
    }

    public void Show(IValueRW<int> value)
    {
        this.value = value;
        text.text = value.value.ToString();
    }

    public void More()
    {
        value.value += 1;
        text.text = value.value.ToString();
    }

    public void Less()
    {
        value.value -= 1;
        text.text = value.value.ToString();
    }
}