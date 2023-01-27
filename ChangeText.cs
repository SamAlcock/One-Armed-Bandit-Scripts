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
    bool trial_started = false;
    GameObject GoodBadText;
    GameObject CooldownText;
    GameObject ChooseText;
    GameObject TooSlowText;
    GameObject ZSpritePress;
    GameObject MSpritePress;
    GameObject ZSprite;
    GameObject MSprite;
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

        ZSpritePress = GameObject.Find("Z Key pressed");
        MSpritePress = GameObject.Find("M Key pressed");
        ZSprite = GameObject.Find("Z Key");
        MSprite = GameObject.Find("M Key");

        TooSlowText.SetActive(false);

        ZSpritePress.SetActive(false);
        MSpritePress.SetActive(false);

        _input = new DefaultInputActions();


        // Enabling key presses
        _input.Presses.FirstButton.Enable();
        _input.Presses.FirstButton.performed += FirstButton_performed;

        _input.Presses.SecondButton.Enable();
        _input.Presses.SecondButton.performed += SecondButton_performed;
    }

    

    // Update is called once per frame
    void Update()
    {
        if (trial_started) // Need this if statement to ensure that the trials only start after the user's first button press
        {
            Cooldown = CalculateTime(Cooldown); // Find and return how long is left for cooldown

            trial_time = CalculateTime(trial_time);

            CheckIfNewTrial();
        }
        

    }

    void CheckIfNewTrial()
    {
        float choice_time = 2f;
        time_passed += Time.deltaTime; // Calculate time passed since the start of trial
        if (time_passed >= choice_time) // If time has passed specified amount
        {
            if (!button_pressed) // If a button has not been pressed
            {
                StartCoroutine(ShowTooSlow()); // Display 'Too slow'
            }
            trials_run++; // Go to next trial
            Debug.Log("Trial: " + trials_run);
            time_passed = 0; // Reset timer
        }
        else if (button_pressed && time_passed < choice_time) // If button is pressed and timer has not exceeded maximum
        {
            time_passed = 0; // Reset timer
            trials_run++; // Go to next trial
            Debug.Log("Trial: " + trials_run);
        }
        button_pressed = false; // Reset button press for next trial
        
    }

    IEnumerator ShowTooSlow() // Displays 'Too slow' for specified time
    {
        ChooseText.SetActive(false);
        TooSlowText.SetActive(true);
        yield return new WaitForSeconds(1);
        TooSlowText.SetActive(false);
        ChooseText.SetActive(true);
    }

    IEnumerator ShowKeyPress(GameObject PressedKey, GameObject Key) // Activates/Deactivates key sprites to give user visual feedback for key presses
    {
        PressedKey.SetActive(true);
        Key.SetActive(false);
        yield return new WaitForSeconds(0.3f);
        PressedKey.SetActive(false);
        Key.SetActive(true);
    }

    float CalculateTime(float time)
    {
        if (time > 0f)
        {
            time -= Time.deltaTime; // Subtract time elapsed from time 
        }
        

        return time;
    }

    IEnumerator WaitFor(float seconds)
    {
        yield return new WaitForSeconds(seconds);
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
    IEnumerator NewText(int upper_threshold, string button_pressed)
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
            Score += low_score; // Adds gained score to score total
            inc_display = low_score;
            good_bad = "Bad";
            GoodBadText.GetComponent<TextMeshProUGUI>().color = new Color32(255, 0, 0, 255);
            bad_sound.Play();
        }

        buttonText.text = "SCORE\n" + Score + " (+" + inc_display + ")"; // Update score on screen
        Debug.Log("Started waiting");
        yield return new WaitForSeconds(1.5f);
        Debug.Log("Finshed waiting");
        GoodBadText.GetComponent<TextMeshProUGUI>().text = good_bad;
    }

    public void UpdateProbablity_1() // May be able to use listeners to avoid repetitive code
    {
        
        

        prevClicked = "B1";
        StartCoroutine(NewText(upper_threshold_1, "B1")); // Send updated integer to display up to date score

        GetIncrease("B1");
        
        //CheckForFlip();
        CheckIfFinished();
    }

    public void UpdateProbablity_2()
    {
        

        prevClicked = "B2";
        StartCoroutine(NewText(upper_threshold_2, "B2"));

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
            if (!trial_started)
            {
                trial_started = true;
            }
            Cooldown = 1f; // Reset cooldown
            StartCoroutine(ShowKeyPress(ZSpritePress, ZSprite));
            button_pressed = true;
            UpdateProbablity_1(); // Carry out function
        }
    }

    private void SecondButton_performed(InputAction.CallbackContext obj)
    {
        if (Cooldown <= 0)
        {
            if (!trial_started)
            {
                trial_started = true;
            }
            Cooldown = 1f;
            StartCoroutine(ShowKeyPress(MSpritePress, MSprite));
            button_pressed = true;
            UpdateProbablity_2();
        }
        
    }

}


