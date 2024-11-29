using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "debugUIElements", menuName = "Menu/Debug Elements Factory")]
public class DebugUIElementFactory : ScriptableObject
{
    [SerializeField] 
    public DebugBoxedContent boxer;
    
    [SerializeField] 
    public RectTransform triCountner;
    
    [SerializeField] 
    public RectTransform counter;
    
    [SerializeField] 
    public RectTransform optionPrefab;
    
    [SerializeField] 
    public RectTransform dropdownPrefab;
    
    [SerializeField] 
    public DebugTabButton buttonPrefab;
    
    [SerializeField] 
    public RectTransform stringInputPrefab;
    
    [SerializeField] 
    public RectTransform togglePrefab;
    
    [SerializeField] 
    public RectTransform buttonTogglePrefab;
    
    [SerializeField] 
    public RectTransform slider;
    
    [SerializeField] 
    public RectTransform delimiterPrefab;
    
    [SerializeField] 
    public RectTransform titlePrefab;

    [SerializeField]
    public RectTransform labelPrefab;

    [SerializeField] 
    public DebugWindow debugWindow;
    
    [SerializeField] 
    public DebugTabButton tab;
    public DebugTabButton Tab() => Instantiate(tab);

    public ReactiveScrollRect scrollPrefab;
    
    public void CollectBattleReward(string lobbyId)
    {
        // var results = GetBattleResults(lobbyId)
    }
}