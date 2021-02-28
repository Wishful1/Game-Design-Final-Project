using System.Collections;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    private BoxCollider2D cameraBox;
    private Transform player;
    
    void Start()
    {
        cameraBox = GetComponent<BoxCollider2D>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
    }

    void Update()
    {
        AspectRatioBoxChange();
        FollowPlayer();
    }

    void AspectRatioBoxChange() //for camera size = 7.1
    {
        cameraBox.size = new Vector2((Camera.main.orthographicSize * 2) * Camera.main.aspect, Camera.main.orthographicSize * 2);
    }

    void FollowPlayer()
    {
        if(GameObject.Find("Boundary"))
        {
            transform.position = new Vector3(Mathf.Clamp(player.position.x, GameObject.Find("Boundary").GetComponent<BoxCollider2D>().bounds.min.x + (cameraBox.size.x / 2), GameObject.Find("Boundary").GetComponent<BoxCollider2D>().bounds.max.x - (cameraBox.size.x / 2)),
                                             Mathf.Clamp(player.position.y, GameObject.Find("Boundary").GetComponent<BoxCollider2D>().bounds.min.y + (cameraBox.size.y / 2), GameObject.Find("Boundary").GetComponent<BoxCollider2D>().bounds.max.y - (cameraBox.size.y / 2)), 
                                             transform.position.z);
        }
    }
}
