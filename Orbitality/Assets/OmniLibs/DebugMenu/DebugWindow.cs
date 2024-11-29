using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugWindow : MonoBehaviour
{
    public Button closeButton;
    public DebugLayout layout;
    public Text title;

    void Awake()
    {
        closeButton.ClickStream().Subscribe(() => Destroy(gameObject));
    }
}
