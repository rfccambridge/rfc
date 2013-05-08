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
        // Rounds until reconsidering auction assignment
        public const int ROUNDS_BEFORE_REASSIGN = 10;
        // Radius of circle for forming an auction. Setting to >4 ensures global auctions.
        public const double ISLAND_RADIUS = 1.0; //m
        static private int _roundsCount = 0;

        private class Bid
        {
            public Bid(RobotPath path, double value) { Path = path; Value = value; }
            public RobotPath Path;
            public double Value;
        }

        private class SubAuction
        {
            public Team Team;
            public List<int> Bidders;
            
            // Token from previous round. Tells us if we get to plan this round or not.
            private RobotPath _winnerPath = new RobotPath();

            // The ongoing bids for this round.
            private List<Bid> _currentBids = new List<Bid>();

            public SubAuction(Team team, List<int> bidders)
            {
                Team = team;
                Bidders = bidders;
            }

            /// <summary>
            /// Checks if robot @id is the winner of the auction at team @team
            /// </summary>
            public bool HasWinnerToken(Team team, int id)
            {
                if (_winnerPath == null)
                    return false;

                return _winnerPath.ID == id;
            }

            public bool HasBot(Team team, int id)
            {
                return (team == Team && Bidders.Exists(x => x == id));
            }

            public RobotPath GetWinnerPath()
            {
                return _winnerPath;
            }

            /// <summary>
            /// The winner "token" is up for grabs, time for new auction round
            /// </summary>
            /// <param name="team">Team that performs the auction</param>
            /// <param name="id">Winner of the previous bid releasing token</param>
            public void ReleaseWinnerToken(Team team, int id)
            {
                // Do nothing (due to serial implementation). FinishRound will take care.
            }

            /// <summary>
            /// Place a bid with the marginal path score.
            /// </summary>
            public void PlaceBid(RobotPath bid_path, double bid_value)
            {
                _currentBids.Add(new Bid(bid_path, bid_value));
            }

            /// <summary>
            /// Send the winning path to everyone else on team @team.
            /// </summary>
            public void BroadcastPath(RobotPath bestPath)
            {
                // Do nothing (due to serial implementation). FinishRound will take care.
            }

            /// <summary>
            /// Get the winner of this autcion round at team @team
            /// </summary>
            private RobotPath getWinningBid()
            {
                Bid bestBid = new Bid(null, Double.NegativeInfinity);
                foreach (var bid in _currentBids)
                {
                    if (bid.Value > bestBid.Value)
                        bestBid = bid;
                }
                return bestBid.Path;
            }

            /// <summary>
            /// Clear state from this bidding round and prepare for next
            /// </summary>
            public void FinishRound()
            {
                _winnerPath = getWinningBid();
                _currentBids.Clear();
            }
        }
        
        // The ongoing subauctions for this round.
        static private List<SubAuction> _currentAuctions = new List<SubAuction>();

        static Auction()
        {
            

        }

        /// <summary>
        /// With multiple auctions going on each round, get the one robot @id is on.
        /// </summary>
        static private SubAuction getRobotAuction(Team team, int id)
        {
            foreach (SubAuction auction in _currentAuctions)
                if (auction.HasBot(team, id))
                    return auction;

            return null;
        }

        static public void PlaceBid(RobotPath bid_path, double bid_value)
        {
            SubAuction auction = getRobotAuction(bid_path.Team, bid_path.ID);
            if (auction == null)
                return;

            auction.PlaceBid(bid_path, bid_value);
        }

        static public bool HasWinnerToken(Team team, int id)
        {
            SubAuction auction = getRobotAuction(team, id);
            if (auction == null)
                return true;
            
            return auction.HasWinnerToken(team, id);
        }

        static public RobotPath GetWinnerPath(Team team, int id)
        {
            SubAuction auction = getRobotAuction(team, id);
            if (auction == null)
                return null;

            return auction.GetWinnerPath();
        }

        static public void BroadcastPath(RobotPath path)
        {
            SubAuction auction = getRobotAuction(path.Team, path.ID);
            if (auction == null)
                return;

            auction.BroadcastPath(path);
        }

        static public void ReleaseWinnerToken(Team team, int id)
        {
            SubAuction auction = getRobotAuction(team, id);
            if (auction == null)
                return;

            auction.ReleaseWinnerToken(team, id);
        }

        static public void FinishRound(Team team, IPredictor predictor)
        {
            foreach (SubAuction auction in _currentAuctions)
                if (auction.Team == team)
                    auction.FinishRound();

            _roundsCount++;

            // Reassign 'islands' of communication
            // For now, just look at robots withing a certain range of each other
            // TODO: If we have access to destinations, we can run a few iterations of RRT
            if (_roundsCount % ROUNDS_BEFORE_REASSIGN == 0)
            {
                _roundsCount = 0;

                // Destroy old auctions
                _currentAuctions.RemoveAll(x => x.Team == team);

                // And create new ones with the bots close enough to each other
                List<RobotInfo> bots = predictor.GetRobots(team);
                foreach (RobotInfo bot in bots)
                {
                    // Already assigned to auction by someone
                    if (getRobotAuction(team, bot.ID) != null)
                        continue;

                    List<RobotInfo> botsInRange = bots.FindAll(x => x.Position.distance(bot.Position) <= ISLAND_RADIUS);
                    if (botsInRange.Count > 0) {
                        List<int> botIDs = new List<int>();
                        botIDs.Add(bot.ID);
                        foreach (RobotInfo closeBot in botsInRange)
                            botIDs.Add(closeBot.ID);

                        _currentAuctions.Add(new SubAuction(team, botIDs));
                    }
                }
            }
        }
    }
}
