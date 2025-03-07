using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static DialogueArea;

public class DialogueArea : MonoBehaviour
{
    private RectTransform rectTransformPositionDialogue;

    [SerializeField] private GameObject prefubPlayerDialogue;

    public delegate void DialogueStarted(PlayerDialogue sciptPlayerDialogue); // шаблон функции
    public event DialogueStarted onDialogueStarted;         // экземл€р(?) функции/сигнала(?)


    private void OnTriggerEnter2D(Collider2D other)
    {
        StartDialogue(other);
        gameObject.SetActive(false); // пока что исходим из положени€ о том, что разговор активируетс€ при первом входе в диалоговую зону
    }

    private void StartDialogue(Collider2D other)
    {
        // подписываемс€ в FsmStateWalkEnemy 3397123971237912379
        if (other.gameObject.CompareTag("Player"))
        {
            string nameDialogueFile = SceneManager.GetActiveScene().name + "/" + gameObject.name; // по идее должно быть нечто вроде "SampleScene/Dialogue1"
            GameManager.Instance.nameDialogueCurrent = nameDialogueFile;

            rectTransformPositionDialogue = GameObject.Find("PositionForDialogueWindow").GetComponent<RectTransform>();
            RectTransform UI = GameObject.Find("UI").GetComponent<RectTransform>();
            PlayerDialogue sciptPlayerDialogue = Instantiate(prefubPlayerDialogue, rectTransformPositionDialogue.position, rectTransformPositionDialogue.rotation, UI).GetComponent<PlayerDialogue>();
            onDialogueStarted?.Invoke(sciptPlayerDialogue);
        }
    }
}