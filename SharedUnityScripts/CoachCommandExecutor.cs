using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CoachCommandExecutor : MonoBehaviour
{
    [System.Serializable]
    public struct CoachCommand
    {
        public string name;
        public UnityEvent onRecognized;
    }

    public List<CoachCommand> coachCommands;

    /*
     * 
     * 
     */
    public void ExecuteCommand(string data)
    {
        //check if data contains command, then invoke corresponding UnityEvent
        //example format of commands: \COMMAND_turn_off_music
        foreach(CoachCommand coachCommand in coachCommands)
        {
            if (data.Contains(coachCommand.name))
            {
                coachCommand.onRecognized.Invoke();
                Debug.Log("Executing coach command:" + coachCommand.name);
            }
        }
    }
}
