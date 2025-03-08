using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static DialogueArea;

public class DialogueArea : MonoBehaviour
{




    private void OnTriggerEnter2D(Collider2D other)
    {
        StartDialogue(other);
        gameObject.SetActive(false); // пока что исходим из положения о том, что разговор активируется при первом входе в диалоговую зону
    }

    private void StartDialogue(Collider2D other)
    {
        // подписываемся в FsmStateWalkEnemy 3397123971237912379
        if (other.gameObject.CompareTag("Player"))
        {
            string nameDialogueFile = SceneManager.GetActiveScene().name + "/" + gameObject.name; // по идее должно быть нечто вроде "SampleScene/Dialogue1"
            GameManager.Instance.StartDialogue(nameDialogueFile);
        }
    }
}