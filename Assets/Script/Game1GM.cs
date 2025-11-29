using UnityEngine;
using TMPro;

public class Game1GM : MonoBehaviour
{

    public TMP_Text scoreText;

    private int score = 0;

    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        scoreText.text = score + "";
    }

    public void addOneScorePoint()
    {
        score++;
    }
}
