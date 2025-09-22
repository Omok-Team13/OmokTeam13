using UnityEngine;

public class EventMark : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            this.gameObject.SetActive(false);
        }
    }
}
