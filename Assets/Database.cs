﻿using UnityEngine;

public class Database : MonoBehaviour {

    public SoundDatabase sounds;
    public SpriteDatabase sprites;

    private void Awake()
    {   // Ensure that these are referenced
        sounds = GetComponent<SoundDatabase>();
        sprites = GetComponent<SpriteDatabase>();
    }
}
