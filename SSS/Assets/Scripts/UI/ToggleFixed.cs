using System;
using UnityEngine;
using UnityEngine.UI;

public class ToggleFixed : MonoBehaviour, IControlLifeCicleFunctions
{
    private bool _isToggled = false;
    private string _nameInvokingFunction;
    private Sprite _spriteToSetToggled;
    private Sprite _spriteToSetUntoggled;
    private Image _selfSprite;

    [SerializeField] private Color _colorToggledSprite;
    [SerializeField] private bool _shouldBeToggledAtStart = false;
    [SerializeField] private bool _justCheckMark = false;
    
    public bool awakeWasCalledAlready { get; set; }
    public bool IsToggled
    {   get { return _isToggled; }
        set
        {
            _isToggled = value;
            ControllVisualizationToggle(_isToggled);

            object[] parameters = new object[] { value, (RectTransform)transform };
            StaticClassForAdditionalFunctions.CallFunctionByName(_nameInvokingFunction, EventBus.Instance, parameters); // вызываем функцию по имени, эмулирующую сигнал в шине событий
        }
    }
    public void Awake()
    {
        if (!awakeWasCalledAlready) {

            awakeWasCalledAlready = true;

            _selfSprite = GetComponent<Image>();
            Debug.Log(gameObject.name);

            if (_justCheckMark)
            {
                _spriteToSetToggled = Resources.Load<Sprite>(C.DK.PathFolderImagesForToggles + "CheckMarckToggled");
                _spriteToSetUntoggled = Resources.Load<Sprite>(C.DK.PathFolderImagesForToggles + "CheckMarckUntoggled");
            }
            else
            {
                _spriteToSetToggled = Resources.Load<Sprite>(C.DK.PathFolderImagesForToggles + name + "Toggled");
                _spriteToSetUntoggled = Resources.Load<Sprite>(C.DK.PathFolderImagesForToggles + name + "Untoggled");
            }



            _nameInvokingFunction = "Trigger" + name;


            Debug.Log(_nameInvokingFunction);
            if (_shouldBeToggledAtStart) // пусть всегда в самом начале тумблер, если он должен быть нажат, вызывает привязанный к нему метод. Если кнопка не должна быть прожата
            {                            // в начале, то просто ничего не делаем
                IsToggled = true;
                return;
            }
            ControllVisualizationToggle(_shouldBeToggledAtStart); // если тумблер не должен быть вжат в начале, то просто проставляем правильную визуализацию для него. Если должен,
                                                                  // то выполнится IsToggled = true и визуализация проставится уже в свойстве
        }
    }

    public virtual void OnMouseClicked()
    {
        IsToggled = !IsToggled;
    }


    // авось для Android хватит и OnMouseDown
    private void HandleTouchBegan(Vector2 position)
    {
        // Логика, выполняемая при начале касания
        // Например, можно проверить, был ли объект нажат
    }

    private void ControllVisualizationToggle(bool isToggled)
    {
        if (!isToggled)
        {
            if (_spriteToSetUntoggled != null)
                _selfSprite.sprite = _spriteToSetUntoggled; // по идее этот спрайт всегда должен быть. Ставим его по умолчанию при создании объекта тумблера
            _selfSprite.color = new Color(255f, 255f, 255f);
        }
        else
        {
            if (_spriteToSetToggled != null) // этого спрайта может и не быть. Если нету, то просто меняем немного цвет кнопки
            {
                _selfSprite.sprite = _spriteToSetToggled;
            }
            else
            {
                _selfSprite.color = _colorToggledSprite; // в инспекторе можно выбрать, каков должен быть цвет вжатой кнопки. Можно и не менять, поставив белый
                if (_spriteToSetUntoggled != null)
                    _selfSprite.sprite = _spriteToSetUntoggled;
            }
        }
    }

}
