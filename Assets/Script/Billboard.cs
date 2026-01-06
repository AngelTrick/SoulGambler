using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Transform mainCameraTransform;

    void Start()
    {
        // 메인 카메라의 Transform을 캐싱 (최적화)
        if (Camera.main != null)
        {
            mainCameraTransform = Camera.main.transform;
        }
    }

    void LateUpdate() // 카메라 이동 후(Update)에 실행되어야 떨림이 없음
    {
        if (mainCameraTransform != null)
        {
            // 카메라와 같은 방향을 바라보게 회전
            transform.forward = mainCameraTransform.forward;

            // 혹은 카메라를 쳐다보게 하려면:
             transform.LookAt(transform.position + mainCameraTransform.rotation * Vector3.forward, mainCameraTransform.rotation * Vector3.up);
        }
    }
}
