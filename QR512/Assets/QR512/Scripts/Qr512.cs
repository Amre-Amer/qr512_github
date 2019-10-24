using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Networking;

public class Qr512 : MonoBehaviour
{
    public Texture2D[] texNums;
    public Texture2D texLogo;
    public List<Texture2D> texsAll = new List<Texture2D>();
    //
    int iAnimate = 0;
    int cntFrames;
    const int res = 28;
    const int maxCount = 49;
    const int numCats = 512;
    List<List<int>> countsAll = new List<List<int>>();
    List<int> sumDeltas = new List<int>();
    //
    const float threshold = .5f;
    //
    const string filenameUrlCSV = "url_csv.txt";
    public List<string> linksCSV = new List<string>();
    public string urlCSV = "https://drive.google.com/uc?export=download&id=1Ey2ecHcdybMtA4KTEuX_a_d5VZ_RR-pg";
    public string urlDefault = "https://sites.google.com/amre-amer.com/resume/home/qr512";
    //
    bool ynAnimate;
    //
    GameObject goCube;
    //

    // Start is called before the first frame update
    void Start()
    {
        goCube = GameObject.Find("Cube");
        goCube.SetActive(true);
        LoadCountsAll();
    }

    void Update()
    {
        if (ynAnimate && cntFrames > 100 && cntFrames % 4 == 0)
        {
            if (iAnimate < 512)
            {
                goCube.SetActive(true);
                AnimateContactSheetOne();
            } else
            {
                ynAnimate = false;
                goCube.SetActive(false);
            }
            iAnimate++;
        }
        cntFrames++;
    }

    public void LoadCSV()
    {
        LoadUrlCSV();
        LoadLinksCSV();
    }

    public void AnimateContactSheet()
    {
        ynAnimate = true;
    }

    void AnimateContactSheetOne()
    {
        Color colorB = (Color.gray + Color.black) / 2;
        int numX = 1;
        int numY = 1; 
        int s = 6;
        //
        int resS = res * s;
        int bb = resS * 2 / 3;
        int w = resS + bb * 2;
        int b = bb / 3;
        int dx = w + b * 2;
        int dy = dx;
        int width = numX * dx;
        int height = numY * dy;
        Texture2D texOut = new Texture2D(width, height);
        PaintRect(texOut, 0, 0, width, height, Color.white);
        //
        int x = 0;
        int y = 0;
        AddContactSheetFor(texOut, iAnimate, x, y, w, dx, dy, b, bb, s, colorB);
        texOut.Apply();
        goCube.GetComponent<Renderer>().material.mainTexture = texOut;
    }

    public void LoadLinksCSV()
    {
        InitRandomUrlsToCSV();
        StartCoroutine(GetText());
    }

    System.Collections.IEnumerator GetText()
    {
        UnityWebRequest www = UnityWebRequest.Get(urlCSV);
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {
            string textFile = www.downloadHandler.text;
            string[] lines = textFile.Split('\n');
            if (lines.Length > 0)
            {
                for (int n = 0; n < lines.Length; n++)
                {
                    string line = lines[n];
                    Debug.Log(n + " CSV " + line + "\n");
                    string[] stuff = line.Split(',');
                    if (stuff.Length >= 2)
                    {
                        int num;
                        int.TryParse(stuff[0], out num);
                        if (num >= 0 && stuff[1].ToLower().Contains("http"))
                        {
                            Debug.Log("----------------> try " + num + " " + stuff[1] + "\n");
                            linksCSV[num] = stuff[1];
                        }
                    }
                }
            }
        }
        ShowUrls();
    }

    void InitRandomUrlsToCSV()
    {
        linksCSV.Clear();
        for (int n = 0; n < numCats; n++)
        {
            string url = urlDefault;
            linksCSV.Add(url);
        }
    }

    void ShowUrls()
    {
        string txt = "-----> LinksCSV: \n";
        for (int n = 0; n < linksCSV.Count; n++)
        {
            txt += n + ": " + linksCSV[n] + "\n";
        }
        Debug.Log(txt + "\n");
    }

    public int Match(Texture2D tex)
    {
        float t = Time.realtimeSinceStartup;
        int nCat = FindCatMatch(tex);
        t = Time.realtimeSinceStartup - t;
//        Debug.Log("Match " + t.ToString("F4") + "\n");
        return nCat;
    }

    //public void LoadTexsAll()
    //{
    //    Debug.Log("LoadTexsAll\n");
    //    return;
    //    texsAll.Clear();
    //    for (int n = 0; n < numCats; n++)
    //    {
    //        Texture2D tex = LoadTexQR512(n);
    //        texsAll.Add(tex);
    //    }
    //}

    public int FindCatMatch(Texture2D texSearch)
    {
        List<int> counts = GetCountsFor(texSearch);
        CalcSumDeltasFor(counts);
        int nCat = FindMinSumDelta();
        return nCat;
    }

    int FindMinSumDelta()
    {
        int nMin = -1;
        int minValue = -1;
        for (int n = 0; n < sumDeltas.Count; n++)
        {
            if (nMin == -1 || sumDeltas[n] < minValue)
            {
                minValue = sumDeltas[n];
                nMin = n;
            }
        }
        return nMin;
    }

    void CalcSumDeltasFor(List<int> counts)
    {
        sumDeltas.Clear();
        for(int n = 0; n < countsAll.Count; n++)
        {
            int sumDelta = GetSumDeltaFor(countsAll[n], counts);
            sumDeltas.Add(sumDelta);
        }
    }

    int GetSumDeltaFor(List<int>countsA, List<int>countsB)
    {
        int sumDelta = 0;
        for(int n = 0; n < countsA.Count; n++)
        {
            sumDelta += Mathf.Abs(countsA[n] - countsB[n]);
        }
        return sumDelta;
    }

    List<int> GetCountsBlob4(Texture2D tex, List<int> counts)
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
                int c = n;
                if (c >= 0 && c < maxCount)
                {
                    counts[c] = cnt;
                }
                n++;
            }
        }
        return counts;
    }

    List<int>GetCountsFor(Texture2D tex)
    {
        List<int> counts = InitCounts();
        counts = GetCountsBlob4(tex, counts);
        return counts;
    }

    void LoadCountsAll()
    {
        countsAll.Clear();
        foreach(Texture2D tex in texsAll)
        {
            List<int> counts = GetCountsFor(tex);
            countsAll.Add(counts);
        }
    }

    List<int>InitCounts()
    {
        List<int> counts = new List<int>();
        for (int c = 0; c < maxCount; c++)
        {
            counts.Add(0);
        }
        return counts;
    }

    public void GenerateQR512()
    {
        for (int i = 0; i < numCats; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                Texture2D tex = GenerateQR512Tex(i, 3);
                tex.filterMode = FilterMode.Point;
                SaveTexQR512(tex, i);
            }
        }
        Debug.Log("Now drag 512 images to inspector (lock it)!!!\n");
    }

//    Texture2D LoadTexQR512(int i)
//    {
////        byte[] bytes = tex.EncodeToPNG();
//        string filename = "QR512_" + i;
//        string path = GetPathQR512();
//        string filespec = Path.Combine(path, filename + ".png");
////        File.WriteAllBytes(filespec, bytes);
//        byte[] bytes = File.ReadAllBytes(filespec);
//        Texture2D tex = new Texture2D(res, res);
//        tex.LoadImage(bytes);
//        return tex;
//    }

    void SaveTexQR512(Texture2D tex, int i)
    {
        byte[] bytes = tex.EncodeToPNG();
        string filename = "QR512_" + i;
        string path = GetPathQR512();
        string filespec = Path.Combine(path, filename + ".png");
        File.WriteAllBytes(filespec, bytes);
    }

    string GetPathQR512()
    {
//        string path = Path.Combine(Application.dataPath, "SumDeltaQR", "Textures", "QR512");
        string path = Path.Combine(Application.dataPath, "Resources", "QR512");
        return path;
    }

    public Texture2D GenerateQR512Tex(int i, int divs)
    {
        Texture2D tex = new Texture2D(res, res);
        int w = tex.width;
        int h = tex.height;
        int x1 = 0;
        int y1 = 0;
        int x2 = w - 1;
        int y2 = h - 1;
        float ww = w / (float)divs;
        float hh = h / (float)divs;
        Color color = Color.black;
        PaintRect(tex, x1, y1, x2, y2, color);
        x1 = 1;
        y1 = 1;
        x2 = w - 2;
        y2 = h - 2;
        PaintRect(tex, x1, y1, x2, y2, Color.white);
        //
        string txt = GetBinaryFor(i);
        //
        for (int x = 0; x < divs; x++)
        {
            for (int y = 0; y < divs; y++)
            {
                int n = x * divs + y;
                if (txt.Substring(n, 1) == "1")
                {
                    x1 = (int)(ww * x);
                    y1 = (int)(hh * y);
                    x2 = (int)(x1 + ww);
                    y2 = (int)(y1 + hh);
                    PaintRect(tex, x1, y1, x2, y2, color);
                }
            }
        }
        tex.Apply();
        return tex;
    }

    public void PaintRect(Texture2D tex, int x1, int y1, int x2, int y2, Color color)
    {
        for (int x = x1; x < x2; x++)
        {
            for (int y = y1; y < y2; y++)
            {
                tex.SetPixel(x, y, color);
            }
        }
    }

    public string GetBinaryFor(int i)
    {
        string txt = "";
        for (int n = 8; n >= 0; n--)
        {
            int nP = (int)Mathf.Pow(2, n);
            if (i / nP > 0)
            {
                txt = "1" + txt;
                i -= nP;
            }
            else
            {
                txt = "0" + txt;
            }
        }
        //txt = "0" + txt;
        return txt;
    }

    public void AddContactSheet()
    {
        float t = Time.realtimeSinceStartup;
        Color colorB = (Color.gray + Color.black) / 2;
        int numX = 32;
        int numY = 16; 
        int s = 6;
        //
        int resS = res * s;
        int bb = resS * 2 / 3;
        int w = resS + bb * 2;
        int b = bb / 3;
        int dx = w + b * 2;
        int dy = dx;
        int width = numX * dx;
        int height = numY * dy;
        Texture2D texOut = new Texture2D(width, height);
        PaintRect(texOut, 0, 0, width, height, Color.white);
        //
        for (int x = 0; x < numX; x++)
        {
            for (int y = 0; y < numY; y++)
            {
                int i = numY * x + y;
                Debug.Log(i + "\n");
                AddContactSheetFor(texOut, i, x, y, w, dx, dy, b, bb, s, colorB);
            }
        }
        texOut.Apply();
        Debug.Log("textOut " + texOut.width + " x " + texOut.height + "\n");
        SaveContactSheet(texOut);
        t = Time.realtimeSinceStartup;
        Debug.Log("QR512 Contact Sheet " + t.ToString("F4") + "\n");
        return;
    }

    void AddContactSheetFor(Texture2D texOut, int i, int x, int y, int w, int dx, int dy, int b, int bb, int s, Color colorB)
    {
        int bx1 = x * dx;
        int by1 = y * dy;
        int bx2 = bx1 + dx;
        int by2 = by1 + dy;
        PaintRect(texOut, bx1, by1, bx2, by2, Color.white);
        PaintRect(texOut, bx1 + b, by1 + b, bx2 - b, by2 - b, Color.red);
        int bbb = bb * 2 / 3;
        PaintRect(texOut, bx1 + b + bbb, by1 + b + bbb, bx2 - b - bbb, by2 - b - bbb, Color.yellow);
        bbb = bb * 5 / 6;
        PaintRect(texOut, bx1 + b + bbb, by1 + b + bbb, bx2 - b - bbb, by2 - b - bbb, Color.green);
        Texture2D tex = CopyOfTex(texsAll[i]);
        PaintRect(tex, res-1, 0, res, res, colorB);
        PaintRect(tex, 0, res-1, res, res, colorB);
        tex = ScaleUp(tex, s);
        int xStart = x * dx + b + bb;
        int yStart = y * dy + b + bb;
        for (int x1 = 0; x1 < tex.width; x1++)
        {
            for (int y1 = 0; y1 < tex.height; y1++)
            {
                int x2 = xStart + x1;
                int y2 = yStart + y1;
                Color color = tex.GetPixel(x1, y1);
                texOut.SetPixel(x2, y2, color);
            }
        }

        xStart = x * dx + b;
        yStart = y * dy + b;
        Texture2D texL = texLogo;
        Texture2D texN = GetTexFromNumber(i);
        for (int x1 = 0; x1 < texN.width; x1++)
        {
            for (int y1 = 0; y1 < texN.height; y1++)
            {
                int x2 = xStart + x1 + w / 2 - texN.width / 2;
                int y2 = yStart + y1 + texN.height;
                texOut.SetPixel(x2, y2, texN.GetPixel(x1, y1));
            }
        }
        //
        for (int x1 = 0; x1 < texL.width; x1++)
        {
            for (int y1 = 0; y1 < texL.height; y1++)
            {
                int x2 = xStart + x1 + w / 2 - texL.width / 2;
                int y2 = yStart + y1;
                Color color = texL.GetPixel(x1, y1);
                texOut.SetPixel(x2, y2, color);
            }
        }
    }

    public Texture2D CopyOfTex(Texture2D tex)
    {
        Texture2D texNew = new Texture2D(tex.width, tex.height);
        if (!tex)
        {
            tex = texNew;
            Debug.Log("CopyOfTex null\n");
        }
        texNew.SetPixels(tex.GetPixels());
        texNew.Apply(true);
        return texNew;
    }

    Texture2D ScaleUp(Texture2D tex, int s)
    {
        Texture2D texOut = new Texture2D(tex.width * s, tex.height * s);
        for (int x = 0; x < tex.width; x++)
        {
            for (int y = 0; y < tex.height; y++)
            {
                Color color = tex.GetPixel(x, y);
                PaintRect(texOut, x * s, y * s, x * s + s, y * s + s, color);
            }
        }
        texOut.Apply();
        return texOut;
    }

    Texture2D GetTexFromNumber(int num)
    {
        int w = res;
        int h = res;
        string txt = num.ToString();
        int b = 0;
        int dx = w + b;
        int width = txt.Length * dx;
        Texture2D texOut = new Texture2D(width, h);
        for (int n = 0; n < txt.Length; n++)
        {
            int t = int.Parse(txt.Substring(n, 1));
            Texture2D tex = texNums[t];
            for (int x1 = 0; x1 < tex.width; x1++)
            {
                for (int y1 = 0; y1 < tex.height; y1++)
                {
                    int x2 = n * dx + x1;
                    int y2 = y1;
                    Color color = tex.GetPixel(x1, y1);
                    color = color * .25f + Color.gray * .75f;
                    texOut.SetPixel(x2, y2, color);
                }
            }
        }
        texOut.Apply();
        return texOut;
    }

    void SaveContactSheet(Texture2D tex)
    {
        byte[] bytes = tex.EncodeToPNG();
        string filename = "QR512_contact_sheet";
        string path = Path.Combine(Application.dataPath, "Resources");
        string filespec = Path.Combine(path, filename + ".png");
        File.WriteAllBytes(filespec, bytes);
    }

    public void LoadUrlCSV()
    {
        string filespec = Path.Combine(Application.persistentDataPath, filenameUrlCSV);
        if (File.Exists(filespec))
        {
            byte[] bytes = File.ReadAllBytes(filespec);
            urlCSV = System.Text.Encoding.UTF8.GetString(bytes);
        }
        Debug.Log("LoadUrlCSV " + urlCSV + "\n");
    }

    public void SaveUrlCSV()
    {
        string filespec = Path.Combine(Application.persistentDataPath, filenameUrlCSV);
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(urlCSV);
        File.WriteAllBytes(filespec, bytes);
    }
}
