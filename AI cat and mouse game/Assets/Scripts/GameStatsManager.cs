using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameStatsManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI cheeseText;
    [SerializeField] private TextMeshProUGUI catSeesMouseText;
    [SerializeField] private TextMeshProUGUI mouseWonText;

    [SerializeField] private VerminMasterAI MouseCheese;
    [SerializeField] private CatLineOfSight CatLineOfSight;

    private int cheeseCount;
    private bool doesCatSeeMouse;
    private bool hasMouseWon;

    void Update()
    {
        
        cheeseCount = MouseCheese.GetCheeseCount();
        doesCatSeeMouse = CatLineOfSight.IsCatLookingAtMouse();
        hasMouseWon = MouseCheese.HasMouseEatenALLCheese();

        // Update the text values
        cheeseText.text = $"No. of Cheese -> {cheeseCount}";
        catSeesMouseText.text = $"Does Cat See Mouse?: {doesCatSeeMouse}";
        mouseWonText.text = $"Has The Mouse Won?: {hasMouseWon}";
    }




}
