using UnityEngine;

public class TestMove : MonoBehaviour
{
    float moveSpeed = 4;//속도 변수

    void Update()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");// 부드럽게 증감하는 값 

        //float h1 = Input.GetAxisRaw("Horizontal");
        //float v1 = Input.GetAxisRaw("Vertical"); //딱 떨어지는 값 

        Vector3 dir = new Vector3(h, 0, v);
        
        transform.position += dir * moveSpeed * Time.deltaTime;

       

    }

}
