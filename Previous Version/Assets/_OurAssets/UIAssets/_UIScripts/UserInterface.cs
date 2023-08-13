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
    /// 实现复用
    /// </summary>
    public class UserInterface : MonoBehaviour
    {
        public RectTransform CellRerf;      //Cell预设 的 RectTransform

        public int cellCount = 0;               //计划生成的Cell数量

        public float paddingTop = 10;          //上边界宽度

        public float paddingBottom = 10;         //下边界宽度

        public float spacingY = 10;             //Y向间距

        public string serverIP = "localhost";
        public string serverPort = "8000";
        //视口容差（即左右两侧的Cell出现消失参考点），可避免复用时露馅  //为正时 参考位置向viewport的外围增加。
        protected float viewportOffsetTop = 0;     //上侧视口容差
        protected float viewportOffsetBottom = 0;    //下侧视口容差
        protected int humancount;
        protected int gndex = 0;
        protected ScrollRect scrollRect;        //ScrollRect
        protected RectTransform contentRT;      //Content 的 RectTransform
        protected RectTransform viewportRT;     //viewport 的 RectTransform
        protected TextMeshProUGUI messagebar;     //message bar
        protected GameObject _pathCreator;
        protected HumanGenerator _groundmesh;
        protected float contentHeight;           //Content的总高度
        protected float pivotOffsetY;           //由Cell的pivot决定的起始偏移值

        protected Dictionary<int, RectTransform> cellRTDict;    //index-Cell字典    
        protected Dictionary<int, Tritext> cellTXTDict;    //index-txt字典 
        protected Dictionary<string, Vector3> cellTransDict;
        protected Dictionary<int, Vector3> ranHumanDict;
        protected Stack<RectTransform> unUseCellRTStack;        //空闲Cell堆栈
        protected List<KeyValuePair<int, RectTransform>> cellRTListForSort;     //Cell列表用于辅助Sbling排序

        protected List<int> oldIndexes;         //旧的索引集合
        protected List<int> newIndexes;         //新的索引集合

        protected List<int> appearIndexes;      //将要出现的索引集合   //使用List而非单个，可以支持Content位置跳变
        protected List<int> disAppearIndexes;   //将要消失的索引集合   //使用List而非单个，可以支持Content位置跳变

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

            //依赖的组件
            scrollRect = GetComponent<ScrollRect>();
            contentRT = scrollRect.content;
            viewportRT = scrollRect.viewport;
            _pathCreator = GameObject.Find("PlayerCapsule");
            _groundmesh = GameObject.Find("Environment/Ground_Mesh").GetComponent<HumanGenerator>();
            messagebar = transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>();

            //强制设置 Content的 anchor 和 pivot
            contentRT.anchorMin = new Vector2(contentRT.anchorMin.x, 1);
            contentRT.anchorMax = new Vector2(contentRT.anchorMax.x, 1);
            contentRT.pivot = new Vector2(contentRT.pivot.x, 1);

            //计算由Cell的pivot决定的起始偏移值
            pivotOffsetY = (1 - CellRerf.pivot.y) * CellRerf.rect.height;

            //注册滑动事件
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

        //调整边距
        protected virtual void FixPadding() { }

        //调整间距
        protected virtual void FixspacingY() { }

        //调整视口容差
        protected virtual void FixViewportOffset()
        {
            viewportOffsetTop = spacingY;
            viewportOffsetBottom = spacingY;
        }

        //计算并设置Content大小
        protected virtual void CalcAndSetContentSize()
        {
            //计算和设置Content总宽度
            //当cellCount大于0时，Content总宽度 = 上边界间隙 + 所有Cell的高度总和 + 相邻间距总和 + 下边界间隙
            contentHeight = paddingTop + CellRerf.rect.height * cellCount + spacingY * (cellCount - 1) + paddingBottom;
            contentRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, contentHeight);
        }

        //计算 应出现的索引 和 应消失的索引
        protected virtual void CalcIndexes()
        {
            //始终以viewpoert上边界为参考原点，向上为正方向观察。则有：
            //content上边界 相对于 viewport上边界（含viewportOffset） 的位移为：
            float outHeightFromTop = 0 + (contentRT.anchoredPosition.y + viewportOffsetTop);
            //content下边界 相对于 viewport上边界（含viewportOffset） 的位移为：
            float outHeightFromBottom = 0 + contentRT.anchoredPosition.y - contentHeight + (viewportRT.rect.height + viewportOffsetBottom);
            //Debug.Log("TOP:" + outHeightFromTop);
            //Debug.Log("Bottom:" + outHeightFromBottom);
            //计算完全滑出上边界和完全滑出下边的数量。 要向下取整，即尽量认为其没滑出，以保证可视区域内的正确性。
            int outCountFromTop = 0;    //完全滑出上边界的数量
            int outCountFromBottom = 0;   //完全滑出下边界的数量
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

            //应该显示的开始索引和结束索引
            int startIndex = (outCountFromTop); // 省略了 先+1再-1。 从滑出的下一个开始，索引从0开始;
            int endIndex = (cellCount - 1 - outCountFromBottom);

            //Debug.Log("startIndex, endIndex: " + startIndex + ", " + endIndex);

            for (int index = startIndex; index <= endIndex; index++)
            {
                newIndexes.Add(index);
            }

            ////新旧索引列表输出调试
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

            //找出出现的和消失的
            //出现的：在新列表中，但不在老列表中。
            appearIndexes.Clear();
            foreach (int index in newIndexes)
            {
                if (oldIndexes.IndexOf(index) < 0)
                {
                    //Debug.Log("出现：" + index);
                    appearIndexes.Add(index);
                }
            }

            //消失的：在老列表中，但不在新列表中。
            disAppearIndexes.Clear();
            foreach (int index in oldIndexes)
            {
                if (newIndexes.IndexOf(index) < 0)
                {
                    //Debug.Log("消失：" + index);
                    disAppearIndexes.Add(index);
                }
            }

            //oldIndexes保存当前帧索引数据。
            List<int> temp;
            temp = oldIndexes;
            oldIndexes = newIndexes;
            newIndexes = temp;
            newIndexes.Clear();
        }

        //该消失的消失
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

        //该出现的出现
        protected virtual void AppearCells()
        {
            foreach (int index in appearIndexes)
            {
                RectTransform cellRT = GetOrCreateCell(index);
                cellRTDict[index] = cellRT;
                //Debug.Log(cellRT.anchoredPosition);
                //设置Cell位置
                cellRT.anchoredPosition = new Vector2(0, CalcCellPosY(index));

                //设置Cell数据，对Cell进行初始化
                //CellHandler
                cellRT.GetComponent<CellPref>().SetButtonText(cellTXTDict[index].name.Replace('_', ' '));
                cellRT.GetComponent<CellPref>().SetInfo(cellTXTDict[index].info);
            }
        }

        //计算并设置Cells的SblingIndex
        //调用时机：有新的Cell出现时
        //Cell可能重叠时必须
        //若无需求，可去掉以节省性能
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
                //按index升序
                return x.Key - y.Key;
            });

            foreach (KeyValuePair<int, RectTransform> kvp in cellRTListForSort)
            {
                //索引大的在上
                //kvp.Value.SetAsLastSibling();
                //索引大的在下
                kvp.Value.SetAsFirstSibling();
            }
        }

        //计算Cell的Y坐标
        protected virtual float CalcCellPosY(int index)
        {
            //Y = 上边界间隙 + 由Cell的pivot决定的起始偏移值 + 前面已有Cell的高度总和 + 前面已有的间距总和
            float y = paddingTop + pivotOffsetY + CellRerf.rect.height * index + spacingY * index;

            //Debug.Log("index, cellPosY:" + index + "," + -y);
            return -y;
        }

        //获取或创建Cell
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
                //强制设置Cell的anchor

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
                //强制设置Cell的anchor
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
                 * API访问URL:"http://[ServerIP]:[ServerPort]/location/<string>input"
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