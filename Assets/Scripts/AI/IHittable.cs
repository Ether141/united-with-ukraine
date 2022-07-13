using System;

public interface IHittable
{
    int MaxHealth { get; }
    int CurrentHealth { get; }

    void GiveDamage(int damage);
    void ImmediatelyDie();

    event Action OnDeath;
}