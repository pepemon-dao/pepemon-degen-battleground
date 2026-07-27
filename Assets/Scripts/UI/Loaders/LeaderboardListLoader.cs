using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
using Thirdweb;
using UnityEngine;
using UnityEngine.UI;
using Pepemon.UI;

/// <summary>
/// Handles the loading of player rankings; Creates instances of _playerRankingPrefab controlled by PlayerRankingController
/// </summary>
public class LeaderboardListLoader : MonoBehaviour
{
    [TitleGroup("Component References"), SerializeField] PlayerRankingController _playerRankingPrefab;
    [TitleGroup("Component References"), SerializeField] GameObject _rankingList;
    [TitleGroup("Component References"), SerializeField] Text _loadingMessage;

    private const int FETCH_SIZE = 50;
    private const int TOP_PLAYERS_AMOUNT = 10;
    private bool loadingInProgress = false;

    /// <summary>
    /// Removes all elements in _playerList and loads all entries using _playerRankingPrefab.
    /// </summary>
    public async void ReloadLeaderboard(PepemonMatchmaker.PepemonLeagues league)
    {
        // prevent re-reloading things over and over again with old async calls if
        // the user decides to go back and forth very quickly between screens
        if (loadingInProgress)
            return;

        loadingInProgress = true;

        // try/finally: both early returns below used to leave loadingInProgress stuck at true,
        // which made the guard above reject every later call - so Refresh became a permanent
        // no-op and the screen sat on "Loading leaderboard..." for the rest of the session.
        try
        {
            _loadingMessage.gameObject.SetActive(true);
            _loadingMessage.text = "Loading leaderboard...";

            // destroy before re-creating
            foreach (var playerRanking in _rankingList.GetComponentsInChildren<PlayerRankingController>())
            {
                Destroy(playerRanking.gameObject);
            }

            // should not happen, but if it happens then it won't crash the game
            var account = await ThirdwebManager.Instance.SDK.Wallet.GetAddress();
            if (string.IsNullOrEmpty(account))
            {
                _loadingMessage.text = "Connect your wallet to see the leaderboard";
                PixelNotice.Instance.Show(
                    "No wallet connected",
                    "Connect your wallet to see where you rank.",
                    "CONNECT WALLET",
                    async () =>
                    {
                        if (Web3Controller.instance == null) return;
                        await Web3Controller.instance.ConnectWallet();
                        if (Web3Controller.instance.IsConnected) ReloadLeaderboard(league);
                    });
                return;
            }

            // load all rankings
            List<(string Address, ulong Ranking)> rankings = new();
            try
            {
                var totalPlayers = await PepemonMatchmaker.GetLeaderboardPlayersCount(league);
                for (ulong i = 0; i < totalPlayers; i += FETCH_SIZE)
                {
                    rankings.AddRange(await PepemonMatchmaker.GetPlayersRankings(league, count: FETCH_SIZE, offset: i));
                }

                rankings = rankings.OrderByDescending((i) => i.Ranking).Take(TOP_PLAYERS_AMOUNT).ToList();
            }
            catch (System.Exception e)
            {
                Debug.Log($"Unable to load leaderboard: {e.Message}");
                _loadingMessage.text = "Could not load the leaderboard. Tap Refresh to retry.";
                return;
            }

            // An empty leaderboard is a normal state, not a failure - no PvP battle has ever
            // been completed, so this is what a new player actually sees.
            if (rankings.Count == 0)
            {
                _loadingMessage.text = "No ranked players yet - be the first";
                return;
            }

            foreach (var playerRanking in rankings)
            {
                var playerRankingInstance = Instantiate(_playerRankingPrefab);
                playerRankingInstance.transform.SetParent(_rankingList.transform, false);
                playerRankingInstance.SetInfo(playerRanking.Address, playerRanking.Ranking.ToString());
            }

            _loadingMessage.gameObject.SetActive(false);
        }
        finally
        {
            loadingInProgress = false;
        }
    }
}
