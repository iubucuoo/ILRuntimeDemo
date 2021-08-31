using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Demo : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        HotFixMgr.Instance.appdomain.Invoke("HotFix_Project.HotFixHellowWorld", "HellowWorld", null, null);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
