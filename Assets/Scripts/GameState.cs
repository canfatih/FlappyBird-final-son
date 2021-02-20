using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// Oyunun durumları
public enum GameState
{
    Intro, //başlangıç
    Playing, // oynanıyor
    Dead // oyun sonu
}

public static class GameStateManager
{
    public static GameState GameState { get; set; }

    static GameStateManager ()
    {
        GameState = GameState.Intro;
    }



}

