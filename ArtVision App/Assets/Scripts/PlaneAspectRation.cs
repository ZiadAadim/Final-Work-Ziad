using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneAspectRatio : MonoBehaviour
{
    public Texture2D texture;

    void Start()
    {
        if (texture != null)
        {
            float width = texture.width;
            float height = texture.height;
            float aspectRatio = width / height;

            Vector3 newScale = transform.localScale;
            newScale.x = newScale.z * aspectRatio; // For default Unity plane
            transform.localScale = newScale;
        }
    }
}
