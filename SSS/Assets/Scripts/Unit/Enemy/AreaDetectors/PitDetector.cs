using UnityEngine;

public class PitDetector : MonoBehaviour
{
    public delegate void DetectedPit();
    public event DetectedPit OnDetectedPit;
    private void OnTriggerExit2D(Collider2D other)
    {
        // ������������� �� ������� � ��������� FsmStateWalkEnemy
        Debug.Log("JUMPPPPPPPPPPPPPPPPPPPPP");
        if (other.gameObject.CompareTag("Ground")) { OnDetectedPit?.Invoke(); Debug.Log("JUMPPPPPPPPPPPPPPPPPPPPP"); }
    }
}
