using UnityEngine;

public class Player2D : MonoBehaviour
{
    [SerializeField] private float Move_Speed = 5f;
    [SerializeField] private Vector3 Move_Direction;

    private void Update()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        Move_Direction = new Vector2(moveX, moveY); //.normalized

        transform.Translate(Move_Direction * Move_Speed * Time.deltaTime);
    }
}
