using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;

    // при первом обращении к этому свойству (а более не к чему в начале) создастся экземпляр класса GlobalGameScript, запишется в _instance и вернёт ссылку на этот
    // экземпляр. При повторных обращениях будет возвращать ссылку на этот же экземпляр (у нас ибо static поле _instance, применится ко всему классу), static же для
    // свойства Instance нужен для того, чтоб можно было изначально создать экземпляр данного класса. Далее в Awake мы проверяем, существует ли уже экземпляр
    // данного класса и равен ли он объекту, из которого вызывается Awake, если да, то сохраням ссылку на него в _instance (вообще, эта логика в Awake нужно для того,
    // чтобы не было проблем при ручном создании (ну мало ли) данного синглтона. Второй раз создать его не даст в любом случае.

    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                var obj = new GameObject("GameManager");
                _instance = obj.AddComponent<GameManager>();
                DontDestroyOnLoad(obj);
            }
            return _instance;
        }
    }

    // метод вообще ничего не делает, но как-то инициализировать наш синглтон надо, создавать переменную и присваивать ей ненужную ссылку на наш объект желания нет. 
    // Увы, просто GameManager.Instance сделать нельзя
    public void Initialize() { }


    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // Игнорируем столкновения между слоем "Enemy" и самим собой. Происходит игнорирование также всех зон/коллайдеров для данного слоя (слой можно назначить как для родительского,
        // так и для всех объектов. Если у объекта изменить слой у одного из дочерних элементов, будет происходить детекция коллизий коллайдеров и зон только для этого элемента
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Enemy"), LayerMask.NameToLayer("Enemy"));
    }
}