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
    bool firstB1 = true;
    bool button_pressed;
    bool trial_started = false;
    bool trial_finished = false;
    bool keys_enabled;
    GameObject GoodBadText;
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

        keys_enabled = true;
        _input.Presses.FirstButton.Enable();
        _input.Presses.FirstButton.performed += FirstButton_performed;

        _input.Presses.SecondButton.Enable();
        _input.Presses.SecondButton.performed += SecondButton_performed;
    }

    /* BUGS WITH UNKNOWN REASONS
     *  */

    // Update is called once per frame
    void Update()
    {
        if (trial_started) // Need this if statement to ensure that the trials only start after the user's first button press
        {
            Cooldown = CalculateTime(Cooldown); // Find and return how long is left for cooldown
        }
    }

    void StartTrial()
    {
        InvokeRepeating("CheckIfNewTrial", 2f, 2f);
    }
    void CheckIfNewTrial()
    {
        if (!button_pressed && !trial_finished) // If a button has not been pressed and trial is not finished
        {
            Debug.Log("Starting TooSlow Coroutine");
            StartCoroutine(ShowTooSlow()); 
        }
        else if (button_pressed && !trial_finished)
        {
            trial_finished = true;
        }

        if (trial_finished) 
        {
            trials_run++; // Go to next trial
            Debug.Log("Trial: " + trials_run);
            trial_finished = false;
            button_pressed = false; // Reset button press for next trial
        }
        keys_enabled = true;
    }

    IEnumerator ShowTooSlow() // Displays 'Too slow' for specified time
    {
        keys_enabled = false;

        ChooseText.SetActive(false);
        TooSlowText.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        TooSlowText.SetActive(false);
        ChooseText.SetActive(true);

        keys_enabled = true;

        trial_finished = true;
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
        time -= Time.deltaTime; // Add time elapsed to time 
        return time;
    }

    void GetIncrease(string button_pressed)
    {
        System.Random rand = new();
        int prob = 1; // Probability of buttons jumping
        int randy = rand.Next(8);

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
        GoodBadText.GetComponent<TextMeshProUGUI>().text = good_bad;

        keys_enabled = false;

        yield return new WaitForSeconds(1.5f);

        keys_enabled = true;

        TooSlowText.SetActive(false);
        ChooseText.SetActive(true);
        
    }

    public void UpdateProbablity_1() // May be able to use listeners to avoid repetitive code
    {
        prevClicked = "B1";
        StartCoroutine(NewText(upper_threshold_1, "B1")); // Send updated integer to display up to date score

        GetIncrease("B1");

        CheckIfFinished();
    }

    public void UpdateProbablity_2()
    {
        prevClicked = "B2";
        StartCoroutine(NewText(upper_threshold_2, "B2"));

        GetIncrease("B2");

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
        Debug.Log(Cooldown);
        if (Cooldown <= 0 && keys_enabled) // If the cooldown has finished
        {
            if (!trial_started)
            {
                trial_started = true;
                StartTrial();
            }
            Cooldown = 1f; // Reset cooldown
            StartCoroutine(ShowKeyPress(ZSpritePress, ZSprite));
            button_pressed = true;
            ChooseText.SetActive(false);
            UpdateProbablity_1(); // Carry out function
        }
    }

    private void SecondButton_performed(InputAction.CallbackContext obj)
    {
        if (Cooldown <= 0 && keys_enabled)
        {
            if (!trial_started)
            {
                trial_started = true;
                StartTrial();
            }
            Cooldown = 1f;
            StartCoroutine(ShowKeyPress(MSpritePress, MSprite));
            button_pressed = true;
            ChooseText.SetActive(false);
            UpdateProbablity_2();
        }
        
    }

}


