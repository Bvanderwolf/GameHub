using UnityEngine;

public class MatchThreeGameState : MonoBehaviour
{
    public static bool GameOver { get; private set; }

    private void Awake ()
    {
        ResourceManager.AddResource<Sprite>("gemBlue", "MatchThree/gemBlue", HubGames.MATCHTHREE);
        ResourceManager.AddResource<Sprite>("gemGreen", "MatchThree/gemGreen", HubGames.MATCHTHREE);
        ResourceManager.AddResource<Sprite>("gemRed", "MatchThree/gemRed", HubGames.MATCHTHREE);
        ResourceManager.AddResource<Sprite>("gemYellow", "MatchThree/gemYellow", HubGames.MATCHTHREE);
        ResourceManager.AddResource<Texture>("dotGreen", "MatchThree/greendot", HubGames.MATCHTHREE);
        ResourceManager.AddResource<Texture>("dotRed", "MatchThree/reddot", HubGames.MATCHTHREE);
    }
}