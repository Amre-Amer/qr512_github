using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ManagerQR512 : MonoBehaviour
{
    int maxCount = 49;
    int numCategories = 512;
    int cntFrames;
    int iMinSumDelta;
    int cntFps;
    int iInputField;
    int nMatches;
    int numVisible;
    int x1Green;
    int y1Green;
    int x2Green;
    int y2Green;
    const int res = 28;

    public int intervalFrameWebcam;
    //
    float iFHighlight;
    const float threshold = .5f;
    float smoothCategoryButtons = .5f;
    List<float> countsClassify = new List<float>();
    //
    const string urlInfo = "https://drive.google.com/open?id=1bHUt_urV429QneWALI7DS6szf-hcXh9V";
    string txtButtonsDigit;
    //
    Text textInfo;
    Text textFps;
    Text textSetUrl;
    Text textDigit;
    List<Text> textsSetUrl = new List<Text>();
    //
    Button buttonInfo;
    Button buttonImport;
    Button buttonUrl;
    Button buttonList;
    Button buttonSetUrl;
    Button buttonDigit;
    Button buttonDigitHighlight;
    Button buttonView;
    Button buttonIntro;
    Button buttonIntroOK;
    Button buttonIntroView;
    Button buttonIntroPrint;
    List<Button> buttonsSetUrl = new List<Button>();
    List<Button> buttonsView = new List<Button>();
    List<Button> buttonsDigit = new List<Button>();
    //
    public bool ynInfo;
    bool ynWebcam;
    bool ynList;
    bool ynCrop;
    //
    Image imageWebcam;
    Image imageWebcamFrame;
    Image imageCountsWebcam;
    Image imageCategoryButtons;
    Image imageCategoryButtonsHighlight;
    Image imageSetUrlHighlight;
    Image imageZoom;
    Image imageResult;
    Image imageBackSetUrl;
    List<Image> imagesBackSetUrl = new List<Image>();
    //
    Texture2D texWebcam;
    Texture2D texCountsWebcam;
    Texture2D texCategoryButtons;
    Texture2D texZoom;
    Texture2D texResult;
    //
    List<Vector3> targetCategoryButtons = new List<Vector3>();
    List<Vector3> posCategoryButtons = new List<Vector3>();
    //
    WebCamTexture webcamTexture;
    //
    GameObject goCanvasMain;
    GameObject goCanvasUrls;
    GameObject goCanvasIntro;
    GameObject goContent;
    //
    InputField inputField;
    InputField inputFieldImport;
    //
    Qr512 qr512;
    //
    Color colorTexClear = new Color(0, 0, 0, .125f);

    // Start is called before the first frame update
    void Awake()
    {
        Application.targetFrameRate = 60;
        Screen.orientation = ScreenOrientation.Portrait;
        intervalFrameWebcam = 2;
        qr512 = GetComponent<Qr512>();
        //
        textInfo = GameObject.Find("TextInfo").GetComponent<Text>();
        imageWebcam = GameObject.Find("ImageWebcam").GetComponent<Image>();
        imageWebcamFrame = GameObject.Find("ImageWebcamFrame").GetComponent<Image>();
        imageZoom = GameObject.Find("ImageZoom").GetComponent<Image>();
        imageResult = GameObject.Find("ImageResult").GetComponent<Image>();
        textFps = GameObject.Find("TextFps").GetComponent<Text>();
        imageCountsWebcam = GameObject.Find("ImageCountsWebcam").GetComponent<Image>();
        goCanvasMain = GameObject.Find("canvasMain");
        goCanvasUrls = GameObject.Find("canvasUrls");
        goCanvasIntro = GameObject.Find("canvasIntro");
        buttonSetUrl = GameObject.Find("ButtonSetUrl").GetComponent<Button>();
        imageBackSetUrl = GameObject.Find("ImageBackSetUrl").GetComponent<Image>();
        buttonView = GameObject.Find("ButtonView").GetComponent<Button>();
        textSetUrl = GameObject.Find("TextSetUrl").GetComponent<Text>();
        inputField = GameObject.Find("InputField").GetComponent<InputField>();
        goContent = GameObject.Find("Content");
        imageCategoryButtons = GameObject.Find("ImageCategoryButtons").GetComponent<Image>();
        imageCategoryButtonsHighlight = GameObject.Find("ImageCategoryButtonsHighlight").GetComponent<Image>();
        buttonDigit = GameObject.Find("ButtonDigit").GetComponent<Button>();
        inputFieldImport = GameObject.Find("InputFieldImport").GetComponent<InputField>();
        textDigit = GameObject.Find("TextDigit").GetComponent<Text>();
        imageSetUrlHighlight = GameObject.Find("ImageSetUrlHighlight").GetComponent<Image>();
        buttonIntroOK = GameObject.Find("ButtonIntroOK").GetComponent<Button>();
        buttonIntroOK.onClick.AddListener(TappedIntroOK);
        buttonIntroView = GameObject.Find("ButtonIntroView").GetComponent<Button>();
        buttonIntroView.onClick.AddListener(TappedIntroView);
        buttonIntroPrint = GameObject.Find("ButtonIntroPrint").GetComponent<Button>();
        buttonIntroPrint.onClick.AddListener(TappedIntroPrint);
    }

    void Start() {
        SetInfo("QR512" + " v" + Application.version);
        //
        StartWebcam();
        //
        LoadButtons();
        //
        qr512.LoadCSV();
        inputFieldImport.text = qr512.urlCSV;
        //
        if (Application.isEditor)
        {
            //qr512.GenerateQR512();
            //qr512.AddContactSheet();
            //qr512.AnimateContactSheet();
        }
        float h = imageCategoryButtons.GetComponent<RectTransform>().sizeDelta.y;
        numVisible = (int)(Screen.width / h);
        if (numVisible % 2 == 0) numVisible++;
        //
        AdjustCanvas();
        AdjustButtonDigits();
        //
        ColorButtons();
        ShowCategoryButtons();
        //
        TappedLive();
        //
        InvokeRepeating("ShowFps", 1, 1);
    }

    // Update is called once per frame
    void Update()
    {
        UpdateSmooth();
        cntFrames++;
        cntFps++;
    }

    void AdjustButtonDigits()
    {
        buttonsDigit.Clear();
        buttonDigit.gameObject.SetActive(true);
        float w = buttonDigit.GetComponent<RectTransform>().sizeDelta.x;
        float width = goContent.GetComponent<RectTransform>().sizeDelta.x;
        Vector2 posContent = goContent.GetComponent<RectTransform>().position;
        for (int n = 0; n < 11; n++)
        {
            Button button = Instantiate(buttonDigit);
            button.transform.SetParent(buttonDigit.transform.parent);
            Vector3 posButton = buttonDigit.GetComponent<RectTransform>().position;
            posButton.x = posContent.x - width/2 + w * n + w/2;
            button.GetComponent<RectTransform>().position = posButton;
            string txt = "";
            if(n < 10) {
                txt = n.ToString();
            }
            else
            {
                txt = "<";
            }
            button.GetComponentInChildren<Text>().text = txt;
            button.onClick.AddListener(delegate { TappedButtonDigit(button); });
            buttonsDigit.Add(button);
        }
        buttonDigit.gameObject.SetActive(false);
    }

    void TappedIntroPrint()
    {
        Application.OpenURL("https://sites.google.com/amre-amer.com/resume/home/qr512sheet");
    }

    void TappedIntroView()
    {
        Application.OpenURL("https://sites.google.com/amre-amer.com/resume/home/qr512sheet");
    }

    void TappedButtonDigit(Button button)
    {
        inputField.gameObject.SetActive(false);
        int nButton = buttonsDigit.IndexOf(button);
        Debug.Log("TappedButtonDigit " + nButton + "\n");
        string txt = nButton.ToString();
        if(nButton < 10)
        {
            txtButtonsDigit += txt;
        } else
        {
            if (txtButtonsDigit.Length > 0)
            {
                txtButtonsDigit = txtButtonsDigit.Substring(0, txtButtonsDigit.Length - 1);
            }
        }
        HighlightButtonDigit(button);
        textDigit.text = txtButtonsDigit;
        ResetContentToTop();
        ShowMatches();
    }

    void HighlightButtonDigit(Button button)
    {
        buttonDigitHighlight = button;
        buttonDigitHighlight.GetComponent<Image>().color = Color.green;
        Invoke("UnHighlightButtonDigit", .1f);
    }

    void UnHighlightButtonDigit()
    {
        buttonDigitHighlight.GetComponent<Image>().color = Color.white;
    }

    void TappedButtonSetUrl(Button button)
    {
        Debug.Log("ButtonSetUrl " + button.GetComponentInChildren<Text>().text + "\n");
        iInputField = buttonsSetUrl.IndexOf(button);
        inputField.gameObject.SetActive(true);
        inputField.ActivateInputField();
        int n = GetLinksCSVnForiInput();
        inputField.text = qr512.linksCSV[n];
        //
        imageSetUrlHighlight.gameObject.SetActive(true);
        HighlightButtonSetUrl();
    }

    void TappedButtonView(Button button)
    {
        Debug.Log("ButtonView " + button.GetComponentInChildren<Text>().text + "\n");
        iInputField = buttonsSetUrl.IndexOf(button);
        inputField.gameObject.SetActive(true);
        inputField.ActivateInputField();
        int n = GetLinksCSVnForiInput();
        string url = qr512.linksCSV[n];
        Application.OpenURL(url);
        //
        imageSetUrlHighlight.gameObject.SetActive(true);
        HighlightButtonSetUrl();

    }

    void HighlightButtonSetUrl()
    {
        Vector2 posButton = buttonsSetUrl[iInputField].GetComponent<RectTransform>().position;
        Vector2 posH = imageSetUrlHighlight.GetComponent<RectTransform>().position;
        posH.y = posButton.y;
        imageSetUrlHighlight.GetComponent<RectTransform>().position = posH;
    }

    public void InputDone()
    {
        Debug.Log("InputDone\n");
        string txtInput = inputField.text;
        textsSetUrl[iInputField].text = txtInput;
        //
        int n = GetLinksCSVnForiInput();
        qr512.linksCSV[n] = txtInput;
        //
        imageSetUrlHighlight.gameObject.SetActive(false);
        inputField.gameObject.SetActive(false);
    }

    int GetLinksCSVnForiInput()
    {
        Button button = buttonsSetUrl[iInputField];
        int num;
        string txt = button.GetComponentInChildren<Text>().text;
        int.TryParse(txt, out num);
        return num;
    }

    public void InputDoneImport()
    {
        string url = inputFieldImport.text;
        url = ConvertGoogleDriveUrl(url);
        qr512.urlCSV = url; 
        qr512.SaveUrlCSV();
        Debug.Log("InputDoneImport " + inputField.text + "\n");
        Debug.Log(">InputDoneImport " + url + "\n");
        inputField.text = url;
    }

    string ConvertGoogleDriveUrl(string url)
    {
        // from https://drive.google.com/file/d/1QBXR0cTZG3lFiFwfPv63DEiXw7GxuGYk/view?usp=sharing
        // to   https://drive.google.com/uc?export=download&id=1Ey2ecHcdybMtA4KTEuX_a_d5VZ_RR-pg
        string txtSearch = "https://drive.google.com/file/d/";
        if(url.Contains(txtSearch))
        {
            string txt = url.Substring(txtSearch.Length);
            txtSearch = "/view?usp=sharing";
            int len = txt.Length - txtSearch.Length;
            txt = txt.Substring(0,len);
            Debug.Log("url " + txt + "\n");
            url = "https://drive.google.com/uc?export=download&id=" + txt; 
        }
        return url;
    }

    void ClearButtonsSetUrl()
    {
        for (int n = 0; n < buttonsSetUrl.Count; n++)
        {
            Destroy(buttonsSetUrl[n].gameObject);
            Destroy(textsSetUrl[n].gameObject);
            Destroy(buttonsView[n].gameObject);
            Destroy(imagesBackSetUrl[n].gameObject);
        }
        buttonsSetUrl.Clear();
        textsSetUrl.Clear();
        buttonsView.Clear();
        imagesBackSetUrl.Clear();
    }

    void ShowMatches()
    {
        ClearButtonsSetUrl();
        nMatches = 0;
        for (int i = 0; i < qr512.linksCSV.Count; i++)
        {
            string txt = i.ToString();
            if (txt.Length >= txtButtonsDigit.Length && txt.Substring(0, txtButtonsDigit.Length) == txtButtonsDigit) {
                AddButtonSetUrl(i, nMatches);
                nMatches++;
                if (nMatches > 15)
                {
                    break;
                }
            }
        }
        if(buttonsSetUrl.Count == 0)
        {
            textDigit.color = Color.red;
        } else
        {
            textDigit.color = Color.black;
        }
    }

    void AddButtonSetUrl(int i, int n)
    {
        buttonSetUrl.gameObject.SetActive(true);
        textSetUrl.gameObject.SetActive(true);
        buttonView.gameObject.SetActive(true);
        imageBackSetUrl.gameObject.SetActive(true);
        //
        float dy = buttonSetUrl.GetComponent<RectTransform>().sizeDelta.y * 1.1f;
        //
        Vector3 posText = textSetUrl.GetComponent<RectTransform>().position;
        Vector3 posButton = buttonSetUrl.GetComponent<RectTransform>().position;
        Vector3 posV = buttonView.GetComponent<RectTransform>().position;
        Vector3 posB = imageBackSetUrl.GetComponent<RectTransform>().position;
        posText.y -= n * dy;
        posButton.y -= n * dy;
        posV.y -= n * dy;
        posB.y -= n * dy;
        //
        Image imageB = Instantiate(imageBackSetUrl);
        imageB.transform.SetParent(imageBackSetUrl.transform.parent);
        imageB.GetComponent<RectTransform>().position = posB;
        imagesBackSetUrl.Add(imageB);
        //
        Text text = Instantiate(textSetUrl);
        text.transform.SetParent(textSetUrl.transform.parent);
        text.GetComponent<RectTransform>().position = posText;
        string txt = qr512.linksCSV[i];
        text.text = txt;
        if(txt == qr512.urlDefault)
        {
            text.color = Color.red;
        }
        textsSetUrl.Add(text);
        //
        Button button = Instantiate(buttonSetUrl);
        button.transform.SetParent(buttonSetUrl.transform.parent);
        button.GetComponent<RectTransform>().position = posButton;
        Texture2D tex = qr512.texsAll[i];
        button.transform.Find("Image").GetComponent<Image>().sprite = ConvertToSprite(tex);
        button.GetComponentInChildren<Text>().text = i.ToString();
        button.onClick.AddListener(delegate { TappedButtonSetUrl(button); });
        buttonsSetUrl.Add(button);
        //
        Button buttonV = Instantiate(buttonView);
        buttonV.transform.SetParent(buttonView.transform.parent);
        buttonV.GetComponent<RectTransform>().position = posV;
        buttonV.onClick.AddListener(delegate { TappedButtonView(button); });
        buttonsView.Add(buttonV);
        //
        buttonSetUrl.gameObject.SetActive(false);
        textSetUrl.gameObject.SetActive(false);
        buttonView.gameObject.SetActive(false);
        imageBackSetUrl.gameObject.SetActive(false);
    }

    void StartWebcam()
    {
        webcamTexture = new WebCamTexture();
        webcamTexture.Play();
    }

    void LoadButtons()
    {
        buttonInfo = GameObject.Find("ButtonInfo").GetComponent<Button>();
        buttonInfo.onClick.AddListener(TappedInfo);
        //
        buttonUrl = GameObject.Find("ButtonUrl").GetComponent<Button>();
        buttonUrl.onClick.AddListener(TappedUrl);
        //
        buttonList = GameObject.Find("ButtonList").GetComponent<Button>();
        buttonList.onClick.AddListener(TappedList);
        //
        buttonImport = GameObject.Find("ButtonImport").GetComponent<Button>();
        buttonImport.onClick.AddListener(TappedImport);
        //
        buttonIntro = GameObject.Find("ButtonIntro").GetComponent<Button>();
        buttonIntro.onClick.AddListener(TappedIntro);
    }

    private void OnLowMemory()
    {
        buttonInfo.GetComponent<Image>().color = Color.red;
        Resources.UnloadUnusedAssets();
    }

    Texture2D SquareTex(Texture2D tex)
    {
        int bx = (tex.width - tex.height) / 2;
        Texture2D texOut = new Texture2D(tex.height, tex.height);
        for(int x = bx; x < tex.width - bx; x++)
        {
            for(int y = 0; y < tex.height; y++)
            {
                texOut.SetPixel(x - bx, y, tex.GetPixel(x,y));
            }
        }
        texOut.Apply();
        return texOut;
    }

    void AdjustWebcamTrainLive()
    {
        EnableDisableWebcamFrame(true);
        EnableDisableWebcam(true);
    }

    void ShowFps()
    {
        textFps.text = cntFps + " fps";
        cntFps = 0;
    }

    void ShowCountsWebcam()
    {
        Destroy(texCountsWebcam);
        countsClassify = GetCountsForTex(texWebcam);
        int s = 3;
        int widthTex = Screen.width;
        int heightTex = (int)imageCountsWebcam.GetComponent<RectTransform>().sizeDelta.y;
        texCountsWebcam = GetTexFromCounts(countsClassify, widthTex/s, heightTex/s, Color.white);
        texCountsWebcam.filterMode = FilterMode.Point;
        imageCountsWebcam.GetComponent<Image>().sprite = ConvertToSprite(texCountsWebcam);
    }

    Texture2D GetTexFromCounts(List<float>counts, int widthTex, int heightTex, Color color)
    {
        float dx = (float)widthTex / (maxCount + 1);
        int w = (int)dx - 1;
        float xStart = (widthTex - maxCount * dx) / 2;
        float max = GetMaxOfCounts(counts);
        float height = heightTex;
        float width = dx * maxCount;
        Texture2D tex = CreateTexWithColor((int)width, (int)height, colorTexClear);
        for (int c = 0; c < maxCount; c++)
        {
            float f = counts[c] / max;
            float h = f * height;
            //
            float x1 = xStart + dx * c;
            float y1 = (height - h) / 2;
            float x2 = x1 + w;
            float y2 = y1 + h;
            qr512.PaintRect(tex, (int)x1, (int)y1, (int)x2, (int)y2, color);
        }
        tex.Apply(true);
        return tex;
    }

    Texture2D CreateTexWithColor(int w, int h, Color color)
    {
        Texture2D tex = new Texture2D(w, h);
        Color fillColor = color;
        Color[] colors = tex.GetPixels();
        for (var i = 0; i < colors.Length; ++i)
        {
            colors[i] = fillColor;
        }
        tex.SetPixels(colors);
        tex.Apply();
        return tex;
    }

    Texture2D CreateTexBlack(int w, int h)
    {
        Texture2D tex = new Texture2D(w, h);
        Color fillColor = new Color(1, 0, 0, 1);
        Color[] colors = tex.GetPixels();
        for (var i = 0; i < colors.Length; ++i)
        {
            colors[i] = fillColor;
        }
        tex.SetPixels(colors);
        tex.Apply();
        return tex;
    }

    void ShowCategoryButtons()
    {
        float h = imageCategoryButtons.GetComponent<RectTransform>().sizeDelta.y;
        float wAll = h * numVisible;
        Vector2 size = new Vector2(wAll, h);
        imageCategoryButtons.GetComponent<RectTransform>().sizeDelta = size;
        Debug.Log("numVisible " + numVisible + "\n");
        int texWidth = res * numVisible;
        int texHeight = res;
        imageCategoryButtons.sprite = null; // amre
        texCategoryButtons = CreateTexWithColor(texWidth, texHeight, colorTexClear);
        imageCategoryButtons.sprite = ConvertToSprite(texCategoryButtons);
        texCategoryButtons.filterMode = FilterMode.Point;
        //
        posCategoryButtons.Clear();
        targetCategoryButtons.Clear();
    }

    void PaintQR512(Texture2D texOut, int i, int xStart, int yStart)
    {
        Texture2D tex = qr512.texsAll[i];
        for (int x = 0; x < tex.width; x++)
        {
            for (int y = 0; y < tex.height; y++)
            {
                int xOut = (int)(xStart + x);
                int yOut = (int)(yStart + y);
                texOut.SetPixel(xOut, yOut, tex.GetPixel(x, y));
            }
        }
        texOut.Apply();
    }

    void HighlightCategoryButton()
    {
        imageCategoryButtonsHighlight.GetComponentInChildren<Text>().text = iMinSumDelta.ToString();
    }

    void UpdateSmooth()
    {
        iFHighlight = smoothCategoryButtons * (float)iMinSumDelta + (1f - smoothCategoryButtons) * iFHighlight;
        HighlightCategoryButtonSmooth(iFHighlight);
    }

    void HighlightCategoryButtonSmooth(float iF)
    {
        int half = numVisible / 2;
        int xStart0 = (int)((iF - (int)iF) * res);
        int xFin = xStart0 + numVisible * res - 1;
        int i = (int)iF;
        for (int n = i - half; n <= i + half; n++)
        {
            if (n < 0)
            {
                qr512.PaintRect(texCategoryButtons, xStart0, 0, xStart0 + res-1, res-1, Color.clear);
                texCategoryButtons.Apply();
            }
            else
            {
                if (n >= numCategories)
                {
                    qr512.PaintRect(texCategoryButtons, xStart0, 0, xFin, res-1, Color.clear);
                    texCategoryButtons.Apply();
                }
                else
                {
                    PaintQR512(texCategoryButtons, n, xStart0, 0);
                }
            }
            xStart0 += res;
        }
    }

    void HighlightCategoryButtonFor(int i)
    {
        int half = numVisible / 2;
        int xStart0 = 0;
        for (int n = i - half; n <= i + half; n++)
        {
            if (n >= 0 && n < numCategories)
            {
                PaintQR512(texCategoryButtons, n, xStart0, 0);
                xStart0 += res;
            }
        }
    }

    float GetMaxOfCounts(List<float>counts)
    {
        float max = -1;
        foreach(float v in counts)
        {
            if (max < 0 || v > max)
            {
                max = v;
            }
        }
        return max;
    }

    public Sprite ConvertToSprite(Texture2D texture)
    {
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
    }

    void EnableDisableWebcam(bool yn)
    {
        ynWebcam = yn;
        imageWebcam.gameObject.SetActive(yn);
        imageCountsWebcam.gameObject.SetActive(yn);
        if(yn)
        {
            webcamTexture.Play();
        } else
        {
            webcamTexture.Pause();
        }
    }

    void EnableDisableWebcamFrame(bool yn)
    {
        imageWebcamFrame.gameObject.SetActive(yn);
    }

    public Texture2D RotRightTex(Texture2D tex)
    {
        Texture2D flipped = new Texture2D(tex.width, tex.height);
        for (int i = 0; i < tex.width; i++)
        {
            for (int j = 0; j < tex.height; j++)
            {
                flipped.SetPixel(i, j, tex.GetPixel(tex.width - j - 1, i));
            }
        }
        flipped.Apply(true);
        return flipped;
    }

    void TappedLive()
    {
        AdjustWebcamTrainLive();
        ColorButtons();
        imageWebcamFrame.color = Color.cyan;
    }

    void TappedUrl()
    {
        Debug.Log("TappedUrl\n");
        Application.OpenURL(buttonUrl.GetComponentInChildren<Text>().text);
        ColorButtons();
    }

    void ResetContentToTop()
    {
        Vector2 pos = goContent.GetComponent<RectTransform>().position;
        pos.y = 0;
        goContent.GetComponent<RectTransform>().position = pos;
    }

    void TappedIntro()
    {
        Debug.Log("TappedIntro\n");
        goCanvasIntro.SetActive(true);
    }

    void TappedIntroOK()
    {
        Debug.Log("TappedIntroOK\n");
        goCanvasIntro.SetActive(false);
    }

    void TappedList()
    {
        Debug.Log("TappedList\n");
        ynList = !ynList;
        if (ynList)
        {
            txtButtonsDigit = "";
            textDigit.text = txtButtonsDigit;
            inputField.text = "";
            inputField.gameObject.SetActive(false);
            imageSetUrlHighlight.gameObject.SetActive(false);
            ResetContentToTop();
            ShowMatches();
        }
        if (ynList) ynInfo = false;
        AdjustCanvas();
        ColorButtons();
    }

    void TappedInfo()
    {
        ynInfo = !ynInfo;
        if (ynInfo)
        {
            ynList = false;
        }
        AdjustCanvas();
        ColorButtons();
        Application.OpenURL(urlInfo);
        Debug.Log("TappedInfo\n");
    }

    void TappedImport()
    {
        Debug.Log("TappedImport\n");
        InputDoneImport();
        qr512.LoadLinksCSV();
        TappedList();
    }

    void AdjustCanvas()
    {
        if (!goCanvasUrls) return;
        goCanvasUrls.SetActive(ynList);
        goCanvasMain.SetActive(!ynList);
        EnableDisableWebcam(!ynList);
    }

    void ColorButtons()
    {
        ColorButton(buttonInfo, ynInfo);
        ColorButton(buttonList, ynList);
    }

    void ColorButton(Button button, bool yn)
    {
        Color color = Color.white;
        if(yn)
        {
            color = Color.green;
        }
        button.GetComponent<Image>().color = color;
    }

    public void RefreshWebcam()
    {
        if (ynWebcam)
        {
            texWebcam = new Texture2D(webcamTexture.width, webcamTexture.height, TextureFormat.RGB24, false);
            //
            texWebcam.SetPixels(webcamTexture.GetPixels());
            //
            int s = 4;
            int h = res * s;
            int w = h * webcamTexture.width / webcamTexture.height;
            TextureScale.Bilinear(texWebcam, w, h);
            //
            texWebcam = SquareTex(texWebcam);
            //
            if (!Application.isEditor)
            {
                texWebcam = RotRightTex(texWebcam);
            }
            Texture2D tex = qr512.CopyOfTex(texWebcam);
            imageWebcam.GetComponent<Image>().sprite = ConvertToSprite(tex);
            //
            BorderGreen(tex);
            texZoom = qr512.CopyOfTex(tex);
            texZoom = CropGreen(texZoom);
            TextureScale.Bilinear(texZoom, res, res);
            imageZoom.GetComponent<Image>().sprite = ConvertToSprite(texZoom);
            //
            iMinSumDelta = qr512.Match(texZoom);
            AdjustButtonUrl();
            texResult = qr512.texsAll[iMinSumDelta];
            imageResult.GetComponent<Image>().sprite = ConvertToSprite(texResult);
            //
            TextureScale.Bilinear(texWebcam, res, res);
            //
            ShowCountsWebcam();
            //
            HighlightCategoryButton();
            //
            if (!Application.isEditor)
            {
                Resources.UnloadUnusedAssets();
            }
        }
    }

    void BorderGreen(Texture2D tex)
    {
        x1Green = -1;
        y1Green = -1;
        x2Green = -1;
        y2Green = -1;
        for (int x = 0; x < tex.width; x++)
        {
            for (int y = 0; y < tex.height; y++)
            {
                Color color = tex.GetPixel(x, y);
                if (IsGreen(color))
                {
                    tex.SetPixel(x, y, Color.green);
                }
            }
        }
        tex.Apply();
        int yMid = tex.height / 2;
        int xMid = tex.width / 2;
        for (int x = 0; x < xMid; x++)
        {
            Color color = tex.GetPixel(x, yMid);
            if (IsGreen(color))
            {
                x1Green = x;
            }
        }
        for (int x = tex.width - 1; x >= xMid; x--)
        {
            Color color = tex.GetPixel(x, yMid);
            if (IsGreen(color)) {
                x2Green = x;
            }
        }
        for (int y = 0; y < yMid; y++)
        {
            Color color = tex.GetPixel(xMid, y);
            if (IsGreen(color))
            {
                y1Green = y;
            }
        }
        for (int y = tex.height - 1; y >= yMid; y--)
        {
            Color color = tex.GetPixel(xMid, y);
            if (IsGreen(color))
            {
                y2Green = y;
            }
        }
    }

    bool IsGreen(Color color)
    {
        if (color.g > .75f * (color.r + color.b))
        {
            return true;
        }
        return false;
    }

    Texture2D CropGreen(Texture2D tex) {
        int x1 = x1Green;
        int y1 = y1Green;
        int x2 = x2Green;
        int y2 = y2Green;
        //
        int w = x2 - x1 + 1;
        int h = y2 - y1 + 1;
        //
        ynCrop = true;
        if (w < res || h < res) {
            x1 = 0;
            x2 = tex.width - 1;
            y1 = 0;
            y2 = tex.height - 1;
            w = tex.width;
            h = tex.height;
            ynCrop = false;
        }
        Texture2D texOut = new Texture2D(w, h);
        for (int x = 0; x < tex.width; x++)
        {
            for (int y = 0; y < tex.height; y++)
            {
                Color color = tex.GetPixel(x, y);
                if (x >= x1 && x <= x2 && y >= y1 && y <= y2)
                {
                    texOut.SetPixel(x - x1, y - y1, color);
                }
            }
        }
        texOut.Apply();
        return texOut;
    }

    void AdjustButtonUrl()
    {
        Color color = Color.gray;
        string url = "";
        if (!ynCrop)
        {
            buttonUrl.GetComponent<Image>().color = color;
            buttonUrl.GetComponentInChildren<Text>().text = url;
            return;
        }
        if (iMinSumDelta < qr512.linksCSV.Count)
        {
            url = qr512.linksCSV[iMinSumDelta];
            color = Color.white;
        }
        buttonUrl.GetComponentInChildren<Text>().text = url;
        buttonUrl.GetComponent<Image>().color = color;
    }

    List<float> GetCountsForTex(Texture2D tex)
    {
        List<float> counts = new List<float>();
        for (int c = 0; c < maxCount; c++)
        {
            counts.Add(0);
        }
        GetCountsBlob4(tex, counts, 0, maxCount);
        return counts;
    }

    void SetInfo(string txt)
    {
        textInfo.text = txt;
        Debug.Log(txt + "\n");
    }

    List<float> GetCountsBlob4(Texture2D tex, List<float> counts, int cStart, int cFin)
    {
        int n = 0;
        for (int x = 0; x < tex.width; x += 4)
        {
            for (int y = 0; y < tex.height; y += 4)
            {
                int cnt = 0;
                for (int dx = 0; dx < 4; dx++)
                {
                    for (int dy = 0; dy < 4; dy++)
                    {
                        Color color = tex.GetPixel(x + dx, y + dy);
                        if (color.grayscale > threshold)
                        {
                            cnt++;
                        }
                    }
                }
                int c = cStart + n;
                if (c >= cStart && c < cFin)
                {
                    counts[c] = cnt;
                }
                n++;
            }
        }
        return counts;
    }
}
