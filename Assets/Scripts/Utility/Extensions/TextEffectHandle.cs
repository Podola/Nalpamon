using TMPro;
using UnityEngine;

public class TextEffectHandle : MonoBehaviour
{
    private TMP_Text text;

    private void Start() => TryGetComponent(out text);

    private void Update()
    {
        text.ForceMeshUpdate();

        TMP_TextInfo textInfo = text.textInfo;

        if (textInfo.linkCount == 0) return;

        Mesh mesh = text.mesh;
        Vector3[] vertices = mesh.vertices;

        for (int i = 0; i < textInfo.linkCount; i++)
        {
            TMP_LinkInfo info = textInfo.linkInfo[i];

            string linkID = info.GetLinkID();

            if (string.IsNullOrEmpty(linkID)) continue;

            ITextEffect textEffect = TextEffectHub.GetEffect(linkID);

            if (textEffect == null) continue;

            int index = linkID.IndexOf('?');

            for (int j = info.linkTextfirstCharacterIndex; j < info.linkTextfirstCharacterIndex + info.linkTextLength; j++)
            {
                textEffect?.OnEffect(ref vertices, textInfo.characterInfo[j], index == -1 ? "" : linkID[(index + 1)..]);
            }
        }

        mesh.vertices = vertices;

        text.canvasRenderer.SetMesh(mesh);
    }
}
