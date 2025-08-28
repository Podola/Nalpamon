using UnityEngine;
using Yarn.Unity;

public class YarnEnvisionHandle : MonoBehaviour
{
    private bool successEnvision;

    private void Start()
    {
        DialogueRunner runner = FindFirstObjectByType<DialogueRunner>();

        runner.AddCommandHandler("EnvisionStart", EnvisionStart);
        runner.AddCommandHandler("EnvisionFail", EnvisionFail);

        runner.AddFunction("CheckEnvision", CheckEnvision);
    }

    private void EnvisionStart() => successEnvision = true;

    private void EnvisionFail() => successEnvision = false;

    private bool CheckEnvision() => successEnvision;
}
