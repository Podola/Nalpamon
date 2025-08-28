using UnityEngine;

/// <summary>
/// @Managers 오브젝트와 그 자식들을 DontDestroyOnLoad로 만들어주는 부트스트랩 스크립트입니다.
/// </summary>
public class ManagerRoot : MonoBehaviour
{
    private void Awake()
    {
        // @Managers 오브젝트(자기 자신)를 DDOL로 설정합니다.
        DontDestroyOnLoad(gameObject);
    }
}