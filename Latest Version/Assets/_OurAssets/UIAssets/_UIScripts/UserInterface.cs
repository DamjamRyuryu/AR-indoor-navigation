using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.Windows.Speech;
//using UnityEngine.UIElements;

/*
 * The most important script in this demo, which contains almost all of the functions an AR device client should acquire.
 * As is mentioned in PageView.cs, the menu part of this script is based on https://github.com/NRatel/Unity-ListView. 
 * I changed the menu to a up-to-down scrolling menu and added several functions to connect other C# script and the server.
 * I have translated the Chinese comments into English for unity.
 *                                                                                          ----Jerry Wang,CIS summer 2023
 */

// The following four classes are used to separate the data downloaded from the server.
[Serializable]
public class Tritext
{
    public string name;
    public string info;
    public Vector3 position;
    public string imagestr;
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
    /// The main script for UI.
    /// </summary>
    public class UserInterface : MonoBehaviour
    {
        public RectTransform CellRerf;          // Represent which cell prefab will be used
        public int cellCount = 0;               // The total number of cells. It will be set automatically by function.
        public float paddingTop = 10;           // The width of gap between the top cell and the upper view port border
        public float paddingBottom = 10;        // The width of gap between the bottom cell and the bottom view port border
        public float spacingY = 10;             // The gap between adjacent cell

        // The IP and port of the server.
        public string serverIP = "localhost";
        public string serverPort = "8000";

        // The parameter for voice recognizer
        public bool englishPhrase = true;       // Whether the keywords are created in English.
        public ConfidenceLevel confidenceLevel = ConfidenceLevel.Medium;

        // Viewport offset. Set the reference point where cells disappear.
        protected float viewportOffsetTop = 0;
        protected float viewportOffsetBottom = 0;

        protected int humancount;               // The number of the random people
        protected int gndex = 0;                // Represent the index of the position that HumanGenerator.cs is going to create this frame

        // Related components and objects
        protected ScrollRect scrollRect;
        protected RectTransform contentRT;
        protected RectTransform viewportRT;
        protected TextMeshProUGUI _messagebar;
        protected GameObject _inspector;
        protected GameObject _detailpanel;
        protected CanvasGroup _voiceRecogIcon;
        protected HumanGenerator _groundmeshHG;
        protected PhraseRecognizer m_phraseRecognizer;

        protected float contentHeight;          // Content's total height
        protected float pivotOffsetY;           // the offset value determined by the initial position of the pivot of the cell

        // Dictionaries, stack and lists
        protected Dictionary<int, RectTransform> cellRTDict;        //index-Cell
        protected Dictionary<int, Tritext> cellTXTDict;             //index-Tritext 
        protected Dictionary<string, Vector3> cellTransDict;        //nameTag-position
        protected Dictionary<int, Vector3> ranHumanDict;            //index-(human clone) position
        protected Dictionary<string, string> phraseTranslateDict;   //keyword-nameTag
        protected Stack<RectTransform> unUseCellRTStack;            //empty Cell stack
        protected List<KeyValuePair<int, RectTransform>> cellRTListForSort;     //Cell list for Sbling sorting
        protected List<string> voiceKeyWD;      // All the keywords for the cells

        // indexes for menu display
        protected List<int> oldIndexes;
        protected List<int> newIndexes;
        protected List<int> appearIndexes;
        protected List<int> disAppearIndexes;

        // Input devices and some bool flags
        protected Keyboard m_keyboard = Keyboard.current;
        protected Mouse m_mouse = Mouse.current;
        protected bool _menuVisibility = true;
        protected bool _updateFlag = false;
        protected bool _Pup = false;

        // Dictionary for translating numbers into English words.
        protected static Dictionary<char, string> _numToEnglish = new()
        {
            {'0', " zero"},{'1', " one"},{'2', " two"},{'3', " three"},{'4', " four"},
            {'5', " five"},{'6', " six"},{'7', " seven"},{'8', " eight"},{'9', " nine"}
        };

        // Awake is called when the object is created or activated, which is prior to Start()
        protected virtual void Awake()
        {
            // Initialize the variables
            cellRTDict = new Dictionary<int, RectTransform>();
            cellTXTDict = new Dictionary<int, Tritext>();
            cellTransDict = new Dictionary<string, Vector3>();
            ranHumanDict = new Dictionary<int, Vector3>();
            phraseTranslateDict = new Dictionary<string, string>();
            unUseCellRTStack = new Stack<RectTransform>();
            cellRTListForSort = new List<KeyValuePair<int, RectTransform>>();
            oldIndexes = new List<int>();
            newIndexes = new List<int>();
            appearIndexes = new List<int>();
            disAppearIndexes = new List<int>();
            voiceKeyWD = new List<string>();

            // Find related components
            scrollRect = GetComponent<ScrollRect>();
            contentRT = scrollRect.content;
            viewportRT = scrollRect.viewport;
            _inspector = GameObject.Find("PlayerCapsule");
            _groundmeshHG = GameObject.Find("Environment/Ground_Mesh").GetComponent<HumanGenerator>();
            _detailpanel = GameObject.Find("Canvas/DetailPanel");
            _voiceRecogIcon = GameObject.Find("Canvas/VoiceRecogIcon").GetComponent<CanvasGroup>();
            _messagebar = transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>();

            // Force set Content's anchor & pivot
            contentRT.anchorMin = new Vector2(contentRT.anchorMin.x, 1);
            contentRT.anchorMax = new Vector2(contentRT.anchorMax.x, 1);
            contentRT.pivot = new Vector2(contentRT.pivot.x, 1);

            // Calculate the initial offset value
            pivotOffsetY = (1 - CellRerf.pivot.y) * CellRerf.rect.height;

            // Register scrolling event
            scrollRect.onValueChanged.AddListener(OnScrollValueChanged);

            // Hide detail panel and voice recognition icon.
            _detailpanel.GetComponent<CanvasGroup>().alpha = 0f;
            _voiceRecogIcon.alpha = 0f;

        }

        protected virtual void Start()
        {
            if (cellCount < 0)
            {
                return;
            }
            // Initialization for the menu
            FixPadding();
            FixspacingY();
            FixViewportOffset();

            // Request the initial data from the server
            GetInfoList();

        }

        /// <summary>
        /// The voice recognizer handler.
        /// </summary>
        /// <param name="args"> The args of the recognized event </param>
        private void PhraseRecognizer_OnPhraseRecognized(PhraseRecognizedEventArgs args)
        {
            Debug.Log("Recogized keyword: " + args.text);
            int _index;
            string _name;

            // Use the recognized keyword text to find the name in the keyword-nameTag dictionary.
            if (englishPhrase)
            {
                string _tag = phraseTranslateDict[args.text];
                _index = voiceKeyWD.BinarySearch(_tag);
            }
            else
            {
                _index = voiceKeyWD.BinarySearch(args.text);
            }
            // Replace the '_'in the original name with space
            _name = cellTXTDict[_index].name.Replace('_', ' ');
            //Debug.Log(_name);
            StringEventListener(_name);
            //throw new NotImplementedException();
        }

        // Called when the scrolling menu is scrolled so the menu refreshing do not need to called every frame
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

        /*
         * What the following functions do is obvious according to the function name.
         */
        protected virtual void FixPadding() { }

        protected virtual void FixspacingY() { }

        protected virtual void FixViewportOffset()
        {
            viewportOffsetTop = spacingY;
            viewportOffsetBottom = spacingY;
        }

        protected virtual void CalcAndSetContentSize()
        {
            // Start from the top, the total height of the content panel is derived from this equation
            contentHeight = paddingTop + CellRerf.rect.height * cellCount + spacingY * (cellCount - 1) + paddingBottom;
            contentRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, contentHeight);
        }

        // Calculate the cells to be displayed and those to be hidden.
        protected virtual void CalcIndexes()
        {
            // Start from the top border the viewport and look from top to bottom.
            // The displacement of the top border of the content referring to the top border of the viewport (offset included)
            float outHeightFromTop = 0 + (contentRT.anchoredPosition.y + viewportOffsetTop);

            // Bottom border displacement, simiar to the top border
            float outHeightFromBottom = 0 + contentRT.anchoredPosition.y - contentHeight + (viewportRT.rect.height + viewportOffsetBottom);
            //Debug.Log("TOP:" + outHeightFromTop);
            //Debug.Log("Bottom:" + outHeightFromBottom);

            // Calculate the numbers of cells which COMPLETELY exceed the top&bottom border of the viewport
            int outCountFromTop = 0;
            int outCountFromBottom = 0;
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

            // Cells between cell start and cell end will be displayed 
            int startIndex = (outCountFromTop);
            int endIndex = (cellCount - 1 - outCountFromBottom);

            //Debug.Log("startIndex, endIndex: " + startIndex + ", " + endIndex);

            for (int index = startIndex; index <= endIndex; index++)
            {
                newIndexes.Add(index);
            }

            ////Index debugging
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

            // Find out all the cells to appear
            appearIndexes.Clear();
            foreach (int index in newIndexes)
            {
                if (oldIndexes.IndexOf(index) < 0)
                {
                    //Debug.Log("³öÏÖ£º" + index);
                    appearIndexes.Add(index);
                }
            }

            // Find out all the cells to disappear.(Appeared in previous frame)
            disAppearIndexes.Clear();
            foreach (int index in oldIndexes)
            {
                if (newIndexes.IndexOf(index) < 0)
                {
                    //Debug.Log("ÏûÊ§£º" + index);
                    disAppearIndexes.Add(index);
                }
            }

            //save current indexes in oldIndexes
            List<int> temp;
            temp = oldIndexes;
            oldIndexes = newIndexes;
            newIndexes = temp;
            newIndexes.Clear();
        }

        // Disable old cells on push them in the stack.
        protected virtual void DisAppearCells()
        {
            foreach (int index in disAppearIndexes)
            {
                RectTransform cellRT = cellRTDict[index];
                cellRTDict.Remove(index);
                cellRT.gameObject.SetActive(false);
                unUseCellRTStack.Push(cellRT);
                //Debug.Log("iterate:" + index)

            }
        }

        // Make new cells appear.
        protected virtual void AppearCells()
        {
            foreach (int index in appearIndexes)
            {
                RectTransform cellRT = GetOrCreateCell(index);
                cellRTDict[index] = cellRT;
                //Debug.Log(cellRT.anchoredPosition);
                // Set position
                cellRT.anchoredPosition = new Vector2(0, CalcCellPosY(index));

                // Add the keyword to the info of each cell
                string _info = "B" + ((index < 10) ? '0' + index.ToString() : index.ToString()) + '-' + cellTXTDict[index].name.Replace('_', ' ') + ':' + cellTXTDict[index].info;
                //cellRT.GetComponent<CellPref>().SetButtonText(cellTXTDict[index].name.Replace('_', ' '));

                // Refresh the displayed content of the cell and change the name tag.
                CellPref _cell = cellRT.GetComponent<CellPref>();
                _cell.SetButtonImage(cellTXTDict[index].imagestr);
                _cell.SetInfo(_info);
                _cell.callname = cellTXTDict[index].name.Replace('_', ' ');
                //cellRT.GetComponent<CellPref>().SetButtonImage(cellTXTDict[index].imagestr);
                //cellRT.GetComponent<CellPref>().SetInfo(_info);
            }
        }

        /// <summary>
        /// Calculate and set the sbling index of the cells
        /// Called when new cell appears
        /// Needed when cells can stack on each other
        /// </summary>
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
                // ascending order
                return x.Key - y.Key;
            });

            foreach (KeyValuePair<int, RectTransform> kvp in cellRTListForSort)
            {
                // Two types of order
                //kvp.Value.SetAsLastSibling();
                kvp.Value.SetAsFirstSibling();
            }
        }

        /// <summary>
        /// Calculate the position of the cell.
        /// </summary>
        /// <param name="index"> The index of the cell in the dictionary </param>
        /// <returns> the offset value to be added to the rect transform position </returns>
        protected virtual float CalcCellPosY(int index)
        {
            //Y = Top padding + pivot offset + total height of all the cells with smaller indexes + total spacing
            float y = paddingTop + pivotOffsetY + CellRerf.rect.height * index + spacingY * index;

            //Debug.Log("index, cellPosY:" + index + "," + -y);
            return -y;  // The cells are arranged from top to bottom, which means each cell has smaller y value than the cell in front of it. Thus, the offset should be negative
        }

        // Get or create cell
        private RectTransform GetOrCreateCell(int index)
        {
            RectTransform cellRT;
            // Reuse unused cell if available
            if (unUseCellRTStack.Count > 0)
            {
                cellRT = unUseCellRTStack.Pop();
                cellRT.gameObject.SetActive(true);
            }
            else
            {
                cellRT = Instantiate(CellRerf.gameObject).GetComponent<RectTransform>();

                cellRT.SetParent(contentRT, false);
                cellRT.anchorMin = new Vector2(cellRT.anchorMin.x, 1);
                cellRT.anchorMax = new Vector2(cellRT.anchorMax.x, 1);
            }

            return cellRT;
        }

        // Get the initial information for the menu
        private void GetInfoList()
        {
            //Debug.Log("Info should be got from server here!");
            _messagebar.SetText("Downloading information from server...");
            string _url = "http://" + serverIP + ':' + serverPort + "/loinfo";  // API address
            //Debug.Log(_url);
            StartCoroutine(GetRequest(_url, 0));
        }

        /// <summary>
        /// Send web request to server. Conduct different operations according to the integer handle
        /// </summary>
        /// <param name="url"> API address </param>
        /// <param name="handle"> Determine which operation will be used to process the data received </param>
        /// <returns></returns>
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
                        switch (handle)
                        {
                            case 0: // handler for "/loinfo" API
                                // Reconstruct the data structure
                                TritextGroup recipt = JsonUtility.FromJson<TritextGroup>(webRequest.downloadHandler.text);
                                int index = 0;
                                cellTXTDict.Clear();
                                cellTransDict.Clear();
                                voiceKeyWD.Clear();
                                // Add the units to the dictionaries
                                foreach (Tritext _tritext in recipt.tritexts)
                                {
                                    string name = _tritext.name.Replace('_', ' ');
                                    Debug.Log(string.Format("name={0},info={1},position={2},imagestr length={3}", name, _tritext.info, _tritext.position, _tritext.imagestr.Length));
                                    cellTXTDict.Add(index, _tritext);
                                    cellTransDict.Add(name, _tritext.position);
                                    // Create and store relevant keyword
                                    string _phrase = "B" + ((index < 10) ? '0' + index.ToString() : index.ToString());
                                    voiceKeyWD.Add(_phrase);
                                    index++;
                                }
                                cellCount = index;
                                CalcAndSetContentSize();
                                CalcIndexes();
                                //DisAppearCells();
                                AppearCells();
                                CalcAndSetCellsSblingIndex();
                                _messagebar.SetText("Welcome to the museum!");

                                // Create keyword recognizer
                                if (m_phraseRecognizer == null)
                                {
                                    Debug.Log("initiating keyword recognizer...");
                                    if (englishPhrase)
                                    {
                                        phraseTranslateDict.Clear();
                                        List<string> _vkeys = new();
                                        foreach (string _vkey in voiceKeyWD)
                                        {
                                            // Translate numbers to English words
                                            string _word = "B" + _numToEnglish[_vkey[1]] + _numToEnglish[_vkey[2]];
                                            Debug.Log("Translate original keyword to English pronounciation: " + _word + "<=>" + _vkey);
                                            _vkeys.Add(_word);
                                            phraseTranslateDict.Add(_word, _vkey);
                                        }
                                        
                                        m_phraseRecognizer = new KeywordRecognizer(_vkeys.ToArray(), confidenceLevel);
                                    }
                                    else
                                    {
                                        m_phraseRecognizer = new KeywordRecognizer(voiceKeyWD.ToArray(), confidenceLevel);
                                    }
                                    // Add the OnRecogized function to the recognizer
                                    m_phraseRecognizer.OnPhraseRecognized += PhraseRecognizer_OnPhraseRecognized;
                                    Debug.Log("initiate complete.");
                                }
                                break;
                            case 1: // handler for "/random_people" API
                                // Reconstruct the data and add units to dictionaries
                                int index1 = 0;
                                PeopleGroup rec_p = JsonUtility.FromJson<PeopleGroup>(webRequest.downloadHandler.text);
                                foreach (Positions _pos in rec_p.positions)
                                {
                                    //Debug.Log(string.Format("position={0}", _pos.transpose));
                                    ranHumanDict.Add(index1, _pos.transpose);
                                    index1++;
                                }
                                // Indicate that there are some models need to be generated
                                humancount = index1 - 1;
                                _Pup = true;
                                break;
                            case 2: // handler for "/details" API with dtype == 0
                                // Delete the \" & \" of the string and display it in the detail panel
                                _detailpanel.GetComponentInChildren<TextMeshProUGUI>().text = webRequest.downloadHandler.text[1..^1];
                                _detailpanel.GetComponent<CanvasGroup>().alpha = 1.0f;
                                break;
                            default:
                                Debug.Log("Error: Web request handle method not found.");
                                break;
                        }
                        break;
                }

            }
        }

        /// <summary>
        /// Handle button event. Use the received name tag to search for relevant position.
        /// Then send the position to the navigation function.
        /// </summary>
        /// <param name="input"> The name tag of the menu cell. </param>
        public void StringEventListener(string input)
        {
            Debug.Log("MainPage receive:" + input);
            if (input.Equals("crowdctrl") && !_updateFlag)
            {
                // Call the "/random_people" service.
                _updateFlag = true;     // Prevent too frequent request
                string _url = "http://" + serverIP + ':' + serverPort + "/random_people";
                StartCoroutine(GetRequest(_url, 1));
                _messagebar.SetText("Crowd Simulation Activated!");
            }
            else
            {
                // Get the position in nameTag-position dictionary and set the result as the destination
                _messagebar.SetText("Navigating to " + input + '.');
                _inspector.GetComponent<PathDisplay>().destination = cellTransDict[input];
                //Debug.Log(input);
            }
        }

        /// <summary>
        /// OnTriggerEnter handeler. Request the details with the name tag given by sensor
        /// </summary>
        /// <param name="triggerName"> The name tag of the sensor </param>
        public void TriggerEnter(string triggerName)
        {
            string _url = "http://" + serverIP + ':' + serverPort + "/details?name=" + triggerName + "&dtype=";
            StartCoroutine(GetRequest(_url + '0', 2));  // Get the text (dtype=0) first.
            //_detailpanel.GetComponent<CanvasGroup>().alpha = 1.0f;
            StartCoroutine(PlayWebAudio(_url + '1'));   // Get and play the mp3 audio.
        }

        /// <summary>
        /// Get mp3 from the server and play in the audio player component in the player capsule
        /// </summary>
        /// <param name="url"> Address of the audio file </param>
        /// <returns></returns>
        IEnumerator PlayWebAudio(string url)
        {
            // The web request type is different from the text request. Make sure the audio type is correct.
            using (UnityWebRequest webRequest = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG))
            {
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
                        // Send the audio clip to the player component and play it.
                        AudioClip clip = DownloadHandlerAudioClip.GetContent(webRequest);
                        _inspector.GetComponent<AudioSource>().clip = clip;
                        _inspector.GetComponent<AudioSource>().Play();
                        break;

                }
            }
        }

        /// <summary>
        /// Close the detail panel and stop the playing audio immediately when user exit the trigger area.
        /// </summary>
        public void TriggerExit()
        {
            _detailpanel.GetComponent<CanvasGroup>().alpha = 0f;
            if (_inspector.GetComponent<AudioSource>().isPlaying) _inspector.GetComponent<AudioSource>().Stop();
        }

        protected virtual void Update()
        {
            // Change visibilty with M key
            if (m_keyboard.mKey.wasPressedThisFrame)
            {
                _menuVisibility = !_menuVisibility;
                //Debug.Log("M Pressed");
                GetComponent<CanvasGroup>().alpha = _menuVisibility ? 1.0f : 0f;
                Cursor.lockState = _menuVisibility ? CursorLockMode.None : CursorLockMode.Locked;
            }
            // Activate the recognizer when T key is pressed.
            if (m_keyboard.tKey.wasPressedThisFrame)
            {
                _voiceRecogIcon.alpha = 1.0f;   // Display the icon so that the user knows the recoginzer is working.
                //CreateVoiceRecognizer(voiceKeyWD, confidenceLevel);
                m_phraseRecognizer.Start();
            }
            // Stop the recognizer when T key is released.
            if (m_keyboard.tKey.wasReleasedThisFrame)
            {
                _voiceRecogIcon.alpha = 0f;     // Hide the icon
                m_phraseRecognizer.Stop();
            }
            // Generate human clone once per frame
            if (_Pup)
            {
                if (gndex < humancount)
                {
                    _groundmeshHG.position = ranHumanDict[gndex];
                    //_groundmeshHG.position = new Vector3(7.40f, 0.56f, 55.94f);
                    _groundmeshHG.SingleGenerate(); // Call the function in HumanGenerator.cs
                    gndex++;
                }
                else
                {
                    // Reset the variables when generation is completed.
                    gndex = 0;
                    _messagebar.SetText("Crowd simulation complete.");
                    ranHumanDict.Clear();
                    _Pup = false;
                    _updateFlag = false;
                }

            }
        }

        // Make sure the keyword recognizer is disposed to free the resources when the object is destroyed.
        private void OnDestroy()
        {
            m_phraseRecognizer?.Dispose();
        }
    }
}
