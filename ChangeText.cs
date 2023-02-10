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
    public int B1increase = 10;
    public int B2increase = 20;
    public int max_trials = 100;
    public int participant_number = 0;
    int p_explore;
    int p_exploit;
    float response_time = 0f;
    bool firstB1 = true;
    bool button_pressed;
    bool trial_started = false;
    bool trial_finished = false;
    bool keys_enabled;
    int too_slow = 0;
    GameObject ChooseText;
    GameObject TooSlowText;
    GameObject ZSpritePress;
    GameObject MSpritePress;
    GameObject ZSprite;
    GameObject MSprite;

    public List<float> participant_data = new List<float>();

    string prevClicked = "";

    CSVManager toCSV;

    public AudioSource good_sound; 
    public AudioSource bad_sound;

    // Start is called before the first frame update
    void Start()
    {
        Score = 0;
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

        participant_data = AddInitialData(participant_data);

        toCSV = GetComponent<CSVManager>();
    }

    // Update is called once per frame
    void Update()
    {
        response_time = CalculateTime(response_time);
    }

    float CalculateTime(float time)
    {
        time += Time.deltaTime;

        return time;
    }

    void StartTrial()
    {
        InvokeRepeating("CheckIfNewTrial", 2f, 2f);
    }
    void CheckIfNewTrial()
    {
        if (!button_pressed && !trial_finished) // If a button has not been pressed and trial is not finished
        {
            too_slow = 1;
            StartCoroutine(ShowTooSlow()); 
        }
        else if (button_pressed && !trial_finished)
        {
            trial_finished = true;
        }

        if (trial_finished) 
        {
            AddData(participant_data);
            response_time = 0f;
            too_slow = 0;
            trials_run++; // Go to next trial
            Debug.Log("Trial: " + trials_run);
            keys_enabled = true;
            trial_finished = false;
            button_pressed = false; // Reset button press for next trial
        }
        
    }

    List<float> AddInitialData(List<float> list)
    {
        list.Add(participant_number);

        return list;
    }

    List<float> AddData(List<float> list) // Needs to run once per trial
    {
        list.Add(trials_run);
        list.Add(p_explore);
        list.Add(p_exploit);
        list.Add(response_time);
        list.Add(too_slow);

        return list;
    }

    Tuple<int, int> DetermineChoice(string prev, string curr)
    {
        /* To figure out whether choice was explore or exploit:
         * - if the participant chose what they believed was highest pay off it's exploit, if not its explore
         * 
         * How to determine:
         * - variable for highest known score - need to make something to determine what scores have been shown e.g. score shown bool
         * 
         * EXPLOIT
         * if they press button thats lower, but dont know the other its exploit
         * 
         * EXPLORE
         * whenever the user switches button, it's explore
         */

        int explore;
        int exploit;

        if (prev == curr)
        {
            explore = 1;
            exploit = 0;
        }
        else
        {
            explore = 0;
            exploit = 1;
        }
        

        return Tuple.Create(explore, exploit);
    }

    IEnumerator ShowTooSlow() // Displays 'Too slow' for specified time
    {
        keys_enabled = false;

        ChooseText.SetActive(false);
        TooSlowText.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        TooSlowText.SetActive(false);
        ChooseText.SetActive(true);

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
        int inc_display = 0;

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

        buttonText.text = "SCORE\n" + Score + " (+" + inc_display + ")"; // Update score on screen

        yield return new WaitForSeconds(1.5f);

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
        if (trials_run == max_trials)
        {
            buttonText.text = "You have \ncompleted all \ntrials";
            _input.Presses.FirstButton.Disable();
            _input.Presses.SecondButton.Disable();

            keys_enabled = false;
            StopAllCoroutines();
            CancelInvoke();

            toCSV.Main();
        }
    }
    private void FirstButton_performed(InputAction.CallbackContext obj)
    {
        if (keys_enabled) // Only do something if keys are enabled
        {
            string currClicked = "B1";
            if (!trial_started)
            {
                trial_started = true;
                StartTrial();
            }
            var choice = DetermineChoice(prevClicked, currClicked);
            p_explore = choice.Item1;
            p_exploit = choice.Item2;
            keys_enabled = false;
            StartCoroutine(ShowKeyPress(ZSpritePress, ZSprite));
            button_pressed = true;
            ChooseText.SetActive(false);
            UpdateProbablity_1(); // Carry out function
        }
    }

    private void SecondButton_performed(InputAction.CallbackContext obj)
    {
        if (keys_enabled)
        {
            string currClicked = "B2";
            if (!trial_started)
            {
                trial_started = true;
                StartTrial();
            }
            var choice = DetermineChoice(prevClicked, currClicked);
            p_explore = choice.Item1;
            p_exploit = choice.Item2;
            keys_enabled = false;
            StartCoroutine(ShowKeyPress(MSpritePress, MSprite));
            button_pressed = true;
            ChooseText.SetActive(false);
            UpdateProbablity_2();
        }
        
    }

}


