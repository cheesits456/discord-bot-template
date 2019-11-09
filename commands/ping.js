module.exports.run = async (client, msg, args) => {
	msg.channel.send([
    `**Ping :** \`${Math.floor(client.ping)}ms\``,
    `**TPS :** \`${(tps()/1000000).toFixed(1)} million\``
  ].join("\n"))
}

module.exports.help = {
  name: "ping"
}

function tps() {
  let i = 0,
    s = Date.now();
  while (Date.now() - s < 1000) i++;
  return i;
};