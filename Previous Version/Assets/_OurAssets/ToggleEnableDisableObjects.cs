using UnityEngine;

public class ToggleEnableDisableObjects : MonoBehaviour
{
    public GameObject[] objectsToToggle;

    // ��С�л�ʱ�䣨�룩
    public float minToggleTime = 5f;

    // ����л�ʱ�䣨�룩
    public float maxToggleTime = 10f;

    // ��¼ÿ��������һ���л���ʱ���
    private float[] nextToggleTimes;

    // �����ýű�ʱ��ʼ����ʱ��
    private void OnEnable()
    {
        // ��ʼ��ÿ�������ʱ���
        nextToggleTimes = new float[objectsToToggle.Length];
        for (int i = 0; i < objectsToToggle.Length; i++)
        {
            nextToggleTimes[i] = Time.time + Random.Range(minToggleTime, maxToggleTime);
        }
    }

    // ���¼�ʱ�����ڵ����л�ʱ��ʱ�л���������ļ���״̬
    private void Update()
    {
        for (int i = 0; i < objectsToToggle.Length; i++)
        {
            if (Time.time >= nextToggleTimes[i])
            {
                // �л�����ļ���״̬
                objectsToToggle[i].SetActive(!objectsToToggle[i].activeSelf);

                // ������һ���л���ʱ���
                nextToggleTimes[i] = Time.time + Random.Range(minToggleTime, maxToggleTime);
            }
        }
    }
}




