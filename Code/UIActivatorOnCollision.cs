using UnityEngine;
using TMPro; // TextMeshPro���g�p���邽�߂ɕK�v

public class UIActivatorOnCollision : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI targetText; // TextMeshPro�̃e�L�X�gUI�v�f
    private int collisionCount = 0; // �����̃R���C�_�[���o�΍�

    void Start()
    {
        if (targetText != null)
        {
            targetText.gameObject.SetActive(false); // ������ԂŔ�\���ɂ���
        }
        else
        {
            Debug.LogWarning("Target TextMeshProUGUI is not assigned in the Inspector!");
        }
    }

    // BoxCollider2D�Ƃ̐ڐG�J�n��
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) // �ڐG�Ώۂ��^�O�Ŕ���
        {
            collisionCount++;
            if (targetText != null && collisionCount == 1) // �ŏ��̐ڐG���̂ݗL����
            {
                targetText.gameObject.SetActive(true);
            }
        }
    }

    // BoxCollider2D�Ƃ̐ڐG�I����
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) // �ڐG�Ώۂ��^�O�Ŕ���
        {
            collisionCount--;
            if (targetText != null && collisionCount == 0) // �S�Ă̐ڐG���I���������̂ݖ�����
            {
                targetText.gameObject.SetActive(false);
            }
        }
    }
}
