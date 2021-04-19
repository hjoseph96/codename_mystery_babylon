using UnityEngine;
using System.Collections;

namespace SS.TwoD
{
    public class Screenshot : MonoBehaviour
    {
        [SerializeField]
        string m_FileName = "screenshot.png";

        [SerializeField]
        float m_ShotTime;

        IEnumerator Start()
        {
            yield return new WaitForSeconds(m_ShotTime);
            yield return new WaitForEndOfFrame();
            Texture2D tex = SS.Tools.ScreenshotTools.ScreenShot();
            Save(tex);
        }

        void Save(Texture2D tex)
        {
            string path = System.IO.Path.Combine(Application.dataPath, m_FileName);
            System.IO.File.WriteAllBytes(path, tex.EncodeToPNG());
            Debug.Log(path);
            Destroy(tex);
        }
    }
}
