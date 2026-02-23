using UnityEngine;
using System.Collections;

public class Door : MonoBehaviour, IPlayerInteract
{
    public GameObject door;
    public BoxCollider doorCollider;
    public float moveTime = 1f;

    private Vector3 _newPosition;

    private void Start()
    {
        doorCollider = GetComponent<BoxCollider>();
        if (doorCollider != null)
        {
            doorCollider.isTrigger = false;
        }
    }

    public void Interact()
    {
        StartCoroutine(MoveDoor());
    }

    private IEnumerator MoveDoor()
    {
        float elapsedTime = 0f;
        Vector3 startingPosition = transform.position;
        doorCollider.isTrigger = false;
        _newPosition = new Vector3(transform.position.x, transform.position.y - 3f, transform.position.z); // Move the door 3 units down

        while (elapsedTime < moveTime)
        {
            transform.position = Vector3.Lerp(startingPosition, _newPosition, elapsedTime / moveTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.position = _newPosition;
        door.SetActive(false); // Deactivate the door after moving
    }
}
