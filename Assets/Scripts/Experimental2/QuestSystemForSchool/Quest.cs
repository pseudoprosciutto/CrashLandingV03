using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace CL03
{
    /// <summary>
    /// Three types of quest types hard is 3, med is 2, easy is 1 task.
    /// </summary>
    public enum QuestType { Hard, Med, Easy }

    [System.Serializable]
    public class Quest
    {
        int QuestID;
        public string Title;
        public string Description;
        public bool Completed = false;
        public int CompletedSoFar;
        public int Required;
        public Task[] Tasks;
        public QuestType _questType;

        public void TaskCompleted()
        {
            Debug.Log("Task Completed");
            CompletedSoFar++;
            if(CompletedSoFar >= Tasks.Length) { QuestComplete(); }
        }
        /// <summary>
        /// when a quest is created it needs to understand how difficult it is and where its tasks go to. 
        /// </summary>
        public Quest(QuestType questType)
        {

            switch (questType)
            {

                case QuestType.Hard:
                    CreateTasks(3);
                    break;
                case QuestType.Med:
                    CreateTasks(2);
                    break;
                case QuestType.Easy:
                    CreateTasks(1);
                    break;
            }
            Completed = false;
            CompletedSoFar = 0;
        }

        public void CreateTasks(int amt)
        {
            for (int i = 0; i < amt; i++)
            {
                
                Tasks[amt] = CreateNewTask();
            }
        }

        Task CreateNewTask()
        {
            return null;
        }

        public void QuestComplete()
        {
            Debug.Log("This quest has completed");
        }

    }

    [System.Serializable]
    public class Task
    {

        bool isActive;
        TaskLocation taskLocation;
    }
}