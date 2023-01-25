using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using UnityEngine.InputSystem;
using Unity.VisualScripting;

public class ChangeText : MonoBehaviour
{
    private DefaultInputActions _input;
    public Text buttonText;
    public int trials_run = 0;
    public int Score;
    public int upper_threshold_1 = 30;
    public int upper_threshold_2 = 50;
    public int clicked_streak = 0;
    public int B1increase = 20;
    public int B2increase = 20;
    int low_score = 0;
    float Cooldown = 0f;
    float trial_time = 0f;
    float time_passed = 0f;
    bool firstB1 = true;
    bool button_pressed;
    GameObject GoodBadText;
    GameObject CooldownText;
    GameObject ChooseText;
    GameObject TooSlowText;
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
        ChooseText = GameObject.Find("Choose Text");
        TooSlowText = GameObject.Find("Too Slow Text");

        TooSlowText.SetActive(true);

        _input = new DefaultInputActions();

        _input.Presses.FirstButton.Enable();
        _input.Presses.FirstButton.performed += FirstButton_performed;

        _input.Presses.SecondButton.Enable();
        _input.Presses.SecondButton.performed += SecondButton_performed;
    }

    

    // Update is called once per frame
    void Update()
    {
        Cooldown = CalculateTime(Cooldown); // Find and return how long is left for cooldown

        trial_time = CalculateTime(trial_time);

        CheckIfNewTrail();

    }

    void CheckIfNewTrail()
    {
        time_passed += Time.deltaTime;
        if (time_passed >= 1.5)
        {
            if (!button_pressed)
            {
                StartCoroutine(ShowTooSlow());
            }
            trials_run++;
            Debug.Log("Trial: " + trials_run);
            time_passed = 0;
        }
        else if (button_pressed && time_passed < 1.5)
        {
            time_passed = 0;
            trials_run++;
            Debug.Log("Trial: " + trials_run);
        }
        button_pressed = false;
        
    }

    IEnumerator ShowTooSlow()
    {
        TooSlowText.SetActive(true);
        yield return new WaitForSeconds(2);
        TooSlowText.SetActive(false);
    }

    float CalculateTime(float time)
    {
        if (time > 0f)
        {
            time -= Time.deltaTime;
        }
        

        return time;
    }

    void GetIncrease(string button_pressed)
    {
        System.Random rand = new();
        float prob = 125f; // Probability of buttons jumping
        int randy = rand.Next(1000);

        if (randy <= prob)
        {
            if (button_pressed == "B1" && B2increase <= B1increase) // If button 1 has been pressed and B2 is currently inferior
            {
                
                B2increase += 20; // Increase the score button 2 gives

                Debug.Log("B2 Increased!");
            }
            else if (button_pressed == "B2" && B1increase <= B2increase)
            {
                if (firstB1)
                {
                    B1increase += 10; // First B1 jump is 30 instead of 20
                    firstB1 = false;
                }
                B1increase += 20;

                Debug.Log("B1 Increased!");
            }
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
        
        

        prevClicked = "B1";
        NewText(upper_threshold_1, "B1"); // Send updated integer to display up to date score

        GetIncrease("B1");
        
        //CheckForFlip();
        CheckIfFinished();
    }

    public void UpdateProbablity_2()
    {
        

        prevClicked = "B2";
        NewText(upper_threshold_2, "B2");

        GetIncrease("B2");

        //CheckForFlip();
        CheckIfFinished();
    }

    void CheckIfFinished()
    {
        if (trials_run == 100)
        {
            GoodBadText.GetComponent<TextMeshProUGUI>().text = "You have completed all trials";
            _input.Presses.FirstButton.Disable();
            _input.Presses.SecondButton.Disable();
        }
    }
    private void FirstButton_performed(InputAction.CallbackContext obj)
    {
        if (Cooldown <= 0) // If the cooldown has finished
        {
            Cooldown = 1f; // Reset cooldown
            button_pressed = true;
            UpdateProbablity_1(); // Carry out function
        }
    }

    private void SecondButton_performed(InputAction.CallbackContext obj)
    {
        if (Cooldown <= 0)
        {
            Cooldown = 1f;
            button_pressed = true;
            UpdateProbablity_2();
        }
        
    }

    void CheckForFlip() // Probably don't need this
    {
        System.Random rand = new();
        float prob = 125f; // Probability of buttons flipping 

        if(rand.Next(1000) <= prob) // If random number is less than probability
        {
            Debug.Log("Buttons flipped!");
            FlipButtons();
        }

    }

    void FlipButtons() // Probably don't need this
    {
        int temp_upper_threshold; // Create temporary variables to store values when switching
        int temp_B_increase;

        temp_upper_threshold = upper_threshold_1;
        upper_threshold_1 = upper_threshold_2;
        upper_threshold_2 = temp_upper_threshold;

        temp_B_increase = B1increase;
        B1increase = B2increase;
        B2increase = temp_B_increase;
    }

}


