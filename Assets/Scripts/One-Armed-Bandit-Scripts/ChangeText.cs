using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using UnityEngine.InputSystem;
using Unity.VisualScripting;
using Unity.PlasticSCM.Editor.WebApi;

public class ChangeText : MonoBehaviour
{
    private DefaultInputActions _input;
    public Text buttonText;
    public int trials_run = 0;
    public int Score;
    public int upper_threshold_1 = 30;
    public int upper_threshold_2 = 50;
    public int clicked_streak = 0;
    // previously 15 and 20
    public int B1increase = 10;
    public int B2increase = 20;
    public int max_trials = 100;
    public int participant_number = 0;
    int p_explore;
    int p_exploit;
    float response_time = 0f;
    bool button_pressed;
    bool trial_started = false;
    bool trial_finished = false;
    bool responded = false;
    int too_slow = 0;
    int z_key = 0;
    int m_key = 0;
    GameObject ChooseText;
    GameObject TooSlowText;
    GameObject ZSpritePress;
    GameObject MSpritePress;
    GameObject ZSprite;
    GameObject MSprite;
    GameObject Instructions;


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

        Instructions = GameObject.Find("Instructions");

        ChooseText.SetActive(false);

        TooSlowText.SetActive(false);

        ZSpritePress.SetActive(false);
        MSpritePress.SetActive(false);

        _input = new DefaultInputActions();


        // Enabling key presses
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
        if (trial_started && !responded)
        {
            response_time = CalculateTime(response_time);
        }  
    }

    float CalculateTime(float time)
    {
        time += Time.deltaTime;

        return time;
    }

    void StartTrial()
    {
        ChooseText.SetActive(true);
        buttonText.text = "Press Z or M key \nto increase \nyour score";
        InvokeRepeating("CheckIfNewTrial", 2f, 2f);
    }
    void CheckIfNewTrial()
    {

        if (!button_pressed && !trial_finished) // If a button has not been pressed and trial is not finished
        {
            too_slow = 1;
            z_key = 0;
            m_key = 0;
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

            _input.Presses.FirstButton.Enable();
            _input.Presses.SecondButton.Enable();

            ChooseText.SetActive(true);
            trial_finished = false;
            button_pressed = false; // Reset button press for next trial
            responded = false;

            CheckIfFinished();
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
        list.Add(Score);
        list.Add(p_explore);
        list.Add(p_exploit);
        list.Add(response_time);
        list.Add(too_slow);
        list.Add(z_key);
        list.Add(m_key);

        return list;
    }

    void OutputDataToConsole() // Outputs all participant data to console
    {
        Debug.Log("Data going to .csv:");

        for (int i = 1; i < participant_data.Count; i+=8)
        {
            Debug.Log(participant_data[0] + "," + participant_data[i] + "," + participant_data[i + 1] + "," + participant_data[i + 2] + "," + participant_data[i + 3] + "," + participant_data[i + 4] + "," + participant_data[i + 5] + "," + participant_data[i + 6] + "," + participant_data[i + 7]);
        }
    }

    Tuple<int, int> DetermineChoice(string prev, string curr, int clicked_score, int other_score)
    {
        /* To figure out whether choice was explore or exploit:
         * High button + stay = exploit
         * High button + leave = explore
         * Low button + stay = exploit
         * Low button + leave = explore
         */

        int explore;
        int exploit;

        if (prev == "B1" || prev == "B2")
        {
            if (prev == curr && clicked_score > other_score || prev == curr && clicked_score < other_score) // if stay + high button or low button
            {
                explore = 0;
                exploit = 1;
            }
            else if (prev != curr && clicked_score > other_score || prev != curr && clicked_score < other_score) // if leave + high button or low button
            {
                explore = 1;
                exploit = 0;
            }
            else // To avoid code crashing if there's an unforseen error
            {
                explore = 0;
                exploit = 0;
            }
        }
        else // Remove this 
        {
            explore = 0;
            exploit = 0;
        }
        

        

        return Tuple.Create(explore, exploit);
    }

    IEnumerator ShowTooSlow() // Displays 'Too slow' for specified time
    {
        _input.Presses.FirstButton.Disable();
        _input.Presses.SecondButton.Disable();

        ChooseText.SetActive(false);
        TooSlowText.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        TooSlowText.SetActive(false);

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

        if (randy < prob)
        {
            if (button_pressed == "B1" && B2increase <= B1increase) // If button 1 has been pressed and B2 is currently inferior
            {
                B2increase += 20; // Increase the score button 2 gives

                //Debug.Log("B2 Increased!");
            }
            else if (button_pressed == "B2" && B1increase <= B2increase)
            {
                B1increase += 20;

                //Debug.Log("B1 Increased!");
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

        //buttonText.text = "SCORE\n" + Score + " (+" + inc_display + ")"; // Update score on screen
        buttonText.text = "You got " + inc_display; 

        yield return new WaitForSeconds(1.5f);

        TooSlowText.SetActive(false);

    }

    public void UpdateProbablity_1() // May be able to use listeners to avoid repetitive code
    {
        prevClicked = "B1";
        StartCoroutine(NewText(upper_threshold_1, "B1")); // Send updated integer to display up to date score

        GetIncrease("B1");
    }

    public void UpdateProbablity_2()
    {
        prevClicked = "B2";
        StartCoroutine(NewText(upper_threshold_2, "B2"));

        GetIncrease("B2");
    }

    void CheckIfFinished()
    {
        if (trials_run == max_trials)
        {
            buttonText.text = "You have \ncompleted all \ntrials";

            _input.Presses.FirstButton.Disable();
            _input.Presses.SecondButton.Disable();

            ChooseText.SetActive(false);

            StopAllCoroutines();
            CancelInvoke();

            OutputDataToConsole();

            Debug.Log(".csv script running!");

            toCSV.Main();
        }
    }
    private void FirstButton_performed(InputAction.CallbackContext obj)
    {
        responded = true;
        string currClicked = "B1";
        z_key = 1;
        m_key = 0;
        if (!trial_started)
        {
            responded = false;
            trial_started = true;
            Instructions.SetActive(false);
            RectTransform move_text = buttonText.GetComponent<RectTransform>();
            move_text.anchoredPosition = new Vector3(0, 39, 0);
            StartTrial();
        }
        else
        {
            var choice = DetermineChoice(prevClicked, currClicked, B1increase, B2increase);
            p_explore = choice.Item1;
            p_exploit = choice.Item2;

            _input.Presses.FirstButton.Disable();
            _input.Presses.SecondButton.Disable();

            StartCoroutine(ShowKeyPress(ZSpritePress, ZSprite));
            button_pressed = true;
            ChooseText.SetActive(false);
            UpdateProbablity_1(); // Carry out function
        }
        
        
    }

    private void SecondButton_performed(InputAction.CallbackContext obj)
    {
        responded = true;
        string currClicked = "B2";
        z_key = 0;
        m_key = 1;
        if (!trial_started)
        {
            responded = false;
            trial_started = true;
            Instructions.SetActive(false);
            RectTransform move_text = buttonText.GetComponent<RectTransform>();
            move_text.anchoredPosition = new Vector3(0, 39, 0);

            StartTrial();
        }
        else
        {
            var choice = DetermineChoice(prevClicked, currClicked, B2increase, B1increase);
            p_explore = choice.Item1;
            p_exploit = choice.Item2;

            _input.Presses.FirstButton.Disable();
            _input.Presses.SecondButton.Disable();

            StartCoroutine(ShowKeyPress(MSpritePress, MSprite));
            button_pressed = true;
            ChooseText.SetActive(false);
            UpdateProbablity_2();
        }
        
        
        
    }

}


