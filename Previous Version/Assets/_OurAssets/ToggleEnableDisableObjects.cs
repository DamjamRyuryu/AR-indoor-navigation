using UnityEngine;

public class ToggleEnableDisableObjects : MonoBehaviour
{
    public GameObject[] objectsToToggle;

    // 最小切换时间（秒）
    public float minToggleTime = 5f;

    // 最大切换时间（秒）
    public float maxToggleTime = 10f;

    // 记录每个物体下一次切换的时间戳
    private float[] nextToggleTimes;

    // 在启用脚本时初始化计时器
    private void OnEnable()
    {
        // 初始化每个物体的时间戳
        nextToggleTimes = new float[objectsToToggle.Length];
        for (int i = 0; i < objectsToToggle.Length; i++)
        {
            nextToggleTimes[i] = Time.time + Random.Range(minToggleTime, maxToggleTime);
        }
    }

    // 更新计时器并在到达切换时间时切换其他物体的激活状态
    private void Update()
    {
        for (int i = 0; i < objectsToToggle.Length; i++)
        {
            if (Time.time >= nextToggleTimes[i])
            {
                // 切换物体的激活状态
                objectsToToggle[i].SetActive(!objectsToToggle[i].activeSelf);

                // 更新下一次切换的时间戳
                nextToggleTimes[i] = Time.time + Random.Range(minToggleTime, maxToggleTime);
            }
        }
    }
}




