using UnityEngine;

public abstract class BaseUIView : MonoBehaviour
{
    public abstract void OnAwake();

    public abstract void OnStart();

    public abstract void Show(object[] parameters);
}