using UnityEngine;
using System.Collections;

public class Menu : MonoBehaviour {

    public SnakeGame Game;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetButtonDown("Menu"))
        {
            Canvas canv = GetComponent<Canvas>();
            if (Game.Status == SnakeGame.STATUS_PLAY)
            {
                canv.enabled = true;
                Game.Status = SnakeGame.STATUS_PAUSED;
            }
            else if (Game.Status == SnakeGame.STATUS_PAUSED)
            {
                canv.enabled = false;
                Game.Status = SnakeGame.STATUS_PLAY;
            }
        }
	}

    public void BtnPlayClick(int level)
    {
        GameObject.Find("CanvasMenu").GetComponent<Canvas>().enabled = false;
        if (Game.Status == SnakeGame.STATUS_PAUSED) {
            Game.Status = SnakeGame.STATUS_PLAY;
        } else if (Game.Status == SnakeGame.STATUS_MAIN_MENU) {
            Game.ResetGame();
            Game.Status = SnakeGame.STATUS_PLAY;
        }
    }

    public void BtnQuitClick()
    {
        Debug.Log("Application.Quit");
        Application.Quit();
    }

}
