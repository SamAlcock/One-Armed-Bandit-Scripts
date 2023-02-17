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

        List<float> Data = new List<float>(ParticipantData.participant_data);

        string path = "leapfrog.csv";

        var participant = new List<ParticipantEntry>();

        for (int i = 1; i < Data.Count; i+=5)
        {
            participant.Add(new ParticipantEntry { Id = (int)Data[0], Trial = (int)Data[i], Explored = (int)Data[i + 1], Exploited = (int)Data[i + 2], ResponseTime = Data[i + 3], TooSlow = (int)Data[i + 4]});
        }

        for (int i = 0; i < participant.Count; i++) 
        {
            Debug.Log(participant[i].Id + "," + participant[i].Trial + "," + participant[i].Explored + "," + participant[i].Exploited + "," + participant[i].ResponseTime + "," + participant[i].TooSlow);
        }

        if (File.Exists(path))
        {
            using FileStream fs = new FileStream(path, FileMode.Append, FileAccess.Write);
            using StreamWriter sw = new StreamWriter(fs);

            InputData(participant, sw);
        }
        else
        {
            using var sw = new StreamWriter("leapfrog.csv");
            sw.WriteLine("Participant Number,Trial Number,Explored,Exploited,Response Time (seconds),Too Slow");

            InputData(participant, sw);
        }


        
    }

    void InputData(List<ParticipantEntry> participant, StreamWriter writer)
    {
        for (int i = 0; i < participant.Count; i++)
        {
            writer.WriteLine(participant[i].Id + "," + participant[i].Trial + "," + participant[i].Explored + "," + participant[i].Exploited + "," + participant[i].ResponseTime + "," + participant[i].TooSlow);
        }
    }

    public class ParticipantEntry
    {
        public int Id { get; set; }
        public int Trial { get; set; }
        public int Explored { get; set; }
        public int Exploited { get; set; }
        public float ResponseTime { get; set; }
        public int TooSlow { get; set; }
    }



}
