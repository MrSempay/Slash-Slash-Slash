using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System.IO;
using System;
using static TitlesParser;
using Unity.VisualScripting;
using System.Collections;


public class ScrollFadeController : MonoBehaviour
{
    [SerializeField] private Transform _tViewBorderTop;
    [SerializeField] private Transform _tViewBorderBottom;
    [SerializeField] private Transform _tConteinerContent;
    [SerializeField] private TitlesParser _titlesParser;
    [SerializeField] private Transform _tTextEmptyPostSpace;
    [SerializeField] private float _periodControllingAlphaChannelTexts = 0.04f;
    [SerializeField] private ScrollRect _scrollRect;
    [SerializeField] private float _scrollSpeed = 0.2f; // скорость автопрокрутк

    private GameObject _prefubTextScrollView;
    private Dictionary<Transform, TextMeshProUGUI> _dictionaryRTAndTextMeshComponents = new();

    private void Awake()
    {
        _prefubTextScrollView = Resources.Load<GameObject>(C.Paths.PrefubTextScrollView);
    }
    private void Start()
    {
        foreach (DeserializeTextObject deserializeTextObject in _titlesParser.listTextsTitle)
        {
            InstantiateTextToScrollView(deserializeTextObject.text, deserializeTextObject.type);
        }

        _tTextEmptyPostSpace.SetParent(_tConteinerContent); // и затыкаем всю процессию пустым пространством

        StartCoroutine(ControlAlphaChannelTextsScrollView());
    }

    private void Update()
    {
        if (_scrollRect.verticalNormalizedPosition > 0f)
        {
            float contentHeight = _scrollRect.content.rect.height;
            float viewportHeight = _scrollRect.viewport.rect.height;

            float scrollableHeight = contentHeight - viewportHeight;
            if (scrollableHeight <= 0f)
                return;

            // Прокрутка в пикселях в секунду
            float deltaPixels = _scrollSpeed * Time.deltaTime;

            // Переводим в нормализованные единицы
            float deltaNormalized = deltaPixels / scrollableHeight;

            _scrollRect.verticalNormalizedPosition -= deltaNormalized;
            _scrollRect.verticalNormalizedPosition = Mathf.Clamp01(_scrollRect.verticalNormalizedPosition);
        }
    }

    private void InstantiateTextToScrollView(string text, DeserializeTextObject.TEXT_TYPE type)
    {
        switch (type)
        {
            case DeserializeTextObject.TEXT_TYPE.Header:
                // Добавляем разделитель сверху
                CreateSeparator();

                // Сам заголовок
                CreateTextElement(text, Color.green, C.Other.Header);

                // Добавляем разделитель снизу
                CreateSeparator();
                break;

            case DeserializeTextObject.TEXT_TYPE.Descripton:
                CreateTextElement(text, Color.white, C.Other.Descripton, 44f);
                break;

            case DeserializeTextObject.TEXT_TYPE.Separator:
                CreateSeparator();
                break;

            case DeserializeTextObject.TEXT_TYPE.DescriptonSmall:
                CreateTextElement(text, Color.white, C.Other.Descripton, 26f);
                break;
        }
    }

    /// <summary>
    /// Создаёт элемент текста в ScrollView
    /// </summary>
    private TextMeshProUGUI CreateTextElement(string text, Color color, string name, float fontSize = 54f)
    {
        var textMesh = Instantiate(_prefubTextScrollView, _tConteinerContent)
                       .GetComponent<TextMeshProUGUI>();

        textMesh.text = text;
        textMesh.color = color;
        textMesh.fontSize = fontSize;
        textMesh.gameObject.name = name;

        ////Debug.Log(textMesh.gameObject.transform);
        ////Debug.Log(textMesh);
        _dictionaryRTAndTextMeshComponents[textMesh.gameObject.transform] = textMesh;

        return textMesh;
    }

    /// <summary>
    /// Создаёт "пустую линию"-разделитель
    /// </summary>
    private void CreateSeparator()
    {
        var separator = Instantiate(_prefubTextScrollView, _tConteinerContent)
                        .GetComponent<TextMeshProUGUI>();

        separator.alpha = 0f;
        separator.text = C.Other.SeparateLine;
        separator.gameObject.name = C.Other.SeparateLine;
    }

    private IEnumerator ControlAlphaChannelTextsScrollView()
    {
        while (true)
        {
            foreach (var rtAndMeshComponentTextScrollView in _dictionaryRTAndTextMeshComponents)
            {
                float currentY = rtAndMeshComponentTextScrollView.Key.position.y;
                float alpha = AlphaWithPlateau(currentY, transform.position.y, _tViewBorderBottom.position.y, _tViewBorderTop.position.y);
                rtAndMeshComponentTextScrollView.Value.alpha = alpha;
                //if (currentYPositionText > _tViewBorderBottom.position.y && currentYPositionText < _tViewBorderTop.position.y)
                //{
                //    float alphaValue = currentYPositionText < transform.position.y? 
                //        (currentYPositionText / transform.position.y) : 
                //        (transform.position.y / currentYPositionText);

                //    rtAndMeshComponentTextScrollView.Value.alpha = alphaValue;
                //}
            }
            yield return new WaitForSeconds(_periodControllingAlphaChannelTexts);
        }
    }

    private float AlphaWithPlateau(float currentY, float centerY, float bottomY, float topY, float edgePercent = 0.2f)
    {
        if (currentY <= bottomY || currentY >= topY)
            return 0f;

        if (currentY < centerY)
        {
            float t = Mathf.InverseLerp(bottomY, centerY, currentY); // 0..1
            if (t < edgePercent)
            {
                // плавный рост от 0 -> 1
                return Mathf.InverseLerp(0f, edgePercent, t);
            }
            else
            {
                // зона плато
                return 1f;
            }
        }
        else
        {
            float t = Mathf.InverseLerp(centerY, topY, currentY); // 0..1
            if (t < (1f - edgePercent))
            {
                // зона плато
                return 1f;
            }
            else
            {
                // плавное уменьшение от 1 -> 0
                return Mathf.InverseLerp(1f, 1f - edgePercent, t);
            }
        }
    }

    private void OnDestroy()
    {
        StopCoroutine(ControlAlphaChannelTextsScrollView());
    }

}
