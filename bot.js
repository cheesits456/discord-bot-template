const fs = require("fs");
const Discord = require("discord.js");
let client = new Discord.Client();

client.config = require("./config.json");
client.commands = new Discord.Collection();

fs.readdir("./commands/", (err, files) => {
  if (err) console.log(err);
  let jsfile = files.filter(f => f.split(".").pop() === "js");
  jsfile.forEach((f, i) => {
    let props = require(`./commands/${f}`);
    client.commands.set(props.help.name, props);
  });
});

client.on("ready", () => {
  console.log(`Logged in as ${client.user.tag}`);
});

client.on("message", async msg => {
  if (msg.author.bot) return;
  if (!msg.guild) return;
  let prefix = client.config.prefix;
  let args = msg.content.slice(prefix.length).trim().split(/ +/g);
  let command = args.shift().toLowerCase();

  let commandfile = client.commands.get(command);
  if (commandfile) commandfile.run(client, msg, args);
});

client.login(client.config.token).catch(err => {
  if (err.message === "getaddrinfo ENOTFOUND discordapp.com") console.log("Unable to connect to discordapp.com (check network)");
  else if (err.message === "Incorrect login details were provided.") console.log("Invalid bot token was provided - check config.json");
});