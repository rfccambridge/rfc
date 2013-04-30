using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Robocup.Core;
using Robocup.Geometry;

namespace Robocup.Core
{
    static public class Auction
    {
        static private int[] _numParticipants = new int[Enum.GetValues(typeof(Team)).Length];
        
        // The ongoing bids for this round.
        static private List<Pair<RobotPath, double>> _currentBids = new List<Pair<RobotPath, double>>();
        
        // Token from previous round. Tells us if we get to plan this round or not.
        static private RobotPath[] _winnerPath = new RobotPath[Enum.GetValues(typeof(Team)).Length];

        /// <summary>
        /// Checks if robot @id is the winner of the auction at team @team
        /// </summary>
        static public bool HasWinnerToken(Team team, int id)
        {
            return _winnerPath[(int)team].ID == id;
        }

        static public RobotPath GetWinnerPath(Team team)
        {
            return _winnerPath[(int)team];
        }

        /// <summary>
        /// The winner "token" is up for grabs, time for new auction round
        /// </summary>
        /// <param name="team">Team that performs the auction</param>
        /// <param name="id">Winner of the previous bid releasing token</param>
        static public void ReleaseWinnerToken(Team team, int id)
        {
            // Do nothing (due to serial implementation). FinishRound will take care.
        }

        /// <summary>
        /// Place a bid with the marginal path score.
        /// </summary>
        static public void PlaceBid(RobotPath bid_path, double bid_value)
        {
            _currentBids.Add(new Pair<RobotPath, double>(bid_path, bid_value));
        }

        /// <summary>
        /// Send the winning path to everyone else on team @team.
        /// </summary>
        static public void BroadcastPath(RobotPath bestPath)
        {
            // Do nothing (due to serial implementation). FinishRound will take care.
        }

        /// <summary>
        /// Get the winner of this autcion round at team @team
        /// </summary>
        static private RobotPath getWinningBid(Team team)
        {
            double maxBid = Double.NegativeInfinity;
            RobotPath bestBid = null;
            foreach (var bid in _currentBids)
            {
                if (bid.First.Team != team)
                    continue;

                if (bid.Second > maxBid)
                {
                    maxBid = bid.Second;
                    bestBid = bid.First;
                }
            }
            return bestBid;
        }

        /// <summary>
        /// Clear state from this bidding round and prepare for next
        /// </summary>
        static public void FinishRound(Team team)
        {
            _currentBids.RemoveAll(x => x.First.Team == team);
            RobotPath winner = getWinningBid(team);
            _winnerPath[(int)team] = winner;
        }
    }
}
