using System;
using System.Collections.Generic;
using System.Linq;
using RoR2;

namespace R2API.Utils
{
    public static class CommandHelper
    {
        /// <summary>
        /// Find a GameObject by its name and index.
        /// </summary>
        /// <param name="name">The name of the GameObject</param>
        /// <param name="index">The index of the GameObject</param>
        /// <returns>The found GameObject</returns>
        public static CharacterBody FindBody(string name, int index)
        {
            var bodies = UnityEngine.Object.FindObjectsOfType<CharacterBody>().Where(b => b.name == name).ToList();
            return bodies.Count > index ? bodies[index] : null;
        }

        public static void RegisterCommands(RoR2.Console self)
        {
            // This is a stub. The original CommandHelper had registration logic here.
            // For modern R2API, console commands are usually registered via R2API.Commands.
        }
    }
}