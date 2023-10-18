using BBRAPIModules;
using Commands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BattleBitBaseModules;

[RequireModule(typeof(CommandHandler))]
[Module("Add reserved slots by setting a password", "1.0.0")]
public class PasswordReservedSlots : BattleBitModule
{
    public PasswordReservedSlotsConfiguration Configuration { get; set; } = null!;

    [ModuleReference]
    public CommandHandler CommandHandler { get; set; } = null!;

    public int getCurrentPlayerCount()
    {
        return Math.Max(this.Server.CurrentPlayerCount + this.Server.InQueuePlayerCount, this.Server.AllPlayers.Count());
    }

    public void handleSlots()
    {
        int currentPlayers = this.getCurrentPlayerCount();
        if (this.Server.MaxPlayerCount - this.Configuration.Slots > currentPlayers)
        {
            this.Server.SetNewPassword("");
        }
        else
        {
            Server.SetNewPassword(this.Configuration.Password);
        }
    }

    public async Task slotHandler()
    {
        while (this.IsLoaded && this.Server.IsConnected)
        {
            this.handleSlots();
            await Task.Delay(this.Configuration.SlotCheckInterval);
        }
    }

    public override Task OnConnected()
    {
        _ = Task.Run(slotHandler);

        return Task.CompletedTask;
    }

    public override Task OnPlayerConnected(RunnerPlayer player)
    {
        this.handleSlots();

        return Task.CompletedTask;
    }

    public override Task OnPlayerDisconnected(RunnerPlayer player)
    {
        this.handleSlots();

        return Task.CompletedTask;
    }

    [CommandCallback("setrspass", Description = "Sets the password for reserved slots", ConsoleCommand = true, Permissions = new[] { "PasswordReservedSlots.SetRSPass" })]
    public string SetRSPassCommand(Context context, string password)
    {
        this.Configuration.Password = password;
        this.Configuration.Save();
        this.handleSlots();

        return "Password set.";
    }

    [CommandCallback("setrsslots", Description = "Sets the number of reserved slots", ConsoleCommand = true, Permissions = new[] { "PasswordReservedSlots.SetRSSlots" })]
    public string SetRSSlotsCommand(Context context, int slots)
    {
        this.Configuration.Slots = slots;
        this.Configuration.Save();
        this.handleSlots();

        return $"Slots set to {slots}.";
    }
}

public class PasswordReservedSlotsConfiguration : ModuleConfiguration
{
    public string Password { get; set; } = "changeme";
    public int Slots { get; set; } = 0;
    public int SlotCheckInterval { get; set; } = 3000;
}
