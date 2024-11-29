using System;
using UnityEngine;
using UnityEngine.UI;
using ZergRush.ReactiveCore;
using ZergRush.ReactiveUI;

public class DebugTabButton : ReusableView
{
    public Button button;
    public Image bg;
    public Text text;

    public Color normalColor;
    public Color selectedColor;

    public Color normalTextColor;
    public Color selectedTextColor;

    public void Show(string name, ICell<bool> selected = null, Action<IDisposable> connectionSink = null)
    {
        text.text = name;
        if (selected == null)
            selected = StaticCell<bool>.Default();
        if (connectionSink == null)
            connectionSink = _ => { };
        
        connectionSink(selected.Bind(sel => text.color = sel ? selectedTextColor : normalTextColor));
        connectionSink(selected.Bind(sel => bg.color = sel ? selectedColor : normalColor));
    }
}