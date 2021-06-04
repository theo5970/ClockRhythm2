using UnityEngine;

[ExecuteInEditMode] // 에디터 내부에서 실행 허용

// 색수차 이펙트
public class ChromaticAberration : MonoBehaviour
{
    public Material mat;
    public float strength = 0.005f; // 이펙트 강도

    private void Update()
    {
        mat.SetFloat("_Amount", strength);
    }

    public void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(source, destination, mat);
    }

    public void SetStrength(float newStrength)
    {
        strength = newStrength;
    }
}
