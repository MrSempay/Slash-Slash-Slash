// Создайте скрипт CreateGradientTexture.cs
using UnityEngine;
using static Unity.Burst.Intrinsics.X86;

public class CreateGradientTexture : MonoBehaviour
{
    [SerializeField] private int width = 256;
    [SerializeField] private int height = 256;

    void Start()
    {
        Texture2D gradientTex = new Texture2D(width, height);

        for (int y = 0; y < height; y++)
        {
            float gradientValue = (float)y / height;
            Color color = new Color(gradientValue, gradientValue, gradientValue, 1f);

            for (int x = 0; x < width; x++)
            {
                gradientTex.SetPixel(x, y, color);
            }
        }

        gradientTex.Apply();

        // Сохраняем текстуру
        string path = "C:\\Users\\Fossa2016\\Documents\\gradient.png";
        //System.IO.File.WriteAllBytes("C:\\Users\\Fossa2016\\Documents\\GitHub\\Slash-Slash-Slash\\SSS\\Assets", gradientTex.EncodeToPNG());
        System.IO.File.WriteAllBytes(path, gradientTex.EncodeToPNG());
        //Debug.Log("Gradient texture saved!");
    }
}