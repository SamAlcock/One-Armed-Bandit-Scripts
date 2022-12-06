using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class ChangeText : MonoBehaviour
{
    public Text buttonText;
    public int Score;
    public int upper_threshold_1 = 30;
    public int upper_threshold_2 = 50;
    public int clicked_streak = 0;
    int streak_limit = 5;
    public int B1increase = 15;
    public int B2increase = 15;
    int low_score = 10;
    GameObject GoodBadText;
    Color32 red = new Color32(255, 0, 0, 255);
    Color32 green = new Color32(0, 255, 0,255);

    string prevClicked = "";

    public Button Button1;
    public Button Button2;

    public AudioSource good_sound; 
    public AudioSource bad_sound;

    // Start is called before the first frame update
    void Start()
    {
        Score = 0;
        GoodBadText = GameObject.Find("Good/Bad Text");
    }

    // Update is called once per frame
    void Update()
    {

    }

    void GetIncrease(string button_pressed)
    {
        if (button_pressed == "B1")
        {
            B2increase++;
        }
        else if (button_pressed == "B2")
        {
            B1increase++;
        }
    }
    public void NewText(int upper_threshold, string button_pressed)
    {
        System.Random random = new System.Random();
        int num = random.Next(1, 100); // Picks random number from 1 - 100
        int inc_display = 0;
        string good_bad = "";

        if(num <= upper_threshold) // If random number is less than or equal to upper threshold
        {
            if(button_pressed == "B1")
            {
                Score += B1increase; // Increase score
                inc_display = B1increase;
            }
            else if (button_pressed == "B2")
            {
                Score += B2increase; // Increase score
                inc_display = B2increase;
            }
            good_bad = "Good";
            GoodBadText.GetComponent<TextMeshProUGUI>().color = new Color32(0, 255, 0, 255);
            good_sound.Play();

        }
        else
        {
            Score += low_score;
            inc_display = low_score;
            good_bad = "Bad";
            GoodBadText.GetComponent<TextMeshProUGUI>().color = new Color32(255, 0, 0, 255);
            bad_sound.Play();
        }

        

        buttonText.text = "SCORE\n" + Score + " (+" + inc_display + ")"; // Update score on screen
        GoodBadText.GetComponent<TextMeshProUGUI>().text = good_bad;
    }

    public void UpdateProbablity_1() // May be able to use listeners to avoid repetitive code
    {
        /*
         * To have an experiment like leapfrog, keep this commented
         * 
        if(upper_threshold_1 > 0) // If upper threshold of button clicked is positive
        {
            upper_threshold_1--; // Decrement
        }
        */
        if (prevClicked == "B1") // If previous button clicked was button 1
        {
            clicked_streak++; // Increment streak
        }
        else if(prevClicked == "B2") // Else if previous button clicked was button 2
        {
            clicked_streak = 0; // Reset streak to 0
        }

        if(upper_threshold_2 < 100 && clicked_streak <= streak_limit) // If upper threshold of button not clicked is less that 100
        {
            upper_threshold_2++; // Increment
        }
        Debug.Log("First button pressed!");
        prevClicked = "B1";
        NewText(upper_threshold_1, "B1"); // Send updated integer to display up to date score

        if (clicked_streak % streak_limit == 0 && clicked_streak != 0)
        {
            GetIncrease("B1");
        }
    }

    public void UpdateProbablity_2()
    {
        /*
        if(upper_threshold_2 > 0)
        {
            upper_threshold_2--;
        }
        */
        if (prevClicked == "B2")
        {
            clicked_streak++;
        }
        else if (prevClicked == "B1")
        {
            clicked_streak = 0;
        }

        if (upper_threshold_1 < 100 && clicked_streak <= streak_limit)
        {
            upper_threshold_1++;
        }
        Debug.Log("Second button pressed!");
        prevClicked = "B2";
        NewText(upper_threshold_2, "B2");

        if (clicked_streak % streak_limit == 0 && clicked_streak != 0)
        {
            GetIncrease("B2");
        }
    }
}
