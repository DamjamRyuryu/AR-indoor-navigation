using UnityEngine;

public class MakeObjectTransparent : MonoBehaviour
{
    public float transparency = 0.5f; // ͸����ֵ����Χ��0��1��0��ʾ��ȫ͸����1��ʾ��ȫ��͸��
    private Material transparentMaterial; // ����͸��Ч���Ĳ���

    void Start()
    {
        // ��ȡ�����Renderer����������������
        Renderer renderer = GetComponent<Renderer>();
        transparentMaterial = new Material(renderer.material);

        // ����������Ϊ͸������
        renderer.material = transparentMaterial;
    }

    void Update()
    {
        // ��̬����͸����
        Color color = transparentMaterial.color;
        color.a = transparency;
        transparentMaterial.color = color;
    }
}
