using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PngCapturer : MonoBehaviour
{
    
    
    public int resWidth = 1920; 
    public int resHeight = 1080;
 
    private bool takeHiResShot = false;
    private bool takeShotWithC;
    private string path = "E:\\Game\\gameDev\\unity\\Tests\\PcgDrawingWithParticles\\screenshots";
    
    string ScreenShotName(int width, int height) {
        return string.Format(path+"/screen_{0}x{1}_{2}.png", 
            width, height, 
            System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
    }
 
    public void TakeHiResShot() {
        takeHiResShot = true;
    }
 
    void LateUpdate()
    {
        takeShotWithC |= Input.GetKeyDown(KeyCode.C);
        if (takeShotWithC)
        {
            ScreenCapture.CaptureScreenshot(ScreenShotName(200,200),4);
            takeShotWithC = false;
            print("screenshot captured");
        }
        
        takeHiResShot |= Input.GetKeyDown("k");
        if (takeHiResShot) {
            RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
            Camera.main.targetTexture = rt;
            Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
            Camera.main.Render();
            RenderTexture.active = rt;
            screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
            Camera.main.targetTexture = null;
            RenderTexture.active = null; // JC: added to avoid errors
            Destroy(rt);
            byte[] bytes = screenShot.EncodeToPNG();
            string filename = ScreenShotName(resWidth, resHeight);
            System.IO.File.WriteAllBytes(filename, bytes);
            Debug.Log(string.Format("Took screenshot to: {0}", filename));
            takeHiResShot = false;
        }
    }

     
    
}
