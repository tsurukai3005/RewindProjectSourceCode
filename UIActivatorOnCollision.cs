using UnityEngine;
using TMPro; // TextMeshProを使用するために必要

public class UIActivatorOnCollision : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI targetText; // TextMeshProのテキストUI要素
    private int collisionCount = 0; // 複数のコライダー検出対策

    void Start()
    {
        if (targetText != null)
        {
            targetText.gameObject.SetActive(false); // 初期状態で非表示にする
        }
        else
        {
            Debug.LogWarning("Target TextMeshProUGUI is not assigned in the Inspector!");
        }
    }

    // BoxCollider2Dとの接触開始時
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) // 接触対象をタグで判定
        {
            collisionCount++;
            if (targetText != null && collisionCount == 1) // 最初の接触時のみ有効化
            {
                targetText.gameObject.SetActive(true);
            }
        }
    }

    // BoxCollider2Dとの接触終了時
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) // 接触対象をタグで判定
        {
            collisionCount--;
            if (targetText != null && collisionCount == 0) // 全ての接触が終了した時のみ無効化
            {
                targetText.gameObject.SetActive(false);
            }
        }
    }
}
