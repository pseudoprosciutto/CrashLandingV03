using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace CL03
{
    /// <summary>
    /// 
    /// </summary>
    public class QuestManager : Singleton<QuestManager>
    {
        static public QuestManager QM;
        public int activeQuests;
        public TaskLocationPool Tasks;
        public int QuestCounter = 0;
        public int maxQuests = 2;
        public bool QuestsFull = false;
        
        List<Quest> QuestPool = new List<Quest>();


        
        public void AddQuest(Quest quest)
        {
            QM.QuestCounter ++;
            QM.QuestPool.Add(quest);
            if (QM.QuestPool.Count >= QM.maxQuests)
            {
                QM.QuestsFull = true;
            }
            else
            { 
                QM.QuestsFull = false;
            }
        }
   

    }
}