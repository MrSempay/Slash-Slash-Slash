using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CustomCombo : MonoBehaviour
{
    private int _currentCombo;
    private float _timeZeroizeCombo;
    private string _baseText;
    private Action<int, int> _actionAfterUpdatingCombo;
    private Coroutine _zeroizeComboCoroutine;

    // в будущем вместо простого UnityAction<int> можно ввести какой-нибудь класс, который будет хранить ссылку на метод, количество комбо для индуцирующего объекта и, может, ещё чего-то.
    private Dictionary<object, UnityAction<int>> _dictionaryListenerMethods = new(); // словарь для всех методов, которые мы подписали на прослушивание событий для данного комбо.
                                                                             // Ключом является ссылка на объект, который индуцировал данное комбо. Нужно для корректного отвязывания
                                                                             // слушателя комбо при изменнии состояния (того или иного) объекта, который дал старт данному комбо

    [SerializeField] private TextEdit _comboTextUI;


    public int CurrentCombo
    {
        get { return _currentCombo; }
        set
        {
            if (_timeZeroizeCombo > 0)
            {
                if (_zeroizeComboCoroutine != null)
                {
                    StopCoroutine(_zeroizeComboCoroutine);
                }
                _zeroizeComboCoroutine = StartCoroutine(ZeroizeCombo());
            }

            _actionAfterUpdatingCombo?.Invoke(CurrentCombo, value);

            _currentCombo = value;
            ChangeComboTextUI(value);
        }
    }

    public Dictionary<object, UnityAction<int>> DictionaryListenerMethods { get { return _dictionaryListenerMethods; } }

    public void Initialize(string baseText, Action<int, int> actionAfterUpdatingCombo, float timeZeroizeCombo)
    {
        _baseText = baseText;
        _actionAfterUpdatingCombo = actionAfterUpdatingCombo;
        _timeZeroizeCombo = timeZeroizeCombo;
        _comboTextUI.SetBaseText(baseText);
        name = baseText;
        if (timeZeroizeCombo > 0)
        {
            _zeroizeComboCoroutine = StartCoroutine(ZeroizeCombo());
        }

    }

    void Awake()
    {
        CurrentCombo = CurrentCombo;
    }

    // Update is called once per frame
    void Start()
    {

    }

    public void UpCombo(int combo)
    {
        CurrentCombo += combo;
    }

    public void ChangeComboTextUI(int combo)
    {
        _comboTextUI.SetNotLocalizableText(combo.ToString());
    }


    public void AddMethodListenerToDictionary(object inductionObject, UnityAction<int> actionUpCombo)
    {
        _dictionaryListenerMethods[inductionObject] = actionUpCombo;
    }
    public void RemoveMethodListenerToDictionary(object inductionObject, UnityAction<int> actionUpCombo)
    {
        _dictionaryListenerMethods.Remove(inductionObject);
    }

    IEnumerator ZeroizeCombo()
    {
        yield return new WaitForSeconds(_timeZeroizeCombo);

        CurrentCombo = 0;

        _zeroizeComboCoroutine = null;
    }

    //-------------------------------- Функции, определяющие, что делается при изменении комбо --------------------------------//

    public void IncreaseDamageHeroeByTick(int lastCombo, int currentCombo)
    {
        Player.instance.damage -= lastCombo; // подразумевается, что единица комбо равна дополнительной единице урона героя. В будущем можно будет ввести коэффициент 
        Player.instance.damage += currentCombo;
    }


}
