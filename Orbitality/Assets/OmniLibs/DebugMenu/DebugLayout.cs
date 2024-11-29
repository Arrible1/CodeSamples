using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using ZergRush;
using ZergRush.ReactiveCore;
using ZergRush.ReactiveUI;

public enum LayoutType
{
    Horizontal,
    Vertical,
    Flow
}

public enum NamingType
{
    Lined,
    Boxed
}

public struct FieldPresentSettings
{
    public string name;
    public NamingType type;
    public static FieldPresentSettings WithName(string name, NamingType type) => new FieldPresentSettings { name = name, type = type };

    public static implicit operator FieldPresentSettings(string name)
    {
        return WithName(name, NamingType.Lined);
    }
}

static class FieldPresentSettingsTools
{
    public static FieldPresentSettings WithNaming(this FieldInfo field, NamingType type)
    {
        return FieldPresentSettings.WithName(field.Name, type);
    }

}

public class DebugLayout : ConnectableObject
{
    public LayoutType type;
    public DebugUIElementFactory factory;
    public RectTransform rect => GetComponent<RectTransform>();

    const int defaultSpacing = 14;

//    void Awake()
//    {
//        if (GetComponent<LayoutGroup>() == null)
//        {
//            switch (type)
//            {
//                case LayoutType.Horizontal:
//                    gameObject.AddComponent<HorizontalLayoutGroup>();
//                    break;
//                case LayoutType.Vertical:
//                    gameObject.AddComponent<VerticalLayoutGroup>();
//                    break;
//                case LayoutType.Flow:
//                    gameObject.AddComponent<FlowLayoutGroup>();
//                    break;
//            }
//        }
//    }

    public static DebugLayout NewLayout(DebugUIElementFactory factory, LayoutType type, GameObject go = null)
    {
        if (go == null)
            go = new GameObject();
        go.layer = 5;
        var l = go.AddComponent<DebugLayout>();
        l.factory = factory;
        l.type = type;
        if (type == LayoutType.Horizontal)
        {
            var group = go.AddComponent<HorizontalLayoutGroup>();
            group.spacing = defaultSpacing;
            group.padding = new RectOffset(defaultSpacing, defaultSpacing, defaultSpacing, defaultSpacing);
            group.childControlWidth = true;
            group.childControlHeight = true;
            group.childForceExpandWidth = false;
            group.childForceExpandHeight = true;
        }
        else if (type == LayoutType.Vertical)
        {
            var group = go.AddComponent<VerticalLayoutGroup>();
            group.spacing = defaultSpacing;
            group.padding = new RectOffset(defaultSpacing, defaultSpacing, defaultSpacing, defaultSpacing);
            group.childControlWidth = true;
            group.childControlHeight = true;
            group.childForceExpandWidth = true;
            group.childForceExpandHeight = false;
        }
        else
        {
            // var group = go.AddComponent<FlowLayoutGroup>();
            // group.padding = new RectOffset(defaultSpacing, defaultSpacing, defaultSpacing, defaultSpacing);
            // group.Spacing = defaultSpacing;
            // group.ChildForceExpandHeight = true;
        }

        return l;
    }

    public void ColumnSelectorEnum<T>(ICellRW<T> curr, Action<T, DebugLayout> fillFunc,
        DebugLayoutOptions opts = default(DebugLayoutOptions))
    {
        Func<string, T> parse = s => (T)Enum.Parse(typeof(T), s);
        ColumnSelector(Enum.GetNames(typeof(T)), curr.MapToString(parse), (s, l) => fillFunc(parse(s), l), opts);
    }
    public void ColumnSelector(string[] names, ICellRW<string> curr, Action<string, DebugLayout> fillFunc,
        DebugLayoutOptions opts = default(DebugLayoutOptions))
    {
        Selector("", names, curr);
        foreach (var name in names)
        {
            var c = Column(opts);
            if (opts.fitSize)
                c.GetComponent<VerticalLayoutGroup>().childControlHeight = false;
            c.HideIf(curr.IsNot(name));
            fillFunc(name, c);
        }
    }

    public DebugLayout Column(DebugLayoutOptions opts)
    {
        var l = NewLayout(factory, LayoutType.Vertical);
        l.rect.SetParent(rect, false);
        AdjustIntoLayout(l, opts);
        return l;
    }
    public DebugLayout Column(float screenPart)
    {
        return Column(DebugLayoutOptions.Part(screenPart));
    }

    public DebugLayout Flow(DebugLayoutOptions opts)
    {
        var l = NewLayout(factory, LayoutType.Flow);
        l.rect.SetParent(rect, false);
        AdjustIntoLayout(l, opts);
        return l;
    }
    public DebugLayout Flow(float screenPart)
    {
        return Flow(DebugLayoutOptions.Part(screenPart));
    }
    public DebugLayout Row(DebugLayoutOptions opts)
    {
        var l = NewLayout(factory, LayoutType.Horizontal);
        l.rect.SetParent(rect, false);
        AdjustIntoLayout(l, opts);
        return l;
    }
    public DebugLayout Row(float screenPart)
    {
        return Row(DebugLayoutOptions.Part(screenPart));
    }

    public DebugLayout ScrollLayout(float takeRelativeLayoutSize = 1)
    {
        var scroll = InstantiateInLayout(factory.scrollPrefab, new DebugLayoutOptions { flexibleSpace = takeRelativeLayoutSize });
        var content = scroll.scroll.content;
        var fitter = content.gameObject.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        var l = NewLayout(factory, LayoutType.Vertical, content.gameObject);
        l.RemovePaddings();
        return l;
    }

    public DebugLayout ScrollSingleSelection<T>(
    IReactiveCollection<T> collection, ICellRW<T> selected, Func<T, string> toString, float takeRelativeLayoutSize = 1)
    {
        var currentSelected = InstantiateInLayout(factory.titlePrefab).GetComponentInChildren<Text>();
        currentSelected.SetTextContent(selected.Select(v => $"current: {toString(v)}"));
        var scroll = InstantiateInLayout(factory.scrollPrefab, new DebugLayoutOptions { flexibleSpace = takeRelativeLayoutSize });

        Rui.PresentInScrollWithLayout(collection, scroll, show:(s, button) =>
        {
            button.Show(toString(s), selected.Select(v => object.ReferenceEquals(v, s)), button.connectionSink);
            button.button.ClickStream().Subscribe(() => selected.value = s);
        }, prefab: PrefabRef.ToPrefabRef(factory.buttonPrefab), layout: Rui.LinearLayout(forceMainSize: 80));
        return this;
    }

    public DebugLayout ScrollSingleSelection(
        IReactiveCollection<string> collection, ICellRW<string> selected, float takeRelativeLayoutSize = 1)
    {
        var currentSelected = InstantiateInLayout(factory.titlePrefab).GetComponentInChildren<Text>();
        currentSelected.SetTextContent(selected.Select(v => $"current: {v}"));
        var scroll = InstantiateInLayout(factory.scrollPrefab, new DebugLayoutOptions { flexibleSpace = takeRelativeLayoutSize });

        Rui.PresentInScrollWithLayout(collection, scroll, PrefabRef.ToPrefabRef(factory.buttonPrefab), (s, button) =>
        {
            button.Show(s, selected.Select(v => v == s), button.connectionSink);
            button.addConnection = button.button.ClickStream().Subscribe(() => selected.value = s);
        }, layout: Rui.LinearLayout(forceMainSize: 80));

        return this;
    }

    public DebugLayout Action(string name, Action onClick, Color color = default(Color), ICell<bool> interactable = null)
    {
        var b = InstantiateInLayout(factory.buttonPrefab);
        if (color != new Color(0, 0, 0, 0))
        {
            b.normalColor = color;
            b.selectedColor = color * 0.5f;
        }
        b.Show(name);
        b.transform.SetParent(rect);
        b.GetComponentInChildren<Text>().text = name;
        b.button.ClickStream().Subscribe(onClick);
        if (interactable != null) b.addConnection = interactable.Bind(v => b.button.interactable = v);
        return this;
    }

    public DebugLayout ActionWithSelectionGroup(
        string name, Cell<string> selectedAction, Action onClick)
    {
        var b = InstantiateInLayout(factory.buttonPrefab);
        b.Show(name, selectedAction.Is(name));
        b.transform.SetParent(rect);
        b.GetComponentInChildren<Text>().text = name;
        b.button.ClickStream().Subscribe(() =>
        {
            selectedAction.value = name;
            onClick();
        });
        return this;
    }

    public DebugLayout Action( // TODO: Add some graphics waiting representation.
        string name, Func<Task> onClick, Color color = default(Color))
    {
        return Action(name, () => WrapTask(onClick), color);
    }
    async void WrapTask(Func<Task> task) { await task(); }
    void AdjustIntoLayout<T>(T obj, DebugLayoutOptions options = default(DebugLayoutOptions)) where T : Component
    {
        var le = obj.gameObject.GetOrAddComponent<LayoutElement>();
        if (options.fitSize)
        {
            le.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }
        if (type == LayoutType.Vertical)
        {
            le.flexibleHeight = options.flexibleSpace;
            le.preferredHeight = options.forceSize == 0 ? obj.GetComponent<RectTransform>().sizeDelta.y : options.forceSize;
        }
        else if (type == LayoutType.Horizontal)
        {
            le.flexibleWidth = options.flexibleSpace;
            le.preferredWidth = options.forceSize == 0 ? obj.GetComponent<RectTransform>().sizeDelta.x : options.forceSize;
        }
        else
        {
            le.flexibleWidth = options.flexibleSpace;
            le.flexibleHeight = options.subflexibleSpace;
            le.preferredWidth = options.forceSize == 0 ? obj.GetComponent<RectTransform>().sizeDelta.x : options.forceSize;
            le.preferredHeight = options.forceSubSize == 0 ? obj.GetComponent<RectTransform>().sizeDelta.y : options.forceSubSize;
        }
    }

    public T InstantiateInLayout<T>(T rtPrefab, DebugLayoutOptions options = default(DebugLayoutOptions)) where T : Component
    {
        var obj = Instantiate(rtPrefab, rect, false);
        AdjustIntoLayout(obj, options);
        return obj;
    }

    public DebugLayout Delimiter(float size = 50)
    {
        InstantiateInLayout(factory.delimiterPrefab, DebugLayoutOptions.Fixed(size));
        return this;
    }

    public DebugLayout FlowDelimiterCut()
    {
        InstantiateInLayout(factory.delimiterPrefab, DebugLayoutOptions.Fixed(2000, 0.001f));
        return this;
    }

    public DebugLayout Toggle(FieldPresentSettings name, IValueRW<bool> value)
    {
        var obj = InstantiateInLayout(factory.buttonTogglePrefab).GetComponentInChildren<Toggle>();
        obj.GetComponentInChildren<Text>().text = name.name;
        obj.isOn = value.value;
        obj.onValueChanged.AddListener(new UnityAction<bool>(i => { value.value = i; }));
        return this;
    }

    public DebugLayout Selector(FieldPresentSettings name, IList<string> options, ICellRW<string> value
        , DebugLayoutOptions opts = default(DebugLayoutOptions))
    {
        Dropdown selector = NamedElementOrLayouted<Dropdown>(name, factory.dropdownPrefab, opts).GetComponentInChildren<Dropdown>();
        selector.options = options.Select(elemName => new Dropdown.OptionData(elemName)).ToList();
        value.Bind(v => selector.value = options.IndexOf(v));
        selector.onValueChanged.AddListener(new UnityAction<int>(i =>
        {
            value.value = options[i];
        }));
        return this;
    }

    public DebugLayout EnumSelector<T>(FieldPresentSettings name, ICellRW<T> value, DebugLayoutOptions opts = default(DebugLayoutOptions))
    {
        return Selector(name, Enum.GetNames(typeof(T)),
            value.MapRW(v => v.ToString(), s => (T)Enum.Parse(typeof(T), s)), opts);
    }

    public DebugLayout EnumSelector(FieldPresentSettings name, Type enumType, ICellRW<object> value,
        DebugLayoutOptions opts = default(DebugLayoutOptions), ExcudeEnumNameInSelector exclude = null)
    {
        return Selector(name, Enum.GetNames(enumType).Where(n => exclude == null || exclude.name.Contains(n) == false).ToList(),
            value.MapRW(v => v.ToString(), s => Enum.Parse(enumType, s)), opts);
    }

    public DebugLayout Title(string name, DebugLayoutOptions opts = default(DebugLayoutOptions))
    {
        var t = InstantiateInLayout(factory.titlePrefab, opts).GetComponentInChildren<Text>();
        t.text = name;
        return this;
    }

    public DebugLayout Label(string name, int fontSize = 40)
    {
        return Label(new StaticCell<string>(name));
    }
    public DebugLayout Label(ICell<string> name, int fontSize = 40)
    {
        var t = InstantiateInLayout(factory.titlePrefab, DebugLayoutOptions.Fixed(fontSize + 6)).GetComponentInChildren<Text>();
        t.resizeTextForBestFit = true;
        t.resizeTextMaxSize = (int)fontSize;
        connections += name.Bind(n => t.text = n);
        return this;
    }

    public DebugLayout Label(ICell<string> textCell, DebugLayoutOptions opts)
    {
        var t = InstantiateInLayout(factory.labelPrefab, opts).GetComponentInChildren<Text>();
        connections += textCell.Bind(text => t.text = text);
        return this;
    }


    public T NamedElementOrLayouted<T>(FieldPresentSettings name, RectTransform prefab, DebugLayoutOptions options = default(DebugLayoutOptions))
        where T : MonoBehaviour
    {
        RectTransform elem = null;
        if (string.IsNullOrEmpty(name.name))
            elem = InstantiateInLayout(prefab, options);
        else if (name.type == NamingType.Lined)
            elem = LinedNameElement(name.name, prefab, options);
        else if (name.type == NamingType.Boxed)
            elem = BoxedNameElement(name.name, prefab, options);
        return elem.GetComponentInChildren<T>();
    }

    RectTransform BoxedNameElement(string name, RectTransform prefab, DebugLayoutOptions options = default(DebugLayoutOptions))
    {
        var option = InstantiateInLayout(factory.boxer, options);
        option.title.text = name.CamelCaseToReadableText();
        var t = Instantiate(prefab, option.content, false);
        var controllRt = t.GetComponent<RectTransform>();
        controllRt.pivot = new Vector2(0.5f, 0.5f);
        controllRt.anchorMin = new Vector2(0, 0f);
        controllRt.anchorMax = new Vector2(1, 1f);
        controllRt.anchoredPosition = Vector2.zero;
        controllRt.sizeDelta = Vector2.zero;
        return t;
    }

    RectTransform LinedNameElement(string name, RectTransform prefab, DebugLayoutOptions options = default(DebugLayoutOptions))
    {
        var option = InstantiateInLayout(factory.optionPrefab, options);
        option.GetComponentInChildren<Text>().text = name.CamelCaseToReadableText() + ":";
        var textRt = option.GetComponentInChildren<Text>().GetComponent<RectTransform>();
        var textLE = textRt.transform.parent.gameObject.AddComponent<LayoutElement>();
        textLE.flexibleWidth = 1;
        var rt = option.GetComponent<RectTransform>();

        var t = Instantiate(prefab, option.transform, false);
        var controllRt = t.GetComponent<RectTransform>();
        controllRt.gameObject.GetOrAddComponent<LayoutElement>().flexibleWidth = 2;
        controllRt.pivot = new Vector2(1, 0.5f);
        controllRt.anchorMin = new Vector2(1, 0f);
        controllRt.anchorMax = new Vector2(1, 1f);
        controllRt.anchoredPosition = Vector2.zero;
        controllRt.sizeDelta = new Vector2(controllRt.sizeDelta.x, 0);

        return t;
    }

    public DebugLayout ShowIf(bool val)
    {
        gameObject.SetActive(!val);
        return this;
    }
    public DebugLayout HideIf(ICell<bool> val)
    {
        connections += val.Bind(disabled => { gameObject.SetActive(!disabled); });
        return this;
    }
    public DebugLayout ShowWhile(ICell<bool> val)
    {
        connections += this.SetActive(val);
        return this;
    }

    public DebugLayout LastElementShowWhile(ICell<bool> shown = null)
    {
        addConnection = rect.GetChild(rect.childCount - 1).SetActive(shown);
        return this;
    }

    public DebugLayout DisableIf(ICell<bool> val)
    {
        var group = gameObject.GetOrAddComponent<CanvasGroup>();
        connections += val.Bind(disabled =>
        {
            group.alpha = disabled ? 0.5f : 1f;
            group.interactable = !disabled;
        });
        return this;
    }

    public DebugLayout ValueSlider(FieldPresentSettings name, int min, int max, IValueRW<int> val,
        DebugLayoutOptions opts = default(DebugLayoutOptions))
    {
        return ValueSlider(name, min, max, val.MapValue(i => (float)i, f => (int)f), true, opts);
    }

    public DebugLayout RemovePaddings()
    {
        if (type == LayoutType.Vertical)
            GetComponent<VerticalLayoutGroup>().padding = new RectOffset(0, 0, 0, 0);
        else if (type == LayoutType.Horizontal)
            GetComponent<HorizontalLayoutGroup>().padding = new RectOffset(0, 0, 0, 0);
        return this;
    }
    
    public DebugLayout SetSpacing(float spacePixels)
    {
        if (type == LayoutType.Vertical)
            GetComponent<VerticalLayoutGroup>().spacing = spacePixels;
        else if (type == LayoutType.Horizontal)
            GetComponent<HorizontalLayoutGroup>().spacing = spacePixels;
        return this;
    }

    public DebugLayout SetChildWidthForceExpand(bool expand)
    {
        if (type == LayoutType.Vertical)
            GetComponent<VerticalLayoutGroup>().childForceExpandWidth = expand;
        else if (type == LayoutType.Horizontal)
            GetComponent<HorizontalLayoutGroup>().childForceExpandWidth = expand;
        return this;
    }
    
    public DebugLayout SetChildControlWidth(bool control)
    {
        if (type == LayoutType.Vertical)
            GetComponent<VerticalLayoutGroup>().childControlWidth = control;
        else if (type == LayoutType.Horizontal)
            GetComponent<HorizontalLayoutGroup>().childControlWidth = control;
        return this;
    }
    
    public DebugLayout SetChildControlHeight(bool control)
    {
        if (type == LayoutType.Vertical)
            GetComponent<VerticalLayoutGroup>().childControlHeight = control;
        else if (type == LayoutType.Horizontal)
            GetComponent<HorizontalLayoutGroup>().childControlHeight = control;
        return this;
    }
    
    public DebugLayout SetChildHeightForceExpand(bool expand)
    {
        if (type == LayoutType.Vertical)
            GetComponent<VerticalLayoutGroup>().childForceExpandHeight = expand;
        else if (type == LayoutType.Horizontal)
            GetComponent<HorizontalLayoutGroup>().childForceExpandHeight = expand;
        return this;
    }

    public DebugLayout ValueSlider(FieldPresentSettings name, float min, float max, IValueRW<float> val, bool wholeNumbers = false,
        DebugLayoutOptions opts = default(DebugLayoutOptions))
    {
        var slider = NamedElementOrLayouted<Slider>(name, factory.slider, opts);
        slider.minValue = min;
        slider.maxValue = max;
        slider.wholeNumbers = wholeNumbers;
        slider.value = val.value;

        var input = slider.GetComponentInChildren<InputField>();
        input.text = val.value.ToString();
        input.onValueChanged.AddListener(i =>
        {
            slider.value = float.Parse(i);
            val.value = slider.value;
        });
        slider.onValueChanged.AddListener(i =>
        {
            val.value = i;
            input.text = i.ToString();
        });

        return this;
    }

    public DebugLayout InputField(FieldPresentSettings name, IValueRW<string> val)
    {
        var input = NamedElementOrLayouted<InputField>(name, factory.stringInputPrefab);
        input.text = val.value;
        input.onValueChanged.AddListener(i => { val.value = i; });
        return this;
    }

    public DebugLayout NumberSelector(FieldPresentSettings name, IValueRW<int> value,
        DebugMenuIntRange range = null, DebugLayoutOptions opts = default(DebugLayoutOptions))
    {
        var attr = range;
        if (attr != null)
        {
            var input = ValueSlider(name, attr.min, attr.max, value, opts);
        }
        else
        {
            var input = NamedElementOrLayouted<InputField>(name, factory.stringInputPrefab, opts);
            input.contentType = UnityEngine.UI.InputField.ContentType.IntegerNumber;
            input.text = value.value.ToString();
            input.onValueChanged.AddListener(new UnityAction<string>(i => value.value = int.Parse(i)));
        }
        return this;
    }

    public DebugLayout NumberSelector(FieldPresentSettings name, IValueRW<float> value, DebugMenuFloatRange range = null, DebugLayoutOptions opts = default(DebugLayoutOptions))
    {
        var attr = range;
        if (attr != null)
        {
            var input = ValueSlider(name, attr.min, attr.max, value, false, opts);
        }
        else
        {
            var input = NamedElementOrLayouted<InputField>(name, factory.stringInputPrefab, opts);
            input.contentType = UnityEngine.UI.InputField.ContentType.DecimalNumber;
            input.text = value.value.ToString();
            input.onValueChanged.AddListener(new UnityAction<string>(i => value.value = float.Parse(i)));
        }
        return this;
    }

    public DebugLayout DrawAllFields(object obj, float selectorSize = 65, NamingType naming = NamingType.Lined, Func<object, FieldInfo, DebugLayout, bool> customFactory = null)
    {
        if (obj == null)
        {
            Label("null", 40);
            return this;
        }
        var opts = DebugLayoutOptions.Fixed(selectorSize);
        foreach (var fieldInfo in obj.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy))
        {
            if (customFactory != null && customFactory(obj, fieldInfo, this)) continue;
            if (fieldInfo.HasAttribute<ExcudeFromDebugMenu>()) continue;
            if (fieldInfo.FieldType.IsEnum)
            {
                EnumSelector(fieldInfo.WithNaming(naming), fieldInfo.FieldType, obj.ReflectionFieldToRW<object>(fieldInfo.Name),
                    exclude: fieldInfo.GetCustomAttribute<ExcudeEnumNameInSelector>(), opts: opts);
            }
            else if (fieldInfo.FieldType == typeof(string))
            {
                var input = NamedElementOrLayouted<InputField>(fieldInfo.WithNaming(naming), factory.stringInputPrefab, opts);
                input.text = (string)fieldInfo.GetValue(obj);
                input.onValueChanged.AddListener(new UnityAction<string>(i => { fieldInfo.SetValue(obj, i); }));
            }
            else if (
                fieldInfo.FieldType == typeof(long))
            {
                var attr = fieldInfo.GetCustomAttributes<DebugMenuIntRange>().FirstOrDefault();
                NumberSelector(fieldInfo.WithNaming(naming), obj.ReflectionFieldToRW<long>(fieldInfo).MapValue(i => (int)i, i => (long)i), attr, opts);
            }
            else if (fieldInfo.FieldType == typeof(int))
            {
                var attr = fieldInfo.GetCustomAttributes<DebugMenuIntRange>().FirstOrDefault();
                NumberSelector(fieldInfo.WithNaming(naming), obj.ReflectionFieldToRW<int>(fieldInfo), attr, opts);
            }
            else if (fieldInfo.FieldType == typeof(bool))
            {
                var input = NamedElementOrLayouted<Toggle>(fieldInfo.WithNaming(naming), factory.togglePrefab, opts);
                input.isOn = (bool)fieldInfo.GetValue(obj);
                input.onValueChanged.AddListener(new UnityAction<bool>(i => { fieldInfo.SetValue(obj, i); }));
            }
            else if (fieldInfo.FieldType.IsClass)
            {
                if (type == LayoutType.Flow) FlowDelimiterCut();
                Title(fieldInfo.Name, type == LayoutType.Flow ? DebugLayoutOptions.Fixed(1000) : opts);
                if (type == LayoutType.Flow) FlowDelimiterCut();
                DrawAllFields(fieldInfo.GetValue(obj), selectorSize, naming, customFactory);
                if (type == LayoutType.Flow) FlowDelimiterCut();
                else Delimiter();
            }
        }

        //        var delimiter = Instantiate(factory.optionPrefab, rect);
        //        delimiter.GetComponentInChildren<Text>().text = "";
        //        delimiter.sizeDelta = new Vector2(100, 20);

        return this;
    }
}