using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Globalization;
public class CSVManager : MonoBehaviour
{

    ChangeText ParticipantData;
    public void Main()
    {
        ParticipantData = GetComponent<ChangeText>();

        List<float> Data = new List<float>(ParticipantData.participant_data); // Take recorded data from task and store it into a list

        string path = "leapfrog.csv"; // Location of .csv file

        var participant = new List<ParticipantEntry>(); // Create a list of all of participants entries

        bool exists = CheckIfFileExists(path); // Check if file exists - have to do this now instead of where the 'if (!exists)' is as the file already exists by then

        for (int i = 1; i < Data.Count; i+=5) // Increment i by 5 because every 5 entries (excluding i = 0) is data for a new trial
        {
            // Assign this data to a new instance of ParticipantEntry
            participant.Add(new ParticipantEntry { Id = (int)Data[0], Trial = (int)Data[i], Explored = (int)Data[i + 1], Exploited = (int)Data[i + 2], ResponseTime = Data[i + 3], TooSlow = (int)Data[i + 4]});
        }

        using FileStream fs = new FileStream(path, FileMode.Append, FileAccess.Write); // Allow appending to keep already recorded data
        using StreamWriter sw = new StreamWriter(fs);
 
        if (!exists) // Create the column headings if the file didn't exist previously
        {
            sw.WriteLine("Participant Number,Trial Number,Explored,Exploited,Response Time (seconds),Too Slow");
        }

        InputData(participant, sw);
    }

    void InputData(List<ParticipantEntry> participant, StreamWriter writer) // Inputs every instances attribute in the correct format
    {
        for (int i = 0; i < participant.Count; i++)
        {
            writer.WriteLine(participant[i].Id + "," + participant[i].Trial + "," + participant[i].Explored + "," + participant[i].Exploited + "," + participant[i].ResponseTime + "," + participant[i].TooSlow);
        }
    }

    bool CheckIfFileExists(string path) // Checks if file exists before writing 
    {
        if (File.Exists(path))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public class ParticipantEntry // Definitions for each entries attributes
    {
        public int Id { get; set; }
        public int Trial { get; set; }
        public int Explored { get; set; }
        public int Exploited { get; set; }
        public float ResponseTime { get; set; }
        public int TooSlow { get; set; }
    }



}
