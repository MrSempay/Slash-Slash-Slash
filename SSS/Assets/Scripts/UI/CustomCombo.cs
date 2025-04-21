using UnityEngine;
using UnityEngine.Events;

public class CustomCombo : MonoBehaviour
{

    [SerializeField] private TextEdit comboTextUI;

    void Awake()
    {

    }

    // Update is called once per frame
    void Start()
    {
        
    }

    public void ChangeComboTextUI(int combo)
    {
        comboTextUI.SetNotLocalizableText(combo.ToString());
    }
}
