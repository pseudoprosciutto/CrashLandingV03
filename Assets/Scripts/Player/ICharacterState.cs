using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CL03
{
    /// <summary>
    /// Interface character state to decouple and make the character scripts less cumbersome.
    /// </summary>
    public interface ICharacterState 
    {
        void handle();
    }
}
