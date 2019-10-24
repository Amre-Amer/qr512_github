using UnityEngine;

public class PostRender : MonoBehaviour
{
    ManagerQR512 managerQR512;
    int cntFrames;

    // Start is called before the first frame update
    void Start()
    {
        managerQR512 = GameObject.Find("QR512").GetComponent<ManagerQR512>();
    }

    // Update is called once per frame
    void Update()
    {
        cntFrames++;
    }

    public void OnPostRender()
    {
        int intervalFrame = managerQR512.intervalFrameWebcam;
        if (cntFrames % intervalFrame == 0)
        {
            managerQR512.RefreshWebcam();
        }
    }
}
