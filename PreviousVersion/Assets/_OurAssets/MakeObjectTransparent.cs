using UnityEngine;

public class MakeObjectTransparent : MonoBehaviour
{
    public float transparency = 0.5f; // 透明度值，范围从0到1。0表示完全透明，1表示完全不透明
    private Material transparentMaterial; // 用于透明效果的材质

    void Start()
    {
        // 获取物体的Renderer组件，并复制其材质
        Renderer renderer = GetComponent<Renderer>();
        transparentMaterial = new Material(renderer.material);

        // 将材质设置为透明材质
        renderer.material = transparentMaterial;
    }

    void Update()
    {
        // 动态更新透明度
        Color color = transparentMaterial.color;
        color.a = transparency;
        transparentMaterial.color = color;
    }
}
