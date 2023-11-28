using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Net.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using MojangAPI;
using MojangAPI.Model;
using System.Linq;
using DSharpPlus.CommandsNext;

namespace DiscordBot2
{
    class Program
    {
        static void Main(string[] args)
        {
            MainAsync().GetAwaiter().GetResult();
        }

        static async Task MainAsync()
        {
            //token for discord bot
            var token = "";
            //Straight Up Server
            //ulong serverId = 0;
            //ulong nameChannelId = 0;
            //ulong joinChannelId = 0;
            //ulong citizenRoleId = 0;
            //ulong noNameRoleId = 0;

            ////Testing Server
            ulong serverId = 0;
            ulong nameChannelId = 0;
            ulong joinChannelId = 0;
            ulong citizenRoleId = 0;
            ulong noNameRoleId = 0;

            var discord = new DiscordClient(new DiscordConfiguration()
            {
                Token = token,
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.All
            });

            discord.UseInteractivity(new InteractivityConfiguration()
            {
                Timeout = TimeSpan.FromDays(1)
            });

            var mojang = new Mojang();

            //server info
            DiscordGuild server = await discord.GetGuildAsync(serverId);
            DiscordChannel nameChannel = await discord.GetChannelAsync(nameChannelId);
            DiscordChannel joinChannel = await discord.GetChannelAsync(joinChannelId);

            //roles
            DiscordRole citizenRole = server.Roles[citizenRoleId];
            DiscordRole noNameRole = server.Roles[noNameRoleId];

            //lists
            List<DiscordRole> newRoles = new List<DiscordRole>() { citizenRole };
            List<DiscordRole> timeOutRoles = new List<DiscordRole> { citizenRole, noNameRole };

            discord.GuildMemberAdded += async (s, e) =>
            {
                _ = Task.Run(async () =>
                {
                    var newGuy = e.Member;
                    var interactivity = s.GetInteractivity();
                    bool success = false;

                    try
                    {
                        await nameChannel.SendMessageAsync(newGuy.Mention + " state your IGN (In game name)");
                    }
                    catch
                    {

                    }
                    while (!success)
                    {
                        var minecraftName = await interactivity.WaitForMessageAsync(x => x.Author == e.Member && x.Channel == nameChannel);
                        if (!minecraftName.TimedOut)
                        {
                            PlayerUUID result;
                            try
                            {
                                result = await mojang.GetUUID(minecraftName.Result.Content);
                            }
                            catch
                            {
                                await nameChannel.SendMessageAsync("Error looking up name, API call failed. Please try again");
                                continue;
                            }
                            if (result.UUID != null)
                            {
                                success = true;
                                await nameChannel.SendMessageAsync(newGuy.Mention + "'s name has been changed to " + minecraftName.Result.Content);
                                try
                                {
                                    await newGuy.ModifyAsync(x => x.Roles = newRoles);
                                    await newGuy.ModifyAsync(x => x.Nickname = minecraftName.Result.Content);
                                }
                                catch (Exception ex)
                                {
                                    await nameChannel.SendMessageAsync(ex.Message);
                                }
                            }
                            else
                            {
                                await minecraftName.Result.RespondAsync("Invalid username, please try again. Reply with ONLY your \"Minecraft: Java Edition\" username EXACTLY as it appears in-game.");//await nameChannel.SendMessageAsync("Invalid username, please try again. Reply with ONLY your \"Minecraft: Java Edition\" username EXACTLY as it appears in-game.");
                            }
                        }
                        else
                        {
                            success = true;
                            await nameChannel.SendMessageAsync(newGuy.Mention + "'s name has been changed to retard");
                            await newGuy.ModifyAsync(x => x.Roles = timeOutRoles);
                            await newGuy.ModifyAsync(x => x.Nickname = "retard");
                        }
                    }
                });
            };

            await discord.ConnectAsync();
            await Task.Delay(-1);
        }
    }
}

//discord.GuildMemberUpdated += async (s, e) =>
//{
//    nameChannel.SendMessageAsync(e.Member.Mention + e.NicknameBefore + " is now " + e.NicknameAfter);
//};


//discord.GuildMemberAdded += async (s, e) =>
//{
//    await Task.Run(async () =>
//    {
//        if (e.Guild == server)
//        {
//            DiscordMember newGuy = e.Member;
//            newMembers.Add(newGuy);
//            await nameChannel.SendMessageAsync(newGuy.Mention + " state your IGN (In game name)");
//        }
//    });
//};

//discord.MessageCreated += async (s, e) =>
//{
//    DiscordChannel theChannel = e.Channel;
//    if (e.Message.Content.ToLower().StartsWith("ping"))
//    {
//        await e.Message.RespondAsync("pong!");
//        await channel.SendMessageAsync("cake");
//    }


//};



//discord.GuildMemberRemoved += async (s, e) =>
//{
//    await Task.Run(async () =>
//    {
//        try { newMembers.Remove(e.Member); }
//        catch { return; }
//    });
//};

//var generalChat = await channel.GetMessageAsync((ulong)channel.LastMessageId);

//discord.GuildMemberAdded += async (s, e) =>
//{
//    //var t = await channel.GetMessageAsync((ulong)channel.LastMessageId);
//    //ulong wyattId = 469733782446931968; //1015419421923221534
//    //ulong aaronId = 406908581817090048; //1015419421923221534
//    //DiscordUser wyattUser = await discord.GetUserAsync(wyattId);
//    //DiscordUser aaronUser = await discord.GetUserAsync(aaronId);

//    //var test = (DiscordMember)wyattUser;
//    //DiscordDmChannel userDmChannel = await test.CreateDmChannelAsync();
//    //await discord.SendMessageAsync(userDmChannel, "name   -wyatt");

//    DiscordMember user = e.Member;
//    DiscordDmChannel userDmChannel = await user.CreateDmChannelAsync();
//    DiscordMessage messageSent = await discord.SendMessageAsync(userDmChannel, "name pls");

//    var response = messageSent.GetNextMessageAsync();

//    //NickNameReply(userDmChannel, discord).GetAwaiter().GetResult();
//};

//static async Task NickNameReply(MessageCreateEventArgs e, DiscordClient discord)
//{
//    //DiscordMember user = (DiscordMember)e.Author;
//    await e.Message.RespondAsync("What is your nickname?");
//    var response = await e.Message.GetNextMessageAsync();

//    if (!response.TimedOut)
//    {
//        await e.Message.RespondAsync("thank you");
//    }
//}


//discord.MessageCreated += async (s, e) =>
//{
//    _ = Task.Run(async () =>
//    {
//        if (e.Channel == nameChannel && (e.Author != s.CurrentUser) && !(e.Message.Content == ""))
//        {
//            DiscordMember newGuy = (DiscordMember)e.Author;
//            if (e.Message.Content == "join")
//            {
//                var interactivity = s.GetInteractivity();
//                await e.Message.RespondAsync("you want to join?");
//                var response = await interactivity.WaitForMessageAsync(x => x.Author == e.Author && x.Content == "yes");
//                if (!response.TimedOut)
//                {
//                    await e.Message.RespondAsync("you're in!");
//                }
//                else
//                {
//                    await e.Message.RespondAsync("I never heard you");
//                }
//            }
//            else if (e.Message.Content == "test")
//            {
//                await e.Message.RespondAsync("tested");
//            }
//            if (newMembers.Contains(newGuy))
//            {
//                PlayerUUID result;
//                try
//                {
//                    result = await mojang.GetUUID(e.Message.Content);
//                }
//                catch
//                {
//                    await nameChannel.SendMessageAsync("Error looking up name, API call failed. Please try again");
//                    return;
//                }
//                if (result.UUID != null)
//                {
//                    newMembers.Remove(newGuy);
//                    await newGuy.ModifyAsync(x => x.Roles = null);
//                    await newGuy.ModifyAsync(x => x.Roles = newRoles);
//                    await newGuy.ModifyAsync(x => x.Nickname = e.Message.Content);
//                }
//                else
//                {
//                    await nameChannel.SendMessageAsync("Invalid username, please try again. Reply with ONLY your \"Minecraft: Java Edition\" username EXACTLY as it appears in-game.");
//                }
//            }
//        }
//    });
//};