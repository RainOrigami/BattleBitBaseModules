using BattleBitAPI.Common;
using BBRAPIModules;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BattleBitBaseModules;

/// <summary>
/// Author: @RainOrigami, @Gunslinger
/// Version: 0.4.11
/// </summary>

public class DFAState
{
    public Dictionary<char, DFAState> Transitions { get; set; }
    public bool IsEndOfWord { get; set; }

    public DFAState()
    {
        Transitions = new Dictionary<char, DFAState>();
        IsEndOfWord = false;
    }
}

public class ProfanityDFAFilter
{
    private DFAState root;

    public ProfanityDFAFilter()
    {
        root = new DFAState();
    }

    public void LoadDictionary(string[] words)
    {
        root = BuildDFA(words);
    }

    private DFAState BuildDFA(string[] words)
    {
        DFAState root = new DFAState();
        foreach (string word in words)
        {
            DFAState currentState = root;
            foreach (char c in word)
            {
                if (!currentState.Transitions.ContainsKey(c))
                {
                    currentState.Transitions.Add(c, new DFAState());
                }
                currentState = currentState.Transitions[c];
            }
            currentState.IsEndOfWord = true;
        }
        return root;
    }

    public bool ContainsProfanity(string text)
    {
        DFAState currentState = root;
        foreach (char c in text)
        {
            if (!currentState.Transitions.ContainsKey(c))
            {
                currentState = root;
                continue;
            }
            currentState = currentState.Transitions[c];
            if (currentState.IsEndOfWord)
            {
                return true;
            }
        }
        return false;
    }
}

public class ProfanityFilter : BattleBitModule
{
    public static ProfanityFilterConfiguration Configuration { get; set; }
    public static ProfanityDFAFilter filter = new();

    public override void OnModulesLoaded()
    {
        // Load dictionary
        filter.LoadDictionary(Configuration.Profanity.ToArray());
    }

    public override Task<bool> OnPlayerTypedMessage(RunnerPlayer player, ChatChannel channel, string message)
    {
        if (filter.ContainsProfanity(message))
        {
            player.Message(Configuration.Message, Configuration.MessageTimeout);
            return Task.FromResult(false);
        }

        return Task.FromResult(true);
    }
}

public class ProfanityFilterConfiguration : ModuleConfiguration
{
    public List<string> Profanity { get; set; } = new();
    public float MessageTimeout { get; set; } = 10f;
    public string Message { get; set; } = "Please do not use profanity in chat.";
}
