using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using UnityEngine.InputSystem;

public class ChangeText : MonoBehaviour
{
    private DefaultInputActions _input;
    public Text buttonText;
    public int Score;
    public int upper_threshold_1 = 30;
    public int upper_threshold_2 = 50;
    public int clicked_streak = 0;
    int streak_limit = 5;
    public int B1increase = 10;
    public int B2increase = 20;
    int low_score = 10;
    float Cooldown = 0f;
    GameObject GoodBadText;
    GameObject CooldownText;
    Color32 red = new Color32(255, 0, 0, 255);
    Color32 green = new Color32(0, 255, 0,255);

    string prevClicked = "";


    public AudioSource good_sound; 
    public AudioSource bad_sound;

    // Start is called before the first frame update
    void Start()
    {
        Score = 0;
        GoodBadText = GameObject.Find("Good/Bad Text");
        CooldownText = GameObject.Find("Cooldown Text");

        _input = new DefaultInputActions();

        _input.Presses.FirstButton.Enable();
        _input.Presses.FirstButton.performed += FirstButton_performed;

        _input.Presses.SecondButton.Enable();
        _input.Presses.SecondButton.performed += SecondButton_performed;
    }

    

    // Update is called once per frame
    void Update()
    {
        Cooldown = CalculateCooldown(Cooldown); // Find and return how long is left for cooldown

        DisplayCooldown();
    }

    void DisplayCooldown()
    {
        double rounded_cooldown = Math.Round(Cooldown, 2);

        Math.Round(Cooldown, 1);
        if (Cooldown < 0)
        {
            CooldownText.GetComponent<TextMeshProUGUI>().text = "0s";
        }
        else
        {
            CooldownText.GetComponent<TextMeshProUGUI>().text = rounded_cooldown.ToString() + "s";
        }
        
    }

    float CalculateCooldown(float cooldown)
    {
        if (cooldown > 0f)
        {
            cooldown -= Time.deltaTime;
            Debug.Log("Cooldown is " + cooldown);
        }
        

        return cooldown;
    }

    void GetIncrease(string button_pressed)
    {
        if (button_pressed == "B1") // If button 1 has been pressed
        {
            B2increase += 20; // Increase the score button 2 gives
        }
        else if (button_pressed == "B2")
        {
            B1increase += 20;
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
    private void FirstButton_performed(InputAction.CallbackContext obj)
    {
        if (Cooldown <= 0) // If the cooldown has finished
        {
            Cooldown = 1f; // Reset cooldown
            UpdateProbablity_1(); // Carry out function
        }
    }

    private void SecondButton_performed(InputAction.CallbackContext obj)
    {
        if (Cooldown <= 0)
        {
            Cooldown = 1f;
            UpdateProbablity_2();
        }
        
    }

}


