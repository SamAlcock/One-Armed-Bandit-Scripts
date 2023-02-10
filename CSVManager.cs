using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using CsvHelper;
using System.Globalization;
public class CSVManager : MonoBehaviour
{

    ChangeText ParticipantData;
    public void Main()
    {
        ParticipantData = GetComponent<ChangeText>();

        List<float> Data = new List<float>(ParticipantData.participant_data);

        var participant = new List<ParticipantEntry>();

        for (int i = 0; i < Data.Count; i+=6)
        {
            participant.Add(new ParticipantEntry { Id = (int)Data[0], Trial = (int)Data[i + 1], Explored = (int)Data[i + 2], Exploited = (int)Data[i + 3], ResponseTime = Data[i + 4], TooSlow = (int)Data[i + 5]});
        }

        using (var writer = new StreamWriter("leapfrog.csv")) 
        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
        {
            csv.WriteRecords(participant);
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
