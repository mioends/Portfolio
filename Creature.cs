using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Creature : MonoBehaviour, IDamage
{
    public float startHP = 100f;
    public float curHP {  get; private set; }
    public bool isDead {  get; protected set; }

    public virtual void OnEnable()
    {
        isDead = false;
        curHP = startHP;
    }

    public virtual void Damage(float damage)
    {
        curHP -= damage;
        if (curHP <= 0) Die();
    }
    public virtual void RestoreHP(float HPUp)
    {
        if(isDead) return;
        curHP += HPUp;
    }
    public virtual void Die()
    {
        isDead = true;
        Invoke("ObjectDisable", 3f);
    }
    void ObjectDisable()
    {
        gameObject.SetActive(false);
    }
}
