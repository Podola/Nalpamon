using UnityEngine;

/// <summary>
/// @Debug 오브젝트와 그 자식들을 DontDestroyOnLoad로 만들어주는 부트스트랩 스크립트입니다.
/// </summary>
public class DebugRoot : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}