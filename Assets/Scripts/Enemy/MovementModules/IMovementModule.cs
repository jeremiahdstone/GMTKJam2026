using UnityEngine;

public interface IMovementModule
{
    void Initialize(Enemy enemy);
    void OnEnableModule();
    void OnDisableModule();
    void Move();
}

