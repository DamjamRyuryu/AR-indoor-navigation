using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Networking;
using UnityEngine.UI;
//using UnityEngine.UIElements;

[Serializable]
public class Tritext
{
    public string name;
    public string info;
    public Vector3 position;
}
[Serializable]
public class TritextGroup
{
    public List<Tritext> tritexts;
}

[Serializable]
public class Positions
{
    public Vector3 transpose;
}
[Serializable]
public class PeopleGroup
{
    public List<Positions> positions;
}
namespace ScrollList.Basic
{

    /// <summary>
    /// ʵ�ָ���
    /// </summary>
    public class UserInterface : MonoBehaviour
    {
        public RectTransform CellRerf;      //CellԤ�� �� RectTransform

        public int cellCount = 0;               //�ƻ����ɵ�Cell����

        public float paddingTop = 10;          //�ϱ߽���

        public float paddingBottom = 10;         //�±߽���

        public float spacingY = 10;             //Y����

        public string serverIP = "localhost";
        public string serverPort = "8000";
        //�ӿ��ݲ�����������Cell������ʧ�ο��㣩���ɱ��⸴��ʱ¶��  //Ϊ��ʱ �ο�λ����viewport����Χ���ӡ�
        protected float viewportOffsetTop = 0;     //�ϲ��ӿ��ݲ�
        protected float viewportOffsetBottom = 0;    //�²��ӿ��ݲ�
        protected int humancount;
        protected int gndex = 0;
        protected ScrollRect scrollRect;        //ScrollRect
        protected RectTransform contentRT;      //Content �� RectTransform
        protected RectTransform viewportRT;     //viewport �� RectTransform
        protected TextMeshProUGUI messagebar;     //message bar
        protected GameObject _pathCreator;
        protected HumanGenerator _groundmesh;
        protected float contentHeight;           //Content���ܸ߶�
        protected float pivotOffsetY;           //��Cell��pivot��������ʼƫ��ֵ

        protected Dictionary<int, RectTransform> cellRTDict;    //index-Cell�ֵ�    
        protected Dictionary<int, Tritext> cellTXTDict;    //index-txt�ֵ� 
        protected Dictionary<string, Vector3> cellTransDict;
        protected Dictionary<int, Vector3> ranHumanDict;
        protected Stack<RectTransform> unUseCellRTStack;        //����Cell��ջ
        protected List<KeyValuePair<int, RectTransform>> cellRTListForSort;     //Cell�б����ڸ���Sbling����

        protected List<int> oldIndexes;         //�ɵ���������
        protected List<int> newIndexes;         //�µ���������

        protected List<int> appearIndexes;      //��Ҫ���ֵ���������   //ʹ��List���ǵ���������֧��Contentλ������
        protected List<int> disAppearIndexes;   //��Ҫ��ʧ����������   //ʹ��List���ǵ���������֧��Contentλ������

        protected Keyboard m_keyboard = Keyboard.current;
        protected Mouse m_mouse = Mouse.current;
        protected bool Visibility = true;
        protected bool _updateFlag = false;
        protected bool _Pup = false;
        protected virtual void Awake()
        {
            cellRTDict = new Dictionary<int, RectTransform>();
            cellTXTDict = new Dictionary<int, Tritext>();
            cellTransDict = new Dictionary<string, Vector3>();
            ranHumanDict = new Dictionary<int, Vector3>();
            unUseCellRTStack = new Stack<RectTransform>();
            cellRTListForSort = new List<KeyValuePair<int, RectTransform>>();

            oldIndexes = new List<int>();
            newIndexes = new List<int>();
            appearIndexes = new List<int>();
            disAppearIndexes = new List<int>();

            //���������
            scrollRect = GetComponent<ScrollRect>();
            contentRT = scrollRect.content;
            viewportRT = scrollRect.viewport;
            _pathCreator = GameObject.Find("PlayerCapsule");
            _groundmesh = GameObject.Find("Environment/Ground_Mesh").GetComponent<HumanGenerator>();
            messagebar = transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>();

            //ǿ������ Content�� anchor �� pivot
            contentRT.anchorMin = new Vector2(contentRT.anchorMin.x, 1);
            contentRT.anchorMax = new Vector2(contentRT.anchorMax.x, 1);
            contentRT.pivot = new Vector2(contentRT.pivot.x, 1);

            //������Cell��pivot��������ʼƫ��ֵ
            pivotOffsetY = (1 - CellRerf.pivot.y) * CellRerf.rect.height;

            //ע�Ử���¼�
            scrollRect.onValueChanged.AddListener(OnScrollValueChanged);

        }

        protected virtual void Start()
        {
            if (cellCount < 0)
            {
                return;
            }
            //string text = "{ \"tritexts\":[{ \"name\":\"the_cool_sphere\",\"info\":\"This is a sphere\",\"position\":[6.82,2.637,-30.8602]},{ \"name\":\"orange_painting\",\"info\":\"This is an orange paintinig\",\"position\":[-17.85389,2.878457,-81.2202]},{ \"name\":\"red_painting\",\"info\":\"This is a red paintinig\",\"position\":[15.98,9.891,-37.816]},{ \"name\":\"green_painting\",\"info\":\"This is a green paintinig\",\"position\":[15.98,9.891,-37.816]},{ \"name\":\"statue\",\"info\":\"This is a statue\",\"position\":[6.79,9.887457,-31.6]},{ \"name\":\"stage\",\"info\":\"This is a stage\",\"position\":[-27.77,9.887457,-77.93]},{ \"name\":\"toilet1\",\"info\":\"This is toilet1\",\"position\":[-28.50988,2.655457,-67.1292]},{ \"name\":\"toilet2\",\"info\":\"This is toilet2\",\"position\":[38.733,9.887457,-36.97]},{ \"name\":\"shop\",\"info\":\"This is shop\",\"position\":[-34.62389,9.887457,-59.0702]},{ \"name\":\"box_obstacle1\",\"info\":\"Meet Obstacle\",\"position\":[12.8,1.252868,79.27]}]}";
            //foreach(Tritext data in JsonUtility.FromJson<TritextGroup>(text).tritexts)
            //{
            //    Debug.Log(data.name+' '+data.info+' '+data.position.ToString());
            //}

            //for (int i = 0; i < cellCount; i++)
            //{
            //    Tritext bitext = new()
            //    {
            //        info = "info." + i,
            //        name = "btn." + i
            //    };

            //    cellTXTDict.Add(i, bitext);
            //}

            FixPadding();
            FixspacingY();
            FixViewportOffset();
            GetInfoList();



            //for (int index = 0; index < cellCount; index++)
            //{
            //    GetOrCreateCell(index);
            //}
        }

        protected virtual void OnScrollValueChanged(Vector2 delta)
        {
            if (cellCount < 0)
            {
                return;
            }
            //Debug.Log("OnScrollValueChanged");
            CalcIndexes();
            DisAppearCells();
            AppearCells();
            CalcAndSetCellsSblingIndex();
        }

        //�����߾�
        protected virtual void FixPadding() { }

        //�������
        protected virtual void FixspacingY() { }

        //�����ӿ��ݲ�
        protected virtual void FixViewportOffset()
        {
            viewportOffsetTop = spacingY;
            viewportOffsetBottom = spacingY;
        }

        //���㲢����Content��С
        protected virtual void CalcAndSetContentSize()
        {
            //���������Content�ܿ��
            //��cellCount����0ʱ��Content�ܿ�� = �ϱ߽��϶ + ����Cell�ĸ߶��ܺ� + ���ڼ���ܺ� + �±߽��϶
            contentHeight = paddingTop + CellRerf.rect.height * cellCount + spacingY * (cellCount - 1) + paddingBottom;
            contentRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, contentHeight);
        }

        //���� Ӧ���ֵ����� �� Ӧ��ʧ������
        protected virtual void CalcIndexes()
        {
            //ʼ����viewpoert�ϱ߽�Ϊ�ο�ԭ�㣬����Ϊ������۲졣���У�
            //content�ϱ߽� ����� viewport�ϱ߽磨��viewportOffset�� ��λ��Ϊ��
            float outHeightFromTop = 0 + (contentRT.anchoredPosition.y + viewportOffsetTop);
            //content�±߽� ����� viewport�ϱ߽磨��viewportOffset�� ��λ��Ϊ��
            float outHeightFromBottom = 0 + contentRT.anchoredPosition.y - contentHeight + (viewportRT.rect.height + viewportOffsetBottom);
            //Debug.Log("TOP:" + outHeightFromTop);
            //Debug.Log("Bottom:" + outHeightFromBottom);
            //������ȫ�����ϱ߽����ȫ�����±ߵ������� Ҫ����ȡ������������Ϊ��û�������Ա�֤���������ڵ���ȷ�ԡ�
            int outCountFromTop = 0;    //��ȫ�����ϱ߽������
            int outCountFromBottom = 0;   //��ȫ�����±߽������
            if (outHeightFromTop > 0)
            {
                outCountFromTop = Mathf.FloorToInt((outHeightFromTop - paddingTop + spacingY) / (CellRerf.rect.height + spacingY));
                outCountFromTop = Mathf.Clamp(outCountFromTop, 0, cellCount);
            }
            if (outHeightFromBottom < 0)
            {
                outCountFromBottom = Mathf.FloorToInt((-outHeightFromBottom - paddingBottom + spacingY) / (CellRerf.rect.height + spacingY));
                outCountFromBottom = Mathf.Clamp(outCountFromBottom, 0, cellCount);
            }

            //Debug.Log("outFromTop, outFromBottom: " + outCountFromTop + ", " + outCountFromBottom);

            //Ӧ����ʾ�Ŀ�ʼ�����ͽ�������
            int startIndex = (outCountFromTop); // ʡ���� ��+1��-1�� �ӻ�������һ����ʼ��������0��ʼ;
            int endIndex = (cellCount - 1 - outCountFromBottom);

            //Debug.Log("startIndex, endIndex: " + startIndex + ", " + endIndex);

            for (int index = startIndex; index <= endIndex; index++)
            {
                newIndexes.Add(index);
            }

            ////�¾������б��������
            //string Str1 = "";
            //foreach (int index in newIndexes)
            //{
            //    Str1 += index + ",";
            //}
            //string Str2 = "";
            //foreach (int index in oldIndexes)
            //{
            //    Str2 += index + ",";
            //}
            //Debug.Log("Str1: " + Str1);
            //Debug.Log("Str2: " + Str2);
            //Debug.Log("-------------------------");

            //�ҳ����ֵĺ���ʧ��
            //���ֵģ������б��У����������б��С�
            appearIndexes.Clear();
            foreach (int index in newIndexes)
            {
                if (oldIndexes.IndexOf(index) < 0)
                {
                    //Debug.Log("���֣�" + index);
                    appearIndexes.Add(index);
                }
            }

            //��ʧ�ģ������б��У����������б��С�
            disAppearIndexes.Clear();
            foreach (int index in oldIndexes)
            {
                if (newIndexes.IndexOf(index) < 0)
                {
                    //Debug.Log("��ʧ��" + index);
                    disAppearIndexes.Add(index);
                }
            }

            //oldIndexes���浱ǰ֡�������ݡ�
            List<int> temp;
            temp = oldIndexes;
            oldIndexes = newIndexes;
            newIndexes = temp;
            newIndexes.Clear();
        }

        //����ʧ����ʧ
        protected virtual void DisAppearCells()
        {
            foreach (int index in disAppearIndexes)
            {
                RectTransform cellRT = cellRTDict[index];
                cellRTDict.Remove(index);
                cellRT.gameObject.SetActive(false);
                unUseCellRTStack.Push(cellRT);
                //Debug.Log("iterate:" + index)
                ;
            }
        }

        //�ó��ֵĳ���
        protected virtual void AppearCells()
        {
            foreach (int index in appearIndexes)
            {
                RectTransform cellRT = GetOrCreateCell(index);
                cellRTDict[index] = cellRT;
                //Debug.Log(cellRT.anchoredPosition);
                //����Cellλ��
                cellRT.anchoredPosition = new Vector2(0, CalcCellPosY(index));

                //����Cell���ݣ���Cell���г�ʼ��
                //CellHandler
                cellRT.GetComponent<CellPref>().SetButtonText(cellTXTDict[index].name.Replace('_', ' '));
                cellRT.GetComponent<CellPref>().SetInfo(cellTXTDict[index].info);
            }
        }

        //���㲢����Cells��SblingIndex
        //����ʱ�������µ�Cell����ʱ
        //Cell�����ص�ʱ����
        //�������󣬿�ȥ���Խ�ʡ����
        protected virtual void CalcAndSetCellsSblingIndex()
        {
            if (appearIndexes.Count <= 0) { return; }

            cellRTListForSort.Clear();
            foreach (KeyValuePair<int, RectTransform> kvp in cellRTDict)
            {
                cellRTListForSort.Add(kvp);
            }
            cellRTListForSort.Sort((x, y) =>
            {
                //��index����
                return x.Key - y.Key;
            });

            foreach (KeyValuePair<int, RectTransform> kvp in cellRTListForSort)
            {
                //�����������
                //kvp.Value.SetAsLastSibling();
                //�����������
                kvp.Value.SetAsFirstSibling();
            }
        }

        //����Cell��Y����
        protected virtual float CalcCellPosY(int index)
        {
            //Y = �ϱ߽��϶ + ��Cell��pivot��������ʼƫ��ֵ + ǰ������Cell�ĸ߶��ܺ� + ǰ�����еļ���ܺ�
            float y = paddingTop + pivotOffsetY + CellRerf.rect.height * index + spacingY * index;

            //Debug.Log("index, cellPosY:" + index + "," + -y);
            return -y;
        }

        //��ȡ�򴴽�Cell
        private RectTransform GetOrCreateCell(int index)
        {
            RectTransform cellRT;
            if (unUseCellRTStack.Count > 0)
            {
                cellRT = unUseCellRTStack.Pop();
                cellRT.gameObject.SetActive(true);
            }
            else
            {
                cellRT = GameObject.Instantiate<GameObject>(CellRerf.gameObject).GetComponent<RectTransform>();

                cellRT.SetParent(contentRT, false);
                cellRT.anchorMin = new Vector2(cellRT.anchorMin.x, 1);
                cellRT.anchorMax = new Vector2(cellRT.anchorMax.x, 1);
                //ǿ������Cell��anchor

            }

            return cellRT;
        }

        /*private RectTransform GetOrCreateCell(int index, string name, string info)
        {
            RectTransform cellRT;

            if (unUseCellRTStack.Count > 0)
            {
                cellRT = unUseCellRTStack.Pop();
                cellRT.GetComponent<CellPref>().SetButtonText(name);
                cellRT.GetComponent<CellPref>().SetInfo(info);
                cellRT.gameObject.SetActive(true);
            }
            else
            {
                cellRT = GameObject.Instantiate<GameObject>(CellRerf.gameObject).GetComponent<RectTransform>();
                cellRT.SetParent(contentRT, false);
                cellRT.GetComponent<CellPref>().SetButtonText(name);
                cellRT.GetComponent<CellPref>().SetInfo(info);
                //ǿ������Cell��anchor
                cellRT.anchorMin = new Vector2(cellRT.anchorMin.x, 0);
                cellRT.anchorMax = new Vector2(cellRT.anchorMax.x, 0);
            }

            return cellRT;
        }*/

        private void GetInfoList()
        {
            //Debug.Log("Info should be got from server here!");
            messagebar.SetText("Downloading information from server...");
            string _url = "http://" + serverIP + ':' + serverPort + "/loinfo/";
            Debug.Log(_url);
            StartCoroutine(GetRequest(_url, 0));

            //Tritext tempdict;
            //int index = 0;
        }
        IEnumerator GetRequest(string url, int handle)
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
            {
                // Request and wait for the desired page.
                yield return webRequest.SendWebRequest();

                string[] pages = url.Split('/');
                int page = pages.Length - 1;

                switch (webRequest.result)
                {
                    case UnityWebRequest.Result.ConnectionError:
                    case UnityWebRequest.Result.DataProcessingError:
                        Debug.LogError(pages[page] + ": Error: " + webRequest.error);
                        break;
                    case UnityWebRequest.Result.ProtocolError:
                        Debug.LogError(pages[page] + ": HTTP Error: " + webRequest.error);
                        break;
                    case UnityWebRequest.Result.Success:
                        Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);
                        /* The Json should be:
                         * jsonstring = {"tritexts":[{"name":"***","info":"***","position":{"x":0.1,"y":0.2,"z":0.3}},{"name":"+++","info":"+++","position":{"x":1.1,"y":2.2,"z":3.3}},...]}
                         *              ----------------------------------------------------------------------------------------------------------------------------------------------------
                         * Remember : there is NO space(' ') in the whole underlined string except the spaces you use in the sentences which is the value the 'name' or 'info'elements
                        */
                        switch (handle)
                        {
                            case 0:
                                TritextGroup recipt = JsonUtility.FromJson<TritextGroup>(webRequest.downloadHandler.text);
                                int index = 0;
                                foreach (Tritext _tritext in recipt.tritexts)
                                {
                                    string name = _tritext.name.Replace('_', ' ');
                                    Debug.Log(string.Format("name={0},info={1},position={2}", name, _tritext.info, _tritext.position));
                                    cellTXTDict.Add(index, _tritext);
                                    cellTransDict.Add(name, _tritext.position);
                                    index++;
                                }
                                cellCount = index;
                                CalcAndSetContentSize();
                                CalcIndexes();
                                //DisAppearCells();
                                AppearCells();
                                CalcAndSetCellsSblingIndex();
                                messagebar.SetText("Welcome to the museum!");
                                break;
                            case 1:
                                int index1 = 0;
                                PeopleGroup rec_p = JsonUtility.FromJson<PeopleGroup>(webRequest.downloadHandler.text);
                                foreach(Positions _pos in rec_p.positions)
                                {
                                    //Debug.Log(string.Format("position={0}", _pos.transpose));
                                    ranHumanDict.Add(index1,_pos.transpose);
                                    index1++;
                                }
                                humancount = index1 - 1;
                                
                                _Pup = true;
                                break;
                        }
                        break;
                }

            }
        }

        //Handle button event
        public void StringEventListener(string input)
        {
            Debug.Log("MainPage receive:" + input);
            if (input.Equals("crowdctrl") && !_updateFlag)
            {
                _updateFlag = true;
                string _url = "http://" + serverIP + ':' + serverPort + "/random_people/";
                StartCoroutine(GetRequest(_url, 1));
                messagebar.SetText("Crowd Simulation Activated!");
            }
            else
            {
                messagebar.SetText("Navigating to " + input + '.');
                //GameObject emptyOb = new();
                //emptyOb.transform.position = cellTransDict[input];
                _pathCreator.GetComponent<PathDisplay>().destination = cellTransDict[input];
                Debug.Log(input);
                //Destroy(emptyOb);
                /*
                 * API����URL:"http://[ServerIP]:[ServerPort]/location/<string>input"
                 * Expected receive: string ("x,y,z")
                 * ------------Possible Code-------------
                 * string[] txts = receipt.split(',');
                 * Transform destination = new Vertor3(float.Parse(x),float.Parse(y),float.parse(z));
                 *  //Change the transform.position of the Navi Agent
                */
            }
        }
        protected virtual void Update()
        {
            //change visibilty
            if (m_keyboard.mKey.wasPressedThisFrame)
            {
                Visibility = !Visibility;
                Debug.Log("M Pressed");
                GetComponent<CanvasGroup>().alpha = Visibility ? 1.0f : 0f;
                Cursor.lockState = Visibility ? CursorLockMode.None : CursorLockMode.Locked;
            }
            if (_Pup)
            {
                if (gndex < humancount)
                {
                    _groundmesh.position = ranHumanDict[gndex];
                    gndex++;
                }
                else
                {
                    gndex = 0;
                    messagebar.SetText("Crowd simulation complete.");
                    _Pup = false;
                    _updateFlag = false;
                }

            }
        }


    }
}