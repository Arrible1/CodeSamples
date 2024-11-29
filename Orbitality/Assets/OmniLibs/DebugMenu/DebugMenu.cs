using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ZergRush;
using ZergRush.ReactiveCore;

public class ExcudeFromDebugMenu : Attribute
{
}
public class ExcudeEnumNameInSelector : Attribute
{
    public string[] name;
    public ExcudeEnumNameInSelector(params object[] name)
    {
        this.name = name.Select(n => n.ToString()).ToArray();
    }
}

public struct DebugLayoutOptions
{
    public float flexibleSpace;
    public float forceSize;
    
    public float subflexibleSpace;
    public float forceSubSize;
    
    public bool fitSize;
    public static DebugLayoutOptions FitSize() => new DebugLayoutOptions {fitSize = true};
    public static DebugLayoutOptions Part(float part) => new DebugLayoutOptions {flexibleSpace = part};
    public static DebugLayoutOptions Part(float part, float subPart) => new DebugLayoutOptions {flexibleSpace = part, subflexibleSpace = subPart};
    public static DebugLayoutOptions Fixed(float size) => new DebugLayoutOptions {forceSize = size};
    public static DebugLayoutOptions Fixed(float size, float subSize) => new DebugLayoutOptions {forceSize = size, forceSubSize = subSize};
}

public class DebugMenu : ConnectableObject
{
    [SerializeField] RectTransform tabBar;
    [SerializeField] RectTransform content;

    [SerializeField] DebugUIElementFactory factory;
    [SerializeField] RectTransform windowAnchor;

    bool viewFilled => tabContents.Count > 0;

    Dictionary<string, RectTransform> tabContents = new Dictionary<string, RectTransform>();


    protected virtual void Init()
    {
    }

    public void ReFillView()
    {
        DisconnectAll();
        tabContents.Clear();
        tabBar.transform.DestroyChildren(t => t.name.Contains("Tab"));
        content.transform.DestroyChildren(t => t.name.Contains("Tab"));
        Init();
        //Tab("Logs And Profiler", () => SRDebug.Instance.ShowDebugPanel());
        connections += selectedDebugMenuTab.Bind(SetActiveTab);
    }

    public Cell<string> selectedDebugMenuTab = new Cell<string>();

    void SetActiveTab(string tab)
    {
        selectedDebugMenuTab.value = tab;
        foreach (var tabContent in tabContents)
        {
            tabContent.Value.SetActiveSafe(tabContent.Key == tab);
        }
    }

    // If special function is set this tab will work as just button
    public DebugLayout Tab(string name, Action specialFunction = null)
    {
        var tab = factory.Tab();
        tab.name = "Tab" + name;
        tab.transform.SetParent(tabBar);
        tab.Show(name, selectedDebugMenuTab.Map(v => v == name), connectionSink);
        tab.button.ClickStream().Subscribe(() =>
        {
            if (specialFunction != null) specialFunction();
            else selectedDebugMenuTab.value = name;
        });
        tab.transform.localScale = Vector3.one;

        var element = DebugLayout.NewLayout(factory, LayoutType.Horizontal);
        element.name = $"Tab{name}Content";
        element.rect.anchorMin = Vector2.zero;
        element.rect.anchorMax = Vector2.one;
        element.rect.offsetMin = Vector2.zero;
        element.rect.offsetMax = Vector2.zero;
        element.transform.SetParent(content, false);
        element.transform.localScale = Vector3.one;
        tabContents[name] = element.GetComponent<RectTransform>();
        return element;
    }

    public Cell<bool> shownCell = new Cell<bool>();
    bool shown { get { return shownCell.value; } set { shownCell.value = value; } }

    public DebugLayout Window(string title)
    {
        var window = Instantiate(factory.debugWindow, windowAnchor);
        window.title.text = title;
        return window.layout;
    }
    
    public void Hide()
    {
        Time.timeScale = 1;
        shownCell.value = false;
        GetComponent<Canvas>().enabled = false;
    }

    public void Show()
    {
        Time.timeScale = 0;
        topRightTime = 0;
        shownCell.value = true;
        GetComponent<Canvas>().enabled = true;
        if (viewFilled == false) ReFillView();
    }

    protected virtual void Update()
    {
        UpdateShowing();
        if (Input.GetKeyDown(KeyCode.F2))
        {
            if (!shown)
            {
                Show();
            }
            else
            {
                Hide();
            }
        }
    }

    float topRightTime, bottomRightTime, topLeftTime, bottomLeftTime;
    [SerializeField] float pressDuration = 2;


    private void UpdateShowing()
    {
        if (shown)
            return;
        foreach (var touch in Input.touches)
            ProcessTouch(touch.position);
        if (Input.GetMouseButtonUp(0))
            ProcessTouch(new Vector2(Input.mousePosition.x, Input.mousePosition.y));
        float time = Mathf.Max(0, Time.time - pressDuration);
        if (topRightTime > time &&
            bottomRightTime > time &&
            topLeftTime > time &&
            bottomLeftTime > time)
            Show();
    }

    private void ProcessTouch(Vector2 position)
    {
        bool right = position.x > Screen.width * 0.5f;
        bool top = position.y > Screen.height * 0.5f;
        if (right)
        {
            if (top)
                topRightTime = Time.time;
            else
                bottomRightTime = Time.time;
        }
        else
        {
            if (top)
                topLeftTime = Time.time;
            else
                bottomLeftTime = Time.time;
        }
    }
}