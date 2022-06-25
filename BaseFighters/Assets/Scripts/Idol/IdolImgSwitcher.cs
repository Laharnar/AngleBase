using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdolImgSwitcher : MonoBehaviour
{
	public UnityEngine.UI.Image img;
	public IdolTexting context;
	public List<Sprite> images;
    public float rate = 2f;

    // Update is called once per frame
    IEnumerator Start ()
    {
        while (true)
        {
            if (img != null && images.Count > 0)
                img.sprite = images[Random.Range(0, images.Count)];
            yield return new WaitForSeconds(rate);
        }
    }
}
