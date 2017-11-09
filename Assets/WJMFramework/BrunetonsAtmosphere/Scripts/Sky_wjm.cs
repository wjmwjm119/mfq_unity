using UnityEngine;
using System.IO;
//using UnityEditor;

namespace BrunetonsAtmosphere
{

    public class Sky_wjm : MonoBehaviour
    {
        private RenderTexture m_skyMap;
        private void Start()
        {
            m_skyMap = new RenderTexture(512, 512, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
            m_skyMap.filterMode = FilterMode.Trilinear;
            m_skyMap.wrapMode = TextureWrapMode.Clamp;
            m_skyMap.useMipMap = true;
            m_skyMap.Create();
        }

        private void Update()
        {
//            Vector3 pos = Camera.main.transform.position;
//           pos.y = 0.0f;
            //centre sky dome at player pos
//            transform.localPosition = pos;

        }

        private void OnDestroy()
        {
            Destroy(m_skyMap);
        }

    }
	
}

