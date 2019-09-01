const Discord = require("discord.js");
const config = require("./config.json");

const client = new Discord.Client();

client.on("ready", () => {
	client.fetchUser(config.owner).then(user => {
		client.owner = user;
		console.log(`Logged in as @${client.user.tag}, owned by @${client.owner.tag}`);
	})
});

client.on("message", msg => {
	if (msg.member.bot) return;
	if (!msg.content.startsWith(config.prefix)) return;
	if (msg.content.startsWith(`${config.prefix}ping`)) {
		msg.channel.send(`**Ping:** \`${client.ping}ms\``);
	}
});

client.login(config.token);