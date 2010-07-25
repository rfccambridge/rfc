using System;
using System.Collections.Generic;
using System.Text;
using Robocup.Geometry;
using Robocup.Core;
using Robocup.Utilities;

namespace Robocup.PlaySystem
{
    /// <summary>
    /// The class that, given a game state, passes it on to
    /// the appropriate play library class and assigns actions
    public class PlayAssigner
    {
        Dictionary<PlayType, PlayLibrary> playlibrarydict;

        public PlayAssigner()
        {
            // list of play libraries
            // this is the second biggest hack of the play system, after the ActionAssigner.
            // It would be nice to use reflection to get this list. Fortunately, the only
            // time this list would ever be edited in the future is if a new PlayType is added
            // (or the name of one changes), which is extremely rare
            List<PlayLibrary> libraries = new List<PlayLibrary>(new PlayLibrary[] {
                new NormalPlayLibrary(), 
                new StoppedLibrary(),
                new KickOff_Ours_SetupLibrary(),
                new KickOff_OursLibrary(), 
                new KickOff_Theirs_SetupLibrary(), 
                new KickOff_TheirsLibrary(),
                new PenaltyKick_Ours_SetupLibrary(), 
                new PenaltyKick_OursLibrary(), 
                new PenaltyKick_TheirsLibrary(),
                new SetPlay_OursLibrary(), 
                new SetPlay_TheirsLibrary(), 
                new HaltLibrary()
            });

            // initialize the dictionary
            playlibrarydict = new Dictionary<PlayType, PlayLibrary>();
            foreach (PlayLibrary library in libraries)
            {
                playlibrarydict[library.getPlayType()] = library;
            }
        }

        /// <summary>
        /// given a PlayType, return the IPlayLibrary that corresponds to it
        /// </summary>
        /// <param name="t"></param>
        private PlayLibrary getPlayClass(PlayType playtype)
        {
            return playlibrarydict[playtype];
        }

        public void assignPlays(GameState state)
        {
            // reset the state
            state.reset();

            // find the appropriate library class and apply its mainplay
            PlayLibrary playLibrary = getPlayClass(state.Playtype);

            playLibrary.mainplay(state);

            // for all unassigned robots, assign the stop command
            foreach (int i in state.UnassignedIDs)
            {
                state.Assigner.Stop(i);
            }
        }

        /// <summary>
        /// reload constants for the play system
        /// </summary>
        public void ReloadConstants()
        {
            foreach (PlayLibrary lib in playlibrarydict.Values)
            {
                lib.ReloadConstants();
            }

            // reload the static classes relating to plays
            SharedPlays.ReloadConstants();
            Field.ReloadConstants();
        }
    }
}
