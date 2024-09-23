using Unity.VisualScripting;
using UnityEngine;

public class CamController : MonoBehaviour
{
    public float moveSpeed;
    [Range(0,1)]
    public float smoothTime;
    public Transform player;

    public void FixedUpdate()
    {
        Vector3 pos = GetComponent<Transform>().position;

        pos.x = Mathf.Lerp(pos.x, player.position.x, smoothTime);
        pos.y = Mathf.Lerp(pos.y, player.position.y, smoothTime);

        GetComponent<Transform>().position = pos;
    }
}