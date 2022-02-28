using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace CL03
{
    /// <summary>
    /// Total collection of task locations in the level
    /// Must have more locations than tasks available
    /// </summary>
    
    public class TaskLocationPool
    {

        public TaskLocation[] TaskLocations;

        // Start is called before the first frame update

        public int TaskLocationsCount
        {
            get { return TaskLocations.Length; }
        }
        public TaskLocation GetTaskLocation(int index)
        {
            return TaskLocations[index];
        }
        public bool TaskActive(TaskLocation taskLocation) { return taskLocation.isActive; }

        
    }


}