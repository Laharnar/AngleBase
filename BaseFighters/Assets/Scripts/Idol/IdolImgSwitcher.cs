using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdolImgSwitcher : MonoBehaviour
{
	public UnityEngine.UI.Image img;
	public IdolTexting context;
	public List<Sprite> images;

    // Update is called once per frame
    void Update()
    {
		if(img != null && images.Count > 0)
			img.sprite = images[Random.Range(0, images.Count)];
    }
}
